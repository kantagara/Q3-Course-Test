using Photon.Deterministic;

namespace Quantum
{
    public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>
    {

        public override void Update(Frame f, ref Filter filter)
        {
            var playerInput = f.GetPlayerInput(filter.PlayerLink->Player);

            if (filter.Weapon->CooldownTime <= FP._0 && playerInput->Fire.WasPressed && NoObstacleInFront(f, filter))
            {
                var weaponData = f.FindAsset(filter.Weapon->WeaponData);
                filter.Weapon->CooldownTime = weaponData.Cooldown;
                f.Signals.CreateBullet(filter.Entity, *filter.Weapon);
            }

            if(filter.Weapon->CooldownTime <= FP._0) return;
            
            filter.Weapon->CooldownTime -= f.DeltaTime;
        }

        private bool NoObstacleInFront(Frame f, Filter filter)
        {
            var initialLineCast = f.Physics2D.Linecast(filter.Transform->Position,
                filter.Transform->Position + filter.Transform->Up);
            return !initialLineCast.HasValue;   
        }

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public Transform2D* Transform;
            public Weapon* Weapon;
        }
    }
}