using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public Transform exitPoint;
    public float boostForce = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collide");
            Teleport(other.transform);
        }
    }

    private void Teleport(Transform player)
    {
        player.position = exitPoint.position;

        Debug.Log("Player called");

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Debug.Log("Teleport");
            Vector2 exitDirection = (exitPoint.position - player.position).normalized;
            playerRb.velocity = exitDirection * boostForce;
        }
    }
}
