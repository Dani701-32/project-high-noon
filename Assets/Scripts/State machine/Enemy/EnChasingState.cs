using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnChasingState : State
{
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField, ReadOnly] float playerDistance;

    [SerializeField] float targetDistance;
    [SerializeField] float lookSpeed;
    [SerializeField] EnFiringState firingState;
    [SerializeField] EnLookingState lookingState;

    Transform mainBody;

    void Start()
    {
        mainBody = stateMachine.transform;
    }
    
    public override void SwitchIntoState() {}

    public override State RunCurrentState()
    {
        GameObject target = stateMachine.trackingObject;
        if (!target) return lookingState;
        
        playerDistance = Vector3.Distance(transform.position, target.transform.position);
        if (playerDistance <= targetDistance) return firingState;

        Quaternion lookOnLook = Quaternion.LookRotation(target.transform.position - transform.position);
        mainBody.rotation = Quaternion.Slerp(mainBody.rotation, lookOnLook, lookSpeed * Time.deltaTime);
        
        return this;
    }
}
