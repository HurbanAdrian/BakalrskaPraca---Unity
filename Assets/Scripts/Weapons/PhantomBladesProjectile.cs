using UnityEngine;

public class PhantomBladesProjectile : Projectile
{
    [Header("Phantom Blades Timers")]
    [Tooltip("Ako dlho meè stojí vo vzduchu, kım vystrelí (v sekundách)")]
    public float hoverTime = 0.4f;

    private float hoverTimer;
    private bool isShooting = false;
    private Vector3 shootDirection;

    protected override void Start()
    {
        base.Start(); // Zavolá základnı Start (nastaví Scale pod¾a Area, Piercing, atï.)

        hoverTimer = hoverTime;

        // Uloíme si smer, kam meè mieri, aby letel správne
        shootDirection = transform.right;

        // Pretoe base.Start() nastavil lineárnu rıchlos pre Dynamic objekty, my ju vynulujeme
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Prepíšeme základnı pohyb
    protected override void FixedUpdate()
    {
        if (!isShooting)
        {
            // Fáza 1: Nabíjanie / Státie na mieste
            hoverTimer -= Time.fixedDeltaTime;
            if (hoverTimer <= 0)
            {
                isShooting = true; // Èas vypršal, ide sa strie¾a!
            }
        }
        else
        {
            // Fáza 2: Bleskovı vıstrel dopredu
            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                Weapon.Stats stats = weapon.GetStats();
                // Rıchlos je ovplyvnená aj rıchlosou samotného hráèa
                float actualSpeed = stats.speed * weapon.Owner.Stats.speed;

                transform.position += shootDirection * actualSpeed * Time.fixedDeltaTime;
                rb.MovePosition(transform.position);
            }
        }
    }
}