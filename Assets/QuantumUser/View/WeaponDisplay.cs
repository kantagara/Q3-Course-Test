using System;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TMP_Text ammoText;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventOnAmmoChanged>(this, OnAmmoChanged);
        QuantumEvent.Subscribe<EventOnPlayerSpawned>(this, OnPlayerSpawned);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void OnPlayerSpawned(EventOnPlayerSpawned callback)
    {
        if(!callback.Game.PlayerIsLocal(callback.Player)) return;
        var f = callback.Game.Frames.Predicted;
        ammoText.SetText(f.Get<Weapon>(callback.Entity).Ammo.ToString());
    }

    private void OnAmmoChanged(EventOnAmmoChanged callback)
    {
        ammoText.SetText(callback.Value.ToString());
    }
}