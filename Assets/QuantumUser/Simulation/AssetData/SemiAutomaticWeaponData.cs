using Photon.Deterministic;


namespace Quantum
{
    public unsafe class SemiAutomaticWeaponData : FiringWeaponData
    {
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
    }
}