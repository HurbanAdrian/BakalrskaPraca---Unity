using UnityEngine;

public class PlayerCollector : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Skontroluj, ci objekt, s ktorým sa hráè stretol, implementuje rozhranie ICollectible
        if (collision.gameObject.TryGetComponent(out ICollectible collectible))
        {
            // Ak áno, zavolaj metódu Collect() na tomto objekte
            collectible.Collect();
        }
    }
}
