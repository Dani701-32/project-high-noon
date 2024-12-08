using System.Collections;
using UnityEngine;

public class EnWaitingState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State nextState;

    [Header("State data")] 
    [SerializeField] bool invulnerableOnEntry;
    [SerializeField] bool invulnerableOnExit;
    [SerializeField] bool isFinalState;
    public float waitTime = 0.01f;
    
    bool waiting = true;

    IEnumerator EndWaiting()
    {
        yield return new WaitForSeconds(waitTime);
        stateMachine.invulnerable = invulnerableOnExit;
        waiting = false;
    }

    public override void SwitchIntoState()
    {
        if (isFinalState)
        {
            if (stateMachine.nav) stateMachine.nav.isStopped = true;
            stateMachine.animator.Play("Dying");
        }
        StartAnim(stateMachine);
        waiting = true;
        stateMachine.invulnerable = invulnerableOnEntry;
        if(!isFinalState)
            StartCoroutine(EndWaiting());
    }

    public override State RunCurrentState()
    {
        if (!waiting) { return nextState; }
        return this;
    }
    
}
