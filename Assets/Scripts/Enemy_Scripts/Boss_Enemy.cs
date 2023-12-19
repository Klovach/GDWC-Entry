using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Use enum state later, if possible. 
public enum BossAnimationState
{
    Idle,
    Hurt,
    Attack
}

public class BossEnemyController : MonoBehaviour
{
    public int maxHealth = 2;
    private int currentHealth;

    public GameObject portalPrefab;
    public Transform portalSpawnPoint;
    protected Animator animator;
    protected Collider2D coll;
    private Transform playerTransform;
    private Rigidbody2D rb; 

    public float chaseSpeed = 5f;
    public float rotationSpeed = 3f;
    public float chaseRange = 10f;
    protected bool isAlive = true;

    void Start()
    {
        currentHealth = maxHealth;
       

        // Get player transform this way for Portal later.
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if the player is within the chase range

        Vector3 direction = (playerTransform.position - transform.position).normalized;


        if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRange)
        {
            ChasePlayer();
            HandleDirection(direction); 
        }
        else
        {
            StopChasing();
        }
       
    }

    void HandleDirection(Vector3 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void ChasePlayer()
    {
        animator.SetTrigger("Attack");

        Vector3 direction = (playerTransform.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f, LayerMask.GetMask("Ground"));
        Debug.Log(hit);

        if ((playerTransform.position.y > transform.position.y && Mathf.Abs(playerTransform.position.x - transform.position.x) < chaseRange) || hit.collider == null)
        {
            // If the player is above the enemy within range OR there is no ground, jump: 
            if (IsGrounded())
            {
                Jump();
            }

            // Check if an obstruction is hit during the chase
            if (!HasHitObstacle())
            {
                transform.Translate(direction * chaseSpeed * Time.deltaTime);
            }
            else
            {
                StopChasing();
            }
        }
        else
        {
            Vector3 movement = new Vector3(direction.x, 0f, 0f);
            transform.Translate(movement * chaseSpeed * Time.deltaTime);
        }
    }

    bool HasHitObstacle()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, chaseSpeed * Time.deltaTime, LayerMask.GetMask("Ground"));

        return hit.collider != null;
    }


    bool IsGrounded()
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        return groundCheck.collider != null;
    }

    void Jump()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float jumpForce = 5f; 
        float maxDownwardVelocity = -5f; 

        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, maxDownwardVelocity));
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    void StopChasing()
    {
        animator.SetTrigger("Idle");
    }


    // FOR LATER 
    public void TakeDamage(int damage)
    {

        // PUSH BACK 
        currentHealth -= damage;
        Vector2 pushDirection = -transform.right; 
        float movementAmount = 10f * Time.deltaTime;
        Vector2 newPosition = (Vector2)transform.position + pushDirection * movementAmount;
        transform.position = newPosition;

        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

 

    void Die()
    {

        // Spawn the portal
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched enemy collider.");
            if ((isAlive && PlayerController.state == State.attacking))
            {
                Debug.Log("Player successfully attacked enemy.");
                animator.SetTrigger("Death");
                isAlive = false;
                Destroy(this.gameObject, 0.4f);
            }
        }
    }

    public void PerformAttack()
    {
        animator.SetTrigger("Attack");
    }
}