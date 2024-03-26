namespace Quantum.Editor {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Photon.Client.StructWrapping;
  using UnityEditor;
  using UnityEditor.IMGUI.Controls;
  using UnityEngine;
  using Object = UnityEngine.Object;

  public class QuantumUnityDBInspector : EditorWindow {
    private Grid _grid = new Grid();

    private void OnEnable() {
      _grid.OnEnable();
    }

    private void OnInspectorUpdate() {
      _grid.OnInspectorUpdate();
    }

    private void OnGUI() {

      using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
        _grid.DrawToolbarReloadButton();
        
        if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) {
          QuantumUnityDBUtilities.ExportAsJson();
        }
      
        _grid.DrawToolbarSyncSelectionButton();
        
        GUILayout.FlexibleSpace();
        
        _grid.DrawToolbarSearchField();
        
        EditorGUI.BeginChangeCheck();
        _grid.OnlyGuidOverrides = GUILayout.Toggle(_grid.OnlyGuidOverrides, "Only GUID Overrides", EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck()) {
          _grid.ResetTree();
        }
      }
      
      
      var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
      _grid.OnGUI(rect);
    }

    [MenuItem("Window/Quantum/Quantum Unity DB")]
    [MenuItem("Tools/Quantum/Window/Quantum Unity DB", false, (int)QuantumEditorMenuPriority.Window + 1)]
    public static void ShowWindow() {
      var window = GetWindow<QuantumUnityDBInspector>(false, ObjectNames.NicifyVariableName(nameof(QuantumUnityDB)));
      window.Show();
    }



    class GridItem : QuantumGridItem {
      public readonly bool                                 IsMainAsset;
      public readonly string                               Name;
      public readonly LazyLoadReference<Quantum.AssetObject> Reference;

      public GridItem(int instanceId, string name, bool isMainAsset) {
        Reference   = new LazyLoadReference<Quantum.AssetObject>(instanceId);
        Name        = name;
        IsMainAsset = isMainAsset;
      }

      public override Object TargetObject => Reference.asset;

      public Type QuantumAssetType {
        get {
          var loaded = Reference.asset;
          if (!loaded) {
            return null;
          }
          return loaded.GetType();
        }
      }

      public string UnityPath => AssetDatabase.GUIDToAssetPath(UnityGuid);

      public string UnityGuid {
        get {
          if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Reference.instanceID, out var guid, out long _)) {
            return default;
          }

          return guid;
        }
      }

      public AssetGuid QuantumGuid => QuantumUnityDBUtilities.GetExpectedAssetGuid(Reference.instanceID, out _);

      public string QuantumPath => QuantumUnityDBUtilities.GetExpectedAssetPath(Reference.instanceID, Name, IsMainAsset);
    
      public string AssetType => QuantumAssetType?.Name ?? "Unknown";

      public AssetObjectState LoadState {
        get {
          return QuantumUnityDB.GetGlobalAssetState(QuantumGuid);
        }
      }

      public bool IsQuantumGuidOverriden => QuantumUnityDBUtilities.IsAssetGuidOverriden(Reference.instanceID);

      // public bool IsLoadedInRunner(QuantumRunner runner) {
      //   return runner.Game.IsAssetLoaded(QuantumGuid);
      // }
      //
    }

    [Serializable]
    class Grid : QuantumGrid<GridItem> {
      
      public bool OnlyGuidOverrides;
      
      protected override IEnumerable<Column> CreateColumns() {
        yield return MakeSimpleColumn(x => x.QuantumGuid, new() {
          headerContent = new GUIContent("GUID"),
          width         = 150,
          maxWidth      = 150,
          cellGUI = (item, rect, selected, focused) => {
            TreeView.DefaultGUI.Label(rect, item.QuantumGuid + (item.IsQuantumGuidOverriden ? "*" : ""), selected, focused);
          },
          getComparer = (order) => (a, b) => a.QuantumGuid.CompareTo(b.QuantumGuid) * order,
        });
        yield return MakeSimpleColumn(x => x.LoadState, new() {
          headerContent   = new GUIContent("", "State"),
          maxWidth        = 40,
          width           = 40,
          contextMenuText = "State",
          cellGUI = (item, rect, _, _) => {
            var   icon = QuantumEditorSkin.LoadStateIcon;
            Color color;
            switch (item.LoadState) {
              case AssetObjectState.Loaded:
                color = Color.green;
                break;
              case AssetObjectState.Loading:
                color = Color.yellow;
                break;
              case AssetObjectState.Disposing:
                color = Color.magenta;
                break;
              case AssetObjectState.NotLoaded:
                color = Color.gray;
                break;
              case AssetObjectState.Error:
                color = Color.red;
                break;
              default:
                icon  = QuantumEditorSkin.ErrorIcon;
                color = Color.white;
                break;
            }
        
            using (new QuantumEditorGUI.ContentColorScope(color)) {
              EditorGUI.LabelField(rect, new GUIContent(icon, item.LoadState.ToString()));
            }
          },
        });
        yield return MakeSimpleColumn(x => x.QuantumPath, new() {
          headerContent   = new GUIContent("Path"),
          width           = 600,
          sortedAscending = true
        });
        yield return MakeSimpleColumn(x => x.Name, new() {
          headerContent    = new GUIContent("Name"),
          initiallyVisible = false,
        });
        yield return MakeSimpleColumn(x => x.IsQuantumGuidOverriden, new() {
          headerContent    = new GUIContent("GUID Override"),
          initiallyVisible = false,
          cellGUI = (item, rect, _, _) => EditorGUI.Toggle(rect, item.IsQuantumGuidOverriden),
          getSearchText = item => null,
        });
        yield return MakeSimpleColumn(x => x.AssetType, new() {
          headerContent = new GUIContent("Asset Type"),
          autoResize    = false,
          width         = 200,
          cellGUI = (item, rect, selected, focused) => {
            var assetType = item.QuantumAssetType;
            if (assetType == null) {
              TreeView.DefaultGUI.Label(rect, "<Unknown>", selected, focused);
            } else {
              using (new QuantumEditorGUI.ContentColorScope(QuantumEditorUtility.GetPersistentColor(assetType.FullName, 192))) {
                TreeView.DefaultGUI.Label(rect, assetType.Name, selected, focused);
              }
            }
          }
        });
        yield return new() {
          headerContent    = new GUIContent("Unity Path"),
          initiallyVisible = false,
        };
        yield return new() {
          headerContent    = new GUIContent("Unity GUID"),
          initiallyVisible = false,
        };
      }

      protected override IEnumerable<GridItem> CreateRows() {
        foreach (var it in QuantumUnityDBUtilities.IterateAssets()) {
          if (OnlyGuidOverrides) {
            if (!QuantumUnityDBUtilities.IsAssetGuidOverriden(it.instanceID)) {
              continue;
            }
          }
        
          yield return new GridItem(it.instanceID, it.name, it.isMainRepresentation) { id = it.instanceID };
        }
      }

      protected override GenericMenu CreateContextMenu(GridItem item, TreeView treeView) {

        var assets = treeView.GetSelection()
          .Select(x => EditorUtility.InstanceIDToObject(x) as Quantum.AssetObject)
          .Where(x => x)
          .ToArray();

        var anyOverriden = assets.Any(x => QuantumUnityDBUtilities.IsAssetGuidOverriden(x));

        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Copy GUID (as Text)"), false, g => GUIUtility.systemCopyBuffer = (string)g, item.QuantumGuid.ToString());
        menu.AddItem(new GUIContent("Copy GUID (as Long)"), false, g => GUIUtility.systemCopyBuffer = (string)g, item.QuantumGuid.Value.ToString());
        menu.AddItem(new GUIContent("Copy Path"), false, g => GUIUtility.systemCopyBuffer           = (string)g, item.QuantumPath);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("GUID Override"), anyOverriden, () => {
          QuantumUnityDBUtilities.SetAssetGuidDeterministic(assets, anyOverriden, warnIfChange: true);
          AssetDatabase.SaveAssets();
        });
        menu.AddSeparator("");

        menu.AddDisabledItem(new GUIContent("Default Resource Manager"));

        IResourceManager resourceManager = QuantumUnityDB.Global;

        var loadedGuids = new HashSet<AssetGuid>(assets.Select(x => x.Guid).Where(x => resourceManager.GetAssetState(x) == AssetObjectState.Loaded).ToArray());

        int loadedAssetCount = assets.Count(x => loadedGuids.Contains(x.Guid));
        if (assets.Length != loadedAssetCount) {
          menu.AddItem(new GUIContent("Load"), false, () => {
            foreach (var asset in assets) {
              try {
                resourceManager.GetAsset(asset.Guid);
              } catch (Exception) { }
            }

            resourceManager.Update();
          });
        } else {
          menu.AddDisabledItem(new GUIContent("Load"));
        }

        if (loadedAssetCount != 0) {
          menu.AddItem(new GUIContent("Unload"), false, () => {
            foreach (var asset in assets) {
              try {
                resourceManager.DisposeAsset(asset.Guid);
              } catch (Exception) { }
            }

            resourceManager.Update();
          });
        } else {
          menu.AddDisabledItem(new GUIContent("Unload"));
        }

        return menu;
      }
    }
  }
}
