using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Game/Weapon Data")]
public class WeaponData : ItemData
{
    [HideInInspector]
    public string behaviour;
    public Weapon.Stats baseStats;
    public Weapon.Stats[] linearGrowth;
    public Weapon.Stats[] randomGrowth;

    // Vrati narast statov / popis dalsieho levelu
    public override Item.LevelData GetLevelData(int level)
    {
        if (level <= 1) return baseStats;

        // Zoberie Staty z dalsieho levelu
        if (level - 2 < linearGrowth.Length)
        {
            return linearGrowth[level - 2];
        }

        // Inac zoberie nahodne staty z randomGrowth arrayu
        if (randomGrowth.Length > 0)
        {
            return randomGrowth[Random.Range(0, randomGrowth.Length)];
        }

        Debug.LogWarning($"Weapon has no growth data for level {level}");
        return new Weapon.Stats();
    }
}
