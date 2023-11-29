using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{

    #region Serialized Fields

    // how long (in seconds) before the platform falls
    [SerializeField] private float timeBeforeFall = 2f;

    // how fast the platform falls
    [SerializeField] private float fallSpeed = 5f;

    // how long (in seconds) before self removal
    [SerializeField] private float lifespan = 12f;

    // If the platform will destroy itself if it collides with the ground
    [SerializeField] private bool destroyOnImpact = false;

    // If the platform will fall at a constant velocity
    [SerializeField] private bool constVelocity = true;

    [SerializeField] private Rigidbody2D rb;

    #endregion

    #region Private Fields

    private bool isFalling = false;
    private bool isShaking = false;

    #endregion

    private void Update()
    {
        // assign the velocity to the fall speed if both conditions are true
        if(isFalling == true && constVelocity == true && isShaking == false) { rb.velocity = Vector3.down * fallSpeed; }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // checks if player has landed on the platform. Also checks if it crashes into objects if destroyOnImpact is enabled.
        if(collision.gameObject.CompareTag("Player") && isFalling == false) { StartCoroutine(Fall()); }
        else if(destroyOnImpact == true && collision.gameObject.CompareTag("Ground")) { Destroy(gameObject, 0); }
    }

    private IEnumerator Fall()
    {
        // make the platform shake
        StartCoroutine(Shake());
        yield return new WaitForSeconds(timeBeforeFall);

        // make the platform fall
        isShaking = false; isFalling = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, lifespan);
    }

    private IEnumerator Shake()
    {
        isShaking = true;

        // shake up
        transform.position += new Vector3(0, 0.025f, 0); yield return new WaitForSeconds(0.05f);
        transform.position += new Vector3(0.025f, 0, 0); yield return new WaitForSeconds(0.05f);
        transform.position += new Vector3(0, -0.025f, 0); yield return new WaitForSeconds(0.05f);
        transform.position += new Vector3(-0.025f, 0, 0); yield return new WaitForSeconds(0.05f);
        // shake down
        transform.position -= new Vector3(0, 0.025f, 0); yield return new WaitForSeconds(0.05f);
        transform.position -= new Vector3(0.025f, 0, 0); yield return new WaitForSeconds(0.05f);
        transform.position -= new Vector3(0, -0.025f, 0); yield return new WaitForSeconds(0.05f);
        transform.position -= new Vector3(-0.025f, 0, 0); yield return new WaitForSeconds(0.05f);
    }
}