namespace Quantum.Profiling
{
  public sealed class QuantumGraphProfilerPredictedFrames : QuantumGraphProfilerValueSeries
  {
    protected override void OnUpdate()
    {
      int predictedFrames = 0;

      QuantumRunner quantumRunner = QuantumRunner.Default;
      if (quantumRunner != null && quantumRunner.Game != null && quantumRunner.Game.Session != null)
      {
        predictedFrames = quantumRunner.Game.Session.PredictedFrames;
      }

      AddValue(predictedFrames);
    }
  }
}
