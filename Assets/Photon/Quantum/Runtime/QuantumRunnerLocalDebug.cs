namespace Quantum {
  using System;
  using System.Collections;
  using System.Linq;
  using Photon.Deterministic;
  using UnityEngine;
#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
  using UnityEngine.AddressableAssets;
#endif
  using UnityEngine.Events;
  using UnityEngine.SceneManagement;
  using UnityEngine.Serialization;
  using static QuantumUnityExtensions;

  /// <summary>
  ///   A debug script that starts the Quantum simulation for <see cref="MaxPlayerCount" /> players when starting the game
  ///   from a gameplay scene.
  ///   Will add <see cref="LocalPlayers" /> as local players during simulation start.
  ///   The script will disable itself when it detectes that other scene were loaded before this (to delegate adding players
  ///   to a menu scene / game bootstrap).
  /// </summary>
  public class QuantumRunnerLocalDebug : QuantumMonoBehaviour {
    /// <summary>
    ///   Set the <see cref="DeltaTypeType" /> to <see cref="SimulationUpdateTime.EngineDeltaTime" /> to not progress the
    ///   simulation during break points.
    ///   Has to be set before starting the runner and can only be changed on the runner directly during runtime: <see cref="SessionRunner.DeltaTimeType"/>.
    /// </summary>
    [Tooltip("Set the DeltaTimeType to EngineDeltaTime to not progress the simulation time during break points.")]
    public SimulationUpdateTime DeltaTypeType = SimulationUpdateTime.EngineDeltaTime;

    [Tooltip("Set RecordingFlags to Default to record input and checksums to enable saving a replay.")]
    public RecordingFlags RecordingFlags = RecordingFlags.Default;

    [Tooltip("Customize the InstantReplaySettings.")]
    public InstantReplaySettings InstantReplayConfig = InstantReplaySettings.Default;

    [FormerlySerializedAs("Config")]
    [Tooltip("RuntimeConfig used for this simulation.")]
    public RuntimeConfig RuntimeConfig;

    [Tooltip("SessionConfig used for this simulation, if null it will search for the global SessionConfig.")]
    public QuantumDeterministicSessionConfigAsset SessionConfig;

    [Tooltip("The local players that are added to the simulation during game start.")]
    [FormerlySerializedAs("Players")]
    public RuntimePlayer[] LocalPlayers;

    [Tooltip("Overwrite the max player count for this simulation otherwise Quantum.Constants.PLAYER_COUNT is used.\nDefault is 0.")]
    public int MaxPlayerCount;

    [Tooltip("Set a factor to increas or decrease the simulation speed and update the simulation during Update(). \nDefault is 1.")]
    public float SimulationSpeedMultiplier = 1.0f;

    [Tooltip("Show the reload simulation button")]
    public bool DisplaySaveAndReloadButton;

    [Tooltip("Enabled loading Addressables before simulation start.")]
    public bool PreloadAddressables = false;

    [Tooltip("Set a dynamic asset db.")]
    public DynamicAssetDBSettings DynamicAssetDB;

    [Tooltip("Enabled the Quantum task profiler. Must be set before starting. Works with debug and release Quantum dlls.")]
    public bool IsTaskProfilerEnabled;

#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
    public async void Start()
#else
  public void Start()
#endif
    {
      if (QuantumRunner.Default != null || SceneManager.sceneCount > 1) {
        // Prevents to start the simulation (again/twice) when..
        // a) there already is a runner, because the scene is releaded during Quantum Unity map loading (AutoLoadSceneFromMap) or
        // b) this scene is not the first scene that is ever loaded (most likely a menu scene is involved that starts the simulation itself)
        enabled = false;
        return;
      }

      // Subscribe to the game started callback to add players
      QuantumCallback.Subscribe(this, (CallbackGameStarted c) => OnGameStarted(c.Game, c.IsResync), game => game == QuantumRunner.Default.Game);

#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
      if (PreloadAddressables) {
        // there's also an overload that accepts a target list paramter
        var addressableAssets = QuantumUnityDB.Global.Entries
         .Where(x => x.Source is QuantumAssetObjectSourceAddressable)
         .Select(x => (x.Guid, ((QuantumAssetObjectSourceAddressable)x.Source).Address));
        
        // preload all the addressable assets
        foreach (var (assetRef, address) in addressableAssets) {
          // there are a few ways to load an asset with Addressables (by label, by IResourceLocation, by address etc.)
          // but it seems that they're not fully interchangeable, i.e. loading by label will not make loading by address
          // be reported as done immediately; hence the only way to preload an asset for Quantum is to replicate
          // what it does internally, i.e. load with the very same parameters
          await Addressables.LoadAssetAsync<Quantum.AssetObject>(address).Task;
        }
      }
#endif
      RuntimeConfig.Seed = (int)DateTime.Now.Ticks;
      StartWithFrame(0, null);
    }

    public void StartWithFrame(int frameNumber = 0, byte[] frameData = null) {
      Log.Debug("### Starting quantum in local debug mode ###");

      var mapdata = FindFirstObjectByType<QuantumMapData>();
      if (mapdata == null) {
        throw new Exception("No MapData object found, can't debug start scene");
      }

      // copy runtime config
      var serializer = new QuantumUnityJsonSerializer();
      var runtimeConfig = serializer.CloneConfig(RuntimeConfig);

      // set map to this maps asset
      runtimeConfig.Map = mapdata.Asset;

      // if not set, try to set simulation config from global default configs
      if (runtimeConfig.SimulationConfig.Id.IsValid == false && QuantumDefaultConfigs.TryGetGlobal(out var defaultConfigs)) {
        runtimeConfig.SimulationConfig = defaultConfigs.SimulationConfig;
      }

      var dynamicDB = new DynamicAssetDB();
      DynamicAssetDB.OnInitialDynamicAssetsRequested?.Invoke(dynamicDB);

      // create start game parameter
      var arguments = new SessionRunner.Arguments {
        RunnerFactory         = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters        = QuantumRunnerUnityFactory.CreateGameParameters,
        RuntimeConfig         = runtimeConfig,
        SessionConfig         = SessionConfig?.Config ?? QuantumDeterministicSessionConfigAsset.DefaultConfig,
        ReplayProvider        = null,
        GameMode              = DeterministicGameMode.Local,
        InitialFrame          = frameNumber,
        FrameData             = frameData,
        RunnerId              = "LOCALDEBUG",
        PlayerCount           = MaxPlayerCount > 0 ? Math.Min(MaxPlayerCount, Input.MAX_COUNT) : Input.MAX_COUNT,
        InstantReplaySettings = InstantReplayConfig,
        InitialDynamicAssets  = dynamicDB,
        DeltaTimeType         = DeltaTypeType,
        GameFlags             = IsTaskProfilerEnabled ? QuantumGameFlags.EnableTaskProfiler : 0,
        RecordingFlags        = RecordingFlags
      };

      var runner = QuantumRunner.StartGame(arguments);
    }

    private void OnGameStarted(QuantumGame game, bool isResync) {
      for (Int32 i = 0; i < LocalPlayers.Length; ++i) {
        game.AddPlayer(i, LocalPlayers[i]);
      }
    }

    public void OnGUI() {
      if (DisplaySaveAndReloadButton && QuantumRunner.Default != null && QuantumRunner.Default.Id == "LOCALDEBUG") {
        if (GUI.Button(new Rect(Screen.width - 150, 10, 140, 25), "Save And Reload")) {
          StartCoroutine(SaveAndReload());
        }
      }
    }

    public void Update() {
      if (QuantumRunner.Default != null && QuantumRunner.Default.Session != null) {
        QuantumRunner.Default.IsSessionUpdateDisabled = SimulationSpeedMultiplier != 1.0f;
        if (QuantumRunner.Default.IsSessionUpdateDisabled) {
          switch (QuantumRunner.Default.DeltaTimeType) {
            case SimulationUpdateTime.Default:
            case SimulationUpdateTime.EngineUnscaledDeltaTime:
              QuantumRunner.Default.Session.Update(Time.unscaledDeltaTime * SimulationSpeedMultiplier);
              QuantumUnityDB.UpdateGlobal();
              break;
            case SimulationUpdateTime.EngineDeltaTime:
              QuantumRunner.Default.Session.Update(Time.deltaTime);
              QuantumUnityDB.UpdateGlobal();
              break;
          }
        }
      }
    }

    IEnumerator SaveAndReload() {
      var frameNumber = QuantumRunner.Default.Game.Frames.Verified.Number;
      var frameData = QuantumRunner.Default.Game.Frames.Verified.Serialize(DeterministicFrameSerializeMode.Blit);

      Log.Info($"Serialized Frame size: {frameData.Length} bytes");

      QuantumRunner.ShutdownAll();

      while (QuantumRunner.ActiveRunners.Any()) {
        yield return null;
      }

      StartWithFrame(frameNumber, frameData);
    }

    [Serializable]
    public struct DynamicAssetDBSettings {
      [Serializable]
      public class InitialDynamicAssetsRequestedUnityEvent : UnityEvent<DynamicAssetDB> {
      }

      public InitialDynamicAssetsRequestedUnityEvent OnInitialDynamicAssetsRequested;
    }
  }
}