namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumRunnerLocalSavegame : QuantumMonoBehaviour {
    public  TextAsset             SavegameFile;
    public  TextAsset             DatabaseFile;
    public  string                DatabasePath;
    public  InstantReplaySettings InstantReplayConfig = InstantReplaySettings.Default;
    private ResourceManagerStatic _resourceManager;
    private Native.Allocator      _resourceAllocator;

    public void Start() {
      if (QuantumRunner.Default != null)
        return;

      if (SavegameFile == null) {
        Debug.LogError("QuantumRunnerLocalSavegame - not savegame file selected.");
        return;
      }

      Debug.Log("### Starting quantum in local savegame mode ###");

      // Load replay file in json or bson
      var serializer = new QuantumUnityJsonSerializer();
      var replayFile = JsonUtility.FromJson<ReplayFile>(SavegameFile.text);

      var arguments = new SessionRunner.Arguments {
        RunnerFactory         = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters        = QuantumRunnerUnityFactory.CreateGameParameters,
        RuntimeConfig         = replayFile.RuntimeConfig,
        SessionConfig         = replayFile.DeterministicConfig,
        GameMode              = DeterministicGameMode.Local,
        FrameData             = replayFile.Frame,
        InitialFrame          = replayFile.LastTick,
        RunnerId              = "LOCALSAVEGAME",
        PlayerCount           = replayFile.DeterministicConfig.PlayerCount,
        InstantReplaySettings = InstantReplayConfig,
      };

      if (DatabaseFile != null) {
        // This is potentially breaking, as it introduces UnityDB-ResourceManager duality
        var assets = serializer.AssetsFromByteArray(DatabaseFile.bytes);
        _resourceAllocator        = new QuantumUnityNativeAllocator();
        _resourceManager          = new ResourceManagerStatic(assets, _resourceAllocator);
        arguments.ResourceManager = _resourceManager;
      }

      QuantumRunner.StartGame(arguments);
    }

    private void OnDestroy() {
      _resourceManager?.Dispose();
      _resourceManager = null;
      _resourceAllocator?.Dispose();
      _resourceAllocator = null;
    }
  }
}