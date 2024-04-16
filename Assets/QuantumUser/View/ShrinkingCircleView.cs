using System;
using DG.Tweening;
using Quantum;
using UnityEngine;

public unsafe class ShrinkingCircleView : QuantumEntityViewComponent
{
    [SerializeField] private Transform targetCircleSprite;
    [SerializeField] private Transform dangerCircleSprite;


    private void Awake()
    {
        QuantumEvent.Subscribe<EventCircleChangedState>(this, CircleChangedState);
    }

    public override void OnActivate(Frame frame)
    {
        var shrinkingCircle = frame.GetSingleton<ShrinkingCircle>();
        targetCircleSprite.localScale = dangerCircleSprite.localScale = new Vector3(shrinkingCircle.CurrentRadius.AsFloat , shrinkingCircle.CurrentRadius.AsFloat );
        dangerCircleSprite.gameObject.SetActive(false);        
    }


    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    public override void OnLateUpdateView()
    {
        var shrinkingCircle = PredictedFrame.Unsafe.GetPointerSingleton<ShrinkingCircle>();
        dangerCircleSprite.localScale = new Vector3(shrinkingCircle->CurrentRadius.AsFloat , shrinkingCircle->CurrentRadius.AsFloat );
    }

    private void CircleChangedState(EventCircleChangedState callback)
    {
        var shrinkingCircle = PredictedFrame.GetSingleton<ShrinkingCircle>();
        var currentState = shrinkingCircle.CurrentState;
        dangerCircleSprite.gameObject.SetActive(currentState.CircleStateUnion.Field is CircleStateUnion.SHRINKSTATE or CircleStateUnion.PRESHRINKSTATE);
        if(currentState.CircleStateUnion.Field == CircleStateUnion.PRESHRINKSTATE)
            targetCircleSprite.DOScale(new Vector3(shrinkingCircle.TargetRadius.AsFloat , shrinkingCircle.TargetRadius.AsFloat ), 1f);
    }
}