namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  [CreateAssetMenu(menuName = "Quantum/Configurations/SessionConfig", order = EditorDefines.AssetMenuPriorityConfigurations + 2)]
  [QuantumGlobalScriptableObject(DefaultPath)]
  public class QuantumDeterministicSessionConfigAsset : QuantumGlobalScriptableObject<QuantumDeterministicSessionConfigAsset> {
    public const string DefaultPath = "Assets/QuantumUser/Resources/SessionConfig.asset";
    
    [DrawInline]
    public DeterministicSessionConfig Config;

    public static DeterministicSessionConfig DefaultConfig => Global.Config;
  }
}