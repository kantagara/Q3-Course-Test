
namespace Quantum
{
    public class AutomaticWeaponData : FiringWeaponData
    {
        public override void OnFireHeld(Frame f, WeaponSystem.Filter filter)
        {
            FireWeapon(f, filter);
        }
    }
}