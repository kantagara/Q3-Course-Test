using System.Collections.Generic;
using System.Linq;
using Quantum;

namespace QuantumUser.View
{
    public class PlayerWeaponView : QuantumEntityViewComponent
    {
        private PlayerWeapon _currentWeapon;
        private Dictionary<WeaponType, PlayerWeapon> _playerWeapons;

        public override void OnActivate(Frame frame)
        {
            base.OnActivate(frame);
            _playerWeapons = GetComponentsInChildren<PlayerWeapon>(true).ToDictionary(x => x.WeaponType, x =>
            {
                x.gameObject.SetActive(false);
                return x;
            });
            _currentWeapon = _playerWeapons[WeaponType.Pistol];
            _currentWeapon.gameObject.SetActive(true);
            QuantumEvent.Subscribe<EventOnWeaponChanged>(this, OnWeaponChanged);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void OnWeaponChanged(EventOnWeaponChanged callback)
        {
            if (callback.Type == _currentWeapon.WeaponType) return;
            if(callback.Entity != EntityRef) return;

            _currentWeapon.Rig.weight = 0;
            _currentWeapon.gameObject.SetActive(false);

            _currentWeapon = _playerWeapons[callback.Type];

            _currentWeapon.Rig.weight = 1;
            _currentWeapon.gameObject.SetActive(true);
        }
    }
}