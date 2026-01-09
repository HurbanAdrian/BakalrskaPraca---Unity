using UnityEngine;

public class GarlicController : WeaponController
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
        GameObject spawnedGarlic = Instantiate(weaponData.Prefab);
        spawnedGarlic.transform.position = transform.position;       // nastavime na rovnaku poziciu ako tento objekt co je hrac (ktory je parented na hraca)
        spawnedGarlic.transform.parent = transform;          // tak aby sa spawnol pod tymto objektom
    }
}
