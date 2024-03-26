using Photon.Deterministic;

namespace Quantum
{
    public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>
    {

        public override void Update(Frame f, ref Filter filter)
        {
            var playerInput = f.GetPlayerInput(filter.PlayerLink->Player);
            
            if (filter.Weapon->CooldownTime <= FP._0 && playerInput->Fire.WasPressed)
            {
                var weaponData = f.FindAsset(filter.Weapon->WeaponData);
                filter.Weapon->CooldownTime = weaponData.Cooldown;
            }

            if(filter.Weapon->CooldownTime <= FP._0) return;
            
            filter.Weapon->CooldownTime -= f.DeltaTime;
        }
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public Weapon* Weapon;
        }
    }
}