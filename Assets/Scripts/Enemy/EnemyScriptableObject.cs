using UnityEngine;

[CreateAssetMenu(fileName ="EnemyScirptableObject", menuName ="ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    // Staty nepriatela
    [SerializeField]
    float moveSpeed;
    public float MoveSpeed { get => moveSpeed; private set => moveSpeed = value; }

    [SerializeField]
    float maxHealth;
    public float MaxHealth { get => maxHealth; private set => maxHealth = value; }

    [SerializeField]
    float damage;
    public float Damage { get => damage; private set => damage = value; }
}
