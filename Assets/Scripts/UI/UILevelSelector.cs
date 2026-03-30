using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Reflection;

public class UILevelSelector : MonoBehaviour
{
    public UISceneDataDisplay statsUI;

    public static int selectedLevel = -1;
    public static SceneData currentLevel;
    public List<SceneData> levels = new List<SceneData>();

    [Header("Template")]
    public Toggle toggleTemplate;
    public string LevelNamePath = "Level Name";
    public string LevelNumberPath = "Level Number";
    public string LevelDescriptionPath = "Level Description";
    public string LevelImagePath = "Level Image";
    public List<Toggle> selectableToggles = new List<Toggle>();

    // Modifikátory úrovne sa aplikujú na hráèov a nepriate¾ov pomocou buffu.
    // Dáta buffu sú uložené v tejto statickej premennej. (greed & growth)
    public static BuffData globalBuff;

    /* 
     * Vždy, keï sa aplikuje globalBuff, skontrolujeme, èi má nejaký úèinok 
     * na hráèa alebo nepriate¾ov a zaznamenáme to sem. Ak nemá, buff neaplikujeme,
     * aby sme ušetrili výpoètový výkon. 
    */
    public static bool globalBuffAffectsPlayer = false, globalBuffAffectsEnemies = false;

    // Regex, ktorý sa používa na identifikáciu, ktoré mapy sú levely (formát: Level 1 - Názov).
    public const string MAP_NAME_FORMAT = "^(Level .*?) ?- ?(.*)$";

    [System.Serializable]
    public class SceneData
    {
#if UNITY_EDITOR
        public SceneAsset scene;
#endif
        [HideInInspector]
        public string sceneName;        // Sem sa automaticky uloží meno scény pre Build

        [Header("UI Display")]
        public string displayName;
        public string label;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Modifiers")]
        public CharacterData.Stats playerModifier;
        public EnemyStats.Stats enemyModifier;
        [Min(-1)] public float timeLimit = 0f, clockSpeed = 1f;
        [TextArea] public string extraNotes = "--";
    }

    public static SceneAsset[] GetAllMaps()
    {
        List<SceneAsset> maps = new List<SceneAsset>();

        // Naplníme zoznam všetkými scénami zaèínajúcimi na "Level -" (iba v Editori).
#if UNITY_EDITOR
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".unity"))
            {
                SceneAsset map = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                if (map != null && Regex.IsMatch(map.name, MAP_NAME_FORMAT))
                {
                    maps.Add(map);
                }
            }
        }
#else
        Debug.LogWarning("Táto funkcia nemôže by volaná v buildoch.");
#endif
        maps.Reverse();
        return maps.ToArray();
    }

    // Pre bežné zmeny scén (napr. návrat do menu).
    public void SceneChange(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1;
    }

    // Na naèítanie levelu z obrazovky výberu úrovní.
    public void LoadSelectedLevel()
    {
        if (selectedLevel >= 0)
        {
            SceneManager.LoadScene(levels[selectedLevel].sceneName);
            currentLevel = levels[selectedLevel];
            selectedLevel = -1;
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("Nebol vybratý žiadny level!");
        }
    }

    /* Vyberie scénu, ktorá bude naèítaná pomocou LoadSelectedLevel().
       Zároveò vytvorí buff, ktorý bude aplikovaný v danom leveli a skontroluje,
       èi sú premenné modifikátorov prázdne. */
    public void Select(int sceneIndex)
    {
        selectedLevel = sceneIndex;
        statsUI.UpdateFields();
        globalBuff = GenerateGlobalBuffData();

        // Kontrola, èi buff reálne nieèo mení, aby sme ho neaplikovali zbytoène.
        globalBuffAffectsPlayer = globalBuff && !IsModifierEmpty(globalBuff.variations[0].playerModifier);
        globalBuffAffectsEnemies = globalBuff && !IsModifierEmpty(globalBuff.variations[0].enemyModifier);
    }

    // Vygeneruje objekt BuffData, ktorý obalí premenné playerModifier a enemyModifier.
    public BuffData GenerateGlobalBuffData()
    {
        BuffData bd = ScriptableObject.CreateInstance<BuffData>();
        bd.name = "Global Level Buff";

        // Inicializujeme variáciu buffu (predpokladáme, že BuffData má pole variations).
        bd.variations[0].damagePerSecond = 0;
        bd.variations[0].duration = 0;
        bd.variations[0].playerModifier = levels[selectedLevel].playerModifier;
        bd.variations[0].enemyModifier = levels[selectedLevel].enemyModifier;

        return bd;
    }

    // Používa sa na kontrolu, èi je playerModifier alebo enemyModifier globálneho buffu prázdny.
    private static bool IsModifierEmpty(object obj)
    {
        if (obj == null) return true;

        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields();
        float sum = 0;

        foreach (FieldInfo f in fields)
        {
            object val = f.GetValue(obj);
            if (val is int) sum += Math.Abs((int)val);
            else if (val is float) sum += Math.Abs((float)val);
        }

        // Ak je súèet všetkých hodnôt (v absolútnej hodnote) približne 0, modifikátor je "prázdny".
        return Mathf.Approximately(sum, 0);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automaticky prekopíruje názov scény zo SceneAssetu do stringu
        foreach (SceneData level in levels)
        {
            if (level.scene != null)
            {
                level.sceneName = level.scene.name;
            }
        }
    }
#endif
}