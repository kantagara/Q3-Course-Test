using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public abstract unsafe class DamageableBase : AssetObject
    {
        public FP MaxHealth;
        
        public abstract void TakeDamage(Frame frame, EntityRef entityTookDamage, EntityRef source, Damageable* damageable, FP damage);
    }

    
}