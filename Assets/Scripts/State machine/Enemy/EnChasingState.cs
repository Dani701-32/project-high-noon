using System.Linq;
using UnityEngine;

public class EnChasingState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] State followupState;
    [SerializeField] State lookingState;
    [SerializeField] State[] statesWithAHeadStart;
    [SerializeField] Transform raycastOrigin;
    
    [Header("State data")]
    [SerializeField] bool switchStateWhenClose;
    [SerializeField] bool relentless;
    [SerializeField] float targetDistanceForStateSwitch = 3;
    [SerializeField] float angleDifferenceForStateSwitch = 20;
    [SerializeField] float maxChaseSpeed = 1;
    [SerializeField] float speedGainMultiplier = 1;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float rotationSpeed = 5.5f;
    
    [Header("Changing stats")]
    [SerializeField, ReadOnly] float playerDistance;
    [SerializeField, ReadOnly] float currSpeed;
    
    Transform mainBody;
    Transform rotationLerp;
    Rigidbody rb;

    void Start()
    {
        rotationLerp = transform;
        mainBody = stateMachine.transform;
        rb = stateMachine.GetComponent<Rigidbody>();
    }

    public override void SwitchIntoState()
    {
        stateMachine.nav.isStopped = false;
        StartAnim(stateMachine);
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

        if (switchStateWhenClose && playerDistance <= targetDistanceForStateSwitch &&
            GetAngle(tpos) < angleDifferenceForStateSwitch)
        {
            stateMachine.nav.isStopped = true;
            if (rb) rb.velocity = Vector3.zero;
            return followupState;
        }
            

        /*
        Vector3 targetPostition = new Vector3(tpos.x, pos.y, tpos.z);
        rotationLerp.LookAt(targetPostition);
        mainBody.rotation = Quaternion.Slerp(mainBody.rotation, rotationLerp.rotation, Time.deltaTime * rotationSpeed);
        
        rb.AddForce(mainBody.forward * currSpeed, ForceMode.Acceleration);

        currSpeed = Mathf.Lerp(currSpeed, maxChaseSpeed, Time.deltaTime * speedGainMultiplier);
        */
        stateMachine.nav.SetDestination(tpos);

        if (!relentless)
        {
            Vector3 dirToTarget = (tpos - pos).normalized;
            if (Physics.Raycast(raycastOrigin.position, dirToTarget, playerDistance, obstacleMask))
                stateMachine.trackingObject = null;
        }
        
        return this;
    }

    float GetAngle(Vector3 target)
    {
        Vector3 targetDir = target - transform.position;
        Vector3 forward = mainBody.forward;
        return Mathf.Abs(Vector3.SignedAngle(targetDir, forward, Vector3.up));
    }
}
