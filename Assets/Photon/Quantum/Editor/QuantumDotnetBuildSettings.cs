namespace Quantum.Editor {
  using System;
  using System.IO;
  using System.IO.Compression;
  using UnityEditor;
  using UnityEngine;

  /// <summary>
  /// A configuration asset to generate and build the non-Unity Quantum simualtion dll.
  /// </summary>
  [Serializable]
  [CreateAssetMenu(menuName = "Quantum/Configurations/Dotnet Build Settings", order = EditorDefines.AssetMenuPriorityConfigurations + 60)]
  [QuantumGlobalScriptableObject(DefaultPath)]
  public class QuantumDotnetBuildSettings : QuantumGlobalScriptableObject<QuantumDotnetBuildSettings> {
    /// <summary>
    /// Default path of the global asset.
    /// </summary>
    public const string DefaultPath = "Assets/QuantumUser/Editor/QuantumDotnetBuildSettings.asset";

    /// <summary>
    /// The platform to build for.
    /// </summary>
    public enum DotnetPlatform {
      /// <summary>
      /// Windows platform
      /// </summary>
      Windows,
      /// <summary>
      /// Linux platform
      /// </summary>
      Linux
    }

    /// <summary>
    /// The configuration to build the dll.
    /// </summary>
    public enum DotnetConfiguration {
      /// <summary>
      /// Release mode
      /// </summary>
      Release,
      /// <summary>
      /// Debug mode
      /// </summary>
      Debug
    }

    /// <summary>
    /// If true, opens and highlights the DLL after compilation.
    /// </summary>
    public bool ShowCompiledDllAfterBuild = true;

    /// <summary>
    /// The project settings to use for the generated csproj.
    /// </summary>
    public QuantumDotnetProjectSettings ProjectSettings;
   
    /// <summary>
    /// The project template to use for the generated csproj.
    /// </summary>
    public TextAsset ProjectTemplate;
   
    /// <summary>
    /// The name of the generated csproj.
    /// </summary>
    public string ProjectFilename = "Quantum.Simulation.Dotnet";
   
    /// <summary>
    /// The path where to output the generated csproj.
    /// </summary>
    public string ProjectOutputPath = "Assets/..";
    
    /// <summary>
    /// Where to output the compiled DLL.
    /// </summary>
    public string BinOutputPath = "Temp/Quantum.Simulation.Dotnet/bin";
    public string LibOutputPath = "Temp/Quantum.Simulation.Dotnet/lib";
    /// <summary>
    /// The path to the Photon Server SDK.
    /// </summary>
    public string PluginSdkPath = "";
    
    /// <summary>
    /// The target platform to build for.
    /// </summary>
    public DotnetPlatform TargetPlatform;
    
    /// <summary>
    /// The target configuration to build for. e.g. Debug or Release.
    /// </summary>
    public DotnetConfiguration TargetConfiguration;


    const string PluginSdkAssetPath = "Photon.Server/deploy_win/Plugins/QuantumPlugin3.0/bin/assets";
    const string PhotonServerPath = "Photon.Server/deploy_win/bin";
    const string PluginSdkLibPath = "Lib";
    const string ProjectAssetDefaultPath = "Assets/Photon/Quantum/Editor/Dotnet/Quantum.Simulation.Dotnet.csproj.txt";
    const string DependencyArchivePath = "Assets/Photon/Quantum/Editor/Dotnet/Quantum.Dotnet.{0}.zip";

    /// <summary>
    /// A quick check if the plugin sdk was found and its path saved.
    /// </summary>
    public bool HasCustomPluginSdk => string.IsNullOrEmpty(PluginSdkPath) == false && Directory.Exists(PluginSdkPath);

    private static string GetUnityProjectRoot {
      get {
        var currentPath = Application.dataPath;
        Debug.Assert(currentPath.EndsWith("/Assets"));
        return currentPath.Substring(0, currentPath.Length - "Assets".Length);
      }
    }
    
    /// <summary>
    /// Try to initialize ProjectSettings and ProjectTemplate when the scriptable object was created.
    /// </summary>
    protected virtual void Awake() {
      if (ProjectSettings == null) {
        QuantumDotnetProjectSettings.TryGetGlobal(out ProjectSettings);
        EditorUtility.SetDirty(this);
      }
      if (ProjectTemplate == null) {
        ProjectTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(ProjectAssetDefaultPath);
        EditorUtility.SetDirty(this);
      }
    }

    /// <summary>
    /// Automatically search for the Photon Server SDK folder.
    /// </summary>
    public void DetectPluginSdk() {
      if (TryFindPluginSdkFolderWithPopup(ref PluginSdkPath) == false) {
        QuantumEditorLog.Warn("Plugin Sdk not found.");
      } else {
        var pluginSdkFullPath = Path.GetFullPath($"{GetUnityProjectRoot}/{PluginSdkPath}");
        QuantumEditorLog.Log("Plugin Sdk found at: " + pluginSdkFullPath);
        EditorUtility.SetDirty(this);
      }
    }

    /// <summary>
    /// Synchronize the Quantum Plugin SDK with the Unity project by exporting the LUT files and Quantum DB and building the project.
    /// </summary>
    /// <param name="settings"></param>
    public static void SynchronizePluginSdk(QuantumDotnetBuildSettings settings) {
      ExportPluginSdkData(settings);
      GenerateProject(settings);
      BuildProject(settings, Path.GetFullPath($"{settings.PluginSdkPath}/{PluginSdkLibPath}"), true);
    }

    /// <summary>
    /// Export the LUT files and Quantum DB to the Quantum Plugin SDK.
    /// </summary>
    /// <param name="settings"></param>
    public static void ExportPluginSdkData(QuantumDotnetBuildSettings settings) {
      ExportLutFiles(Path.GetFullPath($"{settings.PluginSdkPath}/{PluginSdkAssetPath}"));
      ExportQuantumDb(Path.GetFullPath($"{settings.PluginSdkPath}/{PluginSdkAssetPath}/db.json"));
    }

    /// <summary>
    /// Generate a csproj file from the ProjectSettings and ProjectTemplate.
    /// </summary>
    /// <param name="settings">Settings instance</param>
    public static void GenerateProject(QuantumDotnetBuildSettings settings) {
      Assert.Always(settings.ProjectSettings != null, "No Project Settings found");
      Assert.Always(settings.ProjectTemplate != null, "No Project Template found");

      // Export the actual file list
      settings.ProjectSettings.Export($"{settings.ProjectOutputPath}/{settings.ProjectFilename}.csproj.include");

      // Export the csproj template
      File.WriteAllText($"{settings.ProjectOutputPath}/{settings.ProjectFilename}.csproj",
        settings.ProjectTemplate.text);

      // Extract zip folders
      ZipFile.ExtractToDirectory(string.Format(DependencyArchivePath, "Debug"), $"{settings.LibOutputPath}/Debug", true);
      ZipFile.ExtractToDirectory(string.Format(DependencyArchivePath, "Release"), $"{settings.LibOutputPath}/Release", true);
    }

    /// <summary>
    /// Run dotnet build on the generated csproj.
    /// </summary>
    /// <param name="settings">Settings instance</param>
    /// <param name="copyOutputDir">Copy result to output dir</param>
    /// <param name="disablePopup">Disable file explorer popup</param>
    public static void BuildProject(QuantumDotnetBuildSettings settings, string copyOutputDir = null, bool disablePopup = false) {
      var arguments = $" build {Path.GetFullPath(settings.ProjectOutputPath)}/Quantum.Simulation.Dotnet.csproj";
      arguments += $" --configuration {settings.TargetConfiguration}";
      arguments += $" --property:TargetPlatform={settings.TargetPlatform}";
      arguments += $" --property:BaseOutputPath={settings.BinOutputPath}/";
      arguments += $" --property:BaseLibPath={settings.LibOutputPath}/";
      if (string.IsNullOrEmpty(copyOutputDir) == false) {
        arguments += $" --property:CopyOutput=true";
        arguments += $" --property:CopyOutputDir={copyOutputDir}";
      }

      // TODO: Before BUILD try run dotnet command, see if fails, then print error message
      var startInfo = new System.Diagnostics.ProcessStartInfo() {
        FileName = "dotnet",
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      };
      var p = new System.Diagnostics.Process { StartInfo = startInfo };
      p.Start();
      var output = p.StandardOutput.ReadToEnd();
      if (!string.IsNullOrEmpty(output)) {
        if (output.Contains("Build succeeded.")) {
          QuantumEditorLog.Log(output);
          var outputDir = Path.Combine(
            settings.BinOutputPath,
            settings.TargetConfiguration.ToString(),
            "netstandard2.1",
            "Quantum.Simulation.dll"
            );

          if (settings.ShowCompiledDllAfterBuild && disablePopup == false) {
            QuantumEditorLog.Log("Dll was saved to: " + outputDir);
            EditorUtility.RevealInFinder(outputDir);
          }
        } else {
          QuantumEditorLog.Error(output);
        }
      }

      p.WaitForExit();
    }

    /// <summary>
    /// Attempts to find the Photon Server SDK folder. If not found, opens a folder selection dialog.
    /// </summary>
    /// <param name="result">Plugin SDK path</param>
    /// <returns>True when the directory has been found.</returns>
    public static bool TryFindPluginSdkFolderWithPopup(ref string result) {
      if (string.IsNullOrEmpty(result) && TryFindPluginSdkFolder(out result) == false) {
        result = EditorUtility.OpenFolderPanel("Search Quantum Plugin Sdk Directory", Application.dataPath,
          "Photon.Server");
      }

      if (string.IsNullOrEmpty(result)) {
        result = null;
        return false;
      } else {
        result = PathUtils.Normalize(Path.GetRelativePath(GetUnityProjectRoot, result));
        return true;
      }
    }

    /// <summary>
    /// Searching for a folder with the subfolder called Photon.Server inside the unity project and max one above.
    /// </summary>
    /// <param name="result">Plugin SDK path</param>
    /// <returns>True when the Photon.Server directory marked folder can be found automatically.</returns>
    public static bool TryFindPluginSdkFolder(out string result) {
      var currentDirectoryPath = Path.GetFullPath($"{Application.dataPath}");
      var maxDepth = 2;

      for (var i = 0; i < maxDepth; i++) {
        currentDirectoryPath = Path.GetFullPath($"{currentDirectoryPath}/..");
        foreach (var d1 in Directory.GetDirectories(currentDirectoryPath)) {
          foreach (var d2 in Directory.GetDirectories(d1)) {
            if (d2.EndsWith("Photon.Server")) {
              result = d1;
              return true;
            }
          }
        }
      }

      result = null;
      return false;
    }

    /// <summary>
    /// Export the LUT files to the destination path.
    /// </summary>
    /// <param name="destinationPath">The path to export the files.</param>
    public static void ExportLutFiles(string destinationPath) {
      var assetDirectory = Directory.CreateDirectory(destinationPath);
      var assetPath = assetDirectory.FullName;

      // copy lut files
      var lutAssetDirectory = Directory.CreateDirectory($"{assetPath}/LUT");
      var lutAssetPath = lutAssetDirectory.FullName;
      string[] lutFiles = { "FPAcos", "FPAsin", "FPAtan", "FPCos", "FPSin", "FPSinCos", "FPSqrt", "FPTan" };
      foreach (var file in lutFiles) {
        var guids = AssetDatabase.FindAssets(file);
        foreach (var guid in guids) {
          var path = AssetDatabase.GUIDToAssetPath(guid);
          try {
            File.Copy(Path.GetFullPath($"{path}"), $"{lutAssetPath}/{Path.GetFileName(path)}", true);
          } catch (IOException e) {
            QuantumEditorLog.Error(e);
          }

          break;
        }
      }
    }

    /// <summary>
    /// Export the Quantum DB to the destination path.
    /// </summary>
    /// <param name="destinationPath">The path to export the files.</param>
    public static void ExportQuantumDb(string destinationPath) {
      Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
      QuantumUnityDBUtilities.ExportAsJson(destinationPath);
    }

    /// <summary>
    /// Launches PhotonServer.exe from the Plugin SDK folder.
    /// </summary>
    public void LaunchPhotonServer() {
      if (HasCustomPluginSdk == false) {
        QuantumEditorLog.Error("No custom Plugin SDK found.");
        return;
      }

      var arguments = "--run LoadBalancing --config PhotonServer.config";
      var path = Path.Combine(PluginSdkPath, PhotonServerPath);
      var photonServer = Path.Combine(path, "PhotonServer.exe");
      QuantumEditorLog.Log($"Launching Photon Server at: {photonServer} {arguments}");

      var startInfo = new System.Diagnostics.ProcessStartInfo() {
        FileName = "PhotonServer.exe",
        Arguments = arguments,
        WorkingDirectory = path
      };

      var p = new System.Diagnostics.Process { StartInfo = startInfo };

      p.Start();
    }


    #region Menu

    [MenuItem("Tools/Quantum/Export/Generate Dotnet Quantum.Simulation Project", true, (int)QuantumEditorMenuPriority.Export + 21)]
    public static bool GenerateProjectCheck() => TryGetGlobal(out var settings);

    [MenuItem("Tools/Quantum/Export/Generate Dotnet Quantum.Simulation Project", false, (int)QuantumEditorMenuPriority.Export + 21)]
    public static void GenerateProject() {
      if (TryGetGlobal(out var settings)) {
        GenerateProject(settings);
      }
    }

    [MenuItem("Tools/Quantum/Export/Build Dotnet Quantum.Simulation Dll", true, (int)QuantumEditorMenuPriority.Export + 22)]
    public static bool BuildProjectCheck() => TryGetGlobal(out var settings);

    [MenuItem("Tools/Quantum/Export/Build Dotnet Quantum.Simulation Dll", false, (int)QuantumEditorMenuPriority.Export + 22)]
    public static void BuildProject() {
      if (TryGetGlobal(out var settings)) {
        GenerateProject(settings);
        BuildProject(settings);
      }
    }

    [MenuItem("Tools/Quantum/Export/Synchronize Quantum Plugin SDK", true, (int)QuantumEditorMenuPriority.Export + 41)]
    public static bool SynchronizePluginSdkCheck() => TryGetGlobal(out var settings) && settings.HasCustomPluginSdk;

    [MenuItem("Tools/Quantum/Export/Synchronize Quantum Plugin SDK", false, (int)QuantumEditorMenuPriority.Export + 41)]
    public static void SynchronizePluginSdk() {
      if (TryGetGlobal(out var settings)) {
        SynchronizePluginSdk(settings);
      }
    }

    [MenuItem("Tools/Quantum/Export/Export Quantum Plugin Data", true, (int)QuantumEditorMenuPriority.Export + 42)]
    public static bool ExportPluginSdkDataCheck() => TryGetGlobal(out var settings) && settings.HasCustomPluginSdk;

    [MenuItem("Tools/Quantum/Export/Export Quantum Plugin Data", false, (int)QuantumEditorMenuPriority.Export + 42)]
    public static void ExportPluginSdkData() {
      if (TryGetGlobal(out var settings)) {
        ExportPluginSdkData(settings);
      }
    }

    #endregion
  }
}