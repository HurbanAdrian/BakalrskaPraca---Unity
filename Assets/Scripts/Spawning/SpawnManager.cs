using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    int currentWaveIndex; // Index aktuálnej vlny (zoznam zaèína od 0)
    int currentWaveSpawnCount = 0; // Sleduje, ko¾ko nepriate¾ov vytvorila aktuálna vlna
    List<GameObject> existingSpawns = new List<GameObject>();

    public WaveData[] data;
    public Camera referenceCamera;

    [Tooltip("Ak je na scéne viac nepriate¾ov ako toto èíslo, prestaneme ich vytvára. Kvôli vıkonu.")]
    public int maximumEnemyCount = 300;
    
    float spawnTimer; // Èasovaè na urèenie, kedy vytvori ïalšiu skupinu
    float currentWaveDuration = 0f;

    public static SpawnManager instance;

    void Start()
    {
        if(instance) Debug.LogWarning("V scéne je viac ako 1 Spawn Manager! Odstráòte nadbytoèné.");
        instance = this;
    }

    void Update()
    {
        // Aktualizácia èasovaèov v kadom snímku
        spawnTimer -= Time.deltaTime;
        currentWaveDuration += Time.deltaTime;

        if(spawnTimer <= 0) 
        {
            // Skontrolujeme, èi sme pripravení prejs na novú vlnu
            if(HasWaveEnded())
            {
                currentWaveIndex++;
                currentWaveDuration = currentWaveSpawnCount = 0;

                // Ak sme prešli všetky vlny, vypneme tento komponent
                if (currentWaveIndex >= data.Length)
                {
                    Debug.Log("Všetky vlny boli vytvorené! Vypínam manager.", this);
                    enabled = false;
                }

                return;
            }

            // Ak nespåòame podmienky pre spawn (napr. limit nepriate¾ov), preskoèíme cyklus
            if (!CanSpawn())
            {
                spawnTimer += data[currentWaveIndex].GetSpawnInterval();
                return;
            }

            // Získame pole nepriate¾ov, ktorıch ideme v tomto "ticku" vytvori
            GameObject[] spawns = data[currentWaveIndex].GetSpawns(EnemyStats.count);

            // Prejdeme pole a vytvoríme prefaby
            foreach(GameObject prefab in spawns)
            {
                // Ak poèas procesu prekroèíme limit, zastavíme sa
                if (!CanSpawn()) continue;

                // Samotné vytvorenie nepriate¾a na náhodnej pozícii
                existingSpawns.Add(Instantiate(prefab, GeneratePosition(), Quaternion.identity));
                currentWaveSpawnCount++;
            }

            // Regenerácia èasovaèa pre ïalší spawn
            spawnTimer += data[currentWaveIndex].GetSpawnInterval();
        }
    }

    // Spåòame podmienky na pokraèovanie vytvárania nepriate¾ov?
    public bool CanSpawn()
    {
        // Nepridávaj, ak sme prekroèili globálny limit vıkonu
        if (HasExceededMaxEnemies()) return false;

        // Nepridávaj, ak sme dosiahli limit pre túto konkrétnu vlnu
        if (currentWaveSpawnCount >= data[currentWaveIndex].totalSpawns) return false;

        // Nepridávaj, ak vlna u mala skonèi pod¾a èasu
        if (currentWaveDuration > data[currentWaveIndex].duration) return false;

        return true;
    }

    // Umoòuje inım skriptom zisti, èi je na scéne príliš ve¾a nepriate¾ov
    public static bool HasExceededMaxEnemies()
    {
        if (!instance) return false; // Ak manager neexistuje, nelimituj
        if (EnemyStats.count > instance.maximumEnemyCount) return true;
        return false;
    }

    public bool HasWaveEnded()
    {
        WaveData currentWave = data[currentWaveIndex];

        // Ak je trvanie vlny podmienkou konca, skontroluj èas
        if ((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
        {
            if (currentWaveDuration < currentWave.duration) return false;
        }

        // Ak je poèet vytvorenıch nepriate¾ov podmienkou, skontroluj stav
        if ((currentWave.exitConditions & WaveData.ExitCondition.reachedTotalSpawns) > 0)
        {
            if (currentWaveSpawnCount < currentWave.totalSpawns) return false;
        }

        // Ak je zaškrtnuté 'mustKillAll', musíme poèka na smr všetkıch nepriate¾ov
        existingSpawns.RemoveAll(item => item == null);
        if (currentWave.mustKillAll && existingSpawns.Count > 0)
        {
            return false;
        }

        return true;
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }

    // Vytvorí novú pozíciu na umiestnenie nepriate¾a
    public static Vector3 GeneratePosition()
    {
        // Ak nemáme kameru, skúsime nájs hlavnú
        if(!instance.referenceCamera) instance.referenceCamera = Camera.main;

        // Varovanie, ak kamera nie je ortografická (pre 2D roguelike dôleité)
        if(!instance.referenceCamera.orthographic)
            Debug.LogWarning("Referenèná kamera nie je ortografická! Spawny sa môu objavi v zábere.");

        // Vygeneruje náhodné èísla pre osi X a Y (0 a 1 v rámci viewportu)
        float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

        // Náhodne vyberieme, èi "zaokrúhlime" X alebo Y na hranicu (0 alebo 1),
        // aby nepriate¾ vznikol tesne za okrajom obrazovky.
        switch(Random.Range(0, 2)) {
            case 0: default:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y));
            case 1:
                return instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y)));
        }
    }

    // Kontrola, èi je objekt v zábere kamery
    public static bool IsWithinBoundaries(Transform checkedObject)
    {
        Camera c = instance && instance.referenceCamera ? instance.referenceCamera : Camera.main;

        Vector2 viewport = c.WorldToViewportPoint(checkedObject.position);
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;

        return true;
    }
}
