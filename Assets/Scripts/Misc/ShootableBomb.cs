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
