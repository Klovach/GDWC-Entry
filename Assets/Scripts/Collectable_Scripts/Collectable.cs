using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    #region Fields
    protected Animator animator; 
    protected Collider2D coll;
    protected AudioSource collect;
    protected bool isCollected = false;
    #endregion


    #region Initialization 
    // Virtual allows the children to override this function if neccessary. 
    protected virtual void Start()
    {
        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
      //  collect = GetComponent<AudioSource>();
    }
    #endregion


    #region Actions

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("Collision happened.");
        if (coll.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched collectable collider.");
            if (!isCollected)
            {
                PerformCollectAnimation(); 
                isCollected = true;
                Collected();
            }
        }
    }

    protected void PerformCollectAnimation()
    {
        animator.SetTrigger("Pickup");
      //  collect.Play();
    }

    public void Collected()
    {
        Destroy(this.gameObject, 0.4f);
    }
    #endregion
}
