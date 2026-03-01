using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups;       // zoznam skupin nepriatelov, ktore sa spawnuju v tejto vlne
        public int waveQuota;                   // celkovy pocet nepritelov, ktorych treba spawnut v tejto vlne
        public float spawnInterval;             // internaval na spawnovanie nepratelov v tejto vlne
        public int spawnCount;                  // pocet spawnutych nepratelov v tejto vlne
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public int enemyCount;              // pocet nepriatelov na spawnutie v tejto skupine
        public int spawnCount;              // pocet spawnutych nepratelov v tejto skupine
        public GameObject enemyPrefab;
    }

    public List<Wave> waves;                   // zoznam vsetkych vln nepriatelov
    public int currentWaveIndex;                 // index aktualnej vlny, zacina od 0

    [Header("Spawner Attributes")]
    float spawnTimer;                       // timer na rozhodnutie kedy spawnut nepriatelov
    public int enemiesAlive;
    public int maxEnemiesAllowed;           // max povolenych nepriatelov
    public bool maxEnemiesReached = false;  // flag ci mame max nepriatelov
    public float waveInterval;              // Interval medzi vlnami
    bool isWaveActive = false;                      // flag ci je vlna aktivna

    [Header("Spawn Positions")]
    public List<Transform> relativeSpawnPoints;     // list na ulozenie vsetkych relativnych spawn pointov nepriatelov


    Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
        CalculateWaveQuota();
    }

    // Update is called once per frame
    void Update()
    {
        // kontrola ci sucastna vlna skoncila a ma zacat dalsia. zavolanie korutiny
        if (currentWaveIndex < waves.Count && waves[currentWaveIndex].spawnCount == 0 && !isWaveActive)
        {
            StartCoroutine(BeginNextWave());
        }


        spawnTimer += Time.deltaTime;

        // skontroluje ci je cas na spawnovanie ak hej tak spawne
        if (spawnTimer >= waves[currentWaveIndex].spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemies();
        }
    }

    IEnumerator BeginNextWave()
    {
        isWaveActive = true;

        // Vlna pre 'WaveInterval' sekund pred zacatim dalsej vlny
        yield return new WaitForSeconds(waveInterval);

        // Ak mame dalsie vlny po sucastnej, tak sa na ne presunieme
        if (currentWaveIndex < waves.Count - 1)
        {
            isWaveActive = false;
            currentWaveIndex++;
            CalculateWaveQuota();
        }
    }

    void CalculateWaveQuota()
    {
        int currentQuota = 0;
        foreach (var group in waves[currentWaveIndex].enemyGroups)
        {
            currentQuota += group.enemyCount;
        }

        waves[currentWaveIndex].waveQuota = currentQuota;
        //Debug.Log($"Wave {waves[currentWaveIndex].waveName} quota calculated: {currentQuota}");
    }

    // Metóda na spawnovanie neprietlov a ak presiahne pocet nepriatelov na mape MAX tak prestane spawnovat nepriatelov
    void SpawnEnemies()
    {
        // skontroluj ci sme este nedosiahli kvotu pre aktualnu vlnu
        if (waves[currentWaveIndex].spawnCount < waves[currentWaveIndex].waveQuota && !maxEnemiesReached)
        {
            // spawnuj nepriatelov z jednotlivych skupin, kym nedosiahnes kvotu alebo kym nenarazis na skupinu, ktora este nema spawnut vsetkych nepriatelov
            foreach (var group in waves[currentWaveIndex].enemyGroups)
            {
                // skontroluj ci MIN cislo spawnutych nepriatelov v tejto skupine este nedosiahlo pocet nepriatelov, ktorych treba spawnut v tejto skupine
                if (group.spawnCount < group.enemyCount)
                {
                    // Spawnovanie nepriatela na nahodnej pozicii blizko pri hracovi
                    Instantiate(group.enemyPrefab, player.position + relativeSpawnPoints[Random.Range(0, relativeSpawnPoints.Count)].position, Quaternion.identity);

                    group.spawnCount++;
                    waves[currentWaveIndex].spawnCount++;
                    enemiesAlive++;

                    // Limit poctu spawnutych neprietelov v jednom case
                    if (enemiesAlive >= maxEnemiesAllowed)
                    {
                        maxEnemiesReached = true;
                        return;
                    }
                }
            }
        }
    }

    // metoda ktoru zavola nepriatel ked zomrie
    public void OnEnemyKilled()
    {
        enemiesAlive--;

        // reset maxEnemiesReached ak ich je menej
        if (enemiesAlive < maxEnemiesAllowed)
        {
            maxEnemiesReached = false;
        }
    }
}
