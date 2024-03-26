namespace Quantum {
  using Photon.Deterministic;
  using System.IO;
  using UnityEditor;
  using UnityEngine;
#if QUANTUM_ENABLE_NEWTONSOFT
  using Newtonsoft.Json;
#endif

  /// <summary>
  /// An example of how to start a Quantum replay simulation from a replay file.
  /// </summary>
  public class QuantumRunnerLocalReplay : QuantumMonoBehaviour {
    /// <summary>
    /// Set the <see cref="DeltaTypeType" /> to <see cref="SimulationUpdateTime.EngineDeltaTime" /> to not progress the
    /// simulation time during break points.
    /// </summary>
    [Tooltip("Set the DeltaTimeType to EngineDeltaTime to not progress the simulation time during break points.")]
    public SimulationUpdateTime DeltaTypeType = SimulationUpdateTime.EngineDeltaTime;

    /// <summary>
    /// Replay JSON file.
    /// </summary>
    public TextAsset ReplayFile;
    /// <summary>
    /// Quantum asset database Json file.
    /// </summary>
    public TextAsset DatabaseFile;
    /// <summary>
    /// List of checksums to verify against (optional).
    /// </summary>
    public TextAsset ChecksumFile;
    /// <summary>
    /// Simulation speed multiplier to playback the replay in a different speed.
    /// </summary>
    public float SimulationSpeedMultiplier = 1.0f;
    /// <summary>
    /// Toggle the replay gui lable on/off.
    /// </summary>
    public bool ShowReplayLabel;
    /// <summary>
    /// Instant replay configurations to start the replay with.
    /// </summary>
    public InstantReplaySettings InstantReplayConfig = InstantReplaySettings.Default;
    /// <summary>
    /// Force Unity json deserialization of the deplay file even when Newtonsoft is available.
    /// Newtonsoft is very low when compiled with IL2CPP. But Unity Json deserialization expects the byte arrays to be int array instead on base64 strings.
    /// </summary>
    public bool ForceUsingUnityJson;

    QuantumRunner _runner;
    IResourceManager _resourceManager;
    Native.Allocator _resourceAllocator;
    IDeterministicReplayProvider _replayInputProvider;

    void Start() {
      if (_runner != null)
        return;

      if (ReplayFile == null) {
        Debug.LogError("QuantumRunnerLocalReplay - not replay file selected.");
        return;
      }

      var serializer = new QuantumUnityJsonSerializer();
      var replayFile = JsonUtility.FromJson<ReplayFile>(ReplayFile.text);
      
      if (replayFile == null) {
        Debug.LogError("Failed to read replay file or file is empty.");
        return;
      }

      if ((replayFile.InputHistory == null || replayFile.InputHistory.Length == 0) && (replayFile.InputHistoryRaw == null || replayFile.InputHistoryRaw.Length == 0)) {
        Debug.LogError("Failed to read input history.");
        return;
      }

      Debug.Log("### Starting quantum in local replay mode ###");

      // Create a new input provider from the replay file
      if (replayFile.InputHistory != null && replayFile.InputHistory.Length > 0) {
        _replayInputProvider = new InputProvider(replayFile.InputHistory);
      } else {
        var inputStream = new Photon.Deterministic.BitStream(replayFile.InputHistoryRaw);
        _replayInputProvider = new BitStreamReplayInputProvider(inputStream, replayFile.LastTick);
      }

      // Runtime Config can be binary or JSON
      var runtimeConfig = replayFile.RuntimeConfig;
      if (replayFile.RuntimeConfigBinary != null && replayFile.RuntimeConfigBinary.Length > 0) {
        runtimeConfig = serializer.ConfigFromByteArray<RuntimeConfig>(replayFile.RuntimeConfigBinary, compressed: true);
      }

      var arguments = new SessionRunner.Arguments {
        RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
        RuntimeConfig = runtimeConfig,
        SessionConfig = replayFile.DeterministicConfig,
        ReplayProvider = _replayInputProvider,
        GameMode = DeterministicGameMode.Replay,
        RunnerId = "LOCALREPLAY",
        PlayerCount = replayFile.DeterministicConfig.PlayerCount,
        InstantReplaySettings = InstantReplayConfig,
        InitialFrame = replayFile.InitialFrame,
        FrameData = replayFile.InitialFrameData,
        DeltaTimeType = DeltaTypeType,
      };

      if (DatabaseFile != null) {
        // This is potentially breaking, as it introduces UnityDB-ResourceManager duality 
        var assets = serializer.AssetsFromByteArray(DatabaseFile.bytes);
        _resourceAllocator = new QuantumUnityNativeAllocator();
        _resourceManager = new ResourceManagerStatic(assets, new QuantumUnityNativeAllocator());
        arguments.ResourceManager = _resourceManager;
      }

      _runner = QuantumRunner.StartGame(arguments);

      if (ChecksumFile != null) {
        var checksumFile = JsonUtility.FromJson<ChecksumFile>(ChecksumFile.text);
        Assert.Check(checksumFile);
        _runner.Game.StartVerifyingChecksums(checksumFile);
      }
    }

    public void Update() {
      if (QuantumRunner.Default != null && QuantumRunner.Default.Session != null) {
        // Set the session ticking to manual to inject custom delta time.
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

#if UNITY_EDITOR
      if (_replayInputProvider != null && _runner.Session?.IsReplayFinished == true) {
        EditorApplication.isPaused = true;
      }
#endif
    }

    private void OnDestroy() {
      _resourceManager?.Dispose();
      _resourceManager = null;
      _resourceAllocator?.Dispose();
      _resourceAllocator = null;
    }

#if UNITY_EDITOR
    private float guiTimer;

    void OnGUI() {
      if (ShowReplayLabel && _replayInputProvider != null && _runner.Session != null) {
        if (_runner.Session.IsReplayFinished) {
          GUI.contentColor = Color.red;
          GUI.Label(new Rect(10, 10, 200, 100), "REPLAY COMPLETED");
        } else {
          guiTimer += Time.deltaTime;
          if (guiTimer % 2.0f > 1.0f) {
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(10, 10, 200, 100), "REPLAY PLAYING");
          }
        }
      }
    }
#endif
  }
}
