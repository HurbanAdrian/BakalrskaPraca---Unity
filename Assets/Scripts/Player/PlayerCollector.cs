using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;
    public float pullSpeed;

    void Start()
    {
        player = GetComponentInParent<PlayerStats>();         // Collector je dieta hraca
    }

    public void SetRadius(float r)
    {
        if (!detector)
        {
            detector = GetComponent<CircleCollider2D>();
        }

        detector.radius = r;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Skontroluj, èi je GameObject typu Pickup.
        if (collision.TryGetComponent(out PickUp p))
        {
            p.Collect(player, pullSpeed);
        }
    }
}
