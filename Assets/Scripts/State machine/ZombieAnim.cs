using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnim : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] EnemyStateMachine esm;
    [SerializeField] State chaseState;
    [SerializeField] State stunnedState;
    [SerializeField] float animationBlendSpeed = 1;

    float lerp;
    int runID;
    int dieID;
    int stunID;
    bool stunned;
    
    void Start()
    {
        if (!anim)
        {
            Debug.LogError("Object " + gameObject.name + " has a zombie animation component but is missing one or more references!");
            this.enabled = false;
            return;
        }
        runID = Animator.StringToHash("runState");
        dieID = Animator.StringToHash("death");
        stunID = Animator.StringToHash("stunned");
        anim.SetFloat(runID, 0);
    }

    void FixedUpdate()
    {
        lerp = Mathf.Lerp(lerp, esm.currentState == chaseState ? 1 : 0, Time.deltaTime * animationBlendSpeed);
        anim.SetFloat(runID, lerp);
        
        if(esm.IsDying()) { anim.SetBool(dieID, true); }

        if (esm.currentState == stunnedState && !stunned)
        {
            stunned = true;
            anim.SetBool(stunID, true);
            Invoke("Unstun", 0.4f);
        }

        if (stunned && esm.currentState != stunnedState) { stunned = false; }
    }
    
    void Unstun() { anim.SetBool(stunID, false); }
}
