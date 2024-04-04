using Photon.Deterministic;

namespace Quantum
{
    public class DamageablePlayer : DamageableBase
    {
        public override unsafe void TakeDamage(Frame frame, EntityRef entityTookDamage, EntityRef damageSource, Damageable* damageable, FP damage)
        {
            damageable->CurrentHealth -= damage;
            frame.Events.PlayerHealthUpdated(entityTookDamage, damageable->CurrentHealth, damageable->MaxHealth);
            
            if(damageable->CurrentHealth <= FP._0)
            {
                frame.Destroy(entityTookDamage);
            }
        }
    }
}