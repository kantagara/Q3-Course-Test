using Photon.Deterministic;

namespace Quantum.Systems
{
    public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>, ISignalCreateBullet, ISignalBeforePlayerKilled
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

      
        
        


        public void CreateBullet(Frame f, EntityRef owner, AssetRef<FiringWeaponData> weapon)
        {
            var weaponData = f.FindAsset(weapon);
            var bulletData = weaponData.BulletData;

            bulletData.CreateBullet(f, weaponData, owner);
        }

        public void BeforePlayerKilled(Frame f, EntityRef player)
        {
            foreach (var bulletPair in f.GetComponentIterator<Bullet>())
            {
                if (bulletPair.Component.Owner == player)
                    f.Destroy(bulletPair.Entity);
            }
        }

    }
}