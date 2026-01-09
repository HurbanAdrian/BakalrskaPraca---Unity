using System;
using UnityEngine;

// Base script pre vsetky Weapon Controllers
public class WeaponController : MonoBehaviour
{
    [Header("Weapon stats")]
    public WeaponScriptableObject weaponData;
    float currentCooldown;


    protected PlayerMovement pm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        pm = FindAnyObjectByType<PlayerMovement>();
        currentCooldown = weaponData.CooldownDuration;         // na zaciatku musi cakat na cooldown
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        currentCooldown = weaponData.CooldownDuration;
    }
}
