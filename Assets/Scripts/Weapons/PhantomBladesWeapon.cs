using UnityEngine;

public class PhantomBladesWeapon : ProjectileWeapon
{
    [Header("Phantom Blades Settings")]
    [Tooltip("Ako ïaleko za hráèom sa meèe zhmotnia")]
    public float spawnDistanceBehind = 1.0f;

    [Tooltip("Ak je meèov viac, aká je medzi nimi medzera")]
    public float spreadDistance = 0.5f;

    // Uhol letu nechávame základný (tam, kam sa hráè naposledy pohol)
    protected override float GetSpawnAngle()
    {
        return base.GetSpawnAngle();
    }

    // Tu vypoèítame presnú pozíciu za hráèom
    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        int totalProjectiles = currentStats.number;
        int currentIndex = currentAttackCount > 0 ? totalProjectiles - currentAttackCount : 0;

        // 1. Zistíme, kam hráè mieri (smer dopredu)
        Vector2 forwardDir = movement.lastMovedVector.normalized;
        if (forwardDir == Vector2.zero) forwardDir = Vector2.right; // Poistka, ak hráè stojí na zaèiatku hry

        // 2. Smer dozadu (tam chceme meèe)
        Vector2 backwardDir = -forwardDir;

        // 3. Smer kolmý na pohyb (aby sme meèe vedeli nauklada ved¾a seba ako stenu)
        Vector2 rightDir = new Vector2(-forwardDir.y, forwardDir.x);

        // 4. Výpoèet medzier, aby bol rad meèov vycentrovaný za hráèom
        float offsetStep = 0f;
        if (totalProjectiles > 1)
        {
            float halfSpread = (spreadDistance * (totalProjectiles - 1)) / 2f;
            offsetStep = (currentIndex * spreadDistance) - halfSpread;
        }

        // 5. Zoberieme základnú varianciu (ak by si chcel jemný random) a pripoèítame našu formáciu
        Vector2 baseOffset = base.GetSpawnOffset(spawnAngle);

        return baseOffset + (backwardDir * spawnDistanceBehind) + (rightDir * offsetStep);
    }
}