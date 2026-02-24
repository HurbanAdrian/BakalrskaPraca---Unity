using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Zadefinovanie stavov hry
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver
    }

    // Uloženie aktuálneho a predchádzajúceho stavu hry
    public GameState currentState;
    public GameState previousState;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultScreen;

    [Header("Current Stat Displays")]
    // Sucasne stat displeje
    public Text currentHealthDisplay;
    public Text currentRecoveryDisplay;
    public Text currentSpeedDisplay;
    public Text currentMightDisplay;
    public Text currentProjectileSpeedDisplay;
    public Text currentMagnetDisplay;

    [Header("Results Screen Displays")]
    public Image chosenCharacterImage;
    public Text chosenCharacterName;
    public Text levelReachedDisplay;
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    public bool isGameOver = false;

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
        // Zadefinovanie spravania pre jednotlivé stavy hry
        switch (currentState)
        {
            case GameState.Gameplay:
                // Kod pre gameplay stav
                CheckForPauseAndResume();
                break;
            case GameState.Paused:
                // Kod pre pozastavenu hru
                CheckForPauseAndResume();
                break;
            case GameState.GameOver:
                // Kod pre stav konca hry
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0; // Zastaví èas v hre

                    Debug.Log("GAME OVER");
                    DisplayResults();
                }
                break;
            default:
                Debug.LogError("Neznámy stav hry: " + currentState);
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
            ChangeState(GameState.Paused);
            Time.timeScale = 0; // Zastaví èas v hre
            pauseScreen.SetActive(true); // Zobrazí pozastavenú obrazovku
            Debug.Log("Hra pozastavena");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1; // Obnoví èas v hre
            pauseScreen.SetActive(false);
            Debug.Log("Hra obnovena");
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
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    void DisplayResults()
    {
        resultScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterScriptableObject chosenCharacterData)
    {
        chosenCharacterImage.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.CharacterName;
    }

    public void AssignLevelReachedUI(int levelReached)
    {
        levelReachedDisplay.text = levelReached.ToString();
    }

    public void AssignChosenWeaponsAndPassiveItemsUI(List<Image> chosenWeaponsData, List<Image> chosenItemsData)
    {
        if (chosenWeaponsData.Count != chosenWeaponsUI.Count || chosenItemsData.Count != chosenPassiveItemsUI.Count)
        {
            Debug.LogError("Listy pre Zbrane a Itemy maju roznu dlzku");
            return;
        }

        // Pridelenie zvolenych dat zbrani
        for (int i = 0; i < chosenWeaponsData.Count; i++)
        {
            // Kontrola ci zbrane maju sprite a ak ano tak ich zobrazit v UI, ak nie tak skryt ten slot
            if (chosenWeaponsData[i].sprite)
            {
                chosenWeaponsUI[i].enabled = true;
                chosenWeaponsUI[i].sprite = chosenWeaponsData[i].sprite;
            }
            else
            {
                chosenWeaponsUI[i].enabled = false;
            }
        }

        // Pridelenie zvolenych dat itemov
        for (int i = 0; i < chosenItemsData.Count; i++)
        {
            if (chosenItemsData[i].sprite)
            {
                chosenPassiveItemsUI[i].enabled = true;
                chosenPassiveItemsUI[i].sprite = chosenItemsData[i].sprite;
            }
            else
            {
                chosenPassiveItemsUI[i].enabled = false;
            }
        }
    }

}
