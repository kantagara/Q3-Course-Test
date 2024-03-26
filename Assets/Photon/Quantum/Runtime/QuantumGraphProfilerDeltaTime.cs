namespace Quantum.Profiling
{
  using UnityEngine;

  public sealed class QuantumGraphProfilerDeltaTime : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      AddValue(Time.unscaledDeltaTime);
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 1.25f, frameMs * 1.5f, frameMs * 2.0f);
    }
  }
}
