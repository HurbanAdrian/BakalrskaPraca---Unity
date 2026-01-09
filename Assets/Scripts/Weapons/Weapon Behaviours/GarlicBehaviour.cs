using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GarlicBehaviour : MeleeWeaponBehaviour
{
    List<GameObject> markedEnemies;
    protected override void Start()
    {
        base.Start();
        markedEnemies = new List<GameObject>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!markedEnemies.Contains(collision.gameObject))
            {
                markedEnemies.Add(collision.gameObject);

                EnemyStats enemyStats = collision.GetComponent<EnemyStats>();
                enemyStats.TakeDamage(currentDamage);
            }
        }
    }
}
