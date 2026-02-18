using UnityEngine;

public class HealthPotion : PickUp, ICollectible
{
    public int healthRestore;

    public void Collect()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.RestoreHealth(healthRestore);
    }

}
