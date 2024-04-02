using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public abstract unsafe class DamageableBase : AssetObject
    {
        [SerializeField] private FP MaxHealth;

        public void Init(Damageable* damageable)
        {
            damageable->CurrentHealth = MaxHealth;
        }

        public virtual void TakeDamage(Frame frame, EntityRef entityRef, Damageable* damageable, FP damage)
        {
            damageable->CurrentHealth -= damage;
            if (damageable->CurrentHealth <= FP._0)
            {
                frame.Destroy(entityRef);
            }
        }

        
    }
}