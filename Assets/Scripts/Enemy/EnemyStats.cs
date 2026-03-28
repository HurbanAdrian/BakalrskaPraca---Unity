using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    // Sucastne staty
    public float currentMoveSpeed;
    public float currentHealth;
    public float currentDamage;

    Transform player;

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
        player = FindAnyObjectByType<PlayerStats>().transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        movement = GetComponent<EnemyMovement>();
    }

    void Awake()
    {
        count++;
    }

    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        currentHealth -= dmg;
        StartCoroutine(DamageFlash());

        // Vytvorime text pop up
        if (dmg > 0)
        {
            GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(), transform);
        }

        if (knockbackForce > 0)
        {
            // Ziskanie smeru knockbacku
            Vector2 knockbackDirection = (Vector2)transform.position - sourcePosition;
            movement.Knockback(knockbackDirection.normalized * knockbackForce, knockbackDuration);
        }

        if (currentHealth <= 0)
        {
            Kill();
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
        // Kontrola kolizie a referencia na poskodenie pouzitim TakeDamage() metody v PlayerStats
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.TakeDamage(currentDamage, collision.GetContact(0).point);       // berieme prvy contact point z kolizie na lokaciu
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
