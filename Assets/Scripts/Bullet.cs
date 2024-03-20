using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public LayerMask groundLayer;

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
