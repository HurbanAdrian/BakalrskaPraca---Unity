using System.Text;
using System.Reflection;
using UnityEngine;
using TMPro;

public class UIStatsDisplay : MonoBehaviour
{
    public PlayerStats player; // Hr·Ë, ktorÈho ötatistiky toto rozhranie vykresæuje.
    public CharacterData character;
    public bool displayCurrentHealth = false;
    public bool updateInEditor = false;
    TextMeshProUGUI statNames, statValues;

    // Aktualizuj zobrazenie ötatistÌk vûdy, keÔ je objekt nastaven˝ ako aktÌvny.
    void OnEnable()
    {
        UpdateStatFields();
    }

    private void OnDrawGizmosSelected()
    {
        if (updateInEditor)
        {
            UpdateStatFields();
        }
    }

    public CharacterData.Stats GetDisplayedStats()
    {
        // Vr·ti ötatistiky hr·Ëa v hernej scÈne. V scÈne v˝beru postavy vr·ti ötatistiky postavy, pretoûe tam nie je priraden· premenn· 'player'.
        if (player) return player.Stats;
        else if (character) return character.stats;

        return new CharacterData.Stats();
    }

    public void UpdateStatFields()
    {
        if (!player && !character) return;

        // ZÌskaj referenciu na oba textovÈ objekty pre vykreslenie n·zvov a hodnÙt ötatistÌk.
        if (!statNames) statNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!statValues) statValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        // Vykresli vöetky n·zvy a hodnoty ötatistÌk. PouûÌvame StringBuilder, aby bola manipul·cia s textom r˝chlejöia.
        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        // Pridaj aktu·lne zdravie do okna ötatistÌk.
        if (displayCurrentHealth && player != null)
        {
            names.AppendLine("Health");
            values.AppendLine(player.CurrentHealth.ToString());
        }

        FieldInfo[] fields = typeof(CharacterData.Stats).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Vykresli n·zvy ötatistÌk.
            names.AppendLine(field.Name);

            // ZÌskaj hodnotu ötatistiky.
            object val = field.GetValue(GetDisplayedStats());
            float fval = val is int ? (int)val : (float)val;

            // VypÌö to ako percento, ak m· priraden˝ atrib˙t a je to typ float.
            PropertyAttribute attribute = (PropertyAttribute)PropertyAttribute.GetCustomAttribute(field, typeof(PropertyAttribute));
            if (attribute != null && field.FieldType == typeof(float))
            {
                float percentage = Mathf.Round(fval * 100 - 100);

                // Ak je hodnota ötatistiky 0 (ûiadny bonus), vypÌö len pomlËku.
                if (Mathf.Approximately(percentage, 0))
                {
                    values.Append('-').Append('\n');
                }
                else
                {
                    // Ak je bonus kladn˝, pridaj znamienko plus.
                    if (percentage > 0)
                        values.Append('+');

                    values.Append(percentage).Append('%').Append('\n');
                }
            }
            else
            {
                values.Append(fval).Append('\n');
            }

        }
        // Aktualizuj textovÈ polia v UI pomocou vytvoren˝ch reùazcov.
        statNames.text = PrettifyNames(names);
        statValues.text = values.ToString();
    }

    public static string PrettifyNames(StringBuilder input)
    {
        // Ak je StringBuilder pr·zdny, vr·ù pr·zdny reùazec.
        if (input.Length <= 0) return string.Empty;

        StringBuilder result = new StringBuilder();
        char last = '\0';
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // Skontroluj, kedy zmeniù pÌsmeno na veækÈ alebo pridaù medzeru.
            if (last == '\0' || char.IsWhiteSpace(last))
            {
                // Ak je to prvÈ pÌsmeno alebo nasleduje po medzere, zmeÚ ho na veækÈ.
                c = char.ToUpper(c);
            }
            else if (char.IsUpper(c))
            {
                // Vloû medzeru pred veækÈ pÌsmeno (rozdelenie CamelCase).
                result.Append(' ');
            }

            result.Append(c);

            last = c;
        }

        return result.ToString();
    }

    private void Reset()
    {
        player = FindAnyObjectByType<PlayerStats>();
    }
}
