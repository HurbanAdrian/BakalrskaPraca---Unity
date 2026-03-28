using UnityEngine;

public abstract class SpawnData : ScriptableObject
{
    [Tooltip("Zoznam všetkıch monıch Prefabov (GameObjectov), ktoré môu by vytvorené.")]
    public GameObject[] possibleSpawnPrefabs = new GameObject[1];

    [Tooltip("Èas medzi jednotlivımi vlnami (v sekundách). Vyberie náhodné èíslo medzi X a Y.")]
    public Vector2 spawnInterval = new Vector2(2, 3);

    [Tooltip("Ko¾ko nepriate¾ov sa vytvorí pri kadom intervale?")]
    public Vector2Int spawnsPerTick = new Vector2Int(1, 1);

    [Tooltip("Ako dlho (v sekundách) bude trva táto vlna vytvárania nepriate¾ov.")]
    [Min(0.1f)] public float duration = 60;

    // Vráti pole prefabov, ktoré by sa mali vytvori (spawnú). Berie volite¾nı parameter celkového poètu nepriate¾ov, ktorí sú momentálne na scéne.
    public virtual GameObject[] GetSpawns(int totalEnemies = 0)
    {
        // Urèí, ko¾ko nepriate¾ov sa má vytvori na základe náhodného rozsahu.
        int count = Random.Range(spawnsPerTick.x, spawnsPerTick.y);

        // Vygeneruje vısledné pole.
        GameObject[] result = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            // Náhodne vyberie jeden z monıch prefabov a vloí ho do vısledného po¾a.
            result[i] = possibleSpawnPrefabs[Random.Range(0, possibleSpawnPrefabs.Length)];
        }

        return result;
    }

    // Vráti náhodnı interval medzi minimálnou a maximálnou hodnotou.
    public virtual float GetSpawnInterval()
    {
        return Random.Range(spawnInterval.x, spawnInterval.y);
    }
}
