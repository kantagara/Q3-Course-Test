using Quantum;

namespace QuantumUser.Simulation.Systems
{
    public class DamageableSystem : SystemSignalsOnly, ISignalOnComponentAdded<Damageable>
    {
        public unsafe void OnAdded(Frame f, EntityRef entity, Damageable* component)
        {
            var data = f.FindAsset(component->DamageableBase);
            component->CurrentHealth = data.MaxHealth;
        }
    }
}