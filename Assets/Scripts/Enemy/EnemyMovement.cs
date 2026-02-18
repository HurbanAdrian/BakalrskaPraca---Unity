using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    EnemyStats enemy;
    Transform player;
    void Start()
    {
        enemy = GetComponent<EnemyStats>();
        player = FindFirstObjectByType<PlayerMovement>().transform; 
    }


    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, enemy.currentMoveSpeed * Time.deltaTime);          // konstantne pohyb k hracovi
    }
}
