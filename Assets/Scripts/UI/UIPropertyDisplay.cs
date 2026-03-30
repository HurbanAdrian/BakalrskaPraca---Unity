using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

public abstract class UIPropertyDisplay : MonoBehaviour
{
    public bool updateInEditor = false;
    protected TextMeshProUGUI propertyNames, propertyValues;
    public const string DASH = "-";

    // Aktualizuje zobrazenie štatistík vdy, keï je objekt aktivovanı.
    protected virtual void OnEnable() { UpdateFields(); }
    protected virtual void OnDrawGizmosSelected() { if (updateInEditor) UpdateFields(); }

    // Keïe kadá trieda zobrazenia bude definova vlastné premenné pre objekty, ktoré èíta, kadá trieda musí prepísa túto funkciu, aby definovala èítanı objekt.
    public abstract object GetReadObject();

    // Urèuje, èi sa má dané pole spracova a zobrazi alebo nie.
    protected virtual bool IsFieldShown(FieldInfo field) { return true; }

    // Spracuje názov po¾a a pridá ho do StringBuildera.
    protected virtual StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if (!IsFieldShown(field)) return output;
        return output.AppendLine(name);
    }

    // Predvolene táto funkcia spracováva iba celé èísla a desatinné èísla (int a float).
    // Môeme ju prepísa, aby spracovávala aj iné typy, napríklad reazce (string).
    protected virtual StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        if (!IsFieldShown(field)) return output;

        float fval = value is int ? (int)value : value is float ? (float)value : 0;

        // Vytlaèí hodnotu ako percento, ak má priradenı atribút [Range] alebo [Min] a je to float.
        PropertyAttribute attribute = (PropertyAttribute)field.GetCustomAttribute<RangeAttribute>() ?? field.GetCustomAttribute<MinAttribute>();
        if (attribute != null && field.FieldType == typeof(float))
        {
            float percentage = Mathf.Round(fval * 100 - 100);

            // Ak je hodnota štatistiky 0 (resp. 100%), dáme len pomlèku.
            if (Mathf.Approximately(percentage, 0))
            {
                output.Append(DASH).Append('\n');
            }
            else
            {
                if (percentage > 0) output.Append('+');
                output.Append(percentage).Append('%').Append('\n');
            }
        }
        else
        {
            output.Append(value).Append('\n');
        }

        return output;
    }

    // Vráti pole 2 StringBuilderov, ktoré sa pouijú na naplnenie 2 textovıch polí v UI.
    protected virtual StringBuilder[] GetProperties(BindingFlags flags, string targetedType)
    {
        // Pouívame StringBuilder, aby manipulácia s textom beala rıchlejšie.
        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        FieldInfo[] fields = System.Type.GetType(targetedType).GetFields(flags);
        foreach (FieldInfo field in fields)
        {
            // Spracujeme názov štatistiky.
            ProcessName(field.Name, names, field);
            ProcessValue(field.GetValue(GetReadObject()), values, field);
        }

        // Vráti polia s vyèistenımi názvami a hodnotami.
        return new StringBuilder[2] { PrettifyNames(names), values };
    }

    // Abstraktná metóda, ktorú implementujú dcérske triedy na vykreslenie textu do UI.
    public abstract void UpdateFields();

    // Statická funkcia na skrášlenie názvov (pridanie medzier pred ve¾ké písmená).
    public static StringBuilder PrettifyNames(StringBuilder input)
    {
        if (input.Length <= 0) return null;

        StringBuilder result = new StringBuilder();
        char last = '\0';
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // Skontroluje, kedy zmeni na ve¾ké písmeno alebo prida medzeru.
            if (last == '\0' || char.IsWhiteSpace(last))
            {
                c = char.ToUpper(c);
            }
            else if (char.IsUpper(c))
            {
                result.Append(' '); // Vloí medzeru pred ve¾ké písmeno.
            }
            result.Append(c);
            last = c;
        }
        return result;
    }
}