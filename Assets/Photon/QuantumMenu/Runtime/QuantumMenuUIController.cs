namespace Quantum.Menu {
  /// <summary>
  /// Quantum menu controller uses <see cref="QuantumMenuConnectArgs"/>.
  /// </summary>
  public class QuantumMenuUIController : QuantumMenuUIController<QuantumMenuConnectArgs> {
    /// <summary>
    /// Default arguments that can be set in Unity inspector.
    /// RuntimeConfig will be altered during <see cref="QuantumMenuConnectionBehaviourSDK.ConnectAsync(IQuantumMenuConnectArgs)"/>.
    /// </summary>
    public QuantumMenuConnectArgs DefaultConnectionArgs;

    /// <summary>
    /// The factory method to create the initial connection args.
    /// </summary>
    public override QuantumMenuConnectArgs CreateConnectArgs => DefaultConnectionArgs;
  }
}
