using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : Enemy
{
    #region Fields

    protected Rigidbody2D rb;
    public List<Transform> points;
    public int nextID = 0;
    int idChangeValue = 1;
    public float speed = 2;

    #endregion


    protected override void Start()
    {
        base.Start();
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Movement();
    }


    #region Movement

    protected void Movement()
    {
        Transform goalPoint = points[nextID];

        HandleDirection(goalPoint);

        // Move the enemy towards the goal point
        transform.position = Vector2.MoveTowards(transform.position, goalPoint.position, speed * Time.deltaTime);

        // Check the distance between enemy and goal point to trigger the next point
        if (Vector2.Distance(transform.position, goalPoint.position) < 0.2f)
        {
            // Check if we are at the end of the line (make the change -1)
            if (nextID == points.Count - 1)
                idChangeValue = -1;

            // Check if we are at the start of the line (make the change +1)
            if (nextID == 0)
                idChangeValue = 1;

            // Apply the change on the nextID
            nextID += idChangeValue;
        }
    }

    protected void HandleDirection(Transform goalPoint)
    {
        // Flip the enemy transform to look into the point's direction
        if (goalPoint.transform.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    #endregion
}