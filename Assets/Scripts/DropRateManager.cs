using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DropRateManager : MonoBehaviour
{
    [System.Serializable]
    public class Drops 
    {
        public string name;
        public GameObject itemPrefab;

        [UnityEngine.Range(0f, 100f)]
        public float dropRate;
    }

    public List<Drops> drops;

    void OnDestroy()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        List<Drops> possibleDrops = new List<Drops>();

        foreach (Drops rate in drops) 
        {
            if (randomValue <= rate.dropRate) 
            {
                possibleDrops.Add(rate);
            }
        }
        // Ak su dropy tak spawneme nahodny z nich
        if (possibleDrops.Count > 0) 
        {
            Drops drops = possibleDrops[UnityEngine.Random.Range(0, possibleDrops.Count)];
            Instantiate(drops.itemPrefab, transform.position, Quaternion.identity);
        }
    }
}
