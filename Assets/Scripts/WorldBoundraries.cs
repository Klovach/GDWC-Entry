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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

    }
}
