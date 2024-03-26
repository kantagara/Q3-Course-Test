namespace Quantum.Profiling
{
  using global::Unity.Profiling;

  public sealed class QuantumGraphProfilerGC : QuantumGraphProfilerValueSeries
  {
    private ProfilerRecorder _gcCollectRecorder;
    
    protected override void OnActivated()
    {
      base.OnActivated();

      _gcCollectRecorder = ProfilerRecorder.StartNew(new ProfilerCategory("GC"), "GC.Collect");
    }

    protected override void OnDeactivated()
    {
      _gcCollectRecorder.Dispose();

      base.OnDeactivated();
    }

    protected override void OnUpdate()
    {
      AddValue(_gcCollectRecorder.Valid == true ? 0.000001f * _gcCollectRecorder.LastValue : 0.0f);
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 0.25f, frameMs * 0.375f, frameMs * 0.5f);
    }
  }
}
