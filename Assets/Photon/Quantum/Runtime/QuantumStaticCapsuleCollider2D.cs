namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticCapsuleCollider2D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
    
    [MultiTypeReference(typeof(CapsuleCollider2D), typeof(CapsuleCollider))]
    public Component SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FPVector2 Size;

    [DrawIf("SourceCollider", 0)]
    public FPVector2 PositionOffset;
    public FP RotationOffset;
    
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
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
        case CapsuleCollider capsule:
          Size             = new FPVector2(capsule.radius.ToFP(), capsule.height.ToFP());
          PositionOffset   = capsule.center.ToFPVector2();
          Settings.Trigger = capsule.isTrigger;
          break;
#endif

        case CapsuleCollider2D capsule:
          Size             = capsule.size.ToFPVector2();
          PositionOffset   = capsule.offset.ToFPVector2();
          Settings.Trigger = capsule.isTrigger;
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

      var scale = transform.lossyScale;
#if QUANTUM_XY
      var radius = (FPMath.Clamp(Size.X,0,Size.X) / FP._2).AsFloat * scale.x;
      var height = (FPMath.Clamp(Size.Y - (Size.X / FP._2 * FP._2),FP._0, Size.Y) / FP._2).AsFloat * scale.y;
#else
      var radius = (FPMath.Clamp(Size.X,0,Size.X) / FP._2).AsFloat * scale.x;
      var height = (FPMath.Clamp(Size.Y - (Size.X / FP._2 * FP._2),FP._0, Size.Y) / FP._2).AsFloat * scale.z;
#endif
      

      GizmoUtils.DrawGizmosCapsule2D(transform.TransformPoint(PositionOffset.ToUnityVector2()), radius, height, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif

#endif
    }
}