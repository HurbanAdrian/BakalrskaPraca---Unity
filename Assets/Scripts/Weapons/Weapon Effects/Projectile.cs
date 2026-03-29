using UnityEngine;
/// <summary>
/// Komponent priradeny vsetkym Projectile prefabom. Vsetky spawnute projektily budu letiet smerom,
/// ktorym je hrac otoceny a sposobia poskodenie ked trafia nejaky objekt.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : WeaponEffect
{
    public enum DamageSource { projectile, owner };
    public DamageSource damageSource = DamageSource.projectile;
    public bool hasAutoAim = false;
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    protected Rigidbody2D rb;
    protected int piercing = 0;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Weapon.Stats stats = weapon.GetStats();
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.angularVelocity = rotationSpeed.z;
            rb.linearVelocity = transform.right * stats.speed * weapon.Owner.Stats.speed;
        }

        // Zabrani aby area bola 0, nakolko to schova projektil
        float area = weapon.GetArea();
        if (area <= 0)
        {
            area = 1;
        }
        transform.localScale = new Vector3(
            area * Mathf.Sign(transform.localScale.x),
            area * Mathf.Sign(transform.localScale.y),
            1
            );

        piercing = stats.piercing;

        // Znicenie projektilu po uplynuti jeho zivotnosti
        if (stats.lifeSpan > 0) 
        { 
            Destroy(gameObject, stats.lifeSpan);
        }

        // Ak ma projektil autoaim, automaticky najdi vhodneho nepriatela
        if (hasAutoAim)
        {
            AcquireAutoAimFacing();
        }
    }

    // Ak je projektil navadzany, ziskaj smer k nepriatelovi ku ktoremu sa bude pohybovat
    public virtual void AcquireAutoAimFacing()
    {
        float aimAngle;

        EnemyStats[] targets = FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // Zvol nahodneho nepriatela. Inac zvol nahodny uhol.
        if (targets.Length > 0)
        {
            EnemyStats selectedTarget = targets[Random.Range(0, targets.Length)];
            Vector2 difference = selectedTarget.transform.position - transform.position;
            aimAngle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        }
        else
        {
            aimAngle = Random.Range(0, 360);
        }

        // Namierime projektil smerom kde mierime
        transform.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

    // Update je volany raz za snimku
    protected virtual void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Weapon.Stats stats = weapon.GetStats();
            transform.position += transform.right * stats.speed * weapon.Owner.Stats.speed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position);
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats es = other.GetComponent<EnemyStats>();
        BreakableProps p = other.GetComponent<BreakableProps>();

        // Kolizia iba s nepriatelmi alebo rozbitnymi objektami.
        if (es)
        {
            // Ak existuje owner a zdroj poskodenia je nastaveny na ownera, vypocitame knockback pomocou pozicie ownera namiesto projektilu.
            Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;

            es.TakeDamage(GetDamage(), source);

            Weapon.Stats stats = weapon.GetStats();

            weapon.ApplyBuffs(es);

            piercing--;
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity).gameObject, 5f);
            }
        }
        else if (p)
        {
            p.TakeDamage(GetDamage());
            piercing--;

            Weapon.Stats stats = weapon.GetStats();
            if (stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity).gameObject, 5f);
            }
        }

        // Znicenie projektilu ak uz nema ziadnu prieraznost.
        if (piercing <= 0)
        {
            Destroy(gameObject);
        }
    }
}
