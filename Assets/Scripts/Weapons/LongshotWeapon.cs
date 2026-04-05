using UnityEngine;

public class LongshotWeapon : ProjectileWeapon
{
    [Header("Longshot Settings")]
    [Tooltip("Vzdialenos medzi šípmi, ak ich strie¾aš viac naraz")]
    public float spreadDistance = 0.3f;

    // Upravíme pozíciu spawnu tak, aby sa šípy ukladali pekne ved¾a seba
    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        int totalProjectiles = currentStats.number;
        int currentIndex = currentAttackCount > 0 ? totalProjectiles - currentAttackCount : 0;

        // 1. Zistíme, kam hráè mieri
        Vector2 forwardDir = movement.lastMovedVector.normalized;
        if (forwardDir == Vector2.zero) forwardDir = Vector2.right;

        // 2. Vypoèítame kolmý smer (vpravo/v¾avo od smeru letu)
        Vector2 rightDir = new Vector2(-forwardDir.y, forwardDir.x);

        // 3. Vypoèítame presný odskok pre konkrétny šíp v salve
        float offsetStep = 0f;
        if (totalProjectiles > 1)
        {
            float halfSpread = (spreadDistance * (totalProjectiles - 1)) / 2f;
            offsetStep = (currentIndex * spreadDistance) - halfSpread;
        }

        // Vrátime základnú pozíciu hráèa + náš paralelný posun
        return base.GetSpawnOffset(spawnAngle) + (rightDir * offsetStep);
    }
}