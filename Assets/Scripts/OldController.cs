using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldController : MonoBehaviour
{
    // Start is called before the first frame update
    // Our variables
    public float playerSpeed = 5f;
    public float jumpForce = 10f;

    private bool isGrounded;
    // Allows for a single jump. 
    private int remainingJumps = 1;
    // Renders our sprite
    private SpriteRenderer spriteRenderer;
    // Our physics component. 
    private Rigidbody2D rb;

    //INITIALIZE & DECLARE
    float coyoteTime = 0.5f;
    float coyoteTimer;

    private float jumpBufferTime = 0.5f;
    private float jumpBufferTimer;

    private void Start()
    {
        // Get the Rigidbody2D component.
        // We use our Rigidbody to help simulate physics-based  movement. 
        rb = GetComponent<Rigidbody2D>();

        // Get the SpriteRenderer component for flipping the sprite. 
        // Our spriteRenderer is a component that allows you to render 2D sprites and images.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Standard horizontal movement, move the player left or right. 
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontalInput * playerSpeed, rb.velocity.y);

        // Flip the sprite when moving left.
        // We are literally taking the image we have and flipping it.
        if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        // Flip the sprite when moving right.
        // We are taking the image we have and flipping it back. 
        else if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }

        // Reset the jumps that remain when you are grounded. 
        if (isGrounded)
        {
            remainingJumps = 1;
        }


        // This is the solution implemented from https://www.youtube.com/watch?v=7KiK0Aqtmzc
        /* Nutshell Explanation: 
        Ensure that when something is moving down, it speeds up because of gravity, 
        and when it's moving up and the jump button isn't pressed, it slows down.*/
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }


        // COYOTE TIME
        if (isGrounded == true)
        {
            coyoteTimer = coyoteTime;
            remainingJumps = 1;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // JUMP BUFFER TIME
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // WHAT WE'RE COVERING TODAY:
        // If we get "space" as the input, if we are grounded and our remaining jumps is more than zero, we 
        // allow the player to jump once more.  
        if (Input.GetKeyDown(KeyCode.Space) && jumpBufferTime > 0f && coyoteTime > 0f)
        {
            if (isGrounded || remainingJumps > 0)
            {
                // This line makes our object jump upwards by setting its speed to the value of our jumpForce.
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                // This line removes one of the remaining jumps to ensure we cannot jump more than once. 
                remainingJumps--;

                jumpBufferTimer = jumpBufferTime; 

                // If the object is moving downwards, we make it move down faster on account of gravity. 
                if (rb.velocity.y < 0)
                {
                    rb.velocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}