using System;
using UnityEngine;

public class GenericTriggerDamage : MonoBehaviour
{
    public int damage = 1;
    [SerializeField] bool onceOnly = true;
    bool doneAttacking;

    void OnTriggerEnter(Collider other)
    {
        if (doneAttacking) return;
        if (other.gameObject.CompareTag("Player")) 
        {
            PlayerStats playerHit = other.gameObject.GetComponentInParent<PlayerStats>();
            playerHit.Damage(damage);
            doneAttacking = onceOnly;
        }
    }
}
