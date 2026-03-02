using UnityEngine;

// Base script for all projectile weapon behaviour [To be placed on a prefab of a weapon that is a projectile]
public class ProjectileWeaponBehaviour : MonoBehaviour
{
    public WeaponScriptableObject weaponData;
    protected Vector3 direction;
    public float destroyAfterSeconds;
    public float spriteAngleOffset;      // offset to correctly align the sprite with the movement direction to treba vyriesit podla toho ako mas nakresleny sprite

    // Sucastne staty
    protected float currentDamage;
    protected float currentSpeed;
    protected float currentCooldownDuration;
    protected int currentPierce;

    protected PlayerStats playerStats;

    void Awake()
    {
        currentDamage = weaponData.Damage;
        currentSpeed = weaponData.Speed;
        currentCooldownDuration = weaponData.CooldownDuration;
        currentPierce = weaponData.Pierce;
    }

    public float getCurrentDamage() 
    {
        // Ak sme nahodou hraca nenasli v Start (alebo Start este nebezal), najdeme ho teraz
        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }

        if (playerStats != null)
        {
            return currentDamage * playerStats.CurrentMight;
        }

        return currentDamage;
    }

    protected virtual void Start()
    {
        // Najdi hraca len raz pri vytvoreni projektilu
        playerStats = FindAnyObjectByType<PlayerStats>();

        Destroy(gameObject, destroyAfterSeconds);
    }


    public void DirectionChecker(Vector3 dir)
    {
        direction = dir;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += spriteAngleOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = collision.GetComponent<EnemyStats>();
            enemyStats.TakeDamage(getCurrentDamage(), transform.position);
            ReducePierce();
        }
        else if (collision.CompareTag("Prop"))
        {
            if (collision.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(getCurrentDamage());
                ReducePierce();
            }
        }
    }

    void ReducePierce() 
    {
        currentPierce--;
        if (currentPierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}
