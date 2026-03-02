using UnityEngine;

// Base script for all melee weapon behaviors [To be placed on a prefab a a weapon that is melee type]
public class MeleeWeaponBehaviour : MonoBehaviour
{
    public WeaponScriptableObject weaponData;
    public float destroyAfterSeconds;

    // Sucastne staty
    protected float currentDamage;
    protected float currentSpeed;
    protected float currentCooldownDuration;
    protected float currentPierce;

    protected PlayerStats playerStats;

    void Awake()
    {
        currentDamage = weaponData.Damage;
        currentCooldownDuration = weaponData.CooldownDuration;
        currentSpeed = weaponData.Speed;
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

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = collision.GetComponent<EnemyStats>();
            enemyStats.TakeDamage(getCurrentDamage(), transform.position);
        }
        else if (collision.CompareTag("Prop"))
        {
            if (collision.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(getCurrentDamage());
            }
        }
    }
}
