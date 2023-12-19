using UnityEngine;
public class FlyEnemy : Enemy
{
    #region Fields

    public float minX = 0.1f;
    public float maxX = 0.2f;
    public float minY = 0.1f;
    public float maxY = 0.2f;

    private Rigidbody2D rb;
    public float speed = 2f;
    public float positionChangeTime = 0.2f; 

    private Vector3[] randomTargetPositions;
    private int currentPositionIndex = 0;
    private float lastPositionChangeTime;
    private bool obstacleHit = false;

    #endregion

    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        InitializeRandomTargetPositions();
        lastPositionChangeTime = Time.time;
    }

    void Update()
    {
        Movement();

        // Check if the enemy has hit an obstacle or if it's time to change positions.
        if (obstacleHit || Time.time - lastPositionChangeTime > positionChangeTime)
        {
            obstacleHit = false;
            lastPositionChangeTime = Time.time + positionChangeTime;
        }
    }

    #region Movement

    private void Movement()
    {
        float currentSpeed = Mathf.Lerp(0, speed, Vector2.Distance(transform.position, randomTargetPositions[currentPositionIndex]) / 2f);
        transform.position = Vector2.MoveTowards(transform.position, randomTargetPositions[currentPositionIndex], currentSpeed * Time.deltaTime);

        float distance = Vector2.Distance(transform.position, randomTargetPositions[currentPositionIndex]);
        if (distance < 0.2f)
        {
            currentPositionIndex = (currentPositionIndex + 1) % randomTargetPositions.Length;
        }

      
        HandleDirection(randomTargetPositions[currentPositionIndex]);
    }

    private void HandleDirection(Vector3 goalPosition)
    {
        if (goalPosition.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    #endregion

    #region Random Target Position

    private void InitializeRandomTargetPositions()
    {
        randomTargetPositions = new Vector3[3];
        randomTargetPositions[0] = new Vector3(transform.position.x + minX, transform.position.y + minY, 0);
        randomTargetPositions[1] = new Vector3(transform.position.x + (maxX + minX) / 2f, transform.position.y + (maxY + minY) / 2f, 0);
        randomTargetPositions[2] = new Vector3(transform.position.x + maxX, transform.position.y + maxY, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision happened with another Collider2D
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Collision happened!");
            obstacleHit = true;
        }
        else
        {
            Debug.Log("Not detected");
            obstacleHit = false;
        }
    }

    #endregion
}
