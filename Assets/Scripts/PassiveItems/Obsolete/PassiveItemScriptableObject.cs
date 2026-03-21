using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItemScriptableObject", menuName = "ScriptableObjects/PassiveItem")]
public class PassiveItemScriptableObject : ScriptableObject
{
    [SerializeField]
    float multiplier;
    public float Multiplier { get => multiplier; set => multiplier = value; }

    [SerializeField]
    int level;          // nema byt mozne ho editnut v hre, iba v editore
    public int Level { get => level; private set => level = value; }

    [SerializeField]
    GameObject nextLevelPrefab;         // co sa z objektu stane ked ho levelupneme! NIE prefab kt. spawneme v dalsom leveli
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField]
    string itemName;
    public string ItemName { get => itemName; private set => itemName = value; }

    [SerializeField]
    string description;         // ak je upgrade zbrane, tak popis toho co sa zmeni
    public string Description { get => description; private set => description = value; }

    [SerializeField]
    Sprite icon;        // nema byt mozne ho editnut v hre, iba v editore
    public Sprite Icon { get => icon; private set => icon = value; }
}
