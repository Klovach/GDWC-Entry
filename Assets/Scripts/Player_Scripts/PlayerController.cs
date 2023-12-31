using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.SceneManagement;
using static UnityEditor.VersionControl.Asset;




#region Player States 
// By placing the enum in a global space, it can be accessed by any class. Add states as nededed. 
public enum State { idle, running, jumping, falling, hurt, attacking, teleporting, dying }
#endregion 


[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]


public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    static public State state;
    // Speed of the player
    [SerializeField] private float playerSpeed = 10f;
    // Force applied when jumping
    [SerializeField] private float jumpForce = 7f;
    // Time to allow jumping after leaving the ground
    [SerializeField] private float coyoteTime = 0.5f;
    // Time window for accepting jump input
    [SerializeField] private float jumpBufferTime = 0.3f;
    // Speed boost from apex modifier
    [SerializeField] private float apexBoost = 2f;
    // Fall clamp
    [SerializeField] private float fallClamp = -20f;
    // Get ground layer mask
    [SerializeField] private LayerMask groundLayerMask;
    #endregion



    #region Private Fields

    private float invincibilityDuration = 0.4f; 
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;
    private int remainingJumps = 1;
    private int health = 4; 
    private int points; 
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // AttackVariables 
    private float attackTimer = 0.2f;
    private float timer = 0f;

    public bool isAttacking;
    public bool isTeleporting; 

    #endregion

    #region Unity Callbacks


    private void Start()
    {

        InitializeComponents();
        health = 4; 
       
        UIManager.Instance.UpdateHealthDisplay(health);
        UIManager.Instance.UpdatePointsDisplay(points);
    }


    // NOTES; Add edge detection next 
    private void Update()
    {
        if (PortalManager.Instance.isActivePortal())
        {
            state = State.teleporting;
            animator.SetInteger("state", (int)state);
        }


        HandleMovementInput();
        HandleCoyoteTime();
        HandleJumpBuffer();
            HandleAttackInput();

        // Prioritize attacking over other states
        if (isAttacking)
        {
            HandleAttack();
        }
        else
        {
            HandleVariableJumpHeight();
            HandleJumping();
            HandleIdling();
            // HandleFallClamp();
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    #endregion

    #region Player Input Handling



    private void HandleAttackInput()
    {
        if (Input.GetKey(KeyCode.RightControl) && !isAttacking)
        {
            Vector2 attackImpulse = new Vector2(2f, 5f); 
            rb.velocity = attackImpulse;
            Attack();
        }
    }

    private void Attack()
    {
        isAttacking = true;
        state = State.attacking;
        animator.SetInteger("state", (int)state);
        SoundManager.Instance.PlayAttackSound();
    }


    private void HandleAttack()
    {
        if (isAttacking)
        {
            timer += Time.deltaTime;
            if (timer >= attackTimer)
            {
                timer = 0;
                isAttacking = false;
                state = State.idle;
                animator.SetInteger("state", (int)state);

              
            }
        }
    }
    // In Maintenance 
    private void HandleFallClamp()
    {
        if (!IsGrounded())
        {
            if (rb.velocity.y < fallClamp)
            {
                Debug.Log("Applying fall clamp. Current Y velocity: " + rb.velocity.y + ", Fall Clamp: " + fallClamp);
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, fallClamp);
            }
        }
    }

    private void HandleMovementInput()
    {
        // Standard horizontal movement, move the player left or right.
        float horizontalInput = Input.GetAxis("Horizontal");
        Flip(horizontalInput);
        rb.velocity = new Vector2(horizontalInput * playerSpeed, rb.velocity.y);
    }
    #endregion

    #region Jumping and Physics

    private void HandleVariableJumpHeight()
    {
        // Implement our variable jump height logic from last lesson.
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (4f - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        }
    }

  
    private void HandleJumping()
    {
        // Handle jumping input and logic. If we have some coyoteTime left, we're grounded, or we have some remaining jumps, we can jump. 
        if (Input.GetKeyDown(KeyCode.Space) && jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f || IsGrounded() || remainingJumps > 0)
            {
                Jump();
            }
        }
        if (!IsGrounded() && rb.velocity.y < 0 && state != State.teleporting)
            {
            state = State.falling;
            Debug.Log(state);
        }
        else if (IsGrounded() && state != State.falling && !isAttacking)
        {
           state = State.idle; // Only set to idle if grounded and not falling
        }
    }
    private void Jump()
    {

        if (IsGrounded() || remainingJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            remainingJumps--;
            jumpBufferCounter = jumpBufferTime;
            SoundManager.Instance.PlayJumpSound(); 
            // If we're currently in the air (in other words, our y velocity is greater than 0)....
            if (rb.velocity.y < 0)
            {
                //We provide a moment of antigravity by using (2f - 1) is used to reduce gravity. 
                rb.velocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;

                // Apply the speed boost by adding to the horizontal velocity.
                //  Our horizontal velocity is rb.velocity.x. 
                rb.velocity = new Vector2(rb.velocity.x + apexBoost, rb.velocity.y);
            }
        }
    }

    #endregion

    #region Coyote Time and Jump Buffer

    private void HandleCoyoteTime()
    {
        // Our coyote time allows a brief window for jumping after leaving the ground.
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            remainingJumps = 1;
        }
        else
        {
            // Reduce coyote time as time passes when not grounded.
            coyoteTimeCounter -= Time.deltaTime;
        }
    }


    
    private void HandleJumpBuffer()
    {
        // Our jump buffer time allows for delayed jump input to still register.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            // Decrease the jump buffer time as time passes.
            jumpBufferCounter -= Time.deltaTime;
        }
    }
     
    private void HandleIdling()
    {   if (IsGrounded() && rb.velocity.y <= 0 && !isAttacking && health != 0)
        {
            state = State.idle;
            Debug.Log(state);
            animator.SetInteger("state", (int)state);
        }

      
    }

    #endregion

    #region Ground Detection

    private bool IsGrounded()
    {
        // Ground detection logic
        float extraHeight = 0.01f;
        float rayLength = boxCollider2D.bounds.extents.y + extraHeight;

        // Perform a box-shaped raycast directly downward to detect ground.
        // Here's a breakdown of what this line means. Our method takes five parameters.
        // Origin || boxCollider2D.bounds.center: Our origin is our boxCollider2D, which is attachted to our player. We're getting the center of the bounding box.
        // Size || boxCollider2D: Here we're getting the size of our object, which is the bounds of our boxCollider.
        // Angle | 0f: Here we're getting an angle. In our case, our angle isn't offset in any way. We want to look straight down.
        // Direction | Vector2.Down: Here we specify we want to look straight down. 
        // FloatDistance | 0f: Here we specify how far our we want to look. In our case, we want to look as far as the height of our boxCollider plus some additional height/
        // Layer Mask | groundLayerMask: Here we specify the layer we want to look at when we collide, which is our ground layer.
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, rayLength, groundLayerMask);
        Color rayColor;

        // DEBUG ------------------------------
        if (raycastHit.collider != null && raycastHit.normal == Vector2.up)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }

        // Calculate the starting point for the ground detection raycast. 
        Vector3 raycastOrigin = boxCollider2D.bounds.center - new Vector3(0, rayLength / 2f);
        Debug.DrawRay(raycastOrigin, Vector2.down * rayLength, rayColor);
        Debug.DrawRay(raycastOrigin, Vector2.down * rayLength, rayColor);

        
        // Return whether or not the raycast hit something. If this statement is true, we're grounded! If not, we aren't grounded. 
        Debug.Log("Grounded: " +  raycastHit.collider  + "Player State : " + state);
        return raycastHit.collider != null;
    }
    #endregion

    #region Sprite Flipping
    private void Flip(float horizontalInput)
    {
        if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        else if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    #endregion


    #region Collision Handling
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check this condition first. By checking this first, we save time by exiting the method early. 
        if (isInvincible)
            return; 

        switch (collision.gameObject.tag)
        {
            case "Enemy":
                if (state == State.attacking)
                {
                    break;
                }
                else if (state == State.falling || state == State.teleporting)
                {
                    // Player is falling, apply bounce effect after making contact with enemy. 
                    rb.velocity = new Vector2(rb.velocity.x, 8f);
               //     state = State.jumping;
                    break;
                }
                else
                {
               

                    TakeDamage();
                  

                    UIManager.Instance.UpdateHealthDisplay(health); 
                    SoundManager.Instance.PlayDeathSound();
                    

                    Debug.Log("Current Health: " + health);

                    if (health <= 0)
                    {
                        health = 0; 
                        state = State.dying;
                        PerformDeath();
                    }
                    break;
                }
                

            case "Trap":

                Debug.Log("Current Health: " + health);

                TakeDamage();
                SoundManager.Instance.PlayDeathSound();
                UIManager.Instance.UpdateHealthDisplay(health);
                // Player is not falling, take damage and enter hurt state

                if (health <= 0)
                {

                    health = 0;
                    state = State.dying;
                    PerformDeath();
                    break;

                }

             
                break;

            default:
                Debug.Log("Unhandled tag: " + collision.gameObject.tag);
                isTeleporting = false; 
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Not detected");
     
            switch (collision.gameObject.tag)
        {

            case "Portal":
                if (!PortalManager.Instance.isActivePortal())
                {
                    SoundManager.Instance.PlayTeleportSound();
                    HandlePortalCollision(collision);
                    
                }
                else
                {
                    Debug.Log("Portal on cooldown");
                }
                break;


            case "Gateway":
                SoundManager.Instance.PlayTeleportSound();
                PortalManager.Instance.LoadNextLevel(); 
                break; 
            case "Food":

                if (health == 3)
                {
                    break;
                }

                else
                {
                    health++;
                }

                SoundManager.Instance.PlayPickupSound();
                break;

            case "Point":

                SoundManager.Instance.PlayPickupSound();
                points++;


                UIManager.Instance.UpdatePointsDisplay(points);


                break;


         
            default:
                Debug.Log("Unhandled tag: " + collision.gameObject.tag);
                break;
        }
    }
    #endregion

    private void HandlePortalCollision(Collider2D collision)
    {
        Portal entryPortal = collision.GetComponent<Portal>();
        Portal exitPortal = entryPortal.linkedPortal; 
        if (entryPortal != null)
        {
            PortalManager.Instance.Teleport(transform, entryPortal, exitPortal);
        

            Debug.Log(state);
            Debug.Log("Teleporting");
        }
    }

    #region Handle Death
    private void PerformDeath()
    {
        state = State.dying;
        animator.SetInteger("state", (int)state);
        Debug.Log(state);
     
        // Start a coroutine to wait for the death animation to finish.
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Wait for the duration of the death animation.
        yield return new WaitForSeconds(1f);

        // Reload the scene after the animation is complete.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void TakeDamage()
    {
        if (!isInvincible && state != State.falling)
        {
            health--;

            // Set invincibility
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            // Flash the player to indicate invincibility
            StartCoroutine(FlashPlayer());

            state = State.hurt;
            UIManager.Instance.UpdateHealthDisplay(health);
            SoundManager.Instance.PlayDeathSound();
 

            if (health <= 0)
            {
                state = State.dying;
                PerformDeath();
            }

            isInvincible = false; 
        }
    }

    private IEnumerator FlashPlayer()
    {

        float flashDuration = 0.1f;
        int numFlashes = Mathf.FloorToInt(invincibilityDuration / (2 * flashDuration));


        for (int i = 0; i < numFlashes; i++)
        {

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                spriteRenderer.color = Color.white; // Reset to the original color
            }
        }

        spriteRenderer.color = Color.white; // Ensure the color is reset even if the loop doesn't finish
    }
    #endregion 
}