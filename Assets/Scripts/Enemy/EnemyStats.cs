using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : EntityStats
{
    [System.Serializable]
    public struct Resistances
    {
        [Range(-1f, 1f)] public float freeze, kill, debuff;

        // UmoûÚuje n·m priamo n·sobiù odolnosti ËÌslom (faktorom).
        public static Resistances operator *(Resistances r, float factor)
        {
            r.freeze = Mathf.Min(1, r.freeze * factor);
            r.kill = Mathf.Min(1, r.kill * factor);
            r.debuff = Mathf.Min(1, r.debuff * factor);
            return r;
        }

        public static Resistances operator +(Resistances r, Resistances r2)
        {
            r.freeze += r2.freeze;
            r.kill = r2.kill;
            r.debuff = r2.debuff;
            return r;
        }

        // UmoûÚuje n·m navz·jom n·sobiù odolnosti pre multiplikatÌvne buffy.
        public static Resistances operator *(Resistances r1, Resistances r2)
        {
            r1.freeze = Mathf.Min(1, r1.freeze * r2.freeze);
            r1.kill = Mathf.Min(1, r1.kill * r2.kill);
            r1.debuff = Mathf.Min(1, r1.debuff * r2.debuff);

            return r1;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        public float maxHealth, moveSpeed, damage;
        public float knockbackMultiplier;
        public Resistances resistances;

        [System.Flags]
        public enum Boostable { health = 1, moveSpeed = 2, damage = 4, knockbackMultiplier = 8, resistances = 16 }
        public Boostable curseBoosts, levelBoosts;

        private static Stats Boost(Stats s1, float factor, Boostable boostable)
        {
            // Pomocou bitovej logiky (&) skontrolujeme, ktorÈ vlajky s˙ zaökrtnutÈ.
            if ((boostable & Boostable.health) != 0) s1.maxHealth *= factor;
            if ((boostable & Boostable.moveSpeed) != 0) s1.moveSpeed *= factor;
            if ((boostable & Boostable.damage) != 0) s1.damage *= factor;
            if ((boostable & Boostable.knockbackMultiplier) != 0) s1.knockbackMultiplier /= factor;
            if ((boostable & Boostable.resistances) != 0) s1.resistances *= factor;

            return s1;
        }

        // Preùaûenie oper·tora * pre n·sobenie prekliatÌm (Curse).
        public static Stats operator *(Stats s1, float factor) { return Boost(s1, factor, s1.curseBoosts); }

        // Preùaûenie oper·tora ^ (XOR) pre n·sobenie levelmi hr·Ëov.
        public static Stats operator ^(Stats s1, float factor) { return Boost(s1, factor, s1.levelBoosts); }


        public static Stats operator +(Stats s1, Stats s2)
        {
            s1.maxHealth += s2.maxHealth;
            s1.moveSpeed += s2.moveSpeed;
            s1.damage += s2.damage;
            s1.knockbackMultiplier += s2.knockbackMultiplier;
            s1.resistances += s2.resistances;
            return s1;
        }

        public static Stats operator *(Stats s1, Stats s2)
        {
            s1.maxHealth *= s2.maxHealth;
            s1.moveSpeed *= s2.moveSpeed;
            s1.damage *= s2.damage;
            s1.knockbackMultiplier *= s2.knockbackMultiplier;
            s1.resistances *= s2.resistances;
            return s1;
        }
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1, curseBoosts = Stats.Boostable.health | Stats.Boostable.moveSpeed };
    Stats actualStats;
    public Stats Actual
    {
        get { return actualStats; }
    }

    public BuffInfo[] attackEffects;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1f, 0f, 0f);
    public float damageFlashDuration = 0.2f;
    public float deathFadeTime = 0.6f;
    EnemyMovement movement;

    public static int count;

    protected override void Start()
    {
        base.Start();

        // Prid· glob·lny buff levelu, ak nejak˝ existuje.
        if (UILevelSelector.globalBuff && UILevelSelector.globalBuffAffectsEnemies)
        {
            ApplyBuff(UILevelSelector.globalBuff);
        }

        RecalculateStats();
        health = actualStats.maxHealth;

        movement = GetComponent<EnemyMovement>();
    }

    public override bool ApplyBuff(BuffData data, int variant = 0, float durationMultiplier = 1f)
    {
        // HodÌme si ËÌslom a ak uspejeme, zmrazenie ignorujeme.
        if ((data.type & BuffData.Type.freeze) > 0)
        {
            if (Random.value <= Actual.resistances.freeze) return false;
        }

        if ((data.type & BuffData.Type.debuff) > 0)
        {
            if (Random.value <= Actual.resistances.debuff) return false;
        }

        // Ak postava neodolala, zavol·me pÙvodn˙ (z·kladn˙) logiku aplik·cie buffu.
        return base.ApplyBuff(data, variant, durationMultiplier);
    }

    // VypoËÌta aktu·lne ötatistiky nepriateæa na z·klade rÙznych faktorov.
    public override void RecalculateStats()
    {
        float curse = GameManager.GetCumulativeCurse();
        float level = GameManager.GetCumulativeLevels();
        actualStats = (baseStats * curse) ^ level;

        // ¥Premenna pre kumulatÌvne n·sobitele
        Stats multiplier = new Stats
        {
            maxHealth = 1f, moveSpeed = 1f, damage = 1f, knockbackMultiplier = 1f,
            resistances = new Resistances { freeze = 1f, debuff = 1f, kill = 1f }
        };

        foreach (Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();

            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.enemyModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    multiplier *= bd.enemyModifier;
                    break;
            }
        }

        // Nasobiace sa daju ako posledne (keby mame 0 a chceme zrusit pohyb atd.) -> maju konecnu prioritu v tomto. Ked scitame tak hned pripocitame
        actualStats *= multiplier;
    }

    void Awake()
    {
        count++;
    }

    public override void TakeDamage(float dmg)
    {
        health -= dmg;

        // Ak sa poökodenie presne rovn· maxim·lnemu zdraviu, predpoklad·me, ûe ide o insta-kill.
        // Skontrolujeme odolnosù voËi zabitiu (kill resistance), aby sme zistili, Ëi sa mu vyhneme.
        if (dmg == actualStats.maxHealth)
        {
            if (Random.value < actualStats.resistances.kill)
            {
                return;
            }
        }

        // Vytvorenie textovÈho popupu a vizu·lneho efektu, keÔ nepriateæ dostane z·sah.
        if (dmg > 0)
        {
            StartCoroutine(DamageFlash());
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);
        }

        if (health <= 0)
        {
            Kill();
        }
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        TakeDamage(dmg);

        if (health <= 0) return;

        if (knockbackForce > 0)
        {
            // Ziskanie smeru knockbacku
            Vector2 knockbackDirection = (Vector2)transform.position - sourcePosition;
            movement.Knockback(knockbackDirection.normalized * knockbackForce, knockbackDuration);
        }
    }

    public override void RestoreHealth(float amount)
    {
        if (health < actualStats.maxHealth)
        {
            health += amount;
            if (health > actualStats.maxHealth)
            {
                health = actualStats.maxHealth;
            }
        }
    }

    IEnumerator DamageFlash()
    {
        ApplyTint(damageColor);
        yield return new WaitForSeconds(damageFlashDuration);
        RemoveTint(damageColor);
    }

    IEnumerator KillFade()
    {
        // »ak· na koniec jednÈho snÌmku.
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = sprite.color.a;

        while (t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;

            // Postupne zniûujeme alfa kan·l smerom k nule.
            sprite.color = new Color(
                sprite.color.r,
                sprite.color.g,
                sprite.color.b,
                (1 - t / deathFadeTime) * origAlpha
            );
        }

        Destroy(gameObject);
    }

    public override void Kill()
    {
        // Aktivaovanie Dropov
        DropRateManager drops = GetComponent<DropRateManager>();
        if (drops) drops.active = true;

        // 1. ZastavÌme DamageFlash, aby neprepÌsal farbu sp‰ù
        StopAllCoroutines();

        // 2. Vypneme kolÌzie, aby do neho hr·Ë nemohol naraziù
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Zastavenie pohybu ako Fyziky
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Vypneme pohyb
        if (movement != null) movement.enabled = false;
        // 3. Vypneme skript, aby sa nevykon·val Update (napr. pohyb)
        // Pozor: Toto zastavÌ Update(), ale Coroutiny na tomto objekte beûia Ôalej
        this.enabled = false;

        // 4. SpustÌme anim·ciu smrti
        StartCoroutine(KillFade());
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (health <= 0) return;

        if (Mathf.Approximately(Actual.damage, 0)) return;

        // Skontrolujeme, Ëi objekt, do ktorÈho sme narazili, m· komponent PlayerStats.
        if (collision.collider.TryGetComponent(out PlayerStats p))
        {
            Vector2 contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            p.TakeDamage(Actual.damage, contactPoint);

            foreach(BuffInfo b in attackEffects)
            {
                p.ApplyBuff(b);
            }
        }
    }


    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded)
        {
            return;
        }

        count--;
    }

}
