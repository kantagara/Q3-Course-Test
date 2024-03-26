namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticBoxCollider2D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
    
    [MultiTypeReference(typeof(BoxCollider2D), typeof(BoxCollider))]
    public Component SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FPVector2 Size;

    [DrawIf("SourceCollider", 0)]
    public FPVector2 PositionOffset;

    public FP                            RotationOffset;
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
        case BoxCollider box:
          Size             = box.size.ToFPVector2();
          PositionOffset   = box.center.ToFPVector2();
          Settings.Trigger = box.isTrigger;
          break;
#endif

        case BoxCollider2D box:
          Size             = box.size.ToFPVector2();
          PositionOffset   = box.offset.ToFPVector2();
          Settings.Trigger = box.isTrigger;
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

      var size   = Size.ToUnityVector3();
      var offset = Vector3.zero;

#if QUANTUM_XY
    size.z = -Height.AsFloat;
    offset.z = size.z / 2.0f;
#else
      size.y   = Height.AsFloat;
      offset.y = size.y / 2.0f;
#endif

      var t = transform;
      var tLossyScale = t.lossyScale;

      var matrix = Matrix4x4.TRS(
        t.TransformPoint(PositionOffset.ToUnityVector3()),
        t.rotation * RotationOffset.FlipRotation().ToUnityQuaternionDegrees(),
        tLossyScale) * Matrix4x4.Translate(offset);
      GizmoUtils.DrawGizmosBox(matrix, size, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
    
#endif
  }
}