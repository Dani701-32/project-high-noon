using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    Collider col;

    [SerializeField]
    TrailRenderer trail;
    public Rigidbody rb;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public GameObject owner;
    public int damage = 1;
    public char teamTag; 

    private IEnumerator TrailGone()
    {
        while (enabled)
        {
            trail.startWidth /= 2;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((((1 << other.gameObject.layer) & playerLayer) != 0) && IsServer)
        {
            col.enabled = false;
            rb.velocity = rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            StartCoroutine("TrailGone");

            if(other.CompareTag("Player") && other.TryGetComponent<PlayerOnline>(out PlayerOnline playerHit)){

                Debug.Log(playerHit.GetTeam().teamTag);
                if(playerHit.GetTeam().teamTag != teamTag){
                    playerHit.Damage_ServerRpc(); 
                }
            }

            if(IsServer){
                Debug.Log("Teste"); 
                Destroy(gameObject, 1);
            }
        }
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            col.enabled = false;
            rb.velocity = rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            StartCoroutine("TrailGone");
            if(IsServer){
                Destroy(gameObject, 1);
            }
        }
    }

    public override void OnDestroy()
    {
        NetworkObject netBullet = GetComponent<NetworkObject>();
        if (netBullet == null)
            return;
        if (IsSpawned)
        {
            netBullet.Despawn();
        }
        base.OnDestroy();
    }
}
