namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Deterministic;
  using UnityEngine;
  
  [ExecuteInEditMode]
  public unsafe class QuantumMapData : QuantumMonoBehaviour {
    [Flags]
    public enum DrawMode {
      PhysicsArea    = 1 << 2,
      PhysicsBuckets = 1 << 3,
      NavMeshArea    = 1 << 4,
      NavMeshGrid    = 1 << 5,
      All            = PhysicsArea | PhysicsBuckets | NavMeshArea | NavMeshGrid,
    }

    public Quantum.Map Asset;

    public DrawMode DrawGridMode = DrawMode.All;

    [HideInInspector]
    public QuantumMapDataBakeFlags BakeAllMode = QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB;

    // One-to-one mapping of Quantum static collider entries in QAssetMap to their original source scripts. 
    // Purely for convenience to do post bake mappings and not required by the Quantum simulation.
    public List<MonoBehaviour>      StaticCollider2DReferences = new List<MonoBehaviour>();
    public List<MonoBehaviour>      StaticCollider3DReferences = new List<MonoBehaviour>();
    public List<QuantumEntityView> MapEntityReferences        = new List<QuantumEntityView>();

    void Update() {
      transform.position = Vector3.zero;
      transform.rotation = Quaternion.identity;
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
      DrawGizmos(false);
    }

    private void OnDrawGizmosSelected() {
      DrawGizmos(true);
    }

    void DrawGizmos(bool selected) {
      if (Asset) {
        FPMathUtils.LoadLookupTables();
        
        if (DrawGridMode != default) {
          var worldSize   = FPMath.Min(Asset.WorldSize, FP.UseableMax);
          var physicsArea = new FPVector2(worldSize, worldSize);

          if (Asset.SortingAxis == PhysicsCommon.SortAxis.X) {
            physicsArea.X = FPMath.Min(physicsArea.X, FP.UseableMax / 2);
          } else {
            physicsArea.Y = FPMath.Min(physicsArea.Y, FP.UseableMax / 2);
          }

          if ((DrawGridMode & DrawMode.PhysicsArea) == DrawMode.PhysicsArea) {
            GizmoUtils.DrawGizmosBox(transform, physicsArea.ToUnityVector3(), GlobalGizmosSettings.PhysicsGridColor);
          }

          if ((DrawGridMode & DrawMode.PhysicsBuckets) == DrawMode.PhysicsBuckets) {
            var bottomLeft = transform.position - physicsArea.ToUnityVector3() / 2;

            if (Asset.BucketingAxis == PhysicsCommon.BucketAxis.X) {
              var bucketSize = physicsArea.X.AsFloat / Asset.BucketsCount;
              GizmoUtils.DrawGizmoGrid(bottomLeft, Asset.BucketsCount, 1, bucketSize, physicsArea.Y.AsFloat, GlobalGizmosSettings.PhysicsGridColor.Alpha(0.4f));
            } else {
              var bucketSize = physicsArea.Y.AsFloat / Asset.BucketsCount;
              GizmoUtils.DrawGizmoGrid(bottomLeft, 1, Asset.BucketsCount, physicsArea.X.AsFloat, bucketSize, GlobalGizmosSettings.PhysicsGridColor.Alpha(0.4f));
            }
          }

          if ((DrawGridMode & DrawMode.NavMeshArea) == DrawMode.NavMeshArea) {
            GizmoUtils.DrawGizmosBox(transform, new FPVector2(Asset.WorldSizeX, Asset.WorldSizeY).ToUnityVector3(), GlobalGizmosSettings.NavMeshGridColor);
          }

          if ((DrawGridMode & DrawMode.NavMeshGrid) == DrawMode.NavMeshGrid) {
            var bottomLeft = transform.position - (-Asset.WorldOffset).ToUnityVector3();
            GizmoUtils.DrawGizmoGrid(bottomLeft, Asset.GridSizeX, Asset.GridSizeY, Asset.GridNodeSize, GlobalGizmosSettings.NavMeshGridColor.Alpha(0.4f));
          }
        }

        if (QuantumRunner.Default) {
          var mesh = QuantumRunner.Default?.Game?.Frames?.Verified?.Physics3D.SceneMesh;
          if (mesh != null) {
            if (GlobalGizmosSettings.DrawSceneMeshCells) {
              mesh.VisitCells((x, y, z, tris, count) => {
                if (count > 0) {
                  var c = mesh.GetNodeCenter(x, y, z).ToUnityVector3();
                  var s = default(Vector3);
                  s.x = mesh.CellSize;
                  s.y = mesh.CellSize;
                  s.z = mesh.CellSize;
                  GizmoUtils.DrawGizmosBox(c, s, GlobalGizmosSettings.PhysicsGridColor, style: QuantumGizmoStyle.FillDisabled);
                }
              });
            }

            if (GlobalGizmosSettings.DrawSceneMeshTriangles) {
              mesh.VisitCells((x, y, z, tris, count) => {
                for (int i = 0; i < count; ++i) {
                  var t = mesh.GetTriangle(tris[i]);
                  Gizmos.color = GlobalGizmosSettings.PhysicsGridColor;
                  Gizmos.DrawLine(t->A.ToUnityVector3(), t->B.ToUnityVector3());
                  Gizmos.DrawLine(t->B.ToUnityVector3(), t->C.ToUnityVector3());
                  Gizmos.DrawLine(t->C.ToUnityVector3(), t->A.ToUnityVector3());
                }
              });
            }
          }
        }
      }
    }
#endif
  }
}