namespace Quantum.Menu {
  using System.Threading.Tasks;
#if QUANTUM_ENABLE_TEXTMESHPRO
  using Text = TMPro.TMP_Text;
  using InputField = TMPro.TMP_InputField;
#else 
  using Text = UnityEngine.UI.Text;
  using InputField = UnityEngine.UI.InputField;
#endif
  using UnityEngine;

  /// <summary>
  /// The main menu.
  /// </summary>
  public partial class QuantumMenuUIMain : QuantumMenuUIScreen {
    /// <summary>
    /// The username label.
    /// </summary>
    [InlineHelp, SerializeField] protected Text _usernameLabel;
    /// <summary>
    /// The scene thumbnail. Can be null.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Image _sceneThumbnail;
    /// <summary>
    /// The username input UI part.
    /// </summary>
    [InlineHelp, SerializeField] protected GameObject _usernameView;
    /// <summary>
    /// The actual username input field.
    /// </summary>
    [InlineHelp, SerializeField] protected InputField _usernameInput;
    /// <summary>
    /// The username confirmation button (background).
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _usernameConfirmButton;
    /// <summary>
    /// The username change button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _usernameButton;
    /// <summary>
    /// The open character selection button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _characterButton;
    /// <summary>
    /// The open party screen button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _partyButton;
    /// <summary>
    /// The quikc play button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _playButton;
    /// <summary>
    /// The quit button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _quitButton;
    /// <summary>
    /// The open scene screen button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _sceneButton;
    /// <summary>
    /// The open setting button.
    /// </summary>
    [InlineHelp, SerializeField] protected UnityEngine.UI.Button _settingsButton;

    partial void AwakeUser();
    partial void InitUser();
    partial void ShowUser();
    partial void HideUser();

    /// <summary>
    /// The Unity awake method. Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
    /// Applies the current selected graphics settings (loaded from PlayerPrefs)
    /// </summary>
    public override void Awake() {
      base.Awake();

      new QuantumMenuGraphicsSettings().Apply();

#if UNITY_STANDALONE
      _quitButton.gameObject.SetActive(true);
#else 
      _quitButton.gameObject.SetActive(false);
#endif

      AwakeUser();
    }

    /// <summary>
    /// The screen init method. Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
    /// Initialized the default arguments.
    /// </summary>
    public override void Init() {
      base.Init();

      ConnectionArgs.SetDefaults(Config);

      InitUser();
    }

    /// <summary>
    /// The screen show method. Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Show() {
      base.Show();

      _usernameView.SetActive(false);
      _usernameLabel.text = ConnectionArgs.Username;

      if (Config.AvailableScenes.Count > 0) {
        _sceneButton.gameObject.SetActive(true);
      } else {
        _sceneButton.gameObject.SetActive(false);
      }

      if (string.IsNullOrEmpty(ConnectionArgs.Scene.Name)) {
        _playButton.interactable = false;
        _partyButton.interactable = false;
        Debug.LogWarning("No valid scene to start found. Configure the menu config.");
      }

      if (_sceneButton.gameObject.activeInHierarchy && _sceneThumbnail != null) {
        if (ConnectionArgs.Scene.Preview != null) {
          _sceneThumbnail.transform.parent.gameObject.SetActive(true);
          _sceneThumbnail.sprite = ConnectionArgs.Scene.Preview;
          _sceneThumbnail.gameObject.SendMessage("OnResolutionChanged", SendMessageOptions.DontRequireReceiver);
        } else {
          _sceneThumbnail.transform.parent.gameObject.SetActive(false);
          _sceneThumbnail.sprite = null;
        }
      }


      ShowUser();
    }

    /// <summary>
    /// The screen hide method. Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
    /// </summary>
    public override void Hide() {
      base.Hide();
      HideUser();
    }

    /// <summary>
    /// Is called when the sceen background is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnFinishUsernameEdit() {
      OnFinishUsernameEdit(_usernameInput.text);
    }

    /// <summary>
    /// Is called when the <see cref="_usernameInput"/> has finished editing using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnFinishUsernameEdit(string username) {
      _usernameView.SetActive(false);

      if (string.IsNullOrEmpty(username) == false) {
        _usernameLabel.text = username;
        ConnectionArgs.Username = username;
      }
    }

    /// <summary>
    /// Is called when the <see cref="_usernameButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnUsernameButtonPressed() {
      _usernameView.SetActive(true);
      _usernameInput.text = _usernameLabel.text;
    }

    /// <summary>
    /// Is called when the <see cref="_playButton"/> is pressed using SendMessage() from the UI object.
    /// Intitiates the connection and expects the connection object to set further screen states.
    /// </summary>
    protected virtual async void OnPlayButtonPressed() {
      ConnectionArgs.Session = null;
      ConnectionArgs.Creating = false;
      ConnectionArgs.Region = ConnectionArgs.PreferredRegion;

      Controller.Show<QuantumMenuUILoading>();

      var result = await Connection.ConnectAsync(ConnectionArgs);

      await HandleConnectionResult(result, this.Controller);
    }

    /// <summary>
    /// Default connection error handling is reused in a couple places.
    /// </summary>
    /// <param name="result">Connect result</param>
    /// <param name="controller">UI Controller</param>
    /// <returns>When handling is completed</returns>
    public static async Task HandleConnectionResult(ConnectResult result, IQuantumMenuUIController controller) {
      if (result.CustomResultHandling == false) {
        if (result.Success) {
          controller.Show<QuantumMenuUIGameplay>();
        } else if (result.FailReason != ConnectFailReason.ApplicationQuit) {
          var popup = controller.PopupAsync(result.DebugMessage, "Connection Failed");
          if (result.WaitForCleanup != null) {
            await Task.WhenAll(result.WaitForCleanup, popup);
          } else {
            await popup;
          }
          controller.Show<QuantumMenuUIMain>();
        }
      }
    }

    /// <summary>
    /// Is called when the <see cref="_partyButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnPartyButtonPressed() {
      Controller.Show<QuantumMenuUIParty>();
    }

    /// <summary>
    /// Is called when the <see cref="_sceneButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnScenesButtonPressed() {
      Controller.Show<QuantumMenuUIScenes>();
    }

    /// <summary>
    /// Is called when the <see cref="_settingsButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnSettingsButtonPressed() {
      Controller.Show<QuantumMenuUISettings>();
    }

    /// <summary>
    /// Is called when the <see cref="_characterButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnCharacterButtonPressed() {
    }

    /// <summary>
    /// Is called when the <see cref="_quitButton"/> is pressed using SendMessage() from the UI object.
    /// </summary>
    protected virtual void OnQuitButtonPressed() {
      Application.Quit();
    }
  }
}
