using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aura je efekt poškodenia v èase (damage-over-time), ktorı sa aplikuje na špecifickú oblas v èasovıch intervaloch.
/// Pouíva sa na poskytnutie funkcionality Cesnaku (Garlic) a môe sa poui aj na spawnovanie efektov svätenej vody.
/// </summary>
public class Aura : WeaponEffect
{
    Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetsToUnaffect = new List<EnemyStats>();

    // Update sa volá raz za frame
    void Update()
    {
        Dictionary<EnemyStats, float> affectedTargsCopy = new Dictionary<EnemyStats, float>(affectedTargets);

        // Prejdeme kadı cie¾ ovplyvnenı aurou a zníime mu cooldown aury. Ak cooldown dosiahne 0, udelíme mu poškodenie.
        foreach (KeyValuePair<EnemyStats, float> pair in affectedTargsCopy)
        {
            // Je mozne ze nepriatelia v Liste zomru na inu zbran. Pair.Key bude potom null tak ho odstranime.
            if (!pair.Key)
            {
                targetsToUnaffect.Remove(pair.Key);
                affectedTargets.Remove(pair.Key);
                continue;
            }

            affectedTargets[pair.Key] -= Time.deltaTime;

            // Zmenil som ifko pair.Value <= 0
            if (affectedTargets[pair.Key] <= 0)
            {
                if (targetsToUnaffect.Contains(pair.Key))
                {
                    // Ak je cie¾ oznaèenı na odstránenie, odstránime ho.
                    affectedTargets.Remove(pair.Key);
                    targetsToUnaffect.Remove(pair.Key);
                }
                else
                {
                    // Resetujeme cooldown a udelíme poškodenie.
                    Weapon.Stats stats = weapon.GetStats();
                    affectedTargets[pair.Key] = stats.cooldown * Owner.Stats.cooldown;
                    pair.Key.TakeDamage(GetDamage(), transform.position, stats.knockback);

                    // Ak mame hitEffect tak ho spustit
                    if (stats.hitEffect)
                    {
                        Destroy(Instantiate(stats.hitEffect, pair.Key.transform.position, Quaternion.identity).gameObject, 5f);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            // Ak cie¾ ešte nie je ovplyvnenı touto aurou, pridáme ho do nášho zoznamu ovplyvnenıch cie¾ov.
            if (!affectedTargets.ContainsKey(es))
            {
                // Vdy zaèína s intervalom 0, aby dostal poškodenie hneï v ïalšom tiku Update().
                affectedTargets.Add(es, 0);
            }
            else
            {
                if (targetsToUnaffect.Contains(es))
                {
                    targetsToUnaffect.Remove(es);
                }
            }
        }
        else if (other.TryGetComponent(out BreakableProps prop))
        {
            // Udelíme mu poškodenie okamite a nepotrebujeme ho dáva do zoznamu
            prop.TakeDamage(GetDamage());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyStats es))
        {
            // Neodstraòujeme cie¾ priamo pri opustení aury, pretoe stále musíme sledova jeho cooldowny.
            if (affectedTargets.ContainsKey(es))
            {
                targetsToUnaffect.Add(es);
            }
        }
    }
}
