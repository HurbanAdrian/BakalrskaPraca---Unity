using UnityEngine;

/// <summary>
/// Náhrada za triedu PassiveItemScriptableObject. Pointa je, e chceme uklada všetky 
/// dáta o leveloch pasívneho predmetu v jednom jedinom objekte, namiesto toho, aby sme 
/// mali viacero objektov pre jeden pasívny predmet (èo by sme museli robi, ak by sme 
/// naïalej pouívali starı PassiveItemScriptableObject).
/// </summary>
[CreateAssetMenu(fileName = "Passive Data", menuName = "Game/Passive Data")]
public class PassiveData : ItemData
{
    public Passive.Modifier baseStats;
    public Passive.Modifier[] growth;

    public override Item.LevelData GetLevelData(int level)
    {
        if (level <= 1) return baseStats;

        // Vyberie štatistiky pre ïalší level.
        if (level - 2 < growth.Length)
            return growth[level - 2];

        // Vráti prázdnu hodnotu a vypíše varovanie.
        Debug.LogWarning(string.Format("Passive doesn't have its level up stats configured for Level {0}!", level));
        return new Passive.Modifier();
    }
}
