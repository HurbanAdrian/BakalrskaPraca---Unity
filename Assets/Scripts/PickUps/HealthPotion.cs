using UnityEngine;

public class HealthPotion : PickUp
{
    public int healthRestore;

    public override void Collect()
    {
        if (hasBeenCollected)
        {
            return;
        }
        else
        {
            base.Collect();
        }

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.RestoreHealth(healthRestore);
    }

}
