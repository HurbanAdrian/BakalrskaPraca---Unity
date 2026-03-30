using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;
using System.Linq;
using System.Text.RegularExpressions;

[DisallowMultipleComponent]
[CustomEditor(typeof(UILevelSelector))]
public class UILevelSelectorEditor : Editor
{
    UILevelSelector selector;

    void OnEnable()
    {
        // Odkaz na UILevelSelector v inspectore, aby sme mohli pristupovaù k jeho premenn˝m.
        selector = target as UILevelSelector;
    }

    public override void OnInspectorGUI()
    {
        // VytvorÌ tlaËidlo v inspectore, ktorÈ vytvorÌ ötrukt˙ry/öablÛny levelov.
        base.OnInspectorGUI();

        // Ak nie je nastaven· öablÛna prepÌnaËa, zobrazÌme varovanie.
        if (!selector.toggleTemplate)
        {
            EditorGUILayout.HelpBox(
                "You need to assign a Toggle Template for the button below to work properly.",
                MessageType.Warning
            );
        }

        if (GUILayout.Button("Find and Populate Levels"))
        {
            PopulateLevelsList();
            CreateLevelSelectToggles();
        }
    }

    // Funkcia, ktor· n·jde vöetky s˙bory scÈn v projekte a priradÌ ich do zoznamu levelov.
    public void PopulateLevelsList()
    {
        // Zaznamen· zmeny ako undo-ovateænÈ a vymaûe ch˝baj˙ce scÈny.
        Undo.RecordObject(selector, "Create New SceneData structs");
        SceneAsset[] maps = UILevelSelector.GetAllMaps();

        // Odstr·ni zo zoznamu scÈny, ktorÈ uû neexistuj˙ (null).
        selector.levels.RemoveAll(level => level.scene == null);

        foreach (SceneAsset map in maps)
        {
            // Ak scÈna eöte nie je v zozname, prid·me ju (vyhneme sa prepÌsaniu nastavenÌ pouûÌvateæa).
            if (!selector.levels.Any(sceneData => sceneData.scene == map))
            {
                // Extrahuje inform·cie z n·zvu mapy pomocou regexu.
                Match m = Regex.Match(map.name, UILevelSelector.MAP_NAME_FORMAT, RegexOptions.IgnoreCase);
                string mapLabel = "Level", mapName = "New Map";

                if (m.Success)
                {
                    if (m.Groups.Count > 1) mapLabel = m.Groups[1].Value;
                    if (m.Groups.Count > 2) mapName = m.Groups[2].Value;
                }

                // VytvorÌ nov˝ objekt SceneData s predvolen˝mi hodnotami a prid· ho do zoznamu.
                selector.levels.Add(new UILevelSelector.SceneData
                {
                    scene = map,
                    label = mapLabel,
                    displayName = mapName
                });
            }
        }
    }

    // S priradenou öablÛnou vytvorÌ UI prepÌnaËe (toggles), ktorÈ sa pouûÌvaj˙ v hre.
    public void CreateLevelSelectToggles()
    {
        if (!selector.toggleTemplate)
        {
            Debug.LogWarning("Failed to create the Toggles for selecting levels. Please assign the Toggle Template.");
            return;
        }

        // Vymaûe vöetky starÈ objekty pod rodiËom öablÛny okrem öablÛny samotnej.
        for (int i = selector.toggleTemplate.transform.parent.childCount - 1; i >= 0; i--)
        {
            Toggle tog = selector.toggleTemplate.transform.parent.GetChild(i).GetComponent<Toggle>();
            if (tog == selector.toggleTemplate) continue;
            Undo.DestroyObjectImmediate(tog.gameObject);
        }

        // Zaznamen· zmeny v komponente a vyËistÌ zoznam prepÌnaËov.
        Undo.RecordObject(selector, "Updates to UILevelSelector.");
        selector.selectableToggles.Clear();

        // Pre kaûd˝ level v zozname vytvorÌme UI prepÌnaË.
        for (int i = 0; i < selector.levels.Count; i++)
        {
            Toggle tog;
            if (i == 0)
            {
                tog = selector.toggleTemplate;
                Undo.RecordObject(tog, "Modifying the template.");
            }
            else
            {
                tog = Instantiate(selector.toggleTemplate, selector.toggleTemplate.transform.parent);
                Undo.RegisterCreatedObjectUndo(tog.gameObject, "Created a new toggle.");
            }

            tog.gameObject.name = selector.levels[i].scene.name;

            // Hæadanie mena, ËÌsla, popisu a obr·zka levelu v hierarchii prepÌnaËa.
            Transform levelName = tog.transform.Find(selector.LevelImagePath).Find("Name Holder").Find(selector.LevelNamePath);
            if (levelName && levelName.TryGetComponent(out TextMeshProUGUI lvlName))
            {
                lvlName.text = selector.levels[i].displayName;
            }

            Transform levelNumber = tog.transform.Find(selector.LevelImagePath).Find(selector.LevelNumberPath);
            if (levelNumber && levelNumber.TryGetComponent(out TextMeshProUGUI lvlNumber))
            {
                lvlNumber.text = selector.levels[i].label;
            }

            Transform levelDescription = tog.transform.Find(selector.LevelDescriptionPath);
            if (levelDescription && levelDescription.TryGetComponent(out TextMeshProUGUI lvlDescription))
            {
                lvlDescription.text = selector.levels[i].description;
            }

            Transform levelImage = tog.transform.Find(selector.LevelImagePath);
            if (levelImage && levelImage.TryGetComponent(out Image lvlImage))
            {
                lvlImage.sprite = selector.levels[i].icon;
            }

            selector.selectableToggles.Add(tog);

            // Nastavenie eventu OnValueChanged na zavolanie metÛdy Select s indexom levelu.
            for (int j = tog.onValueChanged.GetPersistentEventCount() - 1; j >= 0; j--)
            {
                if (tog.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(tog.onValueChanged, j);
                }
            }
            UnityEventTools.AddIntPersistentListener(tog.onValueChanged, selector.Select, i);
        }

        // OznaËiù objekt ako "öpinav˝", aby Unity uloûilo zmeny.
        EditorUtility.SetDirty(selector);
    }
}