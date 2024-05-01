using Photon.Deterministic;


namespace Quantum
{
    public unsafe class SemiAutomaticWeaponData : FiringWeaponData
    {
        public override void OnFirePressed(Frame f, WeaponSystem.Filter filter)
        {
            FireWeapon(f, filter);
        }
    }
}