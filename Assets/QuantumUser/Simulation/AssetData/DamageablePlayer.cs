using Photon.Deterministic;

namespace Quantum
{
    public class DamageablePlayer : DamageableBase
    {
        public override unsafe void TakeDamage(Frame frame, EntityRef entityTookDamage, EntityRef damageSource, Damageable* damageable, FP damage)
        {
            damageable->CurrentHealth -= damage * GetDamageModifier(frame, entityTookDamage);
            frame.Events.PlayerHealthUpdated(entityTookDamage, damageable->CurrentHealth, damageable->MaxHealth);
            if (damageable->CurrentHealth > FP._0) return;
            
            var transform = frame.Get<Transform2D>(entityTookDamage);
            var lootDrop = frame.Get<LootDrop>(entityTookDamage);

            var healthLoot = frame.Create(lootDrop.HealthLoot);
            frame.Unsafe.GetPointer<Transform2D>(healthLoot)->Position = transform.Position + transform.Right * 2;
                
            var weaponLoot = frame.Create(lootDrop.WeaponLoot);
            frame.Unsafe.GetPointer<Transform2D>(weaponLoot)->Position = transform.Position + transform.Left * 2;
            frame.Signals.BeforePlayerKilled(entityTookDamage);
            frame.Destroy(entityTookDamage);
            frame.Signals.PlayerKilled();
        }

        private FP GetDamageModifier(Frame frame, EntityRef entityTookDamage)
        {
            if (!frame.TryGet(entityTookDamage, out PlayerStats playerStats)) return FP._1;
            var playerStatsConfig = frame.FindAsset(playerStats.PlayerStatsConfig);
            
            return 2 - playerStatsConfig.DamageModifier;
        }
    }
}