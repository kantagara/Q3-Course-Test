#if !QUANTUM_DEV

#region Assets/Photon/Quantum/Runtime/AssetTypes/AssetTypes.Partial.cs

namespace Quantum {

  using System;
  using System.Linq;
  using Photon.Deterministic;
  using Physics2D;
  using Physics3D;
  using UnityEngine;
  using UnityEngine.Serialization;
  using UnityEditor;

  public partial class QPrototypeNavMeshPathfinder {
    [LocalReference]
    [DrawIf("Prototype.InitialTargetNavMesh.Id.Value", 0)]
    public QuantumMapNavMeshUnity InitialTargetNavMeshReference;

    public override void Refresh() {
      if (InitialTargetNavMeshReference != null) {
        Prototype.InitialTargetNavMeshName = InitialTargetNavMeshReference.name;
      }
    }
  }

  public partial class QPrototypePhysicsCollider2D {
    [MultiTypeReference(new Type [] {
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
      typeof(BoxCollider2D), typeof(CircleCollider2D),
#endif
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
      typeof(BoxCollider), typeof(SphereCollider),
#endif
    })]
    public Component SourceCollider;

    public QuantumEntityPrototypeColliderLayerSource LayerSource = QuantumEntityPrototypeColliderLayerSource.GameObject;

    public override void Refresh() {
      if (TrySetShapeConfigFromSourceCollider(Prototype.ShapeConfig, transform, SourceCollider, out bool isTrigger)) {
        Prototype.IsTrigger = isTrigger;
        
        if (LayerSource != QuantumEntityPrototypeColliderLayerSource.Explicit) {
          Prototype.Layer = SourceCollider.gameObject.layer;
        }
      } else if (LayerSource == QuantumEntityPrototypeColliderLayerSource.GameObject) {
        Prototype.Layer = this.gameObject.layer;
      }
    }

    public static bool TrySetShapeConfigFromSourceCollider(Shape2DConfig config, Transform reference, Component collider, out bool isTrigger) {
      if (collider == null) {
        isTrigger = false;
        return false;
      }

      switch (collider) {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
        case BoxCollider box:
          config.ShapeType      = Shape2DType.Box;
          config.BoxExtents     = Vector3.Scale(box.size / 2, box.transform.lossyScale).ToFPVector2();
          config.PositionOffset = reference.transform.InverseTransformPoint(box.transform.TransformPoint(box.center)).ToFPVector2();
          config.RotationOffset = (Quaternion.Inverse(reference.transform.rotation) * box.transform.rotation).ToFPRotation2DDegrees();
          isTrigger             = box.isTrigger;
          break;

        case SphereCollider sphere:
          var sphereScale = sphere.transform.lossyScale;
          config.ShapeType      = Shape2DType.Circle;
          config.CircleRadius   = (Math.Max(Math.Max(Math.Abs(sphereScale.x), Math.Abs(sphereScale.y)), Math.Abs(sphereScale.z)) * sphere.radius).ToFP();
          config.PositionOffset = reference.transform.InverseTransformPoint(sphere.transform.TransformPoint(sphere.center)).ToFPVector2();
          config.RotationOffset = (Quaternion.Inverse(reference.transform.rotation) * sphere.transform.rotation).ToFPRotation2DDegrees();
          isTrigger             = sphere.isTrigger;
          break;
#endif

#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
        case BoxCollider2D box:
          config.ShapeType      = Shape2DType.Box;
          config.BoxExtents     = Vector2.Scale(box.size / 2, box.transform.lossyScale.ToFPVector2().ToUnityVector2()).ToFPVector2();
          config.PositionOffset = reference.transform.InverseTransformPoint(box.transform.TransformPoint(box.offset.ToFPVector2().ToUnityVector3())).ToFPVector2();

          var refBoxTransform2D = Transform2D.Create(reference.transform.position.ToFPVector2(), reference.transform.rotation.ToFPRotation2D());
          var boxTransform2D    = Transform2D.Create(box.transform.position.ToFPVector2(), box.transform.rotation.ToFPRotation2D());
          config.RotationOffset = (boxTransform2D.Rotation - refBoxTransform2D.Rotation) * FP.Rad2Deg;
          isTrigger             = box.isTrigger;
          break;

        case CircleCollider2D circle:
          var circleScale = circle.transform.lossyScale.ToFPVector2().ToUnityVector2();
          config.ShapeType      = Shape2DType.Circle;
          config.CircleRadius   = (Math.Max(Math.Abs(circleScale.x), Math.Abs(circleScale.y)) * circle.radius).ToFP();
          config.PositionOffset = reference.transform.InverseTransformPoint(circle.transform.TransformPoint(circle.offset.ToFPVector2().ToUnityVector3())).ToFPVector2();

          var refCircleTransform2D = Transform2D.Create(reference.transform.position.ToFPVector2(), reference.transform.rotation.ToFPRotation2D());
          var circleTransform2D    = Transform2D.Create(circle.transform.position.ToFPVector2(), circle.transform.rotation.ToFPRotation2D());
          config.RotationOffset = (circleTransform2D.Rotation - refCircleTransform2D.Rotation) * FP.Rad2Deg;
          isTrigger             = circle.isTrigger;
          break;

        case CapsuleCollider2D capsule:
#if QUANTUM_XY
          var capsuleScale = capsule.transform.lossyScale.ToFPVector2().ToUnityVector2();
#else
          var capsuleScale = new Vector2(capsule.transform.lossyScale.x,capsule.transform.lossyScale.z);
#endif
          config.ShapeType      = Shape2DType.Capsule;
          config.CapsuleSize.X   = (Math.Abs(capsuleScale.x) * capsule.size.x).ToFP();
          config.CapsuleSize.Y   = (Math.Abs(capsuleScale.y) * capsule.size.y).ToFP();
          config.PositionOffset = reference.transform.InverseTransformPoint(capsule.transform.TransformPoint(capsule.offset.ToFPVector2().ToUnityVector3())).ToFPVector2();

          var refCapsuleTransform2D = Transform2D.Create(reference.transform.position.ToFPVector2(), reference.transform.rotation.ToFPRotation2D());
          var capsuleTransform2D    = Transform2D.Create(capsule.transform.position.ToFPVector2(), capsule.transform.rotation.ToFPRotation2D());
          config.RotationOffset = (capsuleTransform2D.Rotation - refCapsuleTransform2D.Rotation) * FP.Rad2Deg;
          isTrigger             = capsule.isTrigger;
          break;
#endif

        default:
          throw new NotSupportedException($"Type {collider.GetType().FullName} not supported, needs to be one of: "
#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
            + $"{nameof(BoxCollider2D)} {nameof(CircleCollider2D)} "
#endif
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
            + $"{nameof(BoxCollider)} {nameof(SphereCollider)}"
#endif
          );
      }
      
      return true;
    }
  }

  public partial class QPrototypePhysicsCollider3D {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
    [FormerlySerializedAs("SourceCollider3D")]
    [MultiTypeReference(typeof(BoxCollider), typeof(SphereCollider))]
    public Collider SourceCollider;
    
    public QuantumEntityPrototypeColliderLayerSource LayerSource = QuantumEntityPrototypeColliderLayerSource.GameObject;

    public override void Refresh() {
      if (TrySetShapeConfigFromSourceCollider(Prototype.ShapeConfig, transform, SourceCollider, out bool isTrigger)) {
        Prototype.IsTrigger = isTrigger;
        
        if (LayerSource != QuantumEntityPrototypeColliderLayerSource.Explicit) {
          Prototype.Layer = SourceCollider.gameObject.layer;
        }
      } else if (LayerSource == QuantumEntityPrototypeColliderLayerSource.GameObject) {
        Prototype.Layer = this.gameObject.layer;
      }
    }
#endif
    
    public static bool TrySetShapeConfigFromSourceCollider(Shape3DConfig config, Transform reference, Component collider, out bool isTrigger) {
      if (collider == null) {
        isTrigger = false;
        return false;
      }

      switch (collider) {
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
        case BoxCollider box:
          config.ShapeType      = Shape3DType.Box;
          config.BoxExtents     = Vector3.Scale(box.size / 2, box.transform.lossyScale).ToFPVector3();
          config.PositionOffset = reference.transform.InverseTransformPoint(box.transform.TransformPoint(box.center)).ToFPVector3();
          config.RotationOffset = (Quaternion.Inverse(reference.transform.rotation) * box.transform.rotation).eulerAngles.ToFPVector3();
          isTrigger             = box.isTrigger;
          break;

        case SphereCollider sphere:
          var sphereScale = sphere.transform.lossyScale;
          config.ShapeType      = Shape3DType.Sphere;
          config.SphereRadius   = (Math.Max(Math.Max(Math.Abs(sphereScale.x), Math.Abs(sphereScale.y)), Math.Abs(sphereScale.z)) * sphere.radius).ToFP();
          config.PositionOffset = reference.transform.InverseTransformPoint(sphere.transform.TransformPoint(sphere.center)).ToFPVector3();
          config.RotationOffset = (Quaternion.Inverse(reference.transform.rotation) * sphere.transform.rotation).eulerAngles.ToFPVector3();
          isTrigger             = sphere.isTrigger;
          break;

        case CapsuleCollider capsule:
          var capsuleScale = capsule.transform.lossyScale;
          config.ShapeType      = Shape3DType.Capsule;
          config.CapsuleRadius   = (Math.Max(Math.Max(Math.Abs(capsuleScale.x), Math.Abs(capsuleScale.y)), Math.Abs(capsuleScale.z)) * capsule.radius).ToFP();
          config.CapsuleHeight  =  (Math.Abs(capsuleScale.y) * capsule.height).ToFP();
          config.PositionOffset = reference.transform.InverseTransformPoint(capsule.transform.TransformPoint(capsule.center)).ToFPVector3();
          config.RotationOffset = (Quaternion.Inverse(reference.transform.rotation) * capsule.transform.rotation).eulerAngles.ToFPVector3();
          isTrigger             = capsule.isTrigger;
          break;
#endif

        default:
          throw new NotSupportedException($"Type {collider.GetType().FullName} not supported, needs to be one of: "
#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D
            + $"{nameof(BoxCollider)}, {nameof(SphereCollider)}"
#endif
          );
      }
    
      return true;
    }
    
    private static string CreateTypeNotSupportedMessage(Type colliderType, params Type[] supportedTypes) {
      return $"Type {colliderType.FullName} not supported, needs to be one of {(string.Join(", ", supportedTypes.Select(x => x.Name)))}";
    }
  }

  [RequireComponent(typeof(QuantumEntityPrototype))]
  public partial class QPrototypePhysicsJoints2D {
    private void OnValidate() => AutoConfigureDistance();

    public override void Refresh() => AutoConfigureDistance();

    private void AutoConfigureDistance() {
      if (Prototype.JointConfigs == null) {
        return;
      }

      FPMathUtils.LoadLookupTables();

      foreach (var config in Prototype.JointConfigs) {
        if (config.AutoConfigureDistance && config.JointType != JointType.None) {
          var anchorPos    = transform.position.ToFPVector2() + FPVector2.Rotate(config.Anchor, transform.rotation.ToFPRotation2D());
          var connectedPos = config.ConnectedAnchor;

          if (config.ConnectedEntity != null) {
            var connectedTransform = config.ConnectedEntity.transform;
            connectedPos =  FPVector2.Rotate(connectedPos, connectedTransform.rotation.ToFPRotation2D());
            connectedPos += connectedTransform.position.ToFPVector2();
          }

          config.Distance    = FPVector2.Distance(anchorPos, connectedPos);
          config.MinDistance = config.Distance;
          config.MaxDistance = config.Distance;
        }

        if (config.MinDistance > config.MaxDistance) {
          config.MinDistance = config.MaxDistance;
        }
      }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
      DrawGizmos(selected: false);
    }

    private void OnDrawGizmosSelected() {
      DrawGizmos(selected: true);
    }

    private void DrawGizmos(bool selected) {
      
      if (!QuantumGameGizmos.ShouldDraw(GlobalGizmosSettings.DrawJointGizmos, selected)) {
        return;
      }

      var entity = GetComponent<QuantumEntityPrototype>();

      if (entity == null || Prototype.JointConfigs == null) {
        return;
      }

      FPMathUtils.LoadLookupTables();
      
      foreach (var prototype in Prototype.JointConfigs) {
        
        if (prototype.JointType == JointType.None) {
          return;
        }

        QuantumGizmosJointInfo info;

        switch (prototype.JointType) {
          case JointType.DistanceJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.DistanceJoint2D;
            info.MinDistance = prototype.MinDistance.AsFloat;
            break;

          case JointType.SpringJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.SpringJoint2D;
            info.MinDistance = prototype.Distance.AsFloat;
            break;

          case JointType.HingeJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.HingeJoint2D;
            info.MinDistance = prototype.Distance.AsFloat;
            break;

          default:
            throw new NotSupportedException($"Unsupported joint type {prototype.JointType}");
        }

        info.Selected       = selected;
        info.JointRot       = transform.rotation;
        info.RelRotRef      = Quaternion.Inverse(info.JointRot);
        info.AnchorPos      = transform.position + info.JointRot * prototype.Anchor.ToUnityVector3();
        info.MaxDistance    = prototype.MaxDistance.AsFloat;
        info.UseAngleLimits = prototype.UseAngleLimits;
        info.LowerAngle     = prototype.LowerAngle.AsFloat;
        info.UpperAngle     = prototype.UpperAngle.AsFloat;

        if (prototype.ConnectedEntity == null) {
          info.ConnectedRot = Quaternion.identity;
          info.ConnectedPos = prototype.ConnectedAnchor.ToUnityVector3();
        } else {
          info.ConnectedRot = prototype.ConnectedEntity.transform.rotation;
          info.ConnectedPos = prototype.ConnectedEntity.transform.position + info.ConnectedRot * prototype.ConnectedAnchor.ToUnityVector3();
          info.RelRotRef    = info.ConnectedRot * info.RelRotRef;
        }

#if QUANTUM_XY
        info.Axis = Vector3.back;
#else
        info.Axis = Vector3.up;
#endif

        QuantumGameGizmos.DrawGizmosJointInternal(ref info, GlobalGizmosSettings, GlobalGizmosSettings.JointGizmosStyle);
      }
    }
#endif
  }

  [RequireComponent(typeof(QuantumEntityPrototype))]
  public partial class QPrototypePhysicsJoints3D {
    public override void Refresh() {
      AutoConfigureDistance();
    }

    private void AutoConfigureDistance() {
      if (Prototype.JointConfigs == null) {
        return;
      }

      FPMathUtils.LoadLookupTables();

      foreach (var config in Prototype.JointConfigs) {
        if (config.AutoConfigureDistance && config.JointType != JointType3D.None) {
          var anchorPos    = transform.position.ToFPVector3() + transform.rotation.ToFPQuaternion() * config.Anchor;
          var connectedPos = config.ConnectedAnchor;

          if (config.ConnectedEntity != null) {
            var connectedTransform = config.ConnectedEntity.transform;
            connectedPos =  connectedTransform.rotation.ToFPQuaternion() * connectedPos;
            connectedPos += connectedTransform.position.ToFPVector3();
          }

          config.Distance    = FPVector3.Distance(anchorPos, connectedPos);
          config.MinDistance = config.Distance;
          config.MaxDistance = config.Distance;
        }

        if (config.MinDistance > config.MaxDistance) {
          config.MinDistance = config.MaxDistance;
        }
      }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
      DrawGizmos(selected: false);
    }

    private void OnDrawGizmosSelected() {
      DrawGizmos(selected: true);
    }

    private void DrawGizmos(bool selected) {
      if (!QuantumGameGizmos.ShouldDraw(GlobalGizmosSettings.DrawJointGizmos, selected)) {
        return;
      }

      var entity = GetComponent<QuantumEntityPrototype>();

      if (entity == null || Prototype.JointConfigs == null) {
        return;
      }

      FPMathUtils.LoadLookupTables();
      
      foreach (var prototype in Prototype.JointConfigs) {
        if (prototype.JointType == JointType3D.None) {
          return;
        }

        QuantumGizmosJointInfo info;

        switch (prototype.JointType) {
          case JointType3D.DistanceJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.DistanceJoint3D;
            info.MinDistance = prototype.MinDistance.AsFloat;
            break;

          case JointType3D.SpringJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.SpringJoint3D;
            info.MinDistance = prototype.Distance.AsFloat;
            break;

          case JointType3D.HingeJoint:
            info.Type        = QuantumGizmosJointInfo.GizmosJointType.HingeJoint3D;
            info.MinDistance = prototype.Distance.AsFloat;
            break;

          default:
            throw new NotSupportedException($"Unsupported joint type {prototype.JointType}");
        }

        info.Selected       = selected;
        info.JointRot       = transform.rotation;
        info.RelRotRef      = Quaternion.Inverse(info.JointRot);
        info.AnchorPos      = transform.position + info.JointRot * prototype.Anchor.ToUnityVector3();
        info.MaxDistance    = prototype.MaxDistance.AsFloat;
        info.Axis           = prototype.Axis.ToUnityVector3();
        info.UseAngleLimits = prototype.UseAngleLimits;
        info.LowerAngle     = prototype.LowerAngle.AsFloat;
        info.UpperAngle     = prototype.UpperAngle.AsFloat;

        if (prototype.ConnectedEntity  == null) {
          info.ConnectedRot = Quaternion.identity;
          info.ConnectedPos = prototype.ConnectedAnchor.ToUnityVector3();
        } else {
          info.ConnectedRot = prototype.ConnectedEntity.transform.rotation;
          info.ConnectedPos = prototype.ConnectedEntity.transform.position + info.ConnectedRot * prototype.ConnectedAnchor.ToUnityVector3();
          info.RelRotRef    = info.ConnectedRot * info.RelRotRef;
        }

        QuantumGameGizmos.DrawGizmosJointInternal(ref info, GlobalGizmosSettings, GlobalGizmosSettings.JointGizmosStyle);
      }
    }
#endif
  }

  public partial class QPrototypeTransform2D {
    public bool AutoSetPosition = true;
    public bool AutoSetRotation = true;
    
    public override void Refresh() {
      if (AutoSetPosition) {
        Prototype.Position = transform.position.ToFPVector2();
      }

      if (AutoSetRotation) {
        Prototype.Rotation = transform.rotation.ToFPRotation2DDegrees();
      }
    }
  }

  public partial class QPrototypeTransform2DVertical {
    [Tooltip("If enabled, the lossy scale of the transform in the vertical Quantum asset will be used")]
    public bool AutoSetHeight = true;

    public bool AutoSetPosition = true;
    
    public override void Refresh() {
#if QUANTUM_XY
      var verticalScale = transform.lossyScale.z.ToFP();
      var verticalPos   = -transform.position.z.ToFP();
#else
      var verticalScale = transform.lossyScale.y.ToFP();
      var verticalPos   = transform.position.y.ToFP();
#endif

      if (AutoSetPosition) {
        // based this on MapDataBaker for colliders
        Prototype.Position = verticalPos * verticalScale;
      }

      if (AutoSetHeight) {
        Prototype.Height = verticalScale;
      }
    }
  }

  public partial class QPrototypeTransform3D {
    public bool AutoSetPosition = true;
    public bool AutoSetRotation = true;
    
    public override void Refresh() {
      if (AutoSetPosition) {
        Prototype.Position = transform.position.ToFPVector3();
      }

      if (AutoSetRotation) {
        Prototype.Rotation = transform.rotation.eulerAngles.ToFPVector3();
      }
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/AssetTypes/QuantumUnityComponentPrototype.cs

namespace Quantum {
  using System;
  using UnityEditor;
  using UnityEngine;

  [RequireComponent(typeof(QuantumEntityPrototype))]
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Olive)]
  public abstract class QuantumUnityComponentPrototype
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : EntityComponentBase {}
#pragma warning restore CS0618
  
  [Obsolete("Use QuantumUnityComponentPrototype instead.")]
  [RequireComponent(typeof(QuantumEntityPrototype))]
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Olive)]
  public abstract class EntityComponentBase 
#endif
    : QuantumMonoBehaviour {
    public abstract Type ComponentType { get; }
    public abstract Type PrototypeType { get; }

    private void OnValidate() {
      Refresh();
    }

    public virtual void Refresh() {
    }

    /// <summary>
    /// </summary>
    /// <param name="converter"></param>
    /// <returns></returns>
    public abstract ComponentPrototype CreatePrototype(QuantumEntityPrototypeConverter converter);

    protected ComponentPrototype ConvertPrototype(QuantumEntityPrototypeConverter converter, ComponentPrototype prototype) {
      return prototype;
    }

    protected ComponentPrototype ConvertPrototype(QuantumEntityPrototypeConverter converter, IQuantumUnityPrototypeAdapter prototypeAdapter) {
      return (ComponentPrototype)prototypeAdapter.Convert(converter);
    }

#if UNITY_EDITOR
    [Obsolete("Move custom inspector code to EntityComponentBaseEditor subclass.", true)]
    public virtual void OnInspectorGUI(SerializedObject so, IQuantumEditorGUI QuantumEditorGUI) {
      DrawPrototype(so, QuantumEditorGUI);
      DrawNonPrototypeFields(so, QuantumEditorGUI);
    }

    [Obsolete("Move custom inspector code to EntityComponentBaseEditor subclass.", true)]
    protected void DrawPrototype(SerializedObject so, IQuantumEditorGUI QuantumEditorGUI) {
    }

    [Obsolete("Move custom inspector code to EntityComponentBaseEditor subclass.", true)]
    protected void DrawNonPrototypeFields(SerializedObject so, IQuantumEditorGUI QuantumEditorGUI) {
    }
#endif
  }

  public abstract class QuantumUnityComponentPrototype<TPrototype>
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : EntityComponentBase<TPrototype> where TPrototype : ComponentPrototype, new() { }
#pragma warning restore CS0618
  
  [Obsolete("Use QuantumUnityComponentPrototype<TPrototype> instead.")]
  public abstract class EntityComponentBase<TPrototype> 
#endif
    : QuantumUnityComponentPrototype
    where TPrototype : ComponentPrototype, new() {
    public override Type PrototypeType => typeof(TPrototype);
  }
  
  public interface IQuantumUnityPrototypeWrapperForComponent<T> where T : IComponent {
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/AssetTypes/QuantumUnityPrototypeAdapter.cs

namespace Quantum {
  using System;

  public interface IQuantumUnityPrototypeAdapter
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : IPrototypeAdapter
#pragma warning restore CS0618
  {}
  
  [Obsolete("Use " + nameof(IQuantumUnityPrototypeAdapter) + " instead.")]
  public interface IPrototypeAdapter
#endif
  {
    Type       PrototypedType { get; }
    IPrototype Convert(QuantumEntityPrototypeConverter converter);
  }

  public abstract class QuantumUnityPrototypeAdapter<PrototypeType> 
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : PrototypeAdapter<PrototypeType> where PrototypeType : IPrototype
#pragma warning restore CS0618
  {}
  
  [Obsolete("Use  QuantumUnityPrototypeAdapter instead.")]
  public abstract class PrototypeAdapter<PrototypeType>
#endif
    : IQuantumUnityPrototypeAdapter, IQuantumPrototypeConvertible<PrototypeType>
      where PrototypeType : IPrototype {
    public Type PrototypedType => typeof(PrototypeType);

    
    IPrototype
#if QUANTUM_ENABLE_MIGRATION
     IPrototypeAdapter.Convert
#else
     IQuantumUnityPrototypeAdapter.Convert
#endif
    (QuantumEntityPrototypeConverter converter) {
      return Convert(converter);
    }

    public abstract PrototypeType Convert(QuantumEntityPrototypeConverter converter);
  }
  
  public interface IQuantumPrototypeConvertible<T> {
    public T Convert(QuantumEntityPrototypeConverter converter);
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/IQuantumUnityDispatcher.cs

namespace Quantum {
  using UnityEngine;

  public interface IQuantumUnityDispatcher {
  }

  public static class IQuantumUnityDispatcherExtensions {
    public const uint CustomFlag_IsUnityObject          = 1 << (DispatcherHandlerFlags.CustomFlagsShift + 0);
    public const uint CustomFlag_OnlyIfActiveAndEnabled = 1 << (DispatcherHandlerFlags.CustomFlagsShift + 1);

    internal static DispatcherBase.ListenerStatus GetUnityListenerStatus(this IQuantumUnityDispatcher _, object listener, uint flags) {
      if (listener == null) {
        return DispatcherBase.ListenerStatus.Dead;
      }

      if ((flags & CustomFlag_IsUnityObject) == 0) {
        // not an unity object, so can't be dead
        return DispatcherBase.ListenerStatus.Active;
      }

      // needs to be Unity object now
      Debug.Assert(listener is Object);

      var asUnityObject = (Object)listener;

      if (!asUnityObject) {
        return DispatcherBase.ListenerStatus.Dead;
      }

      if ((flags & CustomFlag_OnlyIfActiveAndEnabled) != 0) {
        if (listener is Behaviour behaviour) {
          return behaviour.isActiveAndEnabled ? DispatcherBase.ListenerStatus.Active : DispatcherBase.ListenerStatus.Inactive;
        } else if (listener is GameObject gameObject) {
          return gameObject.activeInHierarchy ? DispatcherBase.ListenerStatus.Active : DispatcherBase.ListenerStatus.Inactive;
        }
      }

      return DispatcherBase.ListenerStatus.Active;
    }

    internal static DispatcherSubscription Subscribe<TDispatcher, T>(this TDispatcher dispatcher, Object listener, DispatchableHandler<T> handler, bool once = false, bool onlyIfActiveAndEnabled = false, DispatchableFilter filter = null)
      where TDispatcher : DispatcherBase, IQuantumUnityDispatcher
      where T : IDispatchable {
      return dispatcher.Subscribe(listener, handler, once, CustomFlag_IsUnityObject | (onlyIfActiveAndEnabled ? CustomFlag_OnlyIfActiveAndEnabled : 0), filter: filter);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallback.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  public enum UnityCallbackId {
    UnitySceneLoadBegin = CallbackId.UserCallbackIdStart,
    UnitySceneLoadDone,
    UnitySceneUnloadBegin,
    UnitySceneUnloadDone,
  }

  public interface ICallbackUnityScene {
    string SceneName { get; set; }
  }

  public class CallbackUnitySceneLoadBegin : QuantumGame.CallbackBase, ICallbackUnityScene {
    public new const Int32 ID = (int)UnityCallbackId.UnitySceneLoadBegin;
    public CallbackUnitySceneLoadBegin(QuantumGame game) : base(ID, game) { }
    public string SceneName { get; set; }
  }

  public class CallbackUnitySceneLoadDone : QuantumGame.CallbackBase, ICallbackUnityScene {
    public new const Int32 ID = (int)UnityCallbackId.UnitySceneLoadDone;
    public CallbackUnitySceneLoadDone(QuantumGame game) : base(ID, game) { }
    public string SceneName { get; set; }
  }

  public class CallbackUnitySceneUnloadBegin : QuantumGame.CallbackBase, ICallbackUnityScene {
    public new const Int32 ID = (int)UnityCallbackId.UnitySceneUnloadBegin;
    public CallbackUnitySceneUnloadBegin(QuantumGame game) : base(ID, game) { }
    public string SceneName { get; set; }
  }

  public class CallbackUnitySceneUnloadDone : QuantumGame.CallbackBase, ICallbackUnityScene {
    public new const Int32 ID = (int)UnityCallbackId.UnitySceneUnloadDone;
    public CallbackUnitySceneUnloadDone(QuantumGame game) : base(ID, game) { }
    public string SceneName { get; set; }
  }

  public partial class QuantumCallback : QuantumUnityStaticDispatcherAdapter<QuantumUnityCallbackDispatcher, CallbackBase> {
    private QuantumCallback() {
      throw new NotSupportedException();
    }

    [RuntimeInitializeOnLoadMethod]
    static void SetupDefaultHandlers() {
      // default callbacks handlers are initialised here; if you want them disabled, implement partial
      // method IsDefaultHandlerEnabled

      {
        bool enabled = true;
        IsDefaultHandlerEnabled(typeof(QuantumCallbackHandler_DebugDraw), ref enabled);
        if (enabled) {
          QuantumCallbackHandler_DebugDraw.Initialize();
        }
      }
      {
        bool enabled = true;
        IsDefaultHandlerEnabled(typeof(QuantumCallbackHandler_FrameDiffer), ref enabled);
        if (enabled) {
          QuantumCallbackHandler_FrameDiffer.Initialize();
        }
      }
      {
        bool enabled = true;
        IsDefaultHandlerEnabled(typeof(QuantumCallbackHandler_LegacyQuantumCallback), ref enabled);
        if (enabled) {
          QuantumCallbackHandler_LegacyQuantumCallback.Initialize();
        }
      }
      {
        bool enabled = true;
        IsDefaultHandlerEnabled(typeof(QuantumCallbackHandler_StartRecording), ref enabled);
        if (enabled) {
          QuantumCallbackHandler_StartRecording.Initialize();
        }
      }
      {
        bool enabled = true;
        IsDefaultHandlerEnabled(typeof(QuantumCallbackHandler_UnityCallbacks), ref enabled);
        if (enabled) {
          QuantumCallbackHandler_UnityCallbacks.Initialize();
        }
      }
    }

    static partial void IsDefaultHandlerEnabled(Type type, ref bool enabled);
  }

  public partial class QuantumUnityCallbackDispatcher : CallbackDispatcher, IQuantumUnityDispatcher {
    public QuantumUnityCallbackDispatcher() : base(GetCallbackTypes()) { }

    protected override ListenerStatus GetListenerStatus(object listener, uint flags) {
      return this.GetUnityListenerStatus(listener, flags);
    }

    static partial void AddUserTypes(Dictionary<Type, Int32> dict);

    private static Dictionary<Type, Int32> GetCallbackTypes() {
      var types = GetBuiltInTypes();

      // unity-side callback types
      types.Add(typeof(CallbackUnitySceneLoadBegin), CallbackUnitySceneLoadBegin.ID);
      types.Add(typeof(CallbackUnitySceneLoadDone), CallbackUnitySceneLoadDone.ID);
      types.Add(typeof(CallbackUnitySceneUnloadBegin), CallbackUnitySceneUnloadBegin.ID);
      types.Add(typeof(CallbackUnitySceneUnloadDone), CallbackUnitySceneUnloadDone.ID);


      AddUserTypes(types);
      return types;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallbackHandler_DebugDraw.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  public static class QuantumCallbackHandler_DebugDraw {
    public static IDisposable Initialize() {
      var disposables = new CompositeDisposable();

      try {
        disposables.Add(QuantumCallback.SubscribeManual((CallbackGameStarted c) => {
          DebugDraw.Clear();
        }));
        disposables.Add(QuantumCallback.SubscribeManual((CallbackGameDestroyed c) => {
          DebugDraw.Clear();
        }));
        disposables.Add(QuantumCallback.SubscribeManual((CallbackSimulateFinished c) => {
          DebugDraw.TakeAll();
        }));
      } catch {
        // if something goes wrong clean up subscriptions
        disposables.Dispose();
        throw;
      }

      return disposables;
    }

    private class CompositeDisposable : IDisposable {
      private List<IDisposable> _disposables = new List<IDisposable>();

      public void Add(IDisposable disposable) {
        _disposables.Add(disposable);
      }

      public void Dispose() {
        foreach (var disposable in _disposables) {
          try { disposable.Dispose(); } catch (Exception ex) { Debug.LogException(ex); }
        }
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallbackHandler_FrameDiffer.cs

namespace Quantum {
  using System;
  using UnityEngine;

  public static class QuantumCallbackHandler_FrameDiffer {
    public static IDisposable Initialize() {
      if (Application.isEditor)
        return null;

      return QuantumCallback.SubscribeManual((CallbackChecksumErrorFrameDump c) => {
        var gameRunner = QuantumRunner.FindRunner(c.Game);
        if (gameRunner == null) {
          Debug.LogError("Could not find runner for game");
          return;
        }

        var differ    = QuantumFrameDiffer.Show();
        var actorName = QuantumFrameDiffer.TryGetPhotonNickname(gameRunner.NetworkClient, c.ActorId);
        differ.State.AddEntry(gameRunner.Id, c.ActorId, c.FrameNumber, c.FrameDump, actorName);
      });
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallbackHandler_LegacyQuantumCallback.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public static class QuantumCallbackHandler_LegacyQuantumCallback {
    public static IDisposable Initialize() {
      var disposable = new CompositeDisposabe();

      try {
#pragma warning disable CS0618 // Type or member is obsolete
        disposable.Add(QuantumCallback.SubscribeManual((CallbackChecksumError c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnChecksumError(c.Game, c.Error, c.Frames);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackGameDestroyed c) => {
          var instancesCopy = QuantumCallbacks.Instances.ToList();
          for (Int32 i = instancesCopy.Count - 1; i >= 0; --i) {
            try {
              instancesCopy[i].OnGameDestroyed(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackGameInit c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnGameInit(c.Game, c.IsResync);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackGameStarted c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnGameStart(c.Game);
              QuantumCallbacks.Instances[i].OnGameStart(c.Game, c.IsResync);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackGameResynced c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnGameResync(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackSimulateFinished c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnSimulateFinished(c.Game, c.Frame);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackUpdateView c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnUpdateView(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackUnitySceneLoadBegin c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnUnitySceneLoadBegin(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackUnitySceneLoadDone c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnUnitySceneLoadDone(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackUnitySceneUnloadBegin c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnUnitySceneUnloadBegin(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));

        disposable.Add(QuantumCallback.SubscribeManual((CallbackUnitySceneUnloadDone c) => {
          for (Int32 i = QuantumCallbacks.Instances.Count - 1; i >= 0; --i) {
            try {
              QuantumCallbacks.Instances[i].OnUnitySceneUnloadDone(c.Game);
            } catch (Exception exn) {
              Log.Exception(exn);
            }
          }
        }));
#pragma warning restore CS0618 // Type or member is obsolete
      } catch {
        // if something goes wrong clean up subscriptions
        disposable.Dispose();
        throw;
      }

      return disposable;
    }

    private class CompositeDisposabe : IDisposable {
      private List<IDisposable> _disposables = new List<IDisposable>();

      public void Add(IDisposable disposable) {
        _disposables.Add(disposable);
      }

      public void Dispose() {
        foreach (var disposable in _disposables) {
          try { disposable.Dispose(); } catch (Exception ex) { Debug.LogException(ex); }
        }
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallbackHandler_StartRecording.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  public unsafe class QuantumCallbackHandler_StartRecording {
    public static IDisposable Initialize() {
      var disposables = new CompositeDisposable();

      try {
        disposables.Add(QuantumCallback.SubscribeManual((CallbackGameStarted c) => {
          var runner = QuantumRunner.FindRunner(c.Game);
          Debug.Assert(runner);
          Assert.Check(runner.Session.IsPaused == false);

          if (c.IsResync) {
            if (runner.RecordingFlags.HasFlag(RecordingFlags.Input)) {
              // on a resync, start recording from the next frame on
              c.Game.StartRecordingInput(c.Game.Frames.Verified.Number + 1);
            }
          } else {
            if (runner.RecordingFlags.HasFlag(RecordingFlags.Input)) {
              c.Game.StartRecordingInput();
            }

            if (runner.RecordingFlags.HasFlag(RecordingFlags.Checksums)) {
              c.Game.StartRecordingChecksums();
            }
          }
        }));
      } catch {
        // if something goes wrong clean up subscriptions
        disposables.Dispose();
        throw;
      }

      return disposables;
    }

    private class CompositeDisposable : IDisposable {
      private List<IDisposable> _disposables = new List<IDisposable>();

      public void Add(IDisposable disposable) {
        _disposables.Add(disposable);
      }

      public void Dispose() {
        foreach (var disposable in _disposables) {
          try { disposable.Dispose(); } catch (Exception ex) { Debug.LogException(ex); }
        }
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumCallbackHandler_UnityCallbacks.cs

//#define QUANTUM_UNITY_CALLBACKS_VERBOSE_LOG

namespace Quantum {
  using System;
  using System.Collections;
  using System.Diagnostics;
  using UnityEngine;
  using UnityEngine.SceneManagement;
  using Debug = UnityEngine.Debug;
  using Object = UnityEngine.Object;

  public class QuantumCallbackHandler_UnityCallbacks : IDisposable {
    private Coroutine _coroutine;
    private Map       _currentMap;
    private bool      _currentSceneNeedsCleanup;

    private readonly CallbackUnitySceneLoadBegin   _callbackUnitySceneLoadBegin;
    private readonly CallbackUnitySceneLoadDone    _callbackUnitySceneLoadDone;
    private readonly CallbackUnitySceneUnloadBegin _callbackUnitySceneUnloadBegin;
    private readonly CallbackUnitySceneUnloadDone  _callbackUnitySceneUnloadDone;

    public QuantumCallbackHandler_UnityCallbacks(QuantumGame game) {
      _callbackUnitySceneLoadBegin   = new CallbackUnitySceneLoadBegin(game);
      _callbackUnitySceneLoadDone    = new CallbackUnitySceneLoadDone(game);
      _callbackUnitySceneUnloadBegin = new CallbackUnitySceneUnloadBegin(game);
      _callbackUnitySceneUnloadDone  = new CallbackUnitySceneUnloadDone(game);
    }

    public static IDisposable Initialize() {
      return QuantumCallback.SubscribeManual((CallbackGameStarted c) => {
        var runner = QuantumRunner.FindRunner(c.Game);
        if (runner != QuantumRunner.Default) {
          // only work for the default runner
          return;
        }

        var callbacksHost = new QuantumCallbackHandler_UnityCallbacks(c.Game);

        //callbacksHost._currentMap = runner.Game.Frames?.Verified?.Map;

        // TODO: this has a bug: disposing parent sub doesn't cancel following subscriptions
        QuantumCallback.Subscribe(runner.UnityObject, (CallbackGameDestroyed cc) => callbacksHost.Dispose(), runner: runner);
        QuantumCallback.Subscribe(runner.UnityObject, (CallbackUpdateView cc) => callbacksHost.UpdateLoading(cc.Game), runner: runner);
      });
    }

    public void Dispose() {
      QuantumCallback.UnsubscribeListener(this);

      if (_coroutine != null) {
        Log.Warn("Map loading or unloading was still in progress when destroying the game");
      }

      if (_currentMap != null && _currentSceneNeedsCleanup) {
        _coroutine  = QuantumMapLoader.Instance?.StartCoroutine(UnloadScene(_currentMap.Scene));
        _currentMap = null;
      }
    }

    private static void PublishCallback<T>(T callback, string sceneName) where T : CallbackBase, ICallbackUnityScene {
      VerboseLog($"Publishing callback {typeof(T)} with {sceneName}");
      callback.SceneName = sceneName;
      QuantumCallback.Dispatcher.Publish(callback);
    }

    private IEnumerator SwitchScene(string previousSceneName, string newSceneName, bool unloadFirst) {
      if (string.IsNullOrEmpty(previousSceneName)) {
        throw new ArgumentException(nameof(previousSceneName));
      }

      if (string.IsNullOrEmpty(newSceneName)) {
        throw new ArgumentException(nameof(newSceneName));
      }

      VerboseLog($"Switching scenes from {previousSceneName} to {newSceneName} (unloadFirst: {unloadFirst})");

      try {
        LoadSceneMode loadSceneMode = LoadSceneMode.Single;

        if (unloadFirst) {
          if (SceneManager.sceneCount == 1) {
            Debug.Assert(SceneManager.GetActiveScene().name == previousSceneName);
            VerboseLog($"Need to create a temporary scene, because {previousSceneName} is the only scene loaded.");

            SceneManager.CreateScene("QuantumTemporaryEmptyScene");
            loadSceneMode = LoadSceneMode.Single;
          }

          PublishCallback(_callbackUnitySceneUnloadBegin, previousSceneName);
          yield return SceneManager.UnloadSceneAsync(previousSceneName);
          PublishCallback(_callbackUnitySceneUnloadDone, previousSceneName);
        }

        PublishCallback(_callbackUnitySceneLoadBegin, newSceneName);
        Log.Info("HEAR");
        yield return SceneManager.LoadSceneAsync(newSceneName, loadSceneMode);
        var newScene = SceneManager.GetSceneByName(newSceneName);
        if (newScene.IsValid()) {
          SceneManager.SetActiveScene(newScene);
        }

        PublishCallback(_callbackUnitySceneLoadDone, newSceneName);

        if (!unloadFirst) {
          PublishCallback(_callbackUnitySceneUnloadBegin, previousSceneName);
          yield return SceneManager.UnloadSceneAsync(previousSceneName);
          PublishCallback(_callbackUnitySceneUnloadDone, previousSceneName);
        }
      } finally {
        _coroutine = null;
      }
    }

    private IEnumerator LoadScene(string sceneName) {
      try {
        PublishCallback(_callbackUnitySceneLoadBegin, sceneName);
        Object.FindObjectOfType<Camera>().enabled = false;
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        PublishCallback(_callbackUnitySceneLoadDone, sceneName);
      } finally {
        _coroutine = null;
      }
    }

    private IEnumerator UnloadScene(string sceneName) {
      try {
        PublishCallback(_callbackUnitySceneUnloadBegin, sceneName);
        yield return SceneManager.UnloadSceneAsync(sceneName);
        PublishCallback(_callbackUnitySceneUnloadDone, sceneName);
      } finally {
        _coroutine = null;
      }
    }

    private void UpdateLoading(QuantumGame game) {
      var loadMode = game.Configurations.Simulation.AutoLoadSceneFromMap;
      if (loadMode == SimulationConfig.AutoLoadSceneFromMapMode.Disabled) {
        return;
      }

      if (_coroutine != null) {
        return;
      }

      var map = game.Frames.Verified.Map;
      if (map == _currentMap) {
        return;
      }

      bool isNewSceneLoaded = SceneManager.GetSceneByName(map.Scene).IsValid();
      if (isNewSceneLoaded) {
        VerboseLog($"Scene {map.Scene} appears to have been loaded externally.");
        _currentMap               = map;
        _currentSceneNeedsCleanup = false;
        return;
      }

      var coroHost = QuantumMapLoader.Instance;
      Debug.Assert(coroHost != null);

      string previousScene = _currentMap?.Scene ?? string.Empty;
      string newScene      = map.Scene;

      _currentMap               = map;
      _currentSceneNeedsCleanup = true;

      if (SceneManager.GetSceneByName(previousScene).IsValid()) {
        VerboseLog($"Previous scene \"{previousScene}\" was loaded, starting transition with mode {loadMode}");
        if (loadMode == SimulationConfig.AutoLoadSceneFromMapMode.LoadThenUnloadPreviousScene) {
          _coroutine  = coroHost.StartCoroutine(SwitchScene(previousScene, newScene, unloadFirst: false));
          _currentMap = map;
        } else if (loadMode == SimulationConfig.AutoLoadSceneFromMapMode.UnloadPreviousSceneThenLoad) {
          _coroutine  = coroHost.StartCoroutine(SwitchScene(previousScene, newScene, unloadFirst: true));
          _currentMap = map;
        } else {
          // legacy mode
          _coroutine  = coroHost.StartCoroutine(UnloadScene(previousScene));
          _currentMap = null;
        }
      } else {
        // simply load the scene async
        VerboseLog($"Previous scene \"{previousScene}\" was not loaded.");
        _coroutine  = coroHost.StartCoroutine(LoadScene(newScene));
        _currentMap = map;
      }
    }

    [Conditional("QUANTUM_UNITY_CALLBACKS_VERBOSE_LOG")]
    private static void VerboseLog(string msg) {
      Debug.LogFormat("QuantumUnityCallbacks: {0}", msg);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumEvent.cs

namespace Quantum {
  using System;

  public class QuantumEvent : QuantumUnityStaticDispatcherAdapter<QuantumUnityEventDispatcher, EventBase> {
    private QuantumEvent() {
      throw new NotSupportedException();
    }
  }

  public class QuantumUnityEventDispatcher : EventDispatcher, IQuantumUnityDispatcher {
    protected override ListenerStatus GetListenerStatus(object listener, uint flags) {
      return this.GetUnityListenerStatus(listener, flags);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Dispatcher/QuantumUnityStaticDispatcherAdapter.cs

namespace Quantum {
  using System;
  using Photon.Analyzer;
  using Photon.Deterministic;
  using UnityEngine;
  using Object = UnityEngine.Object;

  public abstract class QuantumUnityStaticDispatcherAdapter {
    protected sealed class Worker : QuantumMonoBehaviour {
      public DispatcherBase Dispatcher;

      private void LateUpdate() {
        if (Dispatcher == null) {
          // this may happen when scripts get reloaded in editor
          Destroy(gameObject);
        } else {
          Dispatcher.RemoveDeadListners();
        }
      }
    }
  }

  public abstract class QuantumUnityStaticDispatcherAdapter<TDispatcher, TDispatchableBase> : QuantumUnityStaticDispatcherAdapter
    where TDispatcher : DispatcherBase, IQuantumUnityDispatcher, new()
    where TDispatchableBase : IDispatchable {
    [StaticField]
    protected static Worker _worker;

    [field: StaticField]
    public static TDispatcher Dispatcher { get; } = new TDispatcher();

    [StaticFieldResetMethod]
    public static void Clear() {
      Dispatcher.Clear();
      if (_worker) {
        Object.Destroy(_worker.gameObject);
        _worker = null;
      }
    }

    public static void RemoveDeadListeners() {
      Dispatcher.RemoveDeadListners();
    }

    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, DispatchableFilter filter = null,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      if (onlyIfEntityViewBound) {
        QuantumEntityView view;
        if (listener is Component comp) {
          view = comp.GetComponentInParent<QuantumEntityView>();
        } else if (listener is GameObject go) {
          view = go.GetComponentInParent<QuantumEntityView>();
        } else {
          throw new ArgumentException($"To use {nameof(onlyIfEntityViewBound)} parameter, {nameof(listener)} needs to be a Component or a GameObject", nameof(listener));
        }

        if (view == null) {
          throw new ArgumentException($"Unable to find {nameof(EntityView)} component in {listener} or any of its parents", nameof(listener));
        }

        filter = ComposeFilters((_) => view.EntityRef.IsValid, filter);
      }

      EnsureWorkerExistsAndIsActive();
      return Dispatcher.Subscribe(listener, handler, once, onlyIfActiveAndEnabled, filter: filter);
    }

    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, DeterministicGameMode gameMode, bool exclude = false,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      return Subscribe(listener, handler, (game) => (game.Session.GameMode == gameMode) ^ exclude, once, onlyIfActiveAndEnabled, onlyIfEntityViewBound);
    }

    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, DeterministicGameMode[] gameModes, bool exclude = false,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      return Subscribe(listener, handler, (game) => (Array.IndexOf(gameModes, game.Session.GameMode) >= 0) ^ exclude, once, onlyIfActiveAndEnabled, onlyIfEntityViewBound);
    }


    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, string runnerId,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      return Subscribe(listener, handler, (game) => QuantumRunnerRegistry.Global.FindRunner(game)?.Id == runnerId, once, onlyIfActiveAndEnabled, onlyIfEntityViewBound);
    }

    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, QuantumRunner runner,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      var runnerId = runner.Id;
      return Subscribe(listener, handler, (game) => QuantumRunnerRegistry.Global.FindRunner(game)?.Id == runnerId, once, onlyIfActiveAndEnabled, onlyIfEntityViewBound);
    }

    public static DispatcherSubscription Subscribe<TDispatchable>(Object listener, DispatchableHandler<TDispatchable> handler, QuantumGame game,
      bool once = false, bool onlyIfActiveAndEnabled = false, bool onlyIfEntityViewBound = false)
      where TDispatchable : TDispatchableBase {
      return Subscribe(listener, handler, g => g == game, once, onlyIfActiveAndEnabled, onlyIfEntityViewBound);
    }

    public static IDisposable SubscribeManual<TDispatchable>(object listener, DispatchableHandler<TDispatchable> handler, DispatchableFilter filter = null, bool once = false)
      where TDispatchable : TDispatchableBase {
      return Dispatcher.SubscribeManual(listener, handler, once, filter);
    }

    public static IDisposable SubscribeManual<TDispatchable>(DispatchableHandler<TDispatchable> handler, DispatchableFilter filter = null, bool once = false)
      where TDispatchable : TDispatchableBase {
      return Dispatcher.SubscribeManual(handler, once, filter);
    }

    public static bool Unsubscribe(DispatcherSubscription subscription) {
      return Dispatcher.Unsubscribe(subscription);
    }

    public static bool UnsubscribeListener(object listener) {
      return Dispatcher.UnsubscribeListener(listener);
    }

    public static bool UnsubscribeListener<TDispatchable>(object listener) where TDispatchable : TDispatchableBase {
      return Dispatcher.UnsubscribeListener<TDispatchable>(listener);
    }

    private static void EnsureWorkerExistsAndIsActive() {
      if (_worker) {
        if (!_worker.isActiveAndEnabled)
          throw new InvalidOperationException($"{typeof(Worker)} is disabled");

        return;
      }

      var go = new GameObject(typeof(TDispatcher).Name + nameof(Worker), typeof(Worker));
      go.hideFlags = HideFlags.HideAndDontSave;
      GameObject.DontDestroyOnLoad(go);

      _worker = go.GetComponent<Worker>();
      if (!_worker)
        throw new InvalidOperationException($"Unable to create {typeof(Worker)}");

      _worker.Dispatcher = Dispatcher;
    }

    private static DispatchableFilter ComposeFilters(DispatchableFilter first, DispatchableFilter second) {
      if (first == null && second == null) {
        throw new ArgumentException($"{nameof(first)} and {nameof(second)} can't both be null");
      } else if (first == null) {
        return second;
      } else if (second == null) {
        return first;
      } else {
        return x => first(x) && second(x);
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/EditorAttributes/MultiTypeReferenceAttribute.cs

namespace Quantum {
  using System;
  using UnityEngine;

  [AttributeUsage(AttributeTargets.Field)]
  public class MultiTypeReferenceAttribute : PropertyAttribute {
    public MultiTypeReferenceAttribute(params Type[] types) {
      Types = types;
    }

    public readonly Type[] Types;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/EditorAttributes/QuantumComponentPrototypePropertyPathAttribute.cs

namespace Quantum {
  using System;

  [AttributeUsage(AttributeTargets.Class)]
  [Obsolete]
  public class QuantumComponentPrototypePropertyPathAttribute : Attribute {
    public QuantumComponentPrototypePropertyPathAttribute(string path) {
      Path = path;
    }

    public string Path { get; }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/EditorAttributes/QuantumInspectorAttribute.cs

namespace Quantum {
  using System;
  using UnityEngine;

  [AttributeUsage(AttributeTargets.Field)]
  [Obsolete]
  public class QuantumInspectorAttribute : PropertyAttribute {
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/EditorAttributes/QuantumPropertyAttributeProxyAttribute.cs

namespace Quantum {
  using UnityEngine;

  public abstract class QuantumPropertyAttributeProxyAttribute : PropertyAttribute {
    public PropertyAttribute Attribute => this;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/IQuantumEntityViewPool.cs

namespace Quantum {
  using UnityEngine;

  /// <summary>
  /// Interface to create custom implementation of the entity view pool that can be assigned to the <see cref="QuantumEntityViewUpdater.Pool"/>.
  /// </summary>
  public interface IQuantumEntityViewPool {
    /// <summary>
    /// Returns how many items are inside the pool in total.
    /// </summary>
    int PooledCount { get; }
    /// <summary>
    /// Returns how many pooled items are currently in use.
    /// </summary>
    int BorrowedCount { get; }

    /// <summary>
    /// Create a pooled game object and return the component of chose type.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <param name="prefab">Prefab to instantiate</param>
    /// <param name="activate">Call SetActive() on the game object</param>
    /// <param name="createIfEmpty">Create a new entity if there is no suitable one found in the pool</param>
    /// <returns>Component on the created prefab instance, can be null</returns>
    T Create<T>(T prefab, bool activate = true, bool createIfEmpty = true) where T : Component;

    /// <summary>
    /// Create a pooled game object.
    /// </summary>
    /// <param name="prefab">Prefab to instantiate</param>
    /// <param name="activate">Call SetActive() on the game object</param>
    /// <param name="createIfEmpty">Create a new entity if there is no suitable one found in the pool</param>
    /// <returns>An instance of the prefab</returns>
    GameObject Create(GameObject prefab, bool activate = true, bool createIfEmpty = true);

    /// <summary>
    /// Create a pooled game object and return the component of chose type.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <param name="prefab">Prefab to instantiate</param>
    /// <param name="parent">Calls SetParent(parent) on the new game object transform when set</param>
    /// <param name="activate">Call SetActive() on the game object</param>
    /// <param name="createIfEmpty">Create a new entity if there is no suitable one found in the pool</param>
    /// <returns>Component on the created prefab instance, can be null</returns>
    T Create<T>(T prefab, Transform parent, bool activate = true, bool createIfEmpty = true) where T : Component;

    /// <summary>
    /// Create a pooled game object.
    /// </summary>
    /// <param name="prefab">Prefab to instantiate</param>
    /// <param name="parent">Calls SetParent(parent) on the new game object transform when set</param>
    /// <param name="activate">Call SetActive() on the game object</param>
    /// <param name="createIfEmpty">Create a new entity if there is no suitable one found in the pool</param>
    /// <returns>An instance of the prefab</returns>
    GameObject Create(GameObject prefab, Transform parent, bool activate = true, bool createIfEmpty = true);

    /// <summary>
    /// Destroy or return the pooled game object that the component is attached to.
    /// </summary>
    /// <param name="component">Component that belongs to the pooled game object.</param>
    /// <param name="deactivate">Call SetActive(false) on the pooled game object before returning it to the pool</param>
    void Destroy(Component component, bool deactivate = true);

    /// <summary>
    /// Destroy or return the pooled game object.
    /// </summary>
    /// <param name="instance">Poole game object</param>
    /// <param name="deactivate">Call SetActive(false) on the pooled game object before returning it to the pool</param>
    void Destroy(GameObject instance, bool deactivate = true);

    /// <summary>
    /// Destroy or return the pooled game object after a delay.
    /// </summary>
    /// <param name="instance">Poole game object</param>
    /// <param name="delay">Delay in seconds to complete returning it to the pool</param>
    void Destroy(GameObject instance, float delay);

    /// <summary>
    /// Create prefab instances and fill the pool.
    /// </summary>
    /// <param name="prefab">Prefab to created pooled instances</param>
    /// <param name="desiredCount">The number of instances to create and add to the pool</param>
    void Prepare(GameObject prefab, int desiredCount);
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/Entity/IQuantumViewComponent.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// The interface that the <see cref="QuantumEntityViewUpdater"/> uses to control the view components.
  /// </summary>
  public interface IQuantumViewComponent {
    void Initialize(Dictionary<Type, IQuantumViewContext> contexts);

    void Activate(Frame frame, QuantumGame game, QuantumEntityView entityView);

    void Deactivate();

    void UpdateView();

    void LateUpdateView();

    void GameChanged(QuantumGame game);

    bool IsActive { get; }
  }

  [Obsolete("Use IQuantumViewComponent")]
  public interface IQuantumEntityViewComponent : IQuantumViewComponent {
  }
}



#endregion


#region Assets/Photon/Quantum/Runtime/Entity/IQuantumViewContext.cs

namespace Quantum {
  using System;

  /// <summary>
  /// Use this interface to create view context classes that can be used inside concrete <see cref="QuantumEntityViewComponent{T}"/>.
  /// </summary>
  public interface IQuantumViewContext {
  }

  [Obsolete("Use IQuantumViewContext")]
  public interface IQuantumEntityViewContext : IQuantumViewContext {
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityPrototypeColliderLayerSource.cs

namespace Quantum {
  /// <summary>
  /// Defines the source of the physics collider layer information.
  /// </summary>
  public enum QuantumEntityPrototypeColliderLayerSource {
    /// <summary>
    /// The layer information is retrieved from the Source Collider's GameObject (if one is provided)
    /// or this Prototype's GameObject (otherwise).
    /// </summary>
    GameObject = 0,

    /// <summary>
    /// The layer is defined explicitly from a layer enumeration.
    /// </summary>
    Explicit = 1,
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityPrototypeConverter.cs

namespace Quantum {
  using System;
  using Prototypes;
  using UnityEngine;

  public unsafe partial class QuantumEntityPrototypeConverter {
    public readonly QuantumEntityPrototype[] OrderedMapPrototypes;
    public readonly QuantumEntityPrototype   AssetPrototype;
    public readonly QuantumMapData           Map;

    public QuantumEntityPrototypeConverter(QuantumMapData map, QuantumEntityPrototype[] orderedMapPrototypes) {
      Map = map;
      OrderedMapPrototypes = orderedMapPrototypes;
      InitUser();
    }

    public QuantumEntityPrototypeConverter(QuantumEntityPrototype prototypeAsset) {
      AssetPrototype = prototypeAsset;
      InitUser();
    }

    partial void InitUser();

    public void Convert<T>(T source, out T dest) {
      dest = source;
    }

    public void Convert<T>(IQuantumPrototypeConvertible<T> source, out T dest) {
      dest = (T)source.Convert(this);
    }

    public void Convert<T>(IQuantumPrototypeConvertible<T>[] source, out T[] dest) {
      dest = new T[source.Length];
      for (int i = 0; i < source.Length; ++i) {
        dest[i] = (T)source[i].Convert(this);
      }
    }

    public void Convert(QuantumEntityPrototype prototype, out MapEntityId result) {
      if (AssetPrototype != null) {
        result = AssetPrototype == prototype ? MapEntityId.Create(0) : MapEntityId.Invalid;
      } else {
        var index = Array.IndexOf(OrderedMapPrototypes, prototype);
        result = index >= 0 ? MapEntityId.Create(index) : MapEntityId.Invalid;
      }
    }

    public void Convert(QUnityEntityPrototypeRef unityEntityPrototype, out EntityPrototypeRef result) {
      var sceneReference = unityEntityPrototype.ScenePrototype;
      if (sceneReference != null && sceneReference.gameObject.scene.IsValid()) {
        Debug.Assert(Map != null);
        Debug.Assert(Map.gameObject.scene == sceneReference.gameObject.scene);

        var index = Array.IndexOf(OrderedMapPrototypes, sceneReference);
        if (index >= 0) {
          result = EntityPrototypeRef.FromMasterAsset(Map.Asset, index);
        } else {
          result = EntityPrototypeRef.Invalid;
        }
      } else if (unityEntityPrototype.AssetPrototype.Id.IsValid) {
        result = EntityPrototypeRef.FromPrototypeAsset(unityEntityPrototype.AssetPrototype);
      } else {
        result = default;
      }
    }
    
    public void Convert<T>(QUnityComponentPrototypeRef<T> prototype, out ComponentPrototypeRef result) where T : QuantumUnityComponentPrototype {
      if (prototype == null) {
        result = default;
        return;
      }

      var entityPrototypeRefPrototype = new QUnityEntityPrototypeRef() {
        AssetPrototype = prototype.AssetPrototype,
      };

      if (prototype.ScenePrototype) {
        entityPrototypeRefPrototype.ScenePrototype = prototype.ScenePrototype.GetComponent<QuantumEntityPrototype>();
      }

      Convert(entityPrototypeRefPrototype, out EntityPrototypeRef entityPrototypeRef);

      if (entityPrototypeRef.IsValid) {
        result = ComponentPrototypeRef.FromEntityPrototypeRefAndType(entityPrototypeRef, prototype.AssetComponentType);
      } else {
        result = default;
      }
    }
    
    public void Convert<T>(QUnityComponentPrototypeRef<T>[] source, out ComponentPrototypeRef[] result) where T : QuantumUnityComponentPrototype {
      result = new ComponentPrototypeRef[source.Length];
      for (int i = 0; i < source.Length; ++i) {
        Convert(source[i], out result[i]);
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityPrototypeTransformMode.cs

namespace Quantum {
  public enum QuantumEntityPrototypeTransformMode {
    Transform2D = 0,
    Transform3D = 1,
    None = 2,
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityViewBindBehaviour.cs

namespace Quantum {
  /// <summary>
  /// The view bind behaviour controls when the view is created. For entities on the predicted or entities on the verified frame. 
  /// Because the verified frame is confirmed by the server this bind behaviour will show local entity views delayed.
  /// When using non-verifed it may happen that they get destroyed when the frame is finally confirmed by the server.
  /// </summary>
  public enum QuantumEntityViewBindBehaviour {
    /// <summary>
    /// The entity view is created during a predicted frame.
    /// </summary>
    NonVerified,
    /// <summary>
    /// The entity view is created during a verified frame.
    /// </summary>
    Verified
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityViewComponent.cs

namespace Quantum {
  /// <summary>
  /// The base class to inherit entity view components from.
  /// Entity view components can be used to add features to entity views and gain simple access to all relevant Quantum game API and the Quantum entity.
  /// </summary>
  /// <typeparam name="T">The type of the custom view context used by this view component. Can be `IQuantumEntityViewContext` if not required.</typeparam>
  public abstract class QuantumEntityViewComponent<T> : QuantumViewComponent<T> where T : IQuantumViewContext {
    /// <summary>
    /// The Game that the entity belongs to. This can change after the OnGameChanged() callback.
    /// Set before calling OnActivate(Frame).
    /// </summary>
    public override QuantumGame Game => _entityView?.Game;
    /// <summary>
    /// The Quantum EntityRef that the underlying entity view is attached to.
    /// </summary>
    public EntityRef EntityRef => _entityView.EntityRef;
    /// <summary>
    /// A reference to the parent class to access interesting game and entity data.
    /// </summary>
    public QuantumEntityView EntityView => _entityView;
  }

  /// <summary>
  /// A entity view component without context type.
  /// </summary>
  public abstract class QuantumEntityViewComponent : QuantumEntityViewComponent<IQuantumViewContext> { 
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumEntityViewFlags.cs

﻿namespace Quantum {
  using System;

  /// <summary>
  /// Additional configuration of the entity view that enables of disabled parts of the updating process.
  /// Either for performance reasons or when taking over control.
  /// </summary>
  [Flags]
  public enum QuantumEntityViewFlags {
    /// <summary>
    /// <see cref="QuantumEntityView.UpdateView(bool, bool)"/> and <see cref="QuantumEntityView.LateUpdateView()"/> are not processed and forwarded to entity view components.
    /// </summary>
    DisableUpdateView = 1 << 0,
    /// <summary>
    /// Will completely disable updating the entity view positions.
    /// </summary>
    DisableUpdatePosition = 1 << 1,
    /// <summary>
    /// Use cached transforms to improve the performance by not calling Transform properties.
    /// </summary>
    UseCachedTransform = 1 << 2,
    /// <summary>
    /// The entity game object will be named to resemble the EntityRef, set this flag to prevent naming.
    /// </summary>
    DisableEntityRefNaming = 1 << 3,
    /// <summary>
    /// Disable searching the entity view game object children for entity view components.
    /// </summary>
    DisableSearchChildrenForEntityViewComponents = 1 << 4,
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumSceneViewComponent.cs

namespace Quantum {
  using static QuantumUnityExtensions;
  
  /// <summary>
  /// The SceneViewComponent is able to attach itself to the <see cref="QuantumEntityViewUpdater"/> and received updates from it.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class QuantumSceneViewComponent<T> : QuantumViewComponent<T> where T : IQuantumViewContext {
    /// <summary>
    /// Will attach this view component to this EntityViewUpater so it receives update callbacks from there.
    /// </summary>
    public QuantumEntityViewUpdater Updater;
    /// <summary>
    /// Uses UnityEngine.Object.FindObjectOfType/FindObjectByType to find the <see cref="Updater"/>. This is very slow and not recommended.
    /// </summary>
    public bool UseFindUpdater;

    /// <summary>
    /// Unity OnEnabled, will try to attach this script to the <see cref="Updater"/>.
    /// </summary>
    public virtual void OnEnable() {
      if (Updater == null && UseFindUpdater) {
        Updater = FindFirstObjectByType<QuantumEntityViewUpdater>();
      }

      Updater?.AddViewComponent(this);
    }

    /// <summary>
    /// Unity OnDisabled, will try to detach the script from the <see cref="Updater"/>.
    /// </summary>
    public virtual void OnDisable() {
       Updater?.RemoveViewComponent(this);
    }
  }

  /// <summary>
  /// A scene view component without context.
  /// </summary>
  public abstract class QuantumSceneViewComponent : QuantumSceneViewComponent<IQuantumViewContext> {
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QuantumViewComponent.cs

namespace Quantum {
  using Quantum.Profiling;
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  /// <summary>
  /// The base class to inherit entity view components from.
  /// Entity view components can be used to add features to entity views and gain simple access to all relevant Quantum game API and the Quantum entity.
  /// </summary>
  /// <typeparam name="T">The type of the custom view context used by this view component. Can be `IQuantumEntityViewContext` if not required.</typeparam>
  public abstract class QuantumViewComponent<T> : QuantumMonoBehaviour, IQuantumViewComponent where T : IQuantumViewContext {
    /// <summary>
    /// The Game that the entity belongs to. This can change after the <see cref="OnGameChanged()"/> callback.
    /// Set before calling <see cref="OnActivate(Frame)"/>.
    /// </summary>
    public virtual QuantumGame Game => _game;
    /// <summary>
    /// The newest predicted frame.
    /// Set before calling <see cref="OnActivate(Frame)"/>.
    /// </summary>
    public Frame PredictedFrame => Game?.Frames.Predicted;
    /// <summary>
    /// The newest verified frame.
    /// Set before calling <see cref="OnActivate(Frame)"/>.
    /// </summary>
    public Frame VerifiedFrame => Game?.Frames.Verified;
    /// <summary>
    /// The newest predicted previous frame.
    /// Set before calling <see cref="OnActivate(Frame)"/>.
    /// </summary>
    public Frame PredictedPreviousFrame => Game?.Frames.PredictedPrevious;
    /// <summary>
    /// The view context of the <see cref="QuantumEntityViewUpdater"/> associated with this entity view component.
    /// </summary>
    public T ViewContext { get; private set; }
    /// <summary>
    /// Is the view component currently activated.
    /// </summary>
    public bool IsActive { get; private set; }

    protected QuantumGame _game;
    protected QuantumEntityView _entityView;

    /// <summary>
    /// Is called when the entity view is enabled for the first time.
    /// The <see cref="ViewContext"/> is already set if available.
    /// Access to <see cref="Game"/>, <see cref="VerifiedFrame"/>, <see cref="PredictedFrame"/> and <see cref="PredictedPreviousFrame"/> is not avalable yet.
    /// </summary>
    public virtual void OnInitialize() { }
    /// <summary>
    /// Is called when the entity view is activated and the entity was created.
    /// </summary>
    /// <param name="frame">The frame that the entity was created with, can be predicted or verified base on the <see cref="QuantumEntityViewBindBehaviour"></see></param>.
    public virtual void OnActivate(Frame frame) { }
    /// <summary>
    /// Is called when the view component is deactivated.
    /// </summary>
    public virtual void OnDeactivate() { }
    /// <summary>
    /// Is called from the <see cref="QuantumEntityViewUpdater"/> on a Unity update.
    /// </summary>
    public virtual void OnUpdateView() { }
    /// <summary>
    /// Is called from the <see cref="QuantumEntityViewUpdater"/> on a Unity late update.
    /// </summary>
    public virtual void OnLateUpdateView() { }
    /// <summary>
    /// Is called from the <see cref="QuantumEntityViewUpdater"/> then the observed game is changed.
    /// </summary>
    public virtual void OnGameChanged() { }

    /// <summary>
    /// Is only called internally.
    /// Sets the view context of this entity view component.
    /// </summary>
    /// <param name="contexts">All of the different contexts of the EntityViewUpdater, will select the matching type.</param>
    public void Initialize(Dictionary<Type, IQuantumViewContext> contexts) {
      if (contexts.TryGetValue(typeof(T), out var viewContext)) {
        ViewContext = (T)viewContext;
      } else if (typeof(T) != typeof(IQuantumViewContext)) {
        Debug.LogError($"Cannot find context type {typeof(T)} when initializing the entity view component {name}", this);
      }

      OnInitialize();
    }

    /// <summary>
    /// Is only called internally.
    /// Sets the entity view parent.
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="game"></param>
    /// <param name="entityView"></param>
    public void Activate(Frame frame, QuantumGame game, QuantumEntityView entityView) {
      _game = game;
      _entityView = entityView;
      IsActive = true;
      OnActivate(frame);
    }

    /// <summary>
    /// Is only called internally.
    /// </summary>
    public void Deactivate() {
      OnDeactivate();
      IsActive = false;
    }

    /// <summary>
    /// Is only called internally.
    /// </summary>
    public void UpdateView() {
      HostProfiler.Start("QuantumViewComponent.UpdateView");
      OnUpdateView();
      HostProfiler.End();
    }

    /// <summary>
    /// Is only called internally.
    /// </summary>
    public void LateUpdateView() {
      HostProfiler.Start("QuantumViewComponent.OnLateUpdateView");
      OnLateUpdateView();
      HostProfiler.End();
    }

    /// <summary>
    /// Is only called internally.
    /// </summary>
    public void GameChanged(QuantumGame game) {
      _game = game;
      OnGameChanged();
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QUnityComponentPrototypeRef.cs

namespace Quantum {
  using System;
  using UnityEngine;
  using UnityEngine.Serialization;

  // TODO: Obsolete("Temporary still in to prevent data loss")
  [Serializable]
  public class QUnityComponentPrototypeRef : QUnityComponentPrototypeRef<QuantumUnityComponentPrototype> {
  }
  
  // TODO: Obsolete("Temporary still in to prevent data loss")
  [Serializable]
  public class QUnityComponentPrototypeRef<T> : ISerializationCallbackReceiver where T : QuantumUnityComponentPrototype {
    
    public AssetRef<Quantum.EntityPrototype> AssetPrototype;
    public ComponentTypeRef        AssetComponentType;
    
    [LocalReference]
    [FormerlySerializedAs("_scenePrototype")]
    public T ScenePrototype;
    
    [Obsolete]
    [SerializeField]
    private string _componentTypeName = default;

#pragma warning disable CS0612
    void ISerializationCallbackReceiver.OnBeforeSerialize() {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
      if (AssetPrototype != default) {
        // one at a time
        ScenePrototype = default;
      }

      if (!string.IsNullOrEmpty(_componentTypeName)) {
        AssetComponentType = ComponentTypeRef.FromTypeName(_componentTypeName);
        _componentTypeName = null;
      }

      if (ScenePrototype != null) {
        AssetComponentType = default;
      }
    }
#pragma warning restore CS0612
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Entity/QUnityEntityPrototypeRef.cs

namespace Quantum {
  using System;

  [Serializable]
  public struct QUnityEntityPrototypeRef {
    [LocalReference]
    public QuantumEntityPrototype ScenePrototype;
    public Quantum.AssetRef<Quantum.EntityPrototype> AssetPrototype;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Generated/Quantum.Runtime.Generated.Core.cs

// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial  
// declarations in another file.
// </auto-generated>
namespace Quantum.Prototypes.Unity {
  [System.SerializableAttribute()]
  [Quantum.Prototypes.PrototypeAttribute(typeof(Quantum.PhysicsJoints2D))]
  public class PhysicsJoints2DPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.PhysicsJoints2DPrototype> {
    [Quantum.DynamicCollectionAttribute()]
    public Joint2DConfig[] JointConfigs = System.Array.Empty<Joint2DConfig>();

    public sealed override Quantum.Prototypes.PhysicsJoints2DPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.PhysicsJoints2DPrototype();
      result.JointConfigs = System.Array.ConvertAll(this.JointConfigs, x => x.Convert(converter));
      return result;
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.PrototypeAttribute(typeof(Quantum.Physics2D.Joint))]
  public class Joint2DConfig : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.Joint2DConfig> {
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("If the joint should be materialized with Enabled set to false, not being considered by the Physics Engine.")]
    public System.Boolean StartDisabled;
    [Quantum.DisplayNameAttribute("Type")]
    [UnityEngine.TooltipAttribute("The type of the joint, implying which constraints are applied.")]
    public Quantum.Physics2D.JointType JointType;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("A numerical tag that can be used to identify a joint or a group of joints.")]
    public System.Int32 UserTag;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("A Map Entity that the joint might be connected to.\nThe entity must have at least a Transform2D component.")]
    [Quantum.LocalReference]
    public Quantum.QuantumEntityPrototype ConnectedEntity;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("The anchor point to which the joint connects to.\nIf a Connected Entity is provided, this represents an offset in its local space. Otherwise, the connected anchor is a position in world space.")]
    public Photon.Deterministic.FPVector2 ConnectedAnchor;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("The anchor offset, in the local space of this joint entity's transform.\nThis is the point considered for the joint constraints and where the forces will be applied in the joint entity's body.")]
    public Photon.Deterministic.FPVector2 Anchor;
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The frequency in Hertz (Hz) at which the spring joint will attempt to oscillate.\nTypical values are below half the frequency of the simulation.")]
    public Photon.Deterministic.FP Frequency;
    [UnityEngine.RangeAttribute(0, 2)]
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("A dimensionless value representing the damper capacity of suppressing the spring oscillation, typically between 0 and 1.")]
    public Photon.Deterministic.FP DampingRatio;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("Automatically configure the target Distance to be the current distance between the anchor points in the scene.")]
    public System.Boolean AutoConfigureDistance;
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP Distance;
    [Quantum.DrawIfAttribute("JointType", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The minimum distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP MinDistance;
    [Quantum.DrawIfAttribute("JointType", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The maximum distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP MaxDistance;
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("If the relative angle between the joint transform and its connected anchor should be limited by the hinge joint.\nSet this checkbox to configure the lower and upper limiting angles.")]
    public System.Boolean UseAngleLimits;
    [Quantum.UnitAttribute((Quantum.Units)10)]
    [Quantum.DrawIfAttribute("UseAngleLimits", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The lower limiting angle of the allowed arc of rotation around the connected anchor, in Unit(Units.Degrees).")]
    public Photon.Deterministic.FP LowerAngle;
    [Quantum.UnitAttribute((Quantum.Units)10)]
    [Quantum.DrawIfAttribute("UseAngleLimits", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The upper limiting  angle of the allowed arc of rotation around the connected anchor, in Unit(Units.Degrees).")]
    public Photon.Deterministic.FP UpperAngle;
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("If the hinge joint uses a motor.\nSet this checkbox to configure the motor speed and max torque.")]
    public System.Boolean UseMotor;
    [Quantum.UnitAttribute((Quantum.Units)10)]
    [Quantum.DrawIfAttribute("UseMotor", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The speed at which the hinge motor will attempt to rotate, in angles per second.")]
    public Photon.Deterministic.FP MotorSpeed;
    [Quantum.DrawIfAttribute("UseMotor", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The maximum torque produced by the hinge motor in order to achieve the target motor speed.\nLeave this checkbox unchecked and the motor toque should not be limited.")]
    public Photon.Deterministic.NullableFP MaxMotorTorque;

    public sealed override Quantum.Prototypes.Joint2DConfig Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.Joint2DConfig();
      result.StartDisabled = this.StartDisabled;
      result.JointType = this.JointType;
      result.UserTag = this.UserTag;
      converter.Convert(this.ConnectedEntity, out result.ConnectedEntity);
      result.ConnectedAnchor = this.ConnectedAnchor;
      result.Anchor = this.Anchor;
      result.Frequency = this.Frequency;
      result.DampingRatio = this.DampingRatio;
      result.AutoConfigureDistance = this.AutoConfigureDistance;
      result.Distance = this.Distance;
      result.MinDistance = this.MinDistance;
      result.MaxDistance = this.MaxDistance;
      result.UseAngleLimits = this.UseAngleLimits;
      result.LowerAngle = this.LowerAngle;
      result.UpperAngle = this.UpperAngle;
      result.UseMotor = this.UseMotor;
      result.MotorSpeed = this.MotorSpeed;
      result.MaxMotorTorque = this.MaxMotorTorque;
      return result;
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.PrototypeAttribute(typeof(Quantum.PhysicsJoints3D))]
  public class PhysicsJoints3DPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.PhysicsJoints3DPrototype> {
    [Quantum.DynamicCollectionAttribute()]
    public Joint3DConfig[] JointConfigs = System.Array.Empty<Joint3DConfig>();

    public sealed override Quantum.Prototypes.PhysicsJoints3DPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.PhysicsJoints3DPrototype();
      result.JointConfigs = System.Array.ConvertAll(this.JointConfigs, x => x.Convert(converter));
      return result;
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.PrototypeAttribute(typeof(Quantum.Physics3D.Joint3D))]
  public class Joint3DConfig : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.Joint3DConfig> {
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("If the joint should be materialized with Enabled set to false, not being considered by the Physics Engine.")]
    public System.Boolean StartDisabled;
    [Quantum.DisplayNameAttribute("Type")]
    [UnityEngine.TooltipAttribute("The type of the joint, implying which constraints are applied.")]
    public Quantum.Physics3D.JointType3D JointType;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("A numerical tag that can be used to identify a joint or a group of joints.")]
    public System.Int32 UserTag;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("A Map Entity that the joint might be connected to.\nThe entity must have at least a transform component.")]
    [Quantum.LocalReference]
    public Quantum.QuantumEntityPrototype ConnectedEntity;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("The anchor point to which the joint connects to.\nIf a Connected Entity is provided, this represents an offset in its local space. Otherwise, the connected anchor is a position in world space.")]
    public Photon.Deterministic.FPVector3 ConnectedAnchor;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("The anchor offset, in the local space of this joint entity's transform.\nThis is the point considered for the joint constraints and where the forces will be applied in the joint entity's body.")]
    public Photon.Deterministic.FPVector3 Anchor;
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("Axis around which the joint rotates, defined in the local space of the entity.\nThe vector is normalized before set. If zeroed, FPVector3.Right is used instead.")]
    public Photon.Deterministic.FPVector3 Axis;
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The frequency in Hertz (Hz) at which the spring joint will attempt to oscillate.\nTypical values are below half the frequency of the simulation.")]
    public Photon.Deterministic.FP Frequency;
    [UnityEngine.RangeAttribute(0, 2)]
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("A dimensionless value representing the damper capacity of suppressing the spring oscillation, typically between 0 and 1.")]
    public Photon.Deterministic.FP DampingRatio;
    [Quantum.DrawIfAttribute("JointType", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Compare = (Quantum.CompareOperator)1, Hide = true)]
    [UnityEngine.TooltipAttribute("Automatically configure the target Distance to be the current distance between the anchor points in the scene.")]
    public System.Boolean AutoConfigureDistance;
    [Quantum.DrawIfAttribute("JointType", 2, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP Distance;
    [Quantum.DrawIfAttribute("JointType", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The minimum distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP MinDistance;
    [Quantum.DrawIfAttribute("JointType", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("AutoConfigureDistance", 0, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Mode = (Quantum.DrawIfMode)0)]
    [UnityEngine.TooltipAttribute("The maximum distance between the anchor points that the joint will attempt to maintain.")]
    public Photon.Deterministic.FP MaxDistance;
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("If the relative angle between the joint transform and its connected anchor should be limited by the hinge joint.\nSet this checkbox to configure the lower and upper limiting angles.")]
    public System.Boolean UseAngleLimits;
    [Quantum.DrawIfAttribute("UseAngleLimits", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The lower limiting angle of the allowed arc of rotation around the connected anchor, in degrees.")]
    public Photon.Deterministic.FP LowerAngle;
    [Quantum.DrawIfAttribute("UseAngleLimits", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The upper limiting  angle of the allowed arc of rotation around the connected anchor, in degrees.")]
    public Photon.Deterministic.FP UpperAngle;
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("If the hinge joint uses a motor.\nSet this checkbox to configure the motor speed and max torque.")]
    public System.Boolean UseMotor;
    [Quantum.DrawIfAttribute("UseMotor", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The speed at which the hinge motor will attempt to rotate, in angles per second.")]
    public Photon.Deterministic.FP MotorSpeed;
    [Quantum.DrawIfAttribute("UseMotor", 1, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [Quantum.DrawIfAttribute("JointType", 3, (Quantum.CompareOperator)0, (Quantum.DrawIfMode)0, Hide = true)]
    [UnityEngine.TooltipAttribute("The maximum torque produced by the hinge motor in order to achieve the target motor speed.\nLeave this checkbox unchecked and the motor toque should not be limited.")]
    public Photon.Deterministic.NullableFP MaxMotorTorque;

    public sealed override Quantum.Prototypes.Joint3DConfig Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.Joint3DConfig();
      result.StartDisabled = this.StartDisabled;
      result.JointType = this.JointType;
      result.UserTag = this.UserTag;
      converter.Convert(this.ConnectedEntity, out result.ConnectedEntity);
      result.ConnectedAnchor = this.ConnectedAnchor;
      result.Anchor = this.Anchor;
      result.Axis = this.Axis;
      result.Frequency = this.Frequency;
      result.DampingRatio = this.DampingRatio;
      result.AutoConfigureDistance = this.AutoConfigureDistance;
      result.Distance = this.Distance;
      result.MinDistance = this.MinDistance;
      result.MaxDistance = this.MaxDistance;
      result.UseAngleLimits = this.UseAngleLimits;
      result.LowerAngle = this.LowerAngle;
      result.UpperAngle = this.UpperAngle;
      result.UseMotor = this.UseMotor;
      result.MotorSpeed = this.MotorSpeed;
      result.MaxMotorTorque = this.MaxMotorTorque;
      return result;
    }
  }

}

#endregion


#region Assets/Photon/Quantum/Runtime/IQuantumAssetSource.cs

namespace Quantum {
  using System;

  public interface IQuantumAssetObjectSource {
    System.Type         AssetType { get; }
    void                Acquire(bool synchronous);
    void                Release();
    Quantum.AssetObject WaitForResult();
    bool                IsCompleted { get; }
    string              Description { get; }
    
#if UNITY_EDITOR
    Quantum.AssetObject EditorInstance { get; }
#endif
  }

  [Serializable]
  public class QuantumAssetObjectSourceStatic : QuantumAssetSourceStatic<Quantum.AssetObject>, IQuantumAssetObjectSource {
    public Type AssetType => Object.GetType();
  }
  
  [Serializable]
  public class QuantumAssetObjectSourceStaticLazy : QuantumAssetSourceStaticLazy<Quantum.AssetObject>, IQuantumAssetObjectSource {
    public Type AssetType => Object.asset.GetType();
  }
  
  [Serializable]
  public class QuantumAssetObjectSourceResource : QuantumAssetSourceResource<Quantum.AssetObject>, IQuantumAssetObjectSource {
    public SerializableType<Quantum.AssetObject> SerializableAssetType;

    public Type AssetType => SerializableAssetType;
  }
  
#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
  [Serializable]
  public class QuantumAssetObjectSourceAddressable : QuantumAssetSourceAddressable<Quantum.AssetObject>, IQuantumAssetObjectSource {
    public SerializableType<Quantum.AssetObject> SerializableAssetType;

    public Type AssetType => SerializableAssetType;
  }
#endif
}

#endregion


#region Assets/Photon/Quantum/Runtime/Legacy/IQuantumEditorGUI.cs

namespace Quantum {
  using System;
  using System.Reflection;
  using UnityEditor;
  using UnityEngine;
  
  public interface IQuantumEditorGUI {
#if UNITY_EDITOR
    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    bool Inspector(SerializedProperty prop, GUIContent label = null, string[] filters = null, bool skipRoot = true, bool drawScript = false, QuantumEditorGUIPropertyCallback callback = null);
    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren, params GUILayoutOption[] options);
    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    void MultiTypeObjectField(SerializedProperty prop, GUIContent label, Type[] types, params GUILayoutOption[] options);
#endif
  }

#if UNITY_EDITOR
  public static class IQuantumEditorGUIExtensions {
    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    public static bool Inspector(this IQuantumEditorGUI gui, SerializedObject obj, string[] filters = null, QuantumEditorGUIPropertyCallback callback = null, bool drawScript = true) {
      return gui.Inspector(obj.GetIterator(), filters: filters, skipRoot: true, callback: callback, drawScript: drawScript);
    }

    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    public static bool Inspector(this IQuantumEditorGUI gui, SerializedObject obj, string propertyPath, string[] filters = null, bool skipRoot = true, QuantumEditorGUIPropertyCallback callback = null, bool drawScript = false) {
      return gui.Inspector(obj.FindPropertyOrThrow(propertyPath), filters: filters, skipRoot: skipRoot, callback: callback, drawScript: drawScript);
    }

    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    public static bool PropertyField(this IQuantumEditorGUI gui, SerializedProperty property, params GUILayoutOption[] options) {
      return gui.PropertyField(property, null, false, options);
    }

    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    public static bool PropertyField(this IQuantumEditorGUI gui, SerializedProperty property, GUIContent label, params GUILayoutOption[] options) {
      return gui.PropertyField(property, label, false, options);
    }

    [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
    public static void MultiTypeObjectField(this IQuantumEditorGUI gui, SerializedProperty prop, GUIContent label, params Type[] types) {
      gui.MultiTypeObjectField(prop, label, types);
    }
  }

  [Obsolete("Use EditorGUILayout.PropertyField instead", true)]
  public delegate bool QuantumEditorGUIPropertyCallback(SerializedProperty property, FieldInfo field, Type fieldType);
#endif
}

#endregion


#region Assets/Photon/Quantum/Runtime/Legacy/QuantumRunner.Legacy.cs

namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Photon.Realtime;

  public partial class QuantumRunner {
    [Obsolete("Use Id instead")]
    public string name => Id;

    [Obsolete("The immediate param is not required anymore, use ShutdownAll()")]
    public static void ShutdownAll(bool immediate = false) {
      QuantumRunnerRegistry.Global.ShutdownAll();
    }

    [Obsolete("Use StartGameAsync(SessionRunner.Arguments)")]
    public static QuantumRunner StartGame(string clientId, StartParameters startParameters) {
      var arguments = startParameters.Arguments;
      arguments.ClientId = clientId;
      return StartGame(arguments);
    }

    [Obsolete("Use UnityRunnerFactory.Init()")]
    public static void Init(Boolean force = false) {
      QuantumRunnerUnityFactory.Init(force);
    }

    [Obsolete("Use QuantumRunner.IsSessionUpdateDisabled")]
    public bool OverrideUpdateSession {
      get => IsSessionUpdateDisabled;
      set => IsSessionUpdateDisabled = value;
    }

    [Obsolete("Not required anymore. Use SessionRunner.StartAsync() or SessionRunner.WaitForStartAsync() instead.")]
    public bool HasGameStartTimedOut => false;

    [Obsolete("Use SessionRunner.Arguments")]
    public struct StartParameters {
      public Arguments Arguments;

      public RuntimeConfig RuntimeConfig {
        get => (RuntimeConfig)Arguments.RuntimeConfig;
        set => Arguments.RuntimeConfig = value;
      }

      public DeterministicSessionConfig DeterministicConfig {
        get => Arguments.SessionConfig;
        set => Arguments.SessionConfig = value;
      }

      public IDeterministicReplayProvider ReplayProvider {
        get => Arguments.ReplayProvider;
        set => Arguments.ReplayProvider = value;
      }

      public DeterministicGameMode GameMode {
        get => Arguments.GameMode;
        set => Arguments.GameMode = value;
      }

      public Int32 InitialFrame {
        get => Arguments.InitialFrame;
        set => Arguments.InitialFrame = value;
      }

      public Byte[] FrameData {
        get => Arguments.FrameData;
        set => Arguments.FrameData = value;
      }

      public string RunnerId {
        get => Arguments.RunnerId;
        set => Arguments.RunnerId = value;
      }

      [Obsolete("Only accessible by the QuantumNetworkCommunicator")]
      public QuantumNetworkCommunicator.QuitBehaviour QuitBehaviour { get; set; }

      public Int32 PlayerCount {
        get => Arguments.PlayerCount;
        set => Arguments.PlayerCount = value;
      }

      [Obsolete("Has been replaced by adding players after game start by using Session.AddPlayer()")]
      public Int32 LocalPlayerCount => -1;

      [Obsolete("Use Communicator = new QuantumNetworkComminicator(RealtimeClient client)")]
      public RealtimeClient NetworkClient;

      public IResourceManager ResourceManagerOverride {
        get => Arguments.ResourceManager;
        set => Arguments.ResourceManager = value;
      }

      public InstantReplaySettings InstantReplayConfig {
        get => Arguments.InstantReplaySettings;
        set => Arguments.InstantReplaySettings = value;
      }

      public Int32 HeapExtraCount {
        get => Arguments.HeapExtraCount;
        set => Arguments.HeapExtraCount = value;
      }

      public DynamicAssetDB InitialDynamicAssets {
        get => Arguments.InitialDynamicAssets;
        set => Arguments.InitialDynamicAssets = value;
      }

      public float StartGameTimeoutInSeconds {
        get => Arguments.StartGameTimeoutInSeconds.HasValue ? Arguments.StartGameTimeoutInSeconds.Value : Arguments.DefaultStartGameTimeoutInSeconds;
        set => Arguments.StartGameTimeoutInSeconds = value;
      }

      [Obsolete("IsRejoin is not used anymore")]
      public bool IsRejoin;

      [Obsolete("The property moved to the SessionRunner")]
      public RecordingFlags RecordingFlags;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Map/MapDataBakerCallback.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;

  [AttributeUsage(AttributeTargets.Assembly)]
  public class QuantumMapBakeAssemblyAttribute : System.Attribute {
    public bool Ignore;
  }
  
  public abstract class MapDataBakerCallback {
    /// <summary>
    ///   Is called in the beginning of map baking. Both signatures are called.
    /// </summary>
    /// <param name="data">The MapData object that is currently baked.</param>
    public abstract void OnBeforeBake(QuantumMapData data);

    public virtual void OnBeforeBake(QuantumMapData data, QuantumMapDataBaker.BuildTrigger buildTrigger, QuantumMapDataBakeFlags bakeFlags) { }

    /// <summary>
    ///   Is called after map baking when colliders and prototypes have been baked and before navmesh baking.
    /// </summary>
    /// <param name="data"></param>
    public abstract void OnBake(QuantumMapData data);

    /// <summary>
    ///   Is called before any navmeshes are generated or any bake data is collected.
    /// </summary>
    /// <param name="data">The MapData object that is currently baked.</param>
    public virtual void OnBeforeBakeNavMesh(QuantumMapData data) { }

    /// <summary>
    ///   Is called during navmesh baking with the current list of bake data retreived from Unity navmeshes flagged for Quantum
    ///   navmesh baking.
    ///   Add new BakeData objects to the navMeshBakeData list.
    /// </summary>
    /// <param name="data">The MapData object that is currently baked.</param>
    /// <param name="navMeshBakeData">Current list of bake data to be baked</param>
    public virtual void OnCollectNavMeshBakeData(QuantumMapData data, List<NavMeshBakeData> navMeshBakeData) { }

    /// <summary>
    ///   Is called after navmesh baking before serializing them to assets.
    ///   Add new NavMesh objects the navmeshes list.
    /// </summary>
    /// <param name="data">The MapData object that is currently baked.</param>
    /// <param name="navmeshes">Current list of baked navmeshes to be saved to assets.</param>
    public virtual void OnCollectNavMeshes(QuantumMapData data, List<Quantum.NavMesh> navmeshes) { }

    /// <summary>
    ///   Is called after the navmesh generation has been completed.
    ///   Navmeshes assets references are stored in data.Asset.Settings.NavMeshLinks.
    /// </summary>
    /// <param name="data">The MapData object that is currently baked.</param>
    public virtual void OnBakeNavMesh(QuantumMapData data) { }
  }
  
  [Flags, Serializable]
  public enum QuantumMapDataBakeFlags {
    None,

    [Obsolete("Use BakeMapData instead")]
    Obsolete_BakeMapData = 1 << 0,
    BakeMapData       = BakeMapPrototypes | BakeMapColliders,
    BakeMapPrototypes = 1 << 5,
    BakeMapColliders  = 1 << 6,

    BakeUnityNavMesh   = 1 << 3,
    ImportUnityNavMesh = 1 << 2,
    BakeNavMesh        = 1 << 1,
    ClearUnityNavMesh  = 1 << 8,

    GenerateAssetDB = 1 << 4,
    SaveUnityAssets = 1 << 7,
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Map/MapDataBakerCallbackAttribute.cs

namespace Quantum {
  using System;

  public class MapDataBakerCallbackAttribute : Attribute {
    public int InvokeOrder { get; private set; }

    public MapDataBakerCallbackAttribute(int invokeOrder) {
      InvokeOrder = invokeOrder;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumAssetSource.Common.cs

// merged AssetSource

#region QuantumAssetSourceAddressable.cs

#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
namespace Quantum {
  using System;
#if UNITY_EDITOR
  using UnityEditor;
#endif
  using UnityEngine;
  using UnityEngine.AddressableAssets;
  using UnityEngine.ResourceManagement.AsyncOperations;
  using Object = UnityEngine.Object;

  [Serializable]
  public partial class QuantumAssetSourceAddressable<T> where T : UnityEngine.Object {
    public AssetReference Address;
    
    [NonSerialized]
    private int _acquireCount;

    public void Acquire(bool synchronous) {
      if (_acquireCount == 0) {
        LoadInternal(synchronous);
      }
      _acquireCount++;
    }

    public void Release() {
      if (_acquireCount <= 0) {
        throw new Exception("Asset is not loaded");
      }
      if (--_acquireCount == 0) {
        UnloadInternal();
      }
    }

    public bool IsCompleted => Address.IsDone;

    public T WaitForResult() {
      Debug.Assert(Address.IsValid());
      var op = Address.OperationHandle;
      if (!op.IsDone) {
        try {
          op.WaitForCompletion();
        } catch (Exception e) when (!Application.isPlaying && typeof(Exception) == e.GetType()) {
          Debug.LogError($"An exception was thrown when loading asset: {Address}; since this method " +
            $"was called from the editor, it may be due to the fact that Addressables don't have edit-time load support. Please use EditorInstance instead.");
          throw;
        }
      }
      
      if (op.OperationException != null) {
        throw new InvalidOperationException($"Failed to load asset: {Address}", op.OperationException);
      }
      
      Debug.AssertFormat(op.Result != null, "op.Result != null");
      return ValidateResult(op.Result);
    }
    
    private void LoadInternal(bool synchronous) {
      Debug.Assert(!Address.IsValid());

      var op = Address.LoadAssetAsync<UnityEngine.Object>();
      if (!op.IsValid()) {
        throw new Exception($"Failed to load asset: {Address}");
      }
      if (op.Status == AsyncOperationStatus.Failed) {
        throw new Exception($"Failed to load asset: {Address}", op.OperationException);
      }
      
      if (synchronous) {
        op.WaitForCompletion();
      }
    }

    private void UnloadInternal() {
      if (Address.IsValid()) {
        Address.ReleaseAsset();  
      }
    }

    private T ValidateResult(object result) {
      if (result == null) {
        throw new InvalidOperationException($"Failed to load asset: {Address}; asset is null");
      }
      if (typeof(T).IsSubclassOf(typeof(Component))) {
        if (result is GameObject gameObject == false) {
          throw new InvalidOperationException($"Failed to load asset: {Address}; asset is not a GameObject, but a {result.GetType()}");
        }
        
        var component = ((GameObject)result).GetComponent<T>();
        if (!component) {
          throw new InvalidOperationException($"Failed to load asset: {Address}; asset does not contain component {typeof(T)}");
        }

        return component;
      }

      if (result is T asset) {
        return asset;
      }
      
      throw new InvalidOperationException($"Failed to load asset: {Address}; asset is not of type {typeof(T)}, but {result.GetType()}");
    }
    
    public string Description => "Address: " + Address.RuntimeKey;
    
#if UNITY_EDITOR
    public T EditorInstance {
      get {
        var editorAsset = Address.editorAsset;
        if (string.IsNullOrEmpty(Address.SubObjectName)) {
          return ValidateResult(editorAsset);
        } else {
          var assetPath = AssetDatabase.GUIDToAssetPath(Address.AssetGUID);
          var assets    = AssetDatabase.LoadAllAssetsAtPath(assetPath);
          foreach (var asset in assets) {
            if (asset.name == Address.SubObjectName) {
              return ValidateResult(asset);
            }
          }

          return null;
        }
      }
    }
#endif
  }
}
#endif

#endregion


#region QuantumAssetSourceResource.cs

namespace Quantum {
  using System;
  using System.Runtime.ExceptionServices;
  using UnityEngine;
  using Object = UnityEngine.Object;
  using UnityResources = UnityEngine.Resources;

  [Serializable]
  public partial class QuantumAssetSourceResource<T> where T : UnityEngine.Object {
    [UnityResourcePath(typeof(Object))]
    public string ResourcePath;
    public string SubObjectName;

    [NonSerialized]
    private object _state;
    [NonSerialized]
    private int    _acquireCount;

    public void Acquire(bool synchronous) {
      if (_acquireCount == 0) {
        LoadInternal(synchronous);
      }
      _acquireCount++;
    }

    public void Release() {
      if (_acquireCount <= 0) {
        throw new Exception("Asset is not loaded");
      }
      if (--_acquireCount == 0) {
        UnloadInternal();
      }
    }

    public bool IsCompleted {
      get {
        if (_state == null) {
          // hasn't started
          return false;
        }
        
        if (_state is ResourceRequest asyncOp && !asyncOp.isDone) {
          // still loading, wait
          return false;
        }

        return true;
      }
    }

    public T WaitForResult() {
      Debug.Assert(_state != null);
      if (_state is ResourceRequest asyncOp) {
        if (asyncOp.isDone) {
          FinishAsyncOp(asyncOp);
        } else {
          // just load synchronously, then pass through
          _state = null;
          LoadInternal(synchronous: true);
        }
      }
      
      if (_state == null) {
        throw new InvalidOperationException($"Failed to load asset {typeof(T)}: {ResourcePath}[{SubObjectName}]. Asset is null.");  
      }

      if (_state is T asset) {
        return asset;
      }

      if (_state is ExceptionDispatchInfo exception) {
        exception.Throw();
        throw new NotSupportedException();
      }

      throw new InvalidOperationException($"Failed to load asset {typeof(T)}: {ResourcePath}, SubObjectName: {SubObjectName}");
    }

    private void FinishAsyncOp(ResourceRequest asyncOp) {
      try {
        var asset = string.IsNullOrEmpty(SubObjectName) ? asyncOp.asset : LoadNamedResource(ResourcePath, SubObjectName);
        if (asset) {
          _state = asset;
        } else {
          throw new InvalidOperationException($"Missing Resource: {ResourcePath}, SubObjectName: {SubObjectName}");
        }
      } catch (Exception ex) {
        _state = ExceptionDispatchInfo.Capture(ex);
      }
    }
    
    private static T LoadNamedResource(string resoucePath, string subObjectName) {
      var assets = UnityResources.LoadAll<T>(resoucePath);

      for (var i = 0; i < assets.Length; ++i) {
        var asset = assets[i];
        if (string.Equals(asset.name, subObjectName, StringComparison.Ordinal)) {
          return asset;
        }
      }

      return null;
    }
    
    private void LoadInternal(bool synchronous) {
      Debug.Assert(_state == null);
      try {
        if (synchronous) {
          _state = string.IsNullOrEmpty(SubObjectName) ? UnityResources.Load<T>(ResourcePath) : LoadNamedResource(ResourcePath, SubObjectName);
        } else {
          _state = UnityResources.LoadAsync<T>(ResourcePath);
        }

        if (_state == null) {
          _state = new InvalidOperationException($"Missing Resource: {ResourcePath}, SubObjectName: {SubObjectName}");
        }
      } catch (Exception ex) {
        _state = ExceptionDispatchInfo.Capture(ex);
      }
    }

    private void UnloadInternal() {
      if (_state is ResourceRequest asyncOp) {
        asyncOp.completed += op => {
          // unload stuff
        };
      } else if (_state is Object) {
        // unload stuff
      }

      _state = null;
    }
    
    public string Description => $"Resource: {ResourcePath}{(!string.IsNullOrEmpty(SubObjectName) ? $"[{SubObjectName}]" : "")}";
    
#if UNITY_EDITOR
    public T EditorInstance => string.IsNullOrEmpty(SubObjectName) ? UnityResources.Load<T>(ResourcePath) : LoadNamedResource(ResourcePath, SubObjectName);
#endif
  }
}

#endregion


#region QuantumAssetSourceStatic.cs

namespace Quantum {
  using System;
#if UNITY_EDITOR
  using UnityEditor;
#endif
  using UnityEngine;
  using UnityEngine.Serialization;
  using Object = UnityEngine.Object;

  [Serializable]
  public partial class QuantumAssetSourceStatic<T> where T : UnityEngine.Object {

    [FormerlySerializedAs("Prefab")]
    public T Object;

    [Obsolete("Use Asset instead")]
    public T Prefab {
      get => Object;
      set => Object = value;
    }
    
    public bool IsCompleted => true;

    public void Acquire(bool synchronous) {
      // do nothing
    }

    public void Release() {
      // do nothing
    }

    public T WaitForResult() {
      if (Object == null) {
        throw new InvalidOperationException("Missing static reference");
      }

      return Object;
    }
    
    public string Description {
      get {
        if (Object) {
#if UNITY_EDITOR
          if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Object, out var guid, out long fileID)) {
            return $"Static: {guid}, fileID: {fileID}";
          }
#endif
          return "Static: " + Object;
        } else {
          return "Static: (null)";
        }
      }
    }
    
#if UNITY_EDITOR
    public T EditorInstance => Object;
#endif
  }
}

#endregion


#region QuantumAssetSourceStaticLazy.cs

namespace Quantum {
  using System;
#if UNITY_EDITOR
  using UnityEditor;
#endif
  using UnityEngine;
  using UnityEngine.Serialization;
  using Object = UnityEngine.Object;

  [Serializable]
  public partial class QuantumAssetSourceStaticLazy<T> where T : UnityEngine.Object {
    
    [FormerlySerializedAs("Prefab")] 
    public LazyLoadReference<T> Object;
    
    [Obsolete("Use Object instead")]
    public LazyLoadReference<T> Prefab {
      get => Object;
      set => Object = value;
    }
    
    public bool IsCompleted => true;

    public void Acquire(bool synchronous) {
      // do nothing
    }

    public void Release() {
      // do nothing
    }

    public T WaitForResult() {
      if (Object.asset == null) {
        throw new InvalidOperationException("Missing static reference");
      }

      return Object.asset;
    }
    
    public string Description {
      get {
        if (Object.isBroken) {
          return "Static: (broken)";
        } else if (Object.isSet) {
#if UNITY_EDITOR
          if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Object.instanceID, out var guid, out long fileID)) {
            return $"Static: {guid}, fileID: {fileID}";
          }
#endif
          return "Static: " + Object.asset;
        } else {
          return "Static: (null)";
        }
      }
    }
    
#if UNITY_EDITOR
    public T EditorInstance => Object.asset;
#endif
  }
}

#endregion


#region QuantumGlobalScriptableObjectAddressAttribute.cs

namespace Quantum {
  using System;
  using UnityEngine.Scripting;

#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES 
  using UnityEngine.AddressableAssets;
  using UnityEngine.ResourceManagement.AsyncOperations;
#endif
  
  [Preserve]
  public class QuantumGlobalScriptableObjectAddressAttribute : QuantumGlobalScriptableObjectSourceAttribute {
    public QuantumGlobalScriptableObjectAddressAttribute(Type objectType, string address) : base(objectType) {
      Address = address;
    }

    public string Address { get; }
    
    public override QuantumGlobalScriptableObjectLoadResult Load(Type type) {
#if (QUANTUM_ADDRESSABLES || QUANTUM_ENABLE_ADDRESSABLES) && !QUANTUM_DISABLE_ADDRESSABLES
      Assert.Check(!string.IsNullOrEmpty(Address));
      
      var op = Addressables.LoadAssetAsync<QuantumGlobalScriptableObject>(Address);
      var instance = op.WaitForCompletion();
      if (op.Status == AsyncOperationStatus.Succeeded) {
        Assert.Check(instance);
        return new (instance, x => Addressables.Release(op));
      }
      
      Log.Trace($"Failed to load addressable at address {Address} for type {type.FullName}: {op.OperationException}");
      return default;
#else
      Log.Trace($"Addressables are not enabled. Unable to load addressable for {type.FullName}");
      return default;
#endif
    }
  }
}

#endregion


#region QuantumGlobalScriptableObjectResourceAttribute.cs

namespace Quantum {
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Reflection;
  using UnityEngine;
  using UnityEngine.Scripting;
  using Object = UnityEngine.Object;
  
  [Preserve]
  public class QuantumGlobalScriptableObjectResourceAttribute : QuantumGlobalScriptableObjectSourceAttribute {
    public QuantumGlobalScriptableObjectResourceAttribute(Type objectType, string resourcePath = "") : base(objectType) {
      ResourcePath = resourcePath;
    }
    
    public string ResourcePath { get; }
    public bool InstantiateIfLoadedInEditor { get; set; } = true;
    
    public override QuantumGlobalScriptableObjectLoadResult Load(Type type) {
      
      var attribute = type.GetCustomAttribute<QuantumGlobalScriptableObjectAttribute>();
      Assert.Check(attribute != null);

      string resourcePath;
      if (string.IsNullOrEmpty(ResourcePath)) {
        string defaultAssetPath = attribute.DefaultPath;
        var indexOfResources = defaultAssetPath.LastIndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
        if (indexOfResources < 0) {
          Log.Trace($"The default path {defaultAssetPath} does not contain a /Resources/ folder. Unable to load resource for {type.FullName}.");
          return default;
        }

        // try to load from resources, maybe?
        resourcePath = defaultAssetPath.Substring(indexOfResources + "/Resources/".Length);

        // drop the extension
        if (Path.HasExtension(resourcePath)) {
          resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
        }
      } else {
        resourcePath = ResourcePath;
      }

      var instance = UnityEngine.Resources.Load(resourcePath, type);
      if (!instance) {
        Log.Trace($"Unable to load resource at path {resourcePath} for type {type.FullName}");
        return default;
      }

      if (InstantiateIfLoadedInEditor && Application.isEditor) {
        var clone = Object.Instantiate(instance);
        return new((QuantumGlobalScriptableObject)clone, x => Object.Destroy(clone));
      } else {
        return new((QuantumGlobalScriptableObject)instance, x => UnityEngine.Resources.UnloadAsset(instance));  
      }
    }
  }
}

#endregion



#endregion


#region Assets/Photon/Quantum/Runtime/QuantumAsyncOperationExtension.cs

namespace Quantum {
  using System;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using UnityEngine;

  public static class QuantumAsyncOperationExtension {
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOperation) {
      return asyncOperation.ToTask().GetAwaiter();
    }

    public static System.Threading.Tasks.Task ToTask(this AsyncOperation asyncOperation) {
      if (asyncOperation == null) {
        return System.Threading.Tasks.Task.FromException(new Exception("Operation failed"));
      }

      var completionSource = new TaskCompletionSource<bool>();
      asyncOperation.completed += (a) => {
        completionSource.TrySetResult(true);
      };

      return (System.Threading.Tasks.Task)completionSource.Task;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumCallbacks.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Analyzer;
  using Photon.Deterministic;

  public abstract class QuantumCallbacks : QuantumMonoBehaviour {
    [StaticField]
    public static readonly List<QuantumCallbacks> Instances = new List<QuantumCallbacks>();

    protected virtual void OnEnable() {
      Instances.Add(this);
    }

    protected virtual void OnDisable() {
      Instances.Remove(this);
    }

    public virtual void OnGameInit(QuantumGame game, bool isResync) { }

    [Obsolete("Use OnGameStart(QuantumGame game, bool isResync)")]
    public virtual void OnGameStart(QuantumGame game) { }

    public virtual void OnGameStart(QuantumGame game, bool isResync)                                            { }
    public virtual void OnGameResync(QuantumGame game)                                                          { }
    public virtual void OnGameDestroyed(QuantumGame game)                                                       { }
    public virtual void OnUpdateView(QuantumGame game)                                                          { }
    public virtual void OnSimulateFinished(QuantumGame game, Frame frame)                                       { }
    public virtual void OnUnitySceneLoadBegin(QuantumGame game)                                                 { }
    public virtual void OnUnitySceneLoadDone(QuantumGame game)                                                  { }
    public virtual void OnUnitySceneUnloadBegin(QuantumGame game)                                               { }
    public virtual void OnUnitySceneUnloadDone(QuantumGame game)                                                { }
    public virtual void OnChecksumError(QuantumGame game, DeterministicTickChecksumError error, Frame[] frames) { }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumFrameDifferGUI.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Photon.Deterministic;
  using UnityEngine;

  public abstract class QuantumFrameDifferGUI {
    [Serializable]
    private class StateEntry {
      public string RunnerId;
      public int    ActorId;
      public int    FrameNumber;
      public string CompressedFrameDump;
      public string ActorName;

      [NonSerialized]
      public string FrameDump;
    }

    internal class FrameData {
      public String       String;
      public Int32        Diffs;
      public List<string> Lines = new List<string>();
      public Boolean      Initialized;
      public string       Title;
    }

    public int ReferenceActorId = 0;

    [Serializable]
    public class FrameDifferState : ISerializationCallbackReceiver {
      [SerializeField]
      private List<StateEntry> Entries = new List<StateEntry>();

      private Dictionary<string, Dictionary<int, Dictionary<int, FrameData>>> _byRunner = new Dictionary<string, Dictionary<int, Dictionary<int, FrameData>>>();

      public void Clear() {
        Entries.Clear();
        _byRunner.Clear();
      }

      public void AddEntry(string runnerId, int actorId, int frameNumber, string frameDump, string actorName = null) {
        var entry = new StateEntry() {
          RunnerId    = runnerId,
          ActorId     = actorId,
          FrameDump   = frameDump,
          FrameNumber = frameNumber,
          ActorName   = actorName
        };
        Entries.Add(entry);
        OnEntryAdded(entry);
      }

      public void OnAfterDeserialize() {
        _byRunner.Clear();
        foreach (var entry in Entries) {
          if (!string.IsNullOrEmpty(entry.CompressedFrameDump)) {
            entry.FrameDump = ByteUtils.GZipDecompressString(ByteUtils.Base64Decode(entry.CompressedFrameDump), Encoding.UTF8);
          }

          OnEntryAdded(entry);
        }
      }

      public void OnBeforeSerialize() {
        foreach (var entry in Entries) {
          if (string.IsNullOrEmpty(entry.CompressedFrameDump)) {
            entry.CompressedFrameDump = ByteUtils.Base64Encode(ByteUtils.GZipCompressString(entry.FrameDump, Encoding.UTF8));
          }
        }
      }

      private void OnEntryAdded(StateEntry entry) {
        if (!_byRunner.TryGetValue(entry.RunnerId, out var byFrame)) {
          _byRunner.Add(entry.RunnerId, byFrame = new Dictionary<int, Dictionary<int, FrameData>>());
        }

        if (!byFrame.TryGetValue(entry.FrameNumber, out var byActor)) {
          byFrame.Add(entry.FrameNumber, byActor = new Dictionary<int, FrameData>());
        }

        if (!byActor.ContainsKey(entry.ActorId)) {
          byActor.Add(entry.ActorId, new FrameData() {
            String = entry.FrameDump,
            Title  = entry.ActorName
          });
        }
      }

      public IEnumerable<string> RunnerIds => _byRunner.Keys;

      internal Dictionary<int, FrameData> GetFirstFrameDiff(string runnerId, out int frameNumber) {
        if (_byRunner.TryGetValue(runnerId, out var byFrame)) {
          frameNumber = byFrame.Keys.First();
          return byFrame[frameNumber];
        }

        frameNumber = 0;
        return null;
      }
    }

    String            _search = "";
    String            _gameId;
    Int32             _scrollOffset;
    protected Boolean _hidden;

    const float HeaderHeight = 28.0f;

    protected QuantumFrameDifferGUI(FrameDifferState state) {
      State = state;
    }

    public FrameDifferState State { get; set; }


    public virtual Boolean IsEditor {
      get { return false; }
    }

    public virtual Int32 TextLineHeight {
      get { return 16; }
    }

    public virtual GUIStyle DiffBackground {
      get { return GUI.skin.box; }
    }

    public virtual GUIStyle DiffHeader {
      get { return GUI.skin.box; }
    }

    public virtual GUIStyle DiffHeaderError {
      get { return GUI.skin.box; }
    }

    public virtual GUIStyle DiffLineOverlay {
      get { return GUI.skin.textField; }
    }

    public virtual GUIStyle MiniButton {
      get { return GUI.skin.button; }
    }

    public virtual GUIStyle TextLabel {
      get { return GUI.skin.label; }
    }

    public virtual GUIStyle BoldLabel {
      get { return GUI.skin.label; }
    }

    public virtual GUIStyle MiniButtonLeft {
      get { return GUI.skin.button; }
    }

    public virtual GUIStyle MiniButtonRight {
      get { return GUI.skin.button; }
    }

    public abstract Rect Position {
      get;
    }

    public virtual float ScrollWidth => 16.0f;

    private StringComparer Comparer => StringComparer.InvariantCulture;

    public virtual void Repaint() {
    }

    public abstract void DrawHeader();


    public void Show() {
      _hidden = false;
    }

    public void OnGUI() {
      if (Event.current.type == EventType.ScrollWheel) {
        _scrollOffset += (int)(Event.current.delta.y * 1);
        Repaint();
      }

      DrawSelection();

      if (State?.RunnerIds.Any() != true) {
        DrawNoDumps();
        return;
      }

      DrawDiff();
    }

    void DrawNoDumps() {
      GUILayout.BeginVertical();
      GUILayout.FlexibleSpace();
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.Label("No currently active diffs");
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.EndVertical();
    }

    void DrawSelection() {
      GUILayout.Space(5);
      using (new GUILayout.HorizontalScope()) {
        try {
          DrawHeader();

          if (GUILayout.Button("Clear", MiniButton, GUILayout.Height(16))) {
            State.Clear();
          }

          if (_hidden) {
            return;
          }

          GUILayout.Space(16);

          GUIStyle styleSelectedButton;
          styleSelectedButton        = new GUIStyle(MiniButton);
          styleSelectedButton.normal = styleSelectedButton.active;

          // select the first game if not selected
          if (_gameId == null || !State.RunnerIds.Contains(_gameId)) {
            _gameId = State.RunnerIds.FirstOrDefault();
          }

          foreach (var gameId in State.RunnerIds) {
            if (GUILayout.Button(gameId, gameId == _gameId ? styleSelectedButton : MiniButton, GUILayout.Height(16))) {
              _gameId = gameId;
            }
          }
        } finally {
          GUILayout.FlexibleSpace();
        }
      }

      Rect topBarRect;
      topBarRect        =  CalculateTopBarRect();
      topBarRect.x      =  (topBarRect.width - 200) - 3;
      topBarRect.width  =  200;
      topBarRect.height =  18;
      topBarRect.y      += 3;

      var currentSearch = _search;

      _search = GUI.TextField(topBarRect, _search ?? "");

      if (currentSearch != _search) {
        Search(GetSelectedFrameData().Values.FirstOrDefault(), 0, +1);
      }

      Rect prevButtonRect;
      prevButtonRect        =  topBarRect;
      prevButtonRect.height =  16;
      prevButtonRect.width  =  50;
      prevButtonRect.x      -= 102;
      prevButtonRect.y      += 1;

      if (GUI.Button(prevButtonRect, "Prev", MiniButtonLeft)) {
        Search(GetSelectedFrameData().Values.FirstOrDefault(), _scrollOffset - 1, -1);
      }

      Rect nextButtonRect;
      nextButtonRect   =  prevButtonRect;
      nextButtonRect.x += 50;

      if (GUI.Button(nextButtonRect, "Next", MiniButtonRight)) {
        Search(GetSelectedFrameData().Values.FirstOrDefault(), _scrollOffset + 1, +1);
      }
    }

    void DrawDiff() {
      if (_hidden) {
        return;
      }

      var frameData = GetSelectedFrameData();
      if (frameData == null) {
        return;
      }

      // set of lines that are currently being drawn and have diffs
      List<Rect> modified = new List<Rect>();
      List<Rect> added    = new List<Rect>();
      List<Rect> removed  = new List<Rect>();

      // main background rect
      Rect mainRect;
      mainRect = CalculateMainRect(frameData.Count);

      var scrollBarRect = Position;
      scrollBarRect.y      =  25;
      scrollBarRect.height -= 25;
      scrollBarRect.x      =  scrollBarRect.width - ScrollWidth;
      scrollBarRect.width  =  ScrollWidth;

      // header rect for drawing title/prev/next background
      Rect headerRect;
      headerRect        =  Position;
      headerRect.x      =  4;
      headerRect.y      =  HeaderHeight;
      headerRect.width  -= ScrollWidth;
      headerRect.width  /= frameData.Count;
      headerRect.width  -= 8;
      headerRect.height =  23;

      if (!frameData.TryGetValue(ReferenceActorId, out var baseFrame)) {
        ReferenceActorId = frameData.Keys.OrderBy(x => x).First();
        baseFrame        = frameData[ReferenceActorId];
      }

      var visibleRows = Mathf.FloorToInt((mainRect.height - HeaderHeight) / TextLineHeight);
      var maxScroll   = Math.Max(0, baseFrame.Lines.Count - visibleRows);

      if (visibleRows > maxScroll) {
        _scrollOffset = 0;
        GUI.VerticalScrollbar(scrollBarRect, 0, 1, 0, 1);
      } else {
        _scrollOffset = Mathf.RoundToInt(GUI.VerticalScrollbar(scrollBarRect, _scrollOffset, visibleRows, 0, baseFrame.Lines.Count));
      }

      foreach (var kvp in frameData.OrderBy(x => x.Key)) {
        GUI.Box(mainRect, "", DiffBackground);

        // draw lines
        for (Int32 i = 0; i < 100; ++i) {
          var lineIndex = _scrollOffset + i;
          if (lineIndex < kvp.Value.Lines.Count) {
            var line     = kvp.Value.Lines[lineIndex];
            var baseLine = baseFrame.Lines[lineIndex];

            var r = CalculateLineRect(i, mainRect);

            // label
            if (line == null) {
              if (baseLine != null) {
                removed.Add(r);
              }
            } else {
              GUI.Label(r, line, TextLabel);
              if (baseLine == null) {
                added.Add(r);
              } else if (!Comparer.Equals(line, baseFrame.Lines[lineIndex])) {
                modified.Add(r);
              }
            }
          }
        }

        // draw header background
        if (kvp.Value.Diffs > 0) {
          GUI.Box(headerRect, "", DiffHeaderError);
        } else {
          GUI.Box(headerRect, "", DiffHeader);
        }

        // titel label 
        Rect titleRect;
        titleRect       =  headerRect;
        titleRect.width =  headerRect.width / 2;
        titleRect.y     += 3;
        titleRect.x     += 3;

        var title = String.Format("Client {0}, Diffs: {1}", kvp.Key, kvp.Value.Diffs);
        if (string.IsNullOrEmpty(kvp.Value.Title) == false) {
          title = String.Format("{0}, Client {1}, Diffs {2}", kvp.Value.Title, kvp.Key, kvp.Value.Diffs);
        }

        GUI.Label(titleRect, title, BoldLabel);

        // disable group for prev/next buttons
        GUI.enabled = kvp.Value.Diffs > 0;

        // base button
        Rect setAsReferenceButton = titleRect;
        setAsReferenceButton.height = 15;
        setAsReferenceButton.width  = 60;
        setAsReferenceButton.x      = headerRect.x + (headerRect.width - 195);

        GUI.enabled = (ReferenceActorId != kvp.Key);
        if (GUI.Button(setAsReferenceButton, "Reference", MiniButton)) {
          ReferenceActorId = kvp.Key;
          Diff(frameData);
          GUIUtility.ExitGUI();
        }

        GUI.enabled = true;

        // next button
        Rect nextButtonRect;
        nextButtonRect   =  setAsReferenceButton;
        nextButtonRect.x += 65;

        if (GUI.Button(nextButtonRect, "Next Diff", MiniButton)) {
          SearchDiff(kvp.Value, baseFrame, _scrollOffset + 1, +1);
        }

        // prev button
        Rect prevButtonRect;
        prevButtonRect   =  nextButtonRect;
        prevButtonRect.x += 65;

        if (GUI.Button(prevButtonRect, "Prev Diff", MiniButton)) {
          SearchDiff(kvp.Value, baseFrame, _scrollOffset - 1, -1);
        }

        GUI.enabled = true;

        mainRect.x   += mainRect.width;
        headerRect.x += mainRect.width;
      }

      mainRect = CalculateMainRect(frameData.Count);


      // store gui color
      var c = GUI.color;

      // override with semi red & draw diffing lines overlays
      {
        GUI.color = new Color(1, 0.6f, 0, 0.25f);
        foreach (var diff in modified) {
          GUI.Box(diff, "", DiffLineOverlay);
        }
      }
      {
        GUI.color = new Color(0, 1, 0, 0.25f);
        foreach (var diff in added) {
          GUI.Box(diff, "", DiffLineOverlay);
        }
      }
      {
        GUI.color = new Color(1, 0, 0, 0.25f);
        foreach (var diff in removed) {
          GUI.Box(diff, "", DiffLineOverlay);
        }
      }

      // restore gui color
      GUI.color = c;
    }

    Rect CalculateLineRect(Int32 line, Rect mainRect) {
      Rect r = mainRect;
      r.height =  TextLineHeight;
      r.y      += HeaderHeight;
      r.y      += line * TextLineHeight;
      r.x      += 4;
      r.width  -= 8;

      return r;
    }

    Rect CalculateTopBarRect() {
      Rect mainRect;
      mainRect        = Position;
      mainRect.x      = 0;
      mainRect.y      = 0;
      mainRect.height = 25;
      return mainRect;
    }

    Rect CalculateMainRect(Int32 frameDataCount) {
      Rect mainRect;
      mainRect        =  Position;
      mainRect.x      =  0;
      mainRect.y      =  25;
      mainRect.width  -= ScrollWidth;
      mainRect.width  /= frameDataCount;
      mainRect.height -= mainRect.y;
      return mainRect;
    }

    void SearchDiff(FrameData frameData, FrameData baseFrame, Int32 startIndex, Int32 searchDirection) {
      for (Int32 i = startIndex; i >= 0 && i < frameData.Lines.Count; i += searchDirection) {
        if (!Comparer.Equals(baseFrame.Lines[i], frameData.Lines[i])) {
          _scrollOffset = i;
          break;
        }
      }
    }

    void Search(FrameData frameData, Int32 startIndex, Int32 searchDirection) {
      var term = _search ?? "";
      if (term.Length > 0) {
        for (Int32 i = startIndex; i >= 0 && i < frameData.Lines.Count; i += searchDirection) {
          if (frameData.Lines[i].Contains(term)) {
            _scrollOffset = i;
            break;
          }
        }
      }
    }


    Dictionary<Int32, FrameData> GetSelectedFrameData() {
      var frames = State.GetFirstFrameDiff(_gameId, out int frameNumber);
      if (frames == null)
        return null;

      foreach (var frame in frames.Values) {
        if (!frame.Initialized) {
          Diff(frames);
          break;
        }
      }

      return frames;
    }

    void Diff(Dictionary<Int32, FrameData> frames) {
      foreach (var frame in frames.Values) {
        frame.Initialized = false;
        frame.Diffs       = 0;
        frame.Lines.Clear();
      }

      // diff all lines
      if (!frames.TryGetValue(ReferenceActorId, out var baseFrame)) {
        ReferenceActorId = frames.Keys.OrderBy(x => x).First();
        baseFrame        = frames[ReferenceActorId];
      }

      var otherFrames = frames.Where(x => x.Key != ReferenceActorId).OrderBy(x => x.Key).Select(x => x.Value).ToArray();

      var splits    = new[] { "\r\n", "\r", "\n" };
      var baseLines = baseFrame.String.Split(splits, StringSplitOptions.None);

      var diffs = new List<ValueTuple<string, string>>[otherFrames.Length];

      // compute lcs
      Parallel.For(0, otherFrames.Length, () => new LongestCommonSequence(), (frameIndex, state, lcs) => {
        var frameLines = otherFrames[frameIndex].String.Split(splits, StringSplitOptions.None);
        otherFrames[frameIndex].Diffs = 0;

        var chunks = new List<LongestCommonSequence.DiffChunk>();
        lcs.Diff(baseLines, frameLines, Comparer, chunks);

        var diff = new List<ValueTuple<string, string>>();

        int baseLineIndex  = 0;
        int frameLineIndex = 0;

        foreach (var chunk in chunks) {
          int sameCount = chunk.StartA - baseLineIndex;
          Debug.Assert(chunk.StartB - frameLineIndex == sameCount);

          int modifiedCount = Mathf.Min(chunk.AddedA, chunk.AddedB);
          otherFrames[frameIndex].Diffs += Mathf.Max(chunk.AddedA, chunk.AddedB);

          for (int i = 0; i < sameCount + modifiedCount; ++i) {
            diff.Add((baseLines[baseLineIndex++], frameLines[frameLineIndex++]));
          }

          for (int i = 0; i < chunk.AddedA - modifiedCount; ++i) {
            diff.Add((baseLines[baseLineIndex++], default));
          }

          for (int i = 0; i < chunk.AddedB - modifiedCount; ++i) {
            diff.Add((default, frameLines[frameLineIndex++]));
          }
        }

        Debug.Assert(frameLines.Length - frameLineIndex == baseLines.Length - baseLineIndex);
        for (int i = 0; i < frameLines.Length - frameLineIndex; ++i) {
          diff.Add((baseLines[baseLineIndex + i], frameLines[frameLineIndex + i]));
        }

        diffs[frameIndex] = diff;
        return lcs;
      }, lcs => { });

      int[] prevIndices  = new int[otherFrames.Length];
      int[] paddingCount = new int[otherFrames.Length];

      // reconstruct
      for (int baseIndex = 0; baseIndex < baseLines.Length; ++baseIndex) {
        var baseLine = baseLines[baseIndex];
        for (int diffIndex = 0; diffIndex < diffs.Length; ++diffIndex) {
          var diff = diffs[diffIndex];

          int newLines  = 0;
          int prevIndex = prevIndices[diffIndex];

          for (int i = prevIndex; i < diff.Count; ++i, ++newLines) {
            if (diff[i].Item1 == null) {
              // skip
            } else {
              Debug.Assert(ReferenceEquals(diff[i].Item1, baseLine));
              break;
            }
          }

          paddingCount[diffIndex] = newLines;
        }

        // this is how many lines need to be insert
        int maxPadding = otherFrames.Length > 0 ? paddingCount.Max() : 0;
        Debug.Assert(maxPadding >= 0);

        for (int i = 0; i < maxPadding; ++i) {
          baseFrame.Lines.Add(null);
        }

        baseFrame.Lines.Add(baseLine);

        for (int diffIndex = 0; diffIndex < diffs.Length; ++diffIndex) {
          var diff    = diffs[diffIndex];
          var padding = paddingCount[diffIndex];

          for (int i = 0; i < padding; ++i) {
            otherFrames[diffIndex].Lines.Add(diff[prevIndices[diffIndex] + i].Item2);
          }

          for (int i = 0; i < maxPadding - padding; ++i) {
            otherFrames[diffIndex].Lines.Add(null);
          }

          otherFrames[diffIndex].Lines.Add(diff[prevIndices[diffIndex] + padding].Item2);

          prevIndices[diffIndex] += padding + 1;
        }
      }

      baseFrame.Initialized = true;
      foreach (var frame in otherFrames) {
        frame.Initialized = true;
      }
    }

    private class LongestCommonSequence {
      public struct DiffChunk {
        public int StartA;
        public int StartB;
        public int AddedA;
        public int AddedB;

        public override string ToString() {
          return $"{StartA}, {StartB}, {AddedA}, {AddedB}";
        }
      }


      private       ushort[,] m_c;
      private const int       MaxSlice = 5000;

      public LongestCommonSequence() {
      }

      public void Diff<T>(T[] x, T[] y, IEqualityComparer<T> comparer, List<DiffChunk> result) {
        //
        int lowerX = 0;
        int lowerY = 0;
        int upperX = x.Length;
        int upperY = y.Length;

        while (lowerX < upperX && lowerY < upperY && comparer.Equals(x[lowerX], y[lowerY])) {
          ++lowerX;
          ++lowerY;
        }

        while (lowerX < upperX && lowerY < upperY && comparer.Equals(x[upperX - 1], y[upperY - 1])) {
          // pending add
          --upperX;
          --upperY;
        }

        int x1;
        int y1;

        // this is not strictly correct, but LCS is memory hungry; let's just split into slices
        for (int x0 = lowerX, y0 = lowerY; x0 < upperX || y0 < upperY; x0 = x1, y0 = y1) {
          x1 = Mathf.Min(upperX, x0 + MaxSlice);
          y1 = Mathf.Min(upperY, y0 + MaxSlice);

          if (x0 == x1) {
            result.Add(new DiffChunk() {
              StartA = x0,
              StartB = y0,
              AddedB = y1 - y0
            });
          } else if (y0 == y1) {
            result.Add(new DiffChunk() {
              StartA = x0,
              StartB = y0,
              AddedA = x1 - x0
            });
          } else {
            var sx = new ArraySegment<T>(x, x0, x1 - x0);
            var sy = new ArraySegment<T>(y, y0, y1 - y0);

            AllocateMatrix(x1 - x0, y1 - y0);
            FillMatrix(m_c, sx, sy, comparer);
            FillDiff(m_c, sx, sy, comparer, result);
            var chunks = new List<DiffChunk>();
            FillDiff(m_c, sx, sy, comparer, chunks);
          }
        }
      }

      private void AllocateMatrix(int x, int y) {
        if (m_c == null) {
          m_c = new ushort[x + 1, y + 1];
        } else {
          int len0 = Math.Max(m_c.GetLength(0), x + 1);
          int len1 = Math.Max(m_c.GetLength(1), y + 1);
          if (len0 > m_c.GetLength(0) || len1 > m_c.GetLength(1)) {
            m_c = new ushort[len0, len1];
          }
        }
      }

      private static void FillMatrix<T>(ushort[,] c, ArraySegment<T> x, ArraySegment<T> y, IEqualityComparer<T> comparer) {
        int xcount  = x.Count;
        int ycount  = y.Count;
        int xoffset = x.Offset - 1;
        int yoffset = y.Offset - 1;

        for (int i = 1; i <= xcount; i++) {
          c[i, 0] = 0;
        }

        for (int i = 1; i <= ycount; i++) {
          c[0, i] = 0;
        }

        for (int i = 1; i <= xcount; i++) {
          for (int j = 1; j <= ycount; j++) {
            if (comparer.Equals(x.Array[i + xoffset], y.Array[j + yoffset])) {
              c[i, j] = (ushort)(c[i - 1, j - 1] + 1);
            } else {
              c[i, j] = Math.Max(c[i - 1, j], c[i, j - 1]);
            }
          }
        }
      }

      private static void FillDiff<T>(ushort[,] c, ArraySegment<T> x, ArraySegment<T> y, IEqualityComparer<T> comparer, List<DiffChunk> result) {
        int startIndex = result.Count;
        int i          = x.Count - 1;
        int j          = y.Count - 1;

        var chunk = new DiffChunk();
        chunk.StartA = x.Offset + x.Count;
        chunk.StartB = y.Offset + y.Count;

        while (i >= 0 || j >= 0) {
          if (i >= 0 && j >= 0 && comparer.Equals(x.Array[x.Offset + i], y.Array[y.Offset + j])) {
            if (chunk.AddedA != 0 || chunk.AddedB != 0) {
              result.Add(chunk);
              chunk = default;
            }

            chunk.StartA = i + x.Offset;
            chunk.StartB = j + y.Offset;
            --i;
            --j;
          } else if (j >= 0 && (i < 0 || c[i + 1, j] >= c[i, j + 1])) {
            Debug.Assert(chunk.AddedA == 0);
            chunk.AddedB++;
            chunk.StartB = j + y.Offset;
            --j;
          } else if (i >= 0 && (j < 0 || c[i + 1, j] < c[i, j + 1])) {
            chunk.AddedA++;
            chunk.StartA = i + x.Offset;
            --i;
          } else {
            throw new NotSupportedException();
          }
        }

        if (chunk.AddedA != 0 || chunk.AddedB != 0) {
          result.Add(chunk);
        }

        result.Reverse(startIndex, result.Count - startIndex);
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumGameGizmos.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Photon.Deterministic;
  using Physics3D;
  using UnityEditor;
  using UnityEngine;
  using Joint = Physics2D.Joint;
  using JointType = Physics2D.JointType; 
 
#if UNITY_EDITOR
  public static class QuantumGameGizmos {
    private static Color Desaturate(Color c, float t) {
      return Color.Lerp(new Color(c.grayscale, c.grayscale, c.grayscale), c, t);
    }

    public static bool ShouldDraw(QuantumGameGizmosMode mode, bool selected, bool hasStateDrawer = true) {
      if (Application.isPlaying) {
        if (hasStateDrawer) {
          // state drawer will take over
          return false;
        } else if ((mode & QuantumGameGizmosMode.OnApplicationPlaying) == default) {
          // needs to be set in order to get to OnDraw/OnSelected
          return false;
        }
      }

      if (selected) {
        return (mode & QuantumGameGizmosMode.OnSelected) == QuantumGameGizmosMode.OnSelected;
      } else {
        return (mode & QuantumGameGizmosMode.OnDraw) == QuantumGameGizmosMode.OnDraw;
      }
    }

    public static unsafe void OnDrawGizmos(QuantumGame game, QuantumGameGizmosSettings gizmosSettings) {

      var frame = game.Frames.Predicted;

      if (frame != null) {
        #region Components

        if ((gizmosSettings.DrawColliderGizmos & QuantumGameGizmosMode.OnApplicationPlaying) != 0) {
          // ################## Components: PhysicsCollider2D ##################

          foreach (var (handle, collider) in frame.GetComponentIterator<PhysicsCollider2D>()) {
            DrawCollider2DGizmo(frame, handle, &collider, GetCollider2DColor(frame, handle, gizmosSettings), gizmosSettings.ColliderGizmosStyle);
          }

          // ################## Components: PhysicsCollider3D ##################

          foreach (var (handle, collider) in frame.GetComponentIterator<PhysicsCollider3D>()) {
            DrawCollider3DGizmo(frame, handle, &collider, GetCollider3DColor(frame, handle, gizmosSettings), gizmosSettings.ColliderGizmosStyle);
          }

          // ################## Components: CharacterController2D ##################

          foreach (var (entity, cc) in frame.GetComponentIterator<CharacterController2D>()) {
            if (frame.Unsafe.TryGetPointer(entity, out Transform2D* t) &&
                frame.TryFindAsset(cc.Config, out CharacterController2DConfig config)) {
              DrawCharacterController2DGizmo(t->Position.ToUnityVector3(), config, gizmosSettings.CharacterControllerColor, gizmosSettings.AsleepColliderColor, gizmosSettings.ColliderGizmosStyle);
            }
          }

          // ################## Components: CharacterController3D ##################

          foreach (var (entity, cc) in frame.GetComponentIterator<CharacterController3D>()) {
            if (frame.Unsafe.TryGetPointer(entity, out Transform3D* t) &&
                frame.TryFindAsset(cc.Config, out CharacterController3DConfig config)) {
              DrawCharacterController3DGizmo(t->Position.ToUnityVector3(), config, gizmosSettings.CharacterControllerColor, gizmosSettings.AsleepColliderColor, gizmosSettings.ColliderGizmosStyle);
            }
          }
        }

        // ################## Components: PhysicsJoints2D ##################

        if ((gizmosSettings.DrawJointGizmos & QuantumGameGizmosMode.OnApplicationPlaying) != 0) {
          foreach (var (handle, jointsComponent) in frame.Unsafe.GetComponentBlockIterator<PhysicsJoints2D>()) {
            if (frame.Unsafe.TryGetPointer(handle, out Transform2D* transform) && jointsComponent->TryGetJoints(frame, out var jointsBuffer, out var jointsCount)) {
              for (var i = 0; i < jointsCount; i++) {
                var curJoint = jointsBuffer + i;
                frame.Unsafe.TryGetPointer(curJoint->ConnectedEntity, out Transform2D* connectedTransform);
                DrawGizmosJoint2D(curJoint, transform, connectedTransform, selected: false, gizmosSettings, gizmosSettings.JointGizmosStyle);
              }
            }
          }
        }

        // ################## Components: PhysicsJoints3D ##################

        if ((gizmosSettings.DrawJointGizmos & QuantumGameGizmosMode.OnApplicationPlaying) != 0) {
          foreach (var (handle, jointsComponent) in frame.Unsafe.GetComponentBlockIterator<PhysicsJoints3D>()) {
            if (frame.Unsafe.TryGetPointer(handle, out Transform3D* transform) && jointsComponent->TryGetJoints(frame, out var jointsBuffer, out var jointsCount)) {
              for (var i = 0; i < jointsCount; i++) {
                var curJoint = jointsBuffer + i;
                frame.Unsafe.TryGetPointer(curJoint->ConnectedEntity, out Transform3D* connectedTransform);
                DrawGizmosJoint3D(curJoint, transform, connectedTransform, selected: false, gizmosSettings, gizmosSettings.JointGizmosStyle);
              }
            }
          }
        }


        // ################## Components: NavMesh Agent Components ##################
        Quantum.NavMesh currentNavmeshAsset = null;

        if (gizmosSettings.DrawNavMeshPathfinder || gizmosSettings.DrawNavMeshSteeringAgent || gizmosSettings.DrawNavMeshAvoidanceAgent) {
          foreach (var (entity, navmeshPathfinderAgent) in frame.GetComponentIterator<NavMeshPathfinder>()) {
            var position = Vector3.zero;
            if (frame.Has<Transform2D>(entity)) {
              position = frame.Unsafe.GetPointer<Transform2D>(entity)->Position.ToUnityVector3();
              if (frame.Has<Transform2DVertical>(entity)) {
                position.y = frame.Unsafe.GetPointer<Transform2DVertical>(entity)->Position.AsFloat;
              }
            } else if (frame.Has<Transform3D>(entity)) {
              position = frame.Unsafe.GetPointer<Transform3D>(entity)->Position.ToUnityVector3();
            }

            var config = frame.FindAsset<NavMeshAgentConfig>(navmeshPathfinderAgent.ConfigId);

            var agentRadius = 0.25f;
            if (currentNavmeshAsset == null || currentNavmeshAsset.Identifier.Guid != navmeshPathfinderAgent.NavMeshGuid) {
              // cache the asset, it's likely other agents use the same 
              QuantumUnityDB.TryGetGlobalAsset(navmeshPathfinderAgent.NavMeshGuid, out currentNavmeshAsset);
            }

            if (currentNavmeshAsset != null) {
              agentRadius = currentNavmeshAsset.MinAgentRadius.AsFloat;
            }

            var gizmoScaledSize = gizmosSettings.GizmoIconScale.AsFloat * gizmosSettings.NavMeshComponentGizmoSize;
            if (gizmosSettings.NavMeshComponentScaleWithAgentRadius) {
              gizmoScaledSize *= agentRadius;
            }

            if (gizmosSettings.DrawNavMeshPathfinder && navmeshPathfinderAgent.IsActive) {
              // Draw target and internal target
              GizmoUtils.DrawGizmosCircle(navmeshPathfinderAgent.InternalTarget.ToUnityVector3(), gizmoScaledSize, Color.magenta, style: gizmosSettings.NavMeshComponentGizmoStyle);
              if (navmeshPathfinderAgent.Target != navmeshPathfinderAgent.InternalTarget) {
                var desaturatedColor = Desaturate(Color.magenta, 0.25f);
                GizmoUtils.DrawGizmosCircle(navmeshPathfinderAgent.Target.ToUnityVector3(), gizmoScaledSize * 0.5f, desaturatedColor, style: gizmosSettings.NavMeshComponentGizmoStyle);
                Gizmos.color = desaturatedColor;
                Gizmos.DrawLine(navmeshPathfinderAgent.Target.ToUnityVector3(), navmeshPathfinderAgent.InternalTarget.ToUnityVector3());
              }

              // Draw waypoints
              for (int i = 0; i < navmeshPathfinderAgent.WaypointCount; i++) {
                var waypoint      = navmeshPathfinderAgent.GetWaypoint(frame, i);
                var waypointFlags = navmeshPathfinderAgent.GetWaypointFlags(frame, i);
                if (i > 0) {
                  var lastWaypoint = navmeshPathfinderAgent.GetWaypoint(frame, i - 1);
                  Gizmos.color = gizmosSettings.NavMeshPathfinderColor;
                  Gizmos.DrawLine(lastWaypoint.ToUnityVector3(), waypoint.ToUnityVector3());
                }

                GizmoUtils.DrawGizmosCircle(waypoint.ToUnityVector3(), gizmoScaledSize * 0.75f, gizmosSettings.NavMeshPathfinderColor, style: gizmosSettings.NavMeshComponentGizmoStyle);
                if (i == navmeshPathfinderAgent.WaypointIndex) {
                  GizmoUtils.DrawGizmosCircle(waypoint.ToUnityVector3(), gizmoScaledSize * 0.8f, Color.black, style: QuantumGizmoStyle.FillDisabled);
                }
              }
            }

            if (gizmosSettings.DrawNavMeshSteeringAgent) {
              if (frame.Has<NavMeshSteeringAgent>(entity)) {
                var steeringAgent = frame.Get<NavMeshSteeringAgent>(entity);
                Gizmos.color = gizmosSettings.NavMeshSteeringAgentColor;
                GizmoUtils.DrawGizmoVector(position, position + steeringAgent.Velocity.XOY.ToUnityVector3().normalized, gizmoScaledSize);
              }

              if (config.AvoidanceType != Navigation.AvoidanceType.None && frame.Has<NavMeshAvoidanceAgent>(entity)) {
                GizmoUtils.DrawGizmosCircle(position, config.AvoidanceRadius.AsFloat, gizmosSettings.NavMeshSteeringAgentColor, style: gizmosSettings.NavMeshComponentGizmoStyle);
              }

              GizmoUtils.DrawGizmosCircle(position, agentRadius, navmeshPathfinderAgent.IsActive ? gizmosSettings.NavMeshSteeringAgentColor : Desaturate(gizmosSettings.NavMeshSteeringAgentColor, 0.25f), style: gizmosSettings.NavMeshComponentGizmoStyle);
            }

            if (gizmosSettings.DrawNavMeshAvoidanceAgent) {
              if (config.AvoidanceType != Navigation.AvoidanceType.None && frame.Has<NavMeshAvoidanceAgent>(entity)) {
                GizmoUtils.DrawGizmosCircle(position, config.AvoidanceRadius.AsFloat, gizmosSettings.NavMeshAvoidanceAgentColor, style: gizmosSettings.NavMeshComponentGizmoStyle);
                var avoidanceRange = frame.SimulationConfig.Navigation.AvoidanceRange;
                GizmoUtils.DrawGizmosCircle(position, avoidanceRange.AsFloat, gizmosSettings.NavMeshAvoidanceAgentColor, style: QuantumGizmoStyle.FillDisabled);
              }
            }
          }
        }

        if (gizmosSettings.DrawNavMeshAvoidanceObstacle) {
          foreach (var (entity, navmeshObstacles) in frame.GetComponentIterator<NavMeshAvoidanceObstacle>()) {
            var position = Vector3.zero;

            if (frame.Has<Transform2D>(entity)) {
              position = frame.Unsafe.GetPointer<Transform2D>(entity)->Position.ToUnityVector3();
            } else if (frame.Has<Transform3D>(entity)) {
              position = frame.Unsafe.GetPointer<Transform3D>(entity)->Position.ToUnityVector3();
            }

            GizmoUtils.DrawGizmosCircle(position, navmeshObstacles.Radius.AsFloat, gizmosSettings.NavMeshAvoidanceAgentColor, style: gizmosSettings.NavMeshComponentGizmoStyle);

            if (navmeshObstacles.Velocity != FPVector2.Zero) {
              GizmoUtils.DrawGizmoVector(position, position + navmeshObstacles.Velocity.XOY.ToUnityVector3().normalized, gizmosSettings.GizmoIconScale.AsFloat * gizmosSettings.NavMeshComponentGizmoSize);
            }
          }
        }

        #endregion

        #region Navmesh And Pathfinder

        // ################## NavMeshes ##################

        if (gizmosSettings.DrawNavMesh) {
          var listOfNavmeshes = new List<Quantum.NavMesh>();
          if (gizmosSettings.DrawNavMesh) {
            listOfNavmeshes.AddRange(frame.Map.NavMeshes.Values);
          }

          if (frame.DynamicAssetDB.IsEmpty == false) {
            listOfNavmeshes.AddRange(frame.DynamicAssetDB.Assets.Where(a => a is NavMesh).Select(a => (NavMesh)a).ToList());
          }

          foreach (var navmesh in listOfNavmeshes) {
            QuantumNavMesh.CreateAndDrawGizmoMesh(navmesh, *frame.NavMeshRegionMask, gizmosSettings);

            for (Int32 i = 0; i < navmesh.Triangles.Length; i++) {
              var t = navmesh.Triangles[i];

              if (gizmosSettings.DrawNavMeshRegionIds) {
                if (t.Regions.HasValidRegions) {
                  var s = string.Empty;
                  for (int r = 0; r < frame.Map.Regions.Length; r++) {
                    if (t.Regions.IsRegionEnabled(r)) {
                      s += $"{frame.Map.Regions[r]} ({r})";
                    }
                  }

                  var vertex0 = navmesh.Vertices[t.Vertex0].Point.ToUnityVector3(true);
                  var vertex1 = navmesh.Vertices[t.Vertex1].Point.ToUnityVector3(true);
                  var vertex2 = navmesh.Vertices[t.Vertex2].Point.ToUnityVector3(true);
                  Handles.Label((vertex0 + vertex1 + vertex2) / 3.0f, s);
                }
              }
            }

            if (gizmosSettings.DrawNavMeshVertexNormals) {
              Gizmos.color = Color.blue;
              for (Int32 v = 0; v < navmesh.Vertices.Length; ++v) {
                if (navmesh.Vertices[v].Borders.Length >= 2) {
                  var normal = NavMeshVertex.CalculateNormal(v, navmesh, *frame.NavMeshRegionMask);
                  if (normal != FPVector3.Zero) {
                    GizmoUtils.DrawGizmoVector(navmesh.Vertices[v].Point.ToUnityVector3(true),
                      navmesh.Vertices[v].Point.ToUnityVector3(true) +
                      normal.ToUnityVector3(true) * gizmosSettings.GizmoIconScale.AsFloat * 0.33f,
                      GizmoUtils.DefaultArrowHeadLength * gizmosSettings.GizmoIconScale.AsFloat * 0.33f);
                  }
                }
              }
            }

            if (gizmosSettings.DrawNavMeshLinks) {
              for (Int32 i = 0; i < navmesh.Links.Length; i++) {
                var color = Color.blue;
                var link  = navmesh.Links[i];
                if (navmesh.Links[i].Region.IsSubset(*frame.NavMeshRegionMask) == false) {
                  color = Color.gray;
                }

                Gizmos.color = color;
                GizmoUtils.DrawGizmoVector(
                  navmesh.Links[i].Start.ToUnityVector3(),
                  navmesh.Links[i].End.ToUnityVector3(),
                  GizmoUtils.DefaultArrowHeadLength * gizmosSettings.GizmoIconScale.AsFloat);
                GizmoUtils.DrawGizmosCircle(navmesh.Links[i].Start.ToUnityVector3(), 0.1f * gizmosSettings.GizmoIconScale.AsFloat, color, style: gizmosSettings.ColliderGizmosStyle);
                GizmoUtils.DrawGizmosCircle(navmesh.Links[i].End.ToUnityVector3(), 0.1f * gizmosSettings.GizmoIconScale.AsFloat, color, style: gizmosSettings.ColliderGizmosStyle);
              }
            }
          }
        }

        // ################## NavMesh Borders ##################

        if (gizmosSettings.DrawNavMeshBorders) {
          Gizmos.color = Color.blue;
          var navmeshes = frame.Map.NavMeshes.Values;
          foreach (var navmesh in navmeshes) {
            for (Int32 i = 0; i < navmesh.Borders.Length; i++) {
              var b = navmesh.Borders[i];
              if (navmesh.IsBorderActive(i, *frame.NavMeshRegionMask) == false) {
                // grayed out?
                continue;
              }

              Gizmos.color = Color.black;
              Gizmos.DrawLine(b.V0.ToUnityVector3(true), b.V1.ToUnityVector3(true));

              //// How to do a thick line? Multiple GizmoDrawLine also possible.
              //var color = QuantumGameGizmosSettings.Instance.GetNavMeshColor(b.Regions);
              //UnityEditor.Handles.color = color;
              //UnityEditor.Handles.lighting = true;
              //UnityEditor.Handles.DrawAAConvexPolygon(
              //  b.V0.ToUnityVector3(true), 
              //  b.V1.ToUnityVector3(true), 
              //  b.V1.ToUnityVector3(true) + Vector3.up * 0.05f,
              //  b.V0.ToUnityVector3(true) + Vector3.up * 0.05f);
            }
          }
        }

        // ################## NavMesh Triangle Ids ##################

        if (gizmosSettings.DrawNavMeshTriangleIds) {
          Handles.color = Color.white;
          var navmeshes = frame.Map.NavMeshes.Values;
          foreach (var navmesh in navmeshes) {
            for (Int32 i = 0; i < navmesh.Triangles.Length; i++) {
              Handles.Label(navmesh.Triangles[i].Center.ToUnityVector3(true), i.ToString());
            }
          }
        }

        // ################## Pathfinder ##################

        if (frame.Navigation != null) {
          // Iterate though task contexts:
          var threadCount = frame.Context.TaskContext.ThreadCount;
          for (int t = 0; t < threadCount; t++) {
            // Iterate through path finders:
            var pf = frame.Navigation.GetDebugInformation(t).Item0;
            if (pf.RawPathSize >= 2) {
              if (gizmosSettings.DrawPathfinderRawPath) {
                for (Int32 i = 0; i < pf.RawPathSize; i++) {
                  GizmoUtils.DrawGizmosCircle(pf.RawPath[i].Point.ToUnityVector3(true), 0.1f * gizmosSettings.GizmoIconScale.AsFloat, pf.RawPath[i].Link >= 0 ? Color.black : Color.magenta);
                  if (i > 0) {
                    Gizmos.color = pf.RawPath[i].Link >= 0 && pf.RawPath[i].Link == pf.RawPath[i - 1].Link ? Color.black : Color.magenta;
                    Gizmos.DrawLine(pf.RawPath[i].Point.ToUnityVector3(true), pf.RawPath[i - 1].Point.ToUnityVector3(true));
                  }
                }
              }

              if (gizmosSettings.DrawPathfinderRawTrianglePath) {
                var nmGuid = frame.Navigation.GetDebugInformation(t).Item1;
                if (!string.IsNullOrEmpty(nmGuid)) {
                  QuantumUnityDB.TryGetGlobalAsset(nmGuid, out Quantum.NavMesh nm);
                  for (Int32 i = 0; i < pf.RawPathSize; i++) {
                    var triangleIndex = pf.RawPath[i].Index;
                    if (triangleIndex >= 0) {
                      var vertex0 = nm.Vertices[nm.Triangles[triangleIndex].Vertex0].Point.ToUnityVector3(true);
                      var vertex1 = nm.Vertices[nm.Triangles[triangleIndex].Vertex1].Point.ToUnityVector3(true);
                      var vertex2 = nm.Vertices[nm.Triangles[triangleIndex].Vertex2].Point.ToUnityVector3(true);
                      var color   = Color.magenta.Alpha(0.25f);
                      GizmoUtils.DrawGizmosTriangle(vertex0, vertex1, vertex2, gizmosSettings.GetSelectedColor(color, true));
                      Handles.color    = color;
                      Handles.lighting = true;
                      Handles.DrawAAConvexPolygon(vertex0, vertex1, vertex2);
                    }
                  }
                }
              }

              // Draw funnel on top of raw path
              if (gizmosSettings.DrawPathfinderFunnel) {
                for (Int32 i = 0; i < pf.PathSize; i++) {
                  GizmoUtils.DrawGizmosCircle(pf.Path[i].Point.ToUnityVector3(true), 0.05f * gizmosSettings.GizmoIconScale.AsFloat, pf.Path[i].Link >= 0 ? Color.green * 0.5f : Color.green);
                  if (i > 0) {
                    Gizmos.color = pf.Path[i].Link >= 0 && pf.Path[i].Link == pf.Path[i - 1].Link ? Color.green * 0.5f : Color.green;
                    Gizmos.DrawLine(pf.Path[i].Point.ToUnityVector3(true), pf.Path[i - 1].Point.ToUnityVector3(true));
                  }
                }
              }
            }
          }
        }

        #endregion

        #region Various

        // ################## Prediction Area ##################

        if (gizmosSettings.DrawPredictionArea && frame.Context.Culling != null) {
          var context = frame.Context;
          if (context.PredictionAreaRadius != FP.UseableMax) {
#if QUANTUM_XY
          // The Quantum simulation does not know about QUANTUM_XY and always keeps the vector2 Y component in the vector3 Z component.
          var predictionAreaCenter = new UnityEngine.Vector3(context.PredictionAreaCenter.X.AsFloat, context.PredictionAreaCenter.Z.AsFloat, 0);
#else
            var predictionAreaCenter = context.PredictionAreaCenter.ToUnityVector3();
#endif
            GizmoUtils.DrawGizmosSphere(predictionAreaCenter, context.PredictionAreaRadius.AsFloat, gizmosSettings.PredictionAreaColor);
          }
        }

        #endregion
      }
    }

    public static unsafe void DrawCharacterController2DGizmo(Vector3 position, CharacterController2DConfig config, Color radiusColor, Color extentsColor, QuantumGizmoStyle style) {
      GizmoUtils.DrawGizmosCircle(position + config.Offset.ToUnityVector3(),
        config.Radius.AsFloat, radiusColor, style: style);
      GizmoUtils.DrawGizmosCircle(position + config.Offset.ToUnityVector3(),
        config.Radius.AsFloat + config.Extent.AsFloat, extentsColor, style: style);
    }

    public static unsafe void DrawCharacterController3DGizmo(Vector3 position, CharacterController3DConfig config, Color radiusColor, Color extentsColor, QuantumGizmoStyle style) {
      GizmoUtils.DrawGizmosSphere(position + config.Offset.ToUnityVector3(),
        config.Radius.AsFloat, radiusColor, style: style);
      GizmoUtils.DrawGizmosSphere(position + config.Offset.ToUnityVector3(),
        config.Radius.AsFloat + config.Extent.AsFloat, extentsColor, style: style);
    }
    
    private static unsafe Color GetCollider2DColor(Frame frame, EntityRef handle, QuantumGameGizmosSettings gizmosSettings) {
      if (frame.Unsafe.TryGetPointer(handle, out PhysicsBody2D* body)) {
        if (body->IsKinematic) {
          return gizmosSettings.KinematicColliderColor;
        } else if (body->IsSleeping) {
          return gizmosSettings.AsleepColliderColor;
        } else if (!body->Enabled) {
          return gizmosSettings.DisabledColliderColor;
        } else {
          return gizmosSettings.DynamicColliderColor;
        }
      } else {
        return gizmosSettings.KinematicColliderColor;
      }
    }
    
    private static unsafe Color GetCollider3DColor(Frame frame, EntityRef handle, QuantumGameGizmosSettings gizmosSettings) {
      if (frame.Unsafe.TryGetPointer(handle, out PhysicsBody3D* body)) {
        if (body->IsKinematic) {
          return gizmosSettings.KinematicColliderColor;
        } else if (body->IsSleeping) {
          return gizmosSettings.AsleepColliderColor;
        } else if (!body->Enabled) {
          return gizmosSettings.DisabledColliderColor;
        } else {
          return gizmosSettings.DynamicColliderColor;
        }
      } else {
        return gizmosSettings.KinematicColliderColor;
      }
    }
    
    public static unsafe void DrawCollider3DGizmo(Frame frame, EntityRef handle, PhysicsCollider3D* collider, Color color, QuantumGizmoStyle style) {
      if (!frame.Unsafe.TryGetPointer(handle, out Transform3D* transform)) {
        return;
      }

      if (collider->Shape.Type == Shape3DType.Compound) {
        DrawCompoundShape3D(frame, &collider->Shape, transform, color, style);
      } else {
        DrawShape3DGizmo(collider->Shape, transform->Position.ToUnityVector3(),
          transform->Rotation.ToUnityQuaternion(), color, style);
      }
    }
    
    public static unsafe void DrawCollider2DGizmo(Frame frame, EntityRef handle, PhysicsCollider2D* collider, Color color, QuantumGizmoStyle style) {
      if (!frame.Unsafe.TryGetPointer(handle, out Transform2D* t)) {
        return;
      }
      
      var hasTransformVertical = frame.Unsafe.TryGetPointer<Transform2DVertical>(handle, out var tVertical);
      
      // Set 3d position of 2d object to simulate the vertical offset.
      var height = 0.0f;

#if QUANTUM_XY
    if (hasTransformVertical) {
      height = -tVertical->Height.AsFloat;
    }
#else
      if (hasTransformVertical) {
        height = tVertical->Height.AsFloat;
      }
#endif

      if (collider->Shape.Type == Shape2DType.Compound) {
        DrawCompoundShape2D(frame, &collider->Shape, t, tVertical, color, height, style);
      } else {
        var pos = t->Position.ToUnityVector3();
        var rot = t->Rotation.ToUnityQuaternion();

#if QUANTUM_XY
      if (hasTransformVertical) {
        pos.z = -tVertical->Position.AsFloat;
      }
#else
        if (hasTransformVertical) {
          pos.y = tVertical->Position.AsFloat;
        }
#endif

        DrawShape2DGizmo(collider->Shape, pos, rot, color, height, frame, style);
      }
    }

    public static unsafe void DrawShape3DGizmo(Shape3D s, Vector3 position, Quaternion rotation, Color color, QuantumGizmoStyle style = default) {
      var localOffset   = s.LocalTransform.Position.ToUnityVector3();
      var localRotation = s.LocalTransform.Rotation.ToUnityQuaternion();

      position += rotation * localOffset;
      rotation *= localRotation;

      switch (s.Type) {
        case Shape3DType.Sphere:
          GizmoUtils.DrawGizmosSphere(position, s.Sphere.Radius.AsFloat, color, style: style);
          break;
        case Shape3DType.Box:
          GizmoUtils.DrawGizmosBox(position, s.Box.Extents.ToUnityVector3() * 2, color, style: style, rotation: rotation);
          break;
        case Shape3DType.Capsule:
          GizmoUtils.DrawGizmosCapsule(position, s.Capsule.Radius.AsFloat, s.Capsule.Extent.AsFloat, color, style: style, rotation: rotation);
          break;
      }
    }

    public static unsafe void DrawShape2DGizmo(Shape2D s, Vector3 pos, Quaternion rot, Color color, float height, Frame currentFrame, QuantumGizmoStyle style = default) {
      var localOffset   = s.LocalTransform.Position.ToUnityVector3();
      var localRotation = s.LocalTransform.Rotation.ToUnityQuaternion();

      pos += rot * localOffset;
      rot =  rot * localRotation;

      switch (s.Type) {
        case Shape2DType.Circle:
          GizmoUtils.DrawGizmosCircle(pos, s.Circle.Radius.AsFloat, color, height: height, style: style);
          break;

        case Shape2DType.Box:
          var size = s.Box.Extents.ToUnityVector3() * 2.0f;
#if QUANTUM_XY
        size.z = height;
        pos.z += height * 0.5f;
#else
          size.y =  height;
          pos.y  += height * 0.5f;
#endif
          GizmoUtils.DrawGizmosBox(pos, size, color, rotation: rot, style: style);

          break;

        //TODO: check for the height
        case Shape2DType.Capsule:
          GizmoUtils.DrawGizmosCapsule2D(pos, s.Capsule.Radius.AsFloat, s.Capsule.Extent.AsFloat, color, rotation: rot, style: style);
          break;

        case Shape2DType.Polygon:
          PolygonCollider p;
          if (currentFrame != null) {
            p = currentFrame.FindAsset(s.Polygon.AssetRef);
          } else {
            QuantumUnityDB.TryGetGlobalAsset(s.Polygon.AssetRef, out p);
          }

          if (p != null) {
            GizmoUtils.DrawGizmoPolygon2D(pos, rot, p.Vertices, height, color, style: style);
          }

          break;


        case Shape2DType.Edge:
          var extent = rot * Vector3.right * s.Edge.Extent.AsFloat;
          GizmoUtils.DrawGizmosEdge(pos - extent, pos + extent, height, color);
          break;
      }
    }

    private static unsafe void DrawCompoundShape2D(Frame f, Shape2D* compoundShape, Transform2D* transform, Transform2DVertical* transformVertical, Color color, float height, QuantumGizmoStyle style = default) {
      Debug.Assert(compoundShape->Type == Shape2DType.Compound);

      if (compoundShape->Compound.GetShapes(f, out var shapesBuffer, out var count)) {
        for (var i = 0; i < count; i++) {
          var shape = shapesBuffer + i;

          if (shape->Type == Shape2DType.Compound) {
            DrawCompoundShape2D(f, shape, transform, transformVertical, color, height, style);
          } else {
            var pos = transform->Position.ToUnityVector3();
            var rot = transform->Rotation.ToUnityQuaternion();

#if QUANTUM_XY
          if (transformVertical != null) {
            pos.z = -transformVertical->Position.AsFloat;
          }
#else
            if (transformVertical != null) {
              pos.y = transformVertical->Position.AsFloat;
            }
#endif

            DrawShape2DGizmo(*shape, pos, rot, color, height, f, style);
          }
        }
      }
    }

    private static unsafe void DrawCompoundShape3D(Frame f, Shape3D* compoundShape, Transform3D* transform, Color color, QuantumGizmoStyle style = default) {
      Debug.Assert(compoundShape->Type == Shape3DType.Compound);

      if (compoundShape->Compound.GetShapes(f, out var shapesBuffer, out var count)) {
        for (var i = 0; i < count; i++) {
          var shape = shapesBuffer + i;

          if (shape->Type == Shape3DType.Compound) {
            DrawCompoundShape3D(f, shape, transform, color, style);
          } else {
            DrawShape3DGizmo(*shape, transform->Position.ToUnityVector3(), transform->Rotation.ToUnityQuaternion(), color, style);
          }
        }
      }
    }
    
    private static unsafe void DrawGizmosJoint2D(Joint* joint, Transform2D* jointTransform, Transform2D* connectedTransform, bool selected, QuantumGameGizmosSettings gizmosSettings, QuantumGizmoStyle style = default) {
      if (joint->Type == JointType.None) {
        return;
      }

      var param = default(QuantumGizmosJointInfo);
      param.Selected  = selected;
      param.JointRot  = jointTransform->Rotation.ToUnityQuaternion();
      param.AnchorPos = jointTransform->TransformPoint(joint->Anchor).ToUnityVector3();

      switch (joint->Type) {
        case JointType.DistanceJoint:
          param.Type        = QuantumGizmosJointInfo.GizmosJointType.DistanceJoint2D;
          param.MinDistance = joint->DistanceJoint.MinDistance.AsFloat;
          param.MaxDistance = joint->DistanceJoint.MaxDistance.AsFloat;
          break;

        case JointType.SpringJoint:
          param.Type        = QuantumGizmosJointInfo.GizmosJointType.SpringJoint2D;
          param.MinDistance = joint->SpringJoint.Distance.AsFloat;
          break;

        case JointType.HingeJoint:
          param.Type           = QuantumGizmosJointInfo.GizmosJointType.HingeJoint2D;
          param.RelRotRef      = Quaternion.Inverse(param.JointRot);
          param.UseAngleLimits = joint->HingeJoint.UseAngleLimits;
          param.LowerAngle     = (joint->HingeJoint.LowerLimitRad * FP.Rad2Deg).AsFloat;
          param.UpperAngle     = (joint->HingeJoint.UpperLimitRad * FP.Rad2Deg).AsFloat;
          break;
      }

      if (connectedTransform == null) {
        param.ConnectedRot = Quaternion.identity;
        param.ConnectedPos = joint->ConnectedAnchor.ToUnityVector3();
      } else {
        param.ConnectedRot = connectedTransform->Rotation.ToUnityQuaternion();
        param.ConnectedPos = connectedTransform->TransformPoint(joint->ConnectedAnchor).ToUnityVector3();
        param.RelRotRef    = (param.ConnectedRot * param.RelRotRef).normalized;
      }

#if QUANTUM_XY
      param.Axis = Vector3.back;
#else
      param.Axis = Vector3.up;
#endif

      DrawGizmosJointInternal(ref param, gizmosSettings, style);
    }
    
    private static unsafe void DrawGizmosJoint3D(Joint3D* joint, Transform3D* jointTransform, Transform3D* connectedTransform, bool selected, QuantumGameGizmosSettings gizmosSettings, QuantumGizmoStyle style = default) {
      if (joint->Type == JointType3D.None) {
        return;
      }

      var param = default(QuantumGizmosJointInfo);
      param.Selected  = selected;
      param.JointRot  = jointTransform->Rotation.ToUnityQuaternion();
      param.AnchorPos = jointTransform->TransformPoint(joint->Anchor).ToUnityVector3();

      switch (joint->Type) {
        case JointType3D.DistanceJoint:
          param.Type        = QuantumGizmosJointInfo.GizmosJointType.DistanceJoint3D;
          param.MinDistance = joint->DistanceJoint.MinDistance.AsFloat;
          param.MaxDistance = joint->DistanceJoint.MaxDistance.AsFloat;
          break;

        case JointType3D.SpringJoint:
          param.Type        = QuantumGizmosJointInfo.GizmosJointType.SpringJoint3D;
          param.MinDistance = joint->SpringJoint.Distance.AsFloat;
          break;

        case JointType3D.HingeJoint:
          param.Type           = QuantumGizmosJointInfo.GizmosJointType.HingeJoint3D;
          param.RelRotRef      = joint->HingeJoint.RelativeRotationReference.ToUnityQuaternion();
          param.Axis           = joint->HingeJoint.Axis.ToUnityVector3();
          param.UseAngleLimits = joint->HingeJoint.UseAngleLimits;
          param.LowerAngle     = (joint->HingeJoint.LowerLimitRad * FP.Rad2Deg).AsFloat;
          param.UpperAngle     = (joint->HingeJoint.UpperLimitRad * FP.Rad2Deg).AsFloat;
          break;
      }

      if (connectedTransform == null) {
        param.ConnectedRot = Quaternion.identity;
        param.ConnectedPos = joint->ConnectedAnchor.ToUnityVector3();
      } else {
        param.ConnectedRot = connectedTransform->Rotation.ToUnityQuaternion();
        param.ConnectedPos = connectedTransform->TransformPoint(joint->ConnectedAnchor).ToUnityVector3();
      }

      DrawGizmosJointInternal(ref param, gizmosSettings, style);
    }
    
    public static void DrawGizmosJointInternal(ref QuantumGizmosJointInfo p, QuantumGameGizmosSettings gizmosSettings, QuantumGizmoStyle style = default) {
      const float anchorRadiusFactor           = 0.1f;
      const float barHalfLengthFactor          = 0.1f;
      const float hingeRefAngleBarLengthFactor = 0.5f;

      // how much weaker the alpha of the color of hinge disc is relative to the its rim's alpha
      const float solidDiscAlphaRatio = 0.25f;

      if (p.Type == QuantumGizmosJointInfo.GizmosJointType.None) {
        return;
      }

      var gizmosScale = gizmosSettings.GizmoIconScale.AsFloat;

      var primColor    = gizmosSettings.JointGizmosPrimaryColor;
      var secColor     = gizmosSettings.JointGizmosSecondaryColor;
      var warningColor = gizmosSettings.JointGizmosWarningColor;

      if (p.Selected) {
        primColor    = primColor.Brightness(gizmosSettings.GizmoSelectedBrightness);
        secColor     = secColor.Brightness(gizmosSettings.GizmoSelectedBrightness);
        warningColor = warningColor.Brightness(gizmosSettings.GizmoSelectedBrightness);
      }

      GizmoUtils.DrawGizmosSphere(p.AnchorPos, gizmosScale * anchorRadiusFactor, secColor, style: style);
      GizmoUtils.DrawGizmosSphere(p.ConnectedPos, gizmosScale * anchorRadiusFactor, secColor, style: style);

      Gizmos.color = secColor;
      Gizmos.DrawLine(p.AnchorPos, p.ConnectedPos);

      switch (p.Type) {
        case QuantumGizmosJointInfo.GizmosJointType.DistanceJoint2D:
        case QuantumGizmosJointInfo.GizmosJointType.DistanceJoint3D: {
          var connectedToAnchorDir = Vector3.Normalize(p.AnchorPos - p.ConnectedPos);
          var minDistanceMark      = p.ConnectedPos + connectedToAnchorDir * p.MinDistance;
          var maxDistanceMark      = p.ConnectedPos + connectedToAnchorDir * p.MaxDistance;

          Gizmos.color = Handles.color = primColor;

          Gizmos.DrawLine(minDistanceMark, maxDistanceMark);
          GizmoUtils.DrawGizmoDisc(minDistanceMark, connectedToAnchorDir, barHalfLengthFactor, primColor, style: style);
          GizmoUtils.DrawGizmoDisc(maxDistanceMark, connectedToAnchorDir, barHalfLengthFactor, primColor, style: style);

          Gizmos.color = Handles.color = Color.white;

          break;
        }

        case QuantumGizmosJointInfo.GizmosJointType.SpringJoint2D:
        case QuantumGizmosJointInfo.GizmosJointType.SpringJoint3D: {
          var connectedToAnchorDir = Vector3.Normalize(p.AnchorPos - p.ConnectedPos);
          var distanceMark         = p.ConnectedPos + connectedToAnchorDir * p.MinDistance;

          Gizmos.color = Handles.color = primColor;

          Gizmos.DrawLine(p.ConnectedPos, distanceMark);
          GizmoUtils.DrawGizmoDisc(distanceMark, connectedToAnchorDir, barHalfLengthFactor, primColor, style: style);

          Gizmos.color = Handles.color = Color.white;

          break;
        }

        case QuantumGizmosJointInfo.GizmosJointType.HingeJoint2D: {
          var hingeRefAngleBarLength = hingeRefAngleBarLengthFactor * gizmosScale;
          var connectedAnchorRight   = p.ConnectedRot * Vector3.right;
          var anchorRight            = p.JointRot * Vector3.right;

          Gizmos.color = secColor;
          Gizmos.DrawRay(p.AnchorPos, anchorRight * hingeRefAngleBarLength);

          Gizmos.color = primColor;
          Gizmos.DrawRay(p.ConnectedPos, connectedAnchorRight * hingeRefAngleBarLength);

#if QUANTUM_XY
          var planeNormal = -Vector3.forward;
#else
          var planeNormal = Vector3.up;
#endif

          if (p.UseAngleLimits) {
            var fromDir    = Quaternion.AngleAxis(p.LowerAngle, planeNormal) * connectedAnchorRight;
            var angleRange = p.UpperAngle - p.LowerAngle;
            var arcColor   = angleRange < 0.0f ? warningColor : primColor;
            GizmoUtils.DrawGizmoArc(p.ConnectedPos, planeNormal, fromDir, angleRange, hingeRefAngleBarLength, arcColor, solidDiscAlphaRatio, style: style);
          } else {
            // Draw full disc
            GizmoUtils.DrawGizmoDisc(p.ConnectedPos, planeNormal, hingeRefAngleBarLength, primColor, solidDiscAlphaRatio, style: style);
          }

          Gizmos.color = Handles.color = Color.white;

          break;
        }

        case QuantumGizmosJointInfo.GizmosJointType.HingeJoint3D: {
          var hingeRefAngleBarLength = hingeRefAngleBarLengthFactor * gizmosScale;

          var hingeAxisLocal = p.Axis.sqrMagnitude > float.Epsilon ? p.Axis.normalized : Vector3.right;
          var hingeAxisWorld = p.JointRot * hingeAxisLocal;
          var hingeOrtho     = Vector3.Cross(hingeAxisWorld, p.JointRot * Vector3.up);

          hingeOrtho = hingeOrtho.sqrMagnitude > float.Epsilon ? hingeOrtho.normalized : Vector3.Cross(hingeAxisWorld, p.JointRot * Vector3.forward).normalized;

          Gizmos.color = Handles.color = primColor;

          Gizmos.DrawRay(p.AnchorPos, hingeOrtho * hingeRefAngleBarLength);
          Handles.ArrowHandleCap(0, p.ConnectedPos, Quaternion.FromToRotation(Vector3.forward, hingeAxisWorld), hingeRefAngleBarLengthFactor * 1.5f, EventType.Repaint);

          if (p.UseAngleLimits) {
            var refAngle   = ComputeRelativeAngleHingeJoint(hingeAxisWorld, p.JointRot, p.ConnectedRot, p.RelRotRef);
            var refOrtho   = Quaternion.AngleAxis(refAngle, hingeAxisWorld) * hingeOrtho;
            var fromDir    = Quaternion.AngleAxis(-p.LowerAngle, hingeAxisWorld) * refOrtho;
            var angleRange = p.UpperAngle - p.LowerAngle;
            var arcColor   = angleRange < 0.0f ? warningColor : primColor;
            GizmoUtils.DrawGizmoArc(p.ConnectedPos, hingeAxisWorld, fromDir, -angleRange, hingeRefAngleBarLength, arcColor, solidDiscAlphaRatio, style: style);
          } else {
            // Draw full disc
            GizmoUtils.DrawGizmoDisc(p.ConnectedPos, hingeAxisWorld, hingeRefAngleBarLength, primColor, solidDiscAlphaRatio, style: style);
          }

          Gizmos.color = Handles.color = Color.white;

          break;
        }
      }
    }
    
    private static float ComputeRelativeAngleHingeJoint(Vector3 hingeAxis, Quaternion rotJoint, Quaternion rotConnectedAnchor, Quaternion relRotRef) {
      var rotDiff = rotConnectedAnchor * Quaternion.Inverse(rotJoint);
      var relRot  = rotDiff * Quaternion.Inverse(relRotRef);

      var rotVector     = new Vector3(relRot.x, relRot.y, relRot.z);
      var sinHalfRadAbs = rotVector.magnitude;
      var cosHalfRad    = relRot.w;

      var hingeAngleRad = 2 * Mathf.Atan2(sinHalfRadAbs, Mathf.Sign(Vector3.Dot(rotVector, hingeAxis)) * cosHalfRad);

      // clamp to range [-Pi, Pi]
      if (hingeAngleRad < -Mathf.PI) {
        hingeAngleRad += 2 * Mathf.PI;
      }

      if (hingeAngleRad > Mathf.PI) {
        hingeAngleRad -= 2 * Mathf.PI;
      }

      return hingeAngleRad * Mathf.Rad2Deg;
    }
  }
  
  public struct QuantumGizmosJointInfo {
    public enum GizmosJointType {
      None = 0,

      DistanceJoint2D = 1,
      DistanceJoint3D = 2,

      SpringJoint2D = 3,
      SpringJoint3D = 4,

      HingeJoint2D = 5,
      HingeJoint3D = 6,
    }

    public GizmosJointType Type;
    public bool            Selected;

    public Vector3 AnchorPos;
    public Vector3 ConnectedPos;

    public Quaternion JointRot;
    public Quaternion ConnectedRot;
    public Quaternion RelRotRef;

    public float MinDistance;
    public float MaxDistance;

    public Vector3 Axis;

    public bool  UseAngleLimits;
    public float LowerAngle;
    public float UpperAngle;
  }
#endif
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumGameGizmosSettings.cs

namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;
  
  [Serializable]
  public class QuantumGameGizmosSettings  {
    [Header("Map")]
    public Color PhysicsGridColor = new Color(0.0f, 0.7f, 1f, 0.5f);

    public Color NavMeshGridColor = new Color(0.4f, 1.0f, 0.7f, 0.5f);

    [Header("Gizmos")]
    public FP GizmoIconScale = FP._1;

    public float GizmoSelectedBrightness = 1.1f;

    [Header("Collider Gizmos")]
    public QuantumGameGizmosMode DrawColliderGizmos = QuantumGameGizmosMode.OnDraw | QuantumGameGizmosMode.OnSelected | QuantumGameGizmosMode.OnApplicationPlaying;

    public QuantumGizmoStyle ColliderGizmosStyle = default;
    public QuantumGizmoStyle StaticColliderGizmoStyle = default;

    public Boolean DrawStaticMeshTriangles = true;
    public Boolean DrawStaticMeshNormals = false;
    public Boolean DrawSceneMeshCells = false;
    public Boolean DrawSceneMeshTriangles = false;

    public Color StaticColliderColor = new Color(0.4705882f, 0.7371198f, 1.0f, 0.5f);
    public Color DynamicColliderColor = new Color(0.4925605f, 0.9176471f, 0.5050631f, 0.5f);
    public Color KinematicColliderColor = ColorRGBA.White.AsColor.Alpha(0.5f);
    public Color CharacterControllerColor = ColorRGBA.Yellow.AsColor.Alpha(0.5f);
    public Color AsleepColliderColor = new Color(0.5192922f, 0.4622621f, 0.6985294f, 0.5f);
    public Color DisabledColliderColor = new Color(0.625f, 0.625f, 0.625f, 0.5f);

    [Header("Joint Gizmos")]
    public QuantumGameGizmosMode DrawJointGizmos = QuantumGameGizmosMode.OnSelected | QuantumGameGizmosMode.OnApplicationPlaying;

    public QuantumGizmoStyle JointGizmosStyle = default;
    public Color JointGizmosPrimaryColor = new Color(0, 1, 0, 0.5f);
    public Color JointGizmosSecondaryColor = new Color(0, 1, 1, 0.5f);
    public Color JointGizmosWarningColor = new Color(1, 0, 0, 0.5f);

    [Header("Prediction Culling Gizmos")]
    public Boolean DrawPredictionArea = true;

    public Color PredictionAreaColor = new Color(1, 0, 0, 0.25f);

    [Header("Pathfinder Gizmos")]
    public Boolean DrawPathfinderRawPath = false;

    public Boolean DrawPathfinderRawTrianglePath = false;
    public Boolean DrawPathfinderFunnel = false;

    [Header("NavMesh Component Gizmos")]
    public Boolean DrawNavMeshPathfinder = false;

    public Boolean DrawNavMeshSteeringAgent = false;
    public Boolean DrawNavMeshAvoidanceAgent = false;
    public Boolean DrawNavMeshAvoidanceObstacle = false;
    public QuantumGizmoStyle NavMeshComponentGizmoStyle = QuantumGizmoStyle.FillDisabled;

    [Range(0.01f, 10.0f)]
    public float NavMeshComponentGizmoSize = 0.5f;

    public Boolean NavMeshComponentScaleWithAgentRadius = true;
    public Color NavMeshPathfinderColor = Color.yellow;
    public Color NavMeshSteeringAgentColor = new Color(0, 1, 0, 0.5f);
    public Color NavMeshAvoidanceAgentColor = new Color(0, 0, 1, 0.5f);

    [Header("NavMesh Gizmos")]
    public Boolean DrawNavMesh = false;

    public Boolean DrawNavMeshBorders = false;
    public Boolean DrawNavMeshTriangleIds = false;
    public Boolean DrawNavMeshRegionIds = false;
    public Boolean DrawNavMeshVertexNormals = false;
    public Boolean DrawNavMeshLinks = false;
    public Color NavMeshDefaultColor = new Color(0.0f, 0.75f, 1.0f, 0.5f);
    public Color NavMeshRegionColor = new Color(1.0f, 0.0f, 0.5f, 0.5f);

    public Color GetNavMeshColor(NavMeshRegionMask regionMask) {
      if (regionMask.IsMainArea) {
        return NavMeshDefaultColor;
      }

      return NavMeshRegionColor;
    }

    public float? GetSelectedBrightness(bool selected) {
      return selected ? GizmoSelectedBrightness : (float?)null;
    }

    public Color GetSelectedColor(Color color, bool selected) {
      return selected ? color.Brightness(GizmoSelectedBrightness) : color;
    }
  }
  
  [Flags, Serializable]
  public enum QuantumGameGizmosMode {
    None                 = 0,
    OnDraw               = 1 << 0,
    OnSelected           = 1 << 1,
    OnApplicationPlaying = 1 << 2,
  }
  
  [Flags, Serializable]
  public enum QuantumMeshGizmos {
    DrawTriangles = 1 << 0,
    DrawNormals = 1 << 1,
    
    Default = DrawTriangles | DrawNormals,
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumInstantReplay.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using Photon.Deterministic;
  using Quantum.Core;
  using UnityEngine;

  public enum QuantumInstantReplaySeekMode {
    Disabled,
    FromStartSnapshot,
    FromIntermediateSnapshots,
  }

  public sealed class QuantumInstantReplay : IDisposable {
    // We need this to fast forward the simulation and wait until is fully initialized.
    public const int InitalFramesToSimulation = 4;

    private bool                         _loop;
    private QuantumRunner                _replayRunner;
    private DeterministicFrameRingBuffer _rewindSnapshots;

    public QuantumInstantReplay(QuantumGame liveGame, float length, QuantumInstantReplaySeekMode seekMode = QuantumInstantReplaySeekMode.Disabled, bool loop = false) {
      if (liveGame == null) {
        throw new ArgumentNullException(nameof(liveGame));
      }

      LiveGame = liveGame;
      EndFrame = liveGame.Frames.Verified.Number;

      var deterministicConfig = liveGame.Session.SessionConfig;
      var desiredReplayFrame  = EndFrame - Mathf.FloorToInt(length * deterministicConfig.UpdateFPS);
      // clamp against actual start frame
      desiredReplayFrame = Mathf.Max(deterministicConfig.UpdateFPS, desiredReplayFrame);

      var snapshot = liveGame.GetInstantReplaySnapshot(desiredReplayFrame);
      if (snapshot == null) {
        throw new ArgumentException(nameof(liveGame), "Unable to find a snapshot for frame " + desiredReplayFrame);
      }

      // Chose replay input provider based on if delta compression is enabled.
      var replayInputProvider = default(IDeterministicReplayProvider);
      if (deterministicConfig.InputDeltaCompression) {
        if (liveGame.RecordInputStream != null) {
          liveGame.RecordInputStream.Flush();

          // Seek recorded stream to position 0.
          var recordSteamPosition = liveGame.RecordInputStream.Position;
          liveGame.RecordInputStream.SeekOrThrow(0, SeekOrigin.Begin);

          // Read from the recorded frame until we find the desired start frame.
          StreamReplayInputProvider.ForwardToFrame(liveGame.RecordInputStream, snapshot.Number);

          // Copy part into the memory stream
          var memoryStream = new MemoryStream((int)(recordSteamPosition));
          liveGame.RecordInputStream.CopyTo(memoryStream);

          // Reset the recorded steam position
          liveGame.RecordInputStream.SeekOrThrow(recordSteamPosition, SeekOrigin.Begin);

          // Rewind the copied stream
          memoryStream.SeekOrThrow(0, SeekOrigin.Begin);
          replayInputProvider = new StreamReplayInputProvider(memoryStream, liveGame.Session.FrameVerified.Number);
        }
      } else {
        replayInputProvider = liveGame.Session.IsReplay ? liveGame.Session.ReplayProvider : liveGame.RecordedInputs;
      }

      if (replayInputProvider == null) {
        throw new ArgumentException(nameof(liveGame), "Can't run instant replays without an input provider. Start the game with StartParams including RecordingFlags.Input.");
      }

      StartFrame = Mathf.Max(snapshot.Number, desiredReplayFrame);

      List<Frame> snapshotsForRewind = null;
      if (seekMode == QuantumInstantReplaySeekMode.FromIntermediateSnapshots) {
        snapshotsForRewind = new List<Frame>();
        liveGame.GetInstantReplaySnapshots(desiredReplayFrame, EndFrame, snapshotsForRewind);
        Debug.Assert(snapshotsForRewind.Count >= 1);
      } else if (seekMode == QuantumInstantReplaySeekMode.FromStartSnapshot) {
        snapshotsForRewind = new List<Frame>() { snapshot };
      } else if (loop) {
        throw new ArgumentException(nameof(loop), $"Seek mode not compatible with looping: {seekMode}");
      }

      _loop = loop;

      // Create all required start parameters and serialize the snapshot as start data.
      var arguments = new SessionRunner.Arguments {
        RunnerFactory  = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
        RuntimeConfig  = liveGame.Configurations.Runtime,
        SessionConfig  = deterministicConfig,
        ReplayProvider = replayInputProvider,
        GameMode       = DeterministicGameMode.Replay,
        FrameData      = snapshot.Serialize(DeterministicFrameSerializeMode.Blit),
        InitialFrame   = snapshot.Number,
        RunnerId       = "InstantReplay",
        PlayerCount    = deterministicConfig.PlayerCount,
        HeapExtraCount = snapshotsForRewind?.Count ?? 0,
      };

      _replayRunner                         = QuantumRunner.StartGame(arguments);
      _replayRunner.IsSessionUpdateDisabled = true;

      // Run a couple of frames until fully initialized (replayRunner.Session.FrameVerified is set and session state isRunning).
      for (int i = 0; i < InitalFramesToSimulation; i++) {
        _replayRunner.Session.Update(1.0f / deterministicConfig.UpdateFPS);
      }

      // clone the original snapshots
      Debug.Assert(_rewindSnapshots == null);
      if (snapshotsForRewind != null) {
        _rewindSnapshots = new DeterministicFrameRingBuffer(snapshotsForRewind.Count);
        foreach (var frame in snapshotsForRewind) {
          _rewindSnapshots.PushBack(frame, _replayRunner.Game.CreateFrame);
        }
      }

      if (desiredReplayFrame > CurrentFrame) {
        FastForward(desiredReplayFrame);
      }
    }

    public int StartFrame   { get; }
    public int CurrentFrame => _replayRunner.Game.Frames.Verified.Number;
    public int EndFrame     { get; }

    public bool CanSeek   => _rewindSnapshots?.Count > 0;
    public bool IsRunning => CurrentFrame < EndFrame;

    public QuantumGame LiveGame   { get; }
    public QuantumGame ReplayGame => _replayRunner?.Game;

    public float NormalizedTime {
      get {
        var   currentFrame = _replayRunner.Game.Frames.Verified.Number;
        float result       = (currentFrame - StartFrame) / (float)(EndFrame - StartFrame);
        Debug.Assert(result >= 0.0f);
        return Mathf.Clamp01(result);
      }
    }

    public void Dispose() {
      _rewindSnapshots?.Clear();
      _rewindSnapshots = null;
      _replayRunner?.Shutdown();
      _replayRunner = null;
    }

    public void SeekFrame(int frameNumber) {
      if (!CanSeek) {
        throw new InvalidOperationException("Not seekable");
      }

      Debug.Assert(_rewindSnapshots != null);
      var frame = _rewindSnapshots.Find(frameNumber, DeterministicFrameSnapshotBufferFindMode.ClosestLessThanOrEqual);
      if (frame == null) {
        throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Unable to find a frame with number less or equal to {frameNumber}.");
      }

      _replayRunner.Session.ResetReplay(frame);
      FastForward(frameNumber);
    }

    public void SeekNormalizedTime(float normalizedTime) {
      var frame = Mathf.FloorToInt(Mathf.Lerp(StartFrame, EndFrame, normalizedTime));
      SeekFrame(frame);
    }

    public bool Update(float deltaTime) {
      _replayRunner.Session.Update(deltaTime);

      // Stop the running instant replay.
      if (_replayRunner.Game.Frames.Verified != null &&
          _replayRunner.Game.Frames.Verified.Number >= EndFrame) {
        if (_loop) {
          SeekFrame(StartFrame);
        } else {
          return false;
        }
      }

      return true;
    }

    private void FastForward(int frameNumber) {
      if (frameNumber < CurrentFrame) {
        throw new ArgumentException($"Can't seek backwards to {frameNumber} from {CurrentFrame}", nameof(frameNumber));
      } else if (frameNumber == CurrentFrame) {
        // nothing to do here
        return;
      }

      const int MaxAttempts = 3;
      for (int attemptsLeft = MaxAttempts; attemptsLeft > 0; --attemptsLeft) {
        int beforeUpdate = CurrentFrame;

        double deltaTime = GetDeltaTime(frameNumber - beforeUpdate, _replayRunner.Session.SessionConfig.UpdateFPS);
        _replayRunner.Session.Update(deltaTime);

        int afterUpdate = CurrentFrame;

        if (afterUpdate >= frameNumber) {
          if (afterUpdate > frameNumber) {
            Debug.LogWarning($"Seeked after the target frame {frameNumber} (from {beforeUpdate}), got to {afterUpdate}.");
          }

          return;
        } else {
          Debug.LogWarning($"Failed to seek to frame {frameNumber} (from {beforeUpdate}), got to {afterUpdate}. {attemptsLeft} attempts left.");
        }
      }

      throw new InvalidOperationException($"Unable to seek to frame {frameNumber}, ended up on {CurrentFrame}");
    }

    private static double GetDeltaTime(int frames, int simulationRate) {
      // need repeated sum here, since internally Quantum performs repeated substraction
      double delta  = 1.0 / simulationRate;
      double result = 0;
      for (int i = 0; i < frames; ++i) {
        result += delta;
      }

      return result;
    }
  }

  [Obsolete]
  public class QuantumInstantReplayLegacy {
    public bool        IsRunning     { get; private set; }
    public float       ReplayLength  { get; set; }
    public float       PlaybackSpeed { get; set; }
    public QuantumGame LiveGame      => _liveGame;
    public QuantumGame ReplayGame    => _replayRunner?.Game;

    public int StartFrame { get; private set; }
    public int EndFrame   { get; private set; }

    public bool CanSeek => _rewindSnapshots?.Count > 0;

    public float NormalizedTime {
      get {
        if (!IsRunning) {
          throw new InvalidOperationException("Not running");
        }

        var   currentFrame = _replayRunner.Game.Frames.Verified.Number;
        float result       = (currentFrame - StartFrame) / (float)(EndFrame - StartFrame);
        return result;
      }
    }

    public event Action<QuantumGame> OnReplayStarted;
    public event Action<QuantumGame> OnReplayStopped;

    // We need this to fast forward the simulation and wait until is fully initialized.
    public const int InitalFramesToSimulation = 4;

    private QuantumGame                  _liveGame;
    private QuantumRunner                _replayRunner;
    private DeterministicFrameRingBuffer _rewindSnapshots;
    private bool                         _loop;

    public QuantumInstantReplayLegacy(QuantumGame game) {
      _liveGame = game;
    }

    public void Shutdown() {
      if (IsRunning)
        StopInstantReplay();

      OnReplayStarted = null;
      OnReplayStopped = null;

      _liveGame = null;
    }

    public void Update() {
      if (IsRunning) {
        _replayRunner.Session.Update(Time.unscaledDeltaTime * PlaybackSpeed);

        // Stop the running instant replay.
        if (_replayRunner.Game.Frames.Verified != null &&
            _replayRunner.Game.Frames.Verified.Number >= EndFrame) {
          if (_loop) {
            SeekFrame(StartFrame);
          } else {
            StopInstantReplay();
          }
        }
      }
    }

    public void StartInstantReplay(QuantumInstantReplaySeekMode seekMode = QuantumInstantReplaySeekMode.Disabled, bool loop = false) {
      if (IsRunning) {
        Debug.LogError("Instant replay is already running.");
        return;
      }

      var inputProvider = _liveGame.Session.IsReplay ? _liveGame.Session.ReplayProvider : _liveGame.RecordedInputs;
      if (inputProvider == null) {
        Debug.LogError("Can't run instant replays without an input provider. Start the game with StartParams including RecordingFlags.Input.");
        return;
      }

      IsRunning = true;
      EndFrame  = _liveGame.Frames.Verified.Number;

      var deterministicConfig = _liveGame.Session.SessionConfig;
      var desiredReplayFrame  = EndFrame - Mathf.FloorToInt(ReplayLength * deterministicConfig.UpdateFPS);

      // clamp against actual start frame
      desiredReplayFrame = Mathf.Max(deterministicConfig.UpdateFPS, desiredReplayFrame);

      var snapshot = _liveGame.GetInstantReplaySnapshot(desiredReplayFrame);
      if (snapshot == null) {
        throw new InvalidOperationException("Unable to find a snapshot for frame " + desiredReplayFrame);
      }

      StartFrame = Mathf.Max(snapshot.Number, desiredReplayFrame);

      List<Frame> snapshotsForRewind = null;
      if (seekMode == QuantumInstantReplaySeekMode.FromIntermediateSnapshots) {
        snapshotsForRewind = new List<Frame>();
        _liveGame.GetInstantReplaySnapshots(desiredReplayFrame, EndFrame, snapshotsForRewind);
        Debug.Assert(snapshotsForRewind.Count >= 1);
      } else if (seekMode == QuantumInstantReplaySeekMode.FromStartSnapshot) {
        snapshotsForRewind = new List<Frame>();
        snapshotsForRewind.Add(snapshot);
      } else if (loop) {
        throw new ArgumentException(nameof(loop), $"Seek mode not compatible with looping: {seekMode}");
      }

      _loop = loop;

      // Create all required start parameters and serialize the snapshot as start data.
      var arguments = new SessionRunner.Arguments {
        RunnerFactory  = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
        RuntimeConfig  = _liveGame.Configurations.Runtime,
        SessionConfig  = deterministicConfig,
        ReplayProvider = inputProvider,
        GameMode       = DeterministicGameMode.Replay,
        FrameData      = snapshot.Serialize(DeterministicFrameSerializeMode.Blit),
        InitialFrame   = snapshot.Number,
        RunnerId       = "InstantReplay",
        PlayerCount    = deterministicConfig.PlayerCount,
        HeapExtraCount = snapshotsForRewind?.Count ?? 0,
      };

      _replayRunner                         = QuantumRunner.StartGame(arguments);
      _replayRunner.IsSessionUpdateDisabled = true;

      // Run a couple of frames until fully initialized (replayRunner.Session.FrameVerified is set and session state isRunning).
      for (int i = 0; i < InitalFramesToSimulation; i++) {
        _replayRunner.Session.Update(1.0f / deterministicConfig.UpdateFPS);
      }

      // clone the original snapshots
      Debug.Assert(_rewindSnapshots == null);
      if (snapshotsForRewind != null) {
        _rewindSnapshots = new DeterministicFrameRingBuffer(snapshotsForRewind.Count);
        foreach (var frame in snapshotsForRewind) {
          _rewindSnapshots.PushBack(frame, _replayRunner.Game.CreateFrame);
        }
      }

      FastForwardSimulation(desiredReplayFrame);

      if (OnReplayStarted != null)
        OnReplayStarted(_replayRunner.Game);
    }

    public void SeekNormalizedTime(float seek) {
      var frame = Mathf.FloorToInt(Mathf.Lerp(StartFrame, EndFrame, seek));
      SeekFrame(frame);
    }

    public void SeekFrame(int frameNumber) {
      if (!CanSeek) {
        throw new InvalidOperationException("Not seekable");
      }

      if (!IsRunning) {
        throw new InvalidOperationException("Not running");
      }

      Debug.Assert(_rewindSnapshots != null);
      var frame = _rewindSnapshots.Find(frameNumber, DeterministicFrameSnapshotBufferFindMode.ClosestLessThanOrEqual);
      if (frame == null) {
        throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Unable to find a frame with number less or equal to {frameNumber}.");
      }

      _replayRunner.Session.ResetReplay(frame);
      FastForwardSimulation(frameNumber);
    }

    public void StopInstantReplay() {
      if (!IsRunning) {
        Debug.LogError("Instant replay is not running.");
        return;
      }

      IsRunning = false;

      if (OnReplayStopped != null)
        OnReplayStopped(_replayRunner.Game);

      _rewindSnapshots?.Clear();
      _rewindSnapshots = null;

      _replayRunner?.Shutdown();
      _replayRunner = null;
    }

    private void FastForwardSimulation(int frameNumber) {
      var simulationRate = _replayRunner.Session.SessionConfig.UpdateFPS;
      while (_replayRunner.Session.FrameVerified.Number < frameNumber) {
        _replayRunner.Session.Update(1.0f / simulationRate);
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumLoadBalancingClient.cs

namespace Quantum {
  using Photon.Client;
  using Photon.Realtime;
  using System;

  [Obsolete("Not used anymore. Replace by using RealtimeClient directly.")]
  public class QuantumLoadBalancingClient : RealtimeClient {
    public QuantumLoadBalancingClient(ConnectionProtocol protocol = ConnectionProtocol.Udp) : base(protocol) {
    }

    public virtual bool ConnectUsingSettings(AppSettings appSettings, string nickname) {
      return ConnectUsingSettings(appSettings);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumMapDataBaker.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using Photon.Analyzer;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;
  using UnityEngine.SceneManagement;
  using Debug = UnityEngine.Debug;

  public class QuantumMapDataBaker {
    [StaticField(StaticFieldResetMode.None)]
    public static int NavMeshSerializationBufferSize = 1024 * 1024 * 60;

    public enum BuildTrigger {
      SceneSave,
      PlaymodeChange,
      Build,
      Manual
    }

    public static void BakeMapData(QuantumMapData data, Boolean inEditor, Boolean bakeColliders = true, Boolean bakePrototypes = true, QuantumMapDataBakeFlags bakeFlags = QuantumMapDataBakeFlags.None, BuildTrigger buildTrigger = BuildTrigger.Manual) {
      using var _ = TraceScope("BakeMapData");
      
      using (TraceScope("LoadLookupTables")) {
        FPMathUtils.LoadLookupTables();
      }

      if (inEditor == false && !data.Asset) {
        data.Asset = AssetObject.Create<Map>();
      }

#if UNITY_EDITOR
      if (inEditor) {
        // set scene name
        data.Asset.Scene = data.gameObject.scene.name;

        var path = data.gameObject.scene.path;
        data.Asset.ScenePath = path;
        if (string.IsNullOrEmpty(path)) {
          data.Asset.SceneGuid = string.Empty;
        } else {
          data.Asset.SceneGuid = AssetDatabase.AssetPathToGUID(path);
        }
        
        // map needs to be unloaded before it is modified; otherwise,
        // memory leaks might occur
        QuantumUnityDB.DisposeGlobalAsset(data.Asset.Guid, immediate: true);
      }
#endif

      using (TraceScope("OnBeforeBake")) {
        InvokeCallbacks("OnBeforeBake", data, buildTrigger, bakeFlags);
      }

      using (TraceScope("OnBeforeBake (legacy)")) {
        InvokeCallbacks("OnBeforeBake", data);
      }

      if (bakeColliders) {
        using (TraceScope("BakeColliders")) {
          BakeColliders(data, inEditor);
        }
      }

      if (bakePrototypes) {
        using (TraceScope("BakingPrototypes")) {
          BakePrototypes(data);
        }
      }

      using (TraceScope("OnBake")) {
        // invoke callbacks
        InvokeCallbacks("OnBake", data);
      }
    }

    public static void BakeMeshes(QuantumMapData data, Boolean inEditor) {
      if (inEditor) {
#if UNITY_EDITOR
        var dirPath   = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset));
        var assetPath = Path.Combine(dirPath, data.Asset.name + "_mesh.asset");

        var binaryDataAsset = AssetDatabase.LoadAssetAtPath<Quantum.BinaryData>(assetPath);
        if (binaryDataAsset == null) {
          binaryDataAsset = ScriptableObject.CreateInstance<Quantum.BinaryData>();
          AssetDatabase.CreateAsset(binaryDataAsset, assetPath);
        }

        // Serialize to binary some of the data (max 20 megabytes for now)
        var bytestream = new ByteStream(new Byte[data.Asset.GetStaticColliderTrianglesSerializedSize(isWriting: true)]);
        data.Asset.SerializeStaticColliderTriangles(bytestream, allocator: null, true);

        binaryDataAsset.SetData(bytestream.ToArray(), binaryDataAsset.IsCompressed);
        EditorUtility.SetDirty(binaryDataAsset);

        data.Asset.StaticColliders3DTrianglesData = binaryDataAsset;
#endif
      }
    }

#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI

    public static IEnumerable<Quantum.NavMesh> BakeNavMeshes(QuantumMapData data, Boolean inEditor) {
      FPMathUtils.LoadLookupTables();

      data.Asset.NavMeshLinks = new AssetRef<NavMesh>[0];
      data.Asset.Regions      = new string[0];

      InvokeCallbacks("OnBeforeBakeNavMesh", data);

      var navmeshes = BakeNavMeshesLoop(data).ToList();

      InvokeCallbacks("OnCollectNavMeshes", data, navmeshes);

      if (inEditor) {
#if UNITY_EDITOR
        var        dirPath    = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset));
        ByteStream bytestream = null;
        foreach (var navmesh in navmeshes) {

          // create and write navmesh (binary) _data asset
          {
            var navmeshBinaryFilename = Path.Combine(dirPath, $"{data.Asset.name}_{navmesh.Name}_data.asset");
            var binaryDataAsset = AssetDatabase.LoadAssetAtPath<Quantum.BinaryData>(navmeshBinaryFilename);
            if (binaryDataAsset == null) {
              binaryDataAsset = ScriptableObject.CreateInstance<Quantum.BinaryData>();
              AssetDatabase.CreateAsset(binaryDataAsset, navmeshBinaryFilename);
            }

            // Serialize to binary some of the data (max 60 megabytes for now)
            if (bytestream == null) {
              bytestream = new ByteStream(new Byte[NavMeshSerializationBufferSize]);
            } else {
              bytestream.Reset();
            }

            navmesh.Serialize(bytestream, true);

            binaryDataAsset.SetData(bytestream.ToArray(), binaryDataAsset.IsCompressed);
            EditorUtility.SetDirty(binaryDataAsset);

            navmesh.DataAsset = binaryDataAsset;
          }

          // create and write navmesh Quantum asset
          {
            var navmeshAssetPath = Path.Combine(dirPath, $"{data.Asset.name}_{navmesh.Name}.asset");
            var navMeshAsset = AssetDatabase.LoadAssetAtPath<Quantum.NavMesh>(navmeshAssetPath);
            if (navMeshAsset == null) {
              navMeshAsset = ScriptableObject.CreateInstance<Quantum.NavMesh>();
              AssetDatabase.CreateAsset(navMeshAsset, navmeshAssetPath);
            }
            else {
              navmesh.Guid = navMeshAsset.Guid;
              navmesh.Path = QuantumUnityDB.CreateAssetPathFromUnityPath(navmeshAssetPath);
            }

            // Preprocessing CopySerialized
            navmesh.name = navMeshAsset.name;

            EditorUtility.CopySerialized(navmesh, navMeshAsset);
            EditorUtility.SetDirty(navMeshAsset);

            ArrayUtils.Add(ref data.Asset.NavMeshLinks, (Quantum.AssetRef<Quantum.NavMesh>)navMeshAsset);
            EditorUtility.SetDirty(data.Asset);
          }
        }
#endif
      } else {
        // When executing this during runtime the guids of the created navmesh are added to the map.
        // Binary navmesh files are not created because the fresh navmesh object has everything it needs.
        // Caveat: the returned navmeshes need to be added to the DB by either...
        // A) overwriting the navmesh inside an already existing QAssetNavMesh ScriptableObject or
        // B) Creating new QAssetNavMesh ScriptableObjects (see above) and inject them into the DB (use UnityDB.OnAssetLoad callback).
        foreach (var navmesh in navmeshes) {
          navmesh.Path = data.Asset.name + "_" + navmesh.Name;
          ArrayUtils.Add(ref data.Asset.NavMeshLinks, (Quantum.AssetRef<Quantum.NavMesh>)navmesh);
        }
      }

      InvokeCallbacks("OnBakeNavMesh", data);

      return navmeshes;
    }

#else 
    public static IEnumerable<Quantum.NavMesh> BakeNavMeshes(QuantumMapData data, Boolean inEditor) {
      return null;
    }
#endif

      static StaticColliderData GetStaticData(GameObject gameObject, QuantumStaticColliderSettings settings, int colliderId) {
      return new StaticColliderData {
        Asset         = settings.Asset,
        Name          = gameObject.name,
        Tag           = gameObject.tag,
        Layer         = gameObject.layer,
        IsTrigger     = settings.Trigger,
        ColliderIndex = colliderId,
        MutableMode   = settings.MutableMode,
      };
    }

    public static void BakeColliders(QuantumMapData data, Boolean inEditor) {
      var scene = data.gameObject.scene;
      Debug.Assert(scene.IsValid());

      // clear existing colliders
      data.StaticCollider2DReferences = new List<MonoBehaviour>();
      data.StaticCollider3DReferences = new List<MonoBehaviour>();

      // 2D
      data.Asset.StaticColliders2D = new MapStaticCollider2D[0];
      var staticCollider2DList = new List<MapStaticCollider2D>();

#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
      // circle colliders
      foreach (var collider in FindLocalObjects<QuantumStaticCircleCollider2D>(scene)) {
        collider.BeforeBake();

        var scale = collider.transform.lossyScale;
        var scale2D = scale.ToFPVector2();

        staticCollider2DList.Add(new MapStaticCollider2D {
          Position = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector3()).ToFPVector2(),
          Rotation = collider.transform.rotation.ToFPRotation2D(),
#if QUANTUM_XY
        VerticalOffset = -collider.transform.position.z.ToFP(),
        Height = collider.Height * scale.z.ToFP(),
#else
          VerticalOffset = collider.transform.position.y.ToFP(),
          Height         = collider.Height * scale.y.ToFP(),
#endif
          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider2DList.Count),
          Layer           = collider.gameObject.layer,

          // circle
          ShapeType    = Shape2DType.Circle,
          CircleRadius = collider.Radius * FPMath.Max(scale2D.X, scale2D.Y),
        });

        data.StaticCollider2DReferences.Add(collider);
      }

      // capsule colliders
      foreach (var collider in FindLocalObjects<QuantumStaticCapsuleCollider2D>(scene)) {
        collider.BeforeBake();

        var scale = collider.transform.lossyScale;
        

        staticCollider2DList.Add(new MapStaticCollider2D {
          Position        = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector2()).ToFPVector2(),
          Rotation        = collider.RotationOffset,

          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider2DList.Count),

          // capsule
          ShapeType    = Shape2DType.Capsule,
          CapsuleSize  = new FPVector2(FP.FromFloat_UNSAFE(collider.Size.X.AsFloat * scale.x),FP.FromFloat_UNSAFE(collider.Size.Y.AsFloat * scale.y))

        });

        data.StaticCollider3DReferences.Add(collider);
      }

      // polygon colliders
      foreach (var c in FindLocalObjects<QuantumStaticPolygonCollider2D>(scene)) {
        c.BeforeBake();

        if (c.BakeAsStaticEdges2D) {
          for (var i = 0; i < c.Vertices.Length; i++) {
            var staticEdge = BakeStaticEdge2D(c.transform, c.PositionOffset, c.RotationOffset, c.Vertices[i], c.Vertices[(i + 1) % c.Vertices.Length], c.Height, c.Settings, staticCollider2DList.Count);
            staticCollider2DList.Add(staticEdge);
            data.StaticCollider2DReferences.Add(c);
          }

          continue;
        }

        var s = c.transform.localScale;
        var vertices = c.Vertices.Select(x => {
          var v = x.ToUnityVector3();
          return new Vector3(v.x * s.x, v.y * s.y, v.z * s.z);
        }).Select(x => x.ToFPVector2()).ToArray();
        if (FPVector2.IsClockWise(vertices)) {
          FPVector2.MakeCounterClockWise(vertices);
        }


        var normals        = FPVector2.CalculatePolygonNormals(vertices);
        var rotation       = c.transform.rotation.ToFPRotation2D() + c.RotationOffset.FlipRotation() * FP.Deg2Rad;
        var positionOffset = FPVector2.Rotate(FPVector2.CalculatePolygonCentroid(vertices), rotation);

        staticCollider2DList.Add(new MapStaticCollider2D {
          Position = c.transform.TransformPoint(c.PositionOffset.ToUnityVector3()).ToFPVector2() + positionOffset,
          Rotation = rotation,
#if QUANTUM_XY
        VerticalOffset = -c.transform.position.z.ToFP(),
        Height = c.Height * s.z.ToFP(),
#else
          VerticalOffset = c.transform.position.y.ToFP(),
          Height         = c.Height * s.y.ToFP(),
#endif
          PhysicsMaterial = c.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(c.gameObject, c.Settings, staticCollider2DList.Count),
          Layer           = c.gameObject.layer,

          // polygon
          ShapeType = Shape2DType.Polygon,
          PolygonCollider = new MapStaticCollider2DPolygonData() {
            Vertices = FPVector2.RecenterPolygon(vertices),
            Normals = normals,
          },
        });

        data.StaticCollider2DReferences.Add(c);
      }

      // edge colliders
      foreach (var c in FindLocalObjects<QuantumStaticEdgeCollider2D>(scene)) {
        c.BeforeBake();

        staticCollider2DList.Add(BakeStaticEdge2D(c.transform, c.PositionOffset, c.RotationOffset, c.VertexA, c.VertexB, c.Height, c.Settings, staticCollider2DList.Count));
        data.StaticCollider2DReferences.Add(c);
      }

      // box colliders
      foreach (var collider in FindLocalObjects<QuantumStaticBoxCollider2D>(scene)) {
        collider.BeforeBake();

        var e = collider.Size.ToUnityVector3();
        var s = collider.transform.lossyScale;

        e.x *= s.x;
        e.y *= s.y;
        e.z *= s.z;

        staticCollider2DList.Add(new MapStaticCollider2D {
          Position = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector3()).ToFPVector2(),
          Rotation = collider.transform.rotation.ToFPRotation2D() + collider.RotationOffset.FlipRotation() * FP.Deg2Rad,
#if QUANTUM_XY
        VerticalOffset = -collider.transform.position.z.ToFP(),
        Height = collider.Height * s.z.ToFP(),
#else
          VerticalOffset = collider.transform.position.y.ToFP(),
          Height         = collider.Height * s.y.ToFP(),
#endif
          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider2DList.Count),
          Layer           = collider.gameObject.layer,

          // polygon
          ShapeType  = Shape2DType.Box,
          BoxExtents = e.ToFPVector2() * FP._0_50
        });

        data.StaticCollider2DReferences.Add(collider);
      }

      data.Asset.StaticColliders2D = staticCollider2DList.ToArray();
#endif

      // 3D statics

      // clear existing colliders
      var staticCollider3DList = new List<MapStaticCollider3D>();

      // clear on mono behaviour and assets
      data.Asset.CollidersManagedTriangles = new SortedDictionary<int, MeshTriangleVerticesCcw>();
      data.Asset.StaticColliders3D = Array.Empty<MapStaticCollider3D>();

      // initialize collider references, add default null on offset 0
      data.StaticCollider3DReferences = new List<MonoBehaviour>();

#if QUANTUM_ENABLE_PHYSICS3D && !QUANTUM_DISABLE_PHYSICS3D

      // sphere colliders
      foreach (var collider in FindLocalObjects<QuantumStaticSphereCollider3D>(scene)) {
        collider.BeforeBake();

        var scale = collider.transform.lossyScale;
        var radiusScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);

        var rot = collider.transform.rotation.ToFPQuaternion();
        staticCollider3DList.Add(new MapStaticCollider3D {
          Position        = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector3()).ToFPVector3(),
          Rotation        = rot,
          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider3DList.Count),

          // circle
          ShapeType    = Shape3DType.Sphere,
          SphereRadius = FP.FromFloat_UNSAFE(collider.Radius.AsFloat * radiusScale)
        });

        data.StaticCollider3DReferences.Add(collider);
      }

      // capsule colliders
      foreach (var collider in FindLocalObjects<QuantumStaticCapsuleCollider3D>(scene)) {
        collider.BeforeBake();

        var scale = collider.transform.lossyScale;
        float radiusScale = Mathf.Max(scale.x, scale.z);
        float heightScale = scale.y;
        

        staticCollider3DList.Add(new MapStaticCollider3D {
          Position        = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector3()).ToFPVector3(),
          Rotation        = FPQuaternion.Euler(collider.transform.rotation.eulerAngles.ToFPVector3() + collider.RotationOffset),

          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider3DList.Count),

          // capsule
          ShapeType    = Shape3DType.Capsule,
          CapsuleRadius = FP.FromFloat_UNSAFE(collider.Radius.AsFloat * radiusScale),
          CapsuleHeight = FP.FromFloat_UNSAFE(collider.Height.AsFloat * heightScale)
        });

        data.StaticCollider3DReferences.Add(collider);
      }

      // box colliders
      foreach (var collider in FindLocalObjects<QuantumStaticBoxCollider3D>(scene)) {
        collider.BeforeBake();

        var e = collider.Size.ToUnityVector3();
        var s = collider.transform.lossyScale;

        e.x *= s.x;
        e.y *= s.y;
        e.z *= s.z;

        staticCollider3DList.Add(new MapStaticCollider3D {
          Position        = collider.transform.TransformPoint(collider.PositionOffset.ToUnityVector3()).ToFPVector3(),
          Rotation        = FPQuaternion.Euler(collider.transform.rotation.eulerAngles.ToFPVector3() + collider.RotationOffset),
          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider3DList.Count),

          // box
          ShapeType  = Shape3DType.Box,
          BoxExtents = e.ToFPVector3() * FP._0_50
        });

        data.StaticCollider3DReferences.Add(collider);
      }

      var meshes = FindLocalObjects<QuantumStaticMeshCollider3D>(scene);

      // static 3D mesh colliders
      foreach (var collider in meshes) {
        // our assumed static collider index
        var staticColliderIndex = staticCollider3DList.Count;

        // bake mesh
        if (collider.Bake(staticColliderIndex)) {
          Assert.Check(staticColliderIndex == staticCollider3DList.Count);

          // add on list
          staticCollider3DList.Add(new MapStaticCollider3D {
            Position                   = collider.transform.position.ToFPVector3(),
            Rotation                   = collider.transform.rotation.ToFPQuaternion(),
            PhysicsMaterial            = collider.Settings.PhysicsMaterial,
            SmoothSphereMeshCollisions = collider.SmoothSphereMeshCollisions,

            // mesh
            ShapeType  = Shape3DType.Mesh,
            StaticData = GetStaticData(collider.gameObject, collider.Settings, staticColliderIndex),
          });

          // add to static collider lookup
          data.StaticCollider3DReferences.Add(collider);

          // add to static collider data
          data.Asset.CollidersManagedTriangles.Add(staticColliderIndex, collider.MeshTriangles);
        }
      }

#endif

      var terrains = FindLocalObjects<QuantumStaticTerrainCollider3D>(scene);

      // terrain colliders
      foreach (var terrain in terrains) {
        // our assumed static collider index
        var staticColliderIndex = staticCollider3DList.Count;

        // bake terrain
        terrain.Bake();

        // add to 3d collider list
        staticCollider3DList.Add(new MapStaticCollider3D {
          Position                   = default(FPVector3),
          Rotation                   = FPQuaternion.Identity,
          PhysicsMaterial            = terrain.Asset.PhysicsMaterial,
          SmoothSphereMeshCollisions = terrain.SmoothSphereMeshCollisions,

          // terrains are meshes
          ShapeType = Shape3DType.Mesh,

          // static data for terrain
          StaticData = GetStaticData(terrain.gameObject, terrain.Settings, staticColliderIndex),
        });

        // add to 
        data.StaticCollider3DReferences.Add(terrain);

        // load all triangles
        terrain.Asset.Bake(staticColliderIndex);

        // add to static collider data
        data.Asset.CollidersManagedTriangles.Add(staticColliderIndex, terrain.Asset.MeshTriangles);
      }

      // this has to hold
      Assert.Check(staticCollider3DList.Count == data.StaticCollider3DReferences.Count);

      // assign collider 3d array
      data.Asset.StaticColliders3D = staticCollider3DList.ToArray();

      // clear this so it's not re-used by accident
      staticCollider3DList = null;

      BakeMeshes(data, inEditor);

      if (inEditor) {
        QuantumEditorLog.LogImport($"Baked {data.Asset.StaticColliders2D.Length} 2D static colliders");
        QuantumEditorLog.LogImport($"Baked {data.Asset.StaticColliders3D.Length} 3D static primitive colliders");
        QuantumEditorLog.LogImport($"Baked {data.Asset.CollidersManagedTriangles.Select(x => x.Value.Triangles.Length).Sum()} 3D static triangles");
      }
    }

    public static void BakePrototypes(QuantumMapData data) {
      var scene = data.gameObject.scene;
      Debug.Assert(scene.IsValid());

      data.MapEntityReferences.Clear();

      var components = new List<QuantumUnityComponentPrototype>();
      var prototypes = FindLocalObjects<QuantumEntityPrototype>(scene).ToArray();
      SortBySiblingIndex(prototypes);

      var converter = new QuantumEntityPrototypeConverter(data, prototypes);
      var buffer    = new List<ComponentPrototype>();
      
      ref var mapEntities = ref data.Asset.MapEntities;
      Array.Resize(ref mapEntities, prototypes.Length);
      Array.Clear(mapEntities, 0, mapEntities.Length);
      
#if UNITY_EDITOR
      // this is needed to clear up managed references
      using var so = new SerializedObject(data.Asset);
      so.Update();
#endif

      for (int i = 0; i < prototypes.Length; ++i) {
        var prototype = prototypes[i];

        prototype.GetComponents(components);
        
        prototype.PreSerialize();
        prototype.SerializeImplicitComponents(buffer, out var selfView);

        foreach (var component in components) {
          component.Refresh();
          var proto = component.CreatePrototype(converter);
          buffer.Add(proto);
        }

        mapEntities[i] = ComponentPrototypeSet.FromArray(buffer.ToArray());
        data.MapEntityReferences.Add(selfView);
        buffer.Clear();
        
#if UNITY_EDITOR
        UpdateManagedReferenceIds(data.Asset, prototype, mapEntities[i].Components);
#endif
      }
      
#if UNITY_EDITOR
      so.Update();
      so.ApplyModifiedProperties();
#endif
    }
    
    private static Lazy<Type[]> CallbackTypes = new Lazy<Type[]>(() => {
      List<Type> callbackTypes = new List<Type>();

      if (Application.isEditor) {
#if UNITY_EDITOR
        foreach (var t in TypeCache.GetTypesDerivedFrom(typeof(MapDataBakerCallback))) {
          var assemblyAttribute = t.Assembly.GetCustomAttribute<QuantumMapBakeAssemblyAttribute>();
          if (assemblyAttribute == null) {
            Debug.LogWarning($"{nameof(MapDataBakerCallback)} found ({t.FullName}) in assembly {t.Assembly.FullName} which is not marked with {nameof(QuantumMapBakeAssemblyAttribute)}. " +
                             $"It will be ignored and not used for baking. Please mark the assembly with {nameof(QuantumMapBakeAssemblyAttribute)} if you want to use this callback or " +
                             $"if you want to get rid of this warning.");
            continue;
          }

          if (assemblyAttribute.Ignore) {
            continue;
          }
          callbackTypes.Add(t);
        }
#endif
      } else {
        var markedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
          .Where(x => x.GetCustomAttribute<QuantumMapBakeAssemblyAttribute>()?.Ignore == false);

        foreach (var asm in markedAssemblies) {
          foreach (var t in asm.GetLoadableTypes()) {
            if (!t.IsSubclassOf(typeof(MapDataBakerCallback))) {
              continue;
            }

            callbackTypes.Add(t);
          }
        }
      }
      
      // remove non-instantiable types
      callbackTypes.RemoveAll(t => t.IsAbstract || t.IsGenericTypeDefinition);
      
      callbackTypes.Sort((a, b) => {
        var orderA = a.GetCustomAttribute<MapDataBakerCallbackAttribute>()?.InvokeOrder ?? 0;
        var orderB = b.GetCustomAttribute<MapDataBakerCallbackAttribute>()?.InvokeOrder ?? 0;
        return orderA - orderB;
      });

      return callbackTypes.ToArray();
    });

    private static void InvokeCallbacks(string callbackName, QuantumMapData data, BuildTrigger buildTrigger, QuantumMapDataBakeFlags bakeFlags) {
      foreach (var callback in CallbackTypes.Value) {
        try {
          switch (callbackName) {
            case "OnBeforeBake":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBeforeBake(data, buildTrigger, bakeFlags);
              break;
            default:
              Log.Warn($"Callback `{callbackName}` not found");
              break;
          }
        } catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }

    private static void InvokeCallbacks(string callbackName, QuantumMapData data) {
      foreach (var callback in CallbackTypes.Value) {
        try {
          switch (callbackName) {
            case "OnBeforeBake":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBeforeBake(data);
              break;
            case "OnBake":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBake(data);
              break;
            case "OnBeforeBakeNavMesh":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBeforeBakeNavMesh(data);
              break;
            case "OnBakeNavMesh":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBakeNavMesh(data);
              break;
            default:
              Log.Warn($"Callback `{callbackName}` not found");
              break;
          }
        } catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }

    private static void InvokeCallbacks(string callbackName, QuantumMapData data, List<NavMeshBakeData> bakeData) {
      foreach (var callback in CallbackTypes.Value) {
        try {
          switch (callbackName) {
            case "OnCollectNavMeshBakeData":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnCollectNavMeshBakeData(data, bakeData);
              break;
            default:
              Log.Warn($"Callback `{callbackName}` not found");
              break;
          }
        } catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }

    private static void InvokeCallbacks(string callbackName, QuantumMapData data, List<Quantum.NavMesh> navmeshes) {
      foreach (var callback in CallbackTypes.Value) {
        try {
          switch (callbackName) {
            case "OnCollectNavMeshes":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnCollectNavMeshes(data, navmeshes);
              break;
            default:
              Log.Warn($"Callback `{callbackName}` not found");
              break;
          }
        } catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }

#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI

    static IEnumerable<Quantum.NavMesh> BakeNavMeshesLoop(QuantumMapData data) {

#if UNITY_EDITOR
      QuantumNavMesh.InvalidateGizmos();
#endif
      
      var scene = data.gameObject.scene;
      Debug.Assert(scene.IsValid());

      var allBakeData = new List<NavMeshBakeData>();

      // Collect unity navmeshes
      {
        var unityNavmeshes = data.GetComponentsInChildren<QuantumMapNavMeshUnity>().ToList();

        // The sorting is important to always generate the same order of regions name list.
        unityNavmeshes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        for (int i = 0; i < unityNavmeshes.Count; i++) {
          // If NavMeshSurface installed, this will deactivate non linked surfaces 
          // to make the CalculateTriangulation work only with the selected Unity navmesh.
          List<GameObject> deactivatedObjects = new List<GameObject>();

          try {
            if (unityNavmeshes[i].NavMeshSurfaces != null && unityNavmeshes[i].NavMeshSurfaces.Length > 0) {
#if QUANTUM_ENABLE_AI_NAVIGATION
                var surfaces = FindLocalObjects<Unity.AI.Navigation.NavMeshSurface>(scene);
                foreach (var surface in surfaces) {
                  if (unityNavmeshes[i].NavMeshSurfaces.Contains(surface.gameObject) == false) {
                    surface.gameObject.SetActive(false);
                    deactivatedObjects.Add(surface.gameObject);
                  }
                }
#endif
            }

            var bakeData = QuantumNavMesh.ImportFromUnity(scene, unityNavmeshes[i].Settings, unityNavmeshes[i].name);
            if (bakeData == null) {
              Debug.LogErrorFormat("Could not import navmesh '{0}'", unityNavmeshes[i].name);
            } else {
              bakeData.Name                            = unityNavmeshes[i].name;
              bakeData.AgentRadius                     = QuantumNavMesh.FindSmallestAgentRadius(unityNavmeshes[i].NavMeshSurfaces);
              bakeData.EnableQuantum_XY                = unityNavmeshes[i].Settings.EnableQuantum_XY;
              bakeData.ClosestTriangleCalculation      = unityNavmeshes[i].Settings.ClosestTriangleCalculation;
              bakeData.ClosestTriangleCalculationDepth = unityNavmeshes[i].Settings.ClosestTriangleCalculationDepth;
              allBakeData.Add(bakeData);
            }
          } catch (Exception exn) {
            Debug.LogException(exn);
          }

          foreach (var go in deactivatedObjects) {
            go.SetActive(true);
          }
        }
      }

      // Collect custom bake data
      InvokeCallbacks("OnCollectNavMeshBakeData", data, allBakeData);

      // Bake all collected bake data
      for (int i = 0; i < allBakeData.Count; i++) {
        var navmesh  = default(Quantum.NavMesh);
        var bakeData = allBakeData[i];
        if (bakeData == null) {
          Debug.LogErrorFormat("Navmesh bake data at index {0} is null", i);
          continue;
        }

        try {
          navmesh = NavMeshBaker.BakeNavMesh(data.Asset, bakeData);
          Debug.LogFormat("Baking Quantum NavMesh '{0}' complete ({1}/{2})", bakeData.Name, i + 1, allBakeData.Count);
        } catch (Exception exn) {
          Debug.LogException(exn);
        }

        if (navmesh != null) {
          yield return navmesh;
        } else {
          Debug.LogErrorFormat("Baking Quantum NavMesh '{0}' failed", bakeData.Name);
        }
      }
    }

#endif

    private static void SortBySiblingIndex<T>(T[] array) where T : Component {
      // sort by sibling indices; this should be uniform across machines
      List<int> list0 = new List<int>();
      List<int> list1 = new List<int>();
      Array.Sort(array, (a, b) => CompareLists(GetSiblingIndexPath(a.transform, list0), GetSiblingIndexPath(b.transform, list1)));
    }

    static List<int> GetSiblingIndexPath(Transform t, List<int> buffer) {
      buffer.Clear();
      while (t != null) {
        buffer.Add(t.GetSiblingIndex());
        t = t.parent;
      }

      buffer.Reverse();
      return buffer;
    }

    static int CompareLists(List<int> left, List<int> right) {
      while (left.Count > 0 && right.Count > 0) {
        if (left[0] < right[0]) {
          return -1;
        }

        if (left[0] > right[0]) {
          return 1;
        }

        left.RemoveAt(0);
        right.RemoveAt(0);
      }

      return 0;
    }

#if QUANTUM_ENABLE_PHYSICS2D && !QUANTUM_DISABLE_PHYSICS2D
    static MapStaticCollider2D BakeStaticEdge2D(Transform t, FPVector2 positionOffset, FP rotationOffset, FPVector2 vertexA, FPVector2 vertexB, FP height, QuantumStaticColliderSettings settings, int colliderId) {
      QuantumStaticEdgeCollider2D.GetEdgeGizmosSettings(t, positionOffset, rotationOffset, vertexA, vertexB, height, out var start, out var end, out var scaledHeight);

      var startToEnd = end - start;

      var pos = (start + end) / 2.0f;
      var rot = Quaternion.FromToRotation(Vector3.right, startToEnd);

      return new MapStaticCollider2D {
        Position = pos.ToFPVector2(),
        Rotation = rot.ToFPRotation2D(),
#if QUANTUM_XY
      VerticalOffset = -t.position.z.ToFP(),
      Height = scaledHeight.ToFP(),
#else
        VerticalOffset = t.position.y.ToFP(),
        Height         = scaledHeight.ToFP(),
#endif
        PhysicsMaterial = settings.PhysicsMaterial,
        StaticData      = GetStaticData(t.gameObject, settings, colliderId),
        Layer           = t.gameObject.layer,

        // edge
        ShapeType  = Shape2DType.Edge,
        EdgeExtent = (startToEnd.magnitude / 2.0f).ToFP(),
      };
    }
#endif

    public static List<T> FindLocalObjects<T>(Scene scene) where T : Component {
      List<T> partialResult = new List<T>();
      List<T> fullResult    = new List<T>();
      foreach (var gameObject in scene.GetRootGameObjects()) {
        // GetComponentsInChildren seems to clear the list first, but we're not going to depend
        // on this implementation detail
        if (!gameObject.activeInHierarchy)
          continue;
        partialResult.Clear();
        gameObject.GetComponentsInChildren(partialResult);
        fullResult.AddRange(partialResult);
      }

      return fullResult;
    }

    public static List<Component> FindLocalObjects(Scene scene, Type type) {
      List<Component> result = new List<Component>();
      foreach (var gameObject in scene.GetRootGameObjects()) {
        if (!gameObject.activeInHierarchy)
          continue;
        foreach (var component in gameObject.GetComponentsInChildren(type)) {
          result.Add(component);
        }
      }

      return result;
    }
    
#if UNITY_EDITOR
    private static void UpdateManagedReferenceIds(Quantum.Map context, QuantumEntityPrototype prototype, ComponentPrototype[] componentPrototypes) {
      
      var  id = GlobalObjectId.GetGlobalObjectIdSlow(prototype);

      uint hash = 0;
      
      hash = GetHashCodeDeterministic(id.identifierType, hash);
      hash = GetHashCodeDeterministic(id.assetGUID, hash);
      hash = GetHashCodeDeterministic(id.targetObjectId, hash);
      hash = GetHashCodeDeterministic(id.targetPrefabId, hash);
      
      // leave the highest bit intact, some negative refIds are used for special cases by Unity
      long refIdBase = (long)hash << 31; 
      for (int i = 0; i < componentPrototypes.Length; ++i) {
#if UNITY_2022_2_OR_NEWER
        UnityEngine.Serialization.ManagedReferenceUtility.SetManagedReferenceIdForObject(context, componentPrototypes[i], refIdBase + i);
#else
        SerializationUtility.SetManagedReferenceIdForObject(context, componentPrototypes[i], refIdBase + i);
#endif
      }
    }
    
    private static unsafe uint GetHashCodeDeterministic<T>(T data, uint initialHash = 0) where T : unmanaged {
      var hash = initialHash;
      
      var ptr  = (byte*)&data;
      for (var i = 0; i < sizeof(T); ++i) {
        hash = hash * 31 + ptr[i];
      }
      return hash;
    }
#endif
    
#if QUANTUM_MAP_BAKER_TRACE_ENABLED
    public readonly struct _LogScope : IDisposable {
      private readonly Stopwatch _stopwatch;
      private readonly string    _msg;
      private readonly bool      _trace;
      
      public _LogScope(string msg, bool trace) {
        _msg       = msg;
        _stopwatch = Stopwatch.StartNew();
        _trace     = trace;
      }
      
      public void Dispose() {
        if (_trace) {
          UnityEngine.Debug.Log($"{_msg} ({_stopwatch.Elapsed.TotalMilliseconds:0.00}ms)");
        } else {
          UnityEngine.Debug.Log($"{_msg} ({_stopwatch.Elapsed.TotalMilliseconds:0.00}ms)");  
        }
      }
    }
    
    public static _LogScope TraceScope(string msg) => new _LogScope(msg, true);
#else
    public static IDisposable TraceScope(string msg) => null;
#endif
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/QuantumMonoBehaviour.Partial.cs

namespace Quantum {
  partial class QuantumMonoBehaviour {
#if UNITY_EDITOR
    // TODO: this should be moved somewhere or renamed; the whole idea is that this stuff
    // is only used by behaviours, whereas for simulation gizmos runner can provide its own
    protected QuantumGameGizmosSettings GlobalGizmosSettings => QuantumGameGizmosSettingsScriptableObject.Global.Settings;
#endif
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumNavMesh.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using Photon.Analyzer;
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.SceneManagement;
  using Object = System.Object;
  using Plane = UnityEngine.Plane;
#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI
  using UnityEngine.AI;
  using static Quantum.QuantumNavMesh.DelaunayTriangulation;
#endif

  public partial class QuantumNavMesh {

    #region Importing From Unity

#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI
    public struct Vertex {
      public String        Id;
      public Vector3Double Position;

      public NavMeshBakeDataVertex Convert() {
        return new NavMeshBakeDataVertex {
          Id       = this.Id,
          Position = this.Position.AsFPVector()
        };
      }
    }
#endif

    [StaticField(StaticFieldResetMode.None)]
    public static float DefaultMinAgentRadius = 0.25f;

    [Serializable]
    public class ImportSettings {
      [Tooltip("The Unity NavMesh is a collection of non - connected triangles, this option is very important and combines shared vertices.")]
      public bool WeldIdenticalVertices = true;

      [Tooltip("Don't make the epsilon too small, vertices to fuse are missed, also don't make the value too big as it will deform your navmesh. Min = float.Epsilon.")]
      [Min(float.Epsilon)]
      [DrawIf("WeldIdenticalVertices", true)]
      public float WeldVertexEpsilon = 0.0001f;

      [Tooltip("Post processes imported Unity navmesh with a Delaunay triangulation to reduce long triangles.")]
      public bool DelaunayTriangulation = false;

      [Tooltip("In 3D the triangulation can deform the navmesh on slopes, check this option to restrict the triangulation to triangles that lie in the same plane.")]
      [DrawIf("DelaunayTriangulation", true)]
      public bool DelaunayTriangulationRestrictToPlanes = false;

      [Tooltip("Sometimes vertices are lying on other triangle edges, this will lead to unwanted borders being detected, this option splits those vertices.")]
      public bool FixTrianglesOnEdges = true;

      [Tooltip("Larger scaled navmeshes may require to increase this value (e.g. 0.001) when false-positive borders are detected. Min = float.Epsilon.")]
      [Min(float.Epsilon)]
      [DrawIf("FixTrianglesOnEdges", true)]
      public float FixTrianglesOnEdgesEpsilon = float.Epsilon;

      [Tooltip("Make the height offset considerably larger than FixTrianglesOnEdgesEpsilon to better detect degenerate triangles. Is the navmesh becomes deformed chose a smaller epsilon. . Min = float.Epsilon. Default is 0.05.")]
      [Min(float.Epsilon)]
      [DrawIf("FixTrianglesOnEdges", true)]
      public float FixTrianglesOnEdgesHeightEpsilon = 0.05f;

      [Tooltip("Automatically correct navmesh link position to the closest triangle by searching this distance (default is 0).")]
      public float LinkErrorCorrection = 0.0f;

      [Tooltip("SpiralOut will be considerably faster but fallback triangles can be null.")]
      public NavMeshBakeDataFindClosestTriangle ClosestTriangleCalculation = NavMeshBakeDataFindClosestTriangle.SpiralOut;

      [Tooltip("Number of cells to search triangles in neighbors.")]
      [DrawIf("ClosestTriangleCalculation", (long)NavMeshBakeDataFindClosestTriangle.BruteForce, CompareOperator.NotEqual)]
      public int ClosestTriangleCalculationDepth = 3;

      [Tooltip("Activate this and the navmesh baking will flip Y and Z to support navmeshes generated in the XY plane.")]
      public bool EnableQuantum_XY;

      [Tooltip("The agent radius that the navmesh is build for. The value is retrieved from Unity settings when baking in Editor.")]
      public FP MinAgentRadius = FP._0_25;

      [Tooltip("Toggle the Quantum region import.")]
      public bool ImportRegions = true;

      [Tooltip("The artificial margin is necessary because the Unity NavMesh does not fit the source size very well. The value is added to the navmesh area and checked against all Quantum Region scripts to select the correct region id.")]
      [DrawIf("ImportRegions", true)]
      public float RegionDetectionMargin = 0.4f;

      public List<Int32> RegionAreaIds;
    }

#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI

    public static class ImportUtils {
      public static void WeldIdenticalVertices(ref Vertex[] vertices, ref NavMeshBakeDataTriangle[] triangles, float cleanupEpsilon, Action<float> reporter) {
        int[] vertexRemapTable = new int[vertices.Length];
        for (int i = 0; i < vertexRemapTable.Length; ++i) {
          vertexRemapTable[i] = i;
        }

        for (int i = 0; i < vertices.Length; ++i) {
          reporter.Invoke(i / (float)vertices.Length);
          var v = vertices[i].Position;

          for (int j = i + 1; j < vertices.Length; ++j) {
            if (j != vertexRemapTable[j]) {
              continue;
            }

            var v2 = vertices[j].Position;
            if (Math.Abs(Vector3Double.SqrMagnitude(v2 - v)) <= cleanupEpsilon) {
              vertexRemapTable[j] = i;
            }
          }
        }

        for (int i = 0; i < triangles.Length; ++i) {
          for (int v = 0; v < 3; v++) {
            triangles[i].VertexIds[v] =
              vertexRemapTable[triangles[i].VertexIds[v]];
          }
        }
      }

      public static void RemoveUnusedVertices(ref Vertex[] vertices, ref NavMeshBakeDataTriangle[] triangles, Action<float> reporter) {
        var   newVertices = new List<Vertex>();
        int[] remapArray  = new int[vertices.Length];
        for (int i = 0; i < remapArray.Length; ++i) {
          remapArray[i] = -1;
        }

        for (int t = 0; t < triangles.Length; ++t) {
          reporter.Invoke(t / (float)triangles.Length);
          for (int v = 0; v < 3; v++) {
            int newIndex = remapArray[triangles[t].VertexIds[v]];
            if (newIndex < 0) {
              newIndex                              = newVertices.Count;
              remapArray[triangles[t].VertexIds[v]] = newIndex;
              newVertices.Add(vertices[triangles[t].VertexIds[v]]);
            }

            triangles[t].VertexIds[v] = newIndex;
          }
        }

        //Debug.Log("Removed Unused Vertices: " + (vertices.Length - newVertices.Count));

        vertices = newVertices.ToArray();
      }

      public static void ImportRegions(Scene scene, ref Vertex[] vertices, ref NavMeshBakeDataTriangle[] triangles, int t, ref List<string> regionMap, float regionDetectionMargin) {
        // Expand the triangle until we have an isolated island containing all connected triangles of the same region
        HashSet<int> island    = new HashSet<int>();
        HashSet<int> verticies = new HashSet<int>();
        island.Add(t);
        verticies.Add(triangles[t].VertexIds[0]);
        verticies.Add(triangles[t].VertexIds[1]);
        verticies.Add(triangles[t].VertexIds[2]);
        bool isIslandComplete = false;
        while (!isIslandComplete) {
          isIslandComplete = true;
          for (int j = 0; j < triangles.Length; j++) {
            if (triangles[t].Area == triangles[j].Area && !island.Contains(j)) {
              for (int v = 0; v < 3; v++) {
                if (verticies.Contains(triangles[j].VertexIds[v])) {
                  island.Add(j);
                  verticies.Add(triangles[j].VertexIds[0]);
                  verticies.Add(triangles[j].VertexIds[1]);
                  verticies.Add(triangles[j].VertexIds[2]);
                  isIslandComplete = false;
                  break;
                }
              }
            }
          }
        }

        // Go through all MapNavMeshRegion scripts in the scene and check if all vertices of the islands
        // are within its bounds. Use the smallest possible bounds/region found. Use the RegionIndex from that for all triangles.
        if (island.Count > 0) {
          string regionId             = string.Empty;
          FP     regionCost           = FP._1;
          float  smallestRegionBounds = float.MaxValue;
          var    regions              = QuantumMapDataBaker.FindLocalObjects<QuantumNavMeshRegion>(scene);
          foreach (var region in regions) {
            if (region.CastRegion != QuantumNavMeshRegion.RegionCastType.CastRegion) {
              continue;
            }

            var meshRenderer = region.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null) {
              Debug.LogErrorFormat("MeshRenderer missing on MapNavMeshRegion object {0} with active RegionCasting", region.name);
            } else {
              var bounds = region.gameObject.GetComponent<MeshRenderer>().bounds;
              // Grow the bounds, because the generated map is not exact
              bounds.Expand(regionDetectionMargin);
              bool isInsideBounds = true;
              foreach (var triangleIndex in island) {
                for (int v = 0; v < 3; v++) {
                  var position = vertices[triangles[triangleIndex].VertexIds[v]].Position;
                  if (bounds.Contains(position.AsVector()) == false) {
                    isInsideBounds = false;
                    break;
                  }
                }
              }

              if (isInsideBounds) {
                float size = bounds.extents.sqrMagnitude;
                if (size < smallestRegionBounds) {
                  smallestRegionBounds = size;
                  regionId             = region.Id;
                  regionCost           = region.Cost;

                  if (region.OverwriteCost == false) {
                    // Grab the most recent area cost from Unity (ignore the one in the scene)
                    regionCost = UnityEngine.AI.NavMesh.GetAreaCost(triangles[t].Area).ToFP();
                  }
                }
              }
            }
          }

          // Save the toggle region index on the triangles imported from Unity
          if (string.IsNullOrEmpty(regionId) == false) {
            if (regionMap.Contains(regionId) == false) {
              if (regionMap.Count >= Navigation.Constants.MaxRegions) {
                // Still add to region map, but it won't be set on the triangles.
                Debug.LogErrorFormat("Failed to create region '{0}' because Quantum max region ({1}) reached. Reduce the number of regions.", regionId, Navigation.Constants.MaxRegions);
              }

              regionMap.Add(regionId);
            }

            foreach (var triangleIndex in island) {
              triangles[triangleIndex].RegionId = regionId;
              triangles[triangleIndex].Cost     = regionCost;
            }
          } else {
            Debug.LogWarningFormat("A triangle island (count = {0}) can not be matched with any region bounds, try to increase the RegionDetectionMargin.\n Triangle Ids: {1}", island.Count, String.Join(", ", island.Select(sdfdsf => sdfdsf.ToString()).ToArray()));
          }
        }
      }

      public static void FixTrianglesOnEdges(ref Vertex[] vertices, ref NavMeshBakeDataTriangle[] triangles, int t, int v0, float epsilon, float epsilonHeight) {
        int v1 = (v0 + 1) % 3;
        int vOther;
        int otherTriangle = FindTriangleOnEdge(ref vertices, ref triangles, t, triangles[t].VertexIds[v0], triangles[t].VertexIds[v1], epsilon, epsilonHeight, out vOther);
        if (otherTriangle >= 0) {
          SplitTriangle(ref triangles, t, v0, triangles[otherTriangle].VertexIds[vOther]);
          //Debug.LogFormat("Split triangle {0} at position {1}", t, vertices[triangles[otherTriangle].VertexIds[vOther]].Position);
        }
      }

      public static int FindTriangleOnEdge(ref Vertex[] vertices, ref NavMeshBakeDataTriangle[] triangles, int tri, int v0, int v1, float epsilon, float epsilonHeight, out int triangleVertexIndex) {
        triangleVertexIndex = -1;
        for (int t = 0; t < triangles.Length; ++t) {
          if (t == tri) {
            continue;
          }

          // Triangle shares at least one vertex?
          if (triangles[t].VertexIds[0] == v0 || triangles[t].VertexIds[1] == v0 ||
              triangles[t].VertexIds[2] == v0 || triangles[t].VertexIds[0] == v1 ||
              triangles[t].VertexIds[1] == v1 || triangles[t].VertexIds[2] == v1) {
            if (triangles[t].VertexIds[0] == v0 || triangles[t].VertexIds[1] == v0 || triangles[t].VertexIds[2] == v0) {
              if (triangles[t].VertexIds[0] == v1 || triangles[t].VertexIds[1] == v1 || triangles[t].VertexIds[2] == v1) {
                // Triangle shares two vertices, not interested in that
                return -1;
              }
            }

            if (Vector3Double.IsPointBetween(vertices[triangles[t].VertexIds[0]].Position, vertices[v0].Position, vertices[v1].Position, epsilon, epsilonHeight)) {
              // Returns the triangle that has a vertex on the provided segment and the vertex index that lies on it
              triangleVertexIndex = 0;
              return t;
            }

            if (Vector3Double.IsPointBetween(vertices[triangles[t].VertexIds[1]].Position, vertices[v0].Position, vertices[v1].Position, epsilon, epsilonHeight)) {
              triangleVertexIndex = 1;
              return t;
            }

            if (Vector3Double.IsPointBetween(vertices[triangles[t].VertexIds[2]].Position, vertices[v0].Position, vertices[v1].Position, epsilon, epsilonHeight)) {
              triangleVertexIndex = 2;
              return t;
            }
          }
        }

        return -1;
      }

      public static void SplitTriangle(ref NavMeshBakeDataTriangle[] triangles, int t, int v0, int vNew) {
        // Split edge is between vertex index 0 and 1
        int v1 = (v0 + 1) % 3;
        // Vertex index 2 is opposite of split edge
        int v2 = (v0 + 2) % 3;

        var newTriangle = new NavMeshBakeDataTriangle {
          Area      = triangles[t].Area,
          RegionId  = triangles[t].RegionId,
          Cost      = triangles[t].Cost,
          VertexIds = new int[3]
        };

        // Map new triangle
        newTriangle.VertexIds[0] = vNew;
        newTriangle.VertexIds[1] = triangles[t].VertexIds[v1];
        newTriangle.VertexIds[2] = triangles[t].VertexIds[v2];
        ArrayUtils.Add(ref triangles, newTriangle);

        // Remap old triangle
        triangles[t].VertexIds[v1] = vNew;
      }
    }

    public static NavMeshBakeData ImportFromUnity(Scene scene, ImportSettings settings, string name) {
      var result = new NavMeshBakeData();

      using (var progressBar = new ProgressBar("Importing Unity NavMesh", true)) {
        progressBar.Info = "Calculate Triangulation";
        var unityNavMeshTriangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();

        if (unityNavMeshTriangulation.vertices.Length == 0) {
          Debug.LogError("Unity NavMesh not found");
          return null;
        }

        progressBar.Info = "Loading Vertices";
        var Vertices = new Vertex[unityNavMeshTriangulation.vertices.Length];
        for (int i = 0; i < Vertices.Length; ++i) {
          progressBar.Progress = i / (float)Vertices.Length;
          Vertices[i].Position = new Vector3Double(unityNavMeshTriangulation.vertices[i]);
        }

        progressBar.Info = "Loading Triangles";
        int triangleCount = unityNavMeshTriangulation.indices.Length / 3;
        var Triangles     = new NavMeshBakeDataTriangle[triangleCount];
        for (int i = 0; i < triangleCount; ++i) {
          progressBar.Progress = i / (float)triangleCount;
          int area      = unityNavMeshTriangulation.areas[i];
          int baseIndex = i * 3;
          Triangles[i] = new NavMeshBakeDataTriangle() {
            VertexIds = new int[] { unityNavMeshTriangulation.indices[baseIndex + 0], unityNavMeshTriangulation.indices[baseIndex + 1], unityNavMeshTriangulation.indices[baseIndex + 2] },
            Area      = area,
            RegionId  = null,
            Cost      = FP._1
          };
        }

        // Weld vertices
        if (settings.WeldIdenticalVertices) {
          progressBar.Info = "Welding Identical Vertices";
          ImportUtils.WeldIdenticalVertices(ref Vertices, ref Triangles, settings.WeldVertexEpsilon, p => progressBar.Progress = p);

          progressBar.Info = "Removing Unused Vertices";
          ImportUtils.RemoveUnusedVertices(ref Vertices, ref Triangles, p => progressBar.Progress = p);
        }

        // Merge vertices that lie on triangle edges
        if (settings.FixTrianglesOnEdges) {
          progressBar.Info = "Fixing Triangles On Edges";
          for (int t = 0; t < Triangles.Length; ++t) {
            progressBar.Progress = t / (float)Triangles.Length;
            for (int v = 0; v < 3; ++v) {
              ImportUtils.FixTrianglesOnEdges(ref Vertices, ref Triangles, t, v, settings.FixTrianglesOnEdgesEpsilon, settings.FixTrianglesOnEdgesHeightEpsilon);
            }
          }

          progressBar.Info = "Removing Unused Vertices";
          ImportUtils.RemoveUnusedVertices(ref Vertices, ref Triangles, p => progressBar.Progress = p);
        }

        if (settings.DelaunayTriangulation) {
          progressBar.Info     = "Delaunay Triangulation";
          progressBar.Progress = 0;
          var progressStep = 0.1f / (float)Triangles.Length;

          var triangles = new List<DelaunayTriangulation.Triangle>();

          for (int i = 0; i < Triangles.Length; i++) {
            progressBar.Progress += progressStep;
            triangles.Add(new DelaunayTriangulation.Triangle {
              v1 = new DelaunayTriangulation.HalfEdgeVertex(Vertices[Triangles[i].VertexIds[0]].Position.AsVector(), Triangles[i].VertexIds[0]),
              v2 = new DelaunayTriangulation.HalfEdgeVertex(Vertices[Triangles[i].VertexIds[1]].Position.AsVector(), Triangles[i].VertexIds[1]),
              v3 = new DelaunayTriangulation.HalfEdgeVertex(Vertices[Triangles[i].VertexIds[2]].Position.AsVector(), Triangles[i].VertexIds[2]),
              t  = i
            });
          }

          progressBar.Progress = 0.1f;
          triangles            = DelaunayTriangulation.TriangulateByFlippingEdges(triangles, settings.DelaunayTriangulationRestrictToPlanes, () => progressBar.Progress = (Mathf.Min(progressBar.Progress + 0.1f, 0.9f)));

          progressBar.Progress = 0.9f;
          foreach (var t in triangles) {
            progressBar.Progress        += progressStep;
            Triangles[t.t].VertexIds[0] =  t.v1.index;
            Triangles[t.t].VertexIds[1] =  t.v2.index;
            Triangles[t.t].VertexIds[2] =  t.v3.index;
          }

          progressBar.Progress = 1;
        }

        // Import regions
        List<string> regions = new List<string>() { "MainArea" };
        if (settings.ImportRegions) {
          progressBar.Info = "Importing Regions";
          for (int t = 0; t < Triangles.Length; t++) {
            progressBar.Progress = t / (float)Triangles.Length;
            if (settings.RegionAreaIds != null && settings.RegionAreaIds.Contains(Triangles[t].Area) && string.IsNullOrEmpty(Triangles[t].RegionId)) {
              ImportUtils.ImportRegions(scene, ref Vertices, ref Triangles, t, ref regions, settings.RegionDetectionMargin);
            }
          }
          for (int t = 0; t < Triangles.Length; t++) {
            if (Triangles[t].RegionId == null) {
              Triangles[t].RegionId = "MainArea";
            }
          }
        }

        // Set all vertex string ids (to work with manual editor)
        {
          progressBar.Info     = "Finalizing Vertices";
          progressBar.Progress = 0.5f;
          for (int v = 0; v < Vertices.Length; v++) {
            Vertices[v].Id = v.ToString();
          }
        }

        // Find links
        var links = new List<NavMeshLinkTemp>();
#if QUANTUM_ENABLE_AI_NAVIGATION
        links.AddRange(QuantumMapDataBaker.FindLocalObjects<Unity.AI.Navigation.NavMeshLink>(scene).Select(l => new NavMeshLinkTemp(l)));
#endif
#if !UNITY_2023_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
        links.AddRange(QuantumMapDataBaker.FindLocalObjects<OffMeshLink>(scene).Select(l => new NavMeshLinkTemp(l)));
#pragma warning restore CS0618
#endif
        result.Links = new NavMeshBakeDataLink[0];
        if (links.Count > 0) {
          progressBar.Info     = "Validating OffMeshLinks";
          progressBar.Progress = 0.0f;

          // Insert triangles into a temporary grid to optimize triangle searching
          var triangleGrid = new TriangleGrid(Vertices, Triangles);

          for (int l = 0; l < links.Count; l++) {
            var navMeshRegion = links[l].Object.GetComponent<QuantumNavMeshRegion>();
            var regionId      = navMeshRegion != null && string.IsNullOrEmpty(navMeshRegion.Id) == false ? navMeshRegion.Id : string.Empty;
            if (string.IsNullOrEmpty(regionId) == false && regions.Contains(regionId) == false) {
              // Add new region to global list
              regions.Add(regionId);
            }

            var startPosition = links[l].StartPoint;
            var startTriangle = FindTriangleIndex(Vertices, Triangles, settings.LinkErrorCorrection, triangleGrid, ref startPosition);

            var endPosition = links[l].EndPoint;
            var endTriangle = FindTriangleIndex(Vertices, Triangles, settings.LinkErrorCorrection, triangleGrid, ref endPosition);

            if (startTriangle == -1) {
              Debug.LogError($"Could not map start position {startPosition} of navmesh link to a triangle");
            } else if (endTriangle == -1) {
              Debug.LogError($"Could not map end position {endPosition} of navmesh link to a triangle");
            } else {
              // Add link
#if QUANTUM_XY
            if (settings.EnableQuantum_XY) {
              startPosition = new Vector3(startPosition.x, startPosition.z, startPosition.y);
              endPosition = new Vector3(endPosition.x, endPosition.z, endPosition.y);
            }
#endif
              ArrayUtils.Add(ref result.Links, new NavMeshBakeDataLink {
                Start         = startPosition.ToFPVector3(),
                End           = endPosition.ToFPVector3(),
                StartTriangle = startTriangle,
                EndTriangle   = endTriangle,
                Bidirectional = links[l].Bidirectional,
                CostOverride  = FP.FromFloat_UNSAFE(links[l].CostModifier),
                RegionId      = regionId,
                Name          = links[l].Object.name
              });
            }

            progressBar.Progress = (l + 1) / (float)links.Count;
          }
        }


        result.Vertices  = Vertices.Select(v => v.Convert()).ToArray();
        result.Triangles = Triangles.ToArray();

        regions.Sort((a, b) => {
          if (a == "MainArea") return -1;
          else if (b == "MainArea") return 1;
          return string.CompareOrdinal(a, b);
        });
        result.Regions = regions;

        Debug.LogFormat("Imported Unity NavMesh '{0}', cleaned up {1} vertices, found {2} region(s), found {3} link(s)", name, unityNavMeshTriangulation.vertices.Length - Vertices.Length, result.Regions.Count, result.Links.Length);
      }

      return result;
    }

    public static FP FindSmallestAgentRadius(GameObject[] navmeshSurfaces) {
#if QUANTUM_ENABLE_AI_NAVIGATION      
      if (navmeshSurfaces != null) {
        // Try Unity Navmesh Surface tool
        float agentRadius = float.MaxValue;
        foreach (var surface in navmeshSurfaces) {
          var surfaceComponent = surface.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
          if (surfaceComponent == null) {
            Debug.LogErrorFormat("No NavMeshSurface found on '{0}'", surface.name);
          } else {
            if (surfaceComponent.agentTypeID != -1) {
              var settings = UnityEngine.AI.NavMesh.GetSettingsByID(surfaceComponent.agentTypeID);
              if (settings.agentRadius < agentRadius) {
                agentRadius = settings.agentRadius;
              }
            }
          }
        }

        if (agentRadius < float.MaxValue) {
          return FP.FromFloat_UNSAFE(agentRadius);
        }
      }
#endif

      return FP.FromFloat_UNSAFE(DefaultMinAgentRadius);
    }

    private struct NavMeshLinkTemp {
      public Vector3 StartPoint;
      public Vector3 EndPoint;
      public float Width;
      public float CostModifier;
      public bool Bidirectional;
      public bool AutoUpdatePosition;
      public int Area;
      public GameObject Object;

#if QUANTUM_ENABLE_AI_NAVIGATION
      public NavMeshLinkTemp(Unity.AI.Navigation.NavMeshLink link) {
        StartPoint = link.transform.position + link.startPoint;
        EndPoint = link.transform.position + link.endPoint;
        Width = link.width;
        CostModifier = link.costModifier;
        Bidirectional = link.bidirectional;
        AutoUpdatePosition = link.autoUpdate;
        Area = link.area;
        Object = link.gameObject;
      }
#endif

#if !UNITY_2023_3_OR_NEWER
#pragma warning disable CS0618 // Type or member is obsolete
      public NavMeshLinkTemp(OffMeshLink link) {
        Assert.Always(link.startTransform != null && link.endTransform != null, "Failed to import Off Mesh Link '{0}' start or end transforms are invalid", link.name);

        StartPoint = link.startTransform.position;
        EndPoint = link.endTransform.position;
        Width = 0;
        CostModifier = link.costOverride;
        Bidirectional = link.biDirectional;
        AutoUpdatePosition = link.autoUpdatePositions;
        Area = link.area;
        Object = link.gameObject;
      }
#pragma warning restore CS0618
#endif
    }

    private class TriangleGrid {
      public List<int>[]   Grid        { get; private set; }
      public int           CellCount   { get; private set; }
      public double        CellSize    { get; private set; }
      public Vector2Double MaxPosition { get; private set; }
      public Vector2Double MinPosition { get; private set; }

      public TriangleGrid(Vertex[] vertices, NavMeshBakeDataTriangle[] triangles, int gridCellCount = 100) {
        CellCount = gridCellCount;
        Grid      = new List<int>[CellCount * CellCount];

        var maxPosition = new Vector2Double(double.MinValue, double.MinValue);
        var minPosition = new Vector2Double(double.MaxValue, double.MaxValue);
        for (int i = 0; i < vertices.Length; ++i) {
          maxPosition.X = Math.Max(maxPosition.X, vertices[i].Position.X);
          maxPosition.Y = Math.Max(maxPosition.Y, vertices[i].Position.Z);
          minPosition.X = Math.Min(minPosition.X, vertices[i].Position.X);
          minPosition.Y = Math.Min(minPosition.Y, vertices[i].Position.Z);
        }

        MaxPosition = maxPosition;
        MinPosition = minPosition;
        CellSize    = Math.Max(MaxPosition.X - MinPosition.X, MaxPosition.Y - MinPosition.Y) / CellCount;

        for (int i = 0; i < triangles.Length; ++i) {
          int minCellIndexX = int.MaxValue, maxCellIndexX = int.MinValue, minCellIndexY = int.MaxValue, maxCellIndexY = int.MinValue;
          for (int j = 0; j < 3; j++) {
            var pos = vertices[triangles[i].VertexIds[j]].Position;
            minCellIndexX = Math.Min(minCellIndexX, (int)((pos.X - MinPosition.X) / CellSize));
            maxCellIndexX = Math.Max(maxCellIndexX, (int)((pos.X - MinPosition.X) / CellSize));
            minCellIndexY = Math.Min(minCellIndexY, (int)((pos.Z - MinPosition.Y) / CellSize));
            maxCellIndexY = Math.Max(maxCellIndexY, (int)((pos.Z - MinPosition.Y) / CellSize));
          }

          for (int x = minCellIndexX; x <= maxCellIndexX && x >= 0 && x < CellCount; x++) {
            for (int y = minCellIndexY; y <= maxCellIndexY && y >= 0 && y < CellCount; y++) {
              var gridCellIndex = y * CellCount + x;
              if (Grid[gridCellIndex] == null) {
                Grid[gridCellIndex] = new List<int>();
              }

              Grid[gridCellIndex].Add(i);
            }
          }
        }
      }
    }

    private static int FindTriangleIndex(Vertex[] Vertices, NavMeshBakeDataTriangle[] Triangles, float errorCorrection, TriangleGrid triangleGrid, ref Vector3 position) {
      var resultTriangleIndex    = -1;
      var additionalCellsToCheck = errorCorrection > 0 ? Math.Max(1, (int)(errorCorrection / triangleGrid.CellSize)) : 0;
      var checkedTriangles       = new HashSet<int>();
      var inputPosition          = position;

      // find cell index (expand one cell for error correction)
      var _x              = (int)((position.x - triangleGrid.MinPosition.X) / triangleGrid.CellSize);
      var _y              = (int)((position.z - triangleGrid.MinPosition.Y) / triangleGrid.CellSize);
      var closestDistance = double.MaxValue;

      var xMin = Math.Max(0, _x - additionalCellsToCheck);
      var xMax = Math.Min(triangleGrid.CellCount - 1, _x + additionalCellsToCheck);
      var yMin = Math.Max(0, _y - additionalCellsToCheck);
      var yMax = Math.Min(triangleGrid.CellCount - 1, _y + additionalCellsToCheck);

      for (int x = xMin; x <= xMax; x++) {
        for (int z = yMin; z <= yMax; z++) {
          var c = (int)((z * triangleGrid.CellCount) + x);

          // no triangles in cell
          if (triangleGrid.Grid[c] == null) {
            continue;
          }

          for (int t = 0; t < triangleGrid.Grid[c].Count; t++) {
            var triangleIndex = triangleGrid.Grid[c][t];

            // already checked triangle
            if (checkedTriangles.Contains(triangleIndex)) {
              continue;
            }

            checkedTriangles.Add(triangleIndex);

            var closestPoint = new Vector3Double();
            var d = Vector3Double.ClosestDistanceToTriangle(new Vector3Double(inputPosition),
              Vertices[Triangles[triangleIndex].VertexIds[0]].Position,
              Vertices[Triangles[triangleIndex].VertexIds[1]].Position,
              Vertices[Triangles[triangleIndex].VertexIds[2]].Position,
              ref closestPoint);
            if (d < closestDistance) {
              closestDistance     = d;
              resultTriangleIndex = triangleIndex;
              if (errorCorrection > 0) {
                position = closestPoint.AsVector();
              }
            }
          }
        }
      }

      return resultTriangleIndex;
    }

    public struct Vector2Double {
      public double X;
      public double Y;

      public Vector2Double(double x, double y) {
        X = x;
        Y = y;
      }

      public static Vector2Double operator -(Vector2Double a, Vector2Double b) {
        return new Vector2Double(a.X - b.X, a.Y - b.Y);
      }

      public static double Distance(Vector2Double a, Vector2Double b) {
        var v = a - b;
        return Math.Sqrt(v.X * v.X + v.Y * v.Y);
      }
    }

    public struct Vector3Double {
      public double X;
      public double Y;
      public double Z;

      public Vector3Double(double x, double y, double z) {
        X = x;
        Y = y;
        Z = z;
      }

      public Vector3Double(FPVector3 v) {
        X = v.X.AsDouble;
        Y = v.Y.AsDouble;
        Z = v.Z.AsDouble;
      }

      public Vector3Double(Vector3 v) {
        X = v.x;
        Y = v.y;
        Z = v.z;
      }

      public override Boolean Equals(Object obj) {
        if (obj is Vector3Double) {
          return this == ((Vector3Double)obj);
        }

        return false;
      }

      public override Int32 GetHashCode() {
        unchecked {
          var hash = 17;
          hash = hash * 31 + X.GetHashCode();
          hash = hash * 31 + Y.GetHashCode();
          hash = hash * 31 + Z.GetHashCode();
          return hash;
        }
      }

      public static bool operator ==(Vector3Double a, Vector3Double b) {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
      }

      public static bool operator !=(Vector3Double a, Vector3Double b) {
        return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
      }

      public static Vector3Double operator -(Vector3Double a, Vector3Double b) {
        return new Vector3Double(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
      }

      public static Vector3Double operator +(Vector3Double a, Vector3Double b) {
        return new Vector3Double(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
      }

      public static Vector3Double operator *(Vector3Double a, double b) {
        return new Vector3Double(a.X * b, a.Y * b, a.Z * b);
      }

      public static Vector3Double operator *(double b, Vector3Double a) {
        return new Vector3Double(a.X * b, a.Y * b, a.Z * b);
      }

      public FPVector3 AsFPVector() {
        return new FPVector3(FP.FromFloat_UNSAFE((float)X), FP.FromFloat_UNSAFE((float)Y), FP.FromFloat_UNSAFE((float)Z));
      }

      public Vector3 AsVector() {
        return new Vector3((float)X, (float)Y, (float)Z);
      }

      public double SqrMagnitude() {
        return X * X + Y * Y + Z * Z;
      }

      public static double SqrMagnitude(Vector3Double v) {
        return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
      }

      public double Magnitude() {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
      }

      public static double Distance(Vector3Double a, Vector3Double b) {
        var v = a - b;
        return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
      }

      public void Normalize() {
        var d = Math.Sqrt(X * X + Y * Y + Z * Z);

        if (d == 0) {
          throw new ArgumentException("Vector magnitude is null");
        }

        X = X / d;
        Y = Y / d;
        Z = Z / d;
      }

      public override string ToString() {
        return $"{X} {Y} {Z}";
      }

      public static double Dot(Vector3Double a, Vector3Double b) {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
      }

      public static Vector3Double Cross(Vector3Double a, Vector3Double b) {
        return new Vector3Double(
          a.Y * b.Z - a.Z * b.Y,
          a.Z * b.X - a.X * b.Z,
          a.X * b.Y - a.Y * b.X);
      }

      public static bool IsPointBetween(Vector3Double p, Vector3Double v0, Vector3Double v1, float epsilon, float epsilonHeight) {
        // We don't want to compare end points only is p is "really" in between
        if (p == v0 || p == v1 || v0 == v1)
          return false;

#if QUANTUM_XY
        var p0 = Vector2Double.Distance(new Vector2Double(p.X, p.Y), new Vector2Double(v0.X, v0.Y));
        var p1 = Vector2Double.Distance(new Vector2Double(p.X, p.Y), new Vector2Double(v1.X, v1.Y));
        var v = Vector2Double.Distance(new Vector2Double(v0.X, v0.Y), new Vector2Double(v1.X, v1.Y));
#else
        var p0 = Vector2Double.Distance(new Vector2Double(p.X, p.Z), new Vector2Double(v0.X, v0.Z));
        var p1 = Vector2Double.Distance(new Vector2Double(p.X, p.Z), new Vector2Double(v1.X, v1.Z));
        var v = Vector2Double.Distance(new Vector2Double(v0.X, v0.Z), new Vector2Double(v1.X, v1.Z));
#endif

        // Is between in 2D
        if (Math.Abs(p0 + p1 - v) > epsilon) {
          return false;
        }

        // Check height offset to edge
        var closestPoint = ClosestPointOnSegment(p, v0, v1);

#if QUANTUM_XY
        return Math.Abs(closestPoint.Z - p.Z) < epsilonHeight;
#else
        return Math.Abs(closestPoint.Y - p.Y) < epsilonHeight;
#endif
      }

      private static Vector3Double ClosestPointOnSegment(Vector3Double point, Vector3Double v0, Vector3Double v1) {
        var x = v0.X - v1.X;
        var y = v0.Y - v1.Y;
        var z = v0.Z - v1.Z;
        var l2 = x * x + y * y + z * z;

        if (l2 == 0) return v0;

        x = (point.X - v0.X) * (v1.X - v0.X);
        y = (point.Y - v0.Y) * (v1.Y - v0.Y);
        z = (point.Z - v0.Z) * (v1.Z - v0.Z);
        var t = Math.Max(0, Math.Min(1, (x + y + z) / l2));

        Vector3Double result;
        result.X = v0.X + t * (v1.X - v0.X);
        result.Y = v0.Y + t * (v1.Y - v0.Y);
        result.Z = v0.Z + t * (v1.Z - v0.Z);

        return result;
      }

      public static double ClosestDistanceToTriangle(Vector3Double p, Vector3Double v0, Vector3Double v1, Vector3Double v2, ref Vector3Double closestPoint) {
        var diff  = p - v0;
        var edge0 = v1 - v0;
        var edge1 = v2 - v0;
        var a00   = Dot(edge0, edge0);
        var a01   = Dot(edge0, edge1);
        var a11   = Dot(edge1, edge1);
        var b0    = -Dot(diff, edge0);
        var b1    = -Dot(diff, edge1);
        var det   = a00 * a11 - a01 * a01;
        var t0    = a01 * b1 - a11 * b0;
        var t1    = a01 * b0 - a00 * b1;

        if (t0 + t1 <= det) {
          if (t0 < 0) {
            if (t1 < 0) {
              if (b0 < 0) {
                t1 = 0;
                if (-b0 >= a00) {
                  t0 = 1;
                } else {
                  t0 = -b0 / a00;
                }
              } else {
                t0 = 0;
                if (b1 >= 0) {
                  t1 = 0;
                } else if (-b1 >= a11) {
                  t1 = 1;
                } else {
                  t1 = -b1 / a11;
                }
              }
            } else {
              t0 = 0;
              if (b1 >= 0) {
                t1 = 0;
              } else if (-b1 >= a11) {
                t1 = 1;
              } else {
                t1 = -b1 / a11;
              }
            }
          } else if (t1 < 0) {
            t1 = 0;
            if (b0 >= 0) {
              t0 = 0;
            } else if (-b0 >= a00) {
              t0 = 1;
            } else {
              t0 = -b0 / a00;
            }
          } else {
            t0 /= det;
            t1 /= det;
          }
        } else {
          double tmp0, tmp1, numer, denom;

          if (t0 < 0) {
            tmp0 = a01 + b0;
            tmp1 = a11 + b1;
            if (tmp1 > tmp0) {
              numer = tmp1 - tmp0;
              denom = a00 - 2 * a01 + a11;
              if (numer >= denom) {
                t0 = 1;
                t1 = 0;
              } else {
                t0 = numer / denom;
                t1 = 1 - t0;
              }
            } else {
              t0 = 0;
              if (tmp1 <= 0) {
                t1 = 1;
              } else if (b1 >= 0) {
                t1 = 0;
              } else {
                t1 = -b1 / a11;
              }
            }
          } else if (t1 < 0) {
            tmp0 = a01 + b1;
            tmp1 = a00 + b0;
            if (tmp1 > tmp0) {
              numer = tmp1 - tmp0;
              denom = a00 - 2 * a01 + a11;
              if (numer >= denom) {
                t1 = 1;
                t0 = 0;
              } else {
                t1 = numer / denom;
                t0 = 1 - t1;
              }
            } else {
              t1 = 0;
              if (tmp1 <= 0) {
                t0 = 1;
              } else if (b0 >= 0) {
                t0 = 0;
              } else {
                t0 = -b0 / a00;
              }
            }
          } else {
            numer = a11 + b1 - a01 - b0;
            if (numer <= 0) {
              t0 = 0;
              t1 = 1;
            } else {
              denom = a00 - 2 * a01 + a11;
              if (numer >= denom) {
                t0 = 1;
                t1 = 0;
              } else {
                t0 = numer / denom;
                t1 = 1 - t0;
              }
            }
          }
        }

        closestPoint = v0 + t0 * edge0 + t1 * edge1;
        diff         = p - closestPoint;
        return diff.SqrMagnitude();
      }
    }
#endif

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    
    private struct GizmoNavmeshData {
      public Mesh              GizmoMesh;
      public NavMeshRegionMask CurrentRegionMask;
    }

    [StaticField]
    private static Dictionary<string, GizmoNavmeshData> _navmeshGizmoMap;

    [StaticFieldResetMethod]
    public static void InvalidateGizmos() {
      if (_navmeshGizmoMap != null) {
        _navmeshGizmoMap.Clear();
      }
    }

    /// <summary>
    ///   Creates a Unity mesh from the navmesh data and renders it as a gizmo. Uses submeshes to draw main mesh, regions and
    ///   deactivated regions in different colors.
    ///   The meshes are cached in a static dictionary by their NavMesh.Name. Call InvalidateGizmos() to reset the cache
    ///   manually.
    ///   New meshes are created when the region mask changed.
    /// </summary>
    public static void CreateAndDrawGizmoMesh(NavMesh navmesh, NavMeshRegionMask regionMask, QuantumGameGizmosSettings gizmosSettings) {
      var mesh = CreateGizmoMesh(navmesh, regionMask);
      DrawGizmoMesh(mesh, gizmosSettings.NavMeshDefaultColor, gizmosSettings.NavMeshRegionColor);
    }

    public static Mesh CreateGizmoMesh(NavMesh navmesh, NavMeshRegionMask regionMask) {
      if (_navmeshGizmoMap == null) {
        _navmeshGizmoMap = new Dictionary<string, GizmoNavmeshData>();
      }

      if (!_navmeshGizmoMap.TryGetValue(navmesh.Name, out GizmoNavmeshData gizmoNavmeshData) ||
          gizmoNavmeshData.CurrentRegionMask.Equals(regionMask) == false ||
          gizmoNavmeshData.GizmoMesh == null) {
        var mesh = new Mesh();
        mesh.subMeshCount = 3;

#if QUANTUM_XY
      mesh.vertices = navmesh.Vertices.Select(x => new Vector3(x.Point.X.AsFloat, x.Point.Z.AsFloat, x.Point.Y.AsFloat)).ToArray();
#else
        mesh.vertices = navmesh.Vertices.Select(x => x.Point.ToUnityVector3()).ToArray();
#endif

        mesh.SetTriangles(navmesh.Triangles.SelectMany(x => x.Regions.IsMainArea && x.Regions.IsSubset(regionMask) ? new int[] { x.Vertex0, x.Vertex1, x.Vertex2 } : new int[0]).ToArray(), 0);
        mesh.SetTriangles(navmesh.Triangles.SelectMany(x => x.Regions.HasValidNoneMainRegion && x.Regions.IsSubset(regionMask) ? new int[] { x.Vertex0, x.Vertex1, x.Vertex2 } : new int[0]).ToArray(), 1);
        mesh.SetTriangles(navmesh.Triangles.SelectMany(x => !x.Regions.IsSubset(regionMask) ? new int[] { x.Vertex0, x.Vertex1, x.Vertex2 } : new int[0]).ToArray(), 2);
        mesh.RecalculateNormals();

        gizmoNavmeshData = new GizmoNavmeshData() {
          GizmoMesh         = mesh,
          CurrentRegionMask = regionMask
        };
        _navmeshGizmoMap[navmesh.Name] = gizmoNavmeshData;
      }

      return gizmoNavmeshData.GizmoMesh;
    }

    internal static void DrawGizmoMesh(Mesh mesh, Color color, Color regionColor) {
      var originalColor  = Gizmos.color;
      Gizmos.color = color;
      Gizmos.DrawMesh(mesh, 0);
      Gizmos.color = Gizmos.color.Alpha(Gizmos.color.a * 0.75f);
      Gizmos.DrawWireMesh(mesh, 0);
      Gizmos.color = regionColor;
      Gizmos.DrawMesh(mesh, 1);
      Gizmos.color = Gizmos.color.Alpha(Gizmos.color.a * 0.75f);
      Gizmos.DrawWireMesh(mesh, 1);
      var greyValue = (Gizmos.color.r + Gizmos.color.g + Gizmos.color.b) / 3.0f;
      Gizmos.color = new Color(greyValue, greyValue, greyValue, Gizmos.color.a);
      Gizmos.DrawMesh(mesh, 2);
      Gizmos.DrawWireMesh(mesh, 2);
      Gizmos.color = originalColor;
    }
    
#endif
    
    #endregion

    #region Delaunay Triangulation

#if QUANTUM_ENABLE_AI && !QUANTUM_DISABLE_AI

    //MIT License
    //Copyright(c) 2020 Erik Nordeus
    //Permission is hereby granted, free of charge, to any person obtaining a copy
    //of this software and associated documentation files (the "Software"), to deal
    //in the Software without restriction, including without limitation the rights
    //to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    //copies of the Software, and to permit persons to whom the Software is
    //furnished to do so, subject to the following conditions:
    //The above copyright notice and this permission notice shall be included in all
    //copies or substantial portions of the Software.
    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    //AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    //LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    //OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    //SOFTWARE.
    public static class DelaunayTriangulation {
      //An edge going in a direction
      public class HalfEdge {
        //The vertex it points to
        public HalfEdgeVertex v;

        //The next half-edge inside the face (ordered clockwise)
        //The document says counter-clockwise but clockwise is easier because that's how Unity is displaying triangles
        public HalfEdge nextEdge;

        //The opposite half-edge belonging to the neighbor
        public HalfEdge oppositeEdge;

        //(optionally) the previous halfedge in the face
        //If we assume the face is closed, then we could identify this edge by walking forward
        //until we reach it
        public HalfEdge prevEdge;

        public Triangle t;

        public HalfEdge(HalfEdgeVertex v) {
          this.v = v;
        }
      }

      public class HalfEdgeVertex {
        //The position of the vertex
        public Vector3 position;

        public int index;

        //Each vertex references an half-edge that starts at this point
        //Might seem strange because each halfEdge references a vertex the edge is going to?
        public HalfEdge edge;

        public HalfEdgeVertex(Vector3 position, int index) {
          this.position = position;
          this.index    = index;
        }
      }

      //To store triangle data to get cleaner code
      public class Triangle {
        //Corners of the triangle
        public HalfEdgeVertex v1, v2, v3;
        public int            t;

        public HalfEdge edge;

        public void ChangeOrientation() {
          var temp = v1;
          v1 = v2;
          v2 = temp;
        }
      }


      //Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
      public static List<Triangle> TriangulateByFlippingEdges(List<Triangle> triangles, bool retrictToPlanes, Action reporter) {
        // Change the structure from triangle to half-edge to make it faster to flip edges
        List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

        //Flip edges until we have a delaunay triangulation
        int safety       = 0;
        int flippedEdges = 0;
        while (true) {
          safety += 1;

          if (safety > 100000) {
            Debug.Log("Stuck in endless loop");

            break;
          }

          bool hasFlippedEdge = false;

          //Search through all edges to see if we can flip an edge
          for (int i = 0; i < halfEdges.Count; i++) {
            HalfEdge thisEdge = halfEdges[i];

            //Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
            if (thisEdge.oppositeEdge == null) {
              continue;
            }

            //The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
            var a = thisEdge.v;
            var b = thisEdge.nextEdge.v;
            var c = thisEdge.prevEdge.v;
            var d = thisEdge.oppositeEdge.nextEdge.v;

            if (retrictToPlanes) {
              // Both triangles must be in one plane
              var plane     = new Plane(a.position, b.position, c.position);
              var isOnPlane = Mathf.Abs(plane.GetDistanceToPoint(d.position));
              if (isOnPlane > float.Epsilon) {
                continue;
              }
            }

            Vector2 aPos = new Vector2(a.position.x, a.position.z);
            Vector2 bPos = new Vector2(b.position.x, b.position.z);
            Vector2 cPos = new Vector2(c.position.x, c.position.z);
            Vector2 dPos = new Vector2(d.position.x, d.position.z);

            //Use the circle test to test if we need to flip this edge
            if (IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f) {
              //Are these the two triangles that share this edge forming a convex quadrilateral?
              //Otherwise the edge cant be flipped
              if (IsQuadrilateralConvex(aPos, bPos, cPos, dPos)) {
                //If the new triangle after a flip is not better, then dont flip
                //This will also stop the algoritm from ending up in an endless loop
                if (IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f) {
                  continue;
                }

                //Flip the edge
                flippedEdges += 1;

                hasFlippedEdge = true;

                FlipEdge(thisEdge);
              }
            }
          }

          reporter.Invoke();

          //We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
          if (!hasFlippedEdge) {
            //Debug.Log("Found a delaunay triangulation");
            break;
          }
        }

        Debug.Log("Delaunay triangulation flipped edges: " + flippedEdges);

        //Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
        //original triangles, so the triangles have the data we need

        return triangles;
      }

      //From triangle where each triangle has one vertex to half edge
      private static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles) {
        //Make sure the triangles have the same orientation
        OrientTrianglesClockwise(triangles);

        //First create a list with all possible half-edges
        List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

        for (int i = 0; i < triangles.Count; i++) {
          Triangle t = triangles[i];

          HalfEdge he1 = new HalfEdge(t.v1);
          HalfEdge he2 = new HalfEdge(t.v2);
          HalfEdge he3 = new HalfEdge(t.v3);

          he1.nextEdge = he2;
          he2.nextEdge = he3;
          he3.nextEdge = he1;

          he1.prevEdge = he3;
          he2.prevEdge = he1;
          he3.prevEdge = he2;

          //The vertex needs to know of an edge going from it
          he1.v.edge = he2;
          he2.v.edge = he3;
          he3.v.edge = he1;

          //The face the half-edge is connected to
          t.edge = he1;

          he1.t = t;
          he2.t = t;
          he3.t = t;

          //Add the half-edges to the list
          halfEdges.Add(he1);
          halfEdges.Add(he2);
          halfEdges.Add(he3);
        }

        //Find the half-edges going in the opposite direction
        for (int i = 0; i < halfEdges.Count; i++) {
          HalfEdge he = halfEdges[i];

          var goingToVertex   = he.v;
          var goingFromVertex = he.prevEdge.v;

          for (int j = 0; j < halfEdges.Count; j++) {
            //Dont compare with itself
            if (i == j) {
              continue;
            }

            HalfEdge heOpposite = halfEdges[j];

            //Is this edge going between the vertices in the opposite direction
            if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position) {
              he.oppositeEdge = heOpposite;

              break;
            }
          }
        }


        return halfEdges;
      }

      //Orient triangles so they have the correct orientation
      private static void OrientTrianglesClockwise(List<Triangle> triangles) {
        for (int i = 0; i < triangles.Count; i++) {
          Triangle tri = triangles[i];

          Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.z);
          Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.z);
          Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.z);

          if (!IsTriangleOrientedClockwise(v1, v2, v3)) {
            tri.ChangeOrientation();
          }
        }
      }

      //Is a triangle in 2d space oriented clockwise or counter-clockwise
      //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
      //https://en.wikipedia.org/wiki/Curve_orientation
      private static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3) {
        bool isClockWise = true;

        float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

        if (determinant > 0f) {
          isClockWise = false;
        }

        return isClockWise;
      }

      //Is a point d inside, outside or on the same circle as a, b, c
      //https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
      //Returns positive if inside, negative if outside, and 0 if on the circle
      private static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec) {
        //This first part will simplify how we calculate the determinant
        float a = aVec.x - dVec.x;
        float d = bVec.x - dVec.x;
        float g = cVec.x - dVec.x;

        float b = aVec.y - dVec.y;
        float e = bVec.y - dVec.y;
        float h = cVec.y - dVec.y;

        float c = a * a + b * b;
        float f = d * d + e * e;
        float i = g * g + h * h;

        float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

        return determinant;
      }

      //Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
      private static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
        bool isConvex = false;

        bool abc = IsTriangleOrientedClockwise(a, b, c);
        bool abd = IsTriangleOrientedClockwise(a, b, d);
        bool bcd = IsTriangleOrientedClockwise(b, c, d);
        bool cad = IsTriangleOrientedClockwise(c, a, d);

        if (abc && abd && bcd & !cad) {
          isConvex = true;
        } else if (abc && abd && !bcd & cad) {
          isConvex = true;
        } else if (abc && !abd && bcd & cad) {
          isConvex = true;
        }
        //The opposite sign, which makes everything inverted
        else if (!abc && !abd && !bcd & cad) {
          isConvex = true;
        } else if (!abc && !abd && bcd & !cad) {
          isConvex = true;
        } else if (!abc && abd && !bcd & !cad) {
          isConvex = true;
        }


        return isConvex;
      }

      //Flip an edge
      private static void FlipEdge(HalfEdge one) {
        //The data we need
        //This edge's triangle
        HalfEdge two   = one.nextEdge;
        HalfEdge three = one.prevEdge;
        //The opposite edge's triangle
        HalfEdge four = one.oppositeEdge;
        HalfEdge five = one.oppositeEdge.nextEdge;
        HalfEdge six  = one.oppositeEdge.prevEdge;
        //The vertices
        var a = one.v;
        var b = one.nextEdge.v;
        var c = one.prevEdge.v;
        var d = one.oppositeEdge.nextEdge.v;


        //Flip

        //Change vertex
        a.edge = one.nextEdge;
        c.edge = one.oppositeEdge.nextEdge;

        //Change half-edge
        //Half-edge - half-edge connections
        one.nextEdge = three;
        one.prevEdge = five;

        two.nextEdge = four;
        two.prevEdge = six;

        three.nextEdge = five;
        three.prevEdge = one;

        four.nextEdge = six;
        four.prevEdge = two;

        five.nextEdge = one;
        five.prevEdge = three;

        six.nextEdge = two;
        six.prevEdge = four;

        //Half-edge - vertex connection
        one.v   = b;
        two.v   = b;
        three.v = c;
        four.v  = d;
        five.v  = d;
        six.v   = a;

        //Half-edge - triangle connection
        Triangle t1 = one.t;
        Triangle t2 = four.t;

        one.t   = t1;
        three.t = t1;
        five.t  = t1;

        two.t  = t2;
        four.t = t2;
        six.t  = t2;

        //Opposite-edges are not changing!

        //Triangle connection
        t1.v1 = b;
        t1.v2 = c;
        t1.v3 = d;

        t2.v1 = b;
        t2.v2 = d;
        t2.v3 = a;

        t1.edge = three;
        t2.edge = four;
      }
    }

#endif

    #endregion
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumNetworkCommunicator.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Client;
  using Photon.Deterministic;
  using Photon.Realtime;

  public partial class QuantumNetworkCommunicator : ICommunicator {
    readonly         ByteArraySlice            _sendSlice = new ByteArraySlice();
    private          RaiseEventArgs  _eventOptions;
    private readonly RealtimeClient _loadBalancingClient;
    private readonly Dictionary<Byte, Object>  _parameters;
    private          Action<EventData>         _lastEventCallback;
    public           ShutdownConnectionOptions ShutdownConnectionOptions { get; set; }

    public RealtimeClient NetworkClient => _loadBalancingClient;

    public Boolean IsConnected {
      get {
        return _loadBalancingClient.IsConnected;
      }
    }

    public Int32 RoundTripTime {
      get {
        return _loadBalancingClient.RealtimePeer.RoundTripTime;
      }
    }

    public Int32 ActorNumber => _loadBalancingClient.LocalPlayer.ActorNumber;

    public QuantumNetworkCommunicator(RealtimeClient loadBalancingClient, ShutdownConnectionOptions shutdownConnectionOptions = ShutdownConnectionOptions.Disconnect) {
      _loadBalancingClient                                             = loadBalancingClient;
      _loadBalancingClient.RealtimePeer.PingInterval                   = 50;
      _loadBalancingClient.RealtimePeer.UseByteArraySlicePoolForEvents = true;

      _parameters                              = new Dictionary<Byte, Object>();
      _parameters[ParameterCode.ReceiverGroup] = (byte)ReceiverGroup.All;

      _eventOptions             = new RaiseEventArgs();
      ShutdownConnectionOptions = shutdownConnectionOptions;
    }

    public void DisposeEventObject(object obj) {
      if (obj is ByteArraySlice bas) {
        bas.Release();
      }
    }

    public void RaiseEvent(Byte eventCode, byte[] message, int messageLength, Boolean reliable, Int32[] toPlayers) {
      _sendSlice.Buffer = message;
      _sendSlice.Count  = messageLength;
      _sendSlice.Offset = 0;

      _eventOptions.TargetActors = toPlayers;

      var sendOptions = new SendOptions {
        // Send all unreliable messages via channel 1
        Channel = reliable ? (byte)0 : (byte)1,
        // Send all unreliable messages as Unsequenced
        DeliveryMode = reliable ? DeliveryMode.Reliable : DeliveryMode.UnreliableUnsequenced
      };

      _loadBalancingClient.OpRaiseEvent(eventCode, _sendSlice, _eventOptions, sendOptions);

      // If multiple events are send during a "frame" this only has to be called once after raising them.
      _loadBalancingClient.RealtimePeer.SendOutgoingCommands();
    }

    public void AddEventListener(OnEventReceived onEventReceived) {
      RemoveEventListener();

      // save callback we know how to de-register it
      _lastEventCallback = (eventData) => {
        var bas = eventData.CustomData as ByteArraySlice;
        if (bas != null) {
          onEventReceived(eventData.Code, bas.Buffer, bas.Count, bas);
        }
      };

      _loadBalancingClient.EventReceived += _lastEventCallback;
    }

    public void Service() {
      // Can be optimized by splitting into receiving and sending and called from Quantum accordingly
      _loadBalancingClient.Service();
    }

    public void RemoveEventListener() {
      if (_lastEventCallback != null) {
        _loadBalancingClient.EventReceived -= _lastEventCallback;
        _lastEventCallback                 =  null;
      }
    }

    public void OnDestroy() {
      RemoveEventListener();
      EndConnection(ShutdownConnectionOptions);
    }

    public System.Threading.Tasks.Task OnDestroyAsync() {
      RemoveEventListener();
      return EndConnectionAsync(ShutdownConnectionOptions);
    }

    private void EndConnection(ShutdownConnectionOptions option) {
      switch (option) {
        case ShutdownConnectionOptions.None:
          break;
        case ShutdownConnectionOptions.LeaveRoom:
        case ShutdownConnectionOptions.LeaveRoomAndBecomeInactive:
          if (_loadBalancingClient.State == ClientState.Joined) {
            _loadBalancingClient.OpLeaveRoom(option == ShutdownConnectionOptions.LeaveRoomAndBecomeInactive);
            return;
          }

          break;
      }

      _loadBalancingClient.Disconnect();
    }

    private System.Threading.Tasks.Task EndConnectionAsync(ShutdownConnectionOptions option) {
      switch (option) {
        case ShutdownConnectionOptions.None:
          return System.Threading.Tasks.Task.CompletedTask;
        case ShutdownConnectionOptions.LeaveRoom:
        case ShutdownConnectionOptions.LeaveRoomAndBecomeInactive:
          if (_loadBalancingClient.State == ClientState.Joined) {
            return _loadBalancingClient.LeaveRoomAsync(option == ShutdownConnectionOptions.LeaveRoomAndBecomeInactive);
          }

          break;
      }

      return _loadBalancingClient.DisconnectAsync();
    }

    [Obsolete("Use ShutdownConnectionOptions")]
    public QuitBehaviour ThisQuitBehaviour => QuitBehaviour.None;

    [Obsolete("Use ShutdownConnectionOptions")]
    public enum QuitBehaviour {
      LeaveRoom                  = ShutdownConnectionOptions.LeaveRoom,
      LeaveRoomAndBecomeInactive = ShutdownConnectionOptions.LeaveRoomAndBecomeInactive,
      Disconnect                 = ShutdownConnectionOptions.Disconnect,
      None                       = ShutdownConnectionOptions.None
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumProfilingClient.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Photon.Deterministic;
  using UnityEngine;
#if QUANTUM_REMOTE_PROFILER
  using System.Diagnostics;
  using System.Net;
  using Quantum;
  using Quantum.Profiling;
  using LiteNetLib;
  using LiteNetLib.Utils;
#endif

  public static class QuantumProfilingClientConstants {
    public const string DISCOVER_TOKEN          = "QuantumProfiling/Discover";
    public const string DISCOVER_RESPONSE_TOKEN = "QuantumProfiling/DiscoverResponse";
    public const string CONNECT_TOKEN           = "QuantumProfiling/Connect";

    public const byte ClientInfoMessage = 0;
    public const byte FrameMessage      = 1;
  }

  [Serializable]
  public class QuantumProfilingClientInfo {
    [Serializable]
    public class CustomProperty {
      public string Name;
      public string Value;
    }

    public string                     ProfilerId;
    public DeterministicSessionConfig Config;
    public List<CustomProperty>       Properties = new List<CustomProperty>();

    public QuantumProfilingClientInfo() {
    }

    public QuantumProfilingClientInfo(string clientId, DeterministicSessionConfig config, DeterministicPlatformInfo platformInfo) {
      ProfilerId = Guid.NewGuid().ToString();
      Config     = config;

      Properties.Add(CreateProperty("ClientId", clientId));
      Properties.Add(CreateProperty("MachineName", Environment.MachineName));
      Properties.Add(CreateProperty("Architecture", platformInfo.Architecture));
      Properties.Add(CreateProperty("Platform", platformInfo.Platform));
      Properties.Add(CreateProperty("RuntimeHost", platformInfo.RuntimeHost));
      Properties.Add(CreateProperty("Runtime", platformInfo.Runtime));
      Properties.Add(CreateProperty("UnityVersion", Application.unityVersion));
      Properties.Add(CreateProperty("LogicalCoreCount", SystemInfo.processorCount));
      Properties.Add(CreateProperty("CpuFrequency", SystemInfo.processorFrequency));
      Properties.Add(CreateProperty("MemorySizeMB", SystemInfo.systemMemorySize));
      Properties.Add(CreateProperty("OperatingSystem", SystemInfo.operatingSystem));
      Properties.Add(CreateProperty("DeviceModel", SystemInfo.deviceModel));
      Properties.Add(CreateProperty("DeviceName", SystemInfo.deviceName));
      Properties.Add(CreateProperty("ProcessorType", SystemInfo.processorType));
    }


    public         string         GetProperty(string name, string defaultValue = "Unknown") => Properties.Where(x => x.Name == name).SingleOrDefault()?.Value ?? defaultValue;
    private static CustomProperty CreateProperty<T>(string name, T value)                   => CreateProperty(name, value.ToString());

    private static CustomProperty CreateProperty(string name, string value) {
      return new CustomProperty() {
        Name  = name,
        Value = value,
      };
    }
  }

#if QUANTUM_REMOTE_PROFILER
  public class QuantumProfilingClient : IDisposable {
    const double BROADCAST_INTERVAL = 1;

    QuantumProfilingClientInfo  _clientInfo;
    NetManager                  _manager;
    EventBasedNetListener       _listener;
    NetPeer                     _serverPeer;

    Stopwatch _broadcastTimer;
    double    _broadcastNext;
  

    public QuantumProfilingClient(string clientId, DeterministicSessionConfig config, DeterministicPlatformInfo platformInfo) {
      _clientInfo = new QuantumProfilingClientInfo(clientId, config, platformInfo);
      _broadcastTimer = Stopwatch.StartNew();

      _listener = new EventBasedNetListener();
      _manager = new NetManager(_listener);
      _manager.UnconnectedMessagesEnabled = true;
      _manager.Start(0);
    
      //_manager.Connect("192.168.2.199", 30000, NetDataWriter.FromString(QuantumProfilingServer.CONNECT_TOKEN));

      _listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnectedEvent;
      _listener.PeerConnectedEvent += OnPeerConnected;
      _listener.PeerDisconnectedEvent += OnPeerDisconnected;
    }

    void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo) {
      Log.Info($"QuantumProfilingClient: Disconnected from {peer.EndPoint}");

      _serverPeer = null;
      _broadcastNext = 0;
    }

    void OnPeerConnected(NetPeer peer) {
      Log.Info($"QuantumProfilingClient: Connected to {peer.EndPoint}");
      _serverPeer = peer;

      var writer = new NetDataWriter();
      writer.Put(QuantumProfilingClientConstants.ClientInfoMessage);
      writer.Put(JsonUtility.ToJson(_clientInfo));
      _serverPeer.Send(writer, DeliveryMethod.ReliableUnordered);
    }

    void OnNetworkReceiveUnconnectedEvent(IPEndPoint remoteendpoint, NetPacketReader reader, UnconnectedMessageType messagetype) {
      if (reader.GetString() == QuantumProfilingClientConstants.DISCOVER_RESPONSE_TOKEN) {
        _manager.Connect(remoteendpoint, NetDataWriter.FromString(QuantumProfilingClientConstants.CONNECT_TOKEN));
      }
    }

    public void SendProfilingData(ProfilerContextData data) {
      if (_serverPeer == null) {
        return;
      }

      var writer = new NetDataWriter();
      writer.Put(QuantumProfilingClientConstants.FrameMessage);
      writer.Put(JsonUtility.ToJson(data));
      _serverPeer.Send(writer, DeliveryMethod.ReliableUnordered);
    }

    public void Update() {
      if (_serverPeer == null) {
        var now = _broadcastTimer.Elapsed.TotalSeconds;
        if (now > _broadcastNext) {
          _broadcastNext = now + BROADCAST_INTERVAL;
          _manager.SendBroadcast(NetDataWriter.FromString(QuantumProfilingClientConstants.DISCOVER_TOKEN), 30000);
          Log.Info("QuantumProfilingClient: Looking For Profiling Server");
        }
      }

      _manager.PollEvents();
    }

    public void Dispose() {
      if (_manager != null) {
        _manager.Stop();
        _manager = null;
      }
    }
  }
#endif
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumReconnectInformation.cs

namespace Quantum {
  using Photon.Realtime;
  using UnityEngine;

  public class QuantumReconnectInformation : MatchmakingReconnectInformation {
    public static MatchmakingReconnectInformation Load() {
      var result = JsonUtility.FromJson<QuantumReconnectInformation>(PlayerPrefs.GetString("Quantum.ReconnectInformation"));
      if (result == null) {
        result = new QuantumReconnectInformation();
      }

      return result;
    }

    public override void Set(RealtimeClient client) {
      base.Set(client);

      if (client != null) {
        Save(this);
      }
    }

    public static void Reset() {
      PlayerPrefs.SetString("Quantum.ReconnectInformation", string.Empty);
    }

    public static void Save(QuantumReconnectInformation value) {
      PlayerPrefs.SetString("Quantum.ReconnectInformation", JsonUtility.ToJson(value));
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumRunner.cs

namespace Quantum {
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Photon.Deterministic;
  using Photon.Realtime;
  using UnityEngine;

  public partial class QuantumRunner : SessionRunner {
    public static QuantumGame                DefaultGame   => (QuantumGame)QuantumRunnerRegistry.Global.Default.DeterministicGame;
    public static QuantumRunner              Default       => (QuantumRunner)QuantumRunnerRegistry.Global.Default;
    public static IEnumerable<QuantumRunner> ActiveRunners => QuantumRunnerRegistry.Global.ActiveRunners.Select(r => (QuantumRunner)r);

    public static QuantumRunner FindRunner(string id) {
      return (QuantumRunner)QuantumRunnerRegistry.Global.FindRunner(id);
    }

    public static QuantumRunner FindRunner(IDeterministicGame game) {
      return (QuantumRunner)QuantumRunnerRegistry.Global.FindRunner(game);
    }

    public static void ShutdownAll() {
      QuantumRunnerRegistry.Global.ShutdownAll();
    }

    public static System.Threading.Tasks.Task ShutdownAllAsync() {
      return QuantumRunnerRegistry.Global.ShutdownAllAsync();
    }

    public static QuantumRunner StartGame(Arguments arguments) {
      arguments.CallbackDispatcher      = arguments.CallbackDispatcher ?? QuantumCallback.Dispatcher;
      arguments.AssetSerializer         = arguments.AssetSerializer ?? new QuantumUnityJsonSerializer();
      arguments.EventDispatcher         = QuantumEvent.Dispatcher;
      arguments.ResourceManager         = arguments.ResourceManager ?? QuantumUnityDB.Global;
      arguments.RunnerFactory           = arguments.RunnerFactory ?? QuantumRunnerUnityFactory.DefaultFactory;
      return (QuantumRunner)Start(arguments);
    }

    public async static Task<QuantumRunner> StartGameAsync(Arguments arguments) {
      arguments.CallbackDispatcher      = arguments.CallbackDispatcher ?? QuantumCallback.Dispatcher;
      arguments.AssetSerializer         = arguments.AssetSerializer ?? new QuantumUnityJsonSerializer();
      arguments.EventDispatcher         = QuantumEvent.Dispatcher;
      arguments.ResourceManager         = arguments.ResourceManager ?? QuantumUnityDB.Global;
      arguments.RunnerFactory           = arguments.RunnerFactory ?? QuantumRunnerUnityFactory.DefaultFactory;
      return (QuantumRunner)await StartAsync(arguments);
    }

    /// <summary>
    ///   Disable updating the runner completely. Useful when ticking the simualtion by other means.
    /// </summary>
    public bool IsSessionUpdateDisabled;

    /// <summary>
    ///   Access the QuantumGame.
    /// </summary>
    public QuantumGame Game => (QuantumGame)DeterministicGame;

    /// <summary>
    ///   Hide Gizmos toggle.
    /// </summary>
    public bool HideGizmos { get; set; }
    
    /// <summary>
    ///   Gizmo settings for this runner.
    /// </summary>
    public QuantumGameGizmosSettings GizmoSettings { get; set; }

    /// <summary>
    ///   Access the network client through the Communicator.
    /// </summary>
    public RealtimeClient NetworkClient {
      get {
        if (Communicator != null) {
          return ((QuantumNetworkCommunicator)Communicator).NetworkClient;
        }

        return null;
      }
    }

    /// <summary>
    ///   The reference to the Unity object that is updating this runner.
    /// </summary>
    public GameObject UnityObject { get; private set; }

    public QuantumRunner(QuantumRunnerBehaviour runnerScript) {
      UnityObject = runnerScript.gameObject;
    }

    protected override void OnShutdown(ShutdownCause cause) {
      QuantumRunnerRegistry.Global.RemoveRunner(this);
      if (UnityObject != null && UnityObject.gameObject != null) {
        GameObject.Destroy(UnityObject.gameObject);
      }
    }

    public void Update() {
      // TODO: Replace with AddToPlayerLoop, PlayerLoopSystem
      switch (DeltaTimeType) {
        case SimulationUpdateTime.Default:
          Service();
          break;
        case SimulationUpdateTime.EngineDeltaTime:
          Service(Time.deltaTime);
          break;
        case SimulationUpdateTime.EngineUnscaledDeltaTime:
          Service(Time.unscaledDeltaTime);
          break;
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumRunnerRegistry.cs

namespace Quantum {
  using System.Collections.Generic;
  using Photon.Deterministic;

  public class QuantumRunnerRegistry {
    public static QuantumRunnerRegistry Global {
      get {
        if (_instance == null) {
          _instance = new QuantumRunnerRegistry();
        }

        return _instance;
      }
    }

    private static QuantumRunnerRegistry _instance;

    public SessionRunner              Default       => _activeRunners.Count == 0 ? default(SessionRunner) : _activeRunners[0];
    public IEnumerable<SessionRunner> ActiveRunners => _activeRunners;

    private List<SessionRunner> _activeRunners = new List<SessionRunner>();

    public void ShutdownAll() {
      for (int i = _activeRunners.Count - 1; i >= 0; i--) {
        _activeRunners[i].Shutdown();
      }
    }

    public System.Threading.Tasks.Task ShutdownAllAsync() {
      var tasks = new List<System.Threading.Tasks.Task>();
      for (int i = 0; i < _activeRunners.Count; i++) {
        tasks.Add(_activeRunners[i].ShutdownAsync());
      }

      return System.Threading.Tasks.Task.WhenAll(tasks);
    }

    public void AddRunner(SessionRunner runner) {
      _activeRunners.Add(runner);
    }

    public void RemoveRunner(SessionRunner runner) {
      _activeRunners.Remove(runner);
    }

    public SessionRunner FindRunner(string id) {
      for (int i = 0; i < _activeRunners.Count; ++i) {
        if (_activeRunners[i].Id == id)
          return _activeRunners[i];
      }

      return default(SessionRunner);
    }

    public SessionRunner FindRunner(IDeterministicGame game) {
      for (int i = 0; i < _activeRunners.Count; ++i) {
        if (_activeRunners[i].DeterministicGame == game)
          return _activeRunners[i];
      }

      return default(SessionRunner);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumRunnerUnityFactory.cs

namespace Quantum {
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using Photon.Analyzer;
  using Photon.Deterministic;
  using Photon.Realtime;
  using Profiling;
  using UnityEngine;
  using Profiler = UnityEngine.Profiling.Profiler;

  public class QuantumRunnerUnityFactory : IRunnerFactory {
    [StaticField(StaticFieldResetMode.None)]
    public static IRunnerFactory DefaultFactory;

    public static QuantumGameStartParameters CreateGameParameters => new QuantumGameStartParameters {
      CallbackDispatcher = QuantumCallback.Dispatcher,
      AssetSerializer = new QuantumUnityJsonSerializer(),
      EventDispatcher = QuantumEvent.Dispatcher,
      ResourceManager = QuantumUnityDB.Global,
    };

    public DeterministicPlatformInfo CreatePlaformInfo => CreatePlatformInfo();
    public TaskFactory CreateTaskFactory => AsyncConfig.Global.TaskFactory;
    public CancellationToken CreateCancellationToken => AsyncSetup.GlobalCancellationSource.Token;
    public Action UpdateDB => QuantumUnityDB.UpdateGlobal;

    public SessionRunner CreateRunner(SessionRunner.Arguments arguments) {
      var go = new GameObject($"QuantumRunner ({arguments.RunnerId})");
      GameObject.DontDestroyOnLoad(go);
      var script = go.AddComponent<QuantumRunnerBehaviour>();
      script.Runner = new QuantumRunner(script);
      QuantumRunnerRegistry.Global.AddRunner(script.Runner);
      return script.Runner;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void InitializeOnLoad() {
      Init();
    }

    public IDeterministicGame CreateGame(QuantumGameStartParameters startParameters) {
      return new QuantumGame(startParameters);
    }

    public void CreateProfiler(string clientId, DeterministicSessionConfig deterministicConfig,
      DeterministicPlatformInfo platformInfo, IDeterministicGame game) {
#if QUANTUM_REMOTE_PROFILER
      if (!Application.isEditor) {
        var client = new QuantumProfilingClient(clientId, deterministicConfig, platformInfo);
        ((QuantumGame)game).ProfilerSampleGenerated += (sample) => {
          client.SendProfilingData(sample);
          client.Update();
        };

        Application.quitting += () => {
          if (client != null) {
            try {
              client.Dispose();
            } catch (Exception e) {
              Log.Error("Failed to dispose remote profiler on quit: " + e);
            }
          }
        };
      }
#endif
    }

    public static DeterministicPlatformInfo CreatePlatformInfo() {
      DeterministicPlatformInfo info;
      info = new DeterministicPlatformInfo();
      info.Allocator = new QuantumUnityNativeAllocator();

#if !UNITY_EDITOR && UNITY_WEBGL
      // WebGL does not support multithreading. This forces the simulation to run in a single thread.
      info.TaskRunner = new InactiveTaskRunner();
#else
      info.TaskRunner = QuantumTaskRunnerJobs.GetInstance();
#endif

#if UNITY_EDITOR

      info.Runtime = DeterministicPlatformInfo.Runtimes.Mono;
      info.RuntimeHost = DeterministicPlatformInfo.RuntimeHosts.UnityEditor;
      info.Architecture = DeterministicPlatformInfo.Architectures.x86;
#if UNITY_EDITOR_WIN
      info.Platform = DeterministicPlatformInfo.Platforms.Windows;
#elif UNITY_EDITOR_OSX
    info.Platform = DeterministicPlatformInfo.Platforms.OSX;
#endif

#else // UNITY_EDITOR
    info.RuntimeHost = DeterministicPlatformInfo.RuntimeHosts.Unity;
#if ENABLE_IL2CPP
    info.Runtime = DeterministicPlatformInfo.Runtimes.IL2CPP;
#else
    info.Runtime = DeterministicPlatformInfo.Runtimes.Mono;
#endif // ENABLE_IL2CPP

#if UNITY_STANDALONE_WIN
    info.Platform = DeterministicPlatformInfo.Platforms.Windows;
#elif UNITY_STANDALONE_OSX
    info.Platform = DeterministicPlatformInfo.Platforms.OSX;
#elif UNITY_STANDALONE_LINUX
    info.Platform = DeterministicPlatformInfo.Platforms.Linux;
#elif UNITY_IOS
    info.Platform = DeterministicPlatformInfo.Platforms.IOS;
#elif UNITY_ANDROID
    info.Platform = DeterministicPlatformInfo.Platforms.Android;
#elif UNITY_TVOS
    info.Platform = DeterministicPlatformInfo.Platforms.TVOS;
#elif UNITY_XBOXONE
    info.Platform = DeterministicPlatformInfo.Platforms.XboxOne;
#elif UNITY_PS4
    info.Platform = DeterministicPlatformInfo.Platforms.PlayStation4;
#elif UNITY_SWITCH
    info.Platform = DeterministicPlatformInfo.Platforms.Switch;
#elif UNITY_WEBGL
    info.Platform = DeterministicPlatformInfo.Platforms.WebGL;
#endif // UNITY_STANDALONE_WIN

#endif // UNITY_EDITOR

      return info;
    }

    public static void Init(Boolean force = false) {
      // verify using Unity unsafe utils
      MemoryLayoutVerifier.Platform = new QuantumUnityMemoryLayoutVerifierPlatform();

      // set native platform
      Native.Utils = new QuantumUnityNativeUtility();

      // load lookup table
      FPMathUtils.LoadLookupTables(force);

      // set runner factory and init Realtime.Async
      DefaultFactory = new QuantumRunnerUnityFactory();

      // init profiler
      HostProfiler.Init(
        x => Profiler.BeginSample(x),
        () => Profiler.EndSample());

      // init thread profiling (2019.x and up)
      HostProfiler.InitThread(
        (a, b) => Profiler.BeginThreadProfiling(a, b),
        () => Profiler.EndThreadProfiling());

      // init debug draw functions
      Draw.Init(DebugDraw.Ray, DebugDraw.Line, DebugDraw.Circle, DebugDraw.Sphere, DebugDraw.Rectangle, DebugDraw.Box, DebugDraw.Capsule, DebugDraw.Clear);
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumStallWatcher.cs

namespace Quantum {
#if QUANTUM_STALL_WATCHER_ENABLED
  using System;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Threading;
  using UnityEngine;


  public class QuantumStallWatcher : QuantumMonoBehaviour {

    public const QuantumStallWatcherCrashType DefaultPlayerCrashType =
#if UNITY_STANDALONE_WIN
      QuantumStallWatcherCrashType.DivideByZero;
#elif UNITY_STANDALONE_OSX
      QuantumStallWatcherCrashType.DivideByZero;
#elif UNITY_ANDROID
      QuantumStallWatcherCrashType.AccessViolation;
#elif UNITY_IOS
      QuantumStallWatcherCrashType.Abort;
#else
      QuantumStallWatcherCrashType.AccessViolation;
#endif

    public float Timeout = 10.0f;

    [Tooltip("How to crash if stalling in the Editor")]
    public QuantumStallWatcherCrashType EditorCrashType = QuantumStallWatcherCrashType.DivideByZero;

    [Tooltip("How to crash if stalling in the Player. Which crash types produce crash dump is platform-specific.")]
    public QuantumStallWatcherCrashType PlayerCrashType = DefaultPlayerCrashType;

    public new bool DontDestroyOnLoad = false;


    [Space]
    [InspectorButton("Editor_RestoreDefaultCrashType", "Reset Crash Type To The Target Platform's Default")]
    public bool Button_StartInstantReplay;

    private Worker _worker;
    private bool _started;

    private void Awake() {
      if (DontDestroyOnLoad) {
        DontDestroyOnLoad(gameObject);
      }
    }

    private void Start() {
      _started = true;
      OnEnable();
    }

    private void Update() {
      _worker.NotifyUpdate();
    }

    private void OnEnable() {
      if (!_started) {
        return;
      }
      _worker = new Worker(checked((int)(Timeout * 1000)), Application.isEditor ? EditorCrashType : PlayerCrashType);
    }

    private void OnDisable() {
      _worker.Dispose();
      _worker = null;
    }

    private static class Native {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
      const string LibCName = "msvcrt.dll";
#else
      const string LibCName = "libc";
#endif

      [StructLayout(LayoutKind.Sequential)]
      public struct div_t {
        public int quot;
        public int rem;
      }

      [DllImport(LibCName, EntryPoint = "abort", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
      public static extern void LibCAbort();
      [DllImport(LibCName, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
      public static extern IntPtr LibCMemcpy(IntPtr dest, IntPtr src, UIntPtr count);
      [DllImport(LibCName, EntryPoint = "div", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
      public static extern div_t LibCDiv(int numerator, int denominator);
    }


    private sealed class Worker : IDisposable {

      private Thread thread;
      private AutoResetEvent updateStarted = new AutoResetEvent(false);
      private AutoResetEvent shutdown = new AutoResetEvent(false);

      public Worker(int timeoutMills, QuantumStallWatcherCrashType crashType) {

        thread = new Thread(() => {

          var startedHandles = new WaitHandle[] { shutdown, updateStarted };

          for (; ; ) {
            // wait for the update to finish
            int index = WaitHandle.WaitAny(startedHandles, timeoutMills);
            if (index == 0) {
              // shutdown
              break;
            } else if (index == 1) {
              // ok
            } else {
              int crashResult = Crash(crashType);
              Debug.LogError($"Crash failed with result: {crashResult}");
              // a crash should have happened by now
              break;
            }

          }
        }) {
          Name = "QuantumStallWatcherWorker"
        };
        thread.Start();
      }

      public void NotifyUpdate() {
        updateStarted.Set();
      }

      public void Dispose() {
        shutdown.Set();
        if (thread.Join(1000) == false) {
          Debug.LogError($"Failed to join the {thread.Name}");
        }
      }

      [MethodImpl(MethodImplOptions.NoOptimization)]
      public int Crash(QuantumStallWatcherCrashType type, int zero = 0) {
        Debug.LogWarning($"Going to crash... mode: {type}");

        int result = -1;

        if (type == QuantumStallWatcherCrashType.Abort) {
          Native.LibCAbort();
          result = 0;
        } else if (type == QuantumStallWatcherCrashType.AccessViolation) {
          unsafe {
            int* data = stackalloc int[1];
            data[0] = 5;
            Native.LibCMemcpy(new IntPtr(zero), new IntPtr(data), new UIntPtr(sizeof(int)));
            result = 1;
          }
        } else if (type == QuantumStallWatcherCrashType.DivideByZero) {
          result = Native.LibCDiv(5, zero).quot;
        }

        return result;
      }

    }

    public void Editor_RestoreDefaultCrashType() {
      PlayerCrashType = DefaultPlayerCrashType;
    }
  }

  public enum QuantumStallWatcherCrashType {
    AccessViolation,
    Abort,
    DivideByZero
  }
#endif
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumStaticColliderSettings.cs

namespace Quantum {
  using System;

  [Serializable]
  public class QuantumStaticColliderSettings {
    public PhysicsCommon.StaticColliderMutableMode MutableMode;
    public Quantum.AssetRef<Quantum.PhysicsMaterial>                 PhysicsMaterial;
    public AssetRef                                Asset;
    
    [DrawIf("^SourceCollider", 0, ErrorOnConditionMemberNotFound = false)]
    public Boolean                                 Trigger;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityJsonSerializer.cs

namespace Quantum {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Text.RegularExpressions;
  using Photon.Deterministic;
  using UnityEngine;
  using UnityEngine.Serialization;

  public class QuantumUnityJsonSerializer : IAssetSerializer {
    private Dictionary<Type, (Type SurrogateType, Delegate Factory)> _surrogateFactories = new();
    private Dictionary<Type, Type> _surrogateToAssetType = new();

    [Obsolete("No longer used")]
    public bool PrettyPrintEnabled {
      get => false;
      set { }
    }
    
    [Obsolete("No longer used")]
    public bool EntityViewPrefabResolvingEnabled { get => false; set {} }
    
    /// <summary>
    /// If true, all BinaryData assets will be decompressed during deserialization.
    /// </summary>
    public bool DecompressBinaryDataOnDeserialization { get; set; } = false;

    /// <summary>
    /// If set to a positive value, all uncompressed BinaryData assets with size over the value will be compressed
    /// during serialization.
    /// </summary>
    public int? CompressBinaryDataOnSerializationThreshold { get; set; } = 1024;
    
    
    /// <summary>
    /// How many digits should a number have to be enquoted.
    /// Some JSON parsers deserialize all numbers as floating points,  which in case of large integers (e.g. entity ids) can lead to precision loss.
    /// If this property is set to true (default), all integers with <see cref="IntegerEnquotingMinDigits"/> or more digits
    /// are enquoted.
    /// </summary>
    public int? IntegerEnquotingMinDigits { get; set; }
    
    /// <summary>
    /// Should all UnityEngine.Object references be nullified in the resulting JSON?
    /// If true, all UnityEngine.Object references will be serialized as null. Otherwise,
    /// they are serialized as { "instanceId": &lt;value&gt; }.
    ///
    /// True by default.
    /// </summary>
    public bool NullifyUnityObjectReferences { get; set; } = true;
    
    /// <summary>
    /// Custom resolver for EntityView prefabs.
    ///
    /// EntityViews are serialized without prefab references (as they are not JSON serializable). Resolving
    /// takes place during deserialization, by looking up the prefab in the global DB.
    /// </summary>
    public Func<AssetGuid, GameObject> EntityViewPrefabResolver { get; set; }
    
    
    public QuantumUnityJsonSerializer() {
      RegisterSurrogate((EntityView asset) => new EntityViewSurrogate() {
        Identifier = asset.Identifier,
      });
      RegisterSurrogate((BinaryData asset) => BinaryDataSurrogate.Create(asset, CompressBinaryDataOnSerializationThreshold));
    }
    
    public void RegisterSurrogate<AssetType, SurrogateType>(Func<AssetType, SurrogateType> factory) 
      where AssetType : AssetObject
      where SurrogateType : AssetObjectSurrogate {
      Assert.Check(factory != null);
      _surrogateFactories.Add(typeof(AssetType), (typeof(SurrogateType), factory));
      _surrogateToAssetType.Add(typeof(SurrogateType), typeof(AssetType));
    }
    
    /// <summary>
    /// Resolves the prefab associated with the provided AssetGuid by looking it up in the global DB.
    /// </summary>
    /// <param name="guid">The AssetGuid of the prefab to be resolved.</param>
    /// <returns>Returns the GameObject associated with the provided AssetGuid.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the prefab associated with the provided AssetGuid cannot be found.</exception>
    protected virtual GameObject ResolvePrefab(AssetGuid guid) {
      if (EntityViewPrefabResolver != null) {
        return EntityViewPrefabResolver(guid);
      }
      
      var globalEntityView = QuantumUnityDB.GetGlobalAsset(guid) as EntityView;
      if (globalEntityView == null) {
        throw new InvalidOperationException($"Unable to resolve prefab for guid {guid}");
      }
    
      return globalEntityView.Prefab;
    }
    
    protected virtual TextReader CreateReader(Stream stream) => new StreamReader(stream, Encoding.UTF8, true, 1024, true);
    protected virtual TextWriter CreateWriter(Stream stream) => new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
    
    public void SerializeConfig(Stream stream, IRuntimeConfig config) {
      using (var writer = CreateWriter(stream)) {
        writer.Write(JsonUtility.ToJson(config));
      }
    }

    public void SerializePlayer(Stream stream, IRuntimePlayer player) {
      using (var writer = CreateWriter(stream)) {
        writer.Write(JsonUtility.ToJson(player));
      }
    }

    public void SerializeAssets(Stream stream, AssetObject[] assets) {
      
      List<object> list = new List<object>(assets.Length);
      for (int i = 0; i < assets.Length; i++) {
        var asset = assets[i];

        if (_surrogateFactories.TryGetValue(asset.GetType(), out var entry)) {
          var surrogate = (AssetObjectSurrogate)entry.Factory.DynamicInvoke(asset);
          Assert.Check(surrogate != null);
          list.Add(surrogate);
        } else {
          list.Add(asset);
        }
      }

      using (var writer = CreateWriter(stream)) {
        JsonUtilityExtensions.ToJsonWithTypeAnnotation(list, writer, 
          integerEnquoteMinDigits: IntegerEnquotingMinDigits,
          typeSerializer: t => {
            if (_surrogateToAssetType.TryGetValue(t, out var assetType)) {
              return SerializableType.GetShortAssemblyQualifiedName(assetType);
            }
            return null;
          }, 
          instanceIDHandler: !NullifyUnityObjectReferences ? null : (_, id) => {
            return "null";
          });
      }
    }

    public IRuntimeConfig DeserializeConfig(Stream stream) {
      using (var reader = CreateReader(stream)) {
        return JsonUtility.FromJson<RuntimeConfig>(reader.ReadToEnd());
      }
    }

    public IRuntimePlayer DeserializePlayer(Stream stream) {
      using (var reader = CreateReader(stream)) {
        return JsonUtility.FromJson<RuntimePlayer>(reader.ReadToEnd());
      }
    }

    public AssetObject[] DeserializeAssets(Stream stream) {
      string json;
      using (var reader = CreateReader(stream)) {
        json = reader.ReadToEnd();
      }

      var list = (IList)JsonUtilityExtensions.FromJsonWithTypeAnnotation(json, typeResolver: t => {
        var type = Type.GetType(t, true);
        Assert.Check(type.IsSubclassOf(typeof(AssetObject)));
        
        // make sure surrogate type is created instead of the asset type, if needed
        if (_surrogateFactories.TryGetValue(type, out var value)) {
          return value.SurrogateType;
        }
        return type;
      });
      
      var result = new AssetObject[list.Count];
      for (int i = 0; i < list.Count; i++) {
        if (list[i] is AssetObjectSurrogate surrogate) {
          result[i] = surrogate.CreateAsset(this);
        } else {
          result[i] = (AssetObject)list[i]; 
        }
      }

      return result;
    }

    public string PrintObject(object obj) {
      return JsonUtility.ToJson(obj, true);
    }

    // public void SerializeObject(Stream stream, object obj) {
    //   using (var writer = CreateWriter(stream)) {
    //     writer.Write(JsonUtility.ToJson(obj));
    //   }
    // }
    
    [Serializable]
    public abstract class AssetObjectSurrogate {
      public AssetObjectIdentifier Identifier;
      public abstract AssetObject CreateAsset(QuantumUnityJsonSerializer serializer);
    }
    
    [Serializable]
    protected class EntityViewSurrogate : AssetObjectSurrogate {
      public override AssetObject CreateAsset(QuantumUnityJsonSerializer serializer) {
        var result = AssetObject.Create<EntityView>();
        result.Identifier = Identifier;
        result.Prefab = serializer.ResolvePrefab(Identifier.Guid);
        return result;
      }
    }

    [Serializable]
    protected class BinaryDataSurrogate : AssetObjectSurrogate {
      public bool IsCompressed;
      [FormerlySerializedAs("Base64Data")] public string Data;

      public static BinaryDataSurrogate Create(BinaryData asset, int? compressThreshold) {
        byte[] data = asset.Data ?? Array.Empty<byte>();
        bool isCompressed = asset.IsCompressed;
        if (!asset.IsCompressed && compressThreshold.HasValue && data.Length >= compressThreshold.Value) {
          data = ByteUtils.GZipCompressBytes(data);
          isCompressed = true;
        }

        return new BinaryDataSurrogate() {
          Identifier = asset.Identifier,
          Data = ByteUtils.Base64Encode(data),
          IsCompressed = isCompressed,
        };
      }
      
      public override AssetObject CreateAsset(QuantumUnityJsonSerializer serializer) {
        var result = AssetObject.Create<BinaryData>();
        result.Identifier = Identifier;
        result.Data = ByteUtils.Base64Decode(Data);
        result.IsCompressed = IsCompressed;
        if (IsCompressed && serializer.DecompressBinaryDataOnDeserialization) {
          result.IsCompressed = false;
          result.Data = ByteUtils.GZipDecompressBytes(result.Data);
        }
        return result;
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityLegacy.cs

[System.Obsolete("Renamed to Quantum.QuantumMapData")]
public abstract class MapData : Quantum.QuantumMapData { }

[System.Obsolete("Renamed to Quantum.QuantumMapDataBaker")]
public abstract class MapDataBaker : Quantum.QuantumMapDataBaker { }

[System.Obsolete("Renamed to Quantum.QuantumNavMeshRegion")]
public abstract class MapNavMeshRegion : Quantum.QuantumNavMeshRegion { }

[System.Obsolete("Renamed to Quantum.QuantumNavMeshDebugDrawer")]
public abstract class MapNavMeshDebugDrawer : Quantum.QuantumNavMeshDebugDrawer { }

[System.Obsolete("Renamed to Quantum.QuantumMapNavMeshUnity")]
public abstract class MapNavMeshUnity : Quantum.QuantumMapNavMeshUnity { }



#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityLogger.cs

namespace Quantum {
  using System;
  using System.Runtime.ExceptionServices;
  using System.Text;
  using System.Threading;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;

  /// <summary>
  /// A log wrapper around the Quantum static logger to bind to the Unity debug logging.
  /// Toggle the LogLevel using `Quantum.Log.LogLevel`. It get's initialized using the defines:
  ///   QUANTUM_LOGLEVEL_TRACE, QUANTUM_LOGLEVEL_DEBUG, QUANTUM_LOGLEVEL_INFO, QUANTUM_LOGLEVEL_WARN, QUANTUM_LOGLEVEL_ERROR
  /// </summary>
  public partial class QuantumUnityLogger : Quantum.ILogger {
    /// <summary>
    /// Implement this to modify values of this logger.
    /// </summary>
    /// <param name="logger"></param>
    static partial void InitializePartial(ref QuantumUnityLogger logger);

    /// <summary>
    /// Customize logged object names for destroyed objects.
    /// </summary>
    public string NameUnavailableObjectDestroyedLabel = "(destroyed)";
    /// <summary>
    /// Cusomize logged object names from other threads.
    /// </summary>
    public string NameUnavailableInWorkerThreadLabel = "";
    /// <summary>
    /// If true, all messages will be prefixed with [Quantum] tag
    /// </summary>
    public bool UseGlobalPrefix;
    /// <summary>
    /// If true, some parts of messages will be enclosed with &lt;color&gt; tags.
    /// </summary>
    public bool UseColorTags;
    /// <summary>
    /// If true, each log message that has a source parameter will be prefixed with a hash code of the source object. 
    /// </summary>
    public bool AddHashCodePrefix;
    /// <summary>
    /// Color of the global prefix (see <see cref="UseGlobalPrefix"/>).
    /// </summary>
    public string GlobalPrefixColor;
    /// <summary>
    /// Min Random Color
    /// </summary>
    public Color32 MinRandomColor;
    /// <summary>
    /// Max Random Color
    /// </summary>
    public Color32 MaxRandomColor;
    /// <summary>
    /// A prefix tag added to each log.
    /// </summary>
    /// 
    public string GlobalPrefix = "Quantum";
    StringBuilder _builder = new StringBuilder();
    Thread _mainThread;

    /// <summary>
    /// Returns the log level defined by QUANTUM_LOGLEVEL_(..) defines.
    /// </summary>
    public static LogType DefinedLogLevel {
      get {
        var definedLogLevel = LogType.Warn;
#if QUANTUM_LOGLEVEL_TRACE
        definedLogLevel = LogType.Trace;
#elif QUANTUM_LOGLEVEL_DEBUG
        definedLogLevel = LogType.Debug;
#elif QUANTUM_LOGLEVEL_INFO
        definedLogLevel = LogType.Info;
#elif QUANTUM_LOGLEVEL_WARN
        definedLogLevel = LogType.Warn;
#elif QUANTUM_LOGLEVEL_ERROR
        definedLogLevel = LogType.Error;
#endif
        return definedLogLevel;
      }
    }

    public QuantumUnityLogger(Thread mainThread = null) {

      _mainThread = mainThread ?? Thread.CurrentThread;

      bool isDarkMode = false;
#if UNITY_EDITOR
      isDarkMode = UnityEditor.EditorGUIUtility.isProSkin;
#endif

      MinRandomColor = isDarkMode ? new Color32(158, 158, 158, 255) : new Color32(30, 30, 30, 255);
      MaxRandomColor = isDarkMode ? new Color32(255, 255, 255, 255) : new Color32(90, 90, 90, 255);

      UseColorTags = true;
      UseGlobalPrefix = true;
      GlobalPrefixColor = Color32ToRGBString(QuantumColor.Log);
    }

    public void Log(LogType logType, string message, in LogContext logContext) {
      Debug.Assert(_builder.Length == 0);
      string fullMessage;

      var obj = logContext.Source as UnityEngine.Object;

      try {
        if (logType == LogType.Debug) {
          _builder.Append("[DEBUG] ");
        } else if (logType == LogType.Trace) {
          _builder.Append("[TRACE] ");
        }

        if (UseGlobalPrefix) {
          if (UseColorTags) {
            _builder.Append("<color=");
            _builder.Append(GlobalPrefixColor);
            _builder.Append(">");
          }

          _builder.Append("[");
          _builder.Append(GlobalPrefix);

          if (!string.IsNullOrEmpty(logContext.Prefix)) {
            _builder.Append("/");
            _builder.Append(logContext.Prefix);
          }

          _builder.Append("]");

          if (UseColorTags) {
            _builder.Append("</color>");
          }
          _builder.Append(" ");
        } else {
          if (!string.IsNullOrEmpty(logContext.Prefix)) {
            _builder.Append(logContext.Prefix);
            _builder.Append(": ");
          }
        }

        if (obj) {
          var pos = _builder.Length;
          AppendNameThreadSafe(_builder, obj);
          if (_builder.Length > pos) {
            _builder.Append(": ");
          }
        } else if (logContext.Source != null) {
          var pos = _builder.Length;
          _builder.Append(logContext.Source);
          if (_builder.Length > pos) {
            _builder.Append(": ");
          }
        }

        _builder.Append(message);

        fullMessage = _builder.ToString();
      } finally {
        _builder.Clear();
      }

      switch (logType) {
        case LogType.Error:
          Debug.LogError(fullMessage, IsInMainThread ? obj : null);
          break;
        case LogType.Warn:
          Debug.LogWarning(fullMessage, IsInMainThread ? obj : null);
          break;
        default:
          Debug.Log(fullMessage, IsInMainThread ? obj : null);
          break;
      }
    }

    public void LogException(Exception ex, in LogContext logContext) {
      Log(LogType.Warn, $"{ex.GetType()} <i>See next error log entry for details.</i>", in logContext);

#if UNITY_EDITOR
      // this is to force console window double click to take you where the exception
      // has been thrown, not where it has been logged
      var edi = ExceptionDispatchInfo.Capture(ex);
      var thread = new Thread(() => {
        edi.Throw();
      });
      thread.Start();
      thread.Join();
#else
      if (logContext.Source is UnityEngine.Object obj) {
        Debug.LogException(ex, obj);
      } else {
        Debug.LogException(ex);
      }
#endif
    }

    int GetRandomColor(int seed) => GetRandomColor(seed, MinRandomColor, MaxRandomColor);

    int GetColorSeed(string name) {
      int hash = 0;
      for (var i = 0; i < name.Length; ++i) {
        hash = hash * 31 + name[i];
      }

      return hash;
    }

    static int GetRandomColor(int seed, Color32 min, Color32 max) {
      var random = new RNGSession(seed);
      var r = random.NextInclusive(min.r, max.r);
      var g = random.NextInclusive(min.g, max.g);
      var b = random.NextInclusive(min.b, max.b);
      r = Mathf.Clamp(r, 0, 255);
      g = Mathf.Clamp(g, 0, 255);
      b = Mathf.Clamp(b, 0, 255);
      int rgb = (r << 16) | (g << 8) | b;
      return rgb;
    }

    static int Color32ToRGB24(Color32 c) {
      return (c.r << 16) | (c.g << 8) | c.b;
    }

    static string Color32ToRGBString(Color32 c) {
      return string.Format("#{0:X6}", Color32ToRGB24(c));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#endif
    static void Initialize() {
      if (Quantum.Log.Initialized) {
        return;
      }

      var logger = new QuantumUnityLogger(Thread.CurrentThread);

      // Optional override of default values
      InitializePartial(ref logger);

      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      if (logger != null) {
        Quantum.Log.Init(logger, DefinedLogLevel);
      }
    }
    
    private void AppendNameThreadSafe(StringBuilder builder, UnityEngine.Object obj) {
      
      if  ((object)obj == null) throw new ArgumentNullException(nameof(obj));
      
      string name;
      bool isDestroyed = obj == null;
      
      if (isDestroyed) {
        name = NameUnavailableObjectDestroyedLabel;
      } else if (!IsInMainThread) {
        name = NameUnavailableInWorkerThreadLabel;
      } else {
        name = obj.name;
      }
      
      if (UseColorTags) {
        int colorSeed = GetColorSeed(name);
        builder.AppendFormat("<color=#{0:X6}>", GetRandomColor(colorSeed));
      }

      if (AddHashCodePrefix) {
        builder.AppendFormat("{0:X8}", obj.GetHashCode());
      }

      if (name?.Length > 0) {
        if (AddHashCodePrefix) {
          builder.Append(" ");
        }
        builder.Append(name);  
      }

      if (UseColorTags) {
        builder.Append("</color>");
      }
    }

    private bool IsInMainThread => _mainThread == Thread.CurrentThread;
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityMemoryLayoutVerifierPlatform.cs

namespace Quantum {
  using System;
  using System.Reflection;
  using global::Unity.Collections.LowLevel.Unsafe;

  public class QuantumUnityMemoryLayoutVerifierPlatform : MemoryLayoutVerifier.IPlatform {
    public int FieldOffset(FieldInfo field) {
      return UnsafeUtility.GetFieldOffset(field);
    }

    public int SizeOf(Type type) {
      return UnsafeUtility.SizeOf(type);
    }

    public bool CanResolveEnumSize {
      get { return true; }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityNativeImplementation.cs

namespace Quantum {
  using System;
  using Photon.Analyzer;
  using Photon.Deterministic;
  using global::Unity.Collections.LowLevel.Unsafe;
  using UnityAllocator = global::Unity.Collections.Allocator;

#if ENABLE_IL2CPP
  using AOT;
  using System.Collections.Generic;
#endif

#if ENABLE_IL2CPP
  internal sealed unsafe class QuantumUnityNativeAllocator_IL2CPP {
    static readonly HashSet<IntPtr> _allocated = new();
    static void TrackAlloc(IntPtr ptr) {
#if DEBUG
      lock (_allocated) {
        _allocated.Add(ptr);
      }
#endif
    }
    static void TrackFree(IntPtr ptr) {
#if DEBUG
      lock (_allocated) {
        if (_allocated.Remove(ptr) == false) {
          throw new Exception($"Tried to free {ptr} which was not allocated");
        }
      }
#endif
    }
    [MonoPInvokeCallback(typeof(Native.AllocateDelegate))]
    public static IntPtr Allocate(UIntPtr size) {
      var ptr = (IntPtr)UnsafeUtility.Malloc((uint)size, 4, UnityAllocator.Persistent);
      TrackAlloc(ptr);
      return ptr;
    }
    [MonoPInvokeCallback(typeof(Native.FreeDelegate))]
    public static void Free(IntPtr ptr) {
      TrackFree(ptr);
      UnsafeUtility.Free((void*)ptr, UnityAllocator.Persistent);
    }
    [MonoPInvokeCallback(typeof(Native.CopyDelegate))]
    public static void Copy(IntPtr dst, IntPtr src, UIntPtr size) {
      UnsafeUtility.MemCpy((void*)dst, (void*)src, (int)size);
    }
    [MonoPInvokeCallback(typeof(Native.MoveDelegate))]
    public static void Move(IntPtr dst, IntPtr src, UIntPtr size) {
      UnsafeUtility.MemMove((void*)dst, (void*)src, (int)size);
    }
    [MonoPInvokeCallback(typeof(Native.SetDelegate))]
    public static void Set(IntPtr ptr, byte value, UIntPtr size) {
      UnsafeUtility.MemSet((void*)ptr, value, (int)size);
    }
    [MonoPInvokeCallback(typeof(Native.CompareDelegate))]
    public static int Compare(IntPtr ptr1, IntPtr ptr2, UIntPtr size) {
      return UnsafeUtility.MemCmp((void*)ptr1, (void*)ptr2, (int)size);
    }
  }
#endif

  public sealed unsafe class QuantumUnityNativeAllocator : Native.Allocator {
    public sealed override void* Alloc(int count) {
      var ptr = UnsafeUtility.Malloc((uint)count, 4, UnityAllocator.Persistent);
      TrackAlloc(ptr);
      return ptr;
    }

    public sealed override void* Alloc(int count, int alignment) {
      var ptr = UnsafeUtility.Malloc((uint)count, alignment, UnityAllocator.Persistent);
      TrackAlloc(ptr);
      return ptr;
    }

    public sealed override void Free(void* ptr) {
      TrackFree(ptr);
      UnsafeUtility.Free(ptr, UnityAllocator.Persistent);
    }

    protected sealed override void Clear(void* dest, int count) {
      UnsafeUtility.MemClear(dest, (uint)count);
    }

    public sealed override Native.AllocatorVTableManaged GetManagedVTable() {
#if ENABLE_IL2CPP
      // IL2CPP does not support marshaling delegates that point to instance methods to native code.
      return new Native.AllocatorVTableManaged(
        new Native.AllocateDelegate(QuantumUnityNativeAllocator_IL2CPP.Allocate),
        new Native.FreeDelegate(QuantumUnityNativeAllocator_IL2CPP.Free),
        new Native.CopyDelegate(QuantumUnityNativeAllocator_IL2CPP.Copy),
        new Native.MoveDelegate(QuantumUnityNativeAllocator_IL2CPP.Move),
        new Native.SetDelegate(QuantumUnityNativeAllocator_IL2CPP.Set),
        new Native.CompareDelegate(QuantumUnityNativeAllocator_IL2CPP.Compare)
      );
#else
      return new Native.AllocatorVTableManaged(this, Native.Utils);
#endif
    }
  }

  public unsafe class QuantumUnityNativeUtility : Native.Utility {
    static class ObjectPinner {
      // this is technically not pinned... but w/e
      [StaticField(StaticFieldResetMode.None)]
      static object _pinLock = new object();

      [StaticField(StaticFieldResetMode.Manual)]
      public static object _pinnedObject;

      [StaticField(StaticFieldResetMode.Manual)]
      public static ulong _pinnedHandle;

      static void VerifyHandle(Native.ObjectHandle handle) {
        if (handle.Identifier == 0) {
          throw new InvalidOperationException("ObjectHandle.Identifier can't be zero");
        }

        if (handle.Address != IntPtr.Zero) {
          throw new InvalidOperationException("ObjectHandle.Address has to be null");
        }
      }

      public static Native.ObjectHandle HandleAcquire(object obj) {
        lock (_pinLock) {
          if (_pinnedObject != null) {
            throw new InvalidOperationException($"{nameof(QuantumUnityNativeUtility)} can only pin one object at a time");
          }

          _pinnedObject = obj;
          ++_pinnedHandle;
          return new Native.ObjectHandle(_pinnedHandle);
        }
      }

      public static void HandleRelease(Native.ObjectHandle handle) {
        lock (_pinLock) {
          VerifyHandle(handle);

          if (_pinnedHandle != handle.Identifier) {
            throw new InvalidOperationException($"Tried to release handle {handle.Identifier} which does not match current handle {_pinnedHandle}");
          }

          ++_pinnedHandle;
          _pinnedObject = null;
        }
      }

      public static object GetObjectForHandle(Native.ObjectHandle handle) {
        lock (_pinLock) {
          VerifyHandle(handle);

          if (_pinnedHandle != handle.Identifier) {
            throw new InvalidOperationException($"Tried to get object for handle {handle.Identifier} which does not match current handle {_pinnedHandle}");
          }

          return _pinnedObject;
        }
      }
    }

    public override Native.ObjectHandle HandleAcquire(object obj) {
      return ObjectPinner.HandleAcquire(obj);
    }

    public override void HandleRelease(Native.ObjectHandle handle) {
      ObjectPinner.HandleRelease(handle);
    }

    public override object GetObjectForHandle(Native.ObjectHandle handle) {
      return ObjectPinner.GetObjectForHandle(handle);
    }

    public override void Clear(void* dest, int count) {
      UnsafeUtility.MemClear(dest, (long)count);
    }

    public override void Copy(void* dest, void* src, int count) {
      UnsafeUtility.MemCpy(dest, src, (long)count);
    }

    public override void Move(void* dest, void* src, int count) {
      UnsafeUtility.MemMove(dest, src, (long)count);
    }

    public override void Set(void* dest, byte value, int count) {
      UnsafeUtility.MemSet(dest, value, count);
    }

    public override unsafe int Compare(void* ptr1, void* ptr2, int count) {
      return UnsafeUtility.MemCmp(ptr1, ptr2, count);
    }

    [StaticFieldResetMethod]
    public static void ResetStatics() {
      ObjectPinner._pinnedObject = null;
      ObjectPinner._pinnedHandle = 0;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityTypes.Common.cs

// merged UnityTypes

#region QuantumGlobalScriptableObject.cs

namespace Quantum {
  using UnityEngine;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using UnityEngine.Scripting;

  public abstract class QuantumGlobalScriptableObject : QuantumScriptableObject {
    private static IEnumerable<T> GetAssemblyAttributes<T>() where T : Attribute {
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        foreach (var attr in assembly.GetCustomAttributes<T>()) {
          yield return attr;
        }
      }
    }
    
    protected static QuantumGlobalScriptableObjectSourceAttribute[] SourceAttributes => s_sourceAttributes.Value;

    private static readonly Lazy<QuantumGlobalScriptableObjectSourceAttribute[]> s_sourceAttributes = new (() => {
      return GetAssemblyAttributes<QuantumGlobalScriptableObjectSourceAttribute>().OrderBy(x => x.Order).ToArray();
    });
  }
  
  public abstract partial class QuantumGlobalScriptableObject<T> : QuantumGlobalScriptableObject where T : QuantumGlobalScriptableObject<T> {
    private static T                                                s_instance;
    private static QuantumGlobalScriptableObjectUnloadDelegate s_unloadHandler;

    
    public bool IsGlobal { get; private set; }
    
    protected virtual void OnLoadedAsGlobal() {}
    protected virtual void OnUnloadedAsGlobal(bool destroyed) {}
    
    protected void OnDestroy() {
      if (IsGlobal) {
        Assert.Check(object.ReferenceEquals(this, s_instance), $"Expected this to be the global instance");
        s_instance = null;
        s_unloadHandler = null;
        
        IsGlobal = false;
        OnUnloadedAsGlobal(true);
      }
    }

    protected static T GlobalInternal {
      get {
        var instance = GetOrLoadGlobalInstance();
        if (ReferenceEquals(instance, null)) {
          throw new InvalidOperationException($"Failed to load {typeof(T).Name}. If this happens in edit mode, make sure Quantum is properly installed in the Quantum HUB. " +
            $"Otherwise, if the default path does not exist or does not point to a Resource, you need to use " +
            $"{nameof(QuantumGlobalScriptableObjectAttribute)} attribute to point to a method that will perform the loading.");
        }

        return instance;
      }
      set {
        if (value == s_instance) {
          return;
        }

        SetGlobalInternal(value, null);
      }
    }
    
    protected static bool IsGlobalLoadedInternal {
      get => s_instance != null;
    }

    protected static bool TryGetGlobalInternal(out T global) {
      var instance = GetOrLoadGlobalInstance();
      if (ReferenceEquals(instance, null)) {
        global = null;
        return false;
      }

      global = instance;
      return true;
    }

    protected static bool UnloadGlobalInternal() {
      
      var instance = s_instance;
      if (!instance) {
        return false;
      }

      Debug.Assert(instance.IsGlobal);
      
      try {
        s_unloadHandler?.Invoke(instance);
      } finally {
        s_unloadHandler = null;
        s_instance = null;

        if (instance.IsGlobal) {
          instance.IsGlobal = false;
          instance.OnUnloadedAsGlobal(false);
        }
      }

      return true;
    }

    private static T GetOrLoadGlobalInstance() {
      if (s_instance) {
        return s_instance;
      }

      T instance = null;
      QuantumGlobalScriptableObjectUnloadDelegate unloadHandler = null;
      
      instance = LoadPlayerInstance(out unloadHandler);

      if (instance) {
        SetGlobalInternal(instance, unloadHandler);
      }
      
      return instance;
    }
    
    private static T LoadPlayerInstance(out QuantumGlobalScriptableObjectUnloadDelegate unloadHandler) {
      
      T instance = null;
      unloadHandler = default;
      
      foreach (var sourceAttribute in SourceAttributes) {
        if (Application.isEditor) {
          if (!Application.isPlaying && !sourceAttribute.AllowEditMode) {
            continue;
          }
        }

        if (sourceAttribute.ObjectType != typeof(T) && !typeof(T).IsSubclassOf(sourceAttribute.ObjectType)) {
          continue;
        }
        
        var result = sourceAttribute.Load(typeof(T));
        if (result.Object) {
          instance = (T)result.Object;
          unloadHandler = result.Unloader;
          break;
        }

        if (!sourceAttribute.AllowFallback) {
          // no fallback allowed
          break;
        }
      }
      
      return instance;
    }
    
    private static void SetGlobalInternal(T value, QuantumGlobalScriptableObjectUnloadDelegate unloadHandler) {
      if (s_instance) {
        throw new InvalidOperationException($"Failed to set {typeof(T).Name} as global. A global instance is already loaded - it needs to be unloaded first");
      }

      Assert.Check(value, "Expected value to be non-null");
      Assert.Check(s_unloadHandler == null, "Expected unload handler to be null");
        
      if (value) {
        s_instance = value;
        s_unloadHandler = unloadHandler;
        
        s_instance.IsGlobal = true;
        s_instance.OnLoadedAsGlobal();
      }
    }
  }
}

#endregion


#region QuantumGlobalScriptableObjectAttribute.cs

namespace Quantum {
  using System;

  [AttributeUsage(AttributeTargets.Class)]
  public class QuantumGlobalScriptableObjectAttribute : Attribute {
    public QuantumGlobalScriptableObjectAttribute(string defaultPath) {
      DefaultPath = defaultPath;
    }
    
    public string DefaultPath { get; }
    public string DefaultContents { get; set; }
    public string DefaultContentsGeneratorMethod { get; set; }
  }
}

#endregion


#region QuantumGlobalScriptableObjectLoaderMethodAttribute.cs

namespace Quantum {
  using System;

  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  public abstract class QuantumGlobalScriptableObjectSourceAttribute : Attribute {
    public QuantumGlobalScriptableObjectSourceAttribute(Type objectType) {
      ObjectType = objectType;
    }
    
    public Type ObjectType { get; }
    public int Order { get; set; }
    public bool AllowEditMode { get; set; } = false;
    public bool AllowFallback { get; set; } = false;

    public abstract QuantumGlobalScriptableObjectLoadResult Load(Type type);
  }
  
  [Obsolete("Use one of QuantumGlobalScriptableObjectSourceAttribute-derived types instead", true)]
  [AttributeUsage(AttributeTargets.Method)]
  public class QuantumGlobalScriptableObjectLoaderMethodAttribute : Attribute {
    public int Order { get; set; }
  }
  
  public delegate QuantumGlobalScriptableObjectLoadResult QuantumGlobalScriptableObjectLoadDelegate(Type type);

  public delegate void QuantumGlobalScriptableObjectUnloadDelegate(QuantumGlobalScriptableObject instance);
  
  public readonly struct QuantumGlobalScriptableObjectLoadResult {
    public readonly QuantumGlobalScriptableObject               Object;
    public readonly QuantumGlobalScriptableObjectUnloadDelegate Unloader;

    public QuantumGlobalScriptableObjectLoadResult(QuantumGlobalScriptableObject obj, QuantumGlobalScriptableObjectUnloadDelegate unloader = null) {
      Object = obj;
      Unloader = unloader;
    }
    
    // implicit cast operators
    public static implicit operator QuantumGlobalScriptableObjectLoadResult(QuantumGlobalScriptableObject result) => new QuantumGlobalScriptableObjectLoadResult(result, null);
  }
}

#endregion


#region QuantumMonoBehaviour.cs

namespace Quantum {
  using UnityEngine;

  public abstract partial class QuantumMonoBehaviour : MonoBehaviour {
    
  }
}

#endregion


#region QuantumScriptableObject.cs

namespace Quantum {
  using UnityEngine;

  public abstract partial class QuantumScriptableObject : ScriptableObject {
    
  }
}

#endregion



#endregion


#region Assets/Photon/Quantum/Runtime/QuantumUnityUtility.Common.cs

// merged UnityUtility

#region JsonUtilityExtensions.cs

namespace Quantum {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Text.RegularExpressions;
  using UnityEngine;

  public static class JsonUtilityExtensions {
    
    public delegate string TypeSerializerDelegate(Type type);
    public delegate string InstanceIDHandlerDelegate(object context, int value);
    
    private const string TypePropertyName = "$type";

    public static string EnquoteIntegers(string json, int minDigits = 8) {
      var result = Regex.Replace(json, $@"(?<="":\s*)(-?[0-9]{{{minDigits},}})(?=[,}}\n\r\s])", "\"$1\"", RegexOptions.Compiled);
      return result;
    }

    public static string ToJsonWithTypeAnnotation(object obj, InstanceIDHandlerDelegate instanceIDHandler = null) {
      var sb = new StringBuilder(1000);
      using (var writer = new StringWriter(sb)) {
        ToJsonWithTypeAnnotation(obj, writer, instanceIDHandler: instanceIDHandler);
      }
      return sb.ToString();
    }

    public static void ToJsonWithTypeAnnotation(object obj, TextWriter writer, int? integerEnquoteMinDigits = null, TypeSerializerDelegate typeSerializer = null, InstanceIDHandlerDelegate instanceIDHandler = null) {
      if (obj == null) {
        writer.Write("null");
        return;
      }

      if (obj is IList list) {
        writer.Write("[");
        for (var i = 0; i < list.Count; ++i) {
          if (i > 0) {
            writer.Write(",");
          }

          ToJsonInternal(list[i], writer, integerEnquoteMinDigits, typeSerializer, instanceIDHandler);
        }

        writer.Write("]");
      } else {
        ToJsonInternal(obj, writer, integerEnquoteMinDigits, typeSerializer, instanceIDHandler);
      }
    }
    
    
    
    public static T FromJsonWithTypeAnnotation<T>(string json, Func<string, Type> typeResolver = null) {
      if (typeof(T).IsArray) {
        var listType = typeof(List<>).MakeGenericType(typeof(T).GetElementType());
        var list = (IList)Activator.CreateInstance(listType);
        FromJsonWithTypeAnnotationInternal(json, typeResolver, list);

        var array = Array.CreateInstance(typeof(T).GetElementType(), list.Count);
        list.CopyTo(array, 0);
        return (T)(object)array;
      }

      if (typeof(T).GetInterface(typeof(IList).FullName) != null) {
        var list = (IList)Activator.CreateInstance(typeof(T));
        FromJsonWithTypeAnnotationInternal(json, typeResolver, list);
        return (T)list;
      }

      return (T)FromJsonWithTypeAnnotationInternal(json, typeResolver);
    }

    public static object FromJsonWithTypeAnnotation(string json, Func<string, Type> typeResolver = null) {
      Assert.Check(json != null);

      var i = SkipWhiteOrThrow(0);
      if (json[i] == '[') {
        var list = new List<object>();

        // list
        ++i;
        for (var expectComma = false;; expectComma = true) {
          i = SkipWhiteOrThrow(i);

          if (json[i] == ']') {
            break;
          }

          if (expectComma) {
            if (json[i] != ',') {
              throw new InvalidOperationException($"Malformed at {i}: expected ,");
            }
            i = SkipWhiteOrThrow(i + 1);
          }

          var item = FromJsonWithTypeAnnotationToObject(ref i, json, typeResolver);
          list.Add(item);
        }

        return list.ToArray();
      }

      return FromJsonWithTypeAnnotationToObject(ref i, json, typeResolver);

      int SkipWhiteOrThrow(int i) {
        while (i < json.Length && char.IsWhiteSpace(json[i])) {
          i++;
        }

        if (i == json.Length) {
          throw new InvalidOperationException($"Malformed at {i}: expected more");
        }

        return i;
      }
    }

    
    private static object FromJsonWithTypeAnnotationInternal(string json, Func<string, Type> typeResolver = null, IList targetList = null) {
      Assert.Check(json != null);

      var i = SkipWhiteOrThrow(0);
      if (json[i] == '[') {
        var list = targetList ?? new List<object>();

        // list
        ++i;
        for (var expectComma = false;; expectComma = true) {
          i = SkipWhiteOrThrow(i);

          if (json[i] == ']') {
            break;
          }

          if (expectComma) {
            if (json[i] != ',') {
              throw new InvalidOperationException($"Malformed at {i}: expected ,");
            }

            i = SkipWhiteOrThrow(i + 1);
          }

          var item = FromJsonWithTypeAnnotationToObject(ref i, json, typeResolver);
          list.Add(item);
        }

        return targetList ?? ((List<object>)list).ToArray();
      }

      if (targetList != null) {
        throw new InvalidOperationException($"Expected list, got {json[i]}");
      }

      return FromJsonWithTypeAnnotationToObject(ref i, json, typeResolver);

      int SkipWhiteOrThrow(int i) {
        while (i < json.Length && char.IsWhiteSpace(json[i])) {
          i++;
        }

        if (i == json.Length) {
          throw new InvalidOperationException($"Malformed at {i}: expected more");
        }

        return i;
      }
    }

    private static void ToJsonInternal(object obj, TextWriter writer, 
      int? integerEnquoteMinDigits = null,
      TypeSerializerDelegate typeResolver = null,
      InstanceIDHandlerDelegate instanceIDHandler = null) {
      Assert.Check(obj != null);
      Assert.Check(writer != null);

      var json = JsonUtility.ToJson(obj);
      if (integerEnquoteMinDigits.HasValue) {
        json = EnquoteIntegers(json, integerEnquoteMinDigits.Value);
      }
      
      var type = obj.GetType();

      writer.Write("{\"");
      writer.Write(TypePropertyName);
      writer.Write("\":\"");

      writer.Write(typeResolver?.Invoke(type) ?? SerializableType.GetShortAssemblyQualifiedName(type));

      writer.Write('\"');

      if (json == "{}") {
        writer.Write("}");
      } else {
        Assert.Check('{' == json[0]);
        Assert.Check('}' == json[^1]);
        writer.Write(',');
        
        if (instanceIDHandler != null) {
          int i = 1;
          
          for (;;) {
            const string prefix = "{\"instanceID\":";
            
            var nextInstanceId = json.IndexOf(prefix, i, StringComparison.Ordinal);
            if (nextInstanceId < 0) {
              break;
            }
            
            // parse the number that follows; may be negative
            var start = nextInstanceId + prefix.Length;
            var end = json.IndexOf('}', start);
            var instanceId = int.Parse(json.AsSpan(start, end - start));
            
            // append that part
            writer.Write(json.AsSpan(i, nextInstanceId - i));
            writer.Write(instanceIDHandler(obj, instanceId));
            i = end + 1;
          }
          
          writer.Write(json.AsSpan(i, json.Length - i));
        } else {
          writer.Write(json.AsSpan(1, json.Length - 1));
        }
      }
    }

    private static object FromJsonWithTypeAnnotationToObject(ref int i, string json, Func<string, Type> typeResolver) {
      if (json[i] == '{') {
        var endIndex = FindScopeEnd(json, i, '{', '}');
        if (endIndex < 0) {
          throw new InvalidOperationException($"Unable to find end of object's end (starting at {i})");
        }
        
        Assert.Check(endIndex > i);
        Assert.Check(json[endIndex] == '}');

        var part = json.Substring(i, endIndex - i + 1);
        i = endIndex + 1;

        // read the object, only care about the type; there's no way to map dollar-prefixed property to a C# field,
        // so some string replacing is necessary
        var typeInfo = JsonUtility.FromJson<TypeNameWrapper>(part.Replace(TypePropertyName, nameof(TypeNameWrapper.__TypeName), StringComparison.Ordinal));
        Assert.Check(!string.IsNullOrEmpty(typeInfo?.__TypeName));

        var type = typeResolver?.Invoke(typeInfo.__TypeName) ?? Type.GetType(typeInfo.__TypeName, true);
        if (type.IsSubclassOf(typeof(ScriptableObject))) {
          var instance = ScriptableObject.CreateInstance(type);
          JsonUtility.FromJsonOverwrite(part, instance);
          return instance;
        } else {
          var instance = JsonUtility.FromJson(part, type);
          return instance;
        }
      }

      if (i + 4 < json.Length && json.AsSpan(i, 4).SequenceEqual("null")) {
        // is this null?
        i += 4;
        return null;
      }

      throw new InvalidOperationException($"Malformed at {i}: expected {{ or null");
    }
    
    internal static int FindObjectEnd(string json, int start = 0) {
      return FindScopeEnd(json, start, '{', '}');
    }
    
    private static int FindScopeEnd(string json, int start, char cstart = '{', char cend = '}') {
      var depth = 0;
      
      if (json[start] != cstart) {
        return -1;
      }

      for (var i = start; i < json.Length; i++) {
        if (json[i] == '"') {
          // can't be escaped
          Assert.Check('\\' != json[i - 1]);
          // now skip until the first unescaped quote
          while (i < json.Length) {
            if (json[++i] == '"')
              // are we escaped?
            {
              if (json[i - 1] != '\\') {
                break;
              }
            }
          }
        } else if (json[i] == cstart) {
          depth++;
        } else if (json[i] == cend) {
          depth--;
          if (depth == 0) {
            return i;
          }
        }
      }

      return -1;
    }
    
    [Serializable]
    private class TypeNameWrapper {
      public string __TypeName;
    }
  }
}

#endregion


#region QuantumUnityExtensions.cs

namespace Quantum {
  using UnityEngine;

  public static class QuantumUnityExtensions {
    
    #region New Find API

#if UNITY_2022_1_OR_NEWER && !UNITY_2022_2_OR_NEWER 
    public enum FindObjectsInactive {
      Exclude,
      Include,
    }

    public enum FindObjectsSortMode {
      None,
      InstanceID,
    }

    public static T FindFirstObjectByType<T>() where T : Object {
      return (T)FindFirstObjectByType(typeof(T), FindObjectsInactive.Exclude);
    }

    public static T FindAnyObjectByType<T>() where T : Object {
      return (T)FindAnyObjectByType(typeof(T), FindObjectsInactive.Exclude);
    }

    public static T FindFirstObjectByType<T>(FindObjectsInactive findObjectsInactive) where T : Object {
      return (T)FindFirstObjectByType(typeof(T), findObjectsInactive);
    }

    public static T FindAnyObjectByType<T>(FindObjectsInactive findObjectsInactive) where T : Object {
      return (T)FindAnyObjectByType(typeof(T), findObjectsInactive);
    }

    public static Object FindFirstObjectByType(System.Type type, FindObjectsInactive findObjectsInactive) {
      return Object.FindObjectOfType(type, findObjectsInactive == FindObjectsInactive.Include);
    }

    public static Object FindAnyObjectByType(System.Type type, FindObjectsInactive findObjectsInactive) {
      return Object.FindObjectOfType(type, findObjectsInactive == FindObjectsInactive.Include);
    }

    public static T[] FindObjectsByType<T>(FindObjectsSortMode sortMode) where T : Object {
      return ConvertObjects<T>(FindObjectsByType(typeof(T), FindObjectsInactive.Exclude, sortMode));
    }

    public static T[] FindObjectsByType<T>(
      FindObjectsInactive findObjectsInactive,
      FindObjectsSortMode sortMode)
      where T : Object {
      return ConvertObjects<T>(FindObjectsByType(typeof(T), findObjectsInactive, sortMode));
    }

    public static Object[] FindObjectsByType(System.Type type, FindObjectsSortMode sortMode) {
      return FindObjectsByType(type, FindObjectsInactive.Exclude, sortMode);
    }

    public static Object[] FindObjectsByType(System.Type type, FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode) {
      return Object.FindObjectsOfType(type, findObjectsInactive == FindObjectsInactive.Include);
    }

    static T[] ConvertObjects<T>(Object[] rawObjects) where T : Object {
      if (rawObjects == null)
        return (T[])null;
      T[] objArray = new T[rawObjects.Length];
      for (int index = 0; index < objArray.Length; ++index)
        objArray[index] = (T)rawObjects[index];
      return objArray;
    }

#endif

    #endregion
  }
}

#endregion



#endregion


#region Assets/Photon/Quantum/Runtime/UnityDB/QuantumUnityDB.Editor.cs

namespace Quantum {
  using System;

  partial class QuantumUnityDB {
#if UNITY_EDITOR
    public static AssetObject GetGlobalAssetEditorInstance(AssetRef assetRef)                          => Global.GetAssetEditorInstance(assetRef);
    public static T           GetGlobalAssetEditorInstance<T>(AssetRef assetRef) where T : AssetObject => Global.GetAssetEditorInstance(assetRef) as T;

    public AssetObject GetAssetEditorInstance(AssetRef assetRef) => GetAssetEditorInstance<Quantum.AssetObject>(assetRef);

    public T GetAssetEditorInstance<T>(AssetRef assetRef) where T : AssetObject {
      // need to go through the resouce container to make sure they'll be available
      var assetSource = GetAssetSource(assetRef.Id);
      if (assetSource == null) {
        // not mapped in the resource container
        return default;
      }

      return assetSource.EditorInstance as T;
    }
    
    public static bool TryGetGlobalAssetEditorInstance<T>(AssetRef assetRef, out T result)
      where T : AssetObject {
      return Global.TryGetAssetObjectEditorInstance(assetRef, out result);
    }
    
    public static bool TryGetGlobalAssetEditorInstance<T>(AssetRef<T> assetRef, out T result)
      where T : AssetObject {
      return Global.TryGetAssetObjectEditorInstance(assetRef, out result);
    }
    
    public bool TryGetAssetObjectEditorInstance<T>(AssetRef<T> assetRef, out T result)
      where T : AssetObject {
      return TryGetAssetObjectEditorInstance((AssetRef)assetRef.Id, out result);
    }
    
    public bool TryGetAssetObjectEditorInstance<T>(AssetRef assetRef, out T result)
      where T : AssetObject { 
      unsafe {
        var assetReference = GetAssetSource(assetRef.Id);
        if (assetReference == null) {
          result = null;
          return false;
        }
        
        var editorInstance = assetReference.EditorInstance;
        if (editorInstance is T assetT) {
          result = assetT;
          return true;
        }

        result = null;
        return false;
      }
    }
    
    public static string CreateAssetPathFromUnityPath(string unityAssetPath, string nestedName = null) {
      var path = PathUtils.GetPathWithoutExtension(unityAssetPath);


      if (!path.StartsWith("Packages/", StringComparison.Ordinal) && PathUtils.MakeRelativeToFolderFast(path, "Assets/", out var relativePath)) {
        path = relativePath;
      }

      if (nestedName != null) {
        path += NestedPathSeparator + nestedName;
      }

      return path;
    }

#endif
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/DebugDraw.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using Photon.Analyzer;
  using UnityEngine;

  /// <summary>
  /// This class will draw shapes issued by the simulation (e.g. <see cref="Draw.Sphere(Photon.Deterministic.FPVector3, Photon.Deterministic.FP, ColorRGBA?, bool)"/>)
  /// The shape drawig is based on the DEBUG define which is enabled in UnityEditor and development builds.
  /// Can be globally toggled of by using <see cref="IsEnabled"/>.
  /// </summary>
  public static class DebugDraw {
    /// <summary>
    /// Globally toggle on/off any simualtion debug shape drawing.
    /// </summary>
    [StaticField] public static bool IsEnabled = true;

    [StaticField] static Queue<Draw.DebugRay> _rays = new Queue<Draw.DebugRay>();
    [StaticField] static Queue<Draw.DebugLine> _lines = new Queue<Draw.DebugLine>();
    [StaticField] static Queue<Draw.DebugCircle> _circles = new Queue<Draw.DebugCircle>();
    [StaticField] static Queue<Draw.DebugSphere> _spheres = new Queue<Draw.DebugSphere>();
    [StaticField] static Queue<Draw.DebugRectangle> _rectangles = new Queue<Draw.DebugRectangle>();
    [StaticField] static Queue<Draw.DebugBox> _boxes = new Queue<Draw.DebugBox>();
    [StaticField] static Queue<Draw.DebugCapsule> _capsules = new Queue<Draw.DebugCapsule>();
    [StaticField] static Dictionary<ColorRGBA, Material> _materials = new Dictionary<ColorRGBA, Material>(ColorRGBA.EqualityComparer.Instance);
    [StaticField] static Draw.DebugRay[] _raysArray = new Draw.DebugRay[64];
    [StaticField] static Draw.DebugLine[] _linesArray = new Draw.DebugLine[64];
    [StaticField] static Draw.DebugCircle[] _circlesArray = new Draw.DebugCircle[64];
    [StaticField] static Draw.DebugSphere[] _spheresArray = new Draw.DebugSphere[64];
    [StaticField] static Draw.DebugRectangle[] _rectanglesArray = new Draw.DebugRectangle[64];
    [StaticField] static Draw.DebugBox[] _boxesArray = new Draw.DebugBox[64];
    [StaticField] static Draw.DebugCapsule[] _capsuleArray = new Draw.DebugCapsule[64];
    [StaticField] static int _raysCount;
    [StaticField] static int _linesCount;
    [StaticField] static int _circlesCount;
    [StaticField] static int _spheresCount;
    [StaticField] static int _rectanglesCount;
    [StaticField] static int _boxesCount;
    [StaticField] static int _capsuleCount;
    [StaticField] static Vector3[] _circlePoints;

    const int CircleResolution = 64;

    static Vector3[] CirclePoints {
      get {
        if (_circlePoints == null) {
          _circlePoints = new Vector3[CircleResolution];
          for (int i = 0; i < CircleResolution; i++) {
            var theta = i / (float)CircleResolution * Mathf.PI * 2.0f;
            _circlePoints[i] = new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta));
          }
        }
        return _circlePoints;
      }
    }

    public static void Ray(Draw.DebugRay ray) {
      if (IsEnabled == false) {
        return;
      }

      lock (_rays) {
        _rays.Enqueue(ray);
      }
    }

    public static void Line(Draw.DebugLine line) {
      if (IsEnabled == false) {
        return;
      }

      lock (_lines) {
        _lines.Enqueue(line);
      }
    }

    public static void Circle(Draw.DebugCircle circle) {
      if (IsEnabled == false) {
        return;
      }

      lock (_circles) {
        _circles.Enqueue(circle);
      }
    }

    public static void Sphere(Draw.DebugSphere sphere) {
      if (IsEnabled == false) {
        return;
      }

      lock (_spheres) {
        _spheres.Enqueue(sphere);
      }
    }

    public static void Rectangle(Draw.DebugRectangle rectangle) {
      if (IsEnabled == false) {
        return;
      }

      lock (_rectangles) {
        _rectangles.Enqueue(rectangle);
      }
    }

    public static void Box(Draw.DebugBox box) {
      if (IsEnabled == false) {
        return;
      }

      lock (_boxes) {
        _boxes.Enqueue(box);
      }
    }

    public static void Capsule(Draw.DebugCapsule box) {
      if (IsEnabled == false) {
        return;
      }

      lock (_capsules) {
        _capsules.Enqueue(box);
      }
    }

    public static Material GetMaterial(ColorRGBA color) {
      if (_materials.TryGetValue(color, out var mat)) {
        if (mat != null) {
          return mat;
        }

        _materials.Remove(color);
      }

      mat = new Material(DebugMesh.DebugMaterial);
      mat.SetColor("_Color", color.AsColor);

      _materials.Add(color, mat);
      return mat;
    }

    [StaticFieldResetMethod]
    public static void Clear() {
      TakeAllFromQueueAndClearLocked(_rays, ref _raysArray);
      TakeAllFromQueueAndClearLocked(_lines, ref _linesArray);
      TakeAllFromQueueAndClearLocked(_circles, ref _circlesArray);
      TakeAllFromQueueAndClearLocked(_spheres, ref _spheresArray);
      TakeAllFromQueueAndClearLocked(_rectangles, ref _rectanglesArray);
      TakeAllFromQueueAndClearLocked(_boxes, ref _boxesArray);
      TakeAllFromQueueAndClearLocked(_capsules, ref _capsuleArray);

      _raysCount = 0;
      _linesCount = 0;
      _circlesCount = 0;
      _spheresCount = 0;
      _rectanglesCount = 0;
      _boxesCount = 0;
      _capsuleCount = 0;
    }

    public static void TakeAll() {
      _raysCount = TakeAllFromQueueAndClearLocked(_rays, ref _raysArray);
      _linesCount = TakeAllFromQueueAndClearLocked(_lines, ref _linesArray);
      _circlesCount = TakeAllFromQueueAndClearLocked(_circles, ref _circlesArray);
      _spheresCount = TakeAllFromQueueAndClearLocked(_spheres, ref _spheresArray);
      _rectanglesCount = TakeAllFromQueueAndClearLocked(_rectangles, ref _rectanglesArray);
      _boxesCount = TakeAllFromQueueAndClearLocked(_boxes, ref _boxesArray);
      _capsuleCount = TakeAllFromQueueAndClearLocked(_capsules, ref _capsuleArray);
    }

    [Obsolete("Moved to OnPostRender because the debug shape drawing is now using GL commands")]
    public static void DrawAll() {
    }

    public static void OnPostRender(Camera camera) {
      if (IsEnabled == false) {
        return;
      }

      for (Int32 i = 0; i < _raysCount; ++i) {
        DrawRay(_raysArray[i]);
      }

      for (Int32 i = 0; i < _linesCount; ++i) {
        DrawLine(_linesArray[i]);
      }

      for (Int32 i = 0; i < _circlesCount; ++i) {
        DrawCircle(_circlesArray[i]);
      }

      for (Int32 i = 0; i < _spheresCount; ++i) {
        DrawSphere(_spheresArray[i]);
      }

      for (Int32 i = 0; i < _rectanglesCount; ++i) {
        DrawRectangle(_rectanglesArray[i]);
      }

      for (Int32 i = 0; i < _boxesCount; ++i) {
        DrawCube(_boxesArray[i]);
      }

      for (Int32 i = 0; i < _capsuleCount; ++i) {
        DrawCapsule(_capsuleArray[i]);
      }
    }

    static void DrawRay(Draw.DebugRay ray) {
      GetMaterial(ray.Color).SetPass(0);
      GL.PushMatrix();
      GL.Begin(GL.LINES);
      GL.Vertex(ray.Origin.ToUnityVector3(true));
      GL.Vertex(ray.Origin.ToUnityVector3(true) + ray.Direction.ToUnityVector3(true));
      GL.End();
      GL.PopMatrix();
    }

    static void DrawLine(Draw.DebugLine line) {
      GetMaterial(line.Color).SetPass(0);
      GL.PushMatrix();
      GL.Begin(GL.LINES);
      GL.Vertex(line.Start.ToUnityVector3(true));
      GL.Vertex(line.End.ToUnityVector3(true));
      GL.End();
      GL.PopMatrix();
    }

    static void DrawSphere(Draw.DebugSphere sphere) {
      Matrix4x4 mat = Matrix4x4.TRS(sphere.Center.ToUnityVector3(true), Quaternion.identity, Vector3.one * (sphere.Radius.AsFloat + sphere.Radius.AsFloat));
      GetMaterial(sphere.Color).SetPass(0);
      GL.wireframe = sphere.Wire;
      Graphics.DrawMeshNow(DebugMesh.SphereMesh, mat);
      GL.wireframe = false;
    }

    static void DrawCircle(Draw.DebugCircle circle) {
      GetMaterial(circle.Color).SetPass(0);

      if (circle.Wire) {
        var m = Matrix4x4.TRS(circle.Center.ToUnityVector3(true), circle.Rotation.ToUnityQuaternion(true), Vector3.one * (circle.Radius.AsFloat + circle.Radius.AsFloat));
        GL.PushMatrix();
        GL.MultMatrix(m);
        GL.Begin(GL.LINE_STRIP);
#if QUANTUM_XY
        for (int i = 0; i < CirclePoints.Length; i++) {
          GL.Vertex3(CirclePoints[i].x * circle.Radius.AsFloat, CirclePoints[i].z * circle.Radius.AsFloat, 0.0f);
        }
        GL.Vertex3(CirclePoints[0].x * circle.Radius.AsFloat, CirclePoints[0].z * circle.Radius.AsFloat, 0.0f);
#else
        for (int i = 0; i < CirclePoints.Length; i++) {
          GL.Vertex3(CirclePoints[i].x * circle.Radius.AsFloat, 0.0f, CirclePoints[i].z * circle.Radius.AsFloat);
        }
        GL.Vertex3(CirclePoints[0].x * circle.Radius.AsFloat, 0.0f, CirclePoints[0].z * circle.Radius.AsFloat);
#endif
        GL.End();
        GL.PopMatrix();
      } else {
        Quaternion rot = Quaternion.identity;
#if QUANTUM_XY
        rot = Quaternion.Euler(180, 0, 0);
#else
        rot = Quaternion.Euler(-90, 0, 0);
#endif
        var m = Matrix4x4.TRS(circle.Center.ToUnityVector3(true), rot, Vector3.one * (circle.Radius.AsFloat + circle.Radius.AsFloat));
        Graphics.DrawMeshNow(DebugMesh.CircleMesh, m);
      }
    }

    static void DrawRectangle(Draw.DebugRectangle rectangle) {
      GetMaterial(rectangle.Color).SetPass(0);

      var m = Matrix4x4.TRS(rectangle.Center.ToUnityVector3(true), rectangle.Rotation.ToUnityQuaternion(true), rectangle.Size.ToUnityVector3(true));

      GL.MultMatrix(m);
      GL.PushMatrix();
#if QUANTUM_XY
      if (rectangle.Wire) {
        GL.Begin(GL.LINE_STRIP);
        GL.Vertex3(0.5f, -0.5f, 0.0f);
        GL.Vertex3(-0.5f, -0.5f, 0.0f);
        GL.Vertex3(-0.5f, 0.5f, 0.0f);
        GL.Vertex3(0.5f, 0.5f, 0.0f);
        GL.Vertex3(0.5f, -0.5f, 0.0f);
      } else {
        GL.Begin(GL.QUADS);
        GL.Vertex3(0.5f, -0.5f, 0.0f);
        GL.Vertex3(-0.5f, -0.5f, 0.0f);
        GL.Vertex3(-0.5f, 0.5f, 0.0f);
        GL.Vertex3(0.5f, 0.5f, 0.0f);
      }
#else
      if (rectangle.Wire) {
        GL.Begin(GL.LINE_STRIP);
        GL.Vertex3(0.5f, 0.0f, -0.5f);
        GL.Vertex3(-0.5f, 0.0f, -0.5f);
        GL.Vertex3(-0.5f, 0.0f, 0.5f);
        GL.Vertex3(0.5f, 0.0f, 0.5f);
        GL.Vertex3(0.5f, 0.0f, -0.5f);
      } else {
        GL.Begin(GL.QUADS);
        GL.Vertex3(0.5f, 0.0f, -0.5f);
        GL.Vertex3(-0.5f, 0.0f, -0.5f);
        GL.Vertex3(-0.5f, 0.0f, 0.5f);
        GL.Vertex3(0.5f, 0.0f, 0.5f);
      }
#endif
      GL.End();
      GL.PopMatrix();
    }

    static void DrawCube(Draw.DebugBox cube) {
      GetMaterial(cube.Color).SetPass(0);

      var m = Matrix4x4.TRS(cube.Center.ToUnityVector3(true), cube.Rotation.ToUnityQuaternion(true), cube.Size.ToUnityVector3(true));

      if (cube.Wire) {
        GL.PushMatrix();
        GL.MultMatrix(m);
        GL.Begin(GL.LINE_STRIP);
        // top
        GL.Vertex3(0.5f, 0.5f, -0.5f);
        GL.Vertex3(-0.5f, 0.5f, -0.5f);
        GL.Vertex3(-0.5f, 0.5f, 0.5f);
        GL.Vertex3(0.5f, 0.5f, 0.5f);
        GL.Vertex3(0.5f, 0.5f, -0.5f);
        // bottom
        GL.Vertex3(0.5f, -0.5f, -0.5f);
        GL.Vertex3(-0.5f, -0.5f, -0.5f);
        GL.Vertex3(-0.5f, -0.5f, 0.5f);
        GL.Vertex3(0.5f, -0.5f, 0.5f);
        GL.Vertex3(0.5f, -0.5f, -0.5f);
        GL.End();
        // missing lines
        GL.Begin(GL.LINES);
        GL.Vertex3(-0.5f, 0.5f, -0.5f);
        GL.Vertex3(-0.5f, -0.5f, -0.5f);
        GL.Vertex3(-0.5f, 0.5f, 0.5f);
        GL.Vertex3(-0.5f, -0.5f, 0.5f);
        GL.Vertex3(0.5f, 0.5f, 0.5f);
        GL.Vertex3(0.5f, -0.5f, 0.5f);
        GL.End();
        GL.PopMatrix();
      } else {
        // TODO: QUADS would also work
        Graphics.DrawMeshNow(DebugMesh.CubeMesh, m);
      }
    }

    static void DrawCapsule(Draw.DebugCapsule capsule) {
      GetMaterial(capsule.Color).SetPass(0);

      if (capsule.Is2D) {
        var m = Matrix4x4.TRS(capsule.Center.ToUnityVector3(true), capsule.Rotation.ToUnityQuaternion(true), Vector3.one);

        // TODO: solid capsule shape, should probably be done with a texture
        //if (capsule.Wire) {
        GL.PushMatrix();
        GL.MultMatrix(m);
        GL.Begin(GL.LINE_STRIP);

        var halfHeight = capsule.Height.AsFloat * 0.5f;

        for (int i = 0; i < CircleResolution / 2; i++) {
#if QUANTUM_XY
          GL.Vertex3(CirclePoints[i].x * capsule.Radius.AsFloat, CirclePoints[i].z * capsule.Radius.AsFloat + halfHeight, 0.0f);
#else
          GL.Vertex3(CirclePoints[i].x * capsule.Radius.AsFloat, 0.0f, CirclePoints[i].z * capsule.Radius.AsFloat + halfHeight);
#endif
        }

#if QUANTUM_XY
        GL.Vertex3(-capsule.Radius.AsFloat, halfHeight, 0.0f);
        GL.Vertex3(-capsule.Radius.AsFloat, -halfHeight, 0.0f);
#else
        GL.Vertex3(-capsule.Radius.AsFloat, 0.0f, halfHeight);
        GL.Vertex3(-capsule.Radius.AsFloat, 0.0f, -halfHeight);
#endif

        for (int i = CircleResolution / 2; i < CircleResolution; i++) {
#if QUANTUM_XY
          GL.Vertex3(CirclePoints[i].x * capsule.Radius.AsFloat, CirclePoints[i].z * capsule.Radius.AsFloat - halfHeight, 0.0f);
#else
          GL.Vertex3(CirclePoints[i].x * capsule.Radius.AsFloat, 0.0f, CirclePoints[i].z * capsule.Radius.AsFloat - halfHeight);
#endif
        }

#if QUANTUM_XY
        GL.Vertex3(capsule.Radius.AsFloat, -halfHeight, 0.0f);
        GL.Vertex3(capsule.Radius.AsFloat, halfHeight, 0.0f);
#else
        GL.Vertex3(capsule.Radius.AsFloat, 0.0f, -halfHeight);
        GL.Vertex3(capsule.Radius.AsFloat, 0.0f, halfHeight);
#endif

        GL.End();
        GL.PopMatrix();
      } else {
        if (capsule.Wire) {
          GL.PushMatrix();

#if QUANTUM_XY
          var r = Quaternion.Euler(90, 0, 0);
#else
          var r = Quaternion.identity;
#endif

          var m = Matrix4x4.TRS(capsule.Center.ToUnityVector3(true), capsule.Rotation.ToUnityQuaternion(true) * r, Vector3.one);
          GL.MultMatrix(m);
          Draw2DCircle(new Vector3(0, capsule.Height.AsFloat * 0.5f, 0), capsule.Radius.AsFloat);
          Draw2DCircle(new Vector3(0, -capsule.Height.AsFloat * 0.5f, 0), capsule.Radius.AsFloat);

#if QUANTUM_XY
          r = Quaternion.identity;
#else
          r = Quaternion.Euler(90, 0, 0);
#endif

          m = Matrix4x4.TRS(capsule.Center.ToUnityVector3(true), capsule.Rotation.ToUnityQuaternion(true) * r, Vector3.one);
          GL.MultMatrix(m);
          Draw2DCapsuleShape(capsule.Height.AsFloat, capsule.Radius.AsFloat);

#if QUANTUM_XY
          r = Quaternion.Euler(0, 90, 0);
#else
          r = Quaternion.Euler(90, 0, 90);
#endif

          m = Matrix4x4.TRS(capsule.Center.ToUnityVector3(true), capsule.Rotation.ToUnityQuaternion(true) * r, Vector3.one);
          GL.MultMatrix(m);
          Draw2DCapsuleShape(capsule.Height.AsFloat, capsule.Radius.AsFloat);

          GL.PopMatrix();
        } else {
          var height = (capsule.Height.AsFloat * 0.5f) + capsule.Radius.AsFloat;
          var diameter = capsule.Radius.AsFloat * 2.0f;
          var m = Matrix4x4.TRS(capsule.Center.ToUnityVector3(true), capsule.Rotation.ToUnityQuaternion(true), (Vector3.up * height) + (Vector3.right + Vector3.forward) * diameter);
          Graphics.DrawMeshNow(DebugMesh.CapsuleMesh, m);
        }
      }
    }

    static void Draw2DCapsuleShape(float height, float radius) {
      GL.Begin(GL.LINE_STRIP);

      var halfHeight = height * 0.5f;

      for (int i = 0; i < CircleResolution / 2; i++) {
#if QUANTUM_XY
        GL.Vertex3(CirclePoints[i].x * radius, CirclePoints[i].z * radius + halfHeight, 0.0f);
#else
        GL.Vertex3(CirclePoints[i].x * radius, 0.0f, CirclePoints[i].z * radius + halfHeight);
#endif
      }

#if QUANTUM_XY
      GL.Vertex3(-radius, halfHeight, 0.0f);
      GL.Vertex3(-radius, -halfHeight, 0.0f);
#else
      GL.Vertex3(-radius, 0.0f, halfHeight);
      GL.Vertex3(-radius, 0.0f, -halfHeight);
#endif

      for (int i = CircleResolution / 2; i < CircleResolution; i++) {
#if QUANTUM_XY
        GL.Vertex3(CirclePoints[i].x * radius, CirclePoints[i].z * radius - halfHeight, 0.0f);
#else
        GL.Vertex3(CirclePoints[i].x * radius, 0.0f, CirclePoints[i].z * radius - halfHeight);
#endif
      }

#if QUANTUM_XY
      GL.Vertex3(radius, -halfHeight, 0.0f);
      GL.Vertex3(radius, halfHeight, 0.0f);
#else
      GL.Vertex3(radius, 0.0f, -halfHeight);
      GL.Vertex3(radius, 0.0f, halfHeight);
#endif

      GL.End();
    }

    static void Draw2DCircle(Vector3 center, float radius) {
      GL.Begin(GL.LINE_STRIP);
      var p = default(Vector3);
#if QUANTUM_XY
        for (int i = 0; i < CirclePoints.Length; i++) {
          p = CirclePoints[i] * radius;
          GL.Vertex3(p.x + center.x, p.z + center.z, p.y + center.y);
        }
        p = CirclePoints[0] * radius;
        GL.Vertex3(p.x + center.x, p.z + center.z, p.y + center.y);
#else
      for (int i = 0; i < CirclePoints.Length; i++) {
        p = CirclePoints[i] * radius;
        GL.Vertex3(p.x + center.x, p.y + center.y, p.z + center.z);
      }

      p = CirclePoints[0] * radius;
      GL.Vertex3(p.x + center.x, p.y + center.y, p.z + center.z);
#endif
      GL.End();
    }

    static Int32 TakeAllFromQueueAndClearLocked<T>(Queue<T> queue, ref T[] result) {
      lock (queue) {
        var count = 0;

        if (queue.Count > 0) {
          // if result array size is less than queue count
          if (result.Length < queue.Count) {
            // find the next new size that is a multiple of the current result size
            var newSize = result.Length;

            while (newSize < queue.Count) {
              newSize = newSize * 2;
            }

            // and re-size array
            Array.Resize(ref result, newSize);
          }

          // grab all
          while (queue.Count > 0) {
            result[count++] = queue.Dequeue();
          }

          // clear queue
          queue.Clear();
        }

        return count;
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/DebugMesh.cs

namespace Quantum {
  using Photon.Analyzer;
  using UnityEngine;

  public static class DebugMesh {
    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _circleMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _sphereMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _quadMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _cylinderMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _cubeMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Mesh _capsuleMesh;

    [StaticField(StaticFieldResetMode.None)]
    private static Material _debugMaterial;

    [StaticField(StaticFieldResetMode.None)]
    private static Material _debugSolidMaterial;

    public static Mesh CircleMesh {
      get {
        if (!_circleMesh) {
          _circleMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoCircleMesh");
        }

        return _circleMesh;
      }
    }

    public static Mesh SphereMesh {
      get {
        if (!_sphereMesh) {
          _sphereMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoSphereMesh");
        }

        return _sphereMesh;
      }
    }

    public static Mesh QuadMesh {
      get {
        if (!_quadMesh) {
          _quadMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoQuadMesh");
        }

        return _quadMesh;
      }
    }

    public static Mesh CubeMesh {
      get {
        if (!_cubeMesh) {
          _cubeMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoCubeMesh");
        }

        return _cubeMesh;
      }
    }

    public static Mesh CapsuleMesh {
      get {
        if (!_capsuleMesh) {
          _capsuleMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoCapsuleMesh");
        }

        return _capsuleMesh;
      }
    }

    public static Mesh CylinderMesh {
      get {
        if (!_cylinderMesh) {
          _cylinderMesh = UnityEngine.Resources.Load<Mesh>("Gizmos/QuantumGizmoCylinderMesh");
        }

        return _cylinderMesh;
      }
    }

    /// <summary>
    /// The material used to draw transparent simulation debug shapes. 
    /// Replace by setting a material before it's ever used.
    /// </summary>
    public static Material DebugMaterial {
      get {
        if (!_debugMaterial) {
          _debugMaterial = UnityEngine.Resources.Load<Material>("Gizmos/QuantumGizmoDebugDrawMaterial");
        }

        return _debugMaterial;
      }

      set {
        _debugMaterial = value;
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/EditorDefines.cs

namespace Quantum {
  using System;

  public static class EditorDefines {
    public const int AssetMenuPriority               = -1000;
    public const int AssetMenuPriorityAssets         = AssetMenuPriority + 0;
    public const int AssetMenuPriorityConfigurations = AssetMenuPriority + 100;
    public const int AssetMenuPriorityScripts        = AssetMenuPriority + 200;

    [Obsolete]
    public const int AssetMenuPrioritQtn             = AssetMenuPriorityScripts;
    [Obsolete]
    public const int AssetMenuPriorityDemo           = AssetMenuPriority + 18;
    [Obsolete]
    public const int AssetMenuPriorityStart          = AssetMenuPriority + 100;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/FloatMinMax.cs

namespace Quantum {
  using System;
  using UnityEngine;

  [Serializable]
  public struct FloatMinMax {
    public Single Min;
    public Single Max;

    public FloatMinMax(Single min, Single max) {
      Min = min;
      Max = max;
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
  public class MinMaxSliderAttribute : PropertyAttribute {
    public readonly float Min;
    public readonly float Max;

    public MinMaxSliderAttribute()
      : this(0, 1) {
    }

    public MinMaxSliderAttribute(float min, float max) {
      Min = min;
      Max = max;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/FPMathUtils.cs

namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine;

  public static class FPMathUtils {
    public static void LoadLookupTables(Boolean force = false) {
      if (FPLut.IsLoaded && force == false) {
        return;
      }

      FPLut.Init(file => {
#if UNITY_EDITOR
        if (!Application.isPlaying) {
          var path = "Assets/Photon/Quantum/Resources/LUT/" + file + ".bytes";
          var textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
          if (textAsset) {
            return textAsset.bytes;
          }
        }
#endif
        return UnityEngine.Resources.Load<TextAsset>("LUT/" + file).bytes;
      });
    }

    public static FP ToFP(this Single v) {
      return FP.FromFloat_UNSAFE(v);
    }

    public static FP FlipRotation(this FP r) {
#if QUANTUM_XY
        return r;
#else
      return -r;
#endif
    }

    public static Quaternion ToUnityQuaternionDegrees(this FP r) {
#if QUANTUM_XY
        return Quaternion.Euler(0, 0, r.AsFloat);
#else
      return Quaternion.Euler(0, -r.AsFloat, 0);
#endif
    }

    public static Quaternion ToUnityQuaternion(this FP r) {
#if QUANTUM_XY
        return Quaternion.Euler(0, 0, (r * FP.Rad2Deg).AsFloat);
#else
      return Quaternion.Euler(0, -(r * FP.Rad2Deg).AsFloat, 0);
#endif
    }

    public static Quaternion ToUnityQuaternion(this FPQuaternion r) {
      Quaternion q;

      q.x = r.X.AsFloat;
      q.y = r.Y.AsFloat;
      q.z = r.Z.AsFloat;
      q.w = r.W.AsFloat;


      // calculate square magnitude
      var sqr = Mathf.Sqrt(Quaternion.Dot(q, q));
      if (sqr < Mathf.Epsilon) {
        return Quaternion.identity;
      }

      q.x /= sqr;
      q.y /= sqr;
      q.z /= sqr;
      q.w /= sqr;

      return q;
    }

    public static Quaternion ToUnityQuaternion(this FPQuaternion r, bool swizzle) {
      var euler = r.AsEuler.ToUnityVector3(swizzle);
      return Quaternion.Euler(euler);
    }

    public static FPQuaternion ToFPQuaternion(this Quaternion r) {
      FPQuaternion q;

      q.X = r.x.ToFP();
      q.Y = r.y.ToFP();
      q.Z = r.z.ToFP();
      q.W = r.w.ToFP();

      return q;
    }

    public static FP ToFPRotation2DDegrees(this Quaternion r) {
#if QUANTUM_XY
        return FP.FromFloat_UNSAFE(r.eulerAngles.z);
#else
      return -FP.FromFloat_UNSAFE(r.eulerAngles.y);
#endif
    }

    public static FP ToFPRotation2D(this Quaternion r) {
#if QUANTUM_XY
        return FP.FromFloat_UNSAFE(r.eulerAngles.z * Mathf.Deg2Rad);
#else
      return -FP.FromFloat_UNSAFE(r.eulerAngles.y * Mathf.Deg2Rad);
#endif
    }

    public static FPVector2 ToFPVector2(this Vector2 v) {
      return new FPVector2(v.x.ToFP(), v.y.ToFP());
    }

    public static Vector2 ToUnityVector2(this FPVector2 v) {
      return new Vector2(v.X.AsFloat, v.Y.AsFloat);
    }

    public static FPVector2 ToFPVector2(this Vector3 v) {
#if QUANTUM_XY
        return new FPVector2(v.x.ToFP(), v.y.ToFP());
#else
      return new FPVector2(v.x.ToFP(), v.z.ToFP());
#endif
    }

    public static FP ToFPVerticalPosition(this Vector3 v) {
#if QUANTUM_XY
        return -v.z.ToFP();
#else
      return v.y.ToFP();
#endif
    }

    public static FPVector3 ToFPVector3(this Vector3 v) {
      return new FPVector3(v.x.ToFP(), v.y.ToFP(), v.z.ToFP());
    }

    public static Vector3 ToUnityVector3(this FPVector2 v) {
#if QUANTUM_XY
        return new Vector3(v.X.AsFloat, v.Y.AsFloat, 0);
#else
      return new Vector3(v.X.AsFloat, 0, v.Y.AsFloat);
#endif
    }

    public static Vector3 ToUnityVector3(this FPVector3 v) {
      return new Vector3(v.X.AsFloat, v.Y.AsFloat, v.Z.AsFloat);
    }

    /// <summary>
    ///   Use this version of ToUnityVector3() when converting a 3D position from the XZ plane in the simulation to the 2D XY
    ///   plane in Unity.
    /// </summary>
    public static Vector3 ToUnityVector3(this FPVector3 v, bool quantumXYSwizzle) {
#if QUANTUM_XY
        if (quantumXYSwizzle) { 
            return new Vector3(v.X.AsFloat, v.Z.AsFloat, v.Y.AsFloat);
        }
#endif

      return new Vector3(v.X.AsFloat, v.Y.AsFloat, v.Z.AsFloat);
    }

    public static Vector2 ToUnityVector2(this FPVector3 v) {
      return new Vector2(v.X.AsFloat, v.Y.AsFloat);
    }

    public static Vector3 RoundToInt(this Vector3 v) {
      v.x = Mathf.RoundToInt(v.x);
      v.y = Mathf.RoundToInt(v.y);
      v.z = Mathf.RoundToInt(v.z);
      return v;
    }

    public static Vector2 RoundToInt(this Vector2 v) {
      v.x = Mathf.RoundToInt(v.x);
      v.y = Mathf.RoundToInt(v.y);
      return v;
    }

    [Obsolete]
    public static Color32 ToColor32(this ColorRGBA clr) {
      return (Color32)clr;
    }

    [Obsolete]
    public static Color ToColor(this ColorRGBA clr) {
      return clr.AsColor;
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/GameObjectUtils.cs

namespace Quantum {
  using UnityEngine;
  using UnityEngine.UI;

  public static class GameObjectUtils {
    public static void Show(this GameObject[] gameObjects) {
      if (gameObjects != null) {
        for (int i = 0; i < gameObjects.Length; ++i) {
          gameObjects[i].SetActive(true);
        }
      }
    }

    public static void Hide(this GameObject[] gameObjects) {
      if (gameObjects != null) {
        for (int i = 0; i < gameObjects.Length; ++i) {
          gameObjects[i].SetActive(false);
        }
      }
    }

    public static void Show(this GameObject gameObject) {
      if (gameObject && !gameObject.activeSelf) {
        gameObject.SetActive(true);
      }
    }

    public static void Hide(this GameObject gameObject) {
      if (gameObject && gameObject.activeSelf) {
        gameObject.SetActive(false);
      }
    }

    public static bool Toggle(this GameObject gameObject) {
      if (gameObject) {
        return gameObject.Toggle(!gameObject.activeSelf);
      }

      return false;
    }

    public static bool Toggle(this GameObject gameObject, bool state) {
      if (gameObject) {
        if (gameObject.activeSelf != state) {
          gameObject.SetActive(state);
        }

        return state;
      }

      return false;
    }

    public static bool Toggle(this Component component, bool state) {
      if (component) {
        return component.gameObject.Toggle(state);
      }

      return false;
    }

    public static void Show(this Component component) {
      if (component) {
        component.gameObject.Show();
      }
    }

    public static void Show(this Image component, Sprite sprite) {
      if (component) {
        component.sprite = sprite;
        component.gameObject.SetActive(true);
      }
    }

    public static void Hide(this Component component) {
      if (component) {
        component.gameObject.Hide();
      }
    }


    public static void Show<T>(this T[] gameObjects) where T : Component {
      if (gameObjects != null) {
        for (int i = 0; i < gameObjects.Length; ++i) {
          if (gameObjects[i].gameObject.activeSelf == false) {
            gameObjects[i].gameObject.SetActive(true);
          }
        }
      }
    }

    public static void Hide<T>(this T[] gameObjects) where T : Component {
      if (gameObjects != null) {
        for (int i = 0; i < gameObjects.Length; ++i) {
          if (gameObjects[i].gameObject.activeSelf) {
            gameObjects[i].gameObject.SetActive(false);
          }
        }
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/GizmoUtils.cs

namespace Quantum {
  using System;
  using System.Linq;
  using Photon.Deterministic;
  using UnityEditor;
  using UnityEngine;
  
  public static class GizmoUtils {
    public static Color Alpha(this Color color, Single a) {
      color.a = a;
      return color;
    }

    public static Color Brightness(this Color color, float brightness) {
      Color.RGBToHSV(color, out var h, out var s, out var v);
      return Color.HSVToRGB(h, s, v * brightness).Alpha(color.a);
    }

    public const float DefaultArrowHeadLength = 0.25f;
    public const float DefaultArrowHeadAngle  = 25.0f;

    public static void DrawGizmosBox(Transform transform, Vector3 size, Color color, Vector3 offset = default, QuantumGizmoStyle style = default) {
      var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);
      DrawGizmosBox(matrix, size, color, style: style);
    }

    public static void DrawGizmosBox(Vector3 center, Vector3 size, Color color, Quaternion? rotation = null, QuantumGizmoStyle style = default) {
      var matrix = Matrix4x4.TRS(center, rotation ?? Quaternion.identity, Vector3.one);
      DrawGizmosBox(matrix, size, color, style: style);
    }

    public static void DrawGizmosCapsule2D(Vector3 center, float radius, float height, Color color, Quaternion? rotation = null, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR

      var matrix = Matrix4x4.TRS(center, rotation ?? Quaternion.identity, Vector3.one);

      Handles.matrix = matrix;
      Gizmos.color = color;
      Handles.color = Gizmos.color;
#if QUANTUM_XY
      var left = Vector3.left * radius;
      var right = Vector3.right * radius;
      Handles.DrawLine(left + Vector3.up * height, left + Vector3.down * height);
      Handles.DrawLine(right + Vector3.up * height, right + Vector3.down * height);
      Handles.DrawWireArc(Vector3.up * height, Vector3.forward, Vector3.right * radius, 180, radius);
      Handles.DrawWireArc(Vector3.down * height, Vector3.back, Vector3.left * radius, -180, radius);
#else
      var left = Vector3.left * radius;
      var right = Vector3.right * radius;
      Handles.DrawLine(left + Vector3.back * height, left + Vector3.forward * height);
      Handles.DrawLine(right + Vector3.back * height, right + Vector3.forward * height);
      Handles.DrawWireArc(Vector3.back * height, Vector3.up, Vector3.right * radius, 180, radius);
      Handles.DrawWireArc(Vector3.forward * height, Vector3.down, Vector3.left * radius, -180, radius);
#endif
      matrix = Matrix4x4.identity;
      Handles.color = Gizmos.color = Color.white;
      Handles.matrix = matrix;
#endif

    }

    public static void DrawGizmosBox(Matrix4x4 matrix, Vector3 size, Color color, QuantumGizmoStyle style = default) {
      Gizmos.matrix = matrix;
      
      if (style.IsFillEnabled) {
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, size);
      }

      if (style.IsWireframeEnabled) {
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, size);
      }

      Gizmos.matrix = Matrix4x4.identity;
      Gizmos.color  = Color.white;
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Color color, Single height = 0.0f, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR
      var        s = Vector3.one;
      Vector3    up;
      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(0, 0, 0);
      s = new Vector3(radius + radius, radius + radius, 1.0f);
      up = Vector3.forward;
#else
      rot = Quaternion.Euler(-90, 0, 0);
      s   = new Vector3(radius + radius, radius + radius, 1.0f);
      up  = Vector3.up;
#endif

      var mesh = height != 0.0f ? DebugMesh.CylinderMesh : DebugMesh.CircleMesh;
      if (height != 0.0f) {
        s.z = height;
      }
      
      Gizmos.color  = color;
      Handles.color = Gizmos.color;

      if (style.IsWireframeEnabled) {
        if (!style.IsFillEnabled) {
          // draw mesh as invisible; this still lets selection to work
          Gizmos.color = default;
          Gizmos.DrawMesh(mesh, 0, position, rot, s);
        }

        Handles.DrawWireDisc(position, up, radius);
      }

      if (style.IsFillEnabled) {
        Gizmos.DrawMesh(mesh, 0, position, rot, s);
      }

      Handles.color = Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmosSphere(Vector3 position, Single radius, Color color, QuantumGizmoStyle style = default) {

      Gizmos.color = color;
      if (style.IsFillEnabled) {
        Gizmos.DrawSphere(position, radius);
      } else {
        if (style.IsWireframeEnabled) {
          Gizmos.DrawWireSphere(position, radius);
        }
      }

      Gizmos.color = Color.white;
    }

    public static void DrawGizmosTriangle(Vector3 A, Vector3 B, Vector3 C, Color color) {
      Gizmos.color = color;
      Gizmos.DrawLine(A, B);
      Gizmos.DrawLine(B, C);
      Gizmos.DrawLine(C, A);
      Gizmos.color = Color.white;
    }

    public static void DrawGizmoGrid(FPVector2 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
      DrawGizmoGrid(bottomLeft.ToUnityVector3(), width, height, nodeSize, nodeSize, color);
    }

    public static void DrawGizmoGrid(Vector3 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
      DrawGizmoGrid(bottomLeft, width, height, nodeSize, nodeSize, color);
    }

    public static void DrawGizmoGrid(Vector3 bottomLeft, Int32 width, Int32 height, float nodeWidth, float nodeHeight, Color color) {
      Gizmos.color = color;

#if QUANTUM_XY
        for (Int32 z = 0; z <= height; ++z) {
            Gizmos.DrawLine(bottomLeft + new Vector3(0.0f, nodeHeight * z, 0.0f), bottomLeft + new Vector3(width * nodeWidth, nodeHeight * z, 0.0f));
        }

        for (Int32 x = 0; x <= width; ++x) {
            Gizmos.DrawLine(bottomLeft + new Vector3(nodeWidth * x, 0.0f, 0.0f), bottomLeft + new Vector3(nodeWidth * x, height * nodeHeight, 0.0f));
        }
#else
      for (Int32 z = 0; z <= height; ++z) {
        Gizmos.DrawLine(bottomLeft + new Vector3(0.0f, 0.0f, nodeHeight * z), bottomLeft + new Vector3(width * nodeWidth, 0.0f, nodeHeight * z));
      }

      for (Int32 x = 0; x <= width; ++x) {
        Gizmos.DrawLine(bottomLeft + new Vector3(nodeWidth * x, 0.0f, 0.0f), bottomLeft + new Vector3(nodeWidth * x, 0.0f, height * nodeHeight));
      }
#endif

      Gizmos.color = Color.white;
    }

    public static void DrawGizmoPolygon2D(Vector3 position, Quaternion rotation, FPVector2[] vertices, Single height, Color color, QuantumGizmoStyle style = default) {
      var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
      DrawGizmoPolygon2D(matrix, vertices, height, false, color, style: style);
    }

    public static void DrawGizmoPolygon2D(Vector3 position, Quaternion rotation, FPVector2[] vertices, Single height, bool drawNormals, Color color, QuantumGizmoStyle style = default) {
      var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
      DrawGizmoPolygon2D(matrix, vertices, height, drawNormals, color, style: style);
    }

    public static void DrawGizmoPolygon2D(Transform transform, FPVector2[] vertices, Single height, bool drawNormals, Color color, QuantumGizmoStyle style = default) {
      var matrix = transform.localToWorldMatrix;
      DrawGizmoPolygon2D(matrix, vertices, height, drawNormals, color, style: style);
    }

    public static void DrawGizmoPolygon2D(Matrix4x4 matrix, FPVector2[] vertices, Single height, bool drawNormals, Color color, QuantumGizmoStyle style = default) {

      if (vertices.Length < 3) return;

      FPMathUtils.LoadLookupTables();

      color = FPVector2.IsPolygonConvex(vertices) && FPVector2.PolygonNormalsAreValid(vertices) ? color : Color.red;

      var transformedVertices = vertices.Select(x => matrix.MultiplyPoint(x.ToUnityVector3())).ToArray();
      DrawGizmoPolygon2DInternal(transformedVertices, height, drawNormals, color, style: style);
    }

    private static void DrawGizmoPolygon2DInternal(Vector3[] vertices, Single height, Boolean drawNormals, Color color, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR
#if QUANTUM_XY
      var upVector = Vector3.forward;
#else
      var upVector = Vector3.up;
#endif
      Gizmos.color  = color;
      Handles.color = color;

      if (style.IsFillEnabled) {
        Handles.DrawAAConvexPolygon(vertices);

        if (height != 0.0f) {
          Handles.matrix = Matrix4x4.Translate(upVector * height);
          Handles.DrawAAConvexPolygon(vertices);
          Handles.matrix = Matrix4x4.identity;
        }
      }

      if (style.IsWireframeEnabled) {
        for (Int32 i = 0; i < vertices.Length; ++i) {
          var v1 = vertices[i];
          var v2 = vertices[(i + 1) % vertices.Length];

          Gizmos.DrawLine(v1, v2);

          if (height != 0.0f) {
            Gizmos.DrawLine(v1 + upVector * height, v2 + upVector * height);
            Gizmos.DrawLine(v1, v1 + upVector * height);
          }

          if (drawNormals) {
#if QUANTUM_XY
          var normal = Vector3.Cross(v2 - v1, upVector).normalized;
#else
            var normal = Vector3.Cross(v1 - v2, upVector).normalized;
#endif

            var center = Vector3.Lerp(v1, v2, 0.5f);
            DrawGizmoVector(center, center + (normal * 0.25f));
          }
        }
      }

      Gizmos.color = Handles.color = Color.white;
#endif
    }

    public static void DrawGizmoDiamond(Vector3 center, Vector2 size) {
      var DiamondWidth  = size.x * 0.5f;
      var DiamondHeight = size.y * 0.5f;

#if QUANTUM_XY
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.up * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.up * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.down * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.down * DiamondHeight);
#else
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.forward * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.forward * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.back * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.back * DiamondHeight);
#endif
    }

    public static void DrawGizmoVector3D(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 25.0f) {
      Gizmos.DrawLine(start, end);
      var     d     = (end - start).normalized;
      Vector3 right = Quaternion.LookRotation(d) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
      Vector3 left  = Quaternion.LookRotation(d) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
      Gizmos.DrawLine(end, end + right * arrowHeadLength);
      Gizmos.DrawLine(end, end + left * arrowHeadLength);
    }

    public static void DrawGizmoVector(Vector3 start, Vector3 end, float arrowHeadLength = DefaultArrowHeadLength, float arrowHeadAngle = DefaultArrowHeadAngle) {
      Gizmos.DrawLine(start, end);

      var l = (start - end).magnitude;

      if (l < arrowHeadLength * 2) {
        arrowHeadLength = l / 2;
      }

      var d = (start - end).normalized;

      float cos = Mathf.Cos(arrowHeadAngle * Mathf.Deg2Rad);
      float sin = Mathf.Sin(arrowHeadAngle * Mathf.Deg2Rad);

      Vector3 left = Vector3.zero;
#if QUANTUM_XY
      left.x = d.x * cos - d.y * sin;
      left.y = d.x * sin + d.y * cos;
#else
      left.x = d.x * cos - d.z * sin;
      left.z = d.x * sin + d.z * cos;
#endif

      sin = -sin;

      Vector3 right = Vector3.zero;
#if QUANTUM_XY
      right.x = d.x * cos - d.y * sin;
      right.y = d.x * sin + d.y * cos;
#else
      right.x = d.x * cos - d.z * sin;
      right.z = d.x * sin + d.z * cos;
#endif

      Gizmos.DrawLine(end, end + left * arrowHeadLength);
      Gizmos.DrawLine(end, end + right * arrowHeadLength);
    }

    public static void DrawGizmoArc(Vector3 position, Vector3 normal, Vector3 from, float angle, float radius, Color color, float alphaRatio = 1.0f, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR
      Handles.color = color;
      Gizmos.color  = color;

      if (style.IsWireframeEnabled) {
        Handles.DrawWireArc(position, normal, from, angle, radius);
        if (!style.IsFillEnabled) {
          var to = Quaternion.AngleAxis(angle, normal) * from;
          Gizmos.color = color.Alpha(color.a * alphaRatio);
          Gizmos.DrawRay(position, from * radius);
          Gizmos.DrawRay(position, to * radius);
        }
      }

      if (style.IsFillEnabled) {
        Handles.color = color.Alpha(color.a * alphaRatio);
        Handles.DrawSolidArc(position, normal, from, angle, radius);
      }

      Gizmos.color = Handles.color = Color.white;
#endif
    }

    public static void DrawGizmoDisc(Vector3 position, Vector3 normal, float radius, Color color, float alphaRatio = 1.0f, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR
      Handles.color = color;
      Gizmos.color  = color;

      if (style.IsWireframeEnabled) {
        Handles.DrawWireDisc(position, normal, radius);
      }

      if (style.IsFillEnabled) {
        Handles.color = Handles.color.Alpha(Handles.color.a * alphaRatio);
        Handles.DrawSolidDisc(position, normal, radius);
      }

      Gizmos.color = Handles.color = Color.white;
#endif
    }

    public static void DrawGizmosEdge(Vector3 start, Vector3 end, float height, Color color, QuantumGizmoStyle style = default) {
      Gizmos.color = color;

      if (Math.Abs(height) > float.Epsilon) {
        var startToEnd = end - start;
        var edgeSize   = startToEnd.magnitude;
        var size       = new Vector3(edgeSize, 0);
        var center     = start + startToEnd / 2;
#if QUANTUM_XY
        size.z = height;
        center.z += height / 2;
#else
        size.y   =  height;
        center.y += height / 2;
#endif
        DrawGizmosBox(center, size, color, rotation: Quaternion.FromToRotation(Vector3.right, startToEnd), style: style);
      } else {
        Gizmos.DrawLine(start, end);
      }

      Gizmos.color = Color.white;
    }

    public static void DrawGizmosCapsule(Vector3 center, float radius, float extent, Color color, Quaternion? rotation = null, QuantumGizmoStyle style = default) {
      var matrix = Matrix4x4.TRS(center, rotation ?? Quaternion.identity, Vector3.one);
      DrawGizmosCapsule(matrix, radius, extent, color, style: style);
    }

    public static void DrawGizmosCapsule(Matrix4x4 matrix, float radius, float extent, Color color, QuantumGizmoStyle style = default) {
#if UNITY_EDITOR
      Handles.matrix = matrix;
      Handles.color = color;

      // TODO: handle QuantumGizmoStyle.IsFillEnabled (see Box gizmos for reference)
      
      var cylinderTop = Vector3.up * extent;
      var cylinderBottom = Vector3.down * extent;
      var radiusRight = Vector3.right * radius;
      var radiusForward = Vector3.forward * radius;

      Handles.DrawWireArc(cylinderTop, Vector3.up, Vector3.left, 360.0f, radius);
      Handles.DrawWireArc(cylinderBottom, Vector3.down, Vector3.left, 360.0f, radius);
      
      Handles.DrawWireArc(cylinderTop, Vector3.right, Vector3.back, 180.0f, radius);
      Handles.DrawWireArc(cylinderTop, Vector3.forward, Vector3.right, 180.0f, radius);
      Handles.DrawWireArc(cylinderBottom, Vector3.left, Vector3.back, 180.0f, radius);
      Handles.DrawWireArc(cylinderBottom, Vector3.back, Vector3.right, 180.0f, radius);
      
      Handles.DrawLine(cylinderTop + radiusRight, cylinderBottom + radiusRight);
      Handles.DrawLine(cylinderTop - radiusRight, cylinderBottom - radiusRight);
      Handles.DrawLine(cylinderTop + radiusForward, cylinderBottom + radiusForward);
      Handles.DrawLine(cylinderTop - radiusForward, cylinderBottom - radiusForward);

      Handles.matrix = Matrix4x4.identity;
      Handles.color = Color.white;
#endif
    }
  }

  [Serializable]
  public struct QuantumGizmoStyle {
    public static QuantumGizmoStyle FillDisabled => new QuantumGizmoStyle() { DisableFill = true };

    public bool DisableFill;

    public bool IsFillEnabled      => !DisableFill;
    public bool IsWireframeEnabled => true;
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/ProgressBar.cs

namespace Quantum {
  using System;
  using System.Diagnostics;
  using UnityEngine;
  using Debug = UnityEngine.Debug;
  using UnityEditor;
  
  
  public class ProgressBar : IDisposable {
    float  _progress;
    string _info;
#pragma warning disable CS0414 // The private field is assigned but its value is never used (#if UNITY_EDITOR)
    string _title;
    bool   _isCancelable;
#pragma warning restore CS0414 // The private field is assigned but its value is never used
    Stopwatch _sw;

    public ProgressBar(string title, bool isCancelable = false, bool logStopwatch = false) {
      _title        = title;
      _isCancelable = isCancelable;
      if (logStopwatch) {
        _sw = Stopwatch.StartNew();
      }
    }

    public string Info {
      set {
        DisplayStopwatch();
        _info     = value;
        _progress = 0.0f;
        Display();
      }
    }

    public float Progress {
      set {
        bool hasChanged = Mathf.Abs(_progress - value) > 0.01f;
        if (!hasChanged)
          return;

        _progress = value;
        Display();
      }

      get {
        return _progress;
      }
    }

    public void SetInfo(string value) {
      Info = value;
    }

    public void SetProgress(float value) {
      Progress = value;
    }

    public void Dispose() {
#if UNITY_EDITOR
      EditorUtility.ClearProgressBar();
      DisplayStopwatch();
#endif
    }

    private void Display() {
#if UNITY_EDITOR
      if (_isCancelable) {
        bool isCanceled = EditorUtility.DisplayCancelableProgressBar(_title, _info, _progress);
        if (isCanceled) {
          throw new Exception(_title + " canceled");
        }
      } else {
        EditorUtility.DisplayProgressBar(_title, _info, _progress);
      }
#endif
    }

    private void DisplayStopwatch() {
      if (_sw != null && !string.IsNullOrEmpty(_info)) {
        Debug.LogFormat("'{0}' took {1} ms", _info, _sw.ElapsedMilliseconds);
        _sw.Reset();
        _sw.Start();
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/QuantumColor.cs

namespace Quantum {
  using UnityEngine;

  public static class QuantumColor {
    public static Color32 Log {
      get {
        bool isDarkMode = false;
#if UNITY_EDITOR
        isDarkMode = UnityEditor.EditorGUIUtility.isProSkin;
#endif
        return isDarkMode ? new Color32(32, 203, 145, 255) : new Color32(18, 75, 60, 255);
      }
    }
  }
}


#endregion


#region Assets/Photon/Quantum/Runtime/Utils/QuantumGlobalScriptableObject.cs

namespace Quantum {
  using System;

  partial class QuantumGlobalScriptableObject<T> {

    [Obsolete("Use " + nameof(Global) + " instead.")]
    public static T Instance => Global;

    public static T Global {
      get => GlobalInternal;
      protected set => GlobalInternal = value;
    } 

    public static bool TryGetGlobal(out T global) => TryGetGlobalInternal(out global);
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/RectExtensions.cs

namespace Quantum {
  using UnityEditor;
  using UnityEngine;

  public static class EditorRectUtils {
    public static Rect SetWidth(this Rect r, float w) {
      r.width = w;
      return r;
    }

    public static Rect SetWidthHeight(this Rect r, Vector2 v) {
      r.width  = v.x;
      r.height = v.y;
      return r;
    }

    public static Rect SetWidthHeight(this Rect r, float w, float h) {
      r.width  = w;
      r.height = h;
      return r;
    }

    public static Rect AddWidth(this Rect r, float w) {
      r.width += w;
      return r;
    }

    public static Rect AddHeight(this Rect r, float h) {
      r.height += h;
      return r;
    }

    public static Rect SetHeight(this Rect r, float h) {
      r.height = h;
      return r;
    }

    public static Rect AddXY(this Rect r, Vector2 xy) {
      r.x += xy.x;
      r.y += xy.y;
      return r;
    }

    public static Rect AddXY(this Rect r, float x, float y) {
      r.x += x;
      r.y += y;
      return r;
    }

    public static Rect AddX(this Rect r, float x) {
      r.x += x;
      return r;
    }

    public static Rect AddY(this Rect r, float y) {
      r.y += y;
      return r;
    }

    public static Rect SetY(this Rect r, float y) {
      r.y = y;
      return r;
    }

    public static Rect SetX(this Rect r, float x) {
      r.x = x;
      return r;
    }

    public static Rect SetXMin(this Rect r, float x) {
      r.xMin = x;
      return r;
    }

    public static Rect SetXMax(this Rect r, float x) {
      r.xMax = x;
      return r;
    }

    public static Rect SetYMin(this Rect r, float y) {
      r.yMin = y;
      return r;
    }

    public static Rect SetYMax(this Rect r, float y) {
      r.yMax = y;
      return r;
    }

    public static Rect AddXMin(this Rect r, float x) {
      r.xMin += x;
      return r;
    }

    public static Rect AddXMax(this Rect r, float x) {
      r.xMax += x;
      return r;
    }

    public static Rect AddYMin(this Rect r, float y) {
      r.yMin += y;
      return r;
    }

    public static Rect AddYMax(this Rect r, float y) {
      r.yMax += y;
      return r;
    }

    public static Rect Adjust(this Rect r, float x, float y, float w, float h) {
      r.x      += x;
      r.y      += y;
      r.width  += w;
      r.height += h;
      return r;
    }

    public static Rect ToRect(this Vector2 v, float w, float h) {
      return new Rect(v.x, v.y, w, h);
    }

    public static Rect ZeroXY(this Rect r) {
      return new Rect(0, 0, r.width, r.height);
    }

    public static Vector2 ToVector2(this Rect r) {
      return new Vector2(r.width, r.height);
    }

#if UNITY_EDITOR
    public static Rect AddLine(this Rect r, int count = 1) {
      return AddY(r, count * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
    }

    public static Rect SetLineHeight(this Rect r) {
      return SetHeight(r, EditorGUIUtility.singleLineHeight);
    }
#endif
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/ReflectionUtils.cs

namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using UnityEditor;

  public static class ReflectionUtils {
    public const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    public class TypeHierarchyComparer : IComparer<Type> {
      public int Compare(Type x, Type y) {
        if (x == y) {
          return 0;
        }
        if (x == null) {
          return -1;
        }
        if (y == null) {
          return 1;
        }
        if (x.IsSubclassOf(y) == true) {
          return -1;
        }
        if (y.IsSubclassOf(x) == true) {
          return 1;
        }
        return 0;
      }
      
      public static readonly TypeHierarchyComparer Instance = new TypeHierarchyComparer();
    }

    public static Type GetUnityLeafType(this Type type) {
      if (type.HasElementType) {
        type = type.GetElementType();
      } else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
        type = type.GetGenericArguments()[0];
      }

      return type;
    }

#if UNITY_EDITOR

    public static T CreateEditorMethodDelegate<T>(string editorAssemblyTypeName, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      return CreateMethodDelegate<T>(typeof(UnityEditor.Editor).Assembly, editorAssemblyTypeName, methodName, flags);
    }

    public static Delegate CreateEditorMethodDelegate(string editorAssemblyTypeName, string methodName, BindingFlags flags, Type delegateType) {
      return CreateMethodDelegate(typeof(UnityEditor.Editor).Assembly, editorAssemblyTypeName, methodName, flags, delegateType);
    }

#endif

    public static T CreateMethodDelegate<T>(this Type type, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      try {
        return CreateMethodDelegateInternal<T>(type, methodName, flags);
      } catch (Exception ex) {
        throw new InvalidOperationException(CreateMethodExceptionMessage<T>(type.Assembly, type.FullName, methodName, flags), ex);
      }
    }

    public static Delegate CreateMethodDelegate(this Type type, string methodName, BindingFlags flags, Type delegateType) {
      return CreateMethodDelegateInternal(type, methodName, flags, delegateType);
    }

    public static T CreateMethodDelegate<T>(Assembly assembly, string typeName, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      try {
        var type = assembly.GetType(typeName, true);
        return CreateMethodDelegateInternal<T>(type, methodName, flags);
      } catch (Exception ex) {
        throw new InvalidOperationException(CreateMethodExceptionMessage<T>(assembly, typeName, methodName, flags), ex);
      }
    }

    public static Delegate CreateMethodDelegate(Assembly assembly, string typeName, string methodName, BindingFlags flags, Type delegateType) {
      try {
        var type = assembly.GetType(typeName, true);
        return CreateMethodDelegateInternal(type, methodName, flags, delegateType);
      } catch (Exception ex) {
        throw new InvalidOperationException(CreateMethodExceptionMessage(assembly, typeName, methodName, flags, delegateType), ex);
      }
    }

    public static T CreateMethodDelegate<T>(this Type type, string methodName, BindingFlags flags, Type delegateType, params DelegateSwizzle[] fallbackSwizzles) where T : Delegate {
      try {
        MethodInfo method = GetMethodOrThrow(type, methodName, flags, delegateType, fallbackSwizzles, out var swizzle);

        var delegateParameters = typeof(T).GetMethod("Invoke").GetParameters();
        var parameters         = new List<ParameterExpression>();

        for (int i = 0; i < delegateParameters.Length; ++i) {
          parameters.Add(Expression.Parameter(delegateParameters[i].ParameterType, $"param_{i}"));
        }

        var convertedParameters = new List<Expression>();
        {
          var methodParameters = method.GetParameters();
          if (swizzle == null) {
            for (int i = 0, j = method.IsStatic ? 0 : 1; i < methodParameters.Length; ++i, ++j) {
              convertedParameters.Add(Expression.Convert(parameters[j], methodParameters[i].ParameterType));
            }
          } else {
            var swizzledParameters = swizzle.Swizzle(parameters.ToArray());
            for (int i = 0, j = method.IsStatic ? 0 : 1; i < methodParameters.Length; ++i, ++j) {
              convertedParameters.Add(Expression.Convert(swizzledParameters[j], methodParameters[i].ParameterType));
            }
          }
        }

        MethodCallExpression callExpression;
        if (method.IsStatic) {
          callExpression = Expression.Call(method, convertedParameters);
        } else {
          var instance = Expression.Convert(parameters[0], method.DeclaringType);
          callExpression = Expression.Call(instance, method, convertedParameters);
        }

        var l   = Expression.Lambda(typeof(T), callExpression, parameters);
        var del = l.Compile();
        return (T)del;
      } catch (Exception ex) {
        throw new InvalidOperationException(CreateMethodExceptionMessage<T>(type.Assembly, type.FullName, methodName, flags), ex);
      }
    }

    public static T CreateConstructorDelegate<T>(this Type type, BindingFlags flags, Type delegateType, params DelegateSwizzle[] fallbackSwizzles) where T : Delegate {
      try {
        var constructor = GetConstructorOrThrow(type, flags, delegateType, fallbackSwizzles, out var swizzle);

        var delegateParameters = typeof(T).GetMethod("Invoke").GetParameters();
        var parameters         = new List<ParameterExpression>();

        for (int i = 0; i < delegateParameters.Length; ++i) {
          parameters.Add(Expression.Parameter(delegateParameters[i].ParameterType, $"param_{i}"));
        }

        var convertedParameters = new List<Expression>();
        {
          var constructorParameters = constructor.GetParameters();
          if (swizzle == null) {
            for (int i = 0, j = 0; i < constructorParameters.Length; ++i, ++j) {
              convertedParameters.Add(Expression.Convert(parameters[j], constructorParameters[i].ParameterType));
            }
          } else {
            var swizzledParameters = swizzle.Swizzle(parameters.ToArray());
            for (int i = 0, j = 0; i < constructorParameters.Length; ++i, ++j) {
              convertedParameters.Add(Expression.Convert(swizzledParameters[j], constructorParameters[i].ParameterType));
            }
          }
        }

        NewExpression newExpression = Expression.New(constructor, convertedParameters);
        var           l             = Expression.Lambda(typeof(T), newExpression, parameters);
        var           del           = l.Compile();
        return (T)del;
      } catch (Exception ex) {
        throw new InvalidOperationException(CreateConstructorExceptionMessage(type.Assembly, type.FullName, flags), ex);
      }
    }

    public static FieldInfo GetFieldOrThrow(this Type type, string fieldName, BindingFlags flags = DefaultBindingFlags) {
      var field = type.GetField(fieldName, flags);
      if (field == null) {
        throw new ArgumentOutOfRangeException(nameof(fieldName), CreateFieldExceptionMessage(type.Assembly, type.FullName, fieldName, flags));
      }

      return field;
    }

    public static FieldInfo GetFieldOrThrow<T>(this Type type, string fieldName, BindingFlags flags = DefaultBindingFlags) {
      return GetFieldOrThrow(type, fieldName, typeof(T), flags);
    }

    public static FieldInfo GetFieldOrThrow(this Type type, string fieldName, Type fieldType, BindingFlags flags = DefaultBindingFlags) {
      var field = type.GetField(fieldName, flags);
      if (field == null) {
        throw new ArgumentOutOfRangeException(nameof(fieldName), CreateFieldExceptionMessage(type.Assembly, type.FullName, fieldName, flags));
      }

      if (field.FieldType != fieldType) {
        throw new InvalidProgramException($"Field {type.FullName}.{fieldName} is of type {field.FieldType}, not expected {fieldType}");
      }

      return field;
    }

    public static PropertyInfo GetPropertyOrThrow<T>(this Type type, string propertyName, BindingFlags flags = DefaultBindingFlags) {
      return GetPropertyOrThrow(type, propertyName, typeof(T), flags);
    }

    public static PropertyInfo GetPropertyOrThrow(this Type type, string propertyName, Type propertyType, BindingFlags flags = DefaultBindingFlags) {
      var property = type.GetProperty(propertyName, flags);
      if (property == null) {
        throw new ArgumentOutOfRangeException(nameof(propertyName), CreateFieldExceptionMessage(type.Assembly, type.FullName, propertyName, flags));
      }

      if (property.PropertyType != propertyType) {
        throw new InvalidProgramException($"Property {type.FullName}.{propertyName} is of type {property.PropertyType}, not expected {propertyType}");
      }

      return property;
    }

    public static ConstructorInfo GetConstructorInfoOrThrow(this Type type, Type[] types, BindingFlags flags = DefaultBindingFlags) {
      var constructor = type.GetConstructor(flags, null, types, null);
      if (constructor == null) {
        throw new ArgumentOutOfRangeException(nameof(types), CreateConstructorExceptionMessage(type.Assembly, type.FullName, types, flags));
      }

      return constructor;
    }

    public static Type GetNestedTypeOrThrow(this Type type, string name, BindingFlags flags) {
      var result = type.GetNestedType(name, flags);
      if (result == null) {
        throw new ArgumentOutOfRangeException(nameof(name), CreateFieldExceptionMessage(type.Assembly, type.FullName, name, flags));
      }

      return result;
    }

    public static InstanceAccessor<FieldType> CreateFieldAccessor<FieldType>(this Type type, string fieldName, Type expectedFieldType = null, BindingFlags flags = DefaultBindingFlags) {
      var field = type.GetFieldOrThrow(fieldName, expectedFieldType ?? typeof(FieldType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return CreateAccessorInternal<FieldType>(field);
    }

    public static StaticAccessor<object> CreateStaticFieldAccessor(this Type type, string fieldName, Type expectedFieldType = null) {
      return CreateStaticFieldAccessor<object>(type, fieldName, expectedFieldType);
    }

    public static StaticAccessor<FieldType> CreateStaticFieldAccessor<FieldType>(this Type type, string fieldName, Type expectedFieldType = null) {
      var field = type.GetFieldOrThrow(fieldName, expectedFieldType ?? typeof(FieldType), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      return CreateStaticAccessorInternal<FieldType>(field);
    }

    public static InstanceAccessor<PropertyType> CreatePropertyAccessor<PropertyType>(this Type type, string fieldName, Type expectedPropertyType = null, BindingFlags flags = DefaultBindingFlags) {
      var field = type.GetPropertyOrThrow(fieldName, expectedPropertyType ?? typeof(PropertyType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return CreateAccessorInternal<PropertyType>(field);
    }

    public static StaticAccessor<object> CreateStaticPropertyAccessor(this Type type, string fieldName, Type expectedFieldType = null) {
      return CreateStaticPropertyAccessor<object>(type, fieldName, expectedFieldType);
    }

    public static StaticAccessor<FieldType> CreateStaticPropertyAccessor<FieldType>(this Type type, string fieldName, Type expectedFieldType = null) {
      var field = type.GetPropertyOrThrow(fieldName, expectedFieldType ?? typeof(FieldType), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      return CreateStaticAccessorInternal<FieldType>(field);
    }

    private static string CreateMethodExceptionMessage<T>(Assembly assembly, string typeName, string methodName, BindingFlags flags) {
      return CreateMethodExceptionMessage(assembly, typeName, methodName, flags, typeof(T));
    }

    private static string CreateMethodExceptionMessage(Assembly assembly, string typeName, string methodName, BindingFlags flags, Type delegateType) {
      return $"{assembly.FullName}.{typeName}.{methodName} with flags: {flags} and type: {delegateType}";
    }

    private static string CreateFieldExceptionMessage(Assembly assembly, string typeName, string fieldName, BindingFlags flags) {
      return $"{assembly.FullName}.{typeName}.{fieldName} with flags: {flags}";
    }

    private static string CreateConstructorExceptionMessage(Assembly assembly, string typeName, BindingFlags flags) {
      return $"{assembly.FullName}.{typeName}() with flags: {flags}";
    }

    private static string CreateConstructorExceptionMessage(Assembly assembly, string typeName, Type[] types, BindingFlags flags) {
      return $"{assembly.FullName}.{typeName}({(string.Join(", ", types.Select(x => x.FullName)))}) with flags: {flags}";
    }

    private static T CreateMethodDelegateInternal<T>(this Type type, string name, BindingFlags flags) where T : Delegate {
      return (T)CreateMethodDelegateInternal(type, name, flags, typeof(T));
    }

    private static Delegate CreateMethodDelegateInternal(this Type type, string name, BindingFlags flags, Type delegateType) {
      MethodInfo method = GetMethodOrThrow(type, name, flags, delegateType);
      return Delegate.CreateDelegate(delegateType, null, method);
    }

    private static MethodInfo GetMethodOrThrow(Type type, string name, BindingFlags flags, Type delegateType) {
      return GetMethodOrThrow(type, name, flags, delegateType, Array.Empty<DelegateSwizzle>(), out _);
    }

    private static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type returnType, params Type[] parameters) {
      var method = type.GetMethod(name, flags, null, parameters, null);

      if (method == null) {
        return null;
      }

      if (method.ReturnType != returnType) {
        return null;
      }

      return method;
    }

    private static ConstructorInfo GetConstructorOrThrow(Type type, BindingFlags flags, Type delegateType, DelegateSwizzle[] swizzles, out DelegateSwizzle firstMatchingSwizzle) {
      var delegateMethod = delegateType.GetMethod("Invoke");

      var allDelegateParameters = delegateMethod.GetParameters().Select(x => x.ParameterType).ToArray();

      var constructor = type.GetConstructor(flags, null, allDelegateParameters, null);
      if (constructor != null) {
        firstMatchingSwizzle = null;
        return constructor;
      }

      if (swizzles != null) {
        foreach (var swizzle in swizzles) {
          Type[] swizzled = swizzle.Swizzle(allDelegateParameters);
          constructor = type.GetConstructor(flags, null, swizzled, null);
          if (constructor != null) {
            firstMatchingSwizzle = swizzle;
            return constructor;
          }
        }
      }

      var constructors = type.GetConstructors(flags);
      throw new ArgumentOutOfRangeException(nameof(delegateType), $"No matching constructor found for {type}, " +
        $"signature \"{delegateType}\", " +
        $"flags \"{flags}\" and " +
        $"params: {string.Join(", ", allDelegateParameters.Select(x => x.FullName))}" +
        $", candidates are\n: {(string.Join("\n", constructors.Select(x => x.ToString())))}");
    }

    private static MethodInfo GetMethodOrThrow(Type type, string name, BindingFlags flags, Type delegateType, DelegateSwizzle[] swizzles, out DelegateSwizzle firstMatchingSwizzle) {
      var delegateMethod = delegateType.GetMethod("Invoke");

      var allDelegateParameters = delegateMethod.GetParameters().Select(x => x.ParameterType).ToArray();

      var method = FindMethod(type, name, flags, delegateMethod.ReturnType, flags.HasFlag(BindingFlags.Static) ? allDelegateParameters : allDelegateParameters.Skip(1).ToArray());
      if (method != null) {
        firstMatchingSwizzle = null;
        return method;
      }

      if (swizzles != null) {
        foreach (var swizzle in swizzles) {
          Type[] swizzled = swizzle.Swizzle(allDelegateParameters);
          if (!flags.HasFlag(BindingFlags.Static) && swizzled[0] != type) {
            throw new InvalidOperationException();
          }

          method = FindMethod(type, name, flags, delegateMethod.ReturnType, flags.HasFlag(BindingFlags.Static) ? swizzled : swizzled.Skip(1).ToArray());
          if (method != null) {
            firstMatchingSwizzle = swizzle;
            return method;
          }
        }
      }

      var methods = type.GetMethods(flags);
      throw new ArgumentOutOfRangeException(nameof(name), $"No method found matching name \"{name}\", " +
        $"signature \"{delegateType}\", " +
        $"flags \"{flags}\" and " +
        $"params: {string.Join(", ", allDelegateParameters.Select(x => x.FullName))}" +
        $", candidates are\n: {(string.Join("\n", methods.Select(x => x.ToString())))}");
    }

    public static bool IsArrayOrList(this Type listType) {
      if (listType.IsArray) {
        return true;
      } else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)) {
        return true;
      }

      return false;
    }

    public static Type GetArrayOrListElementType(this Type listType) {
      if (listType.IsArray) {
        return listType.GetElementType();
      } else if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)) {
        return listType.GetGenericArguments()[0];
      }

      return null;
    }

    public static Type MakeFuncType(params Type[] types) {
      return GetFuncType(types.Length).MakeGenericType(types);
    }

    private static Type GetFuncType(int argumentCount) {
      switch (argumentCount) {
        case 1:  return typeof(Func<>);
        case 2:  return typeof(Func<,>);
        case 3:  return typeof(Func<,,>);
        case 4:  return typeof(Func<,,,>);
        case 5:  return typeof(Func<,,,,>);
        case 6:  return typeof(Func<,,,,,>);
        default: throw new ArgumentOutOfRangeException(nameof(argumentCount));
      }
    }

    public static Type MakeActionType(params Type[] types) {
      if (types.Length == 0) return typeof(Action);
      return GetActionType(types.Length).MakeGenericType(types);
    }

    private static Type GetActionType(int argumentCount) {
      switch (argumentCount) {
        case 1:  return typeof(Action<>);
        case 2:  return typeof(Action<,>);
        case 3:  return typeof(Action<,,>);
        case 4:  return typeof(Action<,,,>);
        case 5:  return typeof(Action<,,,,>);
        case 6:  return typeof(Action<,,,,,>);
        default: throw new ArgumentOutOfRangeException(nameof(argumentCount));
      }
    }

    private static StaticAccessor<T> CreateStaticAccessorInternal<T>(MemberInfo fieldOrProperty) {
      try {
        var  valueParameter = Expression.Parameter(typeof(T), "value");
        bool canWrite       = true;

        UnaryExpression  valueExpression;
        MemberExpression memberExpression;
        if (fieldOrProperty is PropertyInfo property) {
          valueExpression  = Expression.Convert(valueParameter, property.PropertyType);
          memberExpression = Expression.Property(null, property);
          canWrite         = property.CanWrite;
        } else {
          var field = (FieldInfo)fieldOrProperty;
          valueExpression  = Expression.Convert(valueParameter, field.FieldType);
          memberExpression = Expression.Field(null, field);
          canWrite         = field.IsInitOnly == false;
        }

        Func<T> getter;
        var     getExpression = Expression.Convert(memberExpression, typeof(T));
        var     getLambda     = Expression.Lambda<Func<T>>(getExpression);
        getter = getLambda.Compile();

        Action<T> setter = null;
        if (canWrite) {
          var setExpression = Expression.Assign(memberExpression, valueExpression);
          var setLambda     = Expression.Lambda<Action<T>>(setExpression, valueParameter);
          setter = setLambda.Compile();
        }

        return new StaticAccessor<T>() {
          GetValue = getter,
          SetValue = setter
        };
      } catch (Exception ex) {
        throw new InvalidOperationException($"Failed to create accessor for {fieldOrProperty.DeclaringType}.{fieldOrProperty.Name}", ex);
      }
    }

    private static InstanceAccessor<T> CreateAccessorInternal<T>(MemberInfo fieldOrProperty) {
      try {
        var instanceParameter  = Expression.Parameter(typeof(object), "instance");
        var instanceExpression = Expression.Convert(instanceParameter, fieldOrProperty.DeclaringType);

        var  valueParameter = Expression.Parameter(typeof(T), "value");
        bool canWrite       = true;

        UnaryExpression  valueExpression;
        MemberExpression memberExpression;
        if (fieldOrProperty is PropertyInfo property) {
          valueExpression  = Expression.Convert(valueParameter, property.PropertyType);
          memberExpression = Expression.Property(instanceExpression, property);
          canWrite         = property.CanWrite;
        } else {
          var field = (FieldInfo)fieldOrProperty;
          valueExpression  = Expression.Convert(valueParameter, field.FieldType);
          memberExpression = Expression.Field(instanceExpression, field);
          canWrite         = field.IsInitOnly == false;
        }

        Func<object, T> getter;

        var getExpression = Expression.Convert(memberExpression, typeof(T));
        var getLambda     = Expression.Lambda<Func<object, T>>(getExpression, instanceParameter);
        getter = getLambda.Compile();

        Action<object, T> setter = null;
        if (canWrite) {
          var setExpression = Expression.Assign(memberExpression, valueExpression);
          var setLambda     = Expression.Lambda<Action<object, T>>(setExpression, instanceParameter, valueParameter);
          setter = setLambda.Compile();
        }

        return new InstanceAccessor<T>() {
          GetValue = getter,
          SetValue = setter
        };
      } catch (Exception ex) {
        throw new InvalidOperationException($"Failed to create accessor for {fieldOrProperty.DeclaringType}.{fieldOrProperty.Name}", ex);
      }
    }

    public struct InstanceAccessor<TValue> {
      public Func<object, TValue>   GetValue;
      public Action<object, TValue> SetValue;
    }

    public struct StaticAccessor<TValue> {
      public Func<TValue>   GetValue;
      public Action<TValue> SetValue;
    }

    public class DelegateSwizzle {
      private int[] _args;

      public int Count => _args.Length;

      public DelegateSwizzle(params int[] args) {
        _args = args;
      }

      public T[] Swizzle<T>(T[] inputTypes) {
        T[] result = new T[_args.Length];

        for (int i = 0; i < _args.Length; ++i) {
          result[i] = inputTypes[_args[i]];
        }

        return result;
      }
    }
  }
}

#endregion


#region Assets/Photon/Quantum/Runtime/Utils/SerializedObjectExtensions.cs

#if UNITY_EDITOR
namespace Quantum {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Text;
  using System.Text.RegularExpressions;
  using Photon.Analyzer;
  using UnityEditor;
  
  public static class SerializedObjectExtensions {
    [StaticField(StaticFieldResetMode.None)]
    private static readonly Regex _arrayElementRegex = new Regex(@"\.Array\.data\[\d+\]$", RegexOptions.Compiled);

    public static SerializedProperty FindPropertyOrThrow(this SerializedObject so, string propertyPath) {
      var result = so.FindProperty(propertyPath);
      if (result == null)
        throw new ArgumentOutOfRangeException($"Property not found: {propertyPath}");
      return result;
    }

    public static SerializedProperty FindPropertyRelativeOrThrow(this SerializedProperty sp, string relativePropertyPath) {
      var result = sp.FindPropertyRelative(relativePropertyPath);
      if (result == null)
        throw new ArgumentOutOfRangeException($"Property not found: {relativePropertyPath} (in {sp.propertyPath})");
      return result;
    }

    public static SerializedProperty FindPropertyRelativeToParent(this SerializedProperty property, string relativePath) {
      SerializedProperty otherProperty;

      var path = property.propertyPath;

      // array element?
      if (path[path.Length - 1] == ']') {
        var match = _arrayElementRegex.Match(path);
        if (match.Success) {
          path = path.Substring(0, match.Index);
        }
      }

      var lastDotIndex = path.LastIndexOf('.');
      if (lastDotIndex < 0) {
        otherProperty = property.serializedObject.FindProperty(relativePath);
      } else {
        otherProperty = property.serializedObject.FindProperty(path.Substring(0, lastDotIndex));
        if (otherProperty != null) {
          otherProperty = otherProperty.FindPropertyRelative(relativePath);
        }
      }

      return otherProperty;
    }

    public static SerializedProperty FindPropertyRelativeToParentOrThrow(this SerializedProperty property, string relativePath) {
      var result = property.FindPropertyRelativeToParent(relativePath);
      if (result == null) {
        throw new ArgumentOutOfRangeException($"Property relative to the parent of \"{property.propertyPath}\" not found: {relativePath}");
      }

      return result;
    }

    public static Int64 GetIntegerValue(this SerializedProperty sp) {
      switch (sp.type) {
        case "int":
        case "bool": return sp.intValue;
        case "long": return sp.longValue;
        case "FP":   return sp.FindPropertyRelative("RawValue").longValue;
        case "Enum": return sp.intValue;
        default:
          switch (sp.propertyType) {
            case SerializedPropertyType.ObjectReference:
              return sp.objectReferenceInstanceIDValue;
          }

          return 0;
      }
    }

    public static void SetIntegerValue(this SerializedProperty sp, long value) {
      switch (sp.type) {
        case "int":
          sp.intValue = (int)value;
          break;
        case "bool":
          sp.boolValue = value != 0;
          break;
        case "long":
          sp.longValue = value;
          break;
        case "FP":
          sp.FindPropertyRelative("RawValue").longValue = value;
          break;
        case "Enum":
          sp.intValue = (int)value;
          break;
        default:
          throw new NotSupportedException($"Type {sp.type} is not supported");
      }
    }


    public static SerializedPropertyEnumerable Children(this SerializedProperty property, bool visibleOnly = true) {
      return new SerializedPropertyEnumerable(property, visibleOnly);
    }

    public static string GetPropertyPath<T, U>(Expression<Func<T, U>> propertyLambda) {
      Expression    expression  = propertyLambda.Body;
      StringBuilder pathBuilder = new StringBuilder();

      for (;;) {
        var fieldExpression = expression as MemberExpression;
        if (fieldExpression?.Member is FieldInfo field) {
          if (pathBuilder.Length != 0) {
            pathBuilder.Insert(0, '.');
          }

          pathBuilder.Insert(0, field.Name);
          expression = fieldExpression.Expression;
        } else {
          if (expression is ParameterExpression parameterExpression) {
            return pathBuilder.ToString();
          } else {
            throw new ArgumentException($"Only field expressions allowed: {expression}");
          }
        }
      }
    }


    public static SerializedProperty GetArraySizePropertyOrThrow(this SerializedProperty prop) {
      if (prop == null) {
        throw new ArgumentNullException(nameof(prop));
      }

      if (!prop.isArray) {
        throw new ArgumentException("Not an array", nameof(prop));
      }

      var copy = prop.Copy();
      if (!copy.Next(true) || !copy.Next(true)) {
        throw new InvalidOperationException();
      }

      if (copy.propertyType != SerializedPropertyType.ArraySize) {
        throw new InvalidOperationException();
      }

      return copy;
    }

    public struct SerializedPropertyEnumerable : IEnumerable<SerializedProperty> {
      private SerializedProperty property;
      private bool               visible;

      public SerializedPropertyEnumerable(SerializedProperty property, bool visible) {
        this.property = property;
        this.visible  = visible;
      }

      public SerializedPropertyEnumerator GetEnumerator() {
        return new SerializedPropertyEnumerator(property, visible);
      }

      IEnumerator<SerializedProperty> IEnumerable<SerializedProperty>.GetEnumerator() {
        return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
      }
    }

    public struct SerializedPropertyEnumerator : IEnumerator<SerializedProperty> {
      private SerializedProperty current;
      private bool               enterChildren;
      private bool               visible;
      private int                parentDepth;

      public SerializedPropertyEnumerator(SerializedProperty parent, bool visible) {
        current       = parent.Copy();
        enterChildren = true;
        parentDepth   = parent.depth;
        this.visible  = visible;
      }

      public SerializedProperty Current => current;

      SerializedProperty IEnumerator<SerializedProperty>.Current => current;

      object IEnumerator.Current => current;

      public void Dispose() {
        current.Dispose();
      }

      public bool MoveNext() {
        bool entered = visible ? current.NextVisible(enterChildren) : current.Next(enterChildren);
        enterChildren = false;
        if (!entered) {
          return false;
        }

        if (current.depth <= parentDepth) {
          return false;
        }

        return true;
      }

      public void Reset() {
        throw new NotImplementedException();
      }
    }
  }

  public class SerializedPropertyPathBuilder<T> {
    public static string GetPropertyPath<U>(Expression<Func<T, U>> expression) {
      return SerializedObjectExtensions.GetPropertyPath(expression);
    }
  }

  public class SerializedPropertyEqualityComparer : IEqualityComparer<SerializedProperty> {
    [StaticField(StaticFieldResetMode.None)]
    public static SerializedPropertyEqualityComparer Instance = new SerializedPropertyEqualityComparer();

    public bool Equals(SerializedProperty x, SerializedProperty y) {
      return SerializedProperty.DataEquals(x, y);
    }

    public int GetHashCode(SerializedProperty p) {
      bool enterChildren;
      bool isFirst  = true;
      int  hashCode = 0;
      int  minDepth = p.depth + 1;

      do {
        enterChildren = false;

        switch (p.propertyType) {
          case SerializedPropertyType.Integer:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.intValue);
            break;
          case SerializedPropertyType.Boolean:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.boolValue.GetHashCode());
            break;
          case SerializedPropertyType.Float:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.floatValue.GetHashCode());
            break;
          case SerializedPropertyType.String:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.stringValue.GetHashCode());
            break;
          case SerializedPropertyType.Color:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.colorValue.GetHashCode());
            break;
          case SerializedPropertyType.ObjectReference:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.objectReferenceInstanceIDValue);
            break;
          case SerializedPropertyType.LayerMask:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.intValue);
            break;
          case SerializedPropertyType.Enum:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.intValue);
            break;
          case SerializedPropertyType.Vector2:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.vector2Value.GetHashCode());
            break;
          case SerializedPropertyType.Vector3:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.vector3Value.GetHashCode());
            break;
          case SerializedPropertyType.Vector4:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.vector4Value.GetHashCode());
            break;
          case SerializedPropertyType.Vector2Int:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.vector2IntValue.GetHashCode());
            break;
          case SerializedPropertyType.Vector3Int:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.vector3IntValue.GetHashCode());
            break;
          case SerializedPropertyType.Rect:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.rectValue.GetHashCode());
            break;
          case SerializedPropertyType.RectInt:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.rectIntValue.GetHashCode());
            break;
          case SerializedPropertyType.ArraySize:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.intValue);
            break;
          case SerializedPropertyType.Character:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.intValue.GetHashCode());
            break;
          case SerializedPropertyType.AnimationCurve:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.animationCurveValue.GetHashCode());
            break;
          case SerializedPropertyType.Bounds:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.boundsValue.GetHashCode());
            break;
          case SerializedPropertyType.BoundsInt:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.boundsIntValue.GetHashCode());
            break;
          case SerializedPropertyType.ExposedReference:
            hashCode = HashCodeUtils.CombineHashCodes(hashCode, p.exposedReferenceValue.GetHashCode());
            break;
          default: {
            enterChildren = true;
            break;
          }
        }

        if (isFirst) {
          if (!enterChildren) {
            // no traverse needed
            return hashCode;
          }

          // since property is going to be traversed, a copy needs to be made
          p       = p.Copy();
          isFirst = false;
        }
      } while (p.Next(enterChildren) && p.depth >= minDepth);

      return hashCode;
    }
  }
}

#endif

#endregion

#endif
