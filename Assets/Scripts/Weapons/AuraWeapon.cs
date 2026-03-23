using UnityEngine;

public class AuraWeapon : Weapon
{
    protected Aura currentAura;

    // Update sa volá raz za frame
    protected override void Update() { }

    public override void OnEquip()
    {
        // Pokúsi sa nahradi auru, ktorú zbraò má, za novú.
        if (currentStats.auraPrefab)
        {
            if (currentAura) Destroy(currentAura.gameObject);
            currentAura = Instantiate(currentStats.auraPrefab, transform);
            currentAura.weapon = this;
            currentAura.owner = owner;

            // Okamité aplikovanie ve¾kosti pri nasadení zbrane
            currentAura.transform.localScale = new Vector3(currentStats.area, currentStats.area, currentStats.area);
        }
    }

    public override void OnUnequip()
    {
        if (currentAura) Destroy(currentAura.gameObject);
    }

    public override bool DoLevelUp()
    {
        if (!base.DoLevelUp()) return false;

        // Ak je k tejto zbrani pripojená aura, aktualizujeme ju (jej ve¾kos).
        if (currentAura)
        {
            currentAura.transform.localScale = new Vector3(currentStats.area, currentStats.area, currentStats.area);
        }
        return true;
    }
}
