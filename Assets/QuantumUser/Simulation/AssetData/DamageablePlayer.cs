﻿using Photon.Deterministic;

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
                var transform = frame.Get<Transform2D>(entityTookDamage);
                var lootDrop = frame.Get<LootDrop>(entityTookDamage);

                var healthLoot = frame.Create(lootDrop.HealthLoot);
                frame.Unsafe.GetPointer<Transform2D>(healthLoot)->Position = transform.Position + transform.Right * 2;
                
                var weaponLoot = frame.Create(lootDrop.WeaponLoot);
                frame.Unsafe.GetPointer<Transform2D>(weaponLoot)->Position = transform.Position + transform.Left * 2;
                frame.Signals.PlayerKilled(entityTookDamage);
                frame.Destroy(entityTookDamage);
            }
        }
    }
}