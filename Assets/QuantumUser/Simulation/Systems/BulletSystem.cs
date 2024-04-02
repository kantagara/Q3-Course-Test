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
            if (DestroyExpiredBullet(f, filter))
                return;

            var nextPosition = filter.Transform->Position + filter.Transform->Up * filter.Bullet->Speed * f.DeltaTime;
            
            if (CheckCollision(f, filter, nextPosition))
            {
                f.Events.BulletHit(filter.EntityRef);
                f.Destroy(filter.EntityRef);
                return;
            }

            filter.Transform->Position = nextPosition;;
        }
        

        private bool CheckCollision(Frame f, Filter filter, FPVector2 futurePosition)
        {
            var bullet = f.Get<Bullet>(filter.EntityRef);
            var bulletTransform = f.Unsafe.GetPointer<Transform2D>(filter.EntityRef);
            var playerThatShotBullet = bullet.Owner;
            var collisions = f.Physics2D.LinecastAll(bulletTransform->Position, futurePosition, int.MaxValue,QueryOptions.HitAll & ~QueryOptions.HitTriggers);

            for (var i = 0; i < collisions.Count; i++)
            {
                var collision = collisions[i];
                Log.Info(collision.Entity);
                if(collision.Entity == filter.EntityRef || collision.Entity == playerThatShotBullet)
                    continue;
                
                return true;
            }

            return false;
        }

        private static bool DestroyExpiredBullet(Frame f, Filter filter)
        {
            filter.Bullet->Time -= f.DeltaTime;
            if (filter.Bullet->Time <= FP._0)
            {
                f.Destroy(filter.EntityRef);
                return true;
            }

            return false;
        }
        
        private void InitializeBullet(Frame f, EntityRef source, Weapon weapon)
        {
            var weaponData = f.FindAsset<WeaponData>(weapon.WeaponData.Id);
            var bulletData = weaponData.BulletData;
            
            var bulletEntity = f.Create(bulletData.Entity);
            
            var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
            var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
            var sourceTransform = f.Get<Transform2D>(source);

            bulletTransform->Position = sourceTransform.Position + RotateVector(weaponData.Offset.XZ, sourceTransform.Rotation);
            bulletTransform->Rotation = f.Get<Transform2D>(source).Rotation;
            bullet->Speed = bulletData.Speed;
            bullet->HeightOffset = weaponData.Offset.Y;
            bullet->Owner = source;
            bullet->Direction = f.Get<Transform2D>(source).Up;
            bullet->Time = bulletData.Duration;
            //bullet->Damage = bulletData.Damage;
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

        

        public void CreateBullet(Frame f, EntityRef Owner, Weapon Weapon)
        {
            InitializeBullet(f, Owner, Weapon);
        }
    }
}