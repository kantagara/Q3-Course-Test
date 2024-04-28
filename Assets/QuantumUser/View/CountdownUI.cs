using System;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : QuantumSceneViewComponent
{
    [SerializeField] private TMP_Text countdown;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Image image;

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        QuantumEvent.Subscribe<EventCircleChangedState>(this, CircleChangedState);
        QuantumEvent.Subscribe<EventGameManagerChangedState>(this, GameManagerChangedState);
    }

    private void GameManagerChangedState(EventGameManagerChangedState callback)
    {
        if (callback.NewState != GameState.Playing) return;
        MessageTextBasedOnState(callback.Game.Frames.Verified.GetSingleton<ShrinkingCircle>());
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
        var f = PredictedFrame;
        var gameManager = f.GetSingleton<GameManager>();
        
        switch (gameManager.CurrentGameState)
        {
            case GameState.WaitingForPlayers: UpdateGameManagerTimer(f);
                break;
            case GameState.Playing: UpdateShrinkingCircleTimer();
                break;
            case GameState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateGameManagerTimer(Frame f)
    {
        var gameManager = f.GetSingleton<GameManager>();
        var gameManagerConfig = f.FindAsset(gameManager.GameManagerConfig);
        message.SetText("Waiting for all players");
        countdown.SetText($"{Mathf.CeilToInt(gameManager.TimeToWaitForPlayers.AsFloat)}");
        image.fillAmount = gameManager.TimeToWaitForPlayers.AsFloat / gameManagerConfig.TimeToWaitForPlayers.AsFloat ;
    }

    private void UpdateShrinkingCircleTimer()
    {
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

        image.fillAmount = currentTime / time;
    }

    private static string MessageTextBasedOnState(ShrinkingCircle shrinkingCircle)
    {
        return shrinkingCircle.CurrentState.CircleStateUnion.Field switch
        {
            CircleStateUnion.PRESHRINKSTATE => "Go to the safe area!",
            CircleStateUnion.SHRINKSTATE => "Area shrinking!",
            CircleStateUnion.INITIALSTATE => "Area initialized!",
            CircleStateUnion.COOLDOWNSTATE => "Cooldown!",
            _ => throw new ArgumentOutOfRangeException(
                $"Unknown state {shrinkingCircle.CurrentState.CircleStateUnion.Field}")
        };
    }
}