namespace Quantum.Editor {
  using Quantum;
  using System;
  using System.Collections.Generic;
  using UnityEditor;
  using UnityEngine;
  
  public class QuantumEditorShortcutsWindow : EditorWindow {
    public static float ButtonWidth = 200.0f;

    [MenuItem("Tools/Quantum/Find Config/Photon Server Settings", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 1)]
    public static void SearchPhotonServerSettings() => SearchAndSelect<PhotonServerSettings>();

    [MenuItem("Tools/Quantum/Find Config/Quantum Editor Settings", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 2)]
    public static void SearchQuantumEditorSettings() => SearchAndSelect<QuantumEditorSettings>("QuantumEditorSettings");

    [MenuItem("Tools/Quantum/Find Config/Quantum Gizmo Settings", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 3)]
    public static void SearchQuantumGizmoSettings() => SearchAndSelect<QuantumGameGizmosSettingsScriptableObject>("QuantumGameGizmosSettings");

    [MenuItem("Tools/Quantum/Find Config/Quantum Default Configs", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 4)]
    public static void SearchDefaultConfigs() => SearchAndSelect<QuantumDefaultConfigs>();

    [MenuItem("Tools/Quantum/Find Config/Quantum Session Config", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 5)]
    public static void SearchSessionConfig() => SearchAndSelect<QuantumDeterministicSessionConfigAsset>();

    [MenuItem("Tools/Quantum/Find Config/Quantum Simulation Config", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 6)]
    public static void SearchSimulationConfig() => SearchAndSelect<Quantum.SimulationConfig>();

    [MenuItem("Tools/Quantum/Find Config/Quantum Unity DB", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 7)]
    public static void SearchUnityDB() => SearchAndSelect<QuantumUnityDB>();

    [MenuItem("Tools/Quantum/Find Config/Quantum Dotnet Build Settings", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 8)]
    public static void SearchQuantumDotnetBuildSettings() {
      if (QuantumDotnetBuildSettings.TryGetGlobal(out var settings)) {
        Selection.activeObject = settings;
      }
    }

    [MenuItem("Tools/Quantum/Find Config/Quantum Dotnet Project Settings", priority = (int)QuantumEditorMenuPriority.GlobalConfigs + 9)]
    public static void SearchQuantumDotnetProjectSettings() {
      if (QuantumDotnetProjectSettings.TryGetGlobal(out var settings)) {
        Selection.activeObject = settings;
      }
    }

    [MenuItem("Window/Quantum/Global Configs")]
    [MenuItem("Tools/Quantum/Window/Global Configs", priority = (int)QuantumEditorMenuPriority.Window + 3)]
    public static void ShowWindow() {
      GetWindow(typeof(QuantumEditorShortcutsWindow), false, "Quantum Global Configs");
    }

    public class GridScope : IDisposable {
      private bool _endHorizontal;

      public GridScope(int columnCount, ref int currentColumn, bool forceClose = false) {
        if (currentColumn % columnCount == 0) {
          GUILayout.BeginHorizontal();
        }

        _endHorizontal = (++currentColumn % columnCount == 0) || forceClose;
      }

      public void Dispose() {
        if (_endHorizontal) { 
          GUILayout.EndHorizontal();
        }
      }
    }

    public virtual void OnGUI() {
      var columnCount = (int)Mathf.Max(EditorGUIUtility.currentViewWidth / ButtonWidth, 1);
      var currentColumn = 0;

      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("NetworkView Icon"), "Photon Server Settings", EditorStyles.miniButton) && PhotonServerSettings.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Grid Icon"), "Session Configs", EditorStyles.miniButton) && QuantumDeterministicSessionConfigAsset.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings"), "Default Configs", EditorStyles.miniButton) && QuantumDefaultConfigs.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings"), "Simulation Configs", EditorStyles.miniButton)) SearchAndSelect<Quantum.SimulationConfig>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("BuildSettings.Editor.Small"), "Editor Settings", EditorStyles.miniButton) && QuantumEditorSettings.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("BuildSettings.Editor.Small"), "Gizmo Settings", EditorStyles.miniButton) && QuantumGameGizmosSettingsScriptableObject.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("BuildSettings.Editor.Small"), "Unity DB", EditorStyles.miniButton)) SearchAndSelect<QuantumUnityDB>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings"), "Dotnet Build Settings", EditorStyles.miniButton) && QuantumDotnetBuildSettings.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings"), "Dotnet Project Settings", EditorStyles.miniButton) && QuantumDotnetProjectSettings.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings"), "Default Config", EditorStyles.miniButton) && QuantumDefaultConfigs.TryGetGlobal(out var settings)) Selection.activeObject = settings;
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("PhysicMaterial Icon"), "Physics Materials", EditorStyles.miniButton)) SearchAndSelect<Quantum.PhysicsMaterial>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("NavMeshData Icon"), "NavMesh Agent Configs", EditorStyles.miniButton)) SearchAndSelect<Quantum.NavMeshAgentConfig>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("CapsuleCollider2D Icon"), "Character Controller 2D", EditorStyles.miniButton)) SearchAndSelect<Quantum.CharacterController2DConfig>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("CapsuleCollider Icon"), "Character Controller 3D", EditorStyles.miniButton)) SearchAndSelect<Quantum.CharacterController3DConfig>();
      }
      using (new GridScope(columnCount, ref currentColumn, true)) {
        if (GUI.Button(DrawIcon("DefaultAsset Icon"), "Asset Database Window", EditorStyles.miniButton)) {
          var windows = (QuantumUnityDBInspector[])UnityEngine.Resources.FindObjectsOfTypeAll(typeof(QuantumUnityDBInspector));
          if (windows.Length > 0) {
            windows[0].Close();
          } else {
            QuantumUnityDBInspector window = (QuantumUnityDBInspector)GetWindow(typeof(QuantumUnityDBInspector), false, "Quantum Asset DB");
            window.Show();
          }
        }
      }
    }

    [Obsolete("Use DrawIcon() without width parameter")]
    public static Rect DrawIcon(string iconName, float width) {
      return DrawIcon(iconName);
    }

    public static Rect DrawIcon(string iconName) {
      var rect = EditorGUILayout.GetControlRect();
      var width = rect.width;
      rect.width = 20;
      EditorGUI.LabelField(rect, EditorGUIUtility.IconContent(iconName));
      rect.xMin  += rect.width;
      rect.width = width - rect.width;
      return rect;
    }

    public static T SearchAndSelect<T>() where T : UnityEngine.Object {
      var t = typeof(T);
      var guids = AssetDatabase.FindAssets("t:" + t.Name, null);
      if (guids.Length == 0) {
        QuantumEditorLog.Log($"No UnityEngine.Objects of type '{t.Name}' found.");
        return null;
      }

      var selectedObjects = new List<UnityEngine.Object>();
      for (int i = 0; i < guids.Length; i++) {
        selectedObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), t));
      }

      Selection.objects = selectedObjects.ToArray();
      return (T)selectedObjects[0];
    }


    public static T SearchAndSelect<T>(AssetGuid assetGuid) where T : UnityEngine.Object {
      var t = typeof(T);
      var guids = AssetDatabase.FindAssets("t:" + t.Name, null);
      if (guids.Length == 0) {
        QuantumEditorLog.Log($"No UnityEngine.Objects of type '{t.Name}' found.");
        return null;
      }

      if (guids.Length < 2) {
        return SearchAndSelect<T>();
      }

      T specificAsset = null;
      for (int i = 0; i < guids.Length; i++) {
        var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), t);
        if (typeof(Quantum.AssetObject).IsAssignableFrom(typeof(T)) &&
          ((Quantum.AssetObject)asset).Identifier.Guid == assetGuid) {
          specificAsset = (T)asset;
          break;
        }
      }

      if (specificAsset == null || Selection.objects.Length == 1 && Selection.objects[0] == specificAsset) {
        return SearchAndSelect<T>();
      }

      Selection.objects = new UnityEngine.Object[1] { specificAsset };
      return specificAsset;
    }

    public static T SearchAndSelect<T>(string name) where T : UnityEngine.Object {
      var t = typeof(T);
      var guids = AssetDatabase.FindAssets("t:" + t.Name, null);
      if (guids.Length == 0) {
        Debug.LogFormat("No UnityEngine.Objects of type '{0}' found.", t.Name);
        return null;
      }

      if (guids.Length < 2) {
        return SearchAndSelect<T>();
      }

      T specificAsset = null;
      for (int i = 0; i < guids.Length; i++) {
        var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), t);
        if (asset.name == name) { 
          specificAsset = (T)asset;
          break;
        }
      }

      if (specificAsset == null || Selection.objects.Length == 1 && Selection.objects[0] == specificAsset) {
        return SearchAndSelect<T>();
      }

      Selection.objects = new UnityEngine.Object[1] { specificAsset };
      return specificAsset;
    }
  }
}
