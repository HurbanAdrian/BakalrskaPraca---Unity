using System.Text;
using System.Reflection;
using TMPro;
using UnityEngine;

public class UIStatsDisplay : UIPropertyDisplay
{
    public PlayerStats player; // Hr·Ë, ktorÈho ötatistiky tento displej vykresæuje.
    public CharacterData character; // PrÌpadne zobrazenie ötatistÌk z d·t postavy (v menu).
    public bool displayCurrentHealth = false;

    public override object GetReadObject()
    {
        // Vr·ti ötatistiky hr·Ëa v hre, alebo ötatistiky postavy v menu v˝beru.
        if (player) return player.Stats;
        else if (character) return character.stats;
        return new CharacterData.Stats();
    }

    public override void UpdateFields()
    {
        if (!player && !character) return;

        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            "CharacterData+Stats"
        );

        // ZÌskame referencie na Text objekty (prvÈ dve deti tohto objektu).
        if (!propertyNames) propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues) propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        // Ak chceme zobraziù aj aktu·lne zdravie (napr. v HUD poËas hry).
        if (displayCurrentHealth && player != null)
        {
            allStats[0].Insert(0, "Health\n");
            allStats[1].Insert(0, Mathf.CeilToInt(player.CurrentHealth) + "\n");
        }

        // Aktualizujeme textovÈ polia vygenerovan˝mi reùazcami.
        if (propertyNames) propertyNames.text = allStats[0].ToString();
        if (propertyValues) propertyValues.text = allStats[1].ToString();
        propertyValues.fontSize = propertyNames.fontSize;
    }

    void Reset()
    {
        player = FindAnyObjectByType<PlayerStats>();
    }
}