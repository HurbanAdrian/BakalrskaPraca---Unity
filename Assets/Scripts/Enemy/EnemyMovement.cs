using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    protected EnemyStats enemy;
    protected Transform player;

    protected Vector2 knockbackVelocity;
    protected float knockbackDuration;

    public enum OutOfFrameAction { none, respawnAtEdge, despawn }
    public OutOfFrameAction outOfFrameAction = OutOfFrameAction.respawnAtEdge;

    protected bool spawnedOutOfFrame = false;

    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);
        enemy = GetComponent<EnemyStats>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Vyberie náhodného hráèa na obrazovke namiesto toho, aby vždy vybral toho prvého. Toto umožòuje podporu pre lokálny multiplayer.
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allPlayers.Length > 0)
        {
            player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
        }
    }

    protected virtual void Update()
    {
        if (knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            Move();
            HandleOutOfFrameAction();
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

        knockbackVelocity = velocity;
        knockbackDuration = duration;
    }

    public virtual void Move()
    {
        // Neustále presúvaj nepriate¾a smerom k hráèovi.
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.currentMoveSpeed * Time.deltaTime);

        if (spriteRenderer != null && player != null)
        {
            // Vypoèítame smer k hráèovi na osi X
            float directionX = player.position.x - transform.position.x;

            // Ak je hráè na¾avo, preklop sprite (flipX = true). Ak je napravo, nepreklápaj (flipX = false).
            // Poznámka: Predpokladáme, že pôvodná animácia zombie smeruje doprava. (directionX > 0) inac
            if (directionX < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (directionX > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

}
