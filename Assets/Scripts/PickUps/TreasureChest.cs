using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    InventoryManager inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = FindAnyObjectByType<InventoryManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerInventory p = collision.GetComponent<PlayerInventory>();
        if (p)
        {
            bool randomBool = Random.Range(0, 2) == 0;

            OpenTreasureChest(p, randomBool);

            Destroy(gameObject);
        }
    }

    public void OpenTreasureChest(PlayerInventory inventory, bool isHigherTier)
    {
        // Prejde všetky zbrane a skontroluje, èi sa môu vyvinú.
        foreach (PlayerInventory.Slot s in inventory.weaponSlots)
        {
            Weapon w = s.item as Weapon;
            if (w == null || w.data.evolutionData == null) continue; // Ignoruje zbraò, ak sa nemôe vyvinú. alebo nie je plny inventar

            // Prejde všetky moné evolúcie zbrane.
            foreach (ItemData.Evolution e in w.data.evolutionData)
            {
                // Pokúsi sa vyvinú zbrane iba cez evolúciu z truhlice s pokladom.
                if (e.condition == ItemData.Evolution.Condition.treasureChest)
                {
                    bool attempt = w.AttemptEvolution(e, 0);
                    if (attempt) return; // Ak je evolúcia úspešná, zastaví sa (ukonèí metódu).
                }
            }
        }
    }
}
