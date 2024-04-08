using Photon.Deterministic;
using Quantum;

namespace QuantumUser.Simulation.Systems
{
    public unsafe class ShrinkingCircleSystem : SystemMainThreadFilter<ShrinkingCircleSystem.Filter>, ISignalOnComponentAdded<ShrinkingCircle>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public PlayerLink* PlayerLink;
            public Damageable* Damageable;
            public Transform2D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            UpdateState(f);
            CheckIfPlayerIsOutsideCircle(f, filter);
        }

        private void CheckIfPlayerIsOutsideCircle(Frame f, Filter filter)
        {
            var shrinkingCircle = f.Unsafe.GetPointerSingleton<ShrinkingCircle>();
            var config = f.FindAsset(shrinkingCircle->ShrinkingCircleConfig);
            
            if (FPVector2.Distance(filter.Transform->Position, shrinkingCircle->Position) >
                shrinkingCircle->CurrentRadius / 2)
            {
                var damageableAsset = f.FindAsset(filter.Damageable->DamageableBase);
                damageableAsset.TakeDamage(f, filter.EntityRef, EntityRef.None, filter.Damageable, config.DamageDealingPerSecond * f.DeltaTime);
            }
        
        }
        
        private void UpdateState(Frame f)
        {
            var shrinkingCircle = f.Unsafe.GetPointerSingleton<ShrinkingCircle>();
            var states = f.FindAsset(shrinkingCircle->ShrinkingCircleConfig).States;            
            shrinkingCircle->CurrentState.UpdateState(f, shrinkingCircle);
            
            if (shrinkingCircle->CurrentTime > FP._0) return;
            if (shrinkingCircle->CurrentIndex >= states.Length - 1) return;
            
            shrinkingCircle->CurrentIndex++;
            states[shrinkingCircle->CurrentIndex].Materialize(f, ref shrinkingCircle->CurrentState);
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);
            f.Events.CircleChangedState();
        }
        

        public void OnAdded(Frame f, EntityRef entity, ShrinkingCircle* shrinkingCircle)
        {
            shrinkingCircle->CurrentIndex = 0;
            var states = f.FindAsset(shrinkingCircle->ShrinkingCircleConfig).States;            
            states[0].Materialize(f, ref shrinkingCircle->CurrentState);
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);

            var tr = f.Unsafe.GetPointer<Transform2D>(entity);
            tr->Position = new FPVector2(f.RNG->Next((FP)(-20), 20), f.RNG->Next((FP)(-20), 20));
            shrinkingCircle->Position = tr->Position;
        }
    }
}