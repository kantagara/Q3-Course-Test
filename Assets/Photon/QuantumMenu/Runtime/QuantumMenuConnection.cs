namespace Quantum.Menu {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Photon.Deterministic;
  using Photon.Realtime;
  using UnityEngine;
  using UnityEngine.SceneManagement;

  /// <summary>
  /// The Quantum specific implementation of the connection class used by the shared Photon menu.
  /// </summary>
  public class QuantumMenuConnection : IQuantumMenuConnection {
    private CancellationTokenSource _cancellation;
    private CancellationTokenSource _linkedCancellation;
    private string _loadedScene;
    private QuantumMenuConnectionShutdownFlag _shutdownFlags;
    private DisconnectCause _disconnectCause;
    private IDisposable _disconnectSubscription;

    /// <summary>
    /// Return the actual room name of the connection.
    /// </summary>
    public string SessionName => Client?.CurrentRoom?.Name;
    /// <summary>
    /// Return the actual region connected to.
    /// </summary>
    public string Region => Client?.CurrentRegion;
    /// <summary>
    /// Return the actual AppVersion that is used.
    /// </summary>
    public string AppVersion => Client?.AppSettings?.AppVersion;
    /// <summary>
    /// Return the max player count for the Photon room.
    /// </summary>
    public int MaxPlayerCount => Client?.CurrentRoom != null ? Client.CurrentRoom.MaxPlayers : 0;
    /// <summary>
    /// Return a list a Photon client names also connected to the room.
    /// </summary>
    public List<string> Usernames {
      get {
        var frame = Runner?.Game?.Frames?.Verified;
        if (frame != null) {
          var result = new List<string>(frame.PlayerCount);
          for (int i = 0; i < frame.PlayerCount; i++) {
            var isPlayerConnected = (frame.GetPlayerInputFlags(i) & DeterministicInputFlags.PlayerNotPresent) == 0;
            if (isPlayerConnected) {
              var playerNickname = frame.GetPlayerData(i)?.PlayerNickname;
              if (string.IsNullOrEmpty(playerNickname)) {
                playerNickname = $"Player{i:02}";
              }
              result.Add(playerNickname);
            } else {
              result.Add(null);
            }
          }
          return result;
        }
        return null;
      }
    }
    /// <summary>
    /// Return true if connecting or connected to any server.
    /// </summary>
    public bool IsConnected => Client == null ? false : Client.IsConnected;
    /// <summary>
    /// Return the current ping.
    /// </summary>
    public int Ping => Runner?.Session != null ? Runner.Session.Stats.Ping : 0;

    /// <summary>
    /// The controller is used to update the status text of the loading screen.
    /// </summary>
    public QuantumMenuUIController UIController { get; set; }
    /// <summary>
    /// The RealtimeClient object that is operated on.
    /// </summary>
    public RealtimeClient Client { get; private set; }
    /// <summary>
    /// The QuantumRunner object that is created and started.
    /// </summary>
    public QuantumRunner Runner { get; private set; }

    /// <summary>
    /// There is one indirection that wraps the connection object to be able to store in on a GameObject.
    /// To entage it this cast be be used.
    /// </summary>
    /// <param name="connectionBehaviour"></param>
    public static explicit operator QuantumMenuConnection(QuantumMenuConnectionBehaviour connectionBehaviour) {
      return connectionBehaviour?.Connection as QuantumMenuConnection;
    }

    /// <summary>
    /// There is one indirection that wraps the connection object to be able to store in on a GameObject.
    /// To entage it this pseudo-cast be be used.
    /// </summary>
    /// <param name="connection">Any connection object</param>
    /// <returns>QuantumMenuConnection if can be cast or null</returns>
    public static QuantumMenuConnection GetQuantumMenuConnection(IQuantumMenuConnection connection) {
      var quantumConnection = connection as QuantumMenuConnection;
      if (quantumConnection != null) {
        return quantumConnection;
      }

      var quantumConnectionBehaviour = connection as QuantumMenuConnectionBehaviour;
      if (quantumConnectionBehaviour != null) {
        return (QuantumMenuConnection)quantumConnectionBehaviour;
      }

      return null;
    }

    /// <summary>
    /// Connects to the Photon name server and waits for the ping results.
    /// The connection object will be disconnected afterwards.
    /// </summary>
    /// <param name="connectArgs">Connection arguments</param>
    /// <returns>When the region list has been assembled or null on errors.</returns>
    public async Task<List<QuantumMenuOnlineRegion>> RequestAvailableOnlineRegionsAsync(IQuantumMenuConnectArgs connectArgs) {
      // TODO: fix when implemented in realtime.
      var quantumConnectArgs = (QuantumMenuConnectArgs)connectArgs;
      try {
        var client = quantumConnectArgs.Client ?? new RealtimeClient();
        var appSettings = quantumConnectArgs.AppSettings ?? PhotonServerSettings.Global.AppSettings;
        var regionHandler = await client.ConnectToNameserverAndWaitForRegionsAsync(appSettings);
        return regionHandler.EnabledRegions.Select(r => new QuantumMenuOnlineRegion { Code = r.Code, Ping = r.Ping }).ToList();
      } catch (Exception e) {
        Debug.LogException(e);
        return null;
      }
    }

    /// <summary>
    /// Connect to a Photon game server and start the Quantum simluation based on the connection arguments.
    /// The default implementation will also load the required Unity scene, wait until the Quantum simluation has been started and add selected players.
    /// </summary>
    /// <param name="connectArgs">Connection args</param>
    /// <returns>A connect result when successfully connected and started the game.</returns>
    public async Task<ConnectResult> ConnectAsync(IQuantumMenuConnectArgs connectArgs) {
      if (_cancellation != null) {
        throw new Exception("Connection instance still in use");
      }

      var quantumConnectArgs = (QuantumMenuConnectArgs)connectArgs;

      // CONNECT ---------------------------------------------------------------

      // Cancellation is used to stop the connection process at any time.
      _cancellation = new CancellationTokenSource();
      _linkedCancellation = AsyncSetup.CreateLinkedSource(_cancellation.Token);
      _shutdownFlags = quantumConnectArgs.ShutdownFlags;
      _disconnectCause = DisconnectCause.None;

      var arguments = new MatchmakingArguments {
        PhotonSettings = new AppSettings(quantumConnectArgs.AppSettings) { 
          AppVersion = connectArgs.AppVersion,
          FixedRegion = connectArgs.PreferredRegion
        },
        ReconnectInformation = quantumConnectArgs.ReconnectInformation,
        EmptyRoomTtlInSeconds = quantumConnectArgs.ServerSettings.EmptyRoomTtlInSeconds,
        PlayerTtlInSeconds = quantumConnectArgs.ServerSettings.PlayerTtlInSeconds,
        MaxPlayers = quantumConnectArgs.MaxPlayerCount,
        RoomName = quantumConnectArgs.Session,
        CanOnlyJoin = string.IsNullOrEmpty(quantumConnectArgs.Session) == false && !quantumConnectArgs.Creating,
        PluginName = quantumConnectArgs.PhotonPluginName,
        AsyncConfig = new AsyncConfig() {
          TaskFactory = AsyncConfig.CreateUnityTaskFactory(),
          CancellationToken = _linkedCancellation.Token
        },
        NetworkClient = quantumConnectArgs.Client,
        AuthValues = quantumConnectArgs.AuthValues,
      };


      // Connect to Photon
      try {
        if (quantumConnectArgs.Reconnecting == false) {
          UIController.Get<QuantumMenuUILoading>().SetStatusText("Connecting..");
          Client = await MatchmakingExtensions.ConnectToRoomAsync(arguments);
        } else {
          UIController.Get<QuantumMenuUILoading>().SetStatusText("Reconnecting..");
          Client = await MatchmakingExtensions.ReconnectToRoomAsync(arguments);
        }
      } catch (Exception e) {
        Debug.LogException(e);
        return new ConnectResult {
          FailReason =
            AsyncConfig.Global.IsCancellationRequested ? ConnectFailReason.ApplicationQuit :
            _disconnectCause == DisconnectCause.None ? ConnectFailReason.RunnerFailed : ConnectFailReason.Disconnect,
          DisconnectCause = (int)_disconnectCause,
          DebugMessage = e.Message,
          WaitForCleanup = CleanupAsync()};
      }

      // Save region summary
      if (!string.IsNullOrEmpty(Client.SummaryToCache)) {
        quantumConnectArgs.ServerSettings.BestRegionSummary = Client.SummaryToCache;
      }

      //  Make sure to notice socket disconnects during the rest of the connection/start process
      _disconnectSubscription = Client.CallbackMessage.ListenManual<OnDisconnectedMsg>(m => {
        if (_cancellation != null && _cancellation.IsCancellationRequested == false) {
          _disconnectCause = m.cause;
          _cancellation.Cancel();
        }
      });

      // LOAD SCENE ---------------------------------------------------------------

      var preloadMap = false;
      if (quantumConnectArgs.RuntimeConfig != null 
        && quantumConnectArgs.RuntimeConfig.Map.Id.IsValid 
        && quantumConnectArgs.RuntimeConfig.SimulationConfig.Id.IsValid) {
        if (QuantumUnityDB.TryGetGlobalAsset(quantumConnectArgs.RuntimeConfig.SimulationConfig, out Quantum.SimulationConfig simulationConfigAsset)) {
          // Only preload the scene if SimulationConfig.AutoLoadSceneFromMap is not enabled.
          // Caveat: preloading the scene here only works if the runtime config is not expected to change (e.g. by other clients/random matchmaking or webhooks)
          preloadMap = simulationConfigAsset.AutoLoadSceneFromMap == SimulationConfig.AutoLoadSceneFromMapMode.Disabled;
        }
      }

      if (preloadMap) {

        UIController.Get<QuantumMenuUILoading>().SetStatusText("Loading..");
        
        if (QuantumUnityDB.TryGetGlobalAsset(quantumConnectArgs.RuntimeConfig.Map, out Quantum.Map map)) {
          return new ConnectResult {
            FailReason = ConnectFailReason.MapNotFound,
            DebugMessage = $"Requested map {quantumConnectArgs.RuntimeConfig.Map} not found.",
            WaitForCleanup = CleanupAsync()
          };
        }

        using (new ConnectionServiceScope(Client)) {
          try {
            // Load Unity scene async
            await SceneManager.LoadSceneAsync(map.Scene, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.Scene));
            _loadedScene = map.Scene;

            // Check if cancellation was triggerd while loading the map
            if (_linkedCancellation.Token.IsCancellationRequested) {
              throw new TaskCanceledException();
            }

          } catch (Exception e) {
            Debug.LogException(e);
            return new ConnectResult {
              FailReason =
                AsyncConfig.Global.IsCancellationRequested ? ConnectFailReason.ApplicationQuit :
                _disconnectCause == DisconnectCause.None ? ConnectFailReason.RunnerFailed : ConnectFailReason.Disconnect,
              DisconnectCause = (int)_disconnectCause,
              DebugMessage = e.Message,
              WaitForCleanup = CleanupAsync()
            };
          }

          SceneManager.SetActiveScene(SceneManager.GetSceneByName(map.Scene));
        }
      }

      // START GAME ---------------------------------------------------------------

      UIController.Get<QuantumMenuUILoading>().SetStatusText("Starting..");

      var sessionRunnerArguments = new SessionRunner.Arguments {
        RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
        ClientId = quantumConnectArgs.QuantumClientId ?? arguments.UserId,
        RuntimeConfig = quantumConnectArgs.RuntimeConfig,
        SessionConfig = quantumConnectArgs.SessionConfig?.Config ?? QuantumDeterministicSessionConfigAsset.DefaultConfig,
        GameMode = DeterministicGameMode.Multiplayer,
        PlayerCount = quantumConnectArgs.MaxPlayerCount,
        Communicator = new QuantumNetworkCommunicator(Client),
        CancellationToken = _linkedCancellation.Token,
        RecordingFlags = quantumConnectArgs.RecordingFlags,
        InstantReplaySettings = quantumConnectArgs.InstantReplaySettings,
        DeltaTimeType = quantumConnectArgs.DeltaTimeType,
      };

        // Register to plugin disconnect messages to display errors
      string pluginDisconnectReason = null;
      var pluginDisconnectListener = QuantumCallback.SubscribeManual<CallbackPluginDisconnect>(m => pluginDisconnectReason = m.Reason);

      try {
        // Start Quantum and wait for the start protocol to complete
        Runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);
      } catch (Exception e) {
        pluginDisconnectListener.Dispose();
        Debug.LogException(e);
        return new ConnectResult {
          FailReason = DetermineFailReason(_disconnectCause, pluginDisconnectReason),
          DisconnectCause = (int)_disconnectCause,
          DebugMessage = pluginDisconnectReason ?? e.Message,
          WaitForCleanup = CleanupAsync()
        };
      }

      pluginDisconnectListener.Dispose();
      _cancellation.Dispose();
      _cancellation = null;
      _linkedCancellation.Dispose();
      _linkedCancellation = null;
      _disconnectSubscription.Dispose();
      _disconnectSubscription = null;

      for (int i = 0; i < quantumConnectArgs.RuntimePlayers.Length; i++) { 
        Runner.Game.AddPlayer(i, quantumConnectArgs.RuntimePlayers[i]);
      }

      return new ConnectResult { Success = true }; 
    }

    /// <summary>
    /// Match errors to one error number.
    /// </summary>
    /// <param name="disconnectCause">Photon disconnect reason</param>
    /// <param name="pluginDisconnectReason">Plugin disconnect message</param>
    /// <returns></returns>
    public static int DetermineFailReason(DisconnectCause disconnectCause, string pluginDisconnectReason) {
      if (AsyncConfig.Global.IsCancellationRequested) {
        return ConnectFailReason.ApplicationQuit;
      }

      switch (disconnectCause) {
        case DisconnectCause.None:
          return ConnectFailReason.RunnerFailed;
        case DisconnectCause.DisconnectByClientLogic:
          if (string.IsNullOrEmpty(pluginDisconnectReason) == false) {
            return ConnectFailReason.PluginError;
          }
          return ConnectFailReason.Disconnect;
        default:
          return ConnectFailReason.Disconnect;
      }
    }

    /// <summary>
    /// Disrupt the connection process and disconnect. If possible the scene will also be unloaded.
    /// If still in the process of connection the task cancellation is triggered.
    /// This class can be kept around to cleanup after gameplay has been completed, but does not have to.
    /// </summary>
    /// <param name="reason"><see cref="ConnectFailReason"/></param>
    /// <returns></returns>
    public Task DisconnectAsync(int reason) {
      if (_cancellation != null) {
        // Cancel connection logic and let the code handle cancel errors
        _cancellation.Cancel();
        return Task.CompletedTask;
      } else {
        if (reason == ConnectFailReason.UserRequest) {
          QuantumReconnectInformation.Reset();
        }

        // Stop the running game
        return CleanupAsync();
      }
    }

    private async Task CleanupAsync() {
      _cancellation?.Dispose();
      _cancellation = null;
      _linkedCancellation?.Dispose();
      _linkedCancellation = null;
      _disconnectSubscription?.Dispose();
      _disconnectSubscription = null;

      if (Runner != null && (_shutdownFlags & QuantumMenuConnectionShutdownFlag.ShutdownRunner) >= 0) {
        try {
          if (AsyncConfig.Global.IsCancellationRequested) {
            Runner.Shutdown();
          } else {
            await Runner.ShutdownAsync();
          }
        } catch (Exception e) {
          Debug.LogException(e);
        }
      }
      Runner = null;

      if (Client != null && (_shutdownFlags & QuantumMenuConnectionShutdownFlag.Disconnect) >= 0) {
        try {
          if (AsyncConfig.Global.IsCancellationRequested) {
            Client.Disconnect();
          } else {
            await Client.DisconnectAsync();
          }
        } catch (Exception e) {
          Debug.LogException(e);
        }
      }
      Client = null;

      if (string.IsNullOrEmpty(_loadedScene) == false &&
         (_shutdownFlags & QuantumMenuConnectionShutdownFlag.ShutdownRunner) >= 0 &&
        AsyncConfig.Global.IsCancellationRequested == false) {
        try {
          await SceneManager.UnloadSceneAsync(_loadedScene);
        } catch (Exception e) {
          Debug.LogException(e);
        }
      }
      _loadedScene = null;
    }
  }
}
