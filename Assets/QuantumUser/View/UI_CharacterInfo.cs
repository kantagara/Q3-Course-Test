using System;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterInfo : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private Image imgDamageReduction;
    [SerializeField] private Image imgFireRate;
    [SerializeField] private Toggle toggleCharacterSelected;

    public void Init(CharacterInfo info, Action<EntityPrototype> characterSelectedCallback, ToggleGroup toggleGroup, bool isOn)
    {
        characterImage.sprite = info.CharacterSprite;
        characterName.text = info.Name;
        imgDamageReduction.fillAmount = 0.5f * (info.Configuration.DamageModifier.AsFloat - 1) + 0.5f;
        imgFireRate.fillAmount = 0.5f * (info.Configuration.FireRateModifier.AsFloat - 1) + 0.5f;
        toggleCharacterSelected.group = toggleGroup;
        toggleCharacterSelected.isOn = isOn;
        toggleCharacterSelected.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                characterSelectedCallback(info.EntityPrototype);
        });
    }

    private void OnDestroy()
    {
        toggleCharacterSelected.onValueChanged.RemoveAllListeners();
    }
}