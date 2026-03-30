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
        // Ak je priradená predvolená postava, vyber ju hneï pri naèítaní scény.
        if (defaultCharacter) Select(defaultCharacter);
    }

    public static CharacterData[] GetAllCharacterDataAssets()
    {
        List<CharacterData> characters = new List<CharacterData>();

        // Naplníme zoznam všetkými assetmi typu CharacterData (iba v Editori).
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
    Debug.LogWarning("Táto funkcia nemôže by volaná v buildoch (iba v editore).");
#endif

        return characters.ToArray();
    }

    public static CharacterData GetData()
    {
        // Použije vybranú postavu zvolenú vo funkcii Select().
        if (selected)
            return selected;
        else
        {
            // Ak hráme priamo z Editora a žiadna postava nie je vybraná, vyberie jednu náhodne.
            CharacterData[] characters = GetAllCharacterDataAssets();
            if (characters.Length > 0)
                return characters[Random.Range(0, characters.Length)];
        }
        return null;
    }

    public void Select(CharacterData character)
    {
        // Aktualizácia polí so štatistikami na obrazovke výberu postavy.
        selected = statsUI.character = character;
        statsUI.UpdateFields();

        // Aktualizácia obsahu boxu s popisom postavy.
        characterFullName.text = character.FullName;
        characterDescription.text = character.CharacterDescription;
        selectedCharacterIcon.sprite = character.Icon;
        selectedCharacterWeapon.sprite = character.StartingWeapon.icon;
    }
}
