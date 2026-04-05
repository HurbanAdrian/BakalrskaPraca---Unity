using UnityEngine;

public class FireballProjectile : Projectile
{
    private bool hasExploded = false;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return; // Zabraòuje viacnásobnému vıbuchu v jednom frame

        EnemyStats es = other.GetComponent<EnemyStats>();
        BreakableProps p = other.GetComponent<BreakableProps>();

        // Ak sme trafili nepriate¾a alebo rozbitnı objekt, vybuchneme!
        if (es || p)
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        Weapon.Stats stats = weapon.GetStats();
        float explosionRadius = weapon.GetArea(); // Area urèuje, akı obrovskı bude vıbuch

        // 1. Spustíme vizuálny efekt vıbuchu (ak je nastavenı)
        if (stats.hitEffect)
        {
            ParticleSystem vfx = Instantiate(stats.hitEffect, transform.position, Quaternion.identity);

            // Prispôsobíme ve¾kos efektu pod¾a rádiusu vıbuchu
            vfx.transform.localScale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
            Destroy(vfx.gameObject, 2f);
        }

        // 2. Nájdeme a zraníme VŠETKİCH nepriate¾ov v okruhu vıbuchu
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D t in targets)
        {
            EnemyStats enemy = t.GetComponent<EnemyStats>();
            if (enemy)
            {
                Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;
                enemy.TakeDamage(GetDamage(), source);

                // 3. Aplikácia tvojich buffov (Burn atï.)
                weapon.ApplyBuffs(enemy);
            }

            BreakableProps prop = t.GetComponent<BreakableProps>();
            if (prop)
            {
                prop.TakeDamage(GetDamage());
            }
        }

        // 4. Samotnı fireball sa po vıbuchu znièí
        Destroy(gameObject);
    }

    // Pomôcka do Editora: nakreslí èervenı kruh okolo fireballu, aby si videl, akı má rádius vıbuchu
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (weapon != null) Gizmos.DrawWireSphere(transform.position, weapon.GetArea());
    }
}