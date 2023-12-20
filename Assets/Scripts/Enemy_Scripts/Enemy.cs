using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Fields

    protected Animator animator;
    protected Collider2D coll;
  //  protected AudioSource death;

    protected bool isAlive = true;
    #endregion

    protected virtual void Start()
    {

        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
   //     death = GetComponent<AudioSource>();
    
}


    #region Actions

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched enemy collider.");
            if ((isAlive && PlayerController.state == State.attacking || PlayerController.state == State.falling))
            {
                Debug.Log("Player successfully attacked enemy.");
                animator.SetTrigger("Death");
                isAlive = false;
                Destroy(this.gameObject, 0.3f);
            }
        }
    }
    #endregion
}