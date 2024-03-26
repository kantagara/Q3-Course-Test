namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticCircleCollider2D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
    [MultiTypeReference(typeof(CircleCollider2D), typeof(SphereCollider))]
    public Component SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FP Radius;

    [DrawIf("SourceCollider", 0)]
    public FPVector2 PositionOffset;

    public FP                            Height;
    
    [DrawInline, Space]
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();

    private void OnValidate() {
      UpdateFromSourceCollider();
    }

    public void UpdateFromSourceCollider() {
      if (SourceCollider == null) {
        return;
      }

      switch (SourceCollider) {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
        case SphereCollider sphere:
          Radius           = sphere.radius.ToFP();
          PositionOffset   = sphere.center.ToFPVector2();
          Settings.Trigger = sphere.isTrigger;
          break;
#endif

        case CircleCollider2D circle:
          Radius           = circle.radius.ToFP();
          PositionOffset   = circle.offset.ToFPVector2();
          Settings.Trigger = circle.isTrigger;
          break;

        default:
          SourceCollider = null;
          break;
      }
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

      var lossyScale = transform.lossyScale;
      var lossyScale2D = lossyScale.ToFPVector2();

#if QUANTUM_XY
      var heightScale = -lossyScale.z;
#else
      var heightScale = lossyScale.y;
#endif

      var heightScaled = Height.AsFloat * heightScale;
      var radiusScaled = (Radius * FPMath.Max(lossyScale2D.X, lossyScale2D.Y)).AsFloat;

      var t = transform;
      GizmoUtils.DrawGizmosCircle(
        t.TransformPoint(PositionOffset.ToUnityVector3()),
        radiusScaled,
        GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected),
        heightScaled,
        style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
#endif
  }
}