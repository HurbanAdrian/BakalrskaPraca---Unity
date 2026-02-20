using UnityEngine;

[CreateAssetMenu(fileName ="WeaponScriptableObject", menuName ="ScriptableObjects/Weapon")]
public class WeaponScriptableObject : ScriptableObject
{
    [SerializeField]
    GameObject prefab;
    public GameObject Prefab { get => prefab; private set => prefab = value; }

    // Staty zbrane
    [SerializeField]
    float damage;
    public float Damage { get => damage; private set => damage = value; }

    [SerializeField]
    float speed;
    public float Speed { get => speed; private set => speed = value; }

    [SerializeField]
    float cooldownDuration;
    public float CooldownDuration { get => cooldownDuration; private set => cooldownDuration = value; }

    [SerializeField]
    int pierce;
    public int Pierce { get => pierce; private set => pierce = value; }

    [SerializeField]
    int level;          // nema byt mozne ho editnut v hre, iba v editore
    public int Level { get => level; private set => level = value; }

    [SerializeField]
    GameObject nextLevelPrefab;         // co sa z objektu stane ked ho levelupneme! NIE prefab kt. spawneme v dalsom leveli
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField]
    Sprite icon;        // nema byt mozne ho editnut v hre, iba v editore
    public Sprite Icon { get => icon; private set => icon = value; }
}
