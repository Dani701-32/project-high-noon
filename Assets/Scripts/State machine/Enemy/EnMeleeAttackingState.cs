using UnityEngine;

public class EnMeleeAttackingState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State followupState;
    [SerializeField] Collider attackHitbox;

    [Space]
    [Header("State data")] 
    [SerializeField] float attackDuration;
    [SerializeField] float attackInitialThrustForward;
    [SerializeField] float attackConstantThrust;
    [Space][Space][Space]
    [SerializeField] bool setNewWaitTimeOnEnd;
    [SerializeField] float newWaitTime;

    float duration;
    bool doneAttacking;
    Transform mainBody;
    GameObject target;
    Rigidbody rb;

    void Start()
    {
        rb = stateMachine.GetComponent<Rigidbody>();
        mainBody = stateMachine.transform;
        newWaitTime = Mathf.Max(0, newWaitTime);
        
        if(setNewWaitTimeOnEnd && followupState.GetType() != typeof(EnWaitingState))
            Debug.LogWarning("Novo estado após ataque não é EnWaitingState, mas o estado ainda está passando um novo tempo de espera ao objeto.");
    }

    public override void SwitchIntoState()
    {
        target = stateMachine.trackingObject;
        if (target)
        {
            Vector3 tpos = target.transform.position;
            transform.rotation = mainBody.rotation;
            Vector3 targetPostition = new Vector3(tpos.x, transform.position.y, tpos.z);
            mainBody.LookAt(targetPostition);
        }
        
        if(rb && Mathf.Abs(attackInitialThrustForward) > 0)
            rb.AddForce(mainBody.forward * attackInitialThrustForward, ForceMode.Impulse);
        
        attackHitbox.enabled = true;
        duration = 0;
        doneAttacking = false;
    }

    public override State RunCurrentState()
    { 
        if (duration >= attackDuration)
        {
            attackHitbox.enabled = false;
            if(setNewWaitTimeOnEnd)
                stateMachine.SetWaitingTime(newWaitTime);
            return followupState;
        }

        duration += Time.deltaTime;
        if(rb && Mathf.Abs(attackConstantThrust) > 0)
            rb.AddForce(mainBody.forward * attackConstantThrust, ForceMode.Acceleration);
        return this;
    }

    void OnDrawGizmos()
    {
        Bounds bounds = attackHitbox.bounds;
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(bounds.center, bounds.size);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (doneAttacking) return;
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerStats playerHit = other.gameObject.GetComponentInParent<PlayerStats>();
            playerHit.Damage(1);
            doneAttacking = true;
        }
    }
}
