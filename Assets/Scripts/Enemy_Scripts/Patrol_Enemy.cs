using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : Enemy
{
    #region Fields

    private Rigidbody2D rb;
    public float patrolRange = 5f; 
    public float speed = 2;


    private Vector2 patrolStartPosition;
    private bool movingRight = true;



    private bool isHitCooldown = false;
    private float hitCooldownDuration = 1f; 
    private float hitCooldownTimer = 0f;

    #endregion

    protected override void Start()
    {
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        patrolStartPosition = transform.position;
    }

    void Update()
    {
        Patrol();
    }

    #region Patrol

    private void Patrol()
    {
        float patrolDirection = movingRight ? 1 : -1;
        Vector2 nextPosition = new Vector2(patrolStartPosition.x + patrolDirection * patrolRange, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);

        if (HasHitObstacle())
        {
            if (!isHitCooldown)
            {
                
                movingRight = !movingRight;
                patrolStartPosition = transform.position;

               
                isHitCooldown = true;
                hitCooldownTimer = 0f;
            }
        }

    
        if (isHitCooldown)
        {
            hitCooldownTimer += Time.deltaTime;

           
            if (hitCooldownTimer >= hitCooldownDuration)
            {
                isHitCooldown = false;
            }
        }

        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    private bool HasHitObstacle()
    {
        float raycastDistance = 0.2f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * (movingRight ? 1 : -1), raycastDistance);
        return hit.collider != null;
    }

    #endregion
}