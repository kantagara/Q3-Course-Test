using Quantum;
using UnityEngine;
using LayerMask = UnityEngine.LayerMask;

public unsafe class PlayerView : QuantumEntityViewComponent
{
    private static readonly int MoveZ = Animator.StringToHash("moveZ");
    private static readonly int MoveX = Animator.StringToHash("moveX");
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrow;
    private bool _isLocalPlayer;
    private PlayerLink* _playerLink;

    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public override void OnActivate(Frame frame)
    {
        _playerLink = VerifiedFrame.Unsafe.GetPointer<PlayerLink>(EntityRef);
        _isLocalPlayer = _game.PlayerIsLocal(_playerLink->Player);
        
        arrow.SetActive(_isLocalPlayer);
        var layer = LayerMask.NameToLayer(_isLocalPlayer ? "Player_Local" : "Player_Remote");


        foreach (var renderer in _renderers)
        {
            renderer.gameObject.layer = layer;
            renderer.enabled = true;
        }

        QuantumEvent.Subscribe<EventOnPlayerEnteredGrass>(this, OnPlayerEnterGrass);
        QuantumEvent.Subscribe<EventOnPlayerExitGrass>(this, OnPlayerExitGrass);
    }
    

    private void OnPlayerExitGrass(EventOnPlayerExitGrass callback)
    {
        if (callback.player != EntityRef) return;
        if (_isLocalPlayer) return;
        foreach (var renderer in _renderers) renderer.enabled = true;
    }

    private void OnPlayerEnterGrass(EventOnPlayerEnteredGrass callback)
    {
        if (callback.player != EntityRef) return;
        if (_isLocalPlayer) return;
        foreach (var renderer in _renderers) renderer.enabled = false;
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
        if(_playerLink == default) return;
        var input = PredictedFrame.GetPlayerInput(_playerLink->Player);
        animator.SetFloat(MoveX, (float)input->Movement.X);
        animator.SetFloat(MoveZ, (float)input->Movement.Y);
    }
}