using Photon.Deterministic;
using Quantum;

public class PlayerStatsConfig : AssetObject
{
    public FP FireRateModifier = 1;
    public FP DamageModifier = 1;

    private void OnValidate()
    {
        if (DamageModifier > 19 / FP._10)
            DamageModifier = 19 / FP._10;
        else if (DamageModifier < FP._0_99)
            DamageModifier = 1;
    }
}
