namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public class QuantumStaticBoxCollider3D : QuantumMonoBehaviour {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
    public BoxCollider SourceCollider;

    [DrawIf("SourceCollider", 0)]
    public FPVector3 Size;

    [DrawIf("SourceCollider", 0)]
    public FPVector3 PositionOffset;

    public FPVector3                     RotationOffset;
    
    [DrawInline, Space]
    public QuantumStaticColliderSettings Settings = new QuantumStaticColliderSettings();

    private void OnValidate() {
      UpdateFromSourceCollider();
    }

    public void UpdateFromSourceCollider() {
      if (SourceCollider == null) {
        return;
      }

      Size             = SourceCollider.size.ToFPVector3();
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
      var matrix = Matrix4x4.TRS(
        t.TransformPoint(PositionOffset.ToUnityVector3()),
        Quaternion.Euler(t.rotation.eulerAngles + RotationOffset.ToUnityVector3()),
        t.lossyScale);
      GizmoUtils.DrawGizmosBox(matrix, Size.ToUnityVector3(), GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.StaticColliderColor, selected), style: GlobalGizmosSettings.StaticColliderGizmoStyle);
    }
#endif
#endif
  }
}