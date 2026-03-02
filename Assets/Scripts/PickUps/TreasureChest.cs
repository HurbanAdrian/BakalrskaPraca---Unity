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
        if (collision.gameObject.CompareTag("Player"))
        {
            OpenTreasureChest();
            Destroy(gameObject);
        }
    }

    public void OpenTreasureChest()
    {
        if (inventory.GetPossibleEvolution().Count <= 0)
        {
            Debug.Log("No possible evolutions");
            return;
        }

        WeaponEvolutionBlueprint toEvolve = inventory.GetPossibleEvolution()[Random.Range(0, inventory.GetPossibleEvolution().Count)];
        inventory.EvolveWeapon(toEvolve);
    }
}
