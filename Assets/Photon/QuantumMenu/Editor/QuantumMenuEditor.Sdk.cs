#if !QUANTUM_DEV

#region Assets/Photon/QuantumMenu/Editor/QuantumMenuConfigEditor.cs

namespace Quantum.Menu.Editor {
  using UnityEditor;
  using UnityEngine.SceneManagement;
  using UnityEngine;
  using System.Linq;
  using System.Collections.Generic;
  using static QuantumUnityExtensions;

  /// <summary>
  /// Custom inspector for <see cref="QuantumMenuConfig"/>
  /// </summary>
  [CustomEditor(typeof(QuantumMenuConfig))]
  public class QuantumMenuConfigEditor : Editor {
    /// <summary>
    /// Overriding drawing.
    /// </summary>
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      if (GUILayout.Button("AddCurrentSceneToAvailableScenes")) {
        AddCurrentScene((QuantumMenuConfig)target, null);
      }

      if (GUILayout.Button("InitializeAllBuildSettingsScenes")) {
        InitializeAllBuildSettingsScenes((QuantumMenuConfig)target);
      }
    }

    /// <summary>
    /// Add the current open Unity scene to a QuantumMenuConfig.
    /// </summary>
    /// <param name="menuConfig">The menu config asset</param>
    /// <param name="systemsConfig">Set an optional <see cref="SystemsConfig"/></param>
    public static void AddCurrentScene(QuantumMenuConfig menuConfig, SystemsConfig systemsConfig) {
      var mapData = FindFirstObjectByType<QuantumMapData>();
      if (mapData == null) {
        QuantumEditorLog.Error($"Map asset not found in current scene");
        return;
      }

      var debugRunner = FindAnyObjectByType<QuantumRunnerLocalDebug>();

      var scene = SceneManager.GetActiveScene();

      var scenePath = PathUtils.MakeSane(scene.path);
      if (menuConfig.AvailableScenes.Any(s => scenePath.Equals(s.ScenePath, System.StringComparison.Ordinal))) {
        return;
      }

      menuConfig.AvailableScenes.Add(new PhotonMenuSceneInfo {
        Map = mapData.Asset,
        Name = scene.name,
        Preview = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath("e43d4c530865a8342957f23fe8a873b2")),
        ScenePath = scenePath,
        SystemsConfig = systemsConfig ?? (debugRunner != null ? debugRunner.RuntimeConfig.SystemsConfig : null)
      });

      AddScenePathToBuildSettings(scenePath);
    }

    /// <summary>
    /// A tool to initialize all Quantum maps as menu scenes.
    /// </summary>
    /// <param name="menuConfig">The menu config</param>
    public static void InitializeAllBuildSettingsScenes(QuantumMenuConfig menuConfig) {
      var mapGuids = QuantumUnityDB.FindGlobalAssetGuids(typeof(Map));
      foreach (var mapGuid in mapGuids) {
        if (mapGuid.IsValid == false) {
          continue;
        }

        var map = QuantumUnityDB.GetGlobalAsset<Map>(mapGuid);
        if (map == null) {
          continue;
        }

        var scenePath = PathUtils.MakeSane(map.ScenePath);
        if (menuConfig.AvailableScenes.Any(s => scenePath.Equals(s.ScenePath, System.StringComparison.Ordinal))) {
          continue;
        }

        menuConfig.AvailableScenes.Add(new PhotonMenuSceneInfo {
          Map = mapGuid,
          Name = map.name,
          Preview = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath("e43d4c530865a8342957f23fe8a873b2")),
          ScenePath = scenePath,
        });

        AddScenePathToBuildSettings(scenePath);
      }
    }

    private static void AddScenePathToBuildSettings(string scenePath) {
      var editorBuildSettingsScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
      if (editorBuildSettingsScenes.FindIndex(s => s.path.Equals(scenePath, System.StringComparison.Ordinal)) < 0) {
        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene { path = scenePath, enabled = true });
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
      }
    }
  }
}

#endregion

#endif
