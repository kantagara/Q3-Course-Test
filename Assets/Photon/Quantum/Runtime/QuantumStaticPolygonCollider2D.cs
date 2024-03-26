namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticPolygonCollider2D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
    public PolygonCollider2D SourceCollider;

    public bool BakeAsStaticEdges2D = false;

    [DrawIf("SourceCollider", 0)]
    public FPVector2[] Vertices = new FPVector2[3] { new FPVector2(0, 2), new FPVector2(-1, 0), new FPVector2(+1, 0) };

    [DrawIf("SourceCollider", 0)]
    [Tooltip("Additional translation applied to transform position when baking")]
    public FPVector2 PositionOffset;

    [Tooltip("Additional rotation (in degrees) applied to transform rotation when baking")]
    public FP RotationOffset;

    public FP                            Height;
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();

    protected virtual bool UpdateVerticesFromSourceOnBake => true;

    public void UpdateFromSourceCollider(bool updateVertices = true) {
      if (SourceCollider == null) {
        return;
      }

      Settings.Trigger = SourceCollider.isTrigger;
      PositionOffset   = SourceCollider.offset.ToFPVector2();

      if (updateVertices == false) {
        return;
      }

      Vertices = new FPVector2[SourceCollider.points.Length];

      for (var i = 0; i < SourceCollider.points.Length; i++) {
        Vertices[i] = SourceCollider.points[i].ToFPVector2();
      }
    }

    public virtual void BeforeBake() {
      UpdateFromSourceCollider(UpdateVerticesFromSourceOnBake);
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
      if (Application.isPlaying == false) {
        UpdateFromSourceCollider(updateVertices: false);
      }

      DrawGizmos(false);
    }


    void OnDrawGizmosSelected() {
      if (Application.isPlaying == false) {
        UpdateFromSourceCollider(updateVertices: false);
      }

      DrawGizmos(true);
    }

    void DrawGizmos(bool selected) {
      
      if (!QuantumGameGizmos.ShouldDraw(GlobalGizmosSettings.DrawColliderGizmos, selected, false)) {
        return;
      }

      if (BakeAsStaticEdges2D) {
        for (var i = 0; i < Vertices.Length; i++) {
          QuantumStaticEdgeCollider2D.GetEdgeGizmosSettings(transform, PositionOffset, RotationOffset, Vertices[i], Vertices[(i + 1) % Vertices.Length], Height, out var start, out var end, out var edgeHeight);
          GizmoUtils.DrawGizmosEdge(start, end, edgeHeight, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
        }

        return;
      }

      
#if QUANTUM_XY
      var verticalScale = -transform.lossyScale.z;
#else
      var verticalScale = transform.lossyScale.y;
#endif

      var heightScaled = Height.AsFloat * verticalScale;
      var t = transform;
      var matrix = Matrix4x4.TRS(
        t.TransformPoint(PositionOffset.ToUnityVector3()),
        t.rotation * RotationOffset.FlipRotation().ToUnityQuaternionDegrees(),
        t.lossyScale);
      GizmoUtils.DrawGizmoPolygon2D(matrix, Vertices, heightScaled, selected, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
#endif
  }
}