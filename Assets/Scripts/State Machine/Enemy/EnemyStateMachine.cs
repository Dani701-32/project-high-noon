using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public State currentState;
    public GameObject trackingObject;

    [SerializeField] bool hyperArmor;
    [SerializeField] int playerBulletMask;
    [SerializeField] State stunnedState;
    [SerializeField] State chasingState;

    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        State nextState = currentState?.RunCurrentState();

        if (nextState && nextState != currentState)
        {
            SwitchToNewState(nextState);
        }
    }

    private void SwitchToNewState(State nextState)
    {
        currentState = nextState;
        currentState.SwitchIntoState();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != playerBulletMask) return;
        if (currentState == stunnedState) return;

        Bullet bull = other.GetComponent<Bullet>();
        if (bull && bull.owner && bull.owner.activeInHierarchy)
            trackingObject = bull.owner;

        if (!hyperArmor) { SwitchToNewState(stunnedState); }
        else if (trackingObject) { SwitchToNewState(chasingState); }
    }
}
