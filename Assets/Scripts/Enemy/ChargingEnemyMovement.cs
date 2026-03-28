using UnityEngine;

public class ChargingEnemyMovement : EnemyMovement
{
    Vector2 chargeDirection;

    // Najprv vypoèítame smer, ktorým nepriate¾ vyrazí, 
    protected override void Start()
    {
        base.Start();
        if (player != null)
        {
            chargeDirection = (player.transform.position - transform.position).normalized;

            // --- OTOÈENIE SPRITE-U --- staci len raz tu otocit
            if (spriteRenderer != null)
            {
                if (chargeDirection.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
                else if (chargeDirection.x > 0)
                {
                    spriteRenderer.flipX = false;
                }
            }
        }
    }

    // Namiesto neustáleho sledovania hráèa sa len pohybujeme v smere, ktorý sme si urèili na zaèiatku.
    public override void Move()
    {
        transform.position += (Vector3)chargeDirection * enemy.currentMoveSpeed * Time.deltaTime;
    }
}
