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

        private void CheckIfPlayerIsOutsideCircle(Frame frame, Filter filter)
        {
        }
        
        private void UpdateState(Frame f)
        {
            var shrinkingCircle = f.Unsafe.GetPointerSingleton<ShrinkingCircle>();
            var states = f.ResolveList(shrinkingCircle->States);
            
            shrinkingCircle->CurrentState.UpdateState(f, shrinkingCircle);
            
            if (shrinkingCircle->CurrentTime > FP._0) return;
            if (shrinkingCircle->CurrentIndex >= states.Count - 1) return;
            
            shrinkingCircle->CurrentIndex++;
            shrinkingCircle->CurrentState = states[shrinkingCircle->CurrentIndex];
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);
            f.Events.CircleChangedState();
        }
        

        public void OnAdded(Frame f, EntityRef entity, ShrinkingCircle* shrinkingCircle)
        {
            shrinkingCircle->CurrentIndex = 0;
            var states = f.ResolveList(shrinkingCircle->States);
            shrinkingCircle->CurrentState = states[0];
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);

            var tr = f.Unsafe.GetPointer<Transform2D>(entity);
            tr->Position = new FPVector2(f.RNG->Next((FP)(-20), 20), f.RNG->Next((FP)(-20), 20));
        }
    }
}