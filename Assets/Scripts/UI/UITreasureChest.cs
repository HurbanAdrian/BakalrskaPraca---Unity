using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITreasureChest : MonoBehaviour
{
    public static UITreasureChest instance; // Singleton pre jednoduch˝ prÌstup odkiaækoævek
    PlayerCollector collector;
    TreasureChest currentChest;
    TreasureChestDropProfile dropProfile;

    [Header("Visual Elements")]
    public GameObject openingVFX;
    public GameObject beamVFX;
    public GameObject fireworks;
    public GameObject doneButton;
    public GameObject curvedBeams;
    public List<ItemDisplays> items;
    Color originalColor = new Color32(0x42, 0x41, 0x87, 255);

    [Header("UI Elements")]
    public GameObject chestCover;
    public GameObject chestButton;

    [Header("UI Components")]
    public Image chestPanel;
    public TextMeshProUGUI coinText;
    private float coins;

    // Vn˙tornÈ stavy
    private List<Sprite> icons = new List<Sprite>();
    private bool isAnimating = false;
    private Coroutine chestSequenceCoroutine;

    // Audio
    private AudioSource audiosource;
    public AudioClip pickUpSound;

    // ätrukt˙ra pre zobrazenie jednotliv˝ch predmetov (zbranÌ/itemov) v l˙Ëoch
    [System.Serializable]
    public struct ItemDisplays
    {
        public GameObject beam;
        public Image spriteImage;
        public GameObject sprite;
        public GameObject weaponBeam;
    }

    private void Awake()
    {
        audiosource = GetComponent<AudioSource>();
        gameObject.SetActive(false);

        // ZabezpeËÌme, aby na scÈne existovala iba jedna inötancia tohto skriptu.
        if (instance != null && instance != this)
        {
            Debug.LogWarning("More than 1 UI Treasure Chest is found. It has been deleted.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void Activate(PlayerCollector collector, TreasureChest chest)
    {
        if (!instance) Debug.LogWarning("No treasure chest UI GameObject found.");

        // UloûÌme dÙleûitÈ premennÈ z truhlice do n·öho UI systÈmu.
        instance.collector = collector;
        instance.currentChest = chest;
        instance.dropProfile = chest.GetCurrentDropProfile();
        Debug.Log(instance.dropProfile);

        // Aktivujeme hern˝ objekt UI panela.
        GameManager.instance.ChangeState(GameManager.GameState.TreasureChest);

        Time.timeScale = 0f;

        instance.gameObject.SetActive(true);
    }

    // ZobrazÌ ikony vöetk˝ch predmetov zÌskan˝ch z truhlice s pokladom.
    public static void NotifyItemReceived(Sprite icon)
    {
        // Obsahuje varovn˙ spr·vu informuj˙cu pouûÌvateæa o problÈme, ak nie sme schopnÌ aktualizovaù t˙to triedu danou ikonou.
        if (instance)
        {
            instance.icons.Add(icon);
        }
        else
        {
            Debug.LogWarning("No instance of UITreasureChest exists. Unable to update treasure chest UI.");
        }
    }

    // Logika pre blikanie panelu truhlice
    private IEnumerator FlashWhite(Image image, int times, float flashDuration = 0.2f)
    {
        originalColor = image.color;

        // Zablik· panelom truhlice x-kr·t
        for (int i = 0; i < times; i++)
        {
            image.color = Color.white;
            yield return new WaitForSecondsRealtime(flashDuration);

            image.color = originalColor;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    IEnumerator ActivateCurvedBeams(float spawnTime)
    {
        yield return new WaitForSecondsRealtime(spawnTime);
        curvedBeams.SetActive(true);
    }

    // Odovzdanie mincÌ hr·Ëovi a ich animovanÈ zobrazenie
    IEnumerator HandleCoinDisplay(float maxCoins)
    {
        coinText.gameObject.SetActive(true);
        float elapsedTime = 0;
        coins = maxCoins;

        // Anim·cia pripoËÌtavania mincÌ, ktor· sa zastavÌ po dosiahnutÌ cieæovej sumy
        while (elapsedTime < maxCoins)
        {
            elapsedTime += Time.unscaledDeltaTime * 20f;
            coinText.text = string.Format("{0:F2}", elapsedTime);
            yield return null;
        }

        // Aktivuje tlaËidlo 'Done' aû po tom, Ëo mince dosiahnu maximum
        yield return new WaitForSecondsRealtime(2f);
        doneButton.SetActive(true);
    }

    // Nastavenie a zobrazenie jednotliv˝ch l˙Ëov
    private void SetupBeam(int index)
    {
        // POISTKA 1: Ak p˝tame predmet, na ktor˝ v UI nem·me voæn˝ slot, radöej funkciu ukonËÌme, inak hra spadne
        if (index >= items.Count)
        {
            Debug.LogWarning($"SnaûÌö sa zobraziù {index + 1}. predmet, ale v UI m·ö nastaven˝ch len {items.Count} 'ItemDisplays' slotov!");
            return;
        }

        // POISTKA 2: Ak p˝tame ikonu, ktor· neexistuje, radöej funkciu ukonËÌme
        if (index >= icons.Count)
        {
            return;
        }


        items[index].weaponBeam.SetActive(true);
        items[index].beam.SetActive(true);
        items[index].spriteImage.sprite = icons[index];

        // POISTKA 3: Oöetrenie farieb (Ak dizajnÈr nastavil do profilu len 1 farbu, ale padn˙ 3 predmety, pre ostatnÈ predmety pouûijeme t˙ posledn˙ nastaven˙ farbu).
        int colorIndex = Mathf.Min(index, dropProfile.beamColors.Length - 1);

        if (dropProfile.beamColors.Length > 0)
        {
            items[index].beam.GetComponent<Image>().color = dropProfile.beamColors[colorIndex];
        }
    }

    // Zobrazenie oneskoren˝ch l˙Ëov
    private IEnumerator ShowDelayedBeams(int startIndex, int endIndex)
    {
        yield return new WaitForSecondsRealtime(dropProfile.delayTime);

        for (int i = startIndex; i < endIndex; i++)
        {
            SetupBeam(i);
        }
    }

    public void DisplayerBeam(float noOfBeams)
    {
        // V˝poËet indexu, od ktorÈho sa zaËn˙ l˙Ëe zobrazovaù s oneskorenÌm
        int delayedStartIndex = Mathf.Max(0, (int)noOfBeams - dropProfile.delayedBeams);

        // Zobrazenie okamûit˝ch l˙Ëov
        for (int i = 0; i < delayedStartIndex; i++)
        {
            SetupBeam(i);
        }

        // Ak s˙ nastavenÈ oneskorenÈ l˙Ëe, spusti korutÌnu
        if (dropProfile.delayedBeams > 0)
        {
            StartCoroutine(ShowDelayedBeams(delayedStartIndex, (int)noOfBeams));
        }

        StartCoroutine(DisplayItems(noOfBeams));
    }

    private IEnumerator DisplayItems(float noOfBeams)
    {
        // »akanie na ˙vodn˙ anim·ciu z profilu
        yield return new WaitForSecondsRealtime(dropProfile.animDuration);

        int safeBeamsCount = Mathf.Min((int)noOfBeams, items.Count);

        if (safeBeamsCount == 5)
        {
            // äpeci·lna sekvencia pre 5 predmetov (1 -> 2 -> 2) Zobrazenie prvÈho predmetu
            items[0].weaponBeam.SetActive(false);
            items[0].sprite.SetActive(true);
            yield return new WaitForSecondsRealtime(0.3f);

            // Zobrazenie druhÈho a tretieho s˙Ëasne
            for (int i = 1; i <= 2; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(0.3f);

            // Zobrazenie ötvrtÈho a piateho s˙Ëasne
            for (int i = 3; i <= 4; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(0.3f);
        }
        else
        {
            // Fallback pre inÈ poËty predmetov - zobrazia sa postupne jeden po druhom
            for (int i = 0; i < safeBeamsCount; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }
    }

    public IEnumerator Open()
    {
        // Spustenie ohÚostrojov, ak s˙ povolenÈ v profile
        if (dropProfile.hasFireworks)
        {
            isAnimating = false; // Ak s˙ ohÚostroje, zabezpeËÌme, aby sa sekvencia nedala preskoËiù
            StartCoroutine(FlashWhite(chestPanel, 5));
            fireworks.SetActive(true);
            yield return new WaitForSecondsRealtime(dropProfile.fireworksDelay);
        }

        isAnimating = true; // Teraz uû povoæujeme preskakovanie anim·ciÌ

        // Aktiv·cia zakriven˝ch l˙Ëov, ak s˙ povolenÈ
        if (dropProfile.hasCurvedBeams)
        {
            StartCoroutine(ActivateCurvedBeams(dropProfile.curveBeamsSpawnTime));
        }

        // UrËenie poËtu zÌskan˝ch mincÌ (n·hodne z rozsahu) a spustenie ich anim·cie
        StartCoroutine(HandleCoinDisplay(Random.Range(dropProfile.minCoins, dropProfile.maxCoins)));

        DisplayerBeam(dropProfile.noOfItems);
        openingVFX.SetActive(true);
        beamVFX.SetActive(true);

        // »akanie na dÂûku anim·cie, kedy s˙ VFX aktÌvne
        yield return new WaitForSecondsRealtime(dropProfile.animDuration);

        openingVFX.SetActive(false);
    }

    // Aktivuje anim·cie
    public void Begin()
    {
        chestCover.SetActive(false);
        chestButton.SetActive(false);
        chestSequenceCoroutine = StartCoroutine(Open());
        audiosource.clip = dropProfile.openingSound;
        audiosource.Play();
    }

    private void SkipToRewards()
    {
        if (chestSequenceCoroutine != null)
            StopCoroutine(chestSequenceCoroutine);

        StopAllCoroutines();

        // Okamûite zobrazÌ vöetky l˙Ëe a ikony predmetov
        for (int i = 0; i < icons.Count; i++)
        {
            SetupBeam(i);
            if (i < items.Count)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
            }
        }

        // Okamûite nastavÌ fin·lnu hodnotu mincÌ
        coinText.gameObject.SetActive(true);
        coinText.text = coins.ToString("F2");

        // Aktivuje ukonËovacie prvky a vypne doËasnÈ VFX
        doneButton.SetActive(true);
        openingVFX.SetActive(false);
        isAnimating = false;
        chestPanel.color = originalColor;

        // PreskoËÌ zvuk otv·rania takmer na koniec (posledn· sekunda)
        if (audiosource != null && dropProfile.openingSound != null)
        {
            audiosource.clip = dropProfile.openingSound;

            // ZabezpeËÌ, aby sme nepreskoËili mimo dÂûku klipu
            float skipToTime = Mathf.Max(0, audiosource.clip.length - 3.55f);
            audiosource.time = skipToTime;
            audiosource.Play();
        }
    }

    private void Update()
    {
        // Ak prebieha anim·cia a hr·Ë stlaËÌ 'Cancel' (Esc), preskoËÌme na v˝sledky.
        if (isAnimating && Input.GetButtonDown("Cancel"))
        {
            SkipToRewards();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TryPressButton(chestButton);
            TryPressButton(doneButton);
        }
    }

    // Pomocn· metÛda, ktor· programovo vyvol· kliknutie na UI tlaËidlo, ak je aktÌvne.
    private void TryPressButton(GameObject buttonObj)
    {
        if (buttonObj.activeInHierarchy)
        {
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }
    }

    public void CloseUI()
    {
        // PripÌöeme nazbieranÈ mince z truhlice do hr·Ëovho invent·ra (collectora).
        collector.AddCoins(coins);

        if (audiosource != null && pickUpSound != null)
        {
            audiosource.clip = pickUpSound;
            audiosource.time = 0f;
            audiosource.Play();
        }

        // Resetujeme UI a vizu·lne efekty do pÙvodnÈho stavu.
        chestCover.SetActive(true);
        chestButton.SetActive(true);
        icons.Clear();
        beamVFX.SetActive(false);
        coinText.gameObject.SetActive(false);
        gameObject.SetActive(false);
        doneButton.SetActive(false);
        fireworks.SetActive(false);
        curvedBeams.SetActive(false);

        ResetDisplay();

        isAnimating = false;

        Time.timeScale = 1f;

        GameManager.instance.ChangeState(GameManager.GameState.Gameplay);
        currentChest.NotifyComplete();
    }

    // VyËistÌ zobrazenie predmetov v paneli.
    private void ResetDisplay()
    {
        foreach (var item in items)
        {
            item.beam.SetActive(false);
            item.sprite.SetActive(false);
            item.spriteImage.sprite = null;
        }

        dropProfile = null;
        icons.Clear();
    }
}
