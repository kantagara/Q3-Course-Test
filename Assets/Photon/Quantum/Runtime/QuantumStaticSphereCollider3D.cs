namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticSphereCollider3D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
    public SphereCollider SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FP Radius;

    [DrawIf("SourceCollider", 0)]
    public FPVector3 PositionOffset;
    
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

      // the radius with which the sphere with be baked into the map
      var scale = transform.lossyScale;
      var radiusScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
      var radius = Radius.AsFloat * radiusScale;

      GizmoUtils.DrawGizmosSphere(transform.TransformPoint(PositionOffset.ToUnityVector3()), radius, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
#endif
  }
}