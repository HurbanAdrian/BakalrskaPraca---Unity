using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Jednoduchı SaveManager urèenı na ukladanie celkového poètu mincí hráèa.
/// V neskorších èastiach bude slúi na ukladanie všetkıch hernıch dát.
/// </summary>
public class SaveManager
{
    [System.Serializable]
    public class GameData
    {
        public float coins;
    }

    const string SAVE_FILE_NAME = "SaveData.json";

    static GameData lastLoadedGameData;
    public static GameData LastLoadedGameData
    {
        get
        {
            if (lastLoadedGameData == null) Load();
            return lastLoadedGameData;
        }
    }

    public static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    public static void Save(GameData data = null)
    {
        if (data == null)
        {
            // Ak nemáme dáta v pamäti, najprv skúsime naèíta existujúci súbor.
            if (lastLoadedGameData == null) Load();
            data = lastLoadedGameData;
        }

        File.WriteAllText(GetSavePath(), JsonUtility.ToJson(data));
    }

    public static GameData Load(bool usePreviousLoadIfAvailable = false)
    {
        // usePreviousLoadIfAvailable slúi na zrıchlenie volania, aby sme nemuseli èíta disk zakadım, keï potrebujeme prístup k dátam.
        if (usePreviousLoadIfAvailable && lastLoadedGameData != null)
        {
            return lastLoadedGameData;
        }

        // Skúsime nájs súbor na pevnom disku.
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            lastLoadedGameData = JsonUtility.FromJson<GameData>(json);

            if (lastLoadedGameData == null) lastLoadedGameData = new GameData();
        }
        else
        {
            lastLoadedGameData = new GameData();
        }

        return lastLoadedGameData;
    }
}