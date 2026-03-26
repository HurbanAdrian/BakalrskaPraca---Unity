using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vyadujeme komponent VerticalLayoutGroup na tomto GameObjecte, pretoe ho pouívame na rovnomerné rozmiestnenie tlaèidiel.
[RequireComponent(typeof(VerticalLayoutGroup))]
public class UIUpgradeWindow : MonoBehaviour
{
    // Budeme potrebova prístup k padding / spacing atribútom layoutu.
    VerticalLayoutGroup verticalLayout;

    // Šablóny (prefab/template) pre tlaèidlá a nápovedu (tooltip), ktoré musíme priradi.
    public RectTransform upgradeOptionTemplate;
    public TextMeshProUGUI tooltipTemplate;

    [Header("Settings")]
    public int maxOptions = 4; // Nemôeme zobrazi viac moností ako tento poèet.
    public string newText = "New!"; // Text, ktorı sa zobrazí pri úplne novom vylepšení.

    // Farba pre "New!" text a benı text úrovne.
    public Color newTextColor = Color.yellow, levelTextColor = Color.white;

    [Header("Paths")]
    public string iconPath = "Icon/Item Icon";
    public string namePath = "Name", descriptionPath = "Description", buttonPath = "Button", levelPath = "Level";

    // Súkromné premenné pouívané na sledovanie stavu okna.
    RectTransform rectTransform;
    float optionHeight;
    int activeOptions;

    // Zoznam všetkıch tlaèidiel vylepšení v okne.
    List<RectTransform> upgradeOptions = new List<RectTransform>();

    // Pouíva sa na sledovanie šírky/vıšky obrazovky v poslednom snímku.
    // Slúi na detekciu zmeny rozlíšenia, aby sme vedeli prepoèíta ve¾kos okna.
    Vector2 lastScreen;

    // Toto je hlavná funkcia, ktorú budeme na tomto skripte vola.
    // Musíte zada <inventory>, do ktorého sa má predmet prida, a zoznam všetkıch
    // <possibleUpgrades> na zobrazenie. Vyberie poèet <pick> vylepšení a zobrazí ich.
    // Nakoniec, ak zadáte <tooltip>, zobrazí sa text v spodnej èasti okna.
    public void SetUpUpgrades(PlayerInventory inventory, List<ItemData> possibleUpgrades, int pick = 3, string tooltip = "")
    {
        pick = Mathf.Min(maxOptions, pick);

        // Ak nemáme dostatok boxov (tlaèidiel) pre monosti vylepšenia, vytvoríme ich.
        if (maxOptions > upgradeOptions.Count)
        {
            for (int i = upgradeOptions.Count; i < pick; i++)
            {
                GameObject go = Instantiate(upgradeOptionTemplate.gameObject, transform);
                upgradeOptions.Add((RectTransform)go.transform);
            }
        }

        // Ak je zadanı reazec, zapni tooltip (nápovedu).
        tooltipTemplate.text = tooltip;
        tooltipTemplate.gameObject.SetActive(tooltip.Trim() != "");

        // Aktivuj len ten poèet moností vylepšenia, ktoré potrebujeme, a priprav tlaèidlá a atribúty ako popisy atï.
        activeOptions = 0;
        int totalPossibleUpgrades = possibleUpgrades.Count;

        foreach (RectTransform r in upgradeOptions)
        {
            if (activeOptions < pick && activeOptions < totalPossibleUpgrades)
            {
                r.gameObject.SetActive(true);

                // Vyber jedno z monıch vylepšení a následne ho odstráò zo zoznamu.
                ItemData selected = possibleUpgrades[Random.Range(0, possibleUpgrades.Count)];
                possibleUpgrades.Remove(selected);
                Item item = inventory.Get(selected);

                // Vlo názov predmetu.
                TextMeshProUGUI name = r.Find(namePath).GetComponent<TextMeshProUGUI>();
                if (name)
                {
                    name.text = selected.name;
                }

                // Vlo aktuálnu úroveò predmetu alebo text "New!", ak ide o novú zbraò.
                TextMeshProUGUI level = r.Find(levelPath).GetComponent<TextMeshProUGUI>();
                if (level)
                {
                    if (item)
                    {
                        if (item.currentLevel >= item.maxLevel)
                        {
                            level.text = "Max!";
                            level.color = newTextColor;
                        }
                        else
                        {
                            level.text = selected.GetLevelData(item.currentLevel + 1).name;
                            level.color = levelTextColor;
                        }
                    }
                    else
                    {
                        level.text = newText;
                        level.color = newTextColor;
                    }
                }

                // Vlo popis predmetu (èo vylepšenie pridá).
                TextMeshProUGUI desc = r.Find(descriptionPath).GetComponent<TextMeshProUGUI>();
                if (desc)
                {
                    if (item)
                    {
                        desc.text = selected.GetLevelData(item.currentLevel + 1).description;
                    }
                    else
                    {
                        desc.text = selected.GetLevelData(1).description;
                    }
                }

                // Vlo ikonu predmetu.
                Image icon = r.Find(iconPath).GetComponent<Image>();
                if (icon)
                {
                    icon.sprite = selected.icon;
                }

                // Vlo priradenie akcie tlaèidlu.
                Button b = r.Find(buttonPath).GetComponent<Button>();
                if (b)
                {
                    b.onClick.RemoveAllListeners();
                    if (item)
                    {
                        b.onClick.AddListener(() => inventory.LevelUp(item));
                    }
                    else
                    {
                        b.onClick.AddListener(() => inventory.Add(selected));
                    }
                }

                activeOptions++;
            }
            else r.gameObject.SetActive(false);
        }
        // Upraví ve¾kos všetkıch elementov, aby nepresiahli ve¾kos okna.
        RecalculateLayout();
    }

    // Prepoèíta vıšky všetkıch elementov.
    // Volá sa vdy, keï sa zmení ve¾kos okna.
    // Robíme to manuálne, pretoe VerticalLayoutGroup nie vdy rozdelí priestor rovnomerne.
    void RecalculateLayout()
    {
        // Vypoèíta celkovú dostupnú vıšku pre všetky monosti, potom ju vydelí poètom moností.
        optionHeight = (rectTransform.rect.height - verticalLayout.padding.top - verticalLayout.padding.bottom - (maxOptions - 1) * verticalLayout.spacing);

        if (activeOptions == maxOptions && tooltipTemplate.gameObject.activeSelf)
            optionHeight /= maxOptions + 1;
        else
            optionHeight /= maxOptions;

        // Prepoèíta vıšku tooltipu (nápovedy), ak je momentálne aktívny.
        if (tooltipTemplate.gameObject.activeSelf)
        {
            RectTransform tooltipRect = (RectTransform)tooltipTemplate.transform;
            tooltipTemplate.gameObject.SetActive(true);
            tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, optionHeight);
            tooltipTemplate.transform.SetAsLastSibling();
        }

        // Nastaví vıšku kadého aktívneho tlaèidla (Upgrade Option).
        foreach (RectTransform r in upgradeOptions)
        {
            if (!r.gameObject.activeSelf) continue;
            r.sizeDelta = new Vector2(r.sizeDelta.x, optionHeight);
        }
    }

    // Táto funkcia kontroluje, èi je posledná šírka/vıška obrazovky rovnaká ako súèasná.
    // Ak nie, obrazovka zmenila ve¾kos a zavoláme RecalculateLayout() na aktualizáciu vıšky tlaèidiel.
    void Update()
    {
        // Prekreslí boxy v tomto elemente, ak sa zmení ve¾kos obrazovky.
        if (lastScreen.x != Screen.width || lastScreen.y != Screen.height)
        {
            RecalculateLayout();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

    // Awake sa volá pri inicializácii skriptu (ešte pred Start).
    void Awake()
    {
        // Naplní všetky naše dôleité premenné.
        verticalLayout = GetComponentInChildren<VerticalLayoutGroup>();

        if (tooltipTemplate) tooltipTemplate.gameObject.SetActive(false);
        if (upgradeOptionTemplate) upgradeOptions.Add(upgradeOptionTemplate);

        // Získaj RectTransform tohto objektu pre vıpoèty vıšky.
        rectTransform = (RectTransform)transform;
    }

    // Pomocná funkcia na automatické naplnenie našich premennıch.
    // Automaticky vyh¾adá GameObject s názvom "Upgrade Option" a priradí ho ako upgradeOptionTemplate, potom vyh¾adá "Tooltip" a priradí ho ako tooltipTemplate.
    void Reset()
    {
        upgradeOptionTemplate = (RectTransform)transform.Find("Upgrade Option");
        tooltipTemplate = transform.Find("Tooltip").GetComponentInChildren<TextMeshProUGUI>();
    }
}
