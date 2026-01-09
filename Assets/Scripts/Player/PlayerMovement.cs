using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    Rigidbody2D rb;
    [HideInInspector]
    public float LastHorizontalVector;
    [HideInInspector]
    public float LastVerticalVector;
    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public Vector2 lastMovedVector;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastMovedVector = new Vector2(1, 0f);      // default last moved right  lebo ak by sme sa nepohli tak by to bolo (0,0) a tak by nevedel kam hodit noz
    }

    // Update is called once per frame
    void Update()
    {
        InputManagement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void InputManagement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;

        if (moveDir.x != 0)
        {
            LastHorizontalVector = moveDir.x;
            lastMovedVector = new Vector2(LastHorizontalVector, 0f);       // last moved X
        }

        if (moveDir.y != 0)
        {
            LastVerticalVector = moveDir.y;
            lastMovedVector = new Vector2(0f, LastVerticalVector);         // last moved Y
        }

        if (moveDir.x != 0 && moveDir.y != 0)
        {
            lastMovedVector = new Vector2(LastHorizontalVector, LastVerticalVector);    // last moved diagonal
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveDir.x * moveSpeed, moveDir.y * moveSpeed);
    }
}
