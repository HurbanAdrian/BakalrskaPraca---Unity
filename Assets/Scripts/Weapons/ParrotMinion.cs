using System.Collections.Generic;
using UnityEngine;

public class ParrotMinion : Projectile
{
    public enum ParrotState { Orbiting, Attacking, Returning }
    public ParrotState currentState = ParrotState.Orbiting;

    [Header("Parrot Settings")]
    public float orbitDistance = 1.5f;

    [Tooltip("Ak˝ veæk˝ okruh okolo hr·Ëa m· papag·j skenovaù pre ciele.")]
    public float searchRadius = 15f;

    [Tooltip("Spomalenie letu papag·ja (napr. 0.5 = poloviËn· r˝chlosù).")]
    public float speedMultiplier = 1f;

    [HideInInspector] public float currentAngle = 0f;

    private EnemyStats target;
    private float attackCooldownTimer;
    private float attackTimeoutTimer = 0f;

    // NOV…: Uklad·me si, koæko cieæov eöte mÙûe trafiù pred n·vratom
    private int currentPierces;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        attackCooldownTimer = 0f;
    }

    protected override void FixedUpdate()
    {
        // Nech·vame pr·zdne, aby sme prebili pohyb z Projectile.cs
    }

    void Update()
    {
        if (!owner) return;

        switch (currentState)
        {
            case ParrotState.Orbiting:
                OrbitPlayer();
                attackCooldownTimer -= Time.deltaTime;
                if (attackCooldownTimer <= 0f)
                {
                    FindTargetAndAttack();
                }
                break;

            case ParrotState.Attacking:
                UpdateAttackingState();
                break;

            case ParrotState.Returning:
                UpdateReturningState();
                break;
        }
    }

    void UpdateAttackingState()
    {
        attackTimeoutTimer -= Time.deltaTime;

        // Ak cieæ zmizol (umrel) alebo n·m doöiel Ëas, ale st·le m·me prieraznosù, hæad·me Ôalöieho!
        if (attackTimeoutTimer <= 0f || !target || !target.gameObject.activeInHierarchy)
        {
            if (currentPierces > 0)
            {
                if (!FindNextTarget()) StartReturning();
            }
            else
            {
                StartReturning();
            }
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < 0.5f)
        {
            DamageTarget(target);
            return;
        }

        float flySpeed = weapon.GetStats().speed * owner.Stats.speed * speedMultiplier;
        Vector3 nextPos = Vector3.MoveTowards(transform.position, target.transform.position, flySpeed * Time.deltaTime);
        UpdateMovement(nextPos);
    }

    void UpdateReturningState()
    {
        OrbitAngleUpdate();
        Vector3 targetOrbitPos = GetOrbitPosition();
        float distanceToOrbit = Vector3.Distance(transform.position, targetOrbitPos);

        float baseReturnSpeed = weapon.GetStats().speed * owner.Stats.speed * speedMultiplier;
        float minCatchUpSpeed = owner.Stats.speed * 2f;
        float finalReturnSpeed = Mathf.Max(baseReturnSpeed, minCatchUpSpeed);

        if (distanceToOrbit > 5f)
        {
            finalReturnSpeed *= (distanceToOrbit / 3f);
        }

        Vector3 nextRetPos = Vector3.MoveTowards(transform.position, targetOrbitPos, finalReturnSpeed * Time.deltaTime);
        UpdateMovement(nextRetPos);

        if (distanceToOrbit < 0.5f)
        {
            currentState = ParrotState.Orbiting;
        }
    }

    void OrbitPlayer()
    {
        OrbitAngleUpdate();
        UpdateMovement(GetOrbitPosition());
    }

    void OrbitAngleUpdate()
    {
        float orbitSpeed = weapon.GetStats().speed * speedMultiplier;
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > Mathf.PI * 2) currentAngle -= Mathf.PI * 2;
    }

    Vector3 GetOrbitPosition()
    {
        return owner.transform.position + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * orbitDistance;
    }

    void UpdateMovement(Vector3 nextPosition)
    {
        transform.position = nextPosition;
    }

    // --- UPRAVEN¡ LOGIKA ⁄TOKU ---
    void FindTargetAndAttack()
    {
        // NaËÌtame si prieraznosù zo statov zbrane (minim·lne 1, aby vûdy za˙toËil aspoÚ na jednÈho)
        currentPierces = Mathf.Max(1, weapon.GetStats().piercing);

        if (FindNextTarget())
        {
            currentState = ParrotState.Attacking;
        }
        else
        {
            attackCooldownTimer = 0.5f; // Nikto nie je na blÌzku
        }
    }

    bool FindNextTarget()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        List<EnemyStats> validEnemies = new List<EnemyStats>();

        foreach (Collider2D col in hitColliders)
        {
            EnemyStats enemy = col.GetComponent<EnemyStats>();
            // N·jdeme niekoho, kto ûije a KTO NIE JE N¡ä PREDCH¡DZAJ⁄CI CIEº (aby sa nezasekol na jednom)
            if (enemy && enemy.gameObject.activeInHierarchy && enemy != target)
            {
                validEnemies.Add(enemy);
            }
        }

        if (validEnemies.Count > 0)
        {
            target = validEnemies[Random.Range(0, validEnemies.Count)];
            attackTimeoutTimer = 3f;
            return true;
        }

        return false; // Nenaöli sme ûiadny ÔalöÌ cieæ
    }

    void DamageTarget(EnemyStats es)
    {
        Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;
        es.TakeDamage(GetDamage(), source);
        weapon.ApplyBuffs(es);

        Weapon.Stats stats = weapon.GetStats();
        if (stats.hitEffect)
        {
            Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity).gameObject, 5f);
        }

        // ZnÌûime poËet zost·vaj˙cich hitov
        currentPierces--;

        // Ak mÙûe trafiù eöte niekoho, rovno hæad·me ÔalöÌ cieæ (reùazov˝ ˙tok!)
        if (currentPierces > 0)
        {
            if (!FindNextTarget())
            {
                StartReturning();
            }
        }
        else
        {
            StartReturning();
        }
    }

    void StartReturning()
    {
        currentState = ParrotState.Returning;
        // Cooldown zaËÌna plyn˙ù aû keÔ skonËÌ cel˙ svoju ˙toËn˙ reùaz
        attackCooldownTimer = weapon.GetStats().cooldown * owner.Stats.cooldown;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != ParrotState.Attacking) return;

        EnemyStats es = other.GetComponent<EnemyStats>();

        // Ak poËas letu k cieæu n·hodou narazÌ do IN…HO nepriateæa, taktieû mu ublÌûi!
        if (es && es.gameObject.activeInHierarchy)
        {
            DamageTarget(es);
        }
    }
}