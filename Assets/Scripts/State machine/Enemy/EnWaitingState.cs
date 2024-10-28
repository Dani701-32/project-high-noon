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

    void EndWaiting()
    {
        stateMachine.invulnerable = invulnerableOnExit;
        if (animator)
            animator.SetTrigger(DoneSpawning);
        waiting = false;
    }

    public override void SwitchIntoState()
    {
        stateMachine.invulnerable = invulnerableOnEntry;
        Invoke(nameof(EndWaiting), waitTime);
    }

    public override State RunCurrentState()
    {
        if (!waiting) { return nextState; }
        return this;
    }
    
}
