using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector instance;
    public CharacterData characterData;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public static CharacterData GetData()
    {
        // Ak prich·dzame z menu a m·me postavu, vr·ù ju
        if (instance != null && instance.characterData != null)
        {
            return instance.characterData;
        }
        
        // Ak sme hru zapli priamo v mape (Editor), n·jdi n·hodn˙ postavu
        #if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        List<CharacterData> characters = new List<CharacterData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData cd = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (cd != null)
            {
                characters.Add(cd);
            }
        }

        if (characters.Count > 0)
        {
            int randomIndex = Random.Range(0, characters.Count);
            Debug.Log("<color=green>SpustenÈ z Editora: N·hodne vybran· postava -> " + characters[randomIndex].name + "</color>");
            return characters[randomIndex];
        }
        else
        {
            Debug.LogError("POZOR: V celom projekte sa nenaöiel ani jeden CharacterData asset!");
        }
        #endif

        return null;
    }

    public void SelectCharacter(CharacterData character)
    {
        characterData = character;
    }

    public void DestroySingleton()
    {
        instance = null;
        Destroy(gameObject);
    }
}
