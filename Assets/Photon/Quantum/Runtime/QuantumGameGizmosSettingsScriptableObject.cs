namespace Quantum {
  using UnityEngine;

  [CreateAssetMenu(menuName = "Quantum/Configurations/GameGizmoSettings", fileName = "QuantumGameGizmoSettings", order = EditorDefines.AssetMenuPriorityConfigurations + 31)]
  [QuantumGlobalScriptableObject(DefaultPath)]
  public class QuantumGameGizmosSettingsScriptableObject : QuantumGlobalScriptableObject<QuantumGameGizmosSettingsScriptableObject> {
    public const string DefaultPath = "Assets/QuantumUser/Editor/QuantumGameGizmosSettings.asset";
    
    [DrawInline]
    public QuantumGameGizmosSettings Settings;
  }
}