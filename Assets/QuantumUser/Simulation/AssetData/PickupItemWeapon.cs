namespace Quantum
{
    public unsafe class PickupItemWeapon : PickupItemConfigBase
    {
        public WeaponData WeaponData;
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            var weapon = f.Unsafe.GetPointer<Weapon>(entityPickingUp);
            
            weapon->WeaponData = WeaponData;
            weapon->CooldownTime = 0;
            weapon->Type = WeaponData.WeaponType;
            WeaponData.OnInit(f, entityPickingUp, weapon);
            f.Events.OnWeaponChanged(entityPickingUp, WeaponData.WeaponType);
            f.Destroy(entityBeingPickedUp);
        }
    }
}