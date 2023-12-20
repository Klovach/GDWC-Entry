using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlade : MonoBehaviour
{
    #region Serialized Fields

    // how fast the blade travels
    [SerializeField] private float speed;

    #endregion

    #region Private Fields

    public Transform StartPoint;
    public Transform EndPoint;
    bool goBack;

    #endregion

    void Update()
    {
        // check if blade has reached a certian point and change goBack accordingly 
        if (transform.position.x >= StartPoint.position.x) { goBack = true; }
        else if (transform.position.x <= EndPoint.position.x) { goBack = false; }

        // make blade move between points
        if (goBack) { transform.position = Vector2.MoveTowards(transform.position, EndPoint.position, speed * Time.deltaTime); }
        else { transform.position = Vector2.MoveTowards(transform.position, StartPoint.position, speed * Time.deltaTime); }
    }
}
