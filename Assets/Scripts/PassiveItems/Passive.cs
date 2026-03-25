using UnityEngine;

/// <summary>
/// Trieda, ktorá prijíma PassiveData a slúži na zvýšenie hráèových
/// štatistík pri jej získaní.
/// </summary>
public class Passive : Item
{
    public PassiveData data;
    [SerializeField] CharacterData.Stats currentBoosts;

    [System.Serializable]
    public struct Modifier
    {
        public string name, description;
        public CharacterData.Stats boosts;
    }

    // Pre dynamicky vytvorené pasívne predmety zavolaj initialise na nastavenie všetkého.
    public virtual void Initialise(PassiveData data)
    {
        base.Initialise(data);
        this.data = data;
        currentBoosts = data.baseStats.boosts;
    }

    public virtual CharacterData.Stats GetBoosts()
    {
        return currentBoosts;
    }

    // Zvýši úroveò predmetu o 1 a vypoèíta príslušné štatistiky.
    public override bool DoLevelUp()
    {
        // Zabráni zvýšeniu úrovne, ak sme už na maximálnej úrovni.
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("Cannot level up {0} to level {1}, max level of {2} already reached.", name, currentLevel, data.maxLevel));
            return false;
        }

        base.DoLevelUp();

        currentBoosts += data.GetLevelData(currentLevel).boosts;
        return true;
    }
}
