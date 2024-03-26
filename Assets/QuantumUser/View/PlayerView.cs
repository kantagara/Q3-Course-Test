using System;
using Quantum;
using UnityEngine;
using LayerMask = UnityEngine.LayerMask;

public class PlayerView : QuantumEntityViewComponent
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrow;

    private Renderer[] _renderers;
    
    private static readonly int MoveZ = Animator.StringToHash("moveZ");
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private bool _isLocalPlayer;

    private void Awake()
    {
        _renderers = GetComponentsInChildren <Renderer> (true);
    }

    public override void OnActivate(Frame frame)
    {
        _isLocalPlayer = _game.PlayerIsLocal(PredictedFrame.Get<PlayerLink>(EntityRef).Player);
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
        if(_isLocalPlayer) return;
        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }

    private void OnPlayerEnterGrass(EventOnPlayerEnteredGrass callback)
    {
        if (callback.player != EntityRef) return;
        if(_isLocalPlayer) return;
        foreach (var renderer in _renderers)
        {
            renderer.enabled = false;
        }
    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
        base.OnDeactivate();
    }

    public override void OnUpdateView()
    {
        UpdateAnimator();
        ChangeMaterialIfObstructed();
    }

    private void ChangeMaterialIfObstructed()
    {
        
    }

    private unsafe void UpdateAnimator()
    {
        var input = PredictedFrame.GetPlayerInput(PredictedFrame.Get<PlayerLink>(EntityRef).Player);
        animator.SetFloat(MoveX, (float)input->Movement.X);
        animator.SetFloat(MoveZ, (float)input->Movement.Y);
    }
}
