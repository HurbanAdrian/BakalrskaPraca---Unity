using UnityEngine;

[CreateAssetMenu(fileName = "Wave Data", menuName = "Game/Wave Data")]
public class WaveData : SpawnData
{
    [Header("Wave Data")]

    [Tooltip("Ak je na scéne menej nepriate¾ov ako toto èíslo, budeme ich vytvára, kım tento poèet nedosiahneme.")]
    [Min(0)] public int startingCount = 0;

    [Tooltip("Ko¾ko nepriate¾ov maximálne môe táto vlna vytvori?")]
    [Min(1)] public uint totalSpawns = uint.MaxValue;

    [System.Flags] public enum ExitCondition { waveDuration = 1, reachedTotalSpawns = 2 }
    [Tooltip("Nastavte podmienky, ktoré môu spusti koniec tejto vlny.")]
    public ExitCondition exitConditions = (ExitCondition)1;

    [Tooltip("Všetci nepriatelia musia by màtvi, aby sa postúpilo do ïalšej vlny.")]
    public bool mustKillAll = false;

    [HideInInspector] public uint spawnCount; // Poèet nepriate¾ov u vytvorenıch v tejto vlne.

    // Vráti pole prefabov, ktoré môe táto vlna vytvori. Berie volite¾nı parameter celkového poètu nepriate¾ov, ktorí sú momentálne na scéne.
    public override GameObject[] GetSpawns(int totalEnemies = 0)
    {
        // Urèí, ko¾ko nepriate¾ov sa má vytvori (náhodnı rozsah zo základnej triedy).
        int count = Random.Range(spawnsPerTick.x, spawnsPerTick.y);

        // Ak máme na obrazovke menej ako <startingCount>, nastavíme poèet tak, aby sme doplnili stav nepriate¾ov na <startingCount>.
        if (totalEnemies + count < startingCount)
        {
            count = startingCount - totalEnemies;
        }

        // Generovanie vısledku.
        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            // Náhodne vyberie jeden z monıch prefabov a vloí ho do po¾a.
            result[i] = possibleSpawnPrefabs[Random.Range(0, possibleSpawnPrefabs.Length)];
        }

        return result;
    }
}
