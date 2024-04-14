using System.Collections.Generic;

namespace Quantum
{
    public partial class SimulationConfig : AssetObject
    {
        public List<WeaponEntityRef> weaponEntityRefs = new ();

        private Dictionary<WeaponType, WeaponEntityRef> _dictionary;

        public WeaponEntityRef WeaponEntityRefByType(WeaponType weaponType)
        {
            InitDictIfEmpty();
            return _dictionary[weaponType];
        }

        private void InitDictIfEmpty()
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<WeaponType, WeaponEntityRef>();
                foreach (var weapon in weaponEntityRefs)
                {
                    _dictionary.Add(weapon.Type, weapon);
                }
            }
        }
    }
}