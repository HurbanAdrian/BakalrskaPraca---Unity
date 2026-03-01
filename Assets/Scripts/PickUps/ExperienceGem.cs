using UnityEngine;

public class ExperienceGem : PickUp
{
    public int experienceAmount;
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

        //Debug.Log("Collected Experience Gem! Gained " + experienceAmount + " experience.");
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        player.IncreaseExperience(experienceAmount);
    }
}
