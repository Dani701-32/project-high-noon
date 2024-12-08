using System.Collections;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool startsNewAnimation;
    public string animationTriggerName;
    public bool quickTriggerReset = true;
    
    public abstract void SwitchIntoState();
    public abstract State RunCurrentState();

    protected void StartAnim(EnemyStateMachine esm)
    {
        Animator anim = esm.animator;
        if (!startsNewAnimation || animationTriggerName.Equals("") || !anim) return;
        anim.SetTrigger(animationTriggerName);
        if (quickTriggerReset)
        {
            IEnumerator trigger = QuickTriggerReset(anim, animationTriggerName);
            StartCoroutine(trigger);
        }
    }

    IEnumerator QuickTriggerReset(Animator anim, string trigger)
    {
        yield return new WaitForSeconds(0.05f);
        anim.ResetTrigger(trigger);
    }
}
