using UnityEngine;

public class KnifeBehaviour : ProjectileWeaponBehaviour
{
    protected override void Start()
    {
        base.Start();

    }

    void Update()
    {
        transform.position += weaponData.Speed * Time.deltaTime * direction;        // nastavime movement noza
    }
}
