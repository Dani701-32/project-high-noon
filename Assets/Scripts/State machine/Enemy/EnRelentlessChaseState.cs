using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnRelentlessChaseState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State followupState;
    [SerializeField] State lookingState;
    [SerializeField] State[] statesWithAHeadStart;
    
    [Header("State data")]
    [SerializeField] bool switchStateWhenClose;
    [SerializeField] float targetDistanceForStateSwitch;
    [SerializeField] float turnSpeed;
    [SerializeField] float maxChaseSpeed = 1;
    [SerializeField] float speedGainMultiplier = 1;
    
    [Header("Changing stats")]
    [SerializeField, ReadOnly] float playerDistance;
    [SerializeField, ReadOnly] float currSpeed;
    
    Transform mainBody;
    Rigidbody rb;

    void Start()
    {
        mainBody = stateMachine.transform;
        rb = stateMachine.GetComponent<Rigidbody>();
    }

    public override void SwitchIntoState()
    {
        playerDistance = 9000;
        currSpeed = statesWithAHeadStart.Contains(stateMachine.previousState) ? maxChaseSpeed / 4 : 0;
    }

    public override State RunCurrentState()
    {
        GameObject target = stateMachine.trackingObject;
        if (!target) return lookingState;

        Vector3 pos = transform.position;
        Vector3 tpos = target.transform.position;
        playerDistance = Vector3.Distance(pos, tpos);

        if (switchStateWhenClose && playerDistance <= targetDistanceForStateSwitch)
            return followupState;

        Quaternion lookOnLook = Quaternion.LookRotation(tpos - pos);
        mainBody.rotation = Quaternion.Slerp(mainBody.rotation, lookOnLook, turnSpeed * Time.deltaTime);
        
        rb.AddForce(mainBody.forward * currSpeed, ForceMode.Acceleration);

        currSpeed = Mathf.Lerp(currSpeed, maxChaseSpeed, Time.deltaTime * speedGainMultiplier);
        
        return this;
    }
}
