using UnityEngine;

namespace Quantum
{
    public unsafe class PickupItemSpecialWeapon : PickupItemConfigBase
    {
        public WeaponData WeaponData;
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            var weapon = f.Unsafe.GetPointer<Weapon>(entityPickingUp);
            var previousWeapon = WeaponData.WeaponType;

            weapon->CooldownTime = 0;
            WeaponData.OnInit(f, entityPickingUp, weapon);
            
            weapon->WeaponData = WeaponData;
            weapon->Type = WeaponData.WeaponType;
            f.Events.OnPlayerPickedUpSpecialItem(entityPickingUp, f.Get<PlayerLink>(entityPickingUp).Player);
            f.Events.OnWeaponChanged(entityPickingUp, WeaponData.WeaponType, previousWeapon);

            var lootDrop = f.Unsafe.GetPointer<LootDrop>(entityPickingUp);
            lootDrop->WeaponLoot = f.SimulationConfig.WeaponEntityRefByType(weapon->Type).EntityPrototype;
            f.Destroy(entityBeingPickedUp);
        }
    }
}