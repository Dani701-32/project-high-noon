using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnStunnedState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State ChaseState;
    [SerializeField] State LookingState;
    [SerializeField] AudioClip[] grunts;
    [SerializeField] AudioSource gruntSource;

    [SerializeField] float stunTime;
    [SerializeField, ReadOnly] bool stunned = true;

    public override void SwitchIntoState()
    {
        if (stateMachine.nav)
            stateMachine.nav.isStopped = true;
        stateMachine.animator.Play("Idle");
        stunned = true;
        StartCoroutine("StunTimer");
        if (gruntSource && !gruntSource.isPlaying && grunts.Length > 0)
        {
            gruntSource.clip = grunts[Random.Range(0, grunts.Length)];
            gruntSource.Play();
        }
            
    }

    public override State RunCurrentState()
    {
        if (!stunned)
        {
            if (!ChaseState && !LookingState) stateMachine.currentState = null;
            return stateMachine.trackingObject && stateMachine.trackingObject.activeInHierarchy ? ChaseState : LookingState;
        }
            
        
        return this;
    }

    private IEnumerator StunTimer()
    {
        yield return new WaitForSeconds(stunTime);
        stunned = false;
    }
}
