using System;
using Photon.Deterministic;
using UnityEngine.Serialization;

namespace Quantum
{
    [Serializable]
    public unsafe class FiringWeaponData : WeaponData
    {
        public byte Ammo;
        public BulletData BulletData;

        public override void OnInit(Frame f, EntityRef entity, Weapon* weapon)
        {
            weapon->Ammo = Ammo;
            
            //If we still don't have playerLink, that's okay
            if(!f.TryGet(entity, out PlayerLink playerLink)) return;
            
            f.Events.OnAmmoChanged(playerLink.Player, weapon->Ammo);
        }

        public override void OnUpdate(Frame f, WeaponSystem.Filter filter)
        {
            var playerInput = f.GetPlayerInput(filter.PlayerLink->Player);
            
            if (playerInput->Fire.WasPressed)
            {
                OnFirePressed(f,filter);
            }
            else if (playerInput->Fire.WasReleased)
            {
                OnFireReleased(f,filter);
            }
            else if (playerInput->Fire.IsDown)
            {
                OnFireHeld(f, filter);
            }
            
            if(filter.Weapon->CooldownTime <= FP._0) return;
            
            filter.Weapon->CooldownTime -= f.DeltaTime;
        }

        public override void OnFirePressed(Frame f, WeaponSystem.Filter filter)
        {
            if (filter.Weapon->CooldownTime <= FP._0 && NoObstacleInFront(f, filter) && filter.Weapon->Ammo > 0)
            {
                var weaponData = f.FindAsset(filter.Weapon->WeaponData);
                filter.Weapon->CooldownTime = weaponData.Cooldown;
                filter.Weapon->Ammo--;
                f.Events.OnAmmoChanged(filter.PlayerLink->Player, filter.Weapon->Ammo);
                f.Signals.CreateBullet(filter.Entity, filter.Weapon->WeaponData.Id);
            }

            
        }
        
        private static bool NoObstacleInFront(Frame f, WeaponSystem.Filter filter)
        {
            var initialLineCast = f.Physics2D.Linecast(filter.Transform->Position,
                filter.Transform->Position + filter.Transform->Up);
            return !initialLineCast.HasValue;   
        }
    }
}