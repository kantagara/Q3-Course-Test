namespace Quantum.Editor {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Xml.Linq;
  using UnityEditor;
  using UnityEngine;

  [Serializable]
  [CreateAssetMenu(menuName = "Quantum/Configurations/Dotnet Project Settings", order = EditorDefines.AssetMenuPriorityConfigurations + 61)]
  [QuantumGlobalScriptableObject(DefaultPath)]
  public class QuantumDotnetProjectSettings : QuantumGlobalScriptableObject<QuantumDotnetProjectSettings> {
    public const string DefaultPath = "Assets/QuantumUser/Editor/QuantumDotnetProjectSettings.asset";
    public const string IncludeLabel = "QuantumDotnetInclude";

    public string OutputProjectPath = "../Quantum.Simulation.Gen.csproj";
    public bool IncludeAllQtnAssets = true;
    public bool IncludeAllAssetObjectScripts = true;

    private static string GetUnityProjectRoot {
      get {
        var currentPath = Application.dataPath;
        Debug.Assert(currentPath.EndsWith("/Assets"));
        return currentPath.Substring(0, currentPath.Length - "Assets".Length);
      }
    }
    
    [Header("Files and folders can either be marked with " + IncludeLabel + " label or included here.")]
    public string[] IncludePaths = new[] {
      "Assets/QuantumUser/Simulation",
      "Assets/Photon/Quantum/Simulation"
    };

    [EditorButton("Export")]
    public void Export() {
      Export(null);
    }
    
    public void Export(string outputProjectPath) {
      var includes = new List<string>();
      
      if (IncludeAllQtnAssets) {
        var qtnAssets = AssetDatabase.FindAssets($"t:" + nameof(QuantumQtnAsset)).Select(AssetDatabase.GUIDToAssetPath);
        includes.AddRange(qtnAssets);
      }

      if (IncludeAllAssetObjectScripts) {
        var assetScripts = TypeCache.GetTypesDerivedFrom<AssetObject>()
            .Select(type => {
              var script = (MonoScript)UnityInternal.EditorGUIUtility.GetScript(type.Name);
              if (script == null) {
                return null;
              } else {
                return AssetDatabase.GetAssetPath(script);
              }
            })
            .Where(x => !string.IsNullOrEmpty(x))
            .Where(x => Path.GetExtension(x) == ".cs")
            .Distinct();
        includes.AddRange(assetScripts);
      }

      // ... also, add any file marked with a label
      {
        var labeledAssets = AssetDatabase.FindAssets($"l:{IncludeLabel}")
          .Select(AssetDatabase.GUIDToAssetPath);
        includes.AddRange(labeledAssets);
      }
      
      // now remove duplicates and files otherwise included explicitly
      includes = includes.Distinct().ToList();
      
      // add explicit paths
      foreach (var path in IncludePaths) {
        if (Directory.Exists(path)) {
          // remove any path that starts with this folder
          var dirWithTrailingSlash = path.TrimEnd('/') + "/";
          includes.RemoveAll(x => x.StartsWith(dirWithTrailingSlash));
        }
        
        includes.Add(path);
      }
      
      Export(outputProjectPath ?? OutputProjectPath, includes.ToArray());
    }

    public static void Export(string outputPath, string[] includes) {
      // turn current path into path relative to outputPath
      var contents = GeneratePartialQuantumGameCsProj(
        Path.GetRelativePath(Path.GetDirectoryName(outputPath), GetUnityProjectRoot),
        Path.GetRelativePath(Path.GetDirectoryName(outputPath), QuantumCodeGenSettings.CodeGenQtnFolderPath),
        includes
      );
      
      File.WriteAllText(outputPath, contents);
    }

    static string GeneratePartialQuantumGameCsProj(
      string pathPrefix,
      string codegenOutput,
      string[] includes) {

      pathPrefix = PathUtils.Normalize(pathPrefix);
      var projectElement = new XElement("Project");
      
      projectElement.Add(new XComment("Properties"));
      var properties = new XElement("PropertyGroup");
      properties.Add(new XElement("QuantumCodeGenOutput", codegenOutput));
      projectElement.Add(properties);
      
      projectElement.Add(new XComment("Includes"));
      
      var group = new XElement("ItemGroup");
      
      foreach (var source in includes) {
        if (Directory.Exists(source)) {
          group.Add(new XElement("Compile",
            new XAttribute("Include", $"{pathPrefix}/{source}/**/*.cs"),
            new XAttribute("LinkBase", DropAssetsPrefix(source))));
          group.Add(new XElement("None",
            new XAttribute("Include", $"{pathPrefix}/{source}/**/*.qtn"),
            new XAttribute("LinkBase", DropAssetsPrefix(source))));
        } else {
          group.Add(new XElement(Path.GetExtension(source) == ".cs" ? "Compile" : "None",
            new XAttribute("Include", $"{pathPrefix}/{source}"),
            new XElement("Link", DropAssetsPrefix(source)))
          );
        }
      }
      
      projectElement.Add(group);

      var document = new XDocument(projectElement);
      return document.ToString();
    }

    private static string DropAssetsPrefix(string path) {
      Debug.Assert(path.StartsWith("Assets/"));
      return path.Substring("Assets/".Length);
    }
  }
}