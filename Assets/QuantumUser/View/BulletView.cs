using Quantum;
using UnityEngine;

public class BulletView : QuantumEntityViewComponent
{
    [SerializeField] private TrailRenderer trailRenderer;
    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        var localPosition = trailRenderer.transform.localPosition;
        localPosition = new Vector3(localPosition.x, (float)frame.Get<Bullet>(EntityRef).HeightOffset, localPosition.z);
        trailRenderer.transform.localPosition = localPosition;
        trailRenderer.enabled = true;
    }

    public override void OnDeactivate()
    {
        trailRenderer.enabled = false;
    }
}