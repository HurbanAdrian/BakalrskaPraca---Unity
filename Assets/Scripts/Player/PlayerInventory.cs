using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;
        public Image image; // Referencia na UI obrázok (ikonku) v inventári

        // Priradí predmet do tohto slotu a aktualizuje UI
        public void Assign(Item assignedItem)
        {
            item = assignedItem;

            // Zistíme, èi je predmet zbraò
            if (item is Weapon)
            {
                Weapon w = item as Weapon;
                image.enabled = true;
                image.sprite = w.data.icon;
            }
            else
            {
                Passive p = item as Passive;
                image.enabled = true;
                image.sprite = p.data.icon;
            }

            Debug.Log(string.Format("Assigned {0} to player.", item.name));
        }

        // Vyèistí slot (napríklad ak by sme predmet chceli vyhodi)
        public void Clear()
        {
            item = null;
            image.enabled = false;
            image.sprite = null;
        }

        public bool IsEmpty() { return item == null; }
    }

    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);

    [System.Serializable]
    public class UpgradeUI
    {
        public TMP_Text upgradeNameDisplay;
        public TMP_Text upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();   // Zoznam moností vylepšení pre zbrane
    public List<PassiveData> availablePassives = new List<PassiveData>(); // Zoznam moností vylepšení pre pasívne predmety
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();      // Zoznam UI prvkov pre okná vylepšení prítomnıch v scéne

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    // Skontroluje, èi sa v inventári nachádza predmet urèitého typu.
    public bool Has(ItemData type) { return Get(type); }

    public Item Get(ItemData type)
    {
        if (type is WeaponData) return Get(type as WeaponData);
        else if (type is PassiveData) return Get(type as PassiveData);
        return null;
    }

    // Nájde pasívny predmet urèitého typu v inventári.
    public Passive Get(PassiveData type)
    {
        foreach (Slot s in passiveSlots)
        {
            Passive p = s.item as Passive;
            if (p.data == type)
                return p;
        }
        return null;
    }

    // Nájde zbraò urèitého typu v inventári.
    public Weapon Get(WeaponData type)
    {
        foreach (Slot s in weaponSlots)
        {
            Weapon w = s.item as Weapon;
            if (w.data == type)
                return w;
        }
        return null;
    }

    // Odstráni zbraò urèitého typu, špecifikovanú pomocou <data>.
    public bool Remove(WeaponData data, bool removeUpgradeAvailability = false)
    {
        // Odstráni túto zbraò z ponuky (poolu) monıch vylepšení.
        if (removeUpgradeAvailability) availableWeapons.Remove(data);

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon w = weaponSlots[i].item as Weapon;

            if (w != null && w.data == data)
            {
                weaponSlots[i].Clear();
                w.OnUnequip();
                Destroy(w.gameObject);
                return true;
            }
        }

        return false;
    }

    // Odstráni pasívny predmet urèitého typu, špecifikovanı pomocou <data>.
    public bool Remove(PassiveData data, bool removeUpgradeAvailability = false)
    {
        // Odstráni tento pasívny predmet z ponuky (poolu) monıch vylepšení.
        if (removeUpgradeAvailability) availablePassives.Remove(data);

        for (int i = 0; i < passiveSlots.Count; i++)
        {
            Passive p = passiveSlots[i].item as Passive;

            if (p != null && p.data == data)
            {
                passiveSlots[i].Clear();
                p.OnUnequip();
                Destroy(p.gameObject);
                return true;
            }
        }

        return false;
    }

    // Ak je odovzdané ItemData, zistíme o akı typ ide a zavoláme príslušné preaenie (overload).
    // Máme tu aj volite¾nı boolean na odstránenie tohto predmetu zo zoznamu vylepšení.
    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        if (data is PassiveData) return Remove(data as PassiveData, removeUpgradeAvailability);
        else if (data is WeaponData) return Remove(data as WeaponData, removeUpgradeAvailability);
        return false;
    }

    public int Add(WeaponData data)
    {
        int slotNum = -1;

        // Pokus o nájdenie prázdneho slotu. Inak poui weaponSlots.Count.
        for (int i = 0; i < weaponSlots.Capacity; i++)
        {
            if (weaponSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        // Ak nie je iadny prázdny slot, ukonèi funkciu.
        if (slotNum < 0) return slotNum;

        // V opaènom prípade vytvor zbraò v slote. Získaj typ zbrane, ktorú chceme spawnú.
        Type weaponType = Type.GetType(data.behaviour);

        if (weaponType != null)
        {
            // Spawni GameObject zbrane.
            GameObject go = new GameObject(data.baseStats.name + " Controller");
            Weapon spawnedWeapon = (Weapon)go.AddComponent(weaponType);
            spawnedWeapon.Initialise(data);
            spawnedWeapon.transform.SetParent(transform); // Nastav zbraò ako potomka hráèa
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.OnEquip();

            // Priraï zbraò do slotu (aktualizuje UI).
            weaponSlots[slotNum].Assign(spawnedWeapon);

            // Zatvor UI pre Level Up, ak je zapnuté.
            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }

            return slotNum;
        }
        else
        {
            // Ak sme nenašli triedu (skript) zbrane pod¾a zadaného textu, vypíšeme varovanie.
            Debug.LogWarning(string.Format(
                "Invalid weapon type specified for {0}.",
                data.name
            ));
        }

        return -1;
    }

    // Nájde prázdny slot a pridá pasívny predmet urèitého typu. Vráti èíslo slotu, do ktorého bol predmet vloenı.
    public int Add(PassiveData data)
    {
        int slotNum = -1;

        // Pokus o nájdenie prázdneho slotu.
        for (int i = 0; i < passiveSlots.Capacity; i++)
        {
            if (passiveSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        // Ak nie je iadny prázdny slot, ukonèíme to (vráti -1).
        if (slotNum < 0) return slotNum;

        // V opaènom prípade vytvoríme pasívny predmet v danom slote. Vytvoríme novı prázdny hernı objekt.
        GameObject go = new GameObject(data.baseStats.name + " Passive");

        // Pridáme mu komponent Passive a inicializujeme ho.
        Passive p = go.AddComponent<Passive>();
        p.Initialise(data);

        // Nastavíme predmet ako potomka (child) hráèa a vycentrujeme ho.
        p.transform.SetParent(transform);
        p.transform.localPosition = Vector2.zero;

        // Priradíme pasívny predmet do slotu v UI.
        passiveSlots[slotNum].Assign(p);

        // Ak je práve zapnuté okno s vıberom level-upu, zatvoríme ho.
        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }

        // Prepoèítame hráèove staty, aby sa okamite aplikovali bonusy z tohto nového predmetu!
        player.RecalculateStats();

        return slotNum;
    }

    // Ak nevieme, akı predmet sa pridáva, táto funkcia to zistí.
    public int Add(ItemData data)
    {
        if (data is WeaponData) return Add(data as WeaponData);
        else if (data is PassiveData) return Add(data as PassiveData);
        return -1;
    }

    public void LevelUpWeapon(int slotIndex, int upgradeIndex)
    {
        if (weaponSlots.Count > slotIndex)
        {
            Weapon weapon = weaponSlots[slotIndex].item as Weapon;

            if (weapon == null) return;

            // Nevylepšuj zbraò, ak je u na maximálnej úrovni.
            if (!weapon.DoLevelUp())
            {
                Debug.LogWarning(string.Format(
                    "Failed to level up {0}.",
                    weapon.name
                ));
                return;
            }
        }

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void LevelUpPassiveItem(int slotIndex, int upgradeIndex)
    {
        if (passiveSlots.Count > slotIndex)
        {
            Passive p = passiveSlots[slotIndex].item as Passive;

            // MOJA OPRAVA: Pridaná poistka, aby hra nespadla, ak je slot prázdny.
            if (p == null) return;

            if (!p.DoLevelUp())
            {
                Debug.LogWarning(string.Format(
                    "Failed to level up {0}.",
                    p.name
                ));
                return;
            }
        }

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }

        player.RecalculateStats();
    }

    // Urèuje, aké monosti vylepšení by sa mali zobrazi.
    void ApplyUpgradeOptions()
    {
        // Vytvoríme kópiu zoznamov dostupnıch vylepšení zbraní / pasívnych predmetov,
        // aby sme cez ne mohli v tejto funkcii prechádza (iterova).
        List<WeaponData> availableWeaponUpgrades = new List<WeaponData>(availableWeapons);
        List<PassiveData> availablePassiveItemUpgrades = new List<PassiveData>(availablePassives);

        // Prejdeme kadı slot v UI vylepšení.
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            // Ak u nie sú iadne dostupné vylepšenia, potom to prerušíme.
            if (availableWeaponUpgrades.Count == 0 && availablePassiveItemUpgrades.Count == 0)
                return;

            // Urèíme, èi by toto vylepšenie malo by pre pasívne alebo aktívne zbrane.
            int upgradeType;
            if (availableWeaponUpgrades.Count == 0)
            {
                upgradeType = 2;
            }
            else if (availablePassiveItemUpgrades.Count == 0)
            {
                upgradeType = 1;
            }
            else
            {
                // Náhodne vygeneruje èíslo medzi 1 a 2.
                upgradeType = UnityEngine.Random.Range(1, 3);
            }

            // Generuje vylepšenie aktívnej zbrane.
            if (upgradeType == 1)
            {
                // Vyberie vylepšenie zbrane, potom ho odstráni, aby sme ho nedostali dvakrát.
                WeaponData chosenWeaponUpgrade = availableWeaponUpgrades[UnityEngine.Random.Range(0, availableWeaponUpgrades.Count)];
                availableWeaponUpgrades.Remove(chosenWeaponUpgrade);

                // Uistíme sa, e vybrané dáta zbrane sú platné.
                if (chosenWeaponUpgrade != null)
                {
                    // Zapne UI slot.
                    EnableUpgradeUI(upgradeOption);

                    // Prejde všetky naše existujúce zbrane. Ak nájdeme zhodu,
                    // pripojíme event listener na tlaèidlo, ktoré vylepší zbraò,
                    // keï sa na túto monos vylepšenia klikne.
                    bool isLevelUp = false;
                    for (int i = 0; i < weaponSlots.Count; i++)
                    {
                        Weapon w = weaponSlots[i].item as Weapon;
                        if (w != null && w.data == chosenWeaponUpgrade)
                        {
                            // Ak je zbraò u na maximálnej úrovni, nedovo¾ vylepšenie.
                            if (chosenWeaponUpgrade.maxLevel <= w.currentLevel)
                            {
                                //DisableUpgradeUI(upgradeOption);
                                isLevelUp = false;
                                break;
                            }

                            // Nastavíme Event Listener, a popis predmetu a levelu tak, aby zodpovedal ïalšiemu levelu
                            upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpWeapon(i, i)); // Aplikuje funkcionalitu tlaèidla
                            Weapon.Stats nextLevel = chosenWeaponUpgrade.GetLevelData(w.currentLevel + 1);
                            upgradeOption.upgradeDescriptionDisplay.text = nextLevel.description;
                            upgradeOption.upgradeNameDisplay.text = nextLevel.name;
                            upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.icon;
                            isLevelUp = true;
                            break;
                        }
                    }

                    // Ak sa kód dostane sem, znamená to, e budeme pridáva novú zbraò, namiesto
                    // vylepšovania existujúcej zbrane.
                    if (!isLevelUp)
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(() => Add(chosenWeaponUpgrade)); // Aplikuje funkcionalitu tlaèidla
                        upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.baseStats.description; // Aplikuje poèiatoènı popis
                        upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.baseStats.name;       // Aplikuje poèiatoèné meno
                        upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.icon;
                    }
                }
            }
            else if (upgradeType == 2)
            {
                // POZNÁMKA: Budeme musie tento systém preprogramova, pretoe momentálne vypne slot vylepšenia,
                // ak narazíme na zbraò, ktorá u dosiahla maximálny level.
                PassiveData chosenPassiveUpgrade = availablePassiveItemUpgrades[UnityEngine.Random.Range(0, availablePassiveItemUpgrades.Count)];
                availablePassiveItemUpgrades.Remove(chosenPassiveUpgrade);

                if (chosenPassiveUpgrade != null)
                {
                    // Zapne UI slot.
                    EnableUpgradeUI(upgradeOption);

                    // Prejde všetky naše existujúce pasívne predmety. Ak nájdeme zhodu,
                    // pripojíme event listener na tlaèidlo, ktoré vylepší zbraò,
                    // keï sa na túto monos vylepšenia klikne.
                    bool isLevelUp = false;
                    for (int i = 0; i < passiveSlots.Count; i++)
                    {
                        Passive p = passiveSlots[i].item as Passive;
                        if (p != null && p.data == chosenPassiveUpgrade)
                        {
                            // Ak je pasívny predmet u na maximálnej úrovni, nedovo¾ vylepšenie.
                            if (chosenPassiveUpgrade.maxLevel <= p.currentLevel)
                            {
                                //DisableUpgradeUI(upgradeOption);
                                isLevelUp = false;
                                break;
                            }

                            upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpPassiveItem(i, i)); // Aplikuje funkcionalitu tlaèidla
                            Passive.Modifier nextLevel = chosenPassiveUpgrade.GetLevelData(p.currentLevel + 1);
                            upgradeOption.upgradeDescriptionDisplay.text = nextLevel.description;
                            upgradeOption.upgradeNameDisplay.text = nextLevel.name;
                            upgradeOption.upgradeIcon.sprite = chosenPassiveUpgrade.icon;
                            isLevelUp = true;
                            break;
                        }
                    }

                    if (!isLevelUp) // Spawne novı pasívny predmet
                    {
                        upgradeOption.upgradeButton.onClick.AddListener(() => Add(chosenPassiveUpgrade)); // Aplikuje funkcionalitu tlaèidla
                        Passive.Modifier nextLevel = chosenPassiveUpgrade.baseStats;
                        upgradeOption.upgradeDescriptionDisplay.text = nextLevel.description; // Aplikuje poèiatoènı popis
                        upgradeOption.upgradeNameDisplay.text = nextLevel.name;       // Aplikuje poèiatoèné meno
                        upgradeOption.upgradeIcon.sprite = chosenPassiveUpgrade.icon;
                    }
                }
            }
        }
    }

    void RemoveUpgradeOptions()
    {
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            // Odstráni všetky predchádzajúce funkcie priradené k tlaèidlu, aby sa nespúšali viackrát.
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);    // Zavoláme metódu DisableUpgradeUI, aby sme vypli všetky UI monosti predtım, ne na ne aplikujeme vylepšenia
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    }

    void DisableUpgradeUI(UpgradeUI ui)
    {
        // Vypne celı hernı objekt, ktorı je rodièom (parent) tohto textu (èie vypne celé okno/tlaèidlo pre danı upgrade).
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        // Zapne celı hernı objekt, ktorı je rodièom (parent) tohto textu.
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }
}
