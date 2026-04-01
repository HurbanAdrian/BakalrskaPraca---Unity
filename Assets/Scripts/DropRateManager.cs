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

    public bool active = false;
    public List<Drops> drops;

    void OnDestroy()
    {
        if (!active) return;        // zabranenie spawnovaniu dropov ak sa nepriatel despawne

        // Skontrolujeme ci je scena nahrata, aby sme predišli chybam pri znièení objektu v neaktivnej scéne || Overuje ci sme v Play mode, aby sme predišli chybam pri znièení objektu v editore
        if (!gameObject.scene.isLoaded)
        {
            return;
        }

        Drops rarestRolledDrop = null;
        float lowestDropRate = 101f;

        foreach (Drops rate in drops)
        {
            float randomValue = UnityEngine.Random.Range(0f, 100f);

            // 1. Skontrolujeme, èi predmet vôbec padol
            if (randomValue <= rate.dropRate)
            {
                // 2. Ak padol, skontrolujeme, èi je VZÁCNEJŠÍ (má menšie %) ako ten, èo sme si zatia¾ odložili
                if (rate.dropRate < lowestDropRate)
                {
                    rarestRolledDrop = rate;
                    lowestDropRate = rate.dropRate;
                }
            }
        }

        // Ak sme nieèo vyrolovali, spawneme ten najvzácnejší víazný predmet
        if (rarestRolledDrop != null)
        {
            Instantiate(rarestRolledDrop.itemPrefab, transform.position, Quaternion.identity);
        }
    }
}
