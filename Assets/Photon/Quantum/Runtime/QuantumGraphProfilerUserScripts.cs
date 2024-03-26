namespace Quantum.Profiling
{
  public sealed class QuantumGraphProfilerUserScripts : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      AddValue(QuantumGraphProfilers.ScriptsTimer.GetLastSeconds());
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 0.5f, frameMs * 0.75f, frameMs);
    }
  }
}
