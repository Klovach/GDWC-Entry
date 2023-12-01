using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{

    protected Animator animator;
    protected Collider2D coll;
  //  protected AudioSource death;

    protected bool isAlive = true;

    protected virtual void Start()
    {

        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
   //     death = GetComponent<AudioSource>();
    
}


    // This method may be rewritten for different objects. 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if ((isAlive && PlayerController.state == State.attacking))
            {
                selfDestruct(); 
            }
        }
    }


    private void selfDestruct()
    {
        Debug.Log("Player triggered destruction.");
        animator.SetTrigger("Destroy");
        isAlive = false;
        Destroy(this.gameObject, 0.4f);
    }
}