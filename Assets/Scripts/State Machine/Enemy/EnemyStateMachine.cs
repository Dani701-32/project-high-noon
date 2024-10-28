using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [Header("States")]
    public State currentState;
    public State previousState;
    [SerializeField] State stunnedState;
    [SerializeField] State chasingState;
    [SerializeField] State waitingState;
    
    [Header("Enemy stats")]
    public GameObject trackingObject;
    public bool invulnerable;
    [SerializeField] bool hyperArmor;
    [SerializeField] int startingHealth = 10;
    [SerializeField, ReadOnly] int HP;
    
    [Header("Misc")]
    [SerializeField] int playerBulletMask;
    
    bool dying;
    Collider enemyCollider;

    void Start()
    {
        HP = startingHealth;
        enemyCollider = GetComponent<Collider>();
        if(currentState)
            currentState.SwitchIntoState();
    }

    void FixedUpdate()
    {
        if (dying) return;
        if (HP <= 0)
        {
            Death();
            dying = true;
            return;
        }
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        State nextState = currentState?.RunCurrentState();

        if (nextState && nextState != currentState)
        {
            previousState = currentState;
            SwitchToNewState(nextState);
        }
    }

    private void SwitchToNewState(State nextState)
    {
        currentState = nextState;
        currentState.SwitchIntoState();
    }

    void OnCollisionEnter(Collision other)
    {
        // Se não colidimos com uma bala ou estamos morrendo pare a execução
        if (dying || invulnerable) return;
        if (other.gameObject.layer != playerBulletMask) return;

        // Fomos atingidos por uma bala válida? Caso sim, reduza nosso HP, procure seu dono e faça ele nosso alvo
        Bullet bull = other.gameObject.GetComponent<Bullet>();
        if (bull)
        {
            if (bull.owner && bull.owner.activeInHierarchy && chasingState) 
                trackingObject = bull.owner;
            HP -= bull.damage;
            Destroy(bull.gameObject);
        }
        
        if (!hyperArmor) { SwitchToNewState(stunnedState); }
        else if (trackingObject) { SwitchToNewState(chasingState); }
    }

    void Death()
    {
        if (enemyCollider) { enemyCollider.enabled = false; }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) { rb.useGravity = false; }
        Destroy(gameObject, 4);
    }

    public bool IsDying() { return dying; }

    public void SetWaitingTime(float newTime)
    {
        if (!waitingState) return;
        var waiting = (EnWaitingState)waitingState;
        waiting.waitTime = newTime;
    }
}
