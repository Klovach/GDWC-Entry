using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;

    public float teleportCooldown = 0.3f; // Set the cooldown time in seconds
    private bool isTeleportOnCooldown = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SoundManager.Instance.PlayLevelMusic(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void Update()
    {
        // Update cooldown timer
        if (isTeleportOnCooldown)
        {
            teleportCooldown -= Time.deltaTime;

            if (teleportCooldown <= 0)
            {
                isTeleportOnCooldown = false;
                teleportCooldown = 0;
            }
        }
    }


    public bool isActivePortal()
    {
        return isTeleportOnCooldown;
    }

    public void Teleport(Transform player, Portal entryPortal, Portal exitPortal)
    {
        // Check if teleport is on cooldown
        if (isTeleportOnCooldown)
            return;


        player.position = exitPortal.transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        Vector2 portalVelocity = entryPortal.transform.right * rb.velocity;
        rb.velocity = portalVelocity;
        rb.AddForce(exitPortal.transform.up * 10, ForceMode2D.Impulse);

        // Set teleport on cooldown
        isTeleportOnCooldown = true;
        teleportCooldown = 0.5f; // Reset the cooldown time
    }


    public void LoadNextLevel()
    {
        // All counting in an array starts at zero. However, the scene count in build settings does not, so we subtract by one. 
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            SoundManager.Instance.PlayLevelMusic(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels past this point!");
        }
    }
}
