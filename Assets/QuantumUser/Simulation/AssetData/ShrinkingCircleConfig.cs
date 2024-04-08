using Photon.Deterministic;
using Quantum.Prototypes;

namespace Quantum
{
    public class ShrinkingCircleConfig : AssetObject
    {
        public FP DamageDealingPerSecond;
        public ShrinkingCircleStatePrototype[] States;
    }
}