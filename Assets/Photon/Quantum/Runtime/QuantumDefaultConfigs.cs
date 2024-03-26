namespace Quantum {
  using UnityEngine;

  /// <summary>
  /// This class represents a collection of Quantum config assets that are used when no explicit simulation config was assigned to a simulation (through RuntimeConfig).
  /// It's also implementing QuantumGlobalScriptableObject to have one instance globally accessible.
  /// </summary>
  [QuantumGlobalScriptableObject(DefaultPath)]

  [CreateAssetMenu(menuName = "Quantum/Configurations/DefaultConfigs", fileName = "QuantumDefaultConfigs", order = EditorDefines.AssetMenuPriorityConfigurations + 4)]
  public class QuantumDefaultConfigs : QuantumGlobalScriptableObject<QuantumDefaultConfigs> {
    public const string DefaultPath = "Assets/QuantumUser/Resources/QuantumDefaultConfigs.asset";

    public SimulationConfig SimulationConfig;
    public PhysicsMaterial PhysicsMaterial;
    public CharacterController2DConfig CharacterController2DConfig;
    public CharacterController3DConfig CharacterController3DConfig;
    public NavMeshAgentConfig NavMeshAgentConfig;
    public SystemsConfig SystemsConfig;
  }
}