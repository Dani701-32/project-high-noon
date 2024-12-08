using UnityEngine;

public class EnSphereLookState : State
{
    [Header("References")]
    [SerializeField] EnemyStateMachine stateMachine;
    [SerializeField] float searchRange;
    [SerializeField, ReadOnly] bool playerSeen;
    [SerializeField] State chaseState;
    
    public LayerMask playerMask;
    
    Transform mainBody;

    public override void SwitchIntoState()
    {
        StartAnim(stateMachine);
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
