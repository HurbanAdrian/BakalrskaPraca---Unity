using UnityEngine;

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
        if (instance && instance.characterData)
            return instance.characterData;
        else
        {
            // Ak nie sú priradené žiadne dáta postavy, náhodne nejaké vyberieme.
            CharacterData[] characters = Resources.FindObjectsOfTypeAll<CharacterData>();
            if (characters.Length > 0)
            {
                return characters[Random.Range(0, characters.Length)];
            }
        }
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
