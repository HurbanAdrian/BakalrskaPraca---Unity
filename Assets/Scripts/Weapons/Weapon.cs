using UnityEngine;

/// <summary>
/// Komponent priradeny vsetkym Weapon prefabom. Weapon prefaby pracuju spolu s WeaponData ScriptableObjectami,
/// na manazovanie a spustanie Behaviourov vsetkych zbrani
/// </summary>

public abstract class Weapon : Item
{
    [System.Serializable]
    public class Stats : LevelData
    {
        [Header("Visuals")]
        public Projectile projectilePrefab;  // Ak pridame prefab, tak sa projektil bude spawnovat z tejto zbrane na cooldowne
        public Aura auraPrefab;              // Ak pridame prefab, tak sa aura spawne ked budeme mat equipnutu zbran
        public ParticleSystem hitEffect, procEffect;
        public Rect spawnVariance;

        [Header("Values")]
        public float lifeSpan;       // Ak 0, tak bude trvat navzdy
        public float damage, damageVariance, area, speed, cooldown, projectileInterval, knockback;
        public int number, piercing, maxInstances;

        public EntityStats.BuffInfo[] appliedBuffs;

        // Dovoli nam pouzit + operator na zlucenie 2 statov dokopy
        public static Stats operator +(Stats s1, Stats s2)
        {
            Stats result = new Stats();

            result.name = s2.name ?? s1.name;
            result.description = s2.description ?? s1.description;
            result.projectilePrefab = s2.projectilePrefab ?? s1.projectilePrefab;
            result.auraPrefab = s2.auraPrefab ?? s1.auraPrefab;
            result.hitEffect = s2.hitEffect == null ? s1.hitEffect : s2.hitEffect;
            result.procEffect = s2.procEffect == null ? s1.procEffect : s2.procEffect;
            result.spawnVariance = s2.spawnVariance;
            result.lifeSpan = s1.lifeSpan + s2.lifeSpan;
            result.damage = s1.damage + s2.damage;
            result.damageVariance = s1.damageVariance + s2.damageVariance;
            result.area = s1.area + s2.area;
            result.speed = s1.speed + s2.speed;
            result.cooldown = s1.cooldown + s2.cooldown;
            result.number = s1.number + s2.number;
            result.piercing = s1.piercing + s2.piercing;
            result.projectileInterval = s1.projectileInterval + s2.projectileInterval;
            result.knockback = s1.knockback + s2.knockback;
            result.appliedBuffs = s2.appliedBuffs == null || s2.appliedBuffs.Length <= 0 ? s1.appliedBuffs : s2.appliedBuffs; ;

            return result;
        }

        public float GetDamage()
        {
            return damage + Random.Range(0, damageVariance);
        }
    }

    protected Stats currentStats;

    protected float currentCooldown;

    protected PlayerMovement movement;

    // Pre dynamicky vytvorene zbrane, zavolat Initialise() na ich nastavenie
    public virtual void Initialise(WeaponData data)
    {
        base.Initialise(data);
        this.data = data;
        currentStats = data.baseStats;
        movement = GetComponentInParent<PlayerMovement>();
        ActivateCooldown();
    }

    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack(currentStats.number + owner.Stats.amount);
        }
    }

    // Level up zbrane a vypocet jej korespondujucich statov
    public override bool DoLevelUp()
    {
        if (!CanLevelUp())
        {
            Debug.LogWarning(string.Format("Cannot level up {0} to level {1}, max level of {2} already reached.", name, currentLevel, data.maxLevel));
            return false;
        }

        base.DoLevelUp();

        currentStats += (Stats)data.GetLevelData(currentLevel);
        return true;
    }

    public virtual bool CanAttack()
    {
        if (Mathf.Approximately(owner.Stats.might, 0)) return false;
        return currentCooldown <= 0f;
    }

    // Vykona utok zbrane a vrati TRUE ak bol utok uspesny. Parameter je pocet utokov / projektilov.
    // Samo o sebe to nerobi nic. Musime overridnut toto u potomka a pridat tomu spravanie (behaviour)
    protected virtual bool Attack(int attackCount = 1)
    {
        if (CanAttack())
        {
            ActivateCooldown();
            return true;
        }
        return false;
    }

    public virtual float GetDamage()
    {
        return currentStats.GetDamage() * owner.Stats.might;
    }

    public virtual Stats GetStats()
    {
        return currentStats;
    }

    public virtual float GetArea()
    {
        return currentStats.area * owner.Stats.area;
    }

    // Ak <Strict> je true, tak iba ked currentCooldown < 0. Zatial co false aktivuje cooldown aj ked este nepresiel cas
    public virtual bool ActivateCooldown(bool strict = false)
    {
        // KeÔ je povolen˝ reûim <strict> a cooldown eöte neskonËil, neobnovuj (neprehlbuj) cooldown.
        if (strict && currentCooldown > 0) return false;

        // VypoËÌtaj, ak˝ bude cooldown po zohæadnenÌ ötatistiky redukcie cooldownu u hr·Ëskej postavy.
        float actualCooldown = currentStats.cooldown * Owner.Stats.cooldown;

        // Obmedz maxim·lny cooldown na hodnotu <actualCooldown>, aby sme nemohli zv˝öiù cooldown nad t˙to hranicu, ak by sme t˙to funkciu n·hodou zavolali viackr·t.
        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);

        return true;
    }

    // ZabezpeËÌ, aby zbraÚ aplikovala svoje buffy na zasiahnut˝ objekt typu EntityStats.
    public void ApplyBuffs(EntityStats e)
    {
        if (GetStats().appliedBuffs == null || GetStats().appliedBuffs.Length == 0)
            return;

        foreach (EntityStats.BuffInfo b in GetStats().appliedBuffs)
        {
            e.ApplyBuff(b, owner.Actual.duration);
        }
    }


}
