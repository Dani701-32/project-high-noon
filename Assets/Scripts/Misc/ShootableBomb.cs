using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableBomb : MonoBehaviour
{
    [SerializeField] Animator explosionAnimator;
    [SerializeField] float destructionTime;
    
    static readonly int Explode1 = Animator.StringToHash("Explode");
    public bool exploded;

    void OnCollisionEnter(Collision other)
    {
        Bullet bull = other.gameObject.GetComponent<Bullet>();
        if (bull)
        {
            bull.col.enabled = false;
            bull.rb.velocity = bull.rb.angularVelocity = Vector3.zero;
            bull.rb.isKinematic = true;
            bull.StartCoroutine("TrailGone");
            Destroy(bull.gameObject, 1);
        }
        Explode();
    }

    public void Explode()
    {
        exploded = true;
        if(explosionAnimator){
            explosionAnimator.SetTrigger(Explode1);
            Destroy(gameObject, destructionTime);
            return;
        }
        Destroy(gameObject);
    }
}
