using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Z·kladn· trieda pre pasÌvne predmety (Passive) aj zbrane (Weapon). Je prim·rne urËen·
/// na rieöenie evol˙cie zbranÌ, keÔûe chceme, aby sa zbrane aj pasÌvne predmety mohli vyvÌjaù.
/// </summary>
public abstract class Item : MonoBehaviour
{
    public int currentLevel = 1, maxLevel = 1;
    [HideInInspector] public ItemData data;
    protected ItemData.Evolution[] evolutionData;
    protected PlayerInventory inventory;
    protected PlayerStats owner;

    public PlayerStats Owner {  get { return owner; } }

    [System.Serializable]
    public class LevelData
    {
        public string name, description;
    }

    public virtual void Initialise(ItemData data)
    {
        maxLevel = data.maxLevel;
        // UloûÌme d·ta o evol˙cii, pretoûe musÌme sledovaù, Ëi s˙ vöetky katalyz·tory v invent·ri, aby sme mohli zbraÚ vyvin˙ù (evolvovaù).
        evolutionData = data.evolutionData;

        // Nefektivne (mozno zmenit do buducna)
        inventory = FindAnyObjectByType<PlayerInventory>();
        owner = FindAnyObjectByType<PlayerStats>();
    }

    public virtual ItemData.Evolution[] CanEvolve(int levelUpAmount = 1)
    {
        // Ak predmet nem· ûiadne evol˙cie, vr·time pr·zdne pole.
        if (evolutionData == null)
        {
            return new ItemData.Evolution[0];
        }

        List<ItemData.Evolution> possibleEvolutions = new List<ItemData.Evolution>();

        // Skontroluje kaûd˙ uveden˙ evol˙ciu a zistÌ, Ëi s˙ splnenÈ podmienky v invent·ri.
        foreach (ItemData.Evolution e in evolutionData)
        {
            if (CanEvolve(e, levelUpAmount)) possibleEvolutions.Add(e);
        }

        return possibleEvolutions.ToArray();
    }

    // Skontroluje, Ëi je öpecifick· evol˙cia moûn·.
    public virtual bool CanEvolve(ItemData.Evolution evolution, int levelUpAmount = 1)
    {
        // k sme v Inöpektore zabudli nastaviù v˝sledok evol˙cie (Outcome), rovno to zruöÌme.
        if (evolution.outcome.itemType == null)
        {
            Debug.LogWarning(string.Format("Pozor! ZbraÚ {0} sa snaûÌ vyvin˙ù, ale ch˝ba jej 'Outcome Item Type' v Inöpektore!", data.name));
            return false;
        }

        // NemÙûe sa vyvin˙ù, ak predmet nedosiahol ˙roveÚ potrebn˙ na evol˙ciu.
        if (evolution.evolutionLevel > currentLevel + levelUpAmount)
        {
            Debug.LogWarning(string.Format("Evolution failed. Current level {0}, evolution level {1}", currentLevel, evolution.evolutionLevel));
            return false;
        }

        // Skontroluje, Ëi s˙ vöetky katalyz·tory v invent·ri.
        foreach (ItemData.Evolution.Config c in evolution.catalysts)
        {
            Item item = inventory.Get(c.itemType);
            if (!item || item.currentLevel < c.level)
            {
                Debug.LogWarning(string.Format("Evolution failed. Missing {0}", c.itemType.name));
                return false;
            }
        }

        return true;
    }

    // AttemptEvolution spawne nov˙ zbraÚ pre postavu a odstr·ni vöetky zbrane/predmety, ktorÈ maj˙ byù pri tom konzumovanÈ (zniËenÈ).
    public virtual bool AttemptEvolution(ItemData.Evolution evolutionData, int levelUpAmount = 1, bool updateUI = true)
    {
        if (!CanEvolve(evolutionData, levelUpAmount))
            return false;

        // Mali by sme konzumovaù pasÌvne predmety / zbrane?
        bool consumePassives = (evolutionData.consumes & ItemData.Evolution.Consumption.passives) > 0;
        bool consumeWeapons = (evolutionData.consumes & ItemData.Evolution.Consumption.weapons) > 0;

        // Prejdeme vöetky katalyz·tory a skontrolujeme, Ëi by sme ich mali konzumovaù.
        foreach (ItemData.Evolution.Config c in evolutionData.catalysts)
        {
            if (c.itemType is PassiveData && consumePassives) inventory.Remove(c.itemType, true);
            if (c.itemType is WeaponData && consumeWeapons) inventory.Remove(c.itemType, true);
        }

        // Mali by sme konzumovaù aj sami seba?
        if (this is Passive && consumePassives) inventory.Remove((this as Passive).data, true);
        else if (this is Weapon && consumeWeapons) inventory.Remove((this as Weapon).data, true);

        // Prid·me nov˙ zbraÚ do n·öho invent·ra.
        inventory.Add(evolutionData.outcome.itemType, updateUI);

        return true;
    }

    public virtual bool CanLevelUp()
    {
        return currentLevel < maxLevel;
    }

    // Vûdy, keÔ sa predmet vylepöÌ na ÔalöÌ level, pok˙s sa o jeho evol˙ciu.
    public virtual bool DoLevelUp(bool updateUI = true)
    {
        currentLevel++;

        if (evolutionData == null) return true;

        // Pok˙si sa vyvin˙ù do kaûdej uvedenej evol˙cie tejto zbrane, ak je podmienkou evol˙cie zbrane levelovanie (auto).
        foreach (ItemData.Evolution e in evolutionData)
        {
            if (e.condition == ItemData.Evolution.Condition.auto)
                AttemptEvolution(e, 1, updateUI);
        }
        return true;
    }

    // AkÈ efekty zÌskaö pri vybavenÌ (nasadenÌ) predmetu.
    public virtual void OnEquip() { }

    // AkÈ efekty sa odstr·nia pri odobratÌ predmetu.
    public virtual void OnUnequip() { }
}
