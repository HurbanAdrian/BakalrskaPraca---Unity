using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIInventoryIconsDisplay))]
public class UIInventoryIconsDisplayEditor : Editor
{
    UIInventoryIconsDisplay display;
    int targetedItemListIndex = 0;
    string[] itemListOptions;

    // Táto funkcia sa spustí vždy, keï v Unity vyberiete objekt s týmto komponentom. Preh¾adá skript PlayerInventory a nájde všetky premenné typu List<PlayerInventory.Slot>.
    private void OnEnable()
    {
        // Získame prístup k cie¾ovému komponentu.
        display = target as UIInventoryIconsDisplay;

        Type playerInventoryType = typeof(PlayerInventory);

        // Získame všetky polia (premenné) v triede PlayerInventory pomocou Reflexie.
        FieldInfo[] fields = playerInventoryType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Použijeme LINQ na odfiltrovanie len tých polí, ktoré sú typu List<PlayerInventory.Slot>.
        List<string> slotListNames = fields
            .Where(field => field.FieldType.IsGenericType &&
                            field.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                            field.FieldType.GetGenericArguments()[0] == typeof(PlayerInventory.Slot))
            .Select(field => field.Name)
            .ToList();

        // Pridáme možnos "None" a prevedieme na pole pre dropdown menu.
        slotListNames.Insert(0, "None");
        itemListOptions = slotListNames.ToArray();

        targetedItemListIndex = Math.Max(0, Array.IndexOf(itemListOptions, display.targetedItemList));
    }

    // Táto funkcia vykres¾uje samotný vzh¾ad v Inspectore.
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        // Vykreslíme rozba¾ovacie menu (dropdown) pre výber zoznamu.
        targetedItemListIndex = EditorGUILayout.Popup("Targeted Item List", Mathf.Max(0, targetedItemListIndex), itemListOptions);

        if (EditorGUI.EndChangeCheck())
        {
            display.targetedItemList = itemListOptions[targetedItemListIndex].ToString();
            EditorUtility.SetDirty(display); // Oznaèí objekt, aby Unity vedelo, že ho má uloži.
        }

        if (GUILayout.Button("Generate Icons")) RegenerateIcons();
    }

    // Funkcia na automatické vygenerovanie herných objektov (slotov) v UI pod¾a šablóny.
    void RegenerateIcons()
    {
        display = target as UIInventoryIconsDisplay;

        // Zaregistrujeme akciu pre funkciu "Undo", aby ste ju mohli vráti spä (Ctrl+Z).
        Undo.RegisterCompleteObjectUndo(display, "Regenerate Icons");

        // Najprv vyèistíme staré sloty v poli slots, ak nejaké existujú.
        if (display.slots != null && display.slots.Length > 0)
        {
            foreach (GameObject g in display.slots)
            {
                if (!g) continue;

                // Odstránime objekt a zaznamenáme to pre Undo.
                if (g != display.slotTemplate)
                    Undo.DestroyObjectImmediate(g);
            }
        }

        // Pre istotu vyèistíme všetky ostatné deti objektu okrem šablóny (slotTemplate).
        for (int i = 0; i < display.transform.childCount; i++)
        {
            if (display.transform.GetChild(i).gameObject == display.slotTemplate) continue;

            Undo.DestroyObjectImmediate(display.transform.GetChild(i).gameObject);
            i--;
        }

        if (display.maxSlots <= 0) return;

        // Vytvoríme nové pole pre sloty.
        display.slots = new GameObject[display.maxSlots];
        display.slots[0] = display.slotTemplate;
        for (int i = 1; i < display.slots.Length; i++)
        {
            display.slots[i] = Instantiate(display.slotTemplate, display.transform);
            display.slots[i].name = display.slotTemplate.name;

            Undo.RegisterCreatedObjectUndo(display.slots[i], "Regenerate Icons");
        }

        EditorUtility.SetDirty(display);
    }
}
