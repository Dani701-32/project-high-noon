using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] Camera cam;
    private float initialRotation;

    void Start()
    {
        initialRotation = transform.rotation.z;
        if(!cam) { cam = Camera.main; }
    }

    void Update()
    {
        transform.rotation = cam.transform.rotation;
        transform.Rotate(Vector3.up * initialRotation * 180);
    }
}
