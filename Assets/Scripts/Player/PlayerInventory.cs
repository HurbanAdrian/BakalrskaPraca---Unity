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

        // Priradí predmet do tohto slotu a aktualizuje UI
        public void Assign(Item assignedItem)
        {
            item = assignedItem;

            // Zistíme, èi je predmet zbraò
            if (item is Weapon)
            {
                Weapon w = item as Weapon;
            }
            else
            {
                Passive p = item as Passive;
            }

            Debug.Log(string.Format("Assigned {0} to player.", item.name));
        }

        // Vyèistí slot (napríklad ak by sme predmet chceli vyhodi)
        public void Clear()
        {
            item = null;
        }

        public bool IsEmpty() { return item == null; }
    }

    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);
    public UIInventoryIconsDisplay weaponUI, passiveUI;

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();   // Zoznam moností vylepšení pre zbrane
    public List<PassiveData> availablePassives = new List<PassiveData>(); // Zoznam moností vylepšení pre pasívne predmety

    public UIUpgradeWindow upgradeWindow;

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
            if (p != null && p.data == type)
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
            if (w != null && w.data == type)
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
                weaponUI.Refresh();
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
                passiveUI.Refresh();
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

    public int Add(WeaponData data, bool updateUI = true)
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
            spawnedWeapon.transform.SetParent(transform); // Nastav zbraò ako potomka hráèa
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.Initialise(data);
            spawnedWeapon.OnEquip();

            // Priraï zbraò do slotu (aktualizuje UI).
            weaponSlots[slotNum].Assign(spawnedWeapon);
            if (updateUI) weaponUI.Refresh();

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
    public int Add(PassiveData data, bool updateUI = true)
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
        if (updateUI) passiveUI.Refresh();

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
    public int Add(ItemData data, bool updateUI = true)
    {
        if (data is WeaponData) return Add(data as WeaponData, updateUI);
        else if (data is PassiveData) return Add(data as PassiveData, updateUI);
        return -1;
    }

    // Overload, aby sme mohli poui ItemData aj Item na vylepšenie predmetu v inventári.
    public bool LevelUp(ItemData data, bool updateUI = true)
    {
        Item item = Get(data);
        if (item) return LevelUp(item, updateUI);
        return false;
    }

    // Zvıši úroveò vybranej zbrane v inventári hráèa.
    public bool LevelUp(Item item, bool updateUI = true)
    {
        // Pokúsi sa zvıši úroveò predmetu.
        if (!item.DoLevelUp())
        {
            Debug.LogWarning(string.Format(
                "Nepodarilo sa zvıši úroveò pre {0}.",
                item.name
            ));
            return false;
        }

        if (updateUI)
        {
            weaponUI.Refresh();
            passiveUI.Refresh();
        }

        // Následne zatvorí obrazovku vıberu vylepšenia.
        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }

        // Ak ide o pasívny predmet, prepoèítaj štatistiky hráèa.
        if (item is Passive) player.RecalculateStats();

        return true;
    }

    // Skontroluje zoznam slotov a zistí, èi v òom zostali nejaké vo¾né miesta.
    int GetSlotsLeft(List<Slot> slots)
    {
        int count = 0;

        foreach (Slot s in slots)
        {
            if (s.IsEmpty()) count++;
        }

        return count;
    }

    // Urèuje, aké monosti vylepšení by sa mali zobrazi.
    void ApplyUpgradeOptions()
    {
        List<ItemData> availableUpgrades = new List<ItemData>();                // Prazdny List ktory budeme filtrovat
        List<ItemData> allUpgrades = new List<ItemData>(availableWeapons);      // Vsetky upgrady
        allUpgrades.AddRange(availablePassives);

        // Zistenie poètu vo¾nıch slotov.
        int weaponSlotsLeft = GetSlotsLeft(weaponSlots);
        int passiveSlotsLeft = GetSlotsLeft(passiveSlots);

        // Filtrovanie dostupnıch vylepšení.
        foreach (ItemData data in allUpgrades)
        {
            Item obj = Get(data);
            if (obj)
            {
                // Ak predmet máme, pridaj ho len ak nie je na max leveli.
                if (obj.currentLevel < data.maxLevel) availableUpgrades.Add(data);
            }
            else
            {
                // Ak predmet nemáme, pridaj ho len ak je vo¾nı slot.
                if (data is WeaponData && weaponSlotsLeft > 0) availableUpgrades.Add(data);
                else if (data is PassiveData && passiveSlotsLeft > 0) availableUpgrades.Add(data);
            }
        }

        // Zobraz UI okno vylepšení, ak ešte máme nejaké dostupné vylepšenia.
        int availUpgradeCount = availableUpgrades.Count;
        if (availUpgradeCount > 0)
        {
            // Vıpoèet šance na extra predmet na základe štatistiky Luck (Šastie).
            bool getExtraItem = 1f - 1f / player.Stats.luck > UnityEngine.Random.value;

            if (getExtraItem || availUpgradeCount < 4)
                upgradeWindow.SetUpUpgrades(this, availableUpgrades, 4);
            else
                upgradeWindow.SetUpUpgrades(this, availableUpgrades, 3, "Increase your Luck stat for a chance to get 4 items!");
        }
        // Ak u iadne upgrady nie sú, ale hra je v reime vıberu, ukonèi Level Up.
        else if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        ApplyUpgradeOptions();
    }

    // Získa všetky sloty od hráèa urèitého typu, buï Weapon (Zbraò) alebo Passive (Pasívny predmet).
    // Ak zadáš typ Item, vráti oba zoznamy - zbrane aj pasívne predmety.
    public Slot[] GetSlots<T>() where T : Item
    {
        // Skontroluje, ktorú skupinu slotov má vráti.
        // Ak sa pıtaš na Items, spojí zbrane aj pasívne sloty dokopy.
        switch (typeof(T).ToString())
        {
            case "Passive":
                return passiveSlots.ToArray();

            case "Weapon":
                return weaponSlots.ToArray();

            case "Item":
                List<Slot> s = new List<Slot>(passiveSlots);
                s.AddRange(weaponSlots);
                return s.ToArray();
        }

        // Ak by som pridal ïalšie podtriedy Itemu, budem musie prida nové cases hore
        Debug.LogWarning("Generic type provided to GetSlots() call does not have a coded behaviour.");
        return null;
    }

    // Verzia GetSlots(), ktorá pracuje s ItemData namiesto Item.
    public Slot[] GetSlotsFor<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PassiveData))
        {
            return passiveSlots.ToArray();
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return weaponSlots.ToArray();
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<Slot> s = new List<Slot>(passiveSlots);
            s.AddRange(weaponSlots);
            return s.ToArray();
        }

        // Ak by som pridal ïalšie podtriedy Itemu, budem musie prida nové cases hore
        Debug.LogWarning("Generic type provided to GetSlotsFor() call does not have a coded behaviour.");
        return null;
    }

    // Generické varianty GetSlotsLeft(), ktoré sa jednoduchšie pouívajú.
    // Verzia pre Item (objekty v hre)
    public int GetSlotsLeft<T>() where T : Item
    {
        return GetSlotsLeft(new List<Slot>(GetSlots<T>()));
    }

    // Verzia pre ItemData (ScriptableObjects)
    public int GetSlotsLeftFor<T>() where T : ItemData
    {
        return GetSlotsLeft(new List<Slot>(GetSlotsFor<T>()));
    }

    public T[] GetAvailable<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PassiveData))
        {
            return availablePassives.ToArray() as T[];
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return availableWeapons.ToArray() as T[];
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<ItemData> list = new List<ItemData>(availablePassives);
            list.AddRange(availableWeapons);
            return list.ToArray() as T[];
        }

        Debug.LogWarning("Generic type provided to GetAvailable() call does not have a coded behaviour.");
        return null;
    }

    // Získa všetky dostupné predmety (zbrane alebo pasívky), ktoré hráè ešte nevlastní.
    public T[] GetUnowned<T>() where T : ItemData
    {
        // 1. Najprv získame zoznam úplne všetkıch predmetov daného typu v hre.
        var available = GetAvailable<T>();

        if (available == null || available.Length == 0)
            return new T[0]; // Vráti prázdne pole, ak je zoznam null alebo prázdny.

        List<T> list = new List<T>(available);

        // 2. Získame zoznam slotov, ktoré hráè momentálne pouíva.
        var slots = GetSlotsFor<T>();
        if (slots != null)
        {
            foreach (Slot s in slots)
            {
                if (s?.item?.data != null && list.Contains(s.item.data as T))
                {
                    list.Remove(s.item.data as T);
                }
            }
        }

        return list.ToArray();
    }

    public T[] GetEvolvables<T>() where T : Item
    {
        List<T> result = new List<T>();

        foreach (Slot s in GetSlots<T>())
        {
            if (s.item is T t && t != null && t.CanEvolve(0).Length > 0)
            {
                result.Add(t);
            }
        }
        return result.ToArray();
    }

    public T[] GetUpgradables<T>() where T : Item
    {
        List<T> result = new List<T>();

        foreach (Slot s in GetSlots<T>())
        {
            if (s.item is T t && t.CanLevelUp())
            {
                result.Add(t);
            }
        }
        return result.ToArray();
    }
}
