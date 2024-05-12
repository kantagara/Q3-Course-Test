using System;
using Quantum;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Quantum/CharacterInfo")]
public class CharacterInfo : ScriptableObject
{
    [field:SerializeField] public Sprite CharacterSprite { get; private set; }
    [field:SerializeField] public string Name { get; private set; }
    [field:SerializeField] public EntityPrototype EntityPrototype { get; private set; }
    [field:SerializeField] public PlayerStatsConfig Configuration { get; private set; }
}
