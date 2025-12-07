using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PropRandomizer : MonoBehaviour
{
    public List<GameObject> propSpawnsPoints;
    public List<GameObject> propPrefabs;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnProps();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnProps() 
    {
        foreach (var spawnPoint in propSpawnsPoints) 
        {
            int randomIndex = Random.Range(0, propPrefabs.Count);
            GameObject prop =  Instantiate(propPrefabs[randomIndex], spawnPoint.transform.position, Quaternion.identity);
            prop.transform.parent = spawnPoint.transform;
        }
    }
}
