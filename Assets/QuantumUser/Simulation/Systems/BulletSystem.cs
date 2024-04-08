using Photon.Deterministic;

namespace Quantum.Systems
{
    public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>, ISignalCreateBullet
    {

        public struct Filter
        {
            public EntityRef EntityRef;
            public Bullet* Bullet;
            public Transform2D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.Bullet->Time -= f.DeltaTime;
            if (filter.Bullet->Time <= FP._0)
            {
                f.Destroy(filter.EntityRef);
                return;
            }

            var nextPosition = filter.Transform->Position + filter.Transform->Up * filter.Bullet->Speed * f.DeltaTime;
            if (CheckCollision(f, filter, nextPosition, out var entityHit))
            {
                f.Events.BulletHit(filter.EntityRef);

                if (entityHit != EntityRef.None && f.Unsafe.TryGetPointer(entityHit, out Damageable* damageable))
                {
                    var damageableAsset = f.FindAsset(damageable->DamageableBase);
                    damageableAsset.TakeDamage(f, entityHit, filter.Bullet->Owner, damageable, filter.Bullet->Damage);
                }
                
                f.Destroy(filter.EntityRef);
                return;
            }

            filter.Transform->Position = nextPosition;;
        }
        

        private bool CheckCollision(Frame f, Filter filter, FPVector2 futurePosition, out EntityRef entityHit)
        {
            entityHit = EntityRef.None;
            
            var bullet = f.Get<Bullet>(filter.EntityRef);
            var bulletTransform = f.Unsafe.GetPointer<Transform2D>(filter.EntityRef);
            var playerThatShotBullet = bullet.Owner;
            var collisions = f.Physics2D.LinecastAll(bulletTransform->Position, futurePosition, int.MaxValue,QueryOptions.HitAll & ~QueryOptions.HitTriggers);

            for (var i = 0; i < collisions.Count; i++)
            {
                var collision = collisions[i];
                if(collision.Entity == filter.EntityRef || collision.Entity == playerThatShotBullet)
                    continue;
                entityHit = collision.Entity;
                
                return true;
            }

            return false;
        }

      
        
        
        // Helper method to rotate a vector by a given angle
        private FPVector2 RotateVector(FPVector2 vector, FP angle)
        {
            FP cos = FPMath.Cos(angle);
            FP sin = FPMath.Sin(angle);
            return new FPVector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }


        public void CreateBullet(Frame f, EntityRef owner, AssetRef<FiringWeaponData> weapon)
        {
            Log.Info("Creating the bullet");
            var weaponData = f.FindAsset(weapon);
            var bulletData = weaponData.BulletData;
            
            var bulletEntity = f.Create(bulletData.Entity);
            var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
            var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
            var sourceTransform = f.Get<Transform2D>(owner);

            bulletTransform->Position = sourceTransform.Position + RotateVector(weaponData.Offset.XZ, sourceTransform.Rotation);
            bulletTransform->Rotation = f.Get<Transform2D>(owner).Rotation;
            bullet->Speed = bulletData.Speed;
            bullet->HeightOffset = weaponData.Offset.Y;
            bullet->Owner = owner;
            bullet->Direction = f.Get<Transform2D>(owner).Up;
            bullet->Time = bulletData.Duration; 
            bullet->Damage = weaponData.Damage;
        }
    }
}