using UnityEngine;

public class KnifeController : WeaponController
{

    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack() 
    {
        base.Attack();
        GameObject spawnedKnife = Instantiate(weaponData.Prefab);
        spawnedKnife.transform.position = transform.position;       // nastavime na rovnaku poziciu ako tento objekt co je hrac (ktory je parented na hraca)
        spawnedKnife.GetComponent<KnifeBehaviour>().DirectionChecker(pm.lastMovedVector);      // nastavime smer noza podla pohybu hraca cez referenciu
    }
}
