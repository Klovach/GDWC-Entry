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
    public int currentHealth = 1; 

    private SpriteRenderer spriteRenderer;
    public GameObject gatewayPrefab;
    public Transform portalSpawnPoint;
    protected Animator animator;
    protected Collider2D coll;
    private Transform playerTransform;
    private Rigidbody2D rb; 

    public float chaseSpeed = 5f;
    public float rotationSpeed = 3f;
    public float chaseRange = 10f;
    protected bool isAlive = true;
    protected bool isTouchingPlayer = false;

    private float damageCooldown = 0.3f; // Adjust the cooldown duration as needed
    private float damageCooldownTimer = 0f;

    void Start()
    {
       

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();  // Add this line
    }

    void Update()
    {
        // Check if the player is within the chase range
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Update the damage cooldown timer
        if (damageCooldownTimer > 0)
        {
            damageCooldownTimer -= Time.deltaTime;
        }

        if (distanceToPlayer <= chaseRange && !isTouchingPlayer)
        {
            ChasePlayer();
            HandleDirection(direction);
        }
        else
        {
            StopChasing();

            // If player is outside a certain distance, consider them not touching
            if (distanceToPlayer > 2f)
            {
                isTouchingPlayer = false;
            }
        }
    }


    void HandleDirection(Vector3 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(3, 3, 1);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-3, 3, 1);
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
    public void TakeDamage()
    {
        Debug.Log("Take damage is called");
        Debug.Log(damageCooldownTimer); 
        // Check if the damage cooldown timer has elapsed
        if (damageCooldownTimer <= 0)
        {

            StartCoroutine(FlashEnemy());
            // PUSH BACK
            currentHealth -= 1;

            SoundManager.Instance.PlayDeathSound();
            animator.SetTrigger("Hurt");


            if (currentHealth <= 0)
            {
                Die();
            }

            // Reset the damage cooldown timer
            damageCooldownTimer = damageCooldown;
        }
    
}


    private IEnumerator FlashEnemy()
    {
        Debug.Log("Called flash enemy");
        float flashDuration = 0.1f;
        int numFlashes = Mathf.FloorToInt(flashDuration / (2 * flashDuration));

        for (int i = 0; i < numFlashes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        spriteRenderer.color = Color.white; // Ensure the color is reset even if the loop doesn't finish
        Debug.Log("Flash complete");
    }


    void SpawnPortal()
    {
        if (portalSpawnPoint != null)
        {
            // Spawn the portal
            Instantiate(gatewayPrefab, portalSpawnPoint.position, portalSpawnPoint.rotation);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched enemy collider.");
            isTouchingPlayer = true;
            Debug.Log(isAlive);
            Debug.Log(PlayerController.state); 
            if ((isAlive && PlayerController.state == State.attacking || PlayerController.state == State.falling))
            {
                TakeDamage();
            }
        }

    }

    void Die()
    {
        animator.SetTrigger("Death");
        isAlive = false;
        SoundManager.Instance.PlayDeathSound();
        SpawnPortal();
        Destroy(this.gameObject, 0.4f);

    }

    public void PerformAttack()
    {
        animator.SetTrigger("Attack");
    }
}