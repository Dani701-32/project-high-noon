using System.Collections;
using UnityEngine;

public class EnWaitingState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State nextState;
    [SerializeField] Animator animator;

    [Header("State data")] 
    [SerializeField] bool invulnerableOnEntry;
    [SerializeField] bool invulnerableOnExit;
    public float waitTime = 0.01f;
    
    bool waiting = true;
    static readonly int DoneSpawning = Animator.StringToHash("DoneSpawning"); // Nome de animação para spawn

    IEnumerator EndWaiting()
    {
        yield return new WaitForSeconds(waitTime);
        stateMachine.invulnerable = invulnerableOnExit;
        if (animator)
            animator.SetTrigger(DoneSpawning);
        waiting = false;
    }

    public override void SwitchIntoState()
    {
        waiting = true;
        stateMachine.invulnerable = invulnerableOnEntry;
        StartCoroutine(EndWaiting());
    }

    public override State RunCurrentState()
    {
        if (!waiting) { return nextState; }
        return this;
    }
    
}
