using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    public Collider col;

    [SerializeField]
    TrailRenderer trail;
    public Rigidbody rb;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public GameObject owner;
    public int damage;
    public NetworkVariable<int> teamId = new NetworkVariable<int>();
    public NetworkVariable<int> damageOn = new NetworkVariable<int>();

    private IEnumerator TrailGone()
    {
        while (enabled)
        {
            trail.startWidth /= 2;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == owner) return;

        if (other.gameObject.CompareTag("Player"))
        {

            PlayerOnline playerHit = other.gameObject.GetComponentInParent<PlayerOnline>();
            Debug.Log(playerHit.GetTeam().teamId);
            if (playerHit.GetTeam().teamId != teamId.Value )
            {
                playerHit.Damage(damageOn.Value);
            }
            if (IsServer)
            {
                rb.velocity = rb.angularVelocity = Vector3.zero;
                col.enabled = false;
                rb.isKinematic = true;
                Destroy(gameObject, 1);
            }
        }
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            col.enabled = false;
            rb.velocity = rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            StartCoroutine("TrailGone");
            if (IsServer)
            {
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
