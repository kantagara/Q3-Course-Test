namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;
  
  [ExecuteInEditMode]
  public class QuantumStaticTerrainCollider3D : QuantumMonoBehaviour {
    public Quantum.TerrainCollider Asset;
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();
    public QuantumMeshGizmos GizmosOptions = QuantumMeshGizmos.Default;

    [HideInInspector]
    public Boolean SmoothSphereMeshCollisions = false;

#pragma warning disable 618 // use of obsolete
    [Obsolete("Use 'Settings.MutableMode' instead.")]
    public PhysicsCommon.StaticColliderMutableMode MutableMode => Settings.MutableMode;
#pragma warning restore 618

    [NonSerialized]
    private Vector3[] _gizmosTrianglePoints;

    [NonSerialized]
    private int[] _gizmosTriangleSegments;

    [NonSerialized]
    private Vector3[] _gizmosNormalPoints;

    public void Bake() {
#if QUANTUM_ENABLE_TERRAIN && !QUANTUM_DISABLE_TERRAIN
      FPMathUtils.LoadLookupTables();

      var t = GetComponent<Terrain>();

      Asset.Resolution = t.terrainData.heightmapResolution;

      Asset.HeightMap = new FP[Asset.Resolution * Asset.Resolution];
      Asset.Position  = transform.position.ToFPVector3();
      Asset.Scale     = t.terrainData.heightmapScale.ToFPVector3();

      for (int i = 0; i < Asset.Resolution; i++) {
        for (int j = 0; j < Asset.Resolution; j++) {
          Asset.HeightMap[j + i * Asset.Resolution] = FP.FromFloat_UNSAFE(t.terrainData.GetHeight(i, j));
        }
      }

      // support to Terrain Paint Holes: https://docs.unity3d.com/2019.4/Documentation/Manual/terrain-PaintHoles.html
      Asset.HoleMask = new ulong[(Asset.Resolution * Asset.Resolution - 1) / 64 + 1];

      for (int i = 0; i < Asset.Resolution - 1; i++) {
        for (int j = 0; j < Asset.Resolution - 1; j++) {
          if (t.terrainData.IsHole(i, j)) {
            Asset.SetHole(i, j);
          }
        }
      }

#if UNITY_EDITOR
      EditorUtility.SetDirty(Asset);
      EditorUtility.SetDirty(this);
#endif
#endif
    }
    
#if QUANTUM_ENABLE_TERRAIN && !QUANTUM_DISABLE_TERRAIN
#if UNITY_EDITOR
    void OnDrawGizmos() {
      DrawGizmos(false);
    }

    private void OnDrawGizmosSelected() {
      DrawGizmos(true);
    }

    void DrawGizmos(bool selected) {
      if (ShouldDrawTrianglesGizmos()) {
        if (_gizmosTrianglePoints == null || _gizmosTriangleSegments == null) {
          QuantumStaticMeshCollider3D.ComputeTriangleGizmos(Asset.MeshTriangles, ref _gizmosTrianglePoints, ref _gizmosTriangleSegments);
        }

        Handles.color = GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected);
        Handles.matrix = Matrix4x4.identity;
        Handles.DrawLines(_gizmosTrianglePoints, _gizmosTriangleSegments);
        Handles.color = Color.white;
      } else {
        _gizmosTrianglePoints = null;
        _gizmosTriangleSegments = null;
      }

      if (ShouldDrawNormalsGizmos()) {
        if (_gizmosNormalPoints == null) {
          QuantumStaticMeshCollider3D.ComputeNormalGizmos(Asset.MeshTriangles, ref _gizmosNormalPoints);
        }

        Handles.color = GlobalGizmosSettings.GetSelectedColor(Color.red, selected);
        Handles.matrix = Matrix4x4.identity;
        Handles.DrawLines(_gizmosNormalPoints);
        Handles.color = Color.white;
      } else {
        _gizmosNormalPoints = null;
      }
    }

    private bool ShouldDrawTrianglesGizmos() {
      return GlobalGizmosSettings.DrawStaticMeshTriangles && (GizmosOptions & QuantumMeshGizmos.DrawTriangles) == QuantumMeshGizmos.DrawTriangles;
    }

    private bool ShouldDrawNormalsGizmos() {
      return GlobalGizmosSettings.DrawStaticMeshNormals && (GizmosOptions & QuantumMeshGizmos.DrawNormals) == QuantumMeshGizmos.DrawNormals;
    }
#endif
#endif
  }
}
