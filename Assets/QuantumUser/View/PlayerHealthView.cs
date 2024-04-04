using DG.Tweening;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthView : QuantumEntityViewComponent
{
    [SerializeField] private Image healthFill;
    private Tween _tween;
    private void Start()
    {
        QuantumEvent.Subscribe<EventPlayerHealthUpdated>(this, PlayerHealthUpdated);
    }

    private void PlayerHealthUpdated(EventPlayerHealthUpdated callback)
    {
        if (callback.Entity != EntityRef) return ;
        _tween?.Kill();
        _tween = DOTween.To(() => healthFill.fillAmount, x => healthFill.fillAmount = x,
            (callback.CurrentHealth / callback.MaxHealth).AsFloat, .2f);
    }
}