#if QUANTUM_ENABLE_MIGRATION
using Quantum;
#endif
#pragma warning disable IDE0065 // Misplaced using directive
using System;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
#pragma warning restore IDE0065 // Misplaced using directive

namespace Quantum {
  /// <summary>
  /// The Quantum entity view component is the representation of the entity inside Unity.
  /// Instances will be created by the <see cref="EntityViewUpdater"/>.
  /// Quantum entity with the <see cref="View"/> component will references a Quantum <see cref="EntityView"/> asset which will in turn be instantiated as 
  /// <see cref="EntityView.Prefab"/> and the resulting game object includes this script.
  /// </summary>
  [DisallowMultipleComponent]
  [ScriptHelp(BackColor = ScriptHeaderBackColor.Orange)]
  public unsafe class QuantumEntityView
#if QUANTUM_ENABLE_MIGRATION
#pragma warning disable CS0618
    : global::EntityView { }
#pragma warning restore CS0618
} // namespace Quantum
  [Obsolete("Use QuantumEntityView instead")]
  [LastSupportedVersion("3.0")]
  public unsafe abstract class EntityView
#endif
    : QuantumMonoBehaviour {

    /// <summary>
    /// Wrapping UnityEvent(QuantumGame) into a class. Is used by the QuantumEntityView to make create and destroy publish Unity events.
    /// </summary>
    [Serializable]
    public class EntityUnityEvent : UnityEvent<QuantumGame> { }

    /// <summary>
    /// Will set to the <see cref="AssetObject.Guid"/> that the underlying <see cref="EntityView"/> asset has.
    /// Or receives a new Guid when binding this view script to a scene entity.
    /// </summary>
    [NonSerialized]
    public AssetGuid AssetGuid;

    /// <summary>
    /// References the Quantum entity that this view script is liked to.
    /// </summary>
    [NonSerialized]
    public EntityRef EntityRef;

    /// <summary>
    /// Set the entity view bind behaviour. If set to <see cref="QuantumEntityViewBindBehaviour.NonVerified"/> then the view is created during a predicted frame.
    /// Entity views created at that time can be subject to changes and even be destroyed because of misprediction.
    /// Entity views created during <see cref="QuantumEntityViewBindBehaviour.Verified"/> will be more stable but are always created at a later time, when the input has been confirmed by the server.
    /// </summary>
    [FormerlySerializedAs("CreateBehaviour")]
    [InlineHelp]
    public QuantumEntityViewBindBehaviour BindBehaviour;

    /// <summary>
    /// If enabled the QuantumEntityViewUpdater will not destroy (or disable, in case of map entities) this instance, and you are responsible for removing it from the game world yourself.\n\nYou will still receive the OnEntityDestroyed callback.
    /// </summary>
    [FormerlySerializedAs("ManualDestroy")]
    [FormerlySerializedAs("ManualDiposal")]
    [InlineHelp]
    public bool ManualDisposal;
    [Obsolete("Use ManualDisposal")]
    public bool ManualDiposal => ManualDisposal;

    /// <summary>
    /// Set the <see cref="QuantumEntityViewFlags"/> to further configure the entity view.
    /// </summary>
    [InlineHelp]
    public QuantumEntityViewFlags ViewFlags;

    /// <summary>
    /// If set to true, the game object will be renamed to the EntityRef number.
    /// </summary>
    [Obsolete("Use QuantumEntityViewFlags.DisableEntityRefNaming")]
    public bool GameObjectNameIsEntityRef {
      get { 
        return !HasViewFlag(QuantumEntityViewFlags.DisableEntityRefNaming);
      }
      set {
        SetViewFlag(QuantumEntityViewFlags.DisableEntityRefNaming, !value);
      }
    }

    /// <summary>
    ///   <para>
    ///     A factor with dimension of 1/s (Hz) that works as a lower limit for how much
    ///     of the accumulated prediction error is corrected every frame.
    ///     This factor affects both the position and the rotation correction.
    ///     Suggested values are greater than zero and smaller than
    ///     <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///   </para>
    ///   <para>
    ///     E.g.: ErrorCorrectionRateMin = 3, rendering delta time = (1/60)s: at least 5% (3 * 1/60) of the accumulated error
    ///     will be corrected on this rendered frame.
    ///   </para>
    ///   <para>
    ///     This threshold might not be respected if the resultant correction magnitude is
    ///     below the <see cref="ErrorPositionMinCorrection">ErrorPositionMinCorrection</see>
    ///     or above the <see cref="ErrorPositionTeleportDistance">ErrorPositionTeleportDistance</see>, for the position error,
    ///     or above the <see cref="ErrorRotationTeleportDistance">ErrorRotationTeleportDistance</see>, for the rotation error.
    ///   </para>
    /// </summary>
    [Header("Prediction Error Correction")]
    [InlineHelp]
    public Single ErrorCorrectionRateMin = 3.3f;

    /// <summary>
    ///   <para>
    ///     A factor with dimension of 1/s (Hz) that works as a upper limit for how much
    ///     of the accumulated prediction error is corrected every frame.
    ///     This factor affects both the position and the rotation correction.
    ///     Suggested values are greater than <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>
    ///     and smaller than half of a target rendering rate.
    ///   </para>
    ///   <para>
    ///     E.g.: ErrorCorrectionRateMax = 15, rendering delta time = (1/60)s: at maximum 25% (15 * 1/60) of the accumulated
    ///     error
    ///     will be corrected on this rendered frame.
    ///   </para>
    ///   <para>
    ///     This threshold might not be respected if the resultant correction magnitude is
    ///     below the <see cref="ErrorPositionMinCorrection">ErrorPositionMinCorrection</see> or
    ///     above the <see cref="ErrorPositionTeleportDistance">ErrorPositionTeleportDistance</see>, for the position error,
    ///     or above the <see cref="ErrorRotationTeleportDistance">ErrorRotationTeleportDistance</see>, for the rotation error.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorCorrectionRateMax = 10f;

    /// <summary>
    ///   <para>
    ///     The reference for the magnitude of the accumulated position error, in meters,
    ///     at which the position error will be corrected at the
    ///     <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///     Suggested values are greater than <see cref="ErrorPositionMinCorrection">ErrorPositionMinCorrection</see>
    ///     and smaller than <see cref="ErrorPositionBlendEnd">ErrorPositionBlendEnd</see>.
    ///   </para>
    ///   <para>
    ///     In other words, if the magnitude of the accumulated error is equal to or smaller than this threshold,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///     If, instead, the magnitude is between this threshold and
    ///     <see cref="ErrorPositionBlendEnd">ErrorPositionBlendEnd</see>,
    ///     the error is corrected at a rate between <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>
    ///     and <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>, proportionally.
    ///     If it is equal to or greater than <see cref="ErrorPositionBlendEnd">ErrorPositionBlendEnd</see>,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///   </para>
    ///   <para>
    ///     Note: as the factor is expressed in distance units (meters), it might need to be scaled
    ///     proportionally to the overall scale of objects in the scene and speeds at which they move,
    ///     which are factors that affect the expected magnitude of prediction errors.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorPositionBlendStart = 0.25f;

    /// <summary>
    ///   <para>
    ///     The reference for the magnitude of the accumulated position error, in meters,
    ///     at which the position error will be corrected at the
    ///     <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///     Suggested values are greater than <see cref="ErrorPositionBlendStart">ErrorPositionBlendStart</see>
    ///     and smaller than <see cref="ErrorPositionTeleportDistance">ErrorPositionTeleportDistance</see>.
    ///   </para>
    ///   <para>
    ///     In other words, if the magnitude of the accumulated error is equal to or greater than this threshold,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///     If, instead, the magnitude is between <see cref="ErrorPositionBlendStart">ErrorPositionBlendStart</see> and this
    ///     threshold,
    ///     the error is corrected at a rate between <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>
    ///     and <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>, proportionally.
    ///     If it is equal to or smaller than <see cref="ErrorPositionBlendStart">ErrorPositionBlendStart</see>,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///   </para>
    ///   <para>
    ///     Note: as the factor is expressed in distance units (meters), it might need to be scaled
    ///     proportionally to the overall scale of objects in the scene and speeds at which they move,
    ///     which are factors that affect the expected magnitude of prediction errors.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorPositionBlendEnd = 1f;

    /// <summary>
    ///   <para>
    ///     The reference for the magnitude of the accumulated rotation error, in radians,
    ///     at which the rotation error will be corrected at the
    ///     <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///     Suggested values are smaller than <see cref="ErrorRotationBlendEnd">ErrorRotationBlendEnd</see>.
    ///   </para>
    ///   <para>
    ///     In other words, if the magnitude of the accumulated error is equal to or smaller than this threshold,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///     If, instead, the magnitude is between this threshold and
    ///     <see cref="ErrorRotationBlendEnd">ErrorRotationBlendEnd</see>,
    ///     the error is corrected at a rate between <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>
    ///     and <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>, proportionally.
    ///     If it is equal to or greater than <see cref="ErrorRotationBlendEnd">ErrorRotationBlendEnd</see>,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorRotationBlendStart = 0.1f;

    /// <summary>
    ///   <para>
    ///     The reference for the magnitude of the accumulated rotation error, in radians,
    ///     at which the rotation error will be corrected at the
    ///     <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///     Suggested values are greater than <see cref="ErrorRotationBlendStart">ErrorRotationBlendStart</see>
    ///     and smaller than <see cref="ErrorRotationTeleportDistance">ErrorRotationTeleportDistance</see>.
    ///   </para>
    ///   <para>
    ///     In other words, if the magnitude of the accumulated error is equal to or greater than this threshold,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>.
    ///     If, instead, the magnitude is between <see cref="ErrorRotationBlendStart">ErrorRotationBlendStart</see> and this
    ///     threshold,
    ///     the error is corrected at a rate between <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>
    ///     and <see cref="ErrorCorrectionRateMax">ErrorCorrectionRateMax</see>, proportionally.
    ///     If it is equal to or smaller than <see cref="ErrorRotationBlendStart">ErrorRotationBlendStart</see>,
    ///     it will be corrected at the <see cref="ErrorCorrectionRateMin">ErrorCorrectionRateMin</see>.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorRotationBlendEnd = 0.5f;

    /// <summary>
    ///   <para>
    ///     The value, in meters, that represents the minimum magnitude of the accumulated position error
    ///     that will be corrected in a single frame, until it is fully corrected.
    ///   </para>
    ///   <para>
    ///     This setting has priority over the resultant correction rate, i.e. the restriction
    ///     will be respected even if it makes the effective correction rate be different than
    ///     the one computed according to the min/max rates and start/end blend values.
    ///     Suggested values are greater than zero and smaller than
    ///     <see cref="ErrorPositionBlendStart">ErrorPositionBlendStart</see>.
    ///   </para>
    ///   <para>
    ///     Note: as the factor is expressed in distance units (meters), it might need to be scaled
    ///     proportionally to the overall scale of objects in the scene and speeds at which they move,
    ///     which are factors that affect the expected magnitude of prediction errors.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorPositionMinCorrection = 0.025f;

    /// <summary>
    ///   <para>
    ///     The value, in meters, that represents the magnitude of the accumulated
    ///     position error above which the error will be instantaneously corrected,
    ///     effectively teleporting the rendered object to its correct position.
    ///     Suggested values are greater than <see cref="ErrorPositionBlendEnd">ErrorPositionBlendEnd</see>.
    ///   </para>
    ///   <para>
    ///     This setting has priority over the resultant correction rate, i.e. the restriction
    ///     will be respected even if it makes the effective correction rate be different than
    ///     the one computed according to the min/max rates and start/end blend values.
    ///   </para>
    ///   <para>
    ///     Note: as the factor is expressed in distance units (meters), it might need to be scaled
    ///     proportionally to the overall scale of objects in the scene and speeds at which they move,
    ///     which are factors that affect the expected magnitude of prediction errors.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorPositionTeleportDistance = 2f;

    /// <summary>
    ///   <para>
    ///     The value, in radians, that represents the magnitude of the accumulated
    ///     rotation error above which the error will be instantaneously corrected,
    ///     effectively teleporting the rendered object to its correct orientation.
    ///     Suggested values are greater than <see cref="ErrorRotationBlendEnd">ErrorRotationBlendEnd</see>.
    ///   </para>
    ///   <para>
    ///     This setting has priority over the resultant correction rate, i.e. the restriction
    ///     will be respected even if it makes the effective correction rate be different than
    ///     the one computed according to the min/max rates and start/end blend values.
    ///   </para>
    /// </summary>
    [InlineHelp]
    public Single ErrorRotationTeleportDistance = 0.5f;

    /// <summary>
    /// Is called after the entity view has been instantiated.
    /// </summary>
    [Header("Events")]
    public EntityUnityEvent OnEntityInstantiated;
    /// <summary>
    /// Is called before the entity view is destroyed.
    /// </summary>
    public EntityUnityEvent OnEntityDestroyed;

    /// <summary>
    /// Access the entity view components registered to this entity view.
    /// All view components found on this game object during creation are used.
    /// </summary>
    public IQuantumViewComponent[] ViewComponents => _viewComponents;
    /// <summary>
    /// A reference to the entity view updater that controls this entity view.
    /// </summary>
    public QuantumEntityViewUpdater EntityViewUpdater { get; private set; }
    /// <summary>
    /// A reference to the current game that this entity view belongs to <see cref="QuantumEntityViewUpdater.ObservedGame"/>.
    /// </summary>
    public QuantumGame Game { get; private set; }
    /// <summary>
    /// All contexts found on the <see cref="EntityViewUpdater"/> game object accessible by their type.
    /// </summary>
    public Dictionary<Type, IQuantumViewContext> ViewContexts { get; private set; }

    /// <summary>
    /// Set the <see cref="QuantumEntityViewFlags"/> to further configure the entity view.
    /// </summary>
    /// <param name="flag">The flag enum value</param>
    /// <param name="isEnabled">Set or unset the flag.</param>
    public void SetViewFlag(QuantumEntityViewFlags flag, bool isEnabled) {
      if (isEnabled) {
        ViewFlags |= flag;
      }
      else {
        ViewFlags &= ~flag;
      }
    }
    /// <summary>
    /// Test if a view flag is set.
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public bool HasViewFlag(QuantumEntityViewFlags flag) => (ViewFlags & flag) == flag;

    public Transform Transform {
      get {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
          return base.transform;
#endif
        if (_transformCached == false) {
          _cachedTransform = base.transform;
          _transformCached = true;
        }

        return _cachedTransform;
      }
    }


    FP _lastPredictedVerticalPosition2D;
    FPVector2 _lastPredictedPosition2D;
    FPVector3 _lastPredictedPosition3D;
    FP _lastPredictedRotation2D;
    FPQuaternion _lastPredictedRotation3D;
    Vector3 _errorVisualVector;
    Quaternion _errorVisualQuaternion;
    IQuantumViewComponent[] _viewComponents;
    Transform _cachedTransform;
    bool _transformCached;

    public struct UpdatePostionParameter {
      public Vector3 NewPosition;
      public Quaternion NewRotation;
      public Vector3 UninterpolatedPosition;
      public Quaternion UninterpolatedRotation;
      public Vector3 ErrorVisualVector;
      public Quaternion ErrorVisualQuaternion;
      public bool PositionTeleport;
      public bool RotationTeleport;
    }

    public virtual void OnInitialize() { }
    public virtual void OnActivate(Frame frame) { }
    public virtual void OnDeactivate() { }
    public virtual void OnUpdateView() { }
    public virtual void OnLateUpdateView() { }
    public virtual void OnGameChanged() { }

    void Initialize(Dictionary<Type, IQuantumViewContext> contexts, QuantumEntityViewUpdater entityViewUpdater) {
      if ((ViewFlags & QuantumEntityViewFlags.DisableSearchChildrenForEntityViewComponents) > 0) {
        _viewComponents = GetComponents<IQuantumViewComponent>();
      }
      else {
        _viewComponents = GetComponentsInChildren<IQuantumViewComponent>();
      }
      EntityViewUpdater = entityViewUpdater;

      OnInitialize();

      for (int i = 0; i < _viewComponents.Length; i++) {
        _viewComponents[i].Initialize(contexts);
      }
    }

    internal void Activate(QuantumGame game, Frame frame, Dictionary<Type, IQuantumViewContext> contexts, QuantumEntityViewUpdater entityViewUpdater) {
      // TODO: When the observed game is switched, this needs to be propagated here.
      _lastPredictedPosition2D = default(FPVector2);
      _lastPredictedRotation2D = default(FP);
      _lastPredictedPosition3D = default(FPVector3);
      _lastPredictedRotation3D = default(FPQuaternion);
      _errorVisualVector = default(Vector3);
      _errorVisualQuaternion = Quaternion.identity;

      ViewContexts = contexts;

      if (_viewComponents == null) {
        Initialize(contexts, entityViewUpdater);
      }

      Game = game;

      OnActivate(frame);

      for (int i = 0; i < _viewComponents.Length; i++) {
#if QUANTUM_ENABLE_MIGRATION
      _viewComponents[i].Activate(frame, Game, (QuantumEntityView)this);
#else
        _viewComponents[i].Activate(frame, Game, this);
#endif
    }
  }

    internal void GameChanged(QuantumGame game) {
      Game = game;

      OnGameChanged();

      for (int i = 0; i < _viewComponents.Length; i++) {
        _viewComponents[i].GameChanged(game);
      }
    }

    internal void Deactivate() {
      if (_viewComponents == null) {
        // GameObjects already destroyed
        return;
      }

      for (int i = 0; i < _viewComponents.Length; i++) {
        _viewComponents[i].Deactivate();
      }

      OnDeactivate();
    }

    internal void UpdateView(bool useClockAliasingInterpolation, bool useErrorCorrection) {
      if ((ViewFlags & QuantumEntityViewFlags.DisableUpdatePosition) == 0) {
        if (Game.Frames.Predicted.Has<Transform2D>(EntityRef)) {
          // update 2d transform
          UpdateFromTransform2D(Game, useClockAliasingInterpolation, useErrorCorrection);
        } else {
          // update 3d transform
          if (Game.Frames.Predicted.Has<Transform3D>(EntityRef)) {
            UpdateFromTransform3D(Game, useClockAliasingInterpolation, useErrorCorrection);
          }
        }
      }

      if ((ViewFlags & QuantumEntityViewFlags.DisableUpdateView) == 0) {
        OnUpdateView();

        for (int i = 0; i < _viewComponents.Length; i++) {
          if (_viewComponents[i].IsActive) {
            _viewComponents[i].UpdateView();
          }
        }
      }
    }

    internal void LateUpdateView() {
      if ((ViewFlags & QuantumEntityViewFlags.DisableUpdateView) > 0) {
        return;
      }
      
      OnLateUpdateView();

      for (int i = 0; i < _viewComponents.Length; i++) {
        if (_viewComponents[i].IsActive) {
          _viewComponents[i].LateUpdateView();
        }
      }
    }

    public void UpdateFromTransform3D(QuantumGame game, Boolean useClockAliasingInterpolation, Boolean useErrorCorrectionInterpolation) {
      if (game == null || !game.Frames.Predicted.Has<Transform3D>(EntityRef))
        return;

      var transform = game.Frames.Predicted.Unsafe.GetPointer<Transform3D>(EntityRef);

      var param = new UpdatePostionParameter() {
        NewPosition = transform->Position.ToUnityVector3(),
        NewRotation = transform->Rotation.ToUnityQuaternion(),
      };

      param.UninterpolatedPosition = param.NewPosition;
      param.UninterpolatedRotation = param.NewRotation;

      if (game.Frames.PredictedPrevious.Exists(EntityRef)) {
        if (game.Frames.PredictedPrevious.Unsafe.TryGetPointer(EntityRef, out Transform3D* transformPrevious)) {
          if (useClockAliasingInterpolation) {
            param.NewPosition = Vector3.Lerp(transformPrevious->Position.ToUnityVector3(), param.NewPosition, game.InterpolationFactor);
            param.NewRotation = Quaternion.Slerp(transformPrevious->Rotation.ToUnityQuaternion(), param.NewRotation, game.InterpolationFactor);
          }

          if (useErrorCorrectionInterpolation && game.Frames.PreviousUpdatePredicted.Exists(EntityRef)) {
            if (game.Frames.PreviousUpdatePredicted.Unsafe.TryGetPointer(EntityRef, out Transform3D* oldTransform)) {
              var errorPosition = _lastPredictedPosition3D - oldTransform->Position;
              var errorRotation = Quaternion.Inverse(oldTransform->Rotation.ToUnityQuaternion()) * _lastPredictedRotation3D.ToUnityQuaternion();
              _errorVisualVector += errorPosition.ToUnityVector3();
              _errorVisualQuaternion = errorRotation * _errorVisualQuaternion;
            }
          }
        }
      }

      // update rendered position
      UpdateRenderPosition(ref param);

      // store current prediction information
      _lastPredictedPosition3D = transform->Position;
      _lastPredictedRotation3D = transform->Rotation;
    }

    public void UpdateFromTransform2D(QuantumGame game, Boolean useClockAliasingInterpolation, Boolean useErrorCorrectionInterpolation) {
      if (game == null || !game.Frames.Predicted.Has<Transform2D>(EntityRef))
        return;

      var transform = game.Frames.Predicted.Unsafe.GetPointer<Transform2D>(EntityRef);

      var param = new UpdatePostionParameter() {
        NewPosition = transform->Position.ToUnityVector3(),
        NewRotation = transform->Rotation.ToUnityQuaternion(),
      };

      var hasVertical = game.Frames.Predicted.Unsafe.TryGetPointer(EntityRef, out Transform2DVertical* tVertical);
      if (hasVertical) {
#if QUANTUM_XY
      param.NewPosition.z = -tVertical->Position.AsFloat;
#else
        param.NewPosition.y = tVertical->Position.AsFloat;
#endif
      }

      param.UninterpolatedPosition = param.NewPosition;
      param.UninterpolatedRotation = param.NewRotation;

      if (game.Frames.PredictedPrevious.Exists(EntityRef)) {
        if (game.Frames.PredictedPrevious.Unsafe.TryGetPointer(EntityRef, out Transform2D* transformPrevious)) {
          if (useClockAliasingInterpolation) {
            var previousPos = transformPrevious->Position.ToUnityVector3();
            if (game.Frames.PredictedPrevious.Unsafe.TryGetPointer(EntityRef, out Transform2DVertical* tVerticalPrevious)) {
#if QUANTUM_XY
            previousPos.z = -tVerticalPrevious->Position.AsFloat;
#else
              previousPos.y = tVerticalPrevious->Position.AsFloat;
#endif
            }

            param.NewPosition = Vector3.Lerp(previousPos, param.NewPosition, game.InterpolationFactor);
            param.NewRotation = Quaternion.Slerp(transformPrevious->Rotation.ToUnityQuaternion(), param.NewRotation, game.InterpolationFactor);
          }

          if (useErrorCorrectionInterpolation && game.Frames.PreviousUpdatePredicted.Exists(EntityRef)) {
            if (game.Frames.PreviousUpdatePredicted.Unsafe.TryGetPointer(EntityRef, out Transform2D* oldTransform)) {
              // position error
              var errorPosition = _lastPredictedPosition2D - oldTransform->Position;
              var errorVertical = _lastPredictedVerticalPosition2D;
              if (game.Frames.PreviousUpdatePredicted.Unsafe.TryGetPointer(EntityRef, out Transform2DVertical* oldTransformVertical)) {
                errorVertical -= oldTransformVertical->Position;
              }

              var errorVector = errorPosition.ToUnityVector3();
#if QUANTUM_XY
            errorVector.z = -errorVertical.AsFloat;
#else
              errorVector.y = errorVertical.AsFloat;
#endif

              _errorVisualVector += errorVector;

              // rotation error
              var errorRotation = _lastPredictedRotation2D - oldTransform->Rotation;
              _errorVisualQuaternion = errorRotation.ToUnityQuaternion() * _errorVisualQuaternion;
            }
          }
        }
      }

      // update rendered position
      UpdateRenderPosition(ref param);

      // store current prediction information
      _lastPredictedPosition2D = transform->Position;
      _lastPredictedVerticalPosition2D = hasVertical ? tVertical->Position : default;
      _lastPredictedRotation2D = transform->Rotation;
    }

    void UpdateRenderPosition(ref UpdatePostionParameter param) {
      var positionCorrectionRate = ErrorCorrectionRateMin;
      var rotationCorrectionRate = ErrorCorrectionRateMin;

      var positionTeleport = false;
      var rotationTeleport = false;

      // if we're going over teleport distance, we should just teleport
      var positionErrorMagnitude = _errorVisualVector.magnitude;
      if (positionErrorMagnitude > ErrorPositionTeleportDistance) {
        positionTeleport = true;
        _errorVisualVector = default(Vector3);
        // we need to revert the alias interpolation when detecting a visual teleport
        param.NewPosition = param.UninterpolatedPosition;
      } else {
        var blendDiff = ErrorPositionBlendEnd - ErrorPositionBlendStart;
        var blendRate = Mathf.Clamp01((positionErrorMagnitude - ErrorPositionBlendStart) / blendDiff);
        positionCorrectionRate = Mathf.Lerp(ErrorCorrectionRateMin, ErrorCorrectionRateMax, blendRate);
      }

      var quatDot = Quaternion.Dot(_errorVisualQuaternion, Quaternion.identity);
      // ensuring we stay within acos domain
      quatDot = Mathf.Clamp(quatDot, -1, 1);

      // angle, in radians, between the two quaternions
      var rotationErrorMagnitude = Mathf.Acos(quatDot) * 2.0f;
      if (rotationErrorMagnitude > ErrorRotationTeleportDistance) {
        rotationTeleport = true;
        _errorVisualQuaternion = Quaternion.identity;
        param.NewRotation = param.UninterpolatedRotation;
      } else {
        var blendDiff = ErrorRotationBlendEnd - ErrorRotationBlendStart;
        var blendRate = Mathf.Clamp01((rotationErrorMagnitude - ErrorRotationBlendStart) / blendDiff);
        rotationCorrectionRate = Mathf.Lerp(ErrorCorrectionRateMin, ErrorCorrectionRateMax, blendRate);
      }

      // apply new position (+ potential error correction)
      param.ErrorVisualVector = _errorVisualVector;
      param.ErrorVisualQuaternion = _errorVisualQuaternion;
      param.PositionTeleport = positionTeleport;
      param.RotationTeleport = rotationTeleport;

      HostProfiler.Start("QuantumEntityView.ApplyTransform");
      ApplyTransform(ref param);
      HostProfiler.End();

      // reduce position error
      var positionCorrectionMultiplier = 1f - (Time.deltaTime * positionCorrectionRate);
      var positionCorrectionAmount = _errorVisualVector * positionCorrectionMultiplier;
      if (positionCorrectionAmount.magnitude < ErrorPositionMinCorrection) {
        UpdateMinPositionCorrection(positionCorrectionMultiplier, positionCorrectionAmount);
      } else {
        _errorVisualVector *= positionCorrectionMultiplier;
      }

      // reduce rotation error
      _errorVisualQuaternion = Quaternion.Slerp(_errorVisualQuaternion, Quaternion.identity, Time.deltaTime * rotationCorrectionRate);
    }

    protected virtual void ApplyTransform(ref UpdatePostionParameter param) {
      if ((ViewFlags & QuantumEntityViewFlags.UseCachedTransform) > 0) {
        // Override this in subclass to change how the new position is applied to the transform.
        Transform.position = param.NewPosition + param.ErrorVisualVector;

        // Unity's quaternion multiplication is equivalent to applying rhs then lhs (despite their doc saying the opposite)
        Transform.rotation = param.ErrorVisualQuaternion * param.NewRotation;
      }
      else {
        // Override this in subclass to change how the new position is applied to the transform.
        transform.position = param.NewPosition + param.ErrorVisualVector;

        // Unity's quaternion multiplication is equivalent to applying rhs then lhs (despite their doc saying the opposite)
        transform.rotation = param.ErrorVisualQuaternion * param.NewRotation;
      }
    }

    void UpdateMinPositionCorrection(float positionCorrectionMultiplier, Vector3 positionCorrectionAmount) {
      if (_errorVisualVector.x == 0f && _errorVisualVector.y == 0f && _errorVisualVector.z == 0f) {
        return;
      }

      // calculate normalized vector
      var normalized = _errorVisualVector.normalized;

      // store signs so we know when we flip an axis
      var xSign = _errorVisualVector.x >= 0f;
      var ySign = _errorVisualVector.y >= 0f;
      var zSign = _errorVisualVector.z >= 0f;

      // subtract vector by normalized*ErrorPositionMinCorrection
      _errorVisualVector -= (normalized * ErrorPositionMinCorrection);

      // if sign flipped it means we passed zero
      if (xSign != (_errorVisualVector.x >= 0f)) {
        _errorVisualVector.x = 0f;
      }

      if (ySign != (_errorVisualVector.y >= 0f)) {
        _errorVisualVector.y = 0f;
      }

      if (zSign != (_errorVisualVector.z >= 0f)) {
        _errorVisualVector.z = 0f;
      }
    }
  }
#if !QUANTUM_ENABLE_MIGRATION
} // namespace Quantum
#endif