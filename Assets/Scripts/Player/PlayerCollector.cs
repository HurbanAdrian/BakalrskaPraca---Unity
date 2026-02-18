using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D collectorCollider;
    public float pullSpeed;

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>();
        collectorCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        collectorCollider.radius = player.currentMagnet;     // nastavime radius kolidera na zaklade magnet statov hraca
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Skontroluj, ci objekt, s ktorým sa hráè stretol, implementuje rozhranie ICollectible
        if (collision.gameObject.TryGetComponent(out ICollectible collectible))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            Vector2 forceDirection = (transform.position - collision.transform.position).normalized;   // vypocitame smer sily od hraca k objektu
            rb.AddForce(forceDirection * pullSpeed, ForceMode2D.Force);    // aplikujeme silu na objekt, aby sa priblizil k hracovi, je jedno ci dame ForceMode2D.Force, defaultne tam je tak

            // Ak áno, zavolaj metódu Collect() na tomto objekte
            collectible.Collect();
        }
    }
}
