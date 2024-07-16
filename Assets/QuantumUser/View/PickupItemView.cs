using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PickupItemView : QuantumEntityViewComponent
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");


    public override void OnActivate(Frame frame)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        var urlReader = FindObjectOfType<URLReader>();
        urlReader.SendNotification("This is a test notification", "success");
        #endif
}

    public override void OnLateUpdateView()
    {
        if(!PredictedFrame.TryGet(EntityRef, out PickupItem pickupItem)) return;
        if(pickupItem.PickupTime == FP._0) return;
        spriteRenderer.material.SetFloat(FillAmount, (pickupItem.CurrentPickupTime / pickupItem.PickupTime).AsFloat);
    }
}