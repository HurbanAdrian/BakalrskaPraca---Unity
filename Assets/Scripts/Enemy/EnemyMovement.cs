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

    [Header("Obstacle Avoidance")]
    [Tooltip("Vrstva, na ktorej sa nach·dzaj˙ stromy a prek·ûky.")]
    public LayerMask obstacleLayer;
    [Tooltip("Ako Ôaleko pred seba nepriateæ pozer·.")]
    public float obstacleCheckDistance = 0.5f;
    [Tooltip("Ak˝ hrub˝ je l˙Ë hæadania prek·ûky (odpor˙Ëame polomer nepriateæa).")]
    public float avoidanceRadius = 0.25f;

    protected override void Start()
    {
        base.Start();
        spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);
        stats = GetComponent<EnemyStats>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Vyberie n·hodnÈho hr·Ëa na obrazovke namiesto toho, aby vûdy vybral toho prvÈho. Toto umoûÚuje podporu pre lok·lny multiplayer.
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
        // Ak M¡ME fyziku, vöetok pohyb rieöime v 50 FPS FixedUpdate cykle
        if (rb != null)
        {
            if (knockbackDuration > 0)
            {
                rb.linearVelocity = knockbackVelocity;
            }
            else
            {
                // Vol·me tvoju funkciu Move(), ale tentokr·t bezpeËne vo vn˙tri FixedUpdate!
                Move();
            }
        }
    }

    // Ak nepriateæ vypadne z r·mca (mimo kameru), spracuj to.
    protected virtual void HandleOutOfFrameAction()
    {
        // Rieöenie situ·cie, keÔ je nepriateæ mimo r·mca.
        if (!SpawnManager.IsWithinBoundaries(transform))
        {
            switch (outOfFrameAction)
            {
                case OutOfFrameAction.none:
                default:
                    break;

                case OutOfFrameAction.respawnAtEdge:
                    // Ak je nepriateæ mimo r·mca kamery, teleportuj ho sp‰ù na okraj r·mca.
                    transform.position = SpawnManager.GeneratePosition();
                    break;

                case OutOfFrameAction.despawn:
                    // NeniË nepriateæa, ak bol pÙvodne vytvoren˝ (spawnut˝) mimo r·mca.
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

        // Ignoruj zmeny knockbacku, ak je typ variability nastaven˝ na none (0).
        if (knockbackVariance == 0) return;

        // Faktor zmeny zmenÌme len ak multiplier nie je 0 alebo 1.
        float pow = 1;
        bool reducesVelocity = (knockbackVariance & KnockbackVariance.velocity) > 0;
        bool reducesDuration = (knockbackVariance & KnockbackVariance.duration) > 0;

        if (reducesVelocity && reducesDuration) pow = 0.5f;

        // Skontroluj, ktorÈ hodnoty knockbacku maj˙ byù ovplyvnenÈ ötatistikami (multiplierom).
        knockbackVelocity = velocity * (reducesVelocity ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
        knockbackDuration = duration * (reducesDuration ? Mathf.Pow(stats.Actual.knockbackMultiplier, pow) : 1);
    }

    public virtual void Move()
    {
        if (player == null) return;

        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;
        Vector2 directionToPlayer = ((Vector2)player.position - currentPos).normalized;
        Vector2 movementDirection = directionToPlayer;

        // RAYCAST (CIRCLECAST) AVOIDANCE
        // VystrelÌme pred seba kruh, aby sme zistili, Ëi tam nie je prek·ûka
        RaycastHit2D hit = Physics2D.CircleCast(currentPos, avoidanceRadius, directionToPlayer, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // ZistÌme kolmicu plochy, do ktorej sme narazili (smer, ktor˝m sa d· kÂzaù po prek·ûke)
            Vector2 avoidDirection = Vector2.Perpendicular(hit.normal).normalized;

            // Dot product n·m povie, Ëi ideme spr·vnym smerom (bliûöie k hr·Ëovi). Ak ideme na opaËn˙ stranu, otoËÌme to.
            if (Vector2.Dot(avoidDirection, directionToPlayer) < 0)
            {
                avoidDirection = -avoidDirection;
            }

            // Namieöame pÙvodn˝ smer k hr·Ëovi a obch·dzacÌ smer (obch·dzacÌ m· v‰Ëöiu prioritu, preto * 2f)
            movementDirection = (directionToPlayer + avoidDirection * 2f).normalized;
        }

        // Pohybujeme objektom nov˝m vypoËÌtan˝m smerom (uû nie cez MoveTowards, pretoûe ten chce konkrÈtny bod, my teraz menÌme "smer")
        if (rb)
        {
            rb.MovePosition(rb.position + movementDirection * (stats.Actual.moveSpeed * Time.deltaTime));
        }
        else
        {
            transform.position = (Vector2)transform.position + movementDirection * (stats.Actual.moveSpeed * Time.deltaTime);
        }

        if (spriteRenderer != null && player != null)
        {
            // VypoËÌtame smer k hr·Ëovi na osi X
            float directionX = player.position.x - transform.position.x;

            // Ak je hr·Ë naæavo, preklop sprite (flipX = true). Ak je napravo, neprekl·paj (flipX = false).
            // Pozn·mka: Predpoklad·me, ûe pÙvodn· anim·cia zombie smeruje doprava. (directionX > 0) inac -> 0,1 aby nenastal flickering v hordach
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
