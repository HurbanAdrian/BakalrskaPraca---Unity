using UnityEngine;

public class MjolnirWeapon : ProjectileWeapon
{
    [Header("Mjolnir Settings")]
    public float spreadAngle = 15f;

    protected override float GetSpawnAngle()
    {
        int totalProjectiles = currentStats.number;
        int currentIndex = currentAttackCount > 0 ? totalProjectiles - currentAttackCount : 0;
        float baseAngle = base.GetSpawnAngle();

        if (totalProjectiles <= 1) return baseAngle;

        float halfSpread = (spreadAngle * (totalProjectiles - 1)) / 2f;
        float angleOffset = (currentIndex * spreadAngle) - halfSpread;

        return baseAngle + angleOffset;
    }
}