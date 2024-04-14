using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public abstract unsafe class WeaponData : AssetObject
    {
        public WeaponType WeaponType;
        public FPVector3 Offset;
        public FP Cooldown;
        public FP Damage = 30;

        public virtual void OnInit(Frame f, EntityRef entity, Weapon* weapon)
        {
            
        }

        public virtual void OnFirePressed(Frame f, WeaponSystem.Filter filter)
        {
            
        }
        
        public virtual void OnUpdate(Frame frame, WeaponSystem.Filter filter)
        {
            
        }

        public virtual void OnFireHeld(Frame f, WeaponSystem.Filter filter)
        {
            
        }

        public virtual void OnFireReleased(Frame f, WeaponSystem.Filter filter)
        {
            
        }
    }
}