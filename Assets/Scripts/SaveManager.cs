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
    // Šablóna pre kadú budovu
    [System.Serializable]
    public class BuildingSaveData
    {
        public int stat1Level;    // 1. Vylepšenie (napr. Might)
        public int stat2Level;    // 2. Vylepšenie (napr. Armor)
        public int stat3Level;    // 3. Vylepšenie (napr. Amount)
        public int discountLevel; // Vylepšenie z¾avy (Cost Reduction)

        // Pomocná funkcia: Zistí, èi je budova na max leveli
        public bool IsMaxed(int maxLevelPerStat = 4)
        {
            return stat1Level >= maxLevelPerStat &&
                   stat2Level >= maxLevelPerStat &&
                   stat3Level >= maxLevelPerStat &&
                   discountLevel >= maxLevelPerStat;
        }

        // 2. POISTKA: Metóda pre UI obchodu na bezpeènı nákup (vráti true ak sa podarilo)
        public bool TryUpgradeStat1(int maxLevel = 4)
        {
            if (stat1Level < maxLevel) { stat1Level++; return true; }
            return false;
        }
        public bool TryUpgradeStat2(int maxLevel = 4)
        {
            if (stat2Level < maxLevel) { stat2Level++; return true; }
            return false;
        }
        public bool TryUpgradeStat3(int maxLevel = 4)
        {
            if (stat3Level < maxLevel) { stat3Level++; return true; }
            return false;
        }
        public bool TryUpgradeDiscount(int maxLevel = 4)
        {
            if (discountLevel < maxLevel) { discountLevel++; return true; }
            return false;
        }

        // 3. POISTKA (Anti-cheat): Zree hodnoty, ak hráè v JSONe podvádzal
        public void ValidateLevels(int maxLevelPerStat = 4)
        {
            stat1Level = Mathf.Clamp(stat1Level, 0, maxLevelPerStat);
            stat2Level = Mathf.Clamp(stat2Level, 0, maxLevelPerStat);
            stat3Level = Mathf.Clamp(stat3Level, 0, maxLevelPerStat);
            discountLevel = Mathf.Clamp(discountLevel, 0, maxLevelPerStat);
        }
    }

    [System.Serializable]
    public class GameData
    {
        public float coins;

        public BuildingSaveData blacksmith = new BuildingSaveData();
        public BuildingSaveData tavern = new BuildingSaveData();
        public BuildingSaveData market = new BuildingSaveData();

        public void ValidateAllBuildings()
        {
            blacksmith.ValidateLevels();
            tavern.ValidateLevels();
            market.ValidateLevels();
        }
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

        lastLoadedGameData.ValidateAllBuildings();

        return lastLoadedGameData;
    }

    public static void ResetSave()
    {
        // 1. Zistíme, kde sa súbor nachádza a ak existuje, nemilosrdne ho zmaeme
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // 2. Vyèistíme dáta v pamäti (vytvoríme novú, èistú inštanciu GameData)
        lastLoadedGameData = new GameData();

        Debug.Log("Save súbor bol úspešne vymazanı a progres bol resetovanı!");
    }
}