#if QUANTUM_ENABLE_MIGRATION
using Quantum;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Analyzer;
using Photon.Deterministic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Quantum {
  
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Blue)]
  public class QuantumEntityPrototype
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : global::EntityPrototype { }
#pragma warning restore CS0618
} // namespace Quantum
  [Obsolete("Use QuantumEntityPrototype instead")]
  [LastSupportedVersion("3.0")]
  public abstract class EntityPrototype
#endif
    : QuantumMonoBehaviour, IQuantumPrototypeConvertible<MapEntityId> {

  [Serializable]
  public struct Transform2DVerticalInfo {
    [HideInInspector]
    public bool IsEnabled;

    public FP Height;
    public FP PositionOffset;
  }

  [Serializable]
  public struct PhysicsColliderGeneric {
    [DrawIf(nameof(SourceCollider), 0)]
    public bool IsTrigger;

    public AssetRef<PhysicsMaterial> Material;

    public Component SourceCollider;

    [HideInInspector]
    public bool IsEnabled;

    [DisplayName("Type")]
    public Shape2DConfig Shape2D;

    [DisplayName("Type")]
    public Shape3DConfig Shape3D;

    [HideInInspector]
    public Shape2DConfig ScaledShape2D;

    [HideInInspector]
    public Shape3DConfig ScaledShape3D;

    public QuantumEntityPrototypeColliderLayerSource LayerSource;

    [Layer]
    [DrawIf(nameof(LayerSource), (Int32)QuantumEntityPrototypeColliderLayerSource.Explicit)]
    public int Layer;

    public QEnum32<CallbackFlags> CallbackFlags;
  }

  [Serializable]
  public struct PhysicsBodyGeneric {
    [HideInInspector]
    public bool IsEnabled;

    [DisplayName("Config")]
    public PhysicsBody2D.ConfigFlags Config2D;

    [DisplayName("Config")]
    public PhysicsBody3D.ConfigFlags Config3D;

    public RotationFreezeFlags RotationFreeze;

    public FP Mass;
    public FP Drag;
    public FP AngularDrag;

    [DisplayName("Center Of Mass")]
    public FPVector2 CenterOfMass2D;

    [DisplayName("Center Of Mass")]
    public FPVector3 CenterOfMass3D;

    public NullableFP GravityScale;
  }

  [Serializable]
  public struct NavMeshPathfinderInfo {
    [HideInInspector]
    public bool IsEnabled;

    public AssetRef<Quantum.NavMeshAgentConfig> NavMeshAgentConfig;

    [Optional("InitialTarget.IsEnabled")]
    public InitialNavMeshTargetInfo InitialTarget;
  }

  [Serializable]
  public struct InitialNavMeshTargetInfo {
    [HideInInspector]
    public bool IsEnabled;

    public Transform Target;

    [DrawIf("Target", 0)]
    public FPVector3 Position;

    public NavMeshSpec NavMesh;
  }

  [Serializable]
  public struct NavMeshSpec {
    public QuantumMapNavMeshUnity Reference;
    public AssetRef<Quantum.NavMesh> Asset;
    public string Name;
  }

  [Serializable]
  public struct NavMeshSteeringAgentInfo {
    [HideInInspector]
    public bool IsEnabled;

    // TODO: should have used NullableFP for that
    [Optional("MaxSpeed.IsEnabled")]
    public OverrideFP MaxSpeed;

    [Optional("Acceleration.IsEnabled")]
    public OverrideFP Acceleration;
  }

  [Serializable]
  public struct OverrideFP {
    [HideInInspector]
    public bool IsEnabled;

    public FP Value;
  }

  [Serializable]
  public struct NavMeshAvoidanceAgentInfo {
    [HideInInspector]
    public bool IsEnabled;
  }

  public QuantumEntityPrototypeTransformMode TransformMode;

  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.Transform2D, mode: DrawIfMode.Hide)]
  public Transform2DVerticalInfo Transform2DVertical;

  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.None, CompareOperator.NotEqual, DrawIfMode.Hide)]
  public PhysicsColliderGeneric PhysicsCollider;

  [DrawIf("PhysicsCollider.IsTrigger", 0, mode: DrawIfMode.Hide)]
  [DrawIf("PhysicsCollider.IsEnabled", 1, mode: DrawIfMode.Hide)]
  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.None, CompareOperator.NotEqual, DrawIfMode.Hide)]
  [Tooltip("To enable make sure PhysicsCollider is enabled and not a trigger")]
  public PhysicsBodyGeneric PhysicsBody = new PhysicsBodyGeneric() {
    Config2D = PhysicsBody2D.ConfigFlags.Default,
    Config3D = PhysicsBody3D.ConfigFlags.Default,
    Mass = 1,
    Drag = FP._0_50,
    AngularDrag = FP._0_50,
    CenterOfMass2D = FPVector2.Zero,
    CenterOfMass3D = FPVector3.Zero,
    GravityScale = new NullableFP() {
      _hasValue = 0,
      _value = FP._1
    },
  };

  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.None, CompareOperator.NotEqual, DrawIfMode.Hide)]
  public NavMeshPathfinderInfo NavMeshPathfinder;

  [DrawIf("NavMeshPathfinder.IsEnabled", 1, mode: DrawIfMode.Hide)]
  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.None, CompareOperator.NotEqual, DrawIfMode.Hide)]
  public NavMeshSteeringAgentInfo NavMeshSteeringAgent;

  [DrawIf("NavMeshPathfinder.IsEnabled", 1, mode: DrawIfMode.Hide)]
  [DrawIf("NavMeshSteeringAgent.IsEnabled", 1, mode: DrawIfMode.Hide)]
  [DrawIf("TransformMode", (int)QuantumEntityPrototypeTransformMode.None, CompareOperator.NotEqual, DrawIfMode.Hide)]
  public NavMeshAvoidanceAgentInfo NavMeshAvoidanceAgent;

  public AssetRef<Quantum.EntityView> View;

  public void PreSerialize() {
    if (TransformMode == QuantumEntityPrototypeTransformMode.Transform2D) {
      if (QPrototypePhysicsCollider2D.TrySetShapeConfigFromSourceCollider(PhysicsCollider.Shape2D, transform, PhysicsCollider.SourceCollider, out var isTrigger)) {
        PhysicsCollider.IsTrigger = isTrigger;

        if (PhysicsCollider.LayerSource != QuantumEntityPrototypeColliderLayerSource.Explicit) {
          PhysicsCollider.Layer = PhysicsCollider.SourceCollider.gameObject.layer;
        }
      } else if (PhysicsCollider.LayerSource == QuantumEntityPrototypeColliderLayerSource.GameObject) {
        PhysicsCollider.Layer = this.gameObject.layer;
      }

      ScaleShapeConfig2D(this.transform, PhysicsCollider.Shape2D, PhysicsCollider.ScaledShape2D);
    } else if (TransformMode == QuantumEntityPrototypeTransformMode.Transform3D) {
      if (QPrototypePhysicsCollider3D.TrySetShapeConfigFromSourceCollider(PhysicsCollider.Shape3D, transform, PhysicsCollider.SourceCollider, out var isTrigger)) {
        PhysicsCollider.IsTrigger = isTrigger;

        if (PhysicsCollider.LayerSource != QuantumEntityPrototypeColliderLayerSource.Explicit) {
          PhysicsCollider.Layer = PhysicsCollider.SourceCollider.gameObject.layer;
        }
      } else if (PhysicsCollider.LayerSource == QuantumEntityPrototypeColliderLayerSource.GameObject) {
        PhysicsCollider.Layer = this.gameObject.layer;
      }

      ScaleShapeConfig3D(this.transform, PhysicsCollider.Shape3D, PhysicsCollider.ScaledShape3D);
    }

    {
      if (PhysicsBody.IsEnabled) {
        if (TransformMode == QuantumEntityPrototypeTransformMode.Transform2D) {
          PhysicsBody.RotationFreeze = (PhysicsBody.Config2D & PhysicsBody2D.ConfigFlags.FreezeRotation) == PhysicsBody2D.ConfigFlags.FreezeRotation ? RotationFreezeFlags.FreezeAll : default;
        }
      }
    }

    if (NavMeshPathfinder.IsEnabled) {
      if (NavMeshPathfinder.InitialTarget.Target != null) {
        NavMeshPathfinder.InitialTarget.Position = NavMeshPathfinder.InitialTarget.Target.position.ToFPVector3();
      }

      if (NavMeshPathfinder.InitialTarget.NavMesh.Reference != null) {
        NavMeshPathfinder.InitialTarget.NavMesh.Asset = default;
        NavMeshPathfinder.InitialTarget.NavMesh.Name = NavMeshPathfinder.InitialTarget.NavMesh.Reference.name;
      }
    }
  }

  private static void ScaleShapeConfig2D(Transform t, Shape2DConfig from, Shape2DConfig scaledTo) {
    if (Shape2DConfig.Copy(from, scaledTo) == false) {
      return;
    }

    var scale = t.lossyScale.ToFPVector2();

    scaledTo.BoxExtents.X *= scale.X;
    scaledTo.BoxExtents.Y *= scale.Y;
    scaledTo.CircleRadius *= FPMath.Max(scale.X, scale.Y);
    scaledTo.EdgeExtent *= scale.X;

    scaledTo.CapsuleSize.X *= scale.X;
    scaledTo.CapsuleSize.Y *= scale.Y;

    scaledTo.PositionOffset.X *= scale.X;
    scaledTo.PositionOffset.Y *= scale.Y;

    if (scaledTo.CompoundShapes == null) {
      return;
    }

    Assert.Check(from.CompoundShapes != null);
    Assert.Check(from.CompoundShapes.Length == scaledTo.CompoundShapes.Length);

    for (int i = 0; i < scaledTo.CompoundShapes.Length; i++) {
      ref var scaledCompoundTo = ref scaledTo.CompoundShapes[i];

      scaledCompoundTo.BoxExtents.X *= scale.X;
      scaledCompoundTo.BoxExtents.Y *= scale.Y;
      scaledCompoundTo.CircleRadius *= FPMath.Max(scale.X, scale.Y);
      scaledCompoundTo.EdgeExtent *= scale.X;

      scaledCompoundTo.CapsuleSize.X *= scale.X;
      scaledCompoundTo.CapsuleSize.Y *= scale.Y;

      scaledCompoundTo.PositionOffset.X *= scale.X;
      scaledCompoundTo.PositionOffset.Y *= scale.Y;
    }
  }

  private static void ScaleShapeConfig3D(Transform t, Shape3DConfig from, Shape3DConfig scaledTo) {
    if (Shape3DConfig.Copy(from, scaledTo) == false) {
      return;
    }

    var scale = t.lossyScale.ToFPVector3();
    var sphereRadius = FPMath.Max(scale.X, scale.Y, scale.Z);

    scaledTo.BoxExtents.X *= scale.X;
    scaledTo.BoxExtents.Y *= scale.Y;
    scaledTo.BoxExtents.Z *= scale.Z;

    scaledTo.SphereRadius *= sphereRadius;
    scaledTo.CapsuleRadius *= FPMath.Max(scale.X, scale.Z);
    scaledTo.CapsuleHeight *= scale.Y;

    scaledTo.PositionOffset.X *= scale.X;
    scaledTo.PositionOffset.Y *= scale.Y;
    scaledTo.PositionOffset.Z *= scale.Z;

    if (scaledTo.CompoundShapes == null) {
      return;
    }

    Assert.Check(from.CompoundShapes != null);
    Assert.Check(from.CompoundShapes.Length == scaledTo.CompoundShapes.Length);

    for (int i = 0; i < scaledTo.CompoundShapes.Length; i++) {
      ref var scaledCompoundTo = ref scaledTo.CompoundShapes[i];

      scaledCompoundTo.BoxExtents.X *= scale.X;
      scaledCompoundTo.BoxExtents.Y *= scale.Y;
      scaledCompoundTo.BoxExtents.Z *= scale.Z;

      scaledCompoundTo.SphereRadius *= sphereRadius;
      scaledCompoundTo.CapsuleRadius *= sphereRadius;
      scaledCompoundTo.CapsuleHeight *= scale.Y;

      scaledCompoundTo.PositionOffset.X *= scale.X;
      scaledCompoundTo.PositionOffset.Y *= scale.Y;
      scaledCompoundTo.PositionOffset.Z *= scale.Z;
    }
  }

  public void SerializeImplicitComponents(List<ComponentPrototype> result, out QuantumEntityView selfView) {
    if (TransformMode == QuantumEntityPrototypeTransformMode.Transform2D) {
      result.Add(new Quantum.Prototypes.Transform2DPrototype() {
        Position = transform.position.ToFPVector2(),
        Rotation = transform.rotation.ToFPRotation2DDegrees(),
      });

      if (Transform2DVertical.IsEnabled) {
#if QUANTUM_XY
        var verticalScale = transform.lossyScale.z.ToFP();
#else
        var verticalScale = transform.lossyScale.y.ToFP();
#endif
        result.Add(new Quantum.Prototypes.Transform2DVerticalPrototype() {
          Position = transform.position.ToFPVerticalPosition() + (Transform2DVertical.PositionOffset * verticalScale),
          Height = Transform2DVertical.Height * verticalScale,
        });
      }

      if (PhysicsCollider.IsEnabled) {
        result.Add(new Quantum.Prototypes.PhysicsCollider2DPrototype() {
          IsTrigger = PhysicsCollider.IsTrigger,
          Layer = PhysicsCollider.Layer,
          PhysicsMaterial = PhysicsCollider.Material,
          ShapeConfig = PhysicsCollider.ScaledShape2D
        });

        result.Add(new Quantum.Prototypes.PhysicsCallbacks2DPrototype() {
          CallbackFlags = PhysicsCollider.CallbackFlags,
        });

        if (!PhysicsCollider.IsTrigger && PhysicsBody.IsEnabled) {
          result.Add(new Quantum.Prototypes.PhysicsBody2DPrototype() {
            Config = PhysicsBody.Config2D,
            AngularDrag = PhysicsBody.AngularDrag,
            Drag = PhysicsBody.Drag,
            Mass = PhysicsBody.Mass,
            CenterOfMass = PhysicsBody.CenterOfMass2D,
            GravityScale = PhysicsBody.GravityScale,
          });
        }
      }
    } else if (TransformMode == QuantumEntityPrototypeTransformMode.Transform3D) {
      result.Add(new Quantum.Prototypes.Transform3DPrototype() {
        Position = transform.position.ToFPVector3(),
        Rotation = transform.rotation.eulerAngles.ToFPVector3(),
      });

      if (PhysicsCollider.IsEnabled) {
        result.Add(new Quantum.Prototypes.PhysicsCollider3DPrototype() {
          IsTrigger = PhysicsCollider.IsTrigger,
          Layer = PhysicsCollider.Layer,
          PhysicsMaterial = PhysicsCollider.Material,
          ShapeConfig = PhysicsCollider.ScaledShape3D
        });

        result.Add(new Quantum.Prototypes.PhysicsCallbacks3DPrototype() {
          CallbackFlags = PhysicsCollider.CallbackFlags,
        });

        if (!PhysicsCollider.IsTrigger && PhysicsBody.IsEnabled) {
          result.Add(new Quantum.Prototypes.PhysicsBody3DPrototype() {
            Config = PhysicsBody.Config3D,
            AngularDrag = PhysicsBody.AngularDrag,
            Drag = PhysicsBody.Drag,
            Mass = PhysicsBody.Mass,
            RotationFreeze = PhysicsBody.RotationFreeze,
            CenterOfMass = PhysicsBody.CenterOfMass3D,
            GravityScale = PhysicsBody.GravityScale,
          });
        }
      }
    }

    if (NavMeshPathfinder.IsEnabled) {
      var pathfinder = new Quantum.Prototypes.NavMeshPathfinderPrototype() { AgentConfig = NavMeshPathfinder.NavMeshAgentConfig };

      if (NavMeshPathfinder.InitialTarget.IsEnabled) {
        pathfinder.InitialTarget = NavMeshPathfinder.InitialTarget.Position;
        pathfinder.InitialTargetNavMesh = NavMeshPathfinder.InitialTarget.NavMesh.Asset;
        pathfinder.InitialTargetNavMeshName = NavMeshPathfinder.InitialTarget.NavMesh.Name;
      }

      result.Add(pathfinder);

      if (NavMeshSteeringAgent.IsEnabled) {
        result.Add(new Quantum.Prototypes.NavMeshSteeringAgentPrototype() {
          OverrideMaxSpeed = NavMeshSteeringAgent.MaxSpeed.IsEnabled,
          OverrideAcceleration = NavMeshSteeringAgent.Acceleration.IsEnabled,
          MaxSpeed = NavMeshSteeringAgent.MaxSpeed.Value,
          Acceleration = NavMeshSteeringAgent.Acceleration.Value
        });

        if (NavMeshAvoidanceAgent.IsEnabled) {
          result.Add(new Quantum.Prototypes.NavMeshAvoidanceAgentPrototype());
        }
      }
    }

    selfView = GetComponent<QuantumEntityView>();

    if (selfView) {
      // self, don't emit view
    } else if (View.Id.IsValid) {
      result.Add(new Quantum.Prototypes.ViewPrototype() {
        Current = View,
      });
    }
  }

  [Conditional("UNITY_EDITOR")]
  public void CheckComponentDuplicates(Action<string> duplicateCallback) {
    CheckComponentDuplicates((type, sources) => {
      duplicateCallback($"Following components add {type.Name} prototype: {string.Join(", ", sources.Select(x => x.GetType()))}. The last one will be used.");
    });
  }

  [Conditional("UNITY_EDITOR")]
  public void CheckComponentDuplicates(Action<Type, List<Component>> duplicateCallback) {
    var typeToSource = new Dictionary<Type, List<Component>>();


    var implicitPrototypes = new List<ComponentPrototype>();
    SerializeImplicitComponents(implicitPrototypes, out var dummy);

    foreach (var prototype in implicitPrototypes) {
      if (!typeToSource.TryGetValue(prototype.GetType(), out var sources)) {
        sources = new List<Component>();
        typeToSource.Add(prototype.GetType(), sources);
      }

      sources.Add(this);
    }

    foreach (var component in GetComponents<QuantumUnityComponentPrototype>()) {
      if (!typeToSource.TryGetValue(component.PrototypeType, out var sources)) {
        sources = new List<Component>();
        typeToSource.Add(component.PrototypeType, sources);
      }

      sources.Add(component);
    }

    foreach (var kv in typeToSource) {
      if (kv.Value.Count > 1) {
        duplicateCallback(kv.Key, kv.Value);
      }
    }
  }

  [StaticField(StaticFieldResetMode.None)]
  private static readonly List<QuantumUnityComponentPrototype> behaviourBuffer = new List<QuantumUnityComponentPrototype>();

  [StaticField(StaticFieldResetMode.None)]
  private static readonly List<ComponentPrototype> prototypeBuffer = new List<ComponentPrototype>();

  public void InitializeAssetObject(Quantum.EntityPrototype assetObject, Quantum.EntityView selfViewAsset) {
    try {
      // get built-ins first
      PreSerialize();
      SerializeImplicitComponents(prototypeBuffer, out var selfView);

      if (selfView) {
        if (selfViewAsset != null) {
          prototypeBuffer.Add(new Quantum.Prototypes.ViewPrototype() { Current = new() { Id = selfViewAsset.Guid } });
        } else {
          Debug.LogError($"Self-view detected, but the no {nameof(Quantum.EntityView)} provided in {name}. Reimport the prefab.", this);
        }
      }

      var converter = new QuantumEntityPrototypeConverter((QuantumEntityPrototype)this);

      // now get custom ones
      GetComponents(behaviourBuffer);
      {
        foreach (var component in behaviourBuffer) {
          component.Refresh();
          prototypeBuffer.Add(component.CreatePrototype(converter));
        }
      }

      // store
      assetObject.Container = ComponentPrototypeSet.FromArray(prototypeBuffer.ToArray());

    } finally {
      behaviourBuffer.Clear();
      prototypeBuffer.Clear();
    }
  }
  
  MapEntityId IQuantumPrototypeConvertible<MapEntityId>.Convert(QuantumEntityPrototypeConverter converter) {
    var index = Array.IndexOf(converter.OrderedMapPrototypes, this);
    return index >= 0 ? MapEntityId.Create(index) : MapEntityId.Invalid;
  }

#if UNITY_EDITOR
  private void OnValidate() {
    try {
      PreSerialize();
    } catch (Exception ex) {
      Debug.LogError($"EntityPrototype validation error: {ex.Message}", this);
    }

    CheckComponentDuplicates(msg => Debug.LogWarning(msg, gameObject));
  }

  private void OnDrawGizmos() {
    DrawGizmos(false);
  }

  private void OnDrawGizmosSelected() {
    DrawGizmos(true);
  }

  void DrawGizmos(bool selected) {

    if (!QuantumGameGizmos.ShouldDraw(GlobalGizmosSettings.DrawColliderGizmos, selected)) {
      return;
    }

    FPMathUtils.LoadLookupTables();

    try {
      PreSerialize();
    } catch {
    }

    Shape2DConfig config2D = null;
    Shape3DConfig config3D = null;
    bool isDynamic2D = false;
    bool isDynamic3D = false;
    float height = 0.0f;
    Vector3 position2D = transform.position;
    FP rotation2DDeg = transform.rotation.ToFPRotation2DDegrees();
    Vector3 position3D = transform.position;
    Quaternion rotation3D = transform.rotation;


    CharacterController2DConfig configCC2D = null;
    CharacterController3DConfig configCC3D = null;

    if (PhysicsCollider.IsEnabled) {
      if (TransformMode == QuantumEntityPrototypeTransformMode.Transform2D) {
        config2D = PhysicsCollider.ScaledShape2D;
        isDynamic2D = PhysicsBody.IsEnabled && !PhysicsCollider.IsTrigger && (PhysicsBody.Config2D & PhysicsBody2D.ConfigFlags.IsKinematic) == default;
      } else if (TransformMode == QuantumEntityPrototypeTransformMode.Transform3D) {
        config3D = PhysicsCollider.ScaledShape3D;
        isDynamic3D = PhysicsBody.IsEnabled && !PhysicsCollider.IsTrigger && (PhysicsBody.Config3D & PhysicsBody3D.ConfigFlags.IsKinematic) == default;
      }
    }

    if (Transform2DVertical.IsEnabled) {
#if QUANTUM_XY
      var verticalScale = transform.lossyScale.z;
      height = -Transform2DVertical.Height.AsFloat * verticalScale;
      position2D.z -= Transform2DVertical.PositionOffset.AsFloat * verticalScale;
#else
      var verticalScale = transform.lossyScale.y;
      height = Transform2DVertical.Height.AsFloat * verticalScale;
      position2D.y += Transform2DVertical.PositionOffset.AsFloat * verticalScale;
#endif
    }

    // handle overriding from components
    {
      var vertical = SafeGetPrototype<Quantum.Prototypes.Transform2DVerticalPrototype>();
      if (vertical != null) {
#if QUANTUM_XY
        var verticalScale = transform.lossyScale.z;
        height = -vertical.Height.AsFloat * verticalScale;
        position2D.z = -vertical.Position.AsFloat * verticalScale;
#else
        var verticalScale = transform.lossyScale.y;
        height = vertical.Height.AsFloat * verticalScale;
        position2D.y = vertical.Position.AsFloat * verticalScale;
#endif
      }


      var transform2D = SafeGetPrototype<Quantum.Prototypes.Transform2DPrototype>();
      if (TransformMode == QuantumEntityPrototypeTransformMode.Transform2D || transform2D != null) {
        if (transform2D != null) {
          position2D = transform2D.Position.ToUnityVector3();
          rotation2DDeg = transform2D.Rotation;
        }

        config2D = SafeGetPrototype<Quantum.Prototypes.PhysicsCollider2DPrototype>()?.ShapeConfig ?? config2D;
        isDynamic2D |= GetComponent<QPrototypePhysicsBody2D>();

        var cc = SafeGetPrototype<Quantum.Prototypes.CharacterController2DPrototype>();
        if (cc != null) {
          QuantumUnityDB.TryGetGlobalAssetEditorInstance(cc.Config, out configCC2D);
        }
      }

      var transform3D = SafeGetPrototype<Quantum.Prototypes.Transform3DPrototype>();
      if (TransformMode == QuantumEntityPrototypeTransformMode.Transform3D || transform3D != null) {
        if (transform3D != null) {
          position3D = transform3D.Position.ToUnityVector3();
          rotation3D = Quaternion.Euler(transform3D.Rotation.ToUnityVector3());
        }

        config3D = SafeGetPrototype<Quantum.Prototypes.PhysicsCollider3DPrototype>()?.ShapeConfig ?? config3D;
        isDynamic3D |= GetComponent<QPrototypePhysicsBody3D>();

        var cc = SafeGetPrototype<Quantum.Prototypes.CharacterController3DPrototype>();
        if (cc != null) {
          QuantumUnityDB.TryGetGlobalAssetEditorInstance(cc.Config, out configCC3D);
        }
      }
    }

    var style = GlobalGizmosSettings.ColliderGizmosStyle;

    if (config2D != null) {
      var color = isDynamic2D ? GlobalGizmosSettings.DynamicColliderColor : GlobalGizmosSettings.KinematicColliderColor;
      if (config2D.ShapeType == Shape2DType.Polygon) {
        if (QuantumUnityDB.TryGetGlobalAsset(config2D.PolygonCollider, out Quantum.PolygonCollider collider)) {
          QuantumGameGizmos.DrawShape2DGizmo(Shape2D.CreatePolygon(collider, config2D.PositionOffset, FP.FromRaw((config2D.RotationOffset.RawValue * FP.Raw.Deg2Rad) >> FPLut.PRECISION)), position2D, rotation2DDeg.ToUnityQuaternionDegrees(),
            color, height, null, style: style);
        }
      } else if (config2D.ShapeType == Shape2DType.Compound) {
        foreach (var shape in config2D.CompoundShapes) {
          // nested compound shapes are not supported on the editor yet
          if (shape.ShapeType == Shape2DType.Compound) {
            continue;
          }

          if (shape.ShapeType == Shape2DType.Polygon) {
            if (QuantumUnityDB.TryGetGlobalAsset(shape.PolygonCollider, out Quantum.PolygonCollider collider)) {
              QuantumGameGizmos.DrawShape2DGizmo(Shape2D.CreatePolygon(collider, shape.PositionOffset, FP.FromRaw((shape.RotationOffset.RawValue * FP.Raw.Deg2Rad) >> FPLut.PRECISION)), position2D, rotation2DDeg.ToUnityQuaternionDegrees(),
                color, height, null, style: style);
            }
          } else {
            QuantumGameGizmos.DrawShape2DGizmo(shape.CreateShape(null), position2D, rotation2DDeg.ToUnityQuaternionDegrees(),
              color, height, null, style: style);
          }
        }
      } else {
        QuantumGameGizmos.DrawShape2DGizmo(config2D.CreateShape(null), position2D, rotation2DDeg.ToUnityQuaternionDegrees(),
          color, height, null, style: style);
      }
    }

    if (configCC2D != null) {
      QuantumGameGizmos.DrawCharacterController2DGizmo(position2D, configCC2D, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.CharacterControllerColor, selected), GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.AsleepColliderColor, selected), style: style);
    }

    if (config3D != null) {
      var color = isDynamic3D ? GlobalGizmosSettings.DynamicColliderColor : GlobalGizmosSettings.KinematicColliderColor;
      if (config3D.ShapeType == Shape3DType.Compound) {
        foreach (var shape in config3D.CompoundShapes) {
          // nested compound shapes are not supported on the editor yet
          if (shape.ShapeType == Shape3DType.Compound) {
            continue;
          }

          QuantumGameGizmos.DrawShape3DGizmo(shape.CreateShape(null), position3D, rotation3D, color, style: style);
        }
      } else {
        QuantumGameGizmos.DrawShape3DGizmo(config3D.CreateShape(null), position3D, rotation3D, color, style: style);
      }
    }

    if (configCC3D != null) {
      QuantumGameGizmos.DrawCharacterController3DGizmo(position3D, configCC3D, GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.CharacterControllerColor, selected), GlobalGizmosSettings.GetSelectedColor(GlobalGizmosSettings.AsleepColliderColor, selected), style: style);
    }
  }

  private T SafeGetPrototype<T>() where T : ComponentPrototype, new() {
    var component = GetComponent<QuantumUnityComponentPrototype<T>>();
    if (component == null) {
      return null;
    }

    return (T)component.CreatePrototype(null);
  }
#endif
  }

#if !QUANTUM_ENABLE_MIGRATION
} // 
#endif