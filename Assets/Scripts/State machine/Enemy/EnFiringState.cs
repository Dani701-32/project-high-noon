using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnFiringState : State
{
    [SerializeField] bool lostSight;
    
    [SerializeField] EnLookingState lookingState;
    
    public override void SwitchIntoState() {}
    
    public override State RunCurrentState()
    {
        if (lostSight) return lookingState;
        
        return this;
    }
}
