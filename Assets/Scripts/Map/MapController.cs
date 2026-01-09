using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public List<GameObject> terrainChunks;              //pre prefaby terrain chunkov
    public GameObject player;
    public float checkerRadius;
    public Vector3 noTerrainPosition;              // na dalsiu poziciu kde nie je terrain chunk
    public LayerMask terrainMask;           // ktory je teren a ktory nie
    public GameObject currentChunk;
    PlayerMovement playerMovement;

    [Header("Optimization")]
    public List<GameObject> spawnedChunks;        // zoznam spawnnutych chunkov pre optimalizaciu
    GameObject latestChunk;
    public float maxOpDist;         // must be greater than the length and width of a tilemap
    float opDist;
    float optimizerCooldown;
    public float optimizerCooldownDuration;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker() 
    {
        if (!currentChunk)
        {
            return;
        }

        if (playerMovement.moveDir.x > 0 && playerMovement.moveDir.y == 0)      // doprava
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Right").position;
                SpawnChunk();       // zavolat az potom co nastavime noterrainposition
                Debug.Log("Spawning chunk at: " + noTerrainPosition);
            }
        }
        else if (playerMovement.moveDir.x < 0 && playerMovement.moveDir.y == 0) // dolava
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Left").position;
                SpawnChunk();
            }
        }
        else if(playerMovement.moveDir.y > 0 && playerMovement.moveDir.x == 0) // hore
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Up").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Up").position;
                SpawnChunk();
            }
        }
        else if (playerMovement.moveDir.y < 0 && playerMovement.moveDir.x == 0) // dole
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Down").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Down").position;
                SpawnChunk();
            }
        }
        else if (playerMovement.moveDir.x > 0 && playerMovement.moveDir.y > 0)      // doprava hore
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right Up").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Right Up").position;
                SpawnChunk();       // zavolat az potom co nastavime noterrainposition
            }
        }
        else if (playerMovement.moveDir.x > 0 && playerMovement.moveDir.y < 0)      // doprava dole
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Right Down").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Right Down").position;
                SpawnChunk();       // zavolat az potom co nastavime noterrainposition
            }
        }
        else if (playerMovement.moveDir.x < 0 && playerMovement.moveDir.y > 0)      // dolava hore
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left Up").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Left Up").position;
                SpawnChunk();       // zavolat az potom co nastavime noterrainposition
            }
        }
        else if (playerMovement.moveDir.x < 0 && playerMovement.moveDir.y < 0)      // dolava dole
        {
            if (!Physics2D.OverlapCircle(currentChunk.transform.Find("Left Down").position, checkerRadius, terrainMask))
            {
                noTerrainPosition = currentChunk.transform.Find("Left Down").position;
                SpawnChunk();       // zavolat az potom co nastavime noterrainposition
            }
        }
    }

    void SpawnChunk()  
    {
        int randomIndex = Random.Range(0, terrainChunks.Count);
        latestChunk = Instantiate(terrainChunks[randomIndex], noTerrainPosition, Quaternion.identity);
        spawnedChunks.Add(latestChunk);
    }

    void ChunkOptimizer() 
    {
        optimizerCooldown -= Time.deltaTime;

        if (optimizerCooldown <= 0f)
        {
            optimizerCooldown = optimizerCooldownDuration;
        }
        else
        {
            return;
        }

        foreach (GameObject chunk in spawnedChunks)
            {
                opDist = Vector3.Distance(player.transform.position, chunk.transform.position);
                if (opDist > maxOpDist)
                {
                    chunk.SetActive(false);
                }
                else
                {
                    chunk.SetActive(true);
                }
            }
    }
}
