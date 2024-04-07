using Quantum;

namespace QuantumUser.Simulation.Systems
{
    public class DamageableSystem : SystemSignalsOnly, ISignalOnComponentAdded<Damageable>
    {
        public unsafe void OnAdded(Frame f, EntityRef entity, Damageable* damageable)
        {
            var data = f.FindAsset(damageable->DamageableBase);
            damageable->CurrentHealth = data.MaxHealth;
            damageable->MaxHealth = data.MaxHealth;
        }
    }
}