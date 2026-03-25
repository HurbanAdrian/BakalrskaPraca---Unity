using UnityEngine;

public class WhipWeapon : ProjectileWeapon
{
    int currentSpawnCount;          // Ko¾kokrát biè zaútoèil v tejto iterácii.
    float currentSpawnYOffset;      // Ak je viac ako 2 bièe, zaèneme ich posúva smerom nahor.

    protected override bool Attack(int attackCount = 1)
    {
        // Ak nie je priradenı iaden prefab projektilu, vypíš varovanie.
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}", name));
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        // Ak nie je moné útoèi, nastav zbraò na cooldown.
        if (!CanAttack()) return false;

        // Ak sa útok spúša prvıkrát, resetujeme currentSpawnCount.
        if (currentCooldown <= 0)
        {
            currentSpawnCount = 0;
            currentSpawnYOffset = 0f;
        }

        // Vypoèítaj uhol a posun nášho vytvoreného projektilu. Potom, ak je <currentSpawnCount> párne (t.j. viac ako 1 projektil), otoèíme smer vytvorenia (spawn-u).
        float spawnDir = Mathf.Sign(movement.lastMovedVector.x) * (currentSpawnCount % 2 != 0 ? -1 : 1);
        Vector2 spawnOffset = new Vector2(
            spawnDir * Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            currentSpawnYOffset
        );

        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        // A vytvor kópiu projektilu.
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab,
            owner.transform.position + (Vector3)spawnOffset,
            Quaternion.identity
        );

        prefab.owner = owner;

        // Otoè sprite projektilu (zrkadlovo).
        if (spawnDir < 0)
        {
            prefab.transform.localScale = new Vector3(
                -Mathf.Abs(prefab.transform.localScale.x),
                prefab.transform.localScale.y,
                prefab.transform.localScale.z
            );
            Debug.Log(spawnDir + " | " + prefab.transform.localScale);
        }

        // Priraï štatistiky.
        prefab.weapon = this;
        currentCooldown = data.baseStats.cooldown;
        attackCount--;

        // Urèi, kde sa má vytvori ïalší projektil.
        currentSpawnCount++;
        if (currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1;
        }

        // Vykonáme ïalší útok?
        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInterval;
        }

        return true;
    }
}
