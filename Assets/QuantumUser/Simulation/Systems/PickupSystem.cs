using Photon.Deterministic;
using Quantum;

namespace QuantumUser.Simulation.Systems
{
    public unsafe class PickupSystem : SystemMainThreadFilter<PickupSystem.Filter>, ISignalOnComponentAdded<PickupItem>,
        ISignalOnTriggerEnter2D, ISignalOnTriggerExit2D, ISignalPlayerKilled
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public PickupItem* PickupItem;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if(filter.PickupItem->EntityPickingUp == EntityRef.None) return;
            
            filter.PickupItem->CurrentPickupTime += f.DeltaTime;
            if(filter.PickupItem->CurrentPickupTime >= filter.PickupItem->PickupTime)
            {
                var asset = f.FindAsset(filter.PickupItem->PickupItemConfigBase);
                asset.PickupItem(f, filter.EntityRef, filter.PickupItem->EntityPickingUp);
            }
            
        }

        public void OnAdded(Frame f, EntityRef entity, PickupItem* pickupItem)
        {
            var asset = f.FindAsset(pickupItem->PickupItemConfigBase);
            if (asset == null)
            {
                Log.DebugWarn("PickupItemConfigBase not found");
                return;
            }
            asset.Init(pickupItem);
        }

        public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
        {
            if(!f.TryGet(info.Other, out PlayerLink _)) return;
            if(!f.Unsafe.TryGetPointer(info.Entity, out PickupItem* pickupItem)) return;
            if(pickupItem->EntityPickingUp != EntityRef.None) return;
            pickupItem->EntityPickingUp = info.Other;
        }

        public void OnTriggerExit2D(Frame f, ExitInfo2D info)
        {
            if(!f.TryGet(info.Other, out PlayerLink _)) return;
            if(!f.Unsafe.TryGetPointer(info.Entity, out PickupItem* pickupItem)) return;
            if(pickupItem->EntityPickingUp != info.Other) return;
            pickupItem->EntityPickingUp = EntityRef.None;
            pickupItem->CurrentPickupTime = FP._0; 
        }

        public void PlayerKilled(Frame f, EntityRef Player)
        {
            foreach (var entity in f.Unsafe.GetComponentBlockIterator<PickupItem>())
            {
                if(entity.Component->EntityPickingUp == Player)
                {
                    entity.Component->EntityPickingUp = EntityRef.None;
                    entity.Component->CurrentPickupTime = FP._0; 
                }
            }
        }
    }
}