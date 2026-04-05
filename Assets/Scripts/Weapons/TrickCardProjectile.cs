using UnityEngine;

public class TrickCardProjectile : Projectile
{
    [Header("Trick Card Timers")]
    public float flyTime = 0.4f;        // Ako dlho karta letí od hráèa
    public float spinTime = 1.0f;       // Ako dlho stojí na mieste a zraòuje
    public float returnSpeedMultiplier = 1.5f; // Zrıchlenie karty pri návrate k hráèovi

    private float stateTimer;
    private int state = 0; // 0 = Vpred, 1 = Rotácia (stojí), 2 = Návrat

    protected override void Start()
    {
        base.Start(); // Nastaví poèiatoènú rıchlos a rotáciu z Projectile.cs
        stateTimer = flyTime;
    }

    void Update()
    {
        if (owner == null) return;

        stateTimer -= Time.deltaTime;

        if (state == 0) // FÁZA 1: Let dopredu
        {
            // Posúva kartu vpred pod¾a jej natoèenia
            float speed = weapon.GetStats().speed * owner.Stats.speed;
            transform.position += transform.right * speed * Time.deltaTime;

            if (stateTimer <= 0)
            {
                state = 1;
                stateTimer = spinTime;
                rb.linearVelocity = Vector2.zero;

                if (rb.bodyType == RigidbodyType2D.Dynamic)
                    rb.angularVelocity *= 2f;
            }
        }
        else if (state == 1) // FÁZA 2: Rotácia na mieste (AoE poškodenie)
        {
            if (stateTimer <= 0)
            {
                state = 2; // Ide sa spä!
            }
        }
        else if (state == 2) // FÁZA 3: Bumerangovı návrat
        {
            Vector2 direction = (owner.transform.position - transform.position).normalized;
            float speed = weapon.GetStats().speed * owner.Stats.speed * returnSpeedMultiplier;

            // Hıbeme kartu manuálne smerom k hráèovi
            transform.position += (Vector3)direction * speed * Time.deltaTime;

            // Znièíme kartu, keï "trafí" (vráti sa) k hráèovi
            if (Vector2.Distance(transform.position, owner.transform.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }
}