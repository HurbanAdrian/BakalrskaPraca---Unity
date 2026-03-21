using UnityEngine;

public class PassiveItem : MonoBehaviour
{
    protected PlayerStats playerStats;
    public PassiveItemScriptableObject passiveItemData;

    protected virtual void ApplyModifier() 
    {
        // Tu sa aplikuje efekt pasivneho itemu na playera, napr. zvysenie damage, rychlosti atd. Podla toho co mas v pasivnom iteme
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
        ApplyModifier();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
