
namespace Quantum {
  using Photon.Deterministic;

  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnTriggerEnter2D, ISignalOnTriggerExit2D, ISignalOnComponentAdded<KCC>
  {
    public override void OnInit(Frame f)
    {
    }

    public override void Update(Frame f, ref Filter filter)
    {
      var input = f.GetPlayerInput(filter.PlayerLink->Player);
      RotatePlayer(filter, input);
      var direction = input->Movement;
      if (direction.Magnitude > 1)
        direction = direction.Normalized;
      MoveCharacter(f, direction, filter.Entity, filter.Kcc, filter.Transform, input);
    }
      
    private void MoveCharacter(Frame frame, FPVector2 direction, EntityRef entity, KCC* kcc, Transform2D* transform, Input* input)
    {
      KCCSettings kccSettings = frame.FindAsset<KCCSettings>(kcc->Settings.Id);
      KCCMovementData kccMovementData = kccSettings.ComputeRawMovement(frame, entity, direction);
      kccSettings.SteerAndMove(frame, entity, in kccMovementData);
    }

    private void RotatePlayer(Filter filter, Input* input)
    {
      var direction = (input->MousePosition - filter.Transform->Position).XOY;
      filter.Transform->Rotation = FPVector2.RadiansSigned(FPVector2.Up, direction.XZ);
    }
    
    public struct Filter {
      public EntityRef Entity;
      public KCC* Kcc;
      public Transform2D* Transform;
      public PlayerLink* PlayerLink;
    }

    public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
    {
      if(!f.TryGet(info.Entity, out KCC _)) return;
      if(!f.TryGet(info.Other, out Grass _)) return;
      f.Events.OnPlayerEnteredGrass(info.Entity);
    }

    public void OnTriggerExit2D(Frame f, ExitInfo2D info)
    {
      if(!f.TryGet(info.Other, out Grass _)) return;
      if(!f.TryGet(info.Entity, out KCC _)) return;
      f.Events.OnPlayerExitGrass(info.Entity);
    }

    public void OnAdded(Frame f, EntityRef entity, KCC* kcc)
    {
      var settings = f.FindAsset(kcc->Settings);
      kcc->Acceleration = settings.Acceleration;
      kcc->MaxSpeed = settings.BaseSpeed;
    }
  }
}
