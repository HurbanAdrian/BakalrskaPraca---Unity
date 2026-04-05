using System.Collections.Generic;
using UnityEngine;

public class ParrotWeapon : Weapon
{
    List<ParrotMinion> activeParrots = new List<ParrotMinion>();

    // Update nepotrebujeme, papagáje sa hıbu a utoèia samé vo svojom skripte
    protected override void Update() { }

    public override void OnEquip()
    {
        SpawnParrots();
    }

    public override void OnUnequip()
    {
        ClearParrots();
    }

    public override bool DoLevelUp(bool updateUI = true)
    {
        if (!base.DoLevelUp(updateUI)) return false;

        // Ak sa napr. zvıšil stat 'Number' po level upe, prepíšeme papagájov
        ClearParrots();
        SpawnParrots();

        return true;
    }

    void SpawnParrots()
    {
        if (!currentStats.projectilePrefab) return;

        // V tvojom novom systéme aháme poèet z currentStats.number
        int parrotCount = currentStats.number;
        if (parrotCount <= 0) parrotCount = 1;

        for (int i = 0; i < parrotCount; i++)
        {
            Projectile prefab = Instantiate(currentStats.projectilePrefab, transform.position, Quaternion.identity);
            ParrotMinion parrot = prefab as ParrotMinion;

            if (parrot)
            {
                parrot.weapon = this;
                parrot.owner = owner;

                // Rozloíme ich rovnomerne po krunici
                parrot.currentAngle = (Mathf.PI * 2 / parrotCount) * i;

                activeParrots.Add(parrot);
            }
        }
    }

    void ClearParrots()
    {
        foreach (ParrotMinion p in activeParrots)
        {
            if (p) Destroy(p.gameObject);
        }
        activeParrots.Clear();
    }
}