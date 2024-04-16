using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public unsafe partial struct ShrinkingCircleState
    {
        public void EnterState(ShrinkingCircle* circle)
        {
            circle->CurrentTime = TimeToNextState;
            switch (CircleStateUnion.Field)
            {
                case CircleStateUnion.PRESHRINKSTATE:
                    circle->TargetRadius = CircleStateUnion.PreShrinkState->TargetRadius;
                    return;
                case CircleStateUnion.SHRINKSTATE:
                    circle->InitialRadius = circle->CurrentRadius;
                    circle->ShrinkingCircleTime = FP._0;
                    return;
                case CircleStateUnion.INITIALSTATE:
                    circle->CurrentRadius = CircleStateUnion.InitialState->InitialRadius;
                    return;
            }
        }

        public void UpdateState(Frame f, ShrinkingCircle* circle)
        {
            if(circle->CurrentTime <= FP._0) return;
            
            circle->CurrentTime -= f.DeltaTime;
            
            switch (CircleStateUnion.Field)
            {
                case CircleStateUnion.SHRINKSTATE:
                    circle->ShrinkingCircleTime += f.DeltaTime / TimeToNextState;
                    circle->CurrentRadius = FPMath.Lerp(circle->InitialRadius, circle->TargetRadius, 
                        circle->ShrinkingCircleTime);
                    return;
            }
        }
    }
}