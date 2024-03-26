namespace Quantum {
  using System;
  using System.Diagnostics;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using UnityEngine.UI;
  using static QuantumUnityExtensions;

  public class QuantumStats : QuantumMonoBehaviour {
    public Text FrameVerified;
    public Text FramePredicted;
    public Text Predicted;
    public Text Resimulated;
    public Text SimulateTime;
    public Text SimulationState;
    public Text NetworkPing;
    public Text NetworkIn;
    public Text NetworkOut;
    public Text InputOffset;

    public Text         ToggleButtonText;
    public GameObject[] Toggles;
    public Boolean      StartEnabled = true;
    public Boolean UseCurrentBandwidth = true;

    Stopwatch _networkTimer;

    void Start() {
      // create event system if none exists in the scene
      var eventSystem = FindFirstObjectByType<EventSystem>();
      if (eventSystem == null) {
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
      }

      SetState(StartEnabled);
    }

    private float timer = 0;
    void Update() {
      if (QuantumRunner.Default && Toggles[0].activeSelf) {
        if (QuantumRunner.Default.IsRunning) {
          var gameInstance = QuantumRunner.Default.Game;

          if (gameInstance.Session.FramePredicted != null) {
            FrameVerified.text  = gameInstance.Session.FrameVerified.Number.ToString();
            FramePredicted.text = gameInstance.Session.FramePredicted.Number.ToString();
          }

          Predicted.text    = gameInstance.Session.PredictedFrames.ToString();
          NetworkPing.text  = gameInstance.Session.Stats.Ping.ToString();
          SimulateTime.text = Math.Round(gameInstance.Session.Stats.UpdateTime * 1000, 2) + " ms";
          InputOffset.text  = gameInstance.Session.Stats.Offset.ToString();
          Resimulated.text  = gameInstance.Session.Stats.ResimulatedFrames.ToString();

          if (gameInstance.Session.IsStalling) {
            SimulationState.text  = "Stalling";
            SimulationState.color = Color.red;
          } else {
            SimulationState.text  = "Running";
            SimulationState.color = Color.green;
          }
        }

        if (QuantumRunner.Default.NetworkClient != null && QuantumRunner.Default.NetworkClient.IsConnected) {
          QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsEnabled = true;

          if (_networkTimer == null) {
            _networkTimer = Stopwatch.StartNew();
            QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsReset();
          }

          if (UseCurrentBandwidth) {
            CurrentBandwidth();
          } else {
            NetworkIn.text  = (int)(QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsIncoming.TotalPacketBytes / _networkTimer.Elapsed.TotalSeconds) + " bytes/s";
            NetworkOut.text = (int)(QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsOutgoing.TotalPacketBytes / _networkTimer.Elapsed.TotalSeconds) + " bytes/s";
          }
        }
      } else {
        _networkTimer = null;
      }
    }

    private double lastTime = 0;
    private int lastTrafficIn = 0;
    private int lastTrafficOut = 0;
    private void CurrentBandwidth() {
      timer += Time.deltaTime;
      if (timer >= 1) {
        var currentTime = _networkTimer.Elapsed.TotalSeconds;
        var currentTrafficIn = QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsIncoming.TotalPacketBytes;
        var CurrentTrafficOut = QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsOutgoing.TotalPacketBytes;
        var seconds = currentTime - lastTime;
        var trafficIn = currentTrafficIn - lastTrafficIn;
        var trafficOut = CurrentTrafficOut - lastTrafficOut;
        
        NetworkIn.text  = (int)(trafficIn / seconds) + " bytes/s";
        NetworkOut.text = (int)(trafficOut / seconds) + " bytes/s";
        
        lastTrafficIn = currentTrafficIn;
        lastTrafficOut = CurrentTrafficOut;
        lastTime = currentTime;

        timer = 0;
      }
    }

    public void ResetNetworkStats() {
      _networkTimer = null;

      if (QuantumRunner.Default != null && QuantumRunner.Default.NetworkClient != null && QuantumRunner.Default.NetworkClient.IsConnected) {
        QuantumRunner.Default.NetworkClient.RealtimePeer.TrafficStatsReset();
      }
    }

    void SetState(bool state) {
      for (int i = 0; i < Toggles.Length; ++i) {
        Toggles[i].SetActive(state);
      }

      ToggleButtonText.text = state ? "Hide Stats" : "Show Stats";
    }

    public void Toggle() {
      SetState(!Toggles[0].activeSelf);
    }

    public static void Show() {
      GetObject().SetState(true);
    }

    public static void Hide() {
      GetObject().SetState(false);
    }

    public static QuantumStats GetObject() {
      QuantumStats stats;

      // find existing or create new
      if (!(stats = FindFirstObjectByType<QuantumStats>())) {
        stats = Instantiate(UnityEngine.Resources.Load<QuantumStats>(nameof(QuantumStats)));
      }

      return stats;
    }
  }
}