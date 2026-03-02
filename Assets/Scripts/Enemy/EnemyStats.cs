using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    public EnemyScriptableObject enemyData;

    // Sucastne staty
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentDamage;

    public float despawnDistance = 20;
    Transform player;
    EnemySpawner spawner;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1f, 0f, 0f);
    public float damageFlashDuration = 0.2f;
    public float deathFadeTime = 0.6f;
    Color originalColor;
    SpriteRenderer spriteRenderer;
    EnemyMovement movement;

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
        spawner = FindAnyObjectByType<EnemySpawner>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        movement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            ReturnEnemy();
        }
    }

    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentHealth = enemyData.MaxHealth;
        currentDamage = enemyData.Damage;
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
        Color startColor = spriteRenderer.color; // Použijeme aktuálnu farbu (aj ak je èervená po damage)

        while (timer < deathFadeTime)
        {
            timer += Time.deltaTime;

            // Lerp je èistejší spôsob ako poèíta prechod
            float newAlpha = Mathf.Lerp(startColor.a, 0f, timer / deathFadeTime);

            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null; // Èakáme na ïalší frame (netreba WaitForEndOfFrame)
        }

        Destroy(gameObject);
    }

    public void Kill()
    {
        // 1. Zastavíme DamageFlash, aby neprepísal farbu spä
        StopAllCoroutines();

        // 2. Vypneme kolízie, aby do neho hráè nemohol narazi
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Vypneme pohyb
        if (movement != null) movement.enabled = false;
        // 3. Vypneme skript, aby sa nevykonával Update (napr. pohyb)
        // Pozor: Toto zastaví Update(), ale Coroutiny na tomto objekte bežia ïalej
        this.enabled = false;

        // 4. Spustíme animáciu smrti
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

        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }
    }

    void ReturnEnemy()
    {
        if (spawner != null && player != null)
        {
            // Zastavíme všetky efekty (ak ho práve niekto knockbackol alebo bliká)
            StopAllCoroutines();

            // Vrátime farbu na pôvodnú (aby neostal zaseknutý damageColor)
            spriteRenderer.color = originalColor;

            // Presun
            transform.position = player.position + spawner.relativeSpawnPoints[Random.Range(0, spawner.relativeSpawnPoints.Count)].position;
        }
    }

}
