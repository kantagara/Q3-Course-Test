namespace Quantum
{
    public class PickupItemHealth : PickupItemConfigBase
    {
        public override unsafe void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            if (!f.Unsafe.TryGetPointer(entityPickingUp, out Damageable* damageable))
            {
                Log.DebugError("Can't find damageable on " + entityPickingUp);
                return;
            }

            damageable->CurrentHealth = damageable->MaxHealth;
            f.Events.PlayerHealthUpdated(entityPickingUp, damageable->CurrentHealth, damageable->MaxHealth);
            
            f.Destroy(entityBeingPickedUp);
        }
    }
}