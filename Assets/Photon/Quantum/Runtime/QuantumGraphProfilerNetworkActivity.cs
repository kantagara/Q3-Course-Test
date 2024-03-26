namespace Quantum.Profiling
{
  using Photon.Client;

  public sealed class QuantumGraphProfilerNetworkActivity : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      float interval = 0;

      PhotonPeer peer = QuantumGraphProfilersUtility.GetNetworkPeer();
      if (peer != null)
      {
        interval = peer.ConnectionTime - peer.LastReceiveTimestamp;
        if (interval > 9999)
        {
          interval = default;
        }
      }

      AddValue(interval * 0.001f);
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 2.0f, frameMs * 4.0f, frameMs * 8.0f);
    }
  }
}
