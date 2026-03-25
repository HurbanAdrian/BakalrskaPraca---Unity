using UnityEngine;

public class ProjectileWeapon : Weapon
{
    protected float currentAttackInterval;
    protected int currentAttackCount;       // kolko krat nastane tento utok

    protected override void Update()
    {
        base.Update();

        if (currentAttackInterval > 0)
        {
            currentAttackInterval -= Time.deltaTime;
            if (currentAttackInterval <= 0)
            {
                Attack(currentAttackCount);
            }
        }
    }

    public override bool CanAttack()
    {
        if (currentAttackCount > 0)
        {
            return true;
        }

        return base.CanAttack();
    }

    protected override bool Attack(int attackCount = 1)
    {
        // Idiot proofing treba pre niekoho (mna)
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning($"Weapon {name} has no projectile prefab assigned.");
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        if (!CanAttack())
        {
            return false;
        }

        float spawnAngle = GetSpawnAngle();

        // Ak existuje procEffect, tak ho spustime na hracovi
        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        Projectile prefab = Instantiate(
            currentStats.projectilePrefab,
            owner.transform.position + (Vector3)GetSpawnOffset(spawnAngle),
            Quaternion.Euler(0, 0, spawnAngle)
            );

        prefab.weapon = this;
        prefab.owner = owner;

        if (currentCooldown <= 0)
        {
            currentCooldown += currentStats.cooldown;
        }

        attackCount--;

        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInterval;
        }

        return true;
    }

    // Vrati Uhol v ktorom sa spawne projektil
    protected virtual float GetSpawnAngle()
    {
        return Mathf.Atan2(movement.lastMovedVector.y, movement.lastMovedVector.x) * Mathf.Rad2Deg;
    }

    // Vygeneruje nahodny bod na spawnutie projektilu
    protected virtual Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        return Quaternion.Euler(0, 0, spawnAngle) * new Vector2(
            Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            Random.Range(currentStats.spawnVariance.yMin, currentStats.spawnVariance.yMax)
            );
    }
}
