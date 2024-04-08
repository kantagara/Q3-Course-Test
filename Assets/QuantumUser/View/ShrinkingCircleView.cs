using System;
using DG.Tweening;
using Quantum;
using UnityEngine;

public unsafe class ShrinkingCircleView : QuantumEntityViewComponent
{
    [SerializeField] private Transform targetCircleSprite;
    [SerializeField] private Transform dangerCircleSprite;

    private ShrinkingCircle* _shrinkingCircle;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventCircleChangedState>(this, CircleChangedState);
    }

    public override void OnActivate(Frame frame)
    {
        _shrinkingCircle = frame.Unsafe.GetPointerSingleton<ShrinkingCircle>();
        targetCircleSprite.localScale = dangerCircleSprite.localScale = new Vector3(_shrinkingCircle->CurrentRadius.AsFloat / ppu, _shrinkingCircle->CurrentRadius.AsFloat / ppu);
        dangerCircleSprite.gameObject.SetActive(false);        
    }

    private float ppu = 1024;

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    public override void OnLateUpdateView()
    {
        base.OnLateUpdateView();
        dangerCircleSprite.localScale = new Vector3(_shrinkingCircle->CurrentRadius.AsFloat / ppu, _shrinkingCircle->CurrentRadius.AsFloat / ppu);
    }

    private void CircleChangedState(EventCircleChangedState callback)
    {
        var currentState = _shrinkingCircle->CurrentState;
        dangerCircleSprite.gameObject.SetActive(currentState.Field is ShrinkingCircleState.SHRINKSTATE or ShrinkingCircleState.PRESHRINKSTATE);
        if(currentState.Field == ShrinkingCircleState.PRESHRINKSTATE)
            targetCircleSprite.DOScale(new Vector3(_shrinkingCircle->TargetRadius.AsFloat / ppu, _shrinkingCircle->TargetRadius.AsFloat / ppu), 1f);
    }
}