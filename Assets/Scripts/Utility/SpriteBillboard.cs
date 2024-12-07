using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField, ReadOnly] Camera cam;
    private float initialRotation;

    void Start()
    {
        if (!cam) { cam = Camera.main; }
        initialRotation = transform.rotation.z;
    }

    void Update()
    {
        if (!cam) { cam = Camera.main; }
        if (cam)
        {
            transform.rotation = cam.transform.rotation;
            transform.Rotate(Vector3.up * initialRotation * 180);
        }
    }
}
