using UnityEngine;

public class ExperienceGem : PickUp, ICollectible
{
    public int experienceAmount;
    public void Collect()
    {
        //Debug.Log("Collected Experience Gem! Gained " + experienceAmount + " experience.");
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.IncreaseExperience(experienceAmount);
    }
}
