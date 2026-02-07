using UnityEngine;

public class ExperienceGem : MonoBehaviour, ICollectible
{
    public int experienceAmount;
    public void Collect()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.IncreaseExperience(experienceAmount);
        Destroy(gameObject);
    }

}
