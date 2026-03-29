using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;

[DisallowMultipleComponent]
[CustomEditor(typeof(UICharacterSelector))]
public class UICharacterSelectorEditor : Editor
{
    UICharacterSelector selector;

    void OnEnable()
    {
        // Odkaz na UICharacterSelector, keÔ je v inspectore, aby sme mohli pristupovaù k jeho premenn˝m.
        selector = target as UICharacterSelector;
    }

    public override void OnInspectorGUI()
    {
        // VytvorÌ tlaËidlo v inspectore, ktorÈ po kliknutÌ vytvorÌ öablÛny prepÌnaËov (toggles).
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Selectable Characters"))
        {
            CreateTogglesForCharacterData();
        }
    }

    public void CreateTogglesForCharacterData()
    {
        // Ak öablÛna prepÌnaËa nie je priraden·, vypÌö varovanie a preruö oper·ciu.
        if (!selector.toggleTemplate)
        {
            Debug.LogWarning("Please assign a Toggle Template for the UI Character Selector first.");
            return;
        }

        // Prejdi vöetky deti rodiËa öablÛny prepÌnaËa a vymaû vöetko okrem samotnej öablÛny.
        for (int i = selector.toggleTemplate.transform.parent.childCount - 1; i >= 0; i--)
        {
            Toggle tog = selector.toggleTemplate.transform.parent.GetChild(i).GetComponent<Toggle>();
            if (tog == selector.toggleTemplate) continue;
            Undo.DestroyObjectImmediate(tog.gameObject); // Zaznamenaj akciu pre moûnosù vr·tenia (Undo).
        }

        // Zaznamenaj zmeny vykonanÈ v komponente UICharacterSelector a vymaû zoznam prepÌnaËov.
        Undo.RecordObject(selector, "Updates to UICharacterSelector.");
        selector.selectableToggles.Clear();
        CharacterData[] characters = UICharacterSelector.GetAllCharacterDataAssets();

        // Pre kaûd˝ d·tov˝ asset postavy v projekte vytvorÌme prepÌnaË v selektore post·v.
        for (int i = 0; i < characters.Length; i++)
        {
            Toggle tog;
            if (i == 0)
            {
                tog = selector.toggleTemplate;
                Undo.RecordObject(tog, "Modifying the template.");
            }
            else
            {
                tog = Instantiate(selector.toggleTemplate, selector.toggleTemplate.transform.parent); // Vytvor kÛpiu öablÛny. (Toggle sucastneho charu ako dieta origo rodica templatu)
                Undo.RegisterCreatedObjectUndo(tog.gameObject, "Created a new toggle.");
            }

            // Hæadanie mena postavy, ikony a ikony zbrane na priradenie.
            Transform characterName = tog.transform.Find(selector.characterNamePath);
            if (characterName && characterName.TryGetComponent(out TextMeshProUGUI tmp))
            {
                tmp.text = tog.gameObject.name = characters[i].name;
            }

            Transform characterIcon = tog.transform.Find(selector.characterIconPath);
            if (characterIcon && characterIcon.TryGetComponent(out Image chrIcon))
            {
                chrIcon.sprite = characters[i].Icon;
            }

            Transform weaponIcon = tog.transform.Find(selector.weaponIconPath);
            if (weaponIcon && weaponIcon.TryGetComponent(out Image wpnIcon))
            {
                wpnIcon.sprite = characters[i].StartingWeapon.icon;
            }

            selector.selectableToggles.Add(tog);

            // Odstr·Ú vöetky existuj˙ce eventy a pridaj n·ö vlastn˝, ktor˝ kontroluje, na ktor˝ prepÌnaË sa kliklo.
            for (int j = tog.onValueChanged.GetPersistentEventCount() - 1; j >= 0; j--)
            {
                if (tog.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(tog.onValueChanged, j);
                }
            }

            UnityEventTools.AddObjectPersistentListener(tog.onValueChanged, selector.Select, characters[i]);
        }

        // Zaregistruje zmeny, aby sa po dokonËenÌ uloûili.
        EditorUtility.SetDirty(selector);
    }

}