using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chase_Enemy : Enemy
{
    private Rigidbody2D rb;
    private GameObject player;

    public float patrolRange = 5f;
    public float chaseRange = 3f;
    public float speed = 2f;
    public float jumpProbability = 4f;

    private Vector2 patrolStartPosition;
    private bool movingRight = true;
    private bool isHitCooldown = false;
    private float hitCooldownDuration = 1f;
    private float hitCooldownTimer = 0f;

    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        patrolStartPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= chaseRange)
                ChasePlayer();
            else
                Patrol();
        }
    }

    private void Patrol()
    {
        // TEMPORARY TENARY OPERATOR: If movingRight is true, change the patrol direction to right (1). If it is not, move left. 
        float patrolDirection = movingRight ? 1 : -1;
        Vector2 nextPosition = new Vector2(patrolStartPosition.x + patrolDirection * patrolRange, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);

        if (HasHitObstacle() && !isHitCooldown)
        {
            movingRight = !movingRight;
            patrolStartPosition = transform.position;
            isHitCooldown = true;
            hitCooldownTimer = 0f;
        }

        if (isHitCooldown)
        {
            hitCooldownTimer += Time.deltaTime;
            if (hitCooldownTimer >= hitCooldownDuration)
                isHitCooldown = false;
        }

        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    private void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

        if (player.transform.position.y > transform.position.y && Mathf.Abs(player.transform.position.x - transform.position.x) < patrolRange)
        {
            Jump();
            HandleDirection(player.transform.position);
        }
        else
        {
            HandleDirection(player.transform.position);
        }
    }

    private void Jump()
    {
        if (Mathf.Approximately(rb.velocity.y, 0f))
            rb.velocity = new Vector2(rb.velocity.x, 3f);
    }

    private bool HasHitObstacle()
    {
        float raycastDistance = 0.2f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * (movingRight ? 1 : -1), raycastDistance);
        return hit.collider != null;
    }

    private void HandleDirection(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}

