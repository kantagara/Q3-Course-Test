using System;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ShrinkingCircleState
    {
        public void EnterState(ShrinkingCircle* circle)
        {
            switch (Field)
            {
                case PRESHRINKSTATE:
                    circle->CurrentTime = PreShrinkState->TimeToNextState;
                    circle->TargetRadius = PreShrinkState->TargetRadius;
                    return;
                case SHRINKSTATE:
                    circle->CurrentTime = ShrinkState->TimeToNextState;
                    circle->InitialRadius = circle->CurrentRadius;
                    circle->ShrinkingCircleTime = FP._0;
                    return;
                case INITIALSTATE:
                    circle->CurrentTime = InitialState->TimeToNextState;
                    circle->CurrentRadius = InitialState->InitialRadius;
                    return;
                case COOLDOWNSTATE:
                    circle->CurrentTime = CooldownState->TimeToNextState;
                    return;
            }
        }

        public void UpdateState(Frame f, ShrinkingCircle* circle)
        {
            circle->CurrentTime -= f.DeltaTime;
            switch (Field)
            {
                case SHRINKSTATE:
                    circle->ShrinkingCircleTime += f.DeltaTime / ShrinkState->TimeToNextState;
                    circle->CurrentRadius = FPMath.Lerp(circle->InitialRadius, circle->TargetRadius, 
                        circle->ShrinkingCircleTime);
                    return;
            }
        }
    }
}