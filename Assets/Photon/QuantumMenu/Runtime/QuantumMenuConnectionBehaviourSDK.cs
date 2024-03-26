namespace Quantum.Menu {
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// A wrapper for the connection object. Derive this class to add more functionality.
  /// </summary>
  public class QuantumMenuConnectionBehaviourSDK : QuantumMenuConnectionBehaviour {
    /// <summary>
    /// The Quantum UIController will be added to the connection object.
    /// </summary>
    public QuantumMenuUIController UIController;

    /// <summary>
    /// Create IQuantumMenuConnection factory.
    /// </summary>
    /// <returns>Quantum menu connection implementation.</returns>
    public override IQuantumMenuConnection Create() {
      return new QuantumMenuConnection { UIController = UIController };
    }

    /// <summary>
    /// Overrides the connect method to add some last-minute Quantum arguments.
    /// </summary>
    /// <param name="connectionArgs">Connection args.</param>
    /// <returns>Promise of the connection result</returns>
    public override Task<ConnectResult> ConnectAsync(IQuantumMenuConnectArgs connectionArgs) {
      var quantumConnectionArgs = (QuantumMenuConnectArgs)connectionArgs;

      // set global configs for ServerSettings and SessionConfig when null
      quantumConnectionArgs.ServerSettings = quantumConnectionArgs.ServerSettings ?? PhotonServerSettings.Global;
      quantumConnectionArgs.SessionConfig = quantumConnectionArgs.SessionConfig ?? QuantumDeterministicSessionConfigAsset.Global;

      // limit player count
      quantumConnectionArgs.MaxPlayerCount = Math.Min(quantumConnectionArgs.MaxPlayerCount, Input.MaxCount);

      // runtime config alterations
      {
        quantumConnectionArgs.RuntimeConfig.Map = connectionArgs.Scene.Map;

        if (connectionArgs.Scene.SystemsConfig != null) {
          quantumConnectionArgs.RuntimeConfig.SystemsConfig = connectionArgs.Scene.SystemsConfig;
        }

        if (quantumConnectionArgs.RuntimeConfig.Seed == 0) {
          quantumConnectionArgs.RuntimeConfig.Seed = Guid.NewGuid().GetHashCode();
        }

        // if SimulationConfig not set, try to get from global default configs
        if (quantumConnectionArgs.RuntimeConfig.SimulationConfig.Id.IsValid == false && QuantumDefaultConfigs.TryGetGlobal(out var defaultConfigs)) {
          quantumConnectionArgs.RuntimeConfig.SimulationConfig = defaultConfigs.SimulationConfig;
        }
      }

      // runtime player alterations
      {
        if (quantumConnectionArgs.RuntimePlayers != null 
          && quantumConnectionArgs.RuntimePlayers.Length > 0 
          && string.IsNullOrEmpty(quantumConnectionArgs.RuntimePlayers[0].PlayerNickname)) {
          // Overwrite nickname if none is set, yet.
          quantumConnectionArgs.RuntimePlayers[0].PlayerNickname = connectionArgs.Username;
        }
      }

      // auth values
      if (quantumConnectionArgs.AuthValues == null || string.IsNullOrEmpty(quantumConnectionArgs.AuthValues.UserId)) {
        // Set the user id to the username if no authentication values are presented
        quantumConnectionArgs.AuthValues ??= new Photon.Realtime.AuthenticationValues();
        quantumConnectionArgs.AuthValues.UserId = $"{quantumConnectionArgs.Username}({new System.Random().Next(99999999):00000000}";
      }

      return base.ConnectAsync(connectionArgs);
    }
  }
}
