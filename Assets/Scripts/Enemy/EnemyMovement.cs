using UnityEngine;

public class EnemyMovement : Sortable
{
    protected EnemyStats stats;
    protected Transform player;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOfFrameAction { none, respawnAtEdge, despawn }
    public OutOfFrameAction outOfFrameAction = OutOfFrameAction.respawnAtEdge;

    protected bool spawnedOutOfFrame = false;

    [System.Flags]
    public enum KnockbackVariance { duration = 1, velocity = 2 }
    public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Vyberie náhodného hráèa na obrazovke namiesto toho, aby vždy vybral toho prvého. Toto umožòuje podporu pre lokálny multiplayer.
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allPlayers.Length > 0)
        {
            player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (knockbackDuration > 0)
        {
            if (rb == null)
            {
                transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            }

            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            if (rb == null)
            {
                Move();
            }
            HandleOutOfFrameAction();
        }
    }

    protected virtual void FixedUpdate()
    {
        // Ak MÁME fyziku, všetok pohyb riešime v 50 FPS FixedUpdate cykle
        if (rb != null)
        {
            if (knockbackDuration > 0)
            {
                rb.linearVelocity = knockbackVelocity;
            }
            else
            {
                // Voláme tvoju funkciu Move(), ale tentokrát bezpeène vo vnútri FixedUpdate!
                Move();
            }
        }
    }

    // Ak nepriate¾ vypadne z rámca (mimo kameru), spracuj to.
    protected virtual void HandleOutOfFrameAction()
    {
        // Riešenie situácie, keï je nepriate¾ mimo rámca.
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOfFrameAction)
            {
                case OutOfFrameAction.none:
                default:
                    break;

                case OutOfFrameAction.respawnAtEdge:
                    // Ak je nepriate¾ mimo rámca kamery, teleportuj ho spä na okraj rámca.
                    transform.position = SpawnManager.GeneratePosition();
                    break;

                case OutOfFrameAction.despawn:
                    // Neniè nepriate¾a, ak bol pôvodne vytvorený (spawnutý) mimo rámca.
                    if (!spawnedOutOfFrame)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }
        else spawnedOutOfFrame = false;     // Nepriatel bol videny kamerou
    }

    public virtual void Knockback(Vector2 velocity, float duration)
    {
        if (knockbackDuration > 0)
        {
            return;
        }

        // Ignoruj zmeny knockbacku, ak je typ variability nastavený na none (0).
        if (knockbackVariance == 0) return;

        // Faktor zmeny zmeníme len ak multiplier nie je 0 alebo 1.
        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0;
        bool reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;

        if (reducesVelocity && reducesDuration) pow = 0.5f;

        // Skontroluj, ktoré hodnoty knockbacku majú by ovplyvnené štatistikami (multiplierom).
        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
    }

    public virtual void Move()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        // Ak existuje rigidbody, použi ho na pohyb namiesto priameho posúvania pozície (transform). Optimalizacia vykonu
        if (rb)
        {
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime)
            );
            //rb.linearVelocity = direction * stats.Actual.moveSpeed;
        }
        else
        {
            // Neustále presúvaj nepriate¾a smerom k hráèovi (cez transform).
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
            );
        }

        if (spriteRenderer != null && player != null)
        {
            // Vypoèítame smer k hráèovi na osi X
            float directionX = player.position.x - transform.position.x;

            // Ak je hráè na¾avo, preklop sprite (flipX = true). Ak je napravo, nepreklápaj (flipX = false).
            // Poznámka: Predpokladáme, že pôvodná animácia zombie smeruje doprava. (directionX > 0) inac -> 0,1 aby nenastal flickering v hordach
            if (directionX < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (directionX > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

}
