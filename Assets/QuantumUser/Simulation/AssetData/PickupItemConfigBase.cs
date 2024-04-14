using Photon.Deterministic;

namespace Quantum
{
    public unsafe class PickupItemConfigBase : AssetObject
    {
        public FP PickupTime;

        public virtual void Init(PickupItem* pickupItem)
        {
            pickupItem->PickupTime = PickupTime;
        }

        public virtual void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            f.Destroy(entityBeingPickedUp);
        }
    }
}