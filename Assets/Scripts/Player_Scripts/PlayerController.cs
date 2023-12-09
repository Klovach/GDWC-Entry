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
    //Base Player Speed
    [SerializeField] private float baseSpeed = 10f;
    //Current Speed of the player
    [SerializeField] private float playerSpeed;
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
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float deccelleration = 0.7f;
    [SerializeField] private float velPower = 1f;
    #endregion



    #region Private Fields

    private int remainingJumps = 1;
    private int health = 3; 
    private int points; 
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    public bool isAttacking;


	#endregion

	#region Unity Callbacks


	private void Start()
    {

        InitializeComponents();

        state = State.idle;
        playerSpeed = baseSpeed;
        Debug.Log(state);
    }


    // NOTES; Add edge detection next 
    private void Update()
    {
        //Old Movement code
        //HandleMovementInput();
        AcceleratingMovementHandler();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleAttackInput();
        HandleVariableJumpHeight();
        HandleJumping();
        animator.SetInteger("state", (int)state);
        //     HandleFallClamp();
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

    private void AcceleratingMovementHandler()
	{
        #region Running

        float direction = Input.GetAxis("Horizontal");
        float targetSpeed = direction * playerSpeed;

        float speedDif = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deccelleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        Flip(direction);
        rb.AddForce(movement * Vector2.right);

		#endregion
	}

	private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isAttacking)
        {
            animator.SetTrigger("Attack");
            isAttacking = true;
            state = State.attacking;
            playerSpeed = baseSpeed / 2;
            Debug.Log(state);
        }
        else if(Input.GetKeyDown(KeyCode.LeftShift) && isAttacking)
		{
            animator.SetTrigger("Attack");
            isAttacking = false;
            state = State.attacking;
            playerSpeed = baseSpeed;
            Debug.Log(state);
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
        if (Input.GetKeyDown(KeyCode.Space) && jumpBufferCounter > 0f & !isAttacking)
        {
            if (coyoteTimeCounter > 0f || IsGrounded() || remainingJumps > 0)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {

        if (IsGrounded() || remainingJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            remainingJumps--;
            jumpBufferCounter = jumpBufferTime;

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

        //  Debug.Log(raycastHit.collider);
        // -----------------------------------------

        // Return whether or not the raycast hit something. If this statement is true, we're grounded! If not, we aren't grounded. 
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
        switch (collision.gameObject.tag)
        {
            case "Enemy":

                if (state == State.attacking)
                {
                    break;
                }
                else
                {
                    health = health - 1;
                    state = State.hurt;
                }

                Debug.Log("Current Health: " + health);

                if (health == 0)
                {

                    state = State.dying;
                    PerFormDeath(); 
                }

                break; 

            case "Trap":

                Debug.Log("Current Health: " + health);

                if (health == 0)
                {
                    state = State.dying;
                    PerFormDeath();
                    break;

                }


                health--;
                state = State.hurt;
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

                break; 

            case "Point":

                Debug.Log("Current Points: " + points);

                points++; 

                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Not detected");
        switch (collision.gameObject.tag)
        {

            case "Portal":
          //      state = State.teleporting;
                Debug.Log(state);
                    Debug.Log("Telporting");
                break;
        }
    }
    #endregion

    #region Handle Death
    private void PerFormDeath()
    {
        state = State.dying;
        Debug.Log(state);
          if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        } 
    
    }
    #endregion 
}