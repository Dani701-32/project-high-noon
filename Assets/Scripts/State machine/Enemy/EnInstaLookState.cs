using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnInstaLookState : State
{
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] float searchRange;
    [SerializeField, ReadOnly] bool playerSeen;
    [SerializeField] State chaseState;
    
    public LayerMask playerMask;
    
    Transform mainBody;

    public override void SwitchIntoState()
    {
        playerSeen = false;
    }

    public override State RunCurrentState()
    {
        if (playerSeen || (stateMachine.trackingObject && stateMachine.trackingObject.activeInHierarchy) ) { return chaseState; }
        
        FindVisiblePlayers();
        
        return this;
    }

    void FindVisiblePlayers()
    {
        Collider[] playersAround = Physics.OverlapSphere(transform.position, searchRange, playerMask);
        if (playersAround.Length > 0 && playersAround[0].gameObject.activeInHierarchy)
        {
            stateMachine.trackingObject = playersAround[0].gameObject;
            playerSeen = true; 
        }
    }
}
