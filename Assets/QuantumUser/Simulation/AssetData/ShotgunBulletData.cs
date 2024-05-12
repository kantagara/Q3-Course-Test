using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class ShotgunBulletData : BulletData
    {
        public int NumberOfBullets;
        public FP SpreadAngle;
        
        public override void CreateBullet(Frame f, FiringWeaponData weaponData, EntityRef owner)
        {
            var sourceTransform = f.Get<Transform2D>(owner);
            var spreadAngleRad = FP.Deg2Rad * SpreadAngle;

            for (int i = 0; i < NumberOfBullets; i++)
            {
                var bulletEntity = f.Create(Entity);
                var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
                var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
                bulletTransform->Position = sourceTransform.Position + weaponData.Offset.XZ.Rotate(sourceTransform.Rotation);
                bulletTransform->Rotation = f.Get<Transform2D>(owner).Rotation + FPMath.Lerp(-spreadAngleRad, spreadAngleRad, (FP)i / (NumberOfBullets - 1));
                bullet->Speed = Speed;
                bullet->HeightOffset = weaponData.Offset.Y;
                bullet->Owner = owner;
                bullet->Time = Duration; 
                bullet->Damage = weaponData.Damage;
            }
        }
    }
}