using System;
using DG.Tweening;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIView : QuantumEntityViewComponent
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text nicknameText;
    private Tween _tween;
    private void Start()
    {
        QuantumEvent.Subscribe<EventPlayerHealthUpdated>(this, PlayerHealthUpdated);
    }

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        var playerLink = frame.Get<PlayerLink>(EntityRef);
        nicknameText.SetText(frame.GetPlayerData(playerLink.Player).PlayerNickname);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void PlayerHealthUpdated(EventPlayerHealthUpdated callback)
    {
        if (callback.Entity != EntityRef) return ;
        _tween?.Kill();
        _tween = DOTween.To(() => healthFill.fillAmount, x => healthFill.fillAmount = x,
            (callback.CurrentHealth / callback.MaxHealth).AsFloat, .2f);
    }
}