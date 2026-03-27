using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LayoutGroup))]
public class UIInventoryIconsDisplay : MonoBehaviour
{
    public GameObject slotTemplate;
    public uint maxSlots = 6;
    public bool showLevels = true;
    public PlayerInventory inventory;

    public GameObject[] slots;

    [Header("Paths")]
    public string iconPath;
    public string levelTextPath;
    [HideInInspector] public string targetedItemList;

    void Reset()
    {
        slotTemplate = transform.GetChild(0).gameObject;
        inventory = FindAnyObjectByType<PlayerInventory>();
    }

    void OnEnable()
    {
        Refresh();
    }

    // T·to funkcia preËÌta invent·r a zistÌ, Ëi pribudli novÈ predmety.
    public void Refresh()
    {
        if (!inventory)
        {
            Debug.LogWarning("K UI zobrazeniu ikon nie je pripojen˝ ûiadny invent·r.");
            return;
        }

        // Zisti, ktor˝ zoznam (zbrane alebo pasÌvky) chceme zobraziù.
        Type t = typeof(PlayerInventory);
        FieldInfo field = t.GetField(targetedItemList, BindingFlags.Public | BindingFlags.Instance);

        // Ak sa danÈ pole (field) v invent·ri nenaölo, zobraz varovanie.
        if (field == null)
        {
            Debug.LogWarning("Zoznam v invent·ri sa nenaöiel.");
            return;
        }

        // ZÌskaj zoznam invent·rnych slotov cez Reflection.
        List<PlayerInventory.Slot> items = (List<PlayerInventory.Slot>)field.GetValue(inventory);

        // ZaËni napÂÚaù ikony v UI.
        for (int i = 0; i < items.Count; i++)
        {
            // Skontroluj, Ëi m·me v UI dostatok slotov pre predmety z invent·ra. Ak nie, vypÌö varovanie pre v˝voj·ra.
            if (i >= slots.Length)
            {
                Debug.LogWarning(
                    string.Format(
                        "M·ö {0} invent·rnych slotov, ale len {1} slotov v UI.",
                        items.Count, slots.Length
                    )
                );
                break;
            }

            Item item = items[i].item;

            Transform iconObj = slots[i].transform.Find(iconPath);
            if (iconObj)
            {
                Image icon = iconObj.GetComponentInChildren<Image>();


                if (icon != null)
                {
                    // Ak predmet v slote neexistuje, nastav ikonu ako ˙plne priehæadn˙.
                    if (!item)
                    {
                        icon.color = new Color(1, 1, 1, 0); // Priehæadn·, ak nem·me predmet
                    }
                    else
                    {
                        icon.color = new Color(1, 1, 1, 1); // Viditeæn·
                        icon.sprite = item.data.icon;
                    }
                }
            }

            // Nastav aj zobrazenie ˙rovne (levelu).
            Transform levelObj = slots[i].transform.Find(levelTextPath);
            if (levelObj)
            {
                // N·jdi komponent TextMeshPro a vloû doÚ ˙roveÚ.
                TextMeshProUGUI levelTxt = levelObj.GetComponentInChildren<TextMeshProUGUI>();
                if (levelTxt)
                {
                    if (!item || !showLevels) levelTxt.text = "";
                    else
                    {
                        levelTxt.text = item.currentLevel.ToString();
                    }
                }
            }
        }
    }

}
