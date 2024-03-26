namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;

  public class QuantumStaticMeshCollider3D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
    public Mesh                          Mesh;
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();
    public QuantumMeshGizmos             GizmosOptions = QuantumMeshGizmos.Default;

    [Header("Experimental")]
    public Boolean SmoothSphereMeshCollisions = false;

    [NonSerialized]
    public MeshTriangleVerticesCcw MeshTriangles = new MeshTriangleVerticesCcw();

    [NonSerialized]
    private Vector3[] _gizmosTrianglePoints;

    [NonSerialized]
    private int[] _gizmosTriangleSegments;

    [NonSerialized]
    private Vector3[] _gizmosNormalPoints;

    void Reset() {
      // default to mesh collider
      var meshCollider = GetComponent<MeshCollider>();
      if (meshCollider) {
        Mesh = meshCollider.sharedMesh;
      }

      // try mesh filter
      else {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter) {
          Mesh = meshFilter.sharedMesh;
        }
      }
    }

    public bool Bake(Int32 index) {
      FPMathUtils.LoadLookupTables(false);

      if (!Mesh) {
        Reset();

        if (!Mesh) {
          // log warning
          Debug.LogWarning($"No mesh for static mesh collider selected on {gameObject.name}");

          // clear triangles and return
          MeshTriangles.Triangles = Array.Empty<TriangleVerticesCcw>();
          MeshTriangles.Vertices = Array.Empty<FPVector3>();
          MeshTriangles.MeshColliderIndex = index;

          // don't do anything else
          return false;
        }
      }

      var localToWorld = transform.localToWorldMatrix;

      // Normally, Unity Mesh triangles are defined in CW order. However, if the local-to-world
      // transformation scales the mesh with negative values in an even number of axes,
      // this will result in vertices that now define a CCW triangle, which needs to be taken
      // into consideration when baking the transformed vertices in the static mesh collider.
      var scale = localToWorld.lossyScale;
      var isCcw = scale.x * scale.y * scale.z < 0;

      var degenerateCount = 0;

      MeshTriangles.MeshColliderIndex = index;
      MeshTriangles.Vertices = new FPVector3[Mesh.vertices.Length];
      MeshTriangles.Triangles = new TriangleVerticesCcw[Mesh.triangles.Length / 3];

      // Save the arrays to reduce overhead of the property calls during the loop.
      var cachedUnityTriangles = Mesh.triangles;
      var cachedUnityVertices  = Mesh.vertices;

      for (int vertexId = 0; vertexId < cachedUnityVertices.Length; vertexId++) {
        MeshTriangles.Vertices[vertexId] = localToWorld.MultiplyPoint(cachedUnityVertices[vertexId]).ToFPVector3();
      }

      for (int i = 0; i < cachedUnityTriangles.Length; i += 3) {
        var vertexA = cachedUnityTriangles[i];
        var vertexB = cachedUnityTriangles[i + 1];
        var vertexC = cachedUnityTriangles[i + 2];

        TriangleVerticesCcw triVertices;
        if (isCcw) {
          triVertices = new TriangleVerticesCcw(vertexA, vertexB, vertexC);
        } else {
          triVertices = new TriangleVerticesCcw(vertexC, vertexB, vertexA);
        }

        MeshTriangles.Triangles[i/3] = triVertices;

        var vA = MeshTriangles.Vertices[triVertices.VertexA];
        var vB = MeshTriangles.Vertices[triVertices.VertexB];
        var vC = MeshTriangles.Vertices[triVertices.VertexC];
        var edgeAB = vB - vA;
        var edgeBC = vC - vB;
        var edgeCA = vA - vC;
        var normal = FPVector3.Cross(edgeAB, edgeCA).Normalized;

        if (normal == default || edgeAB.SqrMagnitude == default || edgeBC.SqrMagnitude == default || edgeCA.SqrMagnitude == default) {
          degenerateCount++;
          Debug.LogWarning($"Degenerate triangle on game object {gameObject.name} using mesh {Mesh.name}. " +
                            $"Triangle vertices in world space: \n" +
                            $"Vertex A: index {vertexA}, value {localToWorld.MultiplyPoint(cachedUnityVertices[vertexA])} \n" +
                            $"Vertex B: index {vertexB}, value {localToWorld.MultiplyPoint(cachedUnityVertices[vertexB])} \n" +
                            $"Vertex C: index {vertexC}, value {localToWorld.MultiplyPoint(cachedUnityVertices[vertexC])}.");
        }
      }

      if (degenerateCount > 0) {
        Array.Resize(ref MeshTriangles.Triangles, MeshTriangles.Triangles.Length - degenerateCount);
      }

#if UNITY_EDITOR
      if (ShouldDrawTrianglesGizmos()) {
        ComputeTriangleGizmos(MeshTriangles, ref _gizmosTrianglePoints, ref _gizmosTriangleSegments);
      } else {
        _gizmosTrianglePoints = null;
        _gizmosTriangleSegments = null;
      }

      if (ShouldDrawNormalsGizmos()) {
        ComputeNormalGizmos(MeshTriangles, ref _gizmosNormalPoints);
      } else {
        _gizmosNormalPoints = null;
      }

      EditorUtility.SetDirty(this);
#endif
      return MeshTriangles.Triangles.Length > 0;
    }

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
          ComputeTriangleGizmos(MeshTriangles, ref _gizmosTrianglePoints, ref _gizmosTriangleSegments);
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
          ComputeNormalGizmos(MeshTriangles, ref _gizmosNormalPoints);
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

    public static void ComputeTriangleGizmos(MeshTriangleVerticesCcw mesh, ref Vector3[] triPoints, ref int[] triSegments) {
      var gizmosTrianglePointsCount = mesh.Vertices.Length;
      if (triPoints == null || triPoints.Length < gizmosTrianglePointsCount) {
        triPoints = new Vector3[gizmosTrianglePointsCount];
      }

      for (int i = 0; i < mesh.Vertices.Length; i++) {
        triPoints[i] = mesh.Vertices[i].ToUnityVector3();
      }

      var gizmosTriangleSegmentsCount = mesh.Triangles.Length * 6;
      if (triSegments == null || triSegments.Length != gizmosTriangleSegmentsCount) {
        triSegments = new int[gizmosTriangleSegmentsCount];
      }

      for (int i = 0; i < mesh.Triangles.Length; i++) {
        var tri = mesh.Triangles[i];
        var segmentIdx = 6 * i;

        triSegments[segmentIdx++] = tri.VertexA;
        triSegments[segmentIdx++] = tri.VertexB;

        triSegments[segmentIdx++] = tri.VertexB;
        triSegments[segmentIdx++] = tri.VertexC;

        triSegments[segmentIdx++] = tri.VertexC;
        triSegments[segmentIdx] = tri.VertexA;
      }
    }

    public static void ComputeNormalGizmos(MeshTriangleVerticesCcw mesh, ref Vector3[] normalPoints) {
      var gizmosNormalsPointsCount = mesh.Triangles.Length * 2;
      if (normalPoints == null || normalPoints.Length < gizmosNormalsPointsCount) {
        normalPoints = new Vector3[gizmosNormalsPointsCount];
      }

      for (int i = 0; i < mesh.Triangles.Length; i++) {
        var tri = mesh.Triangles[i];

        var vA = mesh.Vertices[tri.VertexA].ToUnityVector3();
        var vB = mesh.Vertices[tri.VertexB].ToUnityVector3();
        var vC = mesh.Vertices[tri.VertexC].ToUnityVector3();

        var center = (vA + vB + vC) / 3f;
        var normal = Vector3.Cross(vB - vA, vA - vC).normalized;

        var pointIdx = 2 * i;
        normalPoints[pointIdx++] = center;
        normalPoints[pointIdx] = center + normal;
      }
    }
    
    public static void DrawMeshTrianglesGizmos(MeshTriangleVerticesCcw mesh, bool drawTriangles, bool drawNormals, bool selected) {
      if (mesh.Triangles == null || drawTriangles == false && drawNormals == false) {
        return;
      }

      var gizmosSettings = QuantumGameGizmosSettingsScriptableObject.Global.Settings;
      foreach (var tri in mesh.Triangles) {
        var vA = mesh.Vertices[tri.VertexA].ToUnityVector3();
        var vB = mesh.Vertices[tri.VertexB].ToUnityVector3();
        var vC = mesh.Vertices[tri.VertexC].ToUnityVector3();

        if (drawTriangles) {
          GizmoUtils.DrawGizmosTriangle(vA, vB, vC, gizmosSettings.GetSelectedColor(gizmosSettings.StaticColliderColor, selected));
        }

        if (drawNormals) {
          var center = (vA + vB + vC) / 3f;
          var normal = Vector3.Cross(vB - vA, vA - vC).normalized;

          Gizmos.color = gizmosSettings.GetSelectedColor(Color.red, selected);
          Gizmos.DrawRay(center, normal);
          Gizmos.color = Color.white;
        }
      }
    }
#endif
#endif
  }
}