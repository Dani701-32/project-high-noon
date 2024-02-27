using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadFollow : MonoBehaviour
{
    Vector3 origPos;
    Transform parent;
    
    [SerializeField] Transform orientation;
    [SerializeField] float turnSpeed = 10;

    void Start()
    {
        origPos = transform.rotation.eulerAngles;
    }

    void Update()
    {
        parent = transform.parent;
        parent.rotation = orientation.rotation;
        transform.rotation = Quaternion.Euler(origPos.x, Mathf.LerpAngle(transform.rotation.eulerAngles.y - parent.rotation.eulerAngles.y, orientation.rotation.eulerAngles.y, Time.deltaTime * turnSpeed), 0);
    }
}
