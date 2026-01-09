using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    Transform player;
    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform; 
    }


    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, enemyData.MoveSpeed * Time.deltaTime);          // konstantne pohyb k hracovi
    }
}
