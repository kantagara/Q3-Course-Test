namespace Quantum.Profiling
{
  using Photon.Client;

  public sealed class QuantumGraphProfilerPing : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      int ping = 0;

      PhotonPeer peer = QuantumGraphProfilersUtility.GetNetworkPeer();
      if (peer != null)
      {
        ping = peer.RoundTripTime;
        if (ping > 9999)
        {
          ping = default;
        }
      }

      AddValue(ping);
    }
  }
}
