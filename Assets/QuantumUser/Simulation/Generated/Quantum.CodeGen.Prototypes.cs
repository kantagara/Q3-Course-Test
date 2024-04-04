// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial
// declarations in another file.
// </auto-generated>
#pragma warning disable 0109
#pragma warning disable 1591


namespace Quantum.Prototypes {
  using Photon.Deterministic;
  using Quantum;
  using Quantum.Core;
  using Quantum.Collections;
  using Quantum.Inspector;
  using Quantum.Physics2D;
  using Quantum.Physics3D;
  using Byte = System.Byte;
  using SByte = System.SByte;
  using Int16 = System.Int16;
  using UInt16 = System.UInt16;
  using Int32 = System.Int32;
  using UInt32 = System.UInt32;
  using Int64 = System.Int64;
  using UInt64 = System.UInt64;
  using Boolean = System.Boolean;
  using String = System.String;
  using Object = System.Object;
  using FlagsAttribute = System.FlagsAttribute;
  using SerializableAttribute = System.SerializableAttribute;
  using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
  using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
  using FieldOffsetAttribute = System.Runtime.InteropServices.FieldOffsetAttribute;
  using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;
  using LayoutKind = System.Runtime.InteropServices.LayoutKind;
  #if QUANTUM_UNITY //;
  using TooltipAttribute = UnityEngine.TooltipAttribute;
  using HeaderAttribute = UnityEngine.HeaderAttribute;
  using SpaceAttribute = UnityEngine.SpaceAttribute;
  using RangeAttribute = UnityEngine.RangeAttribute;
  using HideInInspectorAttribute = UnityEngine.HideInInspector;
  using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;
  using FormerlySerializedAsAttribute = UnityEngine.Serialization.FormerlySerializedAsAttribute;
  using MovedFromAttribute = UnityEngine.Scripting.APIUpdating.MovedFromAttribute;
  using CreateAssetMenu = UnityEngine.CreateAssetMenuAttribute;
  using RuntimeInitializeOnLoadMethodAttribute = UnityEngine.RuntimeInitializeOnLoadMethodAttribute;
  #endif //;
  
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Bullet))]
  public unsafe class BulletPrototype : ComponentPrototype<Quantum.Bullet> {
    public MapEntityId Owner;
    public FP Damage;
    public FP Time;
    public FP Speed;
    public FPVector2 Direction;
    public FP HeightOffset;
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.Bullet component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.Bullet result, in PrototypeMaterializationContext context = default) {
        PrototypeValidator.FindMapEntity(this.Owner, in context, out result.Owner);
        result.Damage = this.Damage;
        result.Time = this.Time;
        result.Speed = this.Speed;
        result.Direction = this.Direction;
        result.HeightOffset = this.HeightOffset;
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Damageable))]
  public unsafe partial class DamageablePrototype : ComponentPrototype<Quantum.Damageable> {
    public FP CurrentHealth;
    public FP MaxHealth;
    public AssetRef<DamageableBase> DamageableBase;
    partial void MaterializeUser(Frame frame, ref Quantum.Damageable result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.Damageable component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.Damageable result, in PrototypeMaterializationContext context = default) {
        result.CurrentHealth = this.CurrentHealth;
        result.MaxHealth = this.MaxHealth;
        result.DamageableBase = this.DamageableBase;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Grass))]
  public unsafe partial class GrassPrototype : ComponentPrototype<Quantum.Grass> {
    [HideInInspector()]
    public Int32 _empty_prototype_dummy_field_;
    partial void MaterializeUser(Frame frame, ref Quantum.Grass result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.Grass component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.Grass result, in PrototypeMaterializationContext context = default) {
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Input))]
  public unsafe partial class InputPrototype : StructPrototype {
    public FPVector2 Movement;
    public FPVector2 MousePosition;
    public Button Fire;
    partial void MaterializeUser(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context = default) {
        result.Movement = this.Movement;
        result.MousePosition = this.MousePosition;
        result.Fire = this.Fire;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.KCC))]
  public unsafe partial class KCCPrototype : ComponentPrototype<Quantum.KCC> {
    public AssetRef<KCCSettings> Settings;
    public FP MaxSpeed;
    public FP Acceleration;
    public FPVector2 Velocity;
    partial void MaterializeUser(Frame frame, ref Quantum.KCC result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.KCC component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.KCC result, in PrototypeMaterializationContext context = default) {
        result.Settings = this.Settings;
        result.MaxSpeed = this.MaxSpeed;
        result.Acceleration = this.Acceleration;
        result.Velocity = this.Velocity;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.PlayerLink))]
  public unsafe partial class PlayerLinkPrototype : ComponentPrototype<Quantum.PlayerLink> {
    public PlayerRef Player;
    partial void MaterializeUser(Frame frame, ref Quantum.PlayerLink result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.PlayerLink component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.PlayerLink result, in PrototypeMaterializationContext context = default) {
        result.Player = this.Player;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.SpawnPoint))]
  public unsafe partial class SpawnPointPrototype : ComponentPrototype<Quantum.SpawnPoint> {
    [HideInInspector()]
    public Int32 _empty_prototype_dummy_field_;
    partial void MaterializeUser(Frame frame, ref Quantum.SpawnPoint result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.SpawnPoint component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.SpawnPoint result, in PrototypeMaterializationContext context = default) {
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.SpawnPointList))]
  public unsafe class SpawnPointListPrototype : ComponentPrototype<Quantum.SpawnPointList> {
    [DynamicCollectionAttribute()]
    public MapEntityId[] availableSpawnPoints = {};
    [DynamicCollectionAttribute()]
    public MapEntityId[] usedSpawnPoints = {};
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.SpawnPointList component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.SpawnPointList result, in PrototypeMaterializationContext context = default) {
        if (this.availableSpawnPoints.Length == 0) {
          result.availableSpawnPoints = default;
        } else {
          var list = frame.AllocateList(out result.availableSpawnPoints, this.availableSpawnPoints.Length);
          for (int i = 0; i < this.availableSpawnPoints.Length; ++i) {
            EntityRef tmp = default;
            PrototypeValidator.FindMapEntity(this.availableSpawnPoints[i], in context, out tmp);
            list.Add(tmp);
          }
        }
        if (this.usedSpawnPoints.Length == 0) {
          result.usedSpawnPoints = default;
        } else {
          var list = frame.AllocateList(out result.usedSpawnPoints, this.usedSpawnPoints.Length);
          for (int i = 0; i < this.usedSpawnPoints.Length; ++i) {
            EntityRef tmp = default;
            PrototypeValidator.FindMapEntity(this.usedSpawnPoints[i], in context, out tmp);
            list.Add(tmp);
          }
        }
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Weapon))]
  public unsafe partial class WeaponPrototype : ComponentPrototype<Quantum.Weapon> {
    public FP CooldownTime;
    public AssetRef<WeaponData> WeaponData;
    partial void MaterializeUser(Frame frame, ref Quantum.Weapon result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.Weapon component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.Weapon result, in PrototypeMaterializationContext context = default) {
        result.CooldownTime = this.CooldownTime;
        result.WeaponData = this.WeaponData;
        MaterializeUser(frame, ref result, in context);
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
