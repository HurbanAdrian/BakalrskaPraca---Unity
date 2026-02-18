using UnityEngine;

public class KnifeBehaviour : ProjectileWeaponBehaviour
{
    protected override void Start()
    {
        base.Start();

    }

    void Update()
    {
        transform.position += currentSpeed * Time.deltaTime * direction;        // nastavime movement noza
    }
}
