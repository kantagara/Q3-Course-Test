namespace Quantum.Profiling
{
  public sealed class QuantumGraphProfilerFrameTime : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      AddValue(QuantumGraphProfilers.FrameTimer.GetLastSeconds());
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 0.75f, frameMs, frameMs * 1.5f);
    }
  }
}
