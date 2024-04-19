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
        QuantumEvent.Subscribe<EventOnWeaponChanged>(this, WeaponChanged);
    }

    private void WeaponChanged(EventOnWeaponChanged callback)
    {
        var f = callback.Game.Frames.Verified;
        weaponImage.sprite = f.FindAsset(f.Get<Weapon>(callback.Entity).WeaponData).WeaponSprite;
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void OnPlayerSpawned(EventOnPlayerSpawned callback)
    {
        if(!callback.Game.PlayerIsLocal(callback.Player)) return;
        var f = callback.Game.Frames.Predicted;
        weaponImage.sprite = f.FindAsset(f.Get<Weapon>(callback.Entity).WeaponData).WeaponSprite;
        ammoText.SetText(f.Get<Weapon>(callback.Entity).Ammo.ToString());
    }

    private void OnAmmoChanged(EventOnAmmoChanged callback)
    {
        ammoText.SetText(callback.Value.ToString());
    }
}