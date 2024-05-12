using Photon.Deterministic;

namespace Quantum
{
    public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>, ISignalOnComponentAdded<Weapon>
    {

        public override void Update(Frame f, ref Filter filter)
        {
            var data = f.FindAsset(filter.Weapon->WeaponData);
            data.OnUpdate(f, filter);
        }

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public PlayerStats* PlayerStats;
            public Transform2D* Transform;
            public Weapon* Weapon;
        }

        public void OnAdded(Frame f, EntityRef entity, Weapon* weapon)
        {
            var data = f.FindAsset(weapon->WeaponData);
            data.OnInit(f, entity, weapon);
        }
    }
}