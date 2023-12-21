using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public Transform exitPoint;
    public PortalManager entryPortal; 
    public float boostForce = 5f;
    public float cooldownTime = 5f;

    private bool isPortalOnCooldown = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPortalOnCooldown && other.CompareTag("Player"))
        {
            Debug.Log("Collide");
            Teleport(other.transform);
            StartCoroutine(StartCooldown());
        }
    }

    private void Teleport(Transform player)
    {
        entryPortal.isPortalOnCooldown = true; 

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

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        isPortalOnCooldown = false;
    }
}
