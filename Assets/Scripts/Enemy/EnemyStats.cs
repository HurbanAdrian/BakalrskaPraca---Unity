using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    [System.Serializable]
    public struct Resistances
    {
        [Range(0f, 1f)] public float freeze, kill, debuff;

        // UmoûÚuje n·m priamo n·sobiù odolnosti ËÌslom (faktorom).
        public static Resistances operator *(Resistances r, float factor)
        {
            r.freeze = Mathf.Min(1, r.freeze * factor);
            r.kill = Mathf.Min(1, r.kill * factor);
            r.debuff = Mathf.Min(1, r.debuff * factor);
            return r;
        }
    }

    [System.Serializable]
    public struct Stats
    {
        [Min(0)] public float maxHealth, moveSpeed, damage;
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
    }

    public Stats baseStats = new Stats { maxHealth = 10, moveSpeed = 1, damage = 3, knockbackMultiplier = 1, curseBoosts = Stats.Boostable.health | Stats.Boostable.moveSpeed };
    Stats actualStats;
    public Stats Actual
    {
        get { return actualStats; }
    }

    float currentHealth;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1f, 0f, 0f);
    public float damageFlashDuration = 0.2f;
    public float deathFadeTime = 0.6f;
    Color originalColor;
    SpriteRenderer spriteRenderer;
    EnemyMovement movement;

    public static int count;

    void Start()
    {
        RecalculateStats();
        currentHealth = actualStats.maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        movement = GetComponent<EnemyMovement>();
    }

    // VypoËÌta aktu·lne ötatistiky nepriateæa na z·klade rÙznych faktorov.
    public void RecalculateStats()
    {
        float curse = GameManager.GetCumulativeCurse();
        float level = GameManager.GetCumulativeLevels();

        actualStats = (baseStats * curse) ^ level;
    }

    void Awake()
    {
        count++;
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        // Ak sa poökodenie presne rovn· maxim·lnemu zdraviu, predpoklad·me, ûe ide o insta-kill.
        // Skontrolujeme odolnosù voËi zabitiu (kill resistance), aby sme zistili, Ëi sa mu nepriateæ vyhne.
        if (Mathf.Approximately(dmg, actualStats.maxHealth))
        {
            // "HodÌme si kockou", aby sme zistili, Ëi sa nepriateæ vyhne smrti. ZÌskame n·hodn˙ hodnotu od 0 do 1. Ak je toto ËÌslo niûöie ako kill resistance, nepriateæ preûije bez poökodenia.
            if (Random.value < actualStats.resistances.kill)
            {
                return;
            }
        }

        currentHealth -= dmg;
        StartCoroutine(DamageFlash());

        // Vytvorime text pop up
        if (dmg > 0)
        {
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);
        }

        if (currentHealth <= 0)
        {
            Kill();
            return;
        }

        if (knockbackForce > 0)
        {
            // Ziskanie smeru knockbacku
            Vector2 knockbackDirection = (Vector2)transform.position - sourcePosition;
            movement.Knockback(knockbackDirection.normalized * knockbackForce, knockbackDuration);
        }
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator KillFade()
    {
        float timer = 0f;
        Color startColor = spriteRenderer.color; // Pouûijeme aktu·lnu farbu (aj ak je Ëerven· po damage)

        while (timer < deathFadeTime)
        {
            timer += Time.deltaTime;

            // Lerp je ËistejöÌ spÙsob ako poËÌtaù prechod
            float newAlpha = Mathf.Lerp(startColor.a, 0f, timer / deathFadeTime);

            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null; // »ak·me na ÔalöÌ frame (netreba WaitForEndOfFrame)
        }

        Destroy(gameObject);
    }

    public void Kill()
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
        if (currentHealth <= 0) return;

        // Skontrolujeme, Ëi objekt, do ktorÈho sme narazili, m· komponent PlayerStats.
        if (collision.collider.TryGetComponent(out PlayerStats p))
        {
            Vector2 contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            p.TakeDamage(Actual.damage, contactPoint);
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
