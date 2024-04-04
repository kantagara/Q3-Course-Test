namespace Quantum {
  using System.Collections.Generic;
  using UnityEditor;
  using UnityEngine;
  
  [ExecuteInEditMode]
  public unsafe class QuantumMapData : QuantumMonoBehaviour {
    public Quantum.Map Asset;

    [HideInInspector]
    public QuantumMapDataBakeFlags BakeAllMode = QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB;

    // One-to-one mapping of Quantum static collider entries in QAssetMap to their original source scripts. 
    // Purely for convenience to do post bake mappings and not required by the Quantum simulation.
    public List<MonoBehaviour>      StaticCollider2DReferences = new List<MonoBehaviour>();
    public List<MonoBehaviour>      StaticCollider3DReferences = new List<MonoBehaviour>();
    public List<QuantumEntityView>  MapEntityReferences        = new List<QuantumEntityView>();

    void Update() {
      transform.position = Vector3.zero;
      transform.rotation = Quaternion.identity;
    }
  }
}