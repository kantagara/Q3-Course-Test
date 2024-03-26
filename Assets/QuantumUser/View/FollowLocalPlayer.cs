using Quantum;

public  class FollowLocalPlayer : QuantumViewComponent<CameraViewContext>
{
    public override void OnActivate(Frame frame)
    {
        FollowPlayerIfLocal(frame);
    }

    private void FollowPlayerIfLocal(Frame frame)
    {
        if(!frame.TryGet(_entityView.EntityRef,out PlayerLink playerLink))
            return;
        if(!_game.PlayerIsLocal(playerLink.Player))
            return;
        ViewContext.VirtualCamera.Follow = _entityView.transform;
    }
}