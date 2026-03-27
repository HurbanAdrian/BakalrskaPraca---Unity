using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Zadefinovanie stavov hry
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        LevelUp
    }

    // Ulo�enie aktu�lneho a predch�dzaj�ceho stavu hry
    public GameState currentState;
    public GameState previousState;

    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 20;
    public TMP_FontAsset textFont;
    public Camera referenceCamera;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultScreen;
    public GameObject levelUpScreen;
    int stackedLevelUps = 0;

    [Header("Current Stat Displays")]
    // Sucasne stat displeje
    public TMP_Text currentHealthDisplay;
    public TMP_Text currentRecoveryDisplay;
    public TMP_Text currentSpeedDisplay;
    public TMP_Text currentMightDisplay;
    public TMP_Text currentProjectileSpeedDisplay;
    public TMP_Text currentMagnetDisplay;

    [Header("Results Screen Displays")]
    public Image chosenCharacterImage;
    public TMP_Text chosenCharacterName;
    public TMP_Text levelReachedDisplay;
    public TMP_Text timeSurvivedDisplay;

    [Header("Stopwatch")]
    public float timeLimit; // casovy limit pre hru v sekundach
    float stopwatchTime;    // cas, ktory uplynul od zaciatku stopwatchu
    public TMP_Text stopwatchDisplay; // UI text pro zobrazenie casu

    // Referencia na hraca objekt
    public GameObject playerObject;

    public bool isGameOver { get { return currentState == GameState.GameOver; } }
    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

    void Awake()
    {
        // Singleton pattern pre GameManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("GameManager instance already exists. Destroying duplicate.");
            Destroy(gameObject);
        }

        DisableScreens();
    }

    void Update()
    {
        // Zadefinovanie spravania pre jednotliv� stavy hry
        switch (currentState)
        {
            case GameState.Gameplay:
                // Kod pre gameplay stav
                CheckForPauseAndResume();
                UpdateStopwatch();          // aby bezali len ked je hra v gameplay stave
                break;
            case GameState.Paused:
                // Kod pre pozastavenu hru
                CheckForPauseAndResume();
                break;
            case GameState.GameOver:
            case GameState.LevelUp:
                break;
            default:
                Debug.LogError("Nezn�my stav hry: " + currentState);
                break;
        }
    }

    IEnumerator GenerateFloatingTextCorutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        // Zacatie generovania plavajuceho textu
        GameObject textObj = new GameObject("DamageFloatingText");
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI textMeshPro = textObj.AddComponent<TextMeshProUGUI>();
        textMeshPro.text = text;
        textMeshPro.fontSize = textFontSize;
        textMeshPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMeshPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        if (textFont)
        {
            textMeshPro.font = textFont;
        }

        // Znicit ked uplynie cas
        Destroy(textObj, duration);

        // Parentnut text k canvasu
        textObj.transform.SetParent(instance.damageTextCanvas.transform);
        textObj.transform.SetSiblingIndex(0);
        textObj.transform.localScale = Vector3.one;             // pre istotu keby Unity chcel menit scale pri parentovani

        // Neukladáme si pozíciu na obrazovke, ale pozíciu vo SVETE.
        // Uložíme si ju do premennej, aby sme nezáviseli od targetu (ak zomrie).
        Vector3 worldPosition;

        // Pozíciu zistíme LEN RAZ na začiatku.
        // Ak target existuje, zoberieme jeho pozíciu. Ak nie, dáme to do stredu.
        if (target != null)
        {
            worldPosition = target.position;
            // Pridáme malý náhodný rozptyl, aby sa texty neprekrývali, ak dáš veľa dmg naraz
            worldPosition += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.3f, 0.3f), 0);
        }
        else
        {
            // Fallback: ak nemáme target, musíme si vymyslieť bod pred kamerou
            worldPosition = referenceCamera.transform.position + referenceCamera.transform.forward * 10f;
        }

        // Animacia plavajuceho textu dohora a postupne zmiznutie
        float timeElapsed = 0f;
        float currentYOffset = 0f;      // Tu si budeme pamätať, o koľko už vyletel hore

        while (timeElapsed < duration)
        {
            // Keby nepriatel zomrel skor akoby text zmizol
            if (textObj == null || rect == null)
            {
                yield break;
            }

            // Zmiznutie textu postupne cez alpha kanal
            textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, 1 - timeElapsed / duration);

            // POHYB:
            // a) Zväčšíme offset (o koľko má text vyletieť hore v pixeloch/jednotkách)
            currentYOffset += speed * Time.deltaTime;

            // b) PREPOČET POZÍCIE (Toto opraví tvoj problém s kamerou)
            // Zoberieme pôvodnú 3D pozíciu -> prevedieme na aktuálnu 2D pozíciu kamery -> pripočítame offset hore
            rect.position = referenceCamera.WorldToScreenPoint(worldPosition) + new Vector3(0, currentYOffset, 0);

            // Pockaj na frame a aktualizuj cas
            yield return null;
            timeElapsed += Time.deltaTime;
        }

    }

    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        //Kontrola ci mame nachystany canvas pre damage texty
        if (!instance.damageTextCanvas)
        {
            return;
        }

        // Najdi relevantnu kameru pre konverziu svetovych suradnic na obrazovkove
        if (!instance.referenceCamera)
        {
            instance.referenceCamera = Camera.main;
        }

        instance.StartCoroutine(
            instance.GenerateFloatingTextCorutine(text, target, duration, speed)
            );
    }

    public void ChangeState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0; // Zastav� �as v hre
            pauseScreen.SetActive(true); // Zobraz� pozastaven� obrazovku
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1; // Obnov� �as v hre
            pauseScreen.SetActive(false);
        }
    }

    // Zadefinovanie metody pre kontrolu vsstupu pre pozastavenie a obnovenie hry
    void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void DisableScreens()
    {
        pauseScreen.SetActive(false);
        resultScreen.SetActive(false);
        levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        timeSurvivedDisplay.text = stopwatchDisplay.text;

        ChangeState(GameState.GameOver);
        Time.timeScale = 0f;
        DisplayResults();
    }

    void DisplayResults()
    {
        resultScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacterData)
    {
        chosenCharacterImage.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.Name;
    }

    public void AssignLevelReachedUI(int levelReached)
    {
        levelReachedDisplay.text = levelReached.ToString();
    }

    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime; // P�id� �as, kter� uplynul od posledn�ho sn�mku

        UpdateStopwatchDisplay();

        if (stopwatchTime >= timeLimit)
        {
            playerObject.SendMessage("Kill");        // Zavol� metodu Die na objektu hr��e
        }
    }

    void UpdateStopwatchDisplay()
    {
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60);

        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartLevelUp()
    {
        ChangeState(GameState.LevelUp);

        if (levelUpScreen.activeSelf) stackedLevelUps++;
        else 
        {
            Time.timeScale = 0f;
            levelUpScreen.SetActive(true);
            playerObject.SendMessage("RemoveAndApplyUpgrades");
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1; // Obnov� �as v hre
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if (stackedLevelUps > 0)
        {
            stackedLevelUps--;
            StartLevelUp();
        }
    }

}
