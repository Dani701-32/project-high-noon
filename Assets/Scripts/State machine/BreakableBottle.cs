using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBottle : MonoBehaviour
{
    [SerializeField] Rigidbody oppositeRB;
    [SerializeField] Collider oppositeCollider;
    [SerializeField] Rigidbody baseRB;
    [SerializeField] LayerMask bulletMask;
    [SerializeField] ParticleSystem particles;

    MeshRenderer mesh;
    Rigidbody rb;

    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & bulletMask) == 0) return;

        Vector3 scale = baseRB.gameObject.transform.localScale;
        ParticleSystem.MainModule part = particles.main;
        ParticleSystem.ShapeModule shape = particles.shape;
        part.startSize = scale.magnitude * 2;
        part.startSpeed = scale.magnitude * 0.18f;
        particles.Play();
        mesh.enabled = false;
        rb.isKinematic = true;
        baseRB.isKinematic = true;
        oppositeRB.isKinematic = false;
        oppositeRB.useGravity = true;
        oppositeCollider.isTrigger = false;
        Destroy(baseRB.gameObject, 8);
    }
}
