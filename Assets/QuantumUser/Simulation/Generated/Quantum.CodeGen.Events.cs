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


namespace Quantum {
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
  
  public unsafe partial class Frame {
    public unsafe partial struct FrameEvents {
      static partial void GetEventTypeCountCodeGen(ref Int32 eventCount) {
        eventCount = 5;
      }
      static partial void GetParentEventIDCodeGen(Int32 eventID, ref Int32 parentEventID) {
        switch (eventID) {
          default: break;
        }
      }
      static partial void GetEventTypeCodeGen(Int32 eventID, ref System.Type result) {
        switch (eventID) {
          case EventPlayerHealthUpdated.ID: result = typeof(EventPlayerHealthUpdated); return;
          case EventOnPlayerEnteredGrass.ID: result = typeof(EventOnPlayerEnteredGrass); return;
          case EventOnPlayerExitGrass.ID: result = typeof(EventOnPlayerExitGrass); return;
          case EventCircleChangedState.ID: result = typeof(EventCircleChangedState); return;
          case EventBulletHit.ID: result = typeof(EventBulletHit); return;
          default: break;
        }
      }
      public EventPlayerHealthUpdated PlayerHealthUpdated(EntityRef Entity, FP CurrentHealth, FP MaxHealth) {
        var ev = _f.Context.AcquireEvent<EventPlayerHealthUpdated>(EventPlayerHealthUpdated.ID);
        ev.Entity = Entity;
        ev.CurrentHealth = CurrentHealth;
        ev.MaxHealth = MaxHealth;
        _f.AddEvent(ev);
        return ev;
      }
      public EventOnPlayerEnteredGrass OnPlayerEnteredGrass(EntityRef player) {
        var ev = _f.Context.AcquireEvent<EventOnPlayerEnteredGrass>(EventOnPlayerEnteredGrass.ID);
        ev.player = player;
        _f.AddEvent(ev);
        return ev;
      }
      public EventOnPlayerExitGrass OnPlayerExitGrass(EntityRef player) {
        var ev = _f.Context.AcquireEvent<EventOnPlayerExitGrass>(EventOnPlayerExitGrass.ID);
        ev.player = player;
        _f.AddEvent(ev);
        return ev;
      }
      public EventCircleChangedState CircleChangedState() {
        var ev = _f.Context.AcquireEvent<EventCircleChangedState>(EventCircleChangedState.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBulletHit BulletHit(EntityRef Bullet) {
        var ev = _f.Context.AcquireEvent<EventBulletHit>(EventBulletHit.ID);
        ev.Bullet = Bullet;
        _f.AddEvent(ev);
        return ev;
      }
    }
  }
  public unsafe partial class EventPlayerHealthUpdated : EventBase {
    public new const Int32 ID = 0;
    public EntityRef Entity;
    public FP CurrentHealth;
    public FP MaxHealth;
    protected EventPlayerHealthUpdated(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventPlayerHealthUpdated() : 
        base(0, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 37;
        hash = hash * 31 + Entity.GetHashCode();
        hash = hash * 31 + CurrentHealth.GetHashCode();
        hash = hash * 31 + MaxHealth.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventOnPlayerEnteredGrass : EventBase {
    public new const Int32 ID = 1;
    public EntityRef player;
    protected EventOnPlayerEnteredGrass(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventOnPlayerEnteredGrass() : 
        base(1, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 41;
        hash = hash * 31 + player.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventOnPlayerExitGrass : EventBase {
    public new const Int32 ID = 2;
    public EntityRef player;
    protected EventOnPlayerExitGrass(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventOnPlayerExitGrass() : 
        base(2, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 43;
        hash = hash * 31 + player.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventCircleChangedState : EventBase {
    public new const Int32 ID = 3;
    protected EventCircleChangedState(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventCircleChangedState() : 
        base(3, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 47;
        return hash;
      }
    }
  }
  public unsafe partial class EventBulletHit : EventBase {
    public new const Int32 ID = 4;
    public EntityRef Bullet;
    protected EventBulletHit(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBulletHit() : 
        base(4, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 53;
        hash = hash * 31 + Bullet.GetHashCode();
        return hash;
      }
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
