using UnityEngine;

public class AxeWeapon : ProjectileWeapon
{
    protected override float GetSpawnAngle()
    {
        // Urèí posun (offset) na základe toho, ko¾ký projektil v poradí sa práve vytvára.
        int offset = currentAttackCount > 0 ? currentStats.number - currentAttackCount : 0;
        // Vypoèíta uhol rotácie tak, aby sekery lietali v mierne odlišných smeroch v závislosti od smeru pohybu hráèa (v¾avo/vpravo).
        return 90f - Mathf.Sign(movement.lastMovedVector.x) * (5 * offset);
    }

    protected override Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        // Vráti náhodnú pozíciu (offset) v rámci definovaného rozsahu (spawnVariance), aby sekery nevznikali presne na tom istom mieste.
        return new Vector2(
            Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            Random.Range(currentStats.spawnVariance.yMin, currentStats.spawnVariance.yMax)
        );
    }
}
