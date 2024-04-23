using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState;
    public GameObject trackingObject;

    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        State nextState = currentState?.RunCurrentState();

        if (nextState)
        {
            SwitchToNewState(nextState);
        }
    }

    private void SwitchToNewState(State nextState)
    {
        currentState = nextState;
    }
}
