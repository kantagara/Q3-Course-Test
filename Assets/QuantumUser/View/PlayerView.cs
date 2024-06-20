using Photon.Client.StructWrapping;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using LayerMask = UnityEngine.LayerMask;

public unsafe class PlayerView : QuantumEntityViewComponent
{
    private static readonly int MoveZ = Animator.StringToHash("moveZ");
    private static readonly int MoveX = Animator.StringToHash("moveX");
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject overHeadUi;
    private bool _isLocalPlayer;

    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public override void OnActivate(Frame frame)
    {
        _isLocalPlayer = _game.PlayerIsLocal(frame.Get<PlayerLink>(EntityRef).Player);
        
        var layer = LayerMask.NameToLayer(_isLocalPlayer ? "Player_Local" : "Player_Remote");


        foreach (var renderer in _renderers)
        {
            renderer.gameObject.layer = layer;
            renderer.enabled = true;
        }
        overHeadUi.SetActive(true);
        QuantumEvent.Subscribe<EventOnPlayerEnteredGrass>(this, OnPlayerEnterGrass);
        QuantumEvent.Subscribe<EventOnPlayerExitGrass>(this, OnPlayerExitGrass);
        QuantumEvent.Subscribe<EventOnPlayerExitGrass>(this, OnPlayerExitGrass);
    }
    

    private void OnPlayerExitGrass(EventOnPlayerExitGrass callback)
    {
        if (callback.player != EntityRef) return;
        if (_isLocalPlayer) return;
        foreach (var renderer in _renderers) renderer.enabled = true;
        overHeadUi.SetActive(true);
    }

    private void OnPlayerEnterGrass(EventOnPlayerEnteredGrass callback)
    {
        if (callback.player != EntityRef) return;
        if (_isLocalPlayer) return;
        foreach (var renderer in _renderers) renderer.enabled = false;
        overHeadUi.SetActive(false);
    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
        base.OnDeactivate();
    }

    public override void OnUpdateView()
    {
        UpdateAnimator();
    }
    
    private void UpdateAnimator()
    {
        if (!EntityRef.IsValid) return;
        //For now we can assume that all players have the same Move Along Rotation field. 
        //Later on we can change that and introduce some setting field that can be modified at runtime 
        var input = PredictedFrame.GetPlayerInput(VerifiedFrame.Get<PlayerLink>(EntityRef).Player);
        var kcc = VerifiedFrame.Get<KCC>(EntityRef);
        var kccSettings = VerifiedFrame.FindAsset(kcc.Settings);
        FPVector2 animatorVector = kccSettings.MoveAlongsideRotation ? input->Movement : kcc.Velocity;
        animator.SetFloat(MoveX, animatorVector.X.AsFloat);
        animator.SetFloat(MoveZ, animatorVector.Y.AsFloat);
    }
}