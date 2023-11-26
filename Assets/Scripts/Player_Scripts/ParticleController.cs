using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem movementParticle;

    [Range(0,10)]
    [SerializeField] int occurAfterVelocity;

    [Range(0, 0.2f)]
    [SerializeField] float dustFormationPeriod;

    [SerializeField] Rigidbody2D playerRb;

    float counter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var emission = movementParticle.emission;
        counter += Time.deltaTime;
        if (Mathf.Abs(playerRb.velocity.x) > occurAfterVelocity)
        {
            if (counter > dustFormationPeriod)
            {
                emission.enabled = true;
                movementParticle.Play();
                counter = 0;
            }
        }
        if (playerRb.velocity.x == 0)
        {
            emission.enabled = false;
        }
    }
}
