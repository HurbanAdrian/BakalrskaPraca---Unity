using UnityEngine;

public class HealthPotion : MonoBehaviour, ICollectible
{
    public int healthRestore;

    public void Collect()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.RestoreHealth(healthRestore);
        Destroy(gameObject);
    }

}
