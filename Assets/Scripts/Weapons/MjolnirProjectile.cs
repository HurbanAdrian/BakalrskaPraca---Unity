using UnityEngine;

public class MjolnirProjectile : Projectile
{
    [Header("Mjolnir Flight Timers")]
    public float flyTime = 0.5f;        // Ako dlho letí dopredu
    public float spinTime = 0.2f;       // Krátka pauza, kedy rotuje na mieste
    public float returnSpeedMultiplier = 1.5f; // Rýchlejší návrat k hráèovi

    [Header("Bobbing (Kmitanie vo vzduchu)")]
    public float bobFrequency = 15f;    // Ako rýchlo sa vlní hore-dole
    public float bobAmplitude = 3f;     // Ako ïaleko do strán sa vychýli

    private float stateTimer;
    private int state = 0; // 0 = Vpred, 1 = Pauza, 2 = Návrat k hráèovi
    private float flightTimer = 0f;     // Èasovaè pre plynulú sínusoidu

    private Vector3 initialForward;
    private Vector3 initialUp;

    protected override void Start()
    {
        base.Start();
        stateTimer = flyTime;

        // Uložíme si pôvodný smer letu
        initialForward = transform.right;

        // Vypoèítame kolmicu na smer letu (aby kladivo kmitalo "do bokov" svojej dráhy)
        initialUp = new Vector3(-initialForward.y, initialForward.x, 0).normalized;
    }

    void Update()
    {
        if (owner == null) return;

        stateTimer -= Time.deltaTime;
        flightTimer += Time.deltaTime; // Neustále bežiaci èas pre výpoèet vlny

        if (state == 0) // FÁZA 1: Let vpred s kmitaním
        {
            float speed = weapon.GetStats().speed * owner.Stats.speed;

            // Smer dopredu (normálny let)
            Vector3 forwardMove = initialForward * speed;

            // Kolmý smer (kmitanie hore/dole pod¾a sínusoidy)
            Vector3 bobMove = initialUp * Mathf.Cos(flightTimer * bobFrequency) * bobAmplitude;

            transform.position += (forwardMove + bobMove) * Time.deltaTime;

            if (stateTimer <= 0)
            {
                state = 1;
                stateTimer = spinTime;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (state == 1) // FÁZA 2: Krátka pauza (stojí a rotuje)
        {
            if (stateTimer <= 0)
            {
                state = 2; // Èas vypršal, ide sa spä!

                transform.Rotate(0, 0, 180f);

                // (Toto tu môže osta, ak by si mu predsa len niekedy pridal rotationSpeed)
                rotationSpeed = -rotationSpeed;
            }
        }
        else if (state == 2) // FÁZA 3: Návrat k hráèovi s kmitaním
        {
            Vector2 direction = (owner.transform.position - transform.position).normalized;
            float speed = weapon.GetStats().speed * owner.Stats.speed * returnSpeedMultiplier;

            // Aj pri návrate chceme aby kmitalo, vypoèítame novú kolmicu
            Vector3 returnUp = new Vector3(-direction.y, direction.x, 0).normalized;

            Vector3 forwardMove = (Vector3)direction * speed;
            Vector3 bobMove = returnUp * Mathf.Cos(flightTimer * bobFrequency) * bobAmplitude;

            transform.position += (forwardMove + bobMove) * Time.deltaTime;

            // Keï sa vráti k hráèovi, znièí sa
            if (Vector2.Distance(transform.position, owner.transform.position) < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }

    // Zabezpeèuje samotnú vizuálnu rotáciu (toèenie) kladiva
    protected override void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // --- ŠPECIÁLNA MECHANIKA PRI NÁRAZE (Blesky) ---
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats es = other.GetComponent<EnemyStats>();
        BreakableProps p = other.GetComponent<BreakableProps>();

        if (es || p)
        {
            Weapon.Stats stats = weapon.GetStats();
            Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;

            // 1. Zásah cie¾a priamo kladivom
            if (es)
            {
                es.TakeDamage(GetDamage(), source);
                weapon.ApplyBuffs(es);
            }
            else if (p)
            {
                p.TakeDamage(GetDamage());
            }

            // 2. PRIVOLANIE BLESKU A PLOŠNÝ (AoE) DAMAGE
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, other.transform.position, Quaternion.identity).gameObject, 2f);
            }

            float aoeRadius = weapon.GetArea();
            Collider2D[] targets = Physics2D.OverlapCircleAll(other.transform.position, aoeRadius);

            foreach (Collider2D t in targets)
            {
                EnemyStats aoeEnemy = t.GetComponent<EnemyStats>();
                // Udelíme plošný damage všetkým okolo, OKREM toho, ktorého sme práve trafili priamo
                if (aoeEnemy && aoeEnemy != es)
                {
                    // Blesk dáva plošný damage (napr. 50% z pôvodného úderu kladiva)
                    aoeEnemy.TakeDamage(GetDamage() * 0.5f, other.transform.position);
                    weapon.ApplyBuffs(aoeEnemy);
                }
            }

            // 3. Piercing
            piercing--;
            if (piercing <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}