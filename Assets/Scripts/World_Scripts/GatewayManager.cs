using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldBoundraries : MonoBehaviour
{
    [SerializeField]
    public GameObject spawnPoint;

    [SerializeField]
    public GameObject player;
    private Collider2D playerCollider;

    [SerializeField]
    public float threshold = -10;

    [SerializeField]
    public GameObject gateway;
    private Collider2D gatewayCollider;

    [SerializeField]
    public string nextLevelName; 

    private void Start()
    {
        playerCollider = player.GetComponent<Collider2D>();
        gatewayCollider = gateway.GetComponent<Collider2D>();
    }

    private void Update()
    {
        HandleGateways();
    }

    void FixedUpdate()
    {
        if (player.transform.position.y < threshold)
        {
            player.transform.position = spawnPoint.transform.position;
        }
    }

    public void HandleGateways()
    {
        if (playerCollider.IsTouching(gatewayCollider))
        {
            Debug.Log("Touching");

            // All counting in an array starts at zero. However, the scene count in build settings does not, so we subtract by one. 
            if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.Log("No more levels past this point!");
            }
        }

    }
}
