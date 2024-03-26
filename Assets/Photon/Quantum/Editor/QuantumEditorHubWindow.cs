namespace Quantum.Editor {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Diagnostics;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using UnityEngine;
  using UnityEditor;
  using Photon.Deterministic;
  using Debug = UnityEngine.Debug;
  using EditorUtility = UnityEditor.EditorUtility;
  using System.Linq;

  class QuantumEditorHubWindowAssetPostprocessor : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
      // Unity handling for post asset processing callback. Checks existence of settings assets every time assets change.
      QuantumEditorHubWindow.EnsureUserFilesExists();
    }
  }

  [InitializeOnLoad]
  public partial class QuantumEditorHubWindow : EditorWindow {
    const int NavWidth = 256 + 2;
    const int IconSize = 32;
    const string UrlDoc = "https://doc.photonengine.com/quantum/v3";
    const string UrlSDK = UrlDoc + "/getting-started/initial-setup";
    const string UrlPublicDiscord = "https://dashboard.photonengine.com/discord/joinphotonengine";
    const string UrlDashboard = "https://dashboard.photonengine.com/";
    const string Url100Tutorial = UrlDoc + "/quantum-100/overview";
    const string UrlDocApi = "https://doc-api.photonengine.com/en/quantum/v3/index.html";
    const string WindowTitle = "Photon Quantum Hub";
    const string TexSupport = "Contact the Photon Team by using the Discord link below and browse the online documentation.";
    const string TextWelcome =
@"Once you have installed the Quantum user files and set up your Quantum App Id, explore the sections on the left to get started.";
    const string TextInstallationInstructions =
@"<b>Quantum requires an installation step to create local user scripts and asset files that won't be overwritten by upgrades:</b>"; 
    const string TextAppIdInstructions =
@"<b>A Quantum App Id is required to start online connections:</b>

  - Open the Photon Dashboard (Log-in as required).
  - Select an existing Quantum 3 App Id, 
     create a new one or migrate an existing Quantum 2 AppId.
  - Copy the App Id and paste into the field below (or into the PhotonServerSettings.asset).";
    const string TitleVersionReformat = "<size=22><color=white>{0}</color></size>";
    const string SectionReformat = "<i><color=lightblue>{0}</color></i>";
    const string Header1Reformat = "<size=22><color=white>{0}</color></size>";
    const string Header2Reformat = "<size=18><color=white>{0}</color></size>";
    const string Header3Reformat = "<b><color=#ffffaaff>{0}</color></b>";
    const string ClassReformat = "<color=#FFDDBB>{0}</color>";
    static Vector2 StatusIconWidth = new Vector2(24, 24);

#if QUANTUM_UPM
    static string BuildInfoFilepath => $"Packages/com.photonengine.quantum/build_info.txt";
    static string ReleaseHistoryFilepath => $"Packages/com.photonengine.quantum/CHANGELOG.md";
    static string ReleaseHistoryRealtimeFilepath => $"Packages/com.photonengine.realtime/CHANGELOG.md";
#else
    static string BuildInfoFilepath => BuildPath(Application.dataPath, "Photon", "Quantum", "build_info.txt");
    static string ReleaseHistoryFilepath => BuildPath(Application.dataPath, "Photon", "Quantum", "CHANGELOG.md");
    static string ReleaseHistoryRealtimeFilepath => BuildPath(Application.dataPath, "Photon", "PhotonRealtime", "Code", "changes-realtime.txt");
    static string QuantumMenuUnitypackagePath => BuildPath(Application.dataPath, "Photon", "QuantumMenu", "Quantum-Menu.unitypackage");
    static string QuantumMenuScenePath => BuildPath(Application.dataPath, "Photon", "QuantumMenu", "QuantumSampleMenu.unity");
#endif
    static string _releaseHistoryHeader; // TODO: add realtime release notes
    static List<string> _releaseHistoryTextAdded;
    static List<string> _releaseHistoryTextChanged;
    static List<string> _releaseHistoryTextFixed;
    static List<string> _releaseHistoryTextRemoved;
    static List<string> _releaseHistoryTextInternal;
    static string _releaseHistory;
    static GUIStyle _navbarHeaderGraphicStyle;
    static GUIStyle _textLabelStyle;
    static GUIStyle _headerLabelStyle;
    static GUIStyle _releaseNotesStyle;
    static GUIStyle _headerTextStyle;
    static GUIStyle _buttonActiveStyle;
    static bool? _ready; // true after InitContent(), reset onDestroy, onEnable, etc.
    static bool _showInstallationInWelcome;
    static bool _showAppIdInWelcome;
    static Vector2 _windowSize;
    static Vector2 _windowPosition = new Vector2(100, 100);

    public GUISkin QuantumHubSkin;
    public Texture2D SetupIcon;
    public Texture2D DocumentationIcon;
    public Texture2D SamplesIcon;
    public Texture2D CommunityIcon;
    public Texture2D ProductLogo;
    public Texture2D PhotonCloudIcon;
    public Texture2D QuantumIcon;
    public Texture2D CorrectIcon;

    Section[] _sections;
    int _currentSection;
    double _nextForceRepaint;
    Vector2 _scrollRect;
    List<Texture2D> _buildInIcons;
    double _welcomeSceenConditionsTimestamp;

    enum Icon {
      BuildIn_ScriptableObjectIcon,
      BuildIn_TextAssetIcon,
      BuildIn_RefreshIcon,
      Setup,
      Documentation,
      Samples,
      Community,
      ProductLogo,
      PhotonCloud,
      QuantumIcon,
    }

    static bool IsAppIdValid {
      get {
        try {
          var photonSettings = PhotonServerSettings.Global;
          var val = photonSettings.AppSettings.AppIdQuantum;
          return IsValidGuid(val);
        } catch {
          return false;
        }
      }
    }

    static bool AreImportatUserFilesInstalled {
      get {
        return PhotonServerSettings.TryGetGlobal(out _) &&
          QuantumDeterministicSessionConfigAsset.TryGetGlobal(out _) &&
          QuantumEditorSettings.TryGetGlobal(out _);
      }
    }

    [MenuItem("Window/Quantum/Quantum Hub")]
    [MenuItem("Tools/Quantum/Quantum Hub %H", false, (int)QuantumEditorMenuPriority.TOP)]
    [MenuItem("Tools/Quantum/Window/Quantum Hub", false, (int)QuantumEditorMenuPriority.Window + 0)]
    public static void Open() {
      if (Application.isPlaying) {
        return;
      }

      var window = GetWindow<QuantumEditorHubWindow>(true, WindowTitle, true);
      window.position = new Rect(_windowPosition, _windowSize);
      _showInstallationInWelcome = !AreImportatUserFilesInstalled;
      _showAppIdInWelcome = !IsAppIdValid;
      window.Show();
    }

    static void ReOpen() {
      if (_ready.HasValue && _ready.Value == false) {
        Open();
      }

      EditorApplication.update -= ReOpen;
    }

    private void Awake() {
      _buildInIcons = new List<Texture2D> {
        (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image,
        (Texture2D)EditorGUIUtility.IconContent("TextAsset Icon").image,
        (Texture2D)EditorGUIUtility.IconContent("Refresh@2x").image,
      };
    }

    void OnEnable() {
      _ready = false;
      _windowSize = new Vector2(850, 600);
      minSize = _windowSize;

      // Pre-load Release History
      PrepareReleaseHistoryText();
      wantsMouseMove = true;
    }

    void OnDestroy() {
      _ready = false;
    }

    void OnGUI() {

      GUI.skin = QuantumHubSkin;

      try {
        RefreshWelcomeSceenConditions();

        InitContent();

        _windowPosition = this.position.position;

        // full window wrapper
        using (new EditorGUILayout.HorizontalScope(GUI.skin.window)) {

          // Left Nav menu
          using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(NavWidth), GUILayout.MinWidth(NavWidth))) {
            DrawHeader();
            DrawLeftNavMenu();
          }

          // Right Main Content
          using (new EditorGUILayout.VerticalScope()) {
            DrawContent();
          }
        }

        DrawFooter();

      } catch (ExitGUIException) { 
        // hide gui exception
      }
      catch (Exception e) {
        QuantumEditorLog.Error($"Exception when drawing the Hub Window: {e.Message}");
      }

      // Force repaints while mouse is over the window, to keep Hover graphics working (Unity quirk)
      var timeSinceStartup = Time.realtimeSinceStartupAsDouble;
      if (Event.current.type == EventType.MouseMove && timeSinceStartup > _nextForceRepaint) {
        // Cap the repaint rate a bit since we are forcing repaint on mouse move
        _nextForceRepaint = timeSinceStartup + .05f;
        Repaint();
      }
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnDidReloadScripts() {
      EnsureUserFilesExists();
    }

    public static void EnsureUserFilesExists() {
      // Check for important user files
      if (AreImportatUserFilesInstalled) { 
        return;
      }

      if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
        EditorApplication.delayCall += EnsureUserFilesExists;
        return;
      }

      EditorApplication.delayCall += Open;
    }

    void DrawContent() {
      {
        var section = _sections[_currentSection];
        GUILayout.Label(section.Description, _headerTextStyle);

        using (new EditorGUILayout.VerticalScope(QuantumHubSkin.box)) {
          _scrollRect = EditorGUILayout.BeginScrollView(_scrollRect);
          section.DrawMethod.Invoke();
          EditorGUILayout.EndScrollView();
        }
      }
    }

    void DrawWelcomeSection() {

      // Top Welcome content box
      GUILayout.Label(TextWelcome);
      GUILayout.Space(16);

      if (_showInstallationInWelcome) {
        DrawInstallationBox();
      }

      if (_showAppIdInWelcome) {
        DrawSetupAppIdBox();
      }
    }

    void DrawSetupSection() {
      DrawInstallationBox();
      DrawSetupAppIdBox();

      using (new EditorGUILayout.VerticalScope(QuantumHubSkin.GetStyle("SteelBox"))) {
        DrawButtonAction(Icon.BuildIn_RefreshIcon, "Clear Quantum PlayerPrefs", "Delete all PlayerPrefs created by Quantum.",
          callback: () => { 
            ClearQuantumPlayerPrefs();
            ClearQuantumMenuPlayerPrefs();
          });
      }
    }

    void ClearQuantumPlayerPrefs() {
      PlayerPrefs.DeleteKey(PhotonServerSettings.Global.BestRegionSummaryKey);
      PlayerPrefs.DeleteKey("Quantum.ReconnectInformation");
    }

    void ClearQuantumMenuPlayerPrefs() {
      PlayerPrefs.DeleteKey("Photon.Menu.Username");
      PlayerPrefs.DeleteKey("Photon.Menu.Region");
      PlayerPrefs.DeleteKey("Photon.Menu.AppVersion");
      PlayerPrefs.DeleteKey("Photon.Menu.MaxPlayerCount");
      PlayerPrefs.DeleteKey("Photon.Menu.Scene");
      PlayerPrefs.DeleteKey("Photon.Menu.Framerate");
      PlayerPrefs.DeleteKey("Photon.Menu.Fullscreen");
      PlayerPrefs.DeleteKey("Photon.Menu.Resolution");
      PlayerPrefs.DeleteKey("Photon.Menu.VSync");
      PlayerPrefs.DeleteKey("Photon.Menu.QualityLevel");
    }

    void DrawSamplesSection() {
      GUILayout.Label("Install Samples", _headerLabelStyle);

      DrawButtonAction(Icon.Samples, "Install Quantum Menu", "Fully-fledged prototyping online game menu",
        callback: () => {
          AssetDatabase.ImportPackage(QuantumMenuUnitypackagePath, false);
          QuantumEditorMenuCreateScene.AddScenePathToBuildSettings(QuantumMenuScenePath, true);
          ClearQuantumMenuPlayerPrefs();
        });

      DrawButtonAction(Icon.Samples, "Install Simple Connection Sample Scene", "Creates a scene that showcases the Quantum online connection sequence.",
        callback: () => {
          QuantumEditorMenuCreateScene.CreateSimpleConnectionScene($"{QuantumEditorUserScriptGeneration.FolderPath}/Scenes/QuantumSimpleConnectionScene.unity");
          GUIUtility.ExitGUI();
        });

      GUILayout.Label("Tutorials", _headerLabelStyle);

      DrawButtonAction(Icon.Documentation, "Quantum 100 Tutorial", "Quantum Fundamentals Tutorial", callback: OpenURL(Url100Tutorial));
    }
    
    void DrawRealtimeReleaseSection() {
      GUILayout.BeginVertical();
      {
        GUILayout.Label(string.Format(TitleVersionReformat, _releaseHistoryHeader));
        GUILayout.Space(5);
        DrawReleaseHistoryItem("Added:", _releaseHistoryTextAdded);
        DrawReleaseHistoryItem("Changed:", _releaseHistoryTextChanged);
        DrawReleaseHistoryItem("Fixed:", _releaseHistoryTextFixed);
        DrawReleaseHistoryItem("Removed:", _releaseHistoryTextRemoved);
        DrawReleaseHistoryItem("Internal:", _releaseHistoryTextInternal);
      }
      GUILayout.EndVertical();
    }

    void DrawReleaseSection() {
      GUILayout.Label(_releaseHistory, _releaseNotesStyle);
    }

    void DrawReleaseHistoryItem(string label, List<string> items) {
      if (items != null && items.Count > 0) {
        GUILayout.BeginVertical();
        {
          GUILayout.Label(string.Format(SectionReformat, label));

          GUILayout.Space(5);

          foreach (string text in items) {
            GUILayout.Label(string.Format("- {0}.", text), _textLabelStyle);
          }
        }
        GUILayout.EndVertical();
      }
    }

    void DrawSupportSection() {

      GUILayout.BeginVertical();
      GUILayout.Space(5);
      GUILayout.Label(TexSupport, _textLabelStyle);
      GUILayout.EndVertical();

      GUILayout.Space(15);

      DrawButtonAction(Icon.Community, "Community", "Join the Photon Discord Server.", callback: OpenURL(UrlPublicDiscord));
      DrawButtonAction(Icon.Documentation, "Online Documentation", "Open the online documentation.", callback: OpenURL(UrlDoc));
      DrawButtonAction(Icon.Documentation, "SDK and Release Notes", "Latest SDK downloads.", callback: OpenURL(UrlSDK));
      DrawButtonAction(Icon.Documentation, "API Reference", "The API library reference documentation.", callback: OpenURL(UrlDocApi));
    }

    void DrawVersionSection() {
      var textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(BuildInfoFilepath, typeof(TextAsset));
      var text = textAsset.text;

      GUILayout.BeginVertical();
      GUILayout.Space(5);
      text = Regex.Replace(text, @"(build):", string.Format(ClassReformat, "$1"));
      text = Regex.Replace(text, @"(date):", string.Format(ClassReformat, "$1"));
      text = Regex.Replace(text, @"(git):", string.Format(ClassReformat, "$1"));
      GUILayout.Label(text, _textLabelStyle);
      GUILayout.EndVertical();

      DrawButtonAction(Icon.BuildIn_TextAssetIcon, "Build Info", "Build Information File",
        callback: () => { EditorGUIUtility.PingObject(textAsset); Selection.activeObject = textAsset; });

      try {
        var codeBase = Assembly.GetAssembly(typeof(FP)).CodeBase;
        var path = Uri.UnescapeDataString(new UriBuilder(codeBase).Path);
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
        GUILayout.Label($"<color=#FFDDBB>{Path.GetFileName(codeBase)}</color>: {fileVersionInfo.ProductVersion}", _textLabelStyle);
      } catch { }

      try {
        string codeBase = Assembly.GetAssembly(typeof(Quantum.Map)).CodeBase;
        string path = Uri.UnescapeDataString(new UriBuilder(codeBase).Path);
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
        GUILayout.Label($"<color=#FFDDBB>{Path.GetFileName(codeBase)}</color>: {fileVersionInfo.ProductVersion}", _textLabelStyle);
      } catch { }
    }

    void DrawGlobalObjectStatus<T>() where T : QuantumGlobalScriptableObject<T> {

      var attribute = typeof(T).GetCustomAttribute<QuantumGlobalScriptableObjectAttribute>();
      Debug.Assert(attribute != null);
      Debug.Assert(attribute.DefaultPath.StartsWith("Assets/"));
      
      var nicePath = PathUtils.GetPathWithoutExtension(attribute.DefaultPath.Substring("Assets/".Length));

      using (new EditorGUILayout.HorizontalScope()) {
        bool hasDefaultInstance = QuantumGlobalScriptableObject<T>.TryGetGlobal(out var defaultInstance);
        using (new EditorGUI.DisabledScope(!hasDefaultInstance)) {
          if (GUILayout.Button(nicePath, QuantumHubSkin.label)) {
            EditorGUIUtility.PingObject(defaultInstance);
          }
        }

        GUILayout.Label(GetStatusIcon(hasDefaultInstance), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
      }
    }
 
    void DrawInstallationBox() {
      using (new EditorGUILayout.VerticalScope(QuantumHubSkin.GetStyle("SteelBox"))) {
        GUILayout.Label(TextInstallationInstructions);

        DrawButtonAction(Icon.QuantumIcon, "Install", "Install Quantum user files.",
          callback: () => {
            InstallAllUserFiles();
          });

        DrawGlobalObjectStatus<PhotonServerSettings>();
        DrawGlobalObjectStatus<QuantumDeterministicSessionConfigAsset>();
        DrawGlobalObjectStatus<QuantumUnityDB>();
        DrawGlobalObjectStatus<QuantumEditorSettings>();
        DrawGlobalObjectStatus<QuantumGameGizmosSettingsScriptableObject>();
        DrawGlobalObjectStatus<QuantumDefaultConfigs>();
        DrawGlobalObjectStatus<QuantumDotnetBuildSettings>();
        DrawGlobalObjectStatus<QuantumDotnetProjectSettings>();

        using (new EditorGUILayout.HorizontalScope()) {
          if (GUILayout.Button(QuantumEditorUserScriptGeneration.FolderPath.Replace("Assets/", "") + " User Workspace", QuantumHubSkin.label)) {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(QuantumEditorUserScriptGeneration.PingWorkspaceFile, typeof(UnityEngine.Object)));
          }
          GUILayout.Label(GetStatusIcon(QuantumEditorUserScriptGeneration.WorkspaceFilesExist), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          if (GUILayout.Button(QuantumEditorUserScriptGeneration.FolderPath.Replace("Assets/", "") + " Partial Classes (*.cs.User)", QuantumHubSkin.label)) {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(QuantumEditorUserScriptGeneration.PingUserFile, typeof(UnityEngine.Object)));
          }
          GUILayout.Label(GetStatusIcon(QuantumEditorUserScriptGeneration.UserFilesExist), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("QuantumUser Scenes");
          var foundAnySceneInUserFolder = Directory.Exists(QuantumEditorUserScriptGeneration.FolderPath) && AssetDatabase.FindAssets("t:Scene", new[] { QuantumEditorUserScriptGeneration.FolderPath }).Length > 0;
          GUILayout.Label(GetStatusIcon(foundAnySceneInUserFolder), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("Quantum Qtn CodeGen");
          GUILayout.Label(GetStatusIcon(Quantum.Input.MaxCount > 0), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("EditorSettings.ProjectGenerationUserExtensions Include Qtn Files");
          GUILayout.Label(GetStatusIcon(EditorSettings.projectGenerationUserExtensions.Contains("qtn")), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }
      }
    }

    public static void InstallAllUserFiles() {
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<PhotonServerSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumEditorSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDeterministicSessionConfigAsset>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumGameGizmosSettingsScriptableObject>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDefaultConfigs>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDotnetProjectSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDotnetBuildSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumUnityDB>();
      QuantumEditorUserScriptGeneration.GenerateWorkspaceFiles();
      QuantumEditorUserScriptGeneration.GenerateUserFiles();

      if (Quantum.Input.MaxCount == 0) {
        EditorApplication.ExecuteMenuItem("Tools/Quantum/CodeGen/Run Qtn CodeGen");
      }

      // Add qtn extension to the VS project generation
      if (EditorSettings.projectGenerationUserExtensions.Contains("qtn") == false) {
        var userExtensions = EditorSettings.projectGenerationUserExtensions;
        ArrayUtils.Add(ref userExtensions, "qtn");
        EditorSettings.projectGenerationUserExtensions = userExtensions;
      }

      if (AssetDatabase.FindAssets("t:Scene", new[] { QuantumEditorUserScriptGeneration.FolderPath }).Length == 0) {
        // Create Quantum game scene
        Directory.CreateDirectory($"{QuantumEditorUserScriptGeneration.FolderPath}/Scenes");
        QuantumEditorMenuCreateScene.CreateNewQuantumScene(
          $"{QuantumEditorUserScriptGeneration.FolderPath}/Scenes/QuantumGameScene.unity",
          $"{QuantumEditorSettings.Global.DefaultNewAssetsLocation}/QuantumMap.asset",
          true,
          true);

        if (Application.isBatchMode == false) {
          // Don't call the gui ecxception when coming from CI
          GUIUtility.ExitGUI();
        }
      }
    }

    void DrawSetupAppIdBox() {
      // Getting server settings data
      PhotonServerSettings.TryGetGlobal(out var photonServerSettings);
      var appId = photonServerSettings?.AppSettings.AppIdQuantum;

      // Setting up AppId content box.
      using (new EditorGUILayout.VerticalScope(QuantumHubSkin.GetStyle("SteelBox"))) {
        GUILayout.Label(TextAppIdInstructions);

        using (new EditorGUILayout.HorizontalScope(QuantumHubSkin.GetStyle("SteelBox"))) {
          GUILayout.Label("<b>App Id:</b>", GUILayout.Width(80));
          using (new EditorGUI.DisabledScope(photonServerSettings == null)) {
            using (new EditorGUILayout.HorizontalScope()) {
              EditorGUI.BeginChangeCheck();
              var editedAppId = EditorGUILayout.TextField("", appId, QuantumHubSkin.textField, GUILayout.Height(StatusIconWidth.y));
              if (EditorGUI.EndChangeCheck()) {
                photonServerSettings.AppSettings.AppIdQuantum = editedAppId;
                EditorUtility.SetDirty(photonServerSettings);
                AssetDatabase.SaveAssets();
              }
            }
          }
          GUILayout.Label(GetStatusIcon(IsAppIdValid), GUILayout.Width(StatusIconWidth.x), GUILayout.Height(StatusIconWidth.y));
        }

        DrawButtonAction(Icon.PhotonCloud, "Open The Photon Dashboard Url", callback: OpenURL(UrlDashboard));
        EditorGUILayout.Space(4);

        DrawButtonAction(Icon.BuildIn_ScriptableObjectIcon, "Select Photon Server Settings Asset", "Select the Photon network transport configuration asset.",
          callback: () => { EditorGUIUtility.PingObject(PhotonServerSettings.Global); Selection.activeObject = PhotonServerSettings.Global; });
      }
    }

    void DrawLeftNavMenu() {
      for (int i = 0; i < _sections.Length; ++i) {
        var section = _sections[i];
        if (DrawNavButton(section, _currentSection == i)) {
          _currentSection = i;
        }
      }
    }

    void RefreshWelcomeSceenConditions() {
      if (EditorApplication.timeSinceStartup < _welcomeSceenConditionsTimestamp + 1.5f) {
        return;
      }

      _welcomeSceenConditionsTimestamp = EditorApplication.timeSinceStartup;
      _showInstallationInWelcome = !AreImportatUserFilesInstalled;
      _showAppIdInWelcome = !IsAppIdValid;
    }

    void DrawHeader() {
      GUILayout.Label(GetIcon(Icon.ProductLogo), _navbarHeaderGraphicStyle);
    }

    void DrawFooter() {
      GUILayout.BeginHorizontal(QuantumHubSkin.window);
      {
        GUILayout.Label("\u00A9 2024, Exit Games GmbH. All rights reserved.");
      }
      GUILayout.EndHorizontal();
    }

    bool DrawNavButton(Section section, bool currentSection) {
      var content = new GUIContent() {
        text  = "  " + section.Title,
        image = GetIcon(section.Icon),
      };

      var renderStyle = currentSection ? _buttonActiveStyle : GUI.skin.button;
      return GUILayout.Button(content, renderStyle);
    }

    void DrawButtonAction(Icon icon, string header, string description = null, bool? active = null, Action callback = null, int? width = null) {
      DrawButtonAction(GetIcon(icon), header, description, active, callback, width);
    }

    static void DrawButtonAction(Texture2D icon, string header, string description = null, bool? active = null, Action callback = null, int? width = null) {
      var padding = GUI.skin.button.padding.top + GUI.skin.button.padding.bottom;
      var height = IconSize + padding;

      var renderStyle = active.HasValue && active.Value == true ? _buttonActiveStyle : GUI.skin.button;
      // Draw text separately (not part of button guiconent) to have control over the space between the icon and the text.
      var rect = EditorGUILayout.GetControlRect(false, height, width.HasValue ? GUILayout.Width(width.Value) : GUILayout.ExpandWidth(true));
      bool clicked = GUI.Button(rect, icon, renderStyle);
      GUI.Label(new Rect(rect) { xMin = rect.xMin + IconSize + 20 }, description == null ? "<b>" + header +"</b>" : string.Format("<b>{0}</b>\n{1}", header, "<color=#aaaaaa>" + description + "</color>"));
      if (clicked && callback != null) {
        callback.Invoke();
      }
    }

    class Section {
      public string Title;
      public string Description;
      public Action DrawMethod;
      public Icon Icon;

      public Section(string title, string description, Action drawMethod, Icon icon) {
        Title = title;
        Description = description;
        DrawMethod = drawMethod;
        Icon = icon;
      }
    }

    Texture2D GetIcon(Icon icon) {
      switch (icon) {
        case Icon.Setup: return SetupIcon;
        case Icon.Documentation: return DocumentationIcon;
        case Icon.Samples: return SamplesIcon;
        case Icon.Community: return CommunityIcon;
        case Icon.ProductLogo: return ProductLogo;
        case Icon.PhotonCloud: return PhotonCloudIcon;
        case Icon.QuantumIcon: return QuantumIcon;
        default: 
          if (icon < Icon.Setup && _buildInIcons != null && _buildInIcons.Count >= (int)icon) {
            return _buildInIcons[(int)icon];
          }
          return null;
      }
    }

    void InitContent() {
      if (_ready.HasValue && _ready.Value) {
        return;
      }

      _sections = new[] {
        new Section("Welcome", "Welcome to Photon Quantum 3", DrawWelcomeSection, Icon.Setup),
        new Section("Quantum Setup", "Install Quantum User Files And Setup Photon AppId", DrawSetupSection, Icon.PhotonCloud),
        new Section("Documentation & Support", "Support, Community Snd Documentation Links", DrawSupportSection, Icon.Community),
        new Section("Tutorials & Samples", "Tutorials and Samples", DrawSamplesSection, Icon.Samples),
        new Section("Quantum Release Notes", "Quantum Release Notes", DrawReleaseSection, Icon.Documentation),
        new Section("Realtime Release Notes", "Realtime Release Notes", DrawRealtimeReleaseSection, Icon.Documentation),
        new Section("Build Info", "Quantum Build Info", DrawVersionSection, Icon.QuantumIcon),
      };

      Color commonTextColor = Color.white;

      var _guiSkin = QuantumHubSkin;

      _navbarHeaderGraphicStyle = new GUIStyle(_guiSkin.button) { alignment = TextAnchor.MiddleCenter };

      _headerTextStyle = new GUIStyle(_guiSkin.label) {
        fontSize = 18,
        padding = new RectOffset(12, 8, 8, 8),
        fontStyle = FontStyle.Bold,
        normal = { textColor = commonTextColor }
      };

      _buttonActiveStyle = new GUIStyle(_guiSkin.button) {
        fontStyle = FontStyle.Bold,
        normal = { background = _guiSkin.button.active.background, textColor = Color.white }
      };


      _textLabelStyle = new GUIStyle(_guiSkin.label) {
        wordWrap = true,
        normal = { textColor = commonTextColor },
        richText = true,

      };
      _headerLabelStyle = new GUIStyle(_textLabelStyle) {
        fontSize = 15,
      };

      _releaseNotesStyle = new GUIStyle(_textLabelStyle) {
        richText = true,
      };

      _ready = true;
    }

    static Action OpenURL(string url, params object[] args) {
      return () => {
        if (args.Length > 0) {
          url = string.Format(url, args);
        }

        Application.OpenURL(url);
      };
    }

    void PrepareReleaseHistoryText() {
      // Converts readme files into Unity RichText.
      {
        var text = (TextAsset)AssetDatabase.LoadAssetAtPath(ReleaseHistoryFilepath, typeof(TextAsset));
        var baseText = text.text;

        // #
        baseText = Regex.Replace(baseText, @"^# (.*)", string.Format(TitleVersionReformat, "$1"));
        baseText = Regex.Replace(baseText, @"(?<=\n)# (.*)", string.Format(Header1Reformat, "$1"));
        // ##
        baseText = Regex.Replace(baseText, @"(?<=\n)## (.*)", string.Format(Header2Reformat, "$1"));
        // ###
        baseText = Regex.Replace(baseText, @"(?<=\n)### (.*)", string.Format(Header3Reformat, "$1"));
        // **Changes**
        baseText = Regex.Replace(baseText, @"(?<=\n)\*\*(.*)\*\*", string.Format(SectionReformat, "$1"));
        // `Class`
        baseText = Regex.Replace(baseText, @"\`([^\`]*)\`", string.Format(ClassReformat, "$1"));

        _releaseHistory = baseText;
      }

      // Realtime
      {
        try {
          var text = (TextAsset)AssetDatabase.LoadAssetAtPath(ReleaseHistoryRealtimeFilepath, typeof(TextAsset));

          var baseText = text.text;

          var regexVersion = new Regex(@"Version (\d+\.?)*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexAdded = new Regex(@"\b(Added:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexChanged = new Regex(@"\b(Changed:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexUpdated = new Regex(@"\b(Updated:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexFixed = new Regex(@"\b(Fixed:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexRemoved = new Regex(@"\b(Removed:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);
          var regexInternal = new Regex(@"\b(Internal:)(.*)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

          var matches = regexVersion.Matches(baseText);

          if (matches.Count > 0) {
            var currentVersionMatch = matches[0];
            var lastVersionMatch = currentVersionMatch.NextMatch();

            if (currentVersionMatch.Success && lastVersionMatch.Success) {
              Func<MatchCollection, List<string>> itemProcessor = (match) => {
                List<string> resultList = new List<string>();
                for (int index = 0; index < match.Count; index++) {
                  resultList.Add(match[index].Groups[2].Value.Trim());
                }
                return resultList;
              };

              string mainText = baseText.Substring(currentVersionMatch.Index + currentVersionMatch.Length,
                  lastVersionMatch.Index - lastVersionMatch.Length - 1).Trim();

              _releaseHistoryHeader = currentVersionMatch.Value.Trim();
              _releaseHistoryTextAdded = itemProcessor(regexAdded.Matches(mainText));
              _releaseHistoryTextChanged = itemProcessor(regexChanged.Matches(mainText));
              _releaseHistoryTextChanged.AddRange(itemProcessor(regexUpdated.Matches(mainText)));
              _releaseHistoryTextFixed = itemProcessor(regexFixed.Matches(mainText));
              _releaseHistoryTextRemoved = itemProcessor(regexRemoved.Matches(mainText));
              _releaseHistoryTextInternal = itemProcessor(regexInternal.Matches(mainText));
            }
          }
        } catch (Exception) {
          _releaseHistoryHeader = null;
          _releaseHistoryTextAdded = new List<string>();
          _releaseHistoryTextChanged = new List<string>();
          _releaseHistoryTextFixed = new List<string>();
          _releaseHistoryTextRemoved = new List<string>();
          _releaseHistoryTextInternal = new List<string>();
        }
      }
    }

    static bool Toggle(bool value) {
      GUIStyle toggle = new GUIStyle("Toggle") {
        margin = new RectOffset(),
        padding = new RectOffset()
      };

      return EditorGUILayout.Toggle(value, toggle, GUILayout.Width(15));
    }

    static string BuildPath(params string[] parts) {
      var basePath = "";

      foreach (var path in parts) {
        basePath = Path.Combine(basePath, path);
      }

      return PathUtils.Normalize(basePath.Replace(Application.dataPath, Path.GetFileName(Application.dataPath)));
    }

    Texture2D GetStatusIcon(bool isValid) {
      return isValid ? CorrectIcon : EditorGUIUtility.FindTexture("console.erroricon.sml");
    }

    static bool IsValidGuid(string appdId) {
      try {
        return new Guid(appdId) != null;
      } catch {
        return false;
      }
    }
  }
}
