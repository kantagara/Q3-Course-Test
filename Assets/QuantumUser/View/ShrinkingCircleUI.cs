using System;
using Photon.Client;
using Photon.Deterministic;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShrinkingCircleUI : QuantumSceneViewComponent
{
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Image image;

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        var shrinkingCircle = frame.GetSingleton<ShrinkingCircle>();
        message.SetText(MessageTextBasedOnState(shrinkingCircle));
        QuantumEvent.Subscribe<EventCircleChangedState>(this, CircleChangedState);
    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void CircleChangedState(EventCircleChangedState callback)
    {
        var shrinkingCircle = VerifiedFrame.GetSingleton<ShrinkingCircle>();
        message.SetText(MessageTextBasedOnState(shrinkingCircle));
    }

    public override void OnUpdateView()
    {
        base.OnUpdateView();
        var shrinkingCircle = PredictedFrame.GetSingleton<ShrinkingCircle>();
        var time = shrinkingCircle.CurrentState.TimeToNextState.AsFloat;
        var currentTime = shrinkingCircle.CurrentTime.AsFloat;
        
        if (currentTime < 0)
        {
            countdown.gameObject.SetActive(false);
            message.gameObject.SetActive(false);
            return;
        }
        countdown.SetText($"{Mathf.CeilToInt(currentTime)}");
        
        image.fillAmount = (currentTime / time);
    }

    private static string MessageTextBasedOnState(ShrinkingCircle shrinkingCircle)
    {
        return shrinkingCircle.CurrentState.CircleStateUnion.Field switch
        {
            CircleStateUnion.PRESHRINKSTATE => "Go to the safe area!",
            CircleStateUnion.SHRINKSTATE => "Area shrinking!",
            CircleStateUnion.INITIALSTATE => "Area initialized!",
            CircleStateUnion.COOLDOWNSTATE => "Cooldown!",
            _ => throw new ArgumentOutOfRangeException($"Unknown state {shrinkingCircle.CurrentState.CircleStateUnion.Field}")
        };
    }
}