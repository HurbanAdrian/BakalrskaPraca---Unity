using UnityEngine;

/// <summary>
/// BuffData je trieda, ktorá sa pouíva na vytvorenie základného buffu na akomko¾vek objekte EntityStats.
/// Tento základnı buff bude majite¾a buï lieèi, alebo mu ude¾ova poškodenie a vyprší po urèitom èase.
/// </summary>
[CreateAssetMenu(fileName = "Buff Data", menuName = "Game/Buff Data")]
public class BuffData : ScriptableObject
{
    public new string name = "New Buff";
    public Sprite icon;

    [System.Flags]
    public enum Type : byte { buff = 1, debuff = 2, freeze = 4, strong = 8 }
    public Type type;

    public enum StackType : byte { refreshDurationOnly, stacksFully, doesNotStack }
    public enum ModifierType : byte { additive, multiplicative }

    [System.Serializable]
    public class Stats
    {
        public string name;

        [Header("Visuals")]
        [Tooltip("Efekt (èastice), ktorı je pripojenı k objektu s tımto buffom.")]
        public ParticleSystem effect;
        [Tooltip("Farba tónovania jednotiek ovplyvnenıch tımto buffom.")]
        public Color tint = new Color(0, 0, 0, 0);
        [Tooltip("Èi tento buff spoma¾uje alebo zrıch¾uje animáciu ovplyvneného objektu.")]
        public float animationSpeed = 1f;

        [Header("Stats")]
        public float duration;
        public float damagePerSecond, healPerSecond;

        [Tooltip("Kontroluje, ako èasto sa aplikuje poškodenie alebo lieèenie za sekundu.")]
        public float tickInterval = 0.25f;

        public StackType stackType;
        public ModifierType modifierType;

        public Stats()
        {
            duration = 10f;
            damagePerSecond = 1f;
            healPerSecond = 1f;
            tickInterval = 0.25f;
        }

        public CharacterData.Stats playerModifier;
        public EnemyStats.Stats enemyModifier;
    }

    public Stats[] variations = new Stats[1] {
        new Stats { name = "Level 1" }
    };

    public float GetTickDamage(int variant = 0)
    {
        Stats s = Get(variant);
        return s.damagePerSecond * s.tickInterval;
    }

    public float GetTickHeal(int variant = 0)
    {
        Stats s = Get(variant);
        return s.healPerSecond * s.tickInterval;
    }

    public Stats Get(int variant = -1)
    {
        return variations[Mathf.Max(0, variant)];
    }
}
