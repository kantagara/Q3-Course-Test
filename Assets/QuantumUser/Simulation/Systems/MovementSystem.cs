namespace Quantum {
  using Photon.Deterministic;

  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D
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
      filter.CharacterController3D->Move(f, filter.Entity, filter.Transform->Rotation * direction.XOY);
    }

    private void RotatePlayer(Filter filter, Input* input)
    {
      var direction = (input->MousePosition - filter.Transform->Position.XZ);
      filter.Transform->Rotation = FPQuaternion.LookRotation(direction.XOY);
    }
    
    public struct Filter {
      public EntityRef Entity;
      public CharacterController3D* CharacterController3D;
      public Transform3D* Transform;
      public PlayerLink* PlayerLink;
    }

    public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
    {
      if(!f.TryGet(info.Entity, out Grass _)) return;
      if(!f.TryGet(info.Other, out CharacterController3D _)) return;
      f.Events.OnPlayerEnteredGrass(info.Other);
    }

    public void OnTriggerExit3D(Frame f, ExitInfo3D info)
    {
      if(!f.TryGet(info.Entity, out Grass _)) return;
      if(!f.TryGet(info.Other, out CharacterController3D _)) return;
      f.Events.OnPlayerExitGrass(info.Other);
    }
  }
}
