using Photon.Deterministic;


namespace Quantum
{
    public abstract unsafe class FiringWeaponData : WeaponData
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
        
        
        protected static bool NoObstacleInFront(Frame f, WeaponSystem.Filter filter)
        {
            var initialLineCast = f.Physics2D.Linecast(filter.Transform->Position,
                filter.Transform->Position + filter.Transform->Up);
            return !initialLineCast.HasValue;   
        }
    }
}