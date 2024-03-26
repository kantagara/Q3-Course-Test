namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticCapsuleCollider3D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
    public CapsuleCollider SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FP Radius;

    [DrawIf("SourceCollider", 0)]
    public FP Height;

    [DrawIf("SourceCollider", 0)]
    public FPVector3 PositionOffset;
    
    public FPVector3 RotationOffset;

    [DrawInline, Space]
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();

    private void OnValidate() {
      UpdateFromSourceCollider();
    }

    public void UpdateFromSourceCollider() {
      if (SourceCollider == null) {
        return;
      }

      Radius           = SourceCollider.radius.ToFP();
      Height           = SourceCollider.height.ToFP();
      PositionOffset   = SourceCollider.center.ToFPVector3();
      Settings.Trigger = SourceCollider.isTrigger;
    }

    public virtual void BeforeBake() {
      UpdateFromSourceCollider();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
      if (Application.isPlaying == false) {
        UpdateFromSourceCollider();
      }

      DrawGizmos(false);
    }

    void OnDrawGizmosSelected() {
      if (Application.isPlaying == false) {
        UpdateFromSourceCollider();
      }

      DrawGizmos(true);
    }

    void DrawGizmos(bool selected) {
      
      if (!QuantumGameGizmos.ShouldDraw(GlobalGizmosSettings.DrawColliderGizmos, selected, false)) {
        return;
      }

      var t = transform;
      var scale = t.lossyScale;
      var radiusScale = Mathf.Max(scale.x, scale.z);
      var extentScale = scale.y;
      
      var matrix = Matrix4x4.TRS(
        t.TransformPoint(PositionOffset.ToUnityVector3()),
        Quaternion.Euler(t.rotation.eulerAngles + RotationOffset.ToUnityVector3()),
        Vector3.one);

      var radius = Math.Max(Radius.AsFloat, 0) * radiusScale;
      var extent = Math.Max((Height.AsFloat * extentScale / 2.0f) - radius, 0);

      GizmoUtils.DrawGizmosCapsule(matrix, radius, extent, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
#endif
  }
}