using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe class BulletData : AssetObject
    {
        public EntityPrototype Entity;
        public FP Duration;
        public FP Speed;

        public virtual void CreateBullet(Frame f, FiringWeaponData weaponData, EntityRef owner)
        {
            var bulletEntity = f.Create(Entity);
            var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
            var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
            var sourceTransform = f.Get<Transform2D>(owner);

            bulletTransform->Position = sourceTransform.Position + weaponData.Offset.XZ.Rotate(sourceTransform.Rotation);
            bulletTransform->Rotation = f.Get<Transform2D>(owner).Rotation;
            bullet->Speed = Speed;
            bullet->HeightOffset = weaponData.Offset.Y;
            bullet->Owner = owner;
            bullet->Time = Duration; 
            bullet->Damage = weaponData.Damage;
        }

        
    }
}