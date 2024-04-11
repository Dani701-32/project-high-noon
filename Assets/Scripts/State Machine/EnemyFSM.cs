using System;
using System.Collections;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    StateFSM state;

    void Start()
    {
        throw new NotImplementedException();
    }

    void Update()
    {
        state?.Update();
    }

    public void SetState(StateFSM state)
    {
        state?.Exit();
        this.state = state;
        state?.Enter();
    }
}
