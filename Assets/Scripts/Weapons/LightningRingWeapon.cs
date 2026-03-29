using System.Collections.Generic;
using UnityEngine;

public class LightningRingWeapon : ProjectileWeapon
{
    List<EnemyStats> allSelectedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        // Ak nie je priradenı prefab zásahu, zanechaj varovnú správu.
        if (!currentStats.hitEffect)
        {
            Debug.LogWarning(string.Format("Hit effect prefab has not been set for {0}", name));
            ActivateCooldown(true);
            currentAttackCount = 0;
            return false;
        }

        // Ak nie je priradenı projektil, nastav zbrani cooldown.
        if (!CanAttack()) return false;

        // Ak je cooldown menší alebo rovnı 0, ide o prvı vıstrel zbrane. Obnov pole vybranıch nepriate¾ov.
        if (currentCooldown <= 0)
        {
            allSelectedEnemies = new List<EnemyStats>(FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
            ActivateCooldown();
            currentAttackCount = attackCount;
        }

        // Nájdi nepriate¾a na mape, do ktorého udrie blesk.
        EnemyStats target = PickEnemy();
        if (target)
        {
            DamageArea(target.transform.position, GetArea(), GetDamage());

            Destroy(Instantiate(currentStats.hitEffect, target.transform.position, Quaternion.identity).gameObject, 5f);
        }

        if (currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform).gameObject, 5f);
        }

        // Ak máme viac ako 1 útok (poèet útokov je väèší ako 0).
        if (attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInterval;
        }
        else
        {
            currentAttackCount = 0;
        }

        return true;
    }

    // Náhodne vyberie nepriate¾a na obrazovke.
    EnemyStats PickEnemy()
    {
        EnemyStats target = null;
        while (!target && allSelectedEnemies.Count > 0)
        {
            int idx = Random.Range(0, allSelectedEnemies.Count);
            target = allSelectedEnemies[idx];

            // Ak je cie¾ u màtvy (znièenı), odstráò ho a preskoè.
            if (!target)
            {
                allSelectedEnemies.RemoveAt(idx);
                continue;
            }

            // Skontroluj, èi je nepriate¾ na obrazovke. Ak nepriate¾ovi chıba Renderer, nemôe by zasiahnutı, pretoe nevieme overi, èi je na obrazovke alebo nie.
            Renderer r = target.GetComponent<Renderer>();
            if (!r || !r.isVisible)
            {
                allSelectedEnemies.Remove(target);
                target = null;
                continue;
            }
        }

        allSelectedEnemies.Remove(target);
        return target;
    }

    // Spôsobuje poškodenie v urèitej oblasti (plošne).
    void DamageArea(Vector2 position, float radius, float damage)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D t in targets)
        {
            EnemyStats es = t.GetComponent<EnemyStats>();
            if (es)
            {
                es.TakeDamage(damage, transform.position);
                ApplyBuffs(es);
            }
        }
    }
}
