using System.Collections;
using UnityEngine;

public class EnStunnedState : State
{
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State ChaseState;
    [SerializeField] State LookingState;

    [SerializeField] float stunTime;
    [SerializeField, ReadOnly] bool stunned = true;

    public override void SwitchIntoState()
    {
        stunned = true;
        StartCoroutine("StunTimer");
    }

    public override State RunCurrentState()
    {
        if (!stunned) return ChaseState;
        
        return this;
    }

    private IEnumerator StunTimer()
    {
        yield return new WaitForSeconds(stunTime);
        stunned = false;
    }
}
