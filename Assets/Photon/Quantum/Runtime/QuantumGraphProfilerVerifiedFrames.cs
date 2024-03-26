namespace Quantum.Profiling
{
  public sealed class QuantumGraphProfilerVerifiedFrames : QuantumGraphProfilerValueSeries
  {
    private int _lastVerifiedFrameNumber;

    protected override void OnUpdate()
    {
      int verifiedFramesSimulated = 0;

      QuantumRunner quantumRunner = QuantumRunner.Default;
      if (quantumRunner != null && quantumRunner.Game != null)
      {
        Frame verifiedFrame = quantumRunner.Game.Frames.Verified;
        if (verifiedFrame != null)
        {
          verifiedFramesSimulated = verifiedFrame.Number - _lastVerifiedFrameNumber;
          _lastVerifiedFrameNumber = verifiedFrame.Number;
        }
      }

      AddValue(verifiedFramesSimulated);
    }
  }
}
