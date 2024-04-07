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
                    circle->CurrentTime = PreShrinkState->TimeToNextState;
                    circle->InitialRadius = circle->CurrentRadius;
                    return;
                case INITIALSTATE:
                    circle->CurrentTime = PreShrinkState->TimeToNextState;
                    circle->CurrentRadius = InitialState->InitialRadius;
                    return;
            }
        }

        public void UpdateState(Frame f, ShrinkingCircle* circle)
        {
            circle->CurrentTime -= f.DeltaTime;
            switch (Field)
            {
                case SHRINKSTATE:
                    circle->CurrentRadius -= FPMath.Lerp(circle->InitialRadius, circle->TargetRadius, f.DeltaTime/ ShrinkState->TimeToNextState);
                    return;
            }
        }
    }
}