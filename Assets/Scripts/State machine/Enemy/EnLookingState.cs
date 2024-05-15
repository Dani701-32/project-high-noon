using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnLookingState : State
{
    [SerializeField] EnemyStateMachine stateMachine;
    
    [SerializeField, ReadOnly] bool playerSeen;
    [SerializeField] State chaseState;

    public float viewRadius;
    [Range(0, 360)] public float viewCone;
    public LayerMask playerMask;
    public LayerMask obstaclesMask;
    [SerializeField] float lookSinePeriod = 1;
    [SerializeField] float lookSineAmplitude = 1;
    
    Transform mainBody;
    float startAngle;
    float sine;

    void Start()
    {
        mainBody = stateMachine.transform;
        startAngle = mainBody.eulerAngles.y;
        sine = 0;
    }

    public override void SwitchIntoState()
    {
        playerSeen = false;
        startAngle = mainBody.eulerAngles.y;
        sine = 0;
    }

    public override State RunCurrentState()
    {
        if (playerSeen || (stateMachine.trackingObject && stateMachine.trackingObject.activeInHierarchy) ) { return chaseState; }

        mainBody.eulerAngles = new Vector3(0, startAngle + Mathf.Sin(sine/lookSinePeriod) * lookSineAmplitude, 0);
        FindVisiblePlayers();
        sine += Time.deltaTime;
        
        return this;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0,
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
        );
    }

    void FindVisiblePlayers()
    {
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewCone, playerMask);
        for (int i = 0; i < targetsInView.Length; i++)
        {
            Transform target = targetsInView[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewCone / 2)
            {
                float targDist = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, targDist, obstaclesMask))
                {
                    stateMachine.trackingObject = target.gameObject;
                    playerSeen = true;
                }
            }
        }
    }
}
