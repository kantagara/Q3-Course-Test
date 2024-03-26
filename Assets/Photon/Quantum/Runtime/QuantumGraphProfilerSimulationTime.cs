namespace Quantum.Profiling
{
  public sealed class QuantumGraphProfilerSimulationTime : QuantumGraphProfilerValueSeries
  {
    private int _lastVerifiedFrameNumber;
    private int _lastPredictedFrameNumber;

    protected override void OnUpdate()
    {
      float updateTime = 0.0f;

      QuantumRunner quantumRunner = QuantumRunner.Default;
      if (quantumRunner != null && quantumRunner.Game != null)
      {
        Frame verifiedFrame = quantumRunner.Game.Frames.Verified;
        if (verifiedFrame != null && verifiedFrame.Number != _lastVerifiedFrameNumber)
        {
          updateTime = (float)quantumRunner.Game.Session.Stats.UpdateTime;
          _lastVerifiedFrameNumber = verifiedFrame.Number;
        }

        Frame predictedFrame = quantumRunner.Game.Frames.Predicted;
        if (predictedFrame != null && predictedFrame.Number != _lastPredictedFrameNumber)
        {
          updateTime = (float)quantumRunner.Game.Session.Stats.UpdateTime;
          _lastPredictedFrameNumber = predictedFrame.Number;
        }
      }

      AddValue(updateTime);
    }

    protected override void OnTargetFPSChaged(int fps)
    {
      float frameMs = 1.0f / fps;
      Graph.SetThresholds(frameMs * 0.25f, frameMs * 0.375f, frameMs * 0.5f);
    }
  }
}
