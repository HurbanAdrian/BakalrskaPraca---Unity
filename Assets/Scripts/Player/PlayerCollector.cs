using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D detector;
    public float pullSpeed = 10;

    public delegate void OnCoinCollected();
    public OnCoinCollected onCoinCollected;

    float coins;
    public UICoinDisplay ui;

    void Start()
    {
        player = GetComponentInParent<PlayerStats>();         // Collector je dieta hraca
        coins = 0;
    }

    public void SetRadius(float r)
    {
        if (!detector)
        {
            detector = GetComponent<CircleCollider2D>();
        }

        detector.radius = r;
    }

    public float GetCoins() { return coins; }
    // Updatne Display a informacie
    public float AddCoins(float amount)
    {
        float greedMultiplier = player.Actual.greed;

        // Ak je greed kladný, peniaze pribudnú. Ak je záporný (napr. -1), peniaze sa odpoèítajú!
        coins += amount * greedMultiplier;

        if (coins < 0)
        {
            coins = 0;
        }

        if (onCoinCollected != null) onCoinCollected();
        return coins;
    }

    // Uloží nazbierané mince do trvalého úložiska (stasha).
    public void SaveCoinsToStash()
    {
        SaveManager.LastLoadedGameData.coins += coins;
        coins = 0;
        SaveManager.Save();
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
