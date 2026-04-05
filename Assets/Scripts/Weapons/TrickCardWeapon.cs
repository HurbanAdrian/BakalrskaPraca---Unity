using UnityEngine;

public class TrickCardWeapon : ProjectileWeapon
{
    [Header("Trick Card Settings")]
    public float spreadAngle = 20f; // Uhol medzi jednotlivými kartami vo vejári

    [Tooltip("Ako ïaleko od hráèa sa karty vytvoria")]
    public float spawnDistance = 1.2f;

    protected override float GetSpawnAngle()
    {
        int totalProjectiles = currentStats.number;

        // Opravená logika indexu (rovnaká ako používaš v AxeWeapon.cs)
        int currentIndex = currentAttackCount > 0 ? totalProjectiles - currentAttackCount : 0;

        // Základný smer hráèa
        float baseAngle = base.GetSpawnAngle();

        // Ak strie¾ame len jednu kartu, pošleme ju rovno
        if (totalProjectiles <= 1) return baseAngle;

        // Výpoèet vejára
        float halfSpread = (spreadAngle * (totalProjectiles - 1)) / 2f;
        float angleOffset = (currentIndex * spreadAngle) - halfSpread;

        return baseAngle + angleOffset;
    }

    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        Vector2 baseOffset = base.GetSpawnOffset(spawnAngle);
        Vector3 distanceOffset = Quaternion.Euler(0, 0, spawnAngle) * Vector3.right * spawnDistance;
        return baseOffset + (Vector2)distanceOffset;
    }
}