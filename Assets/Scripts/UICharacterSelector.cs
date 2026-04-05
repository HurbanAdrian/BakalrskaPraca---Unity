using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UICharacterSelector : MonoBehaviour
{
    public CharacterData defaultCharacter;
    public static CharacterData selected;
    public UIStatsDisplay statsUI;

    [Header("Template")]
    public Toggle toggleTemplate;
    public string characterNamePath = "Character Name";
    public string weaponIconPath = "Weapon Icon";
    public string characterIconPath = "Character Icon";
    public List<Toggle> selectableToggles = new List<Toggle>();

    [Header("DescriptionBox")]
    public TextMeshProUGUI characterFullName;
    public TextMeshProUGUI characterDescription;
    public Image selectedCharacterIcon;
    public Image selectedCharacterWeapon;

    void Start()
    {
        // Ak je priraden· predvolen· postava, vyber ju hneÔ pri naËÌtanÌ scÈny.
        if (defaultCharacter) Select(defaultCharacter);
    }

    public static CharacterData[] GetAllCharacterDataAssets()
    {
        List<CharacterData> characters = new List<CharacterData>();

        // NaplnÌme zoznam vöetk˝mi assetmi typu CharacterData (iba v Editori).
#if UNITY_EDITOR
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".asset"))
            {
                CharacterData characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if (characterData != null)
                {
                    characters.Add(characterData);
                }
            }
        }
#else
    Debug.LogWarning("T·to funkcia nemÙûe byù volan· v buildoch (iba v editore).");
#endif

        return characters.ToArray();
    }

    public static CharacterData GetData()
    {
        // 1. Ak prich·dzame z Menu a postava je vybran·, pouûi ju
        if (selected)
        {
            return selected;
        }

        // 2. Ak sme scÈnu spustili na priamo, hæad·me n·ö bezpeËn˝ DevSettings skript
        DevSettings devSettings = FindFirstObjectByType<DevSettings>();
        if (devSettings != null && devSettings.debugCharacter != null)
        {
            Debug.Log("DEV TESTING: NaËÌtavam testovaciu postavu: " + devSettings.debugCharacter.name);
            return devSettings.debugCharacter;
        }

        // 3. Ak hr·me priamo z Editora a ûiadna postava nie je v DevSettings vybran·, hodÌ n·hodn˙
        Debug.Log("DEV TESTING: éiadna postava nebola zvolen·, h·dûem n·hodn˙.");
        CharacterData[] characters = GetAllCharacterDataAssets();
        if (characters.Length > 0)
        {
            return characters[Random.Range(0, characters.Length)];
        }

        return null;
    }

    public void Select(CharacterData character)
    {
        // Aktualiz·cia polÌ so ötatistikami na obrazovke v˝beru postavy.
        selected = statsUI.character = character;
        statsUI.UpdateFields();

        // Aktualiz·cia obsahu boxu s popisom postavy.
        characterFullName.text = character.FullName;
        characterDescription.text = character.CharacterDescription;
        selectedCharacterIcon.sprite = character.Icon;
        selectedCharacterWeapon.sprite = character.StartingWeapon.icon;
    }
}
