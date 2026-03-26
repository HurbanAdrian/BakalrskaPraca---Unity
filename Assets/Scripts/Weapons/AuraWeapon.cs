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

            float area = GetArea();
            currentAura.transform.localScale = new Vector3(area, area, area);
        }
    }

    public override void OnUnequip()
    {
        if (currentAura) Destroy(currentAura.gameObject);
    }

    public override bool DoLevelUp()
    {
        if (!base.DoLevelUp()) return false;

        // Zabezpecit ze Aura bude refreshnuta ak je priradena ina na vyssom leveli
        OnEquip();

        return true;
    }
}
