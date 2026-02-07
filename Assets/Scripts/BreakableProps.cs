using UnityEngine;

public class BreakableProps : MonoBehaviour
{
    public float health;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Break();
        }
    }

    public void Break()
    {
        // Tu mozes pridat efekty rozbitia, spawn itemov, atd.
        Destroy(gameObject);
    }
}
