using UnityEngine;

public class BoulderProjectile : Projectile
{
    [Header("Boulder Settings")]
    [Tooltip("Ako rýchlo sa balvan vizuálne kotú¾a (násobite¾)")]
    public float rollSpeedMultiplier = 2.5f;

    private Vector3 moveDirection;

    protected override void Start()
    {
        base.Start();

        // Uložíme si pôvodný smer letu hneï pri spawne!
        moveDirection = transform.right;

        // Zabezpeèíme, aby balvan letel rovno a nebol ovplyvnený poèiatoènou fyzikou
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected override void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Weapon.Stats stats = weapon.GetStats();
            float currentSpeed = stats.speed * owner.Stats.speed;

            // 1. Posun balvanu presne v smere výstrelu
            transform.position += moveDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position);

            // 2. Výpoèet rotácie (kotú¾anie)
            // Ak letí doprava, toèí sa do mínusu (v smere hodinových ruèièiek)
            float rotationAmount = -currentSpeed * rollSpeedMultiplier * 100f * Time.fixedDeltaTime;

            // Ak letí do¾ava (napr. x je záporné), chceme, aby sa toèil naopak, no v našom prípade zbraò vždy nastavuje rotáciu objektu na stranu letu.
            // Preto ho jednoducho toèíme vždy tak, akoby sa valil dopredu.
            transform.Rotate(0, 0, rotationAmount);
        }
    }

    // Volite¾né: Ak chceme, aby balvan spomalil, keï prejde cez nepriate¾a (pocit váhy)
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other); // Udelí damage a zníži Piercing pod¾a základnej logiky

        // Zvuk drtenia / shake mozno pridat pozom
    }
}