using UnityEngine;

public class ShootableTarget : MonoBehaviour
{
    [SerializeField] AllTargetsGone targetParent;
    [SerializeField] bool manualTriggerCheck = true;
    bool executed;
    void OnCollisionEnter(Collision other)
    {
        if (!manualTriggerCheck || other.gameObject.layer != 7 || executed) return;

        gameObject.SetActive(false);
        targetParent.TargetCheck();
        executed = true;
    }

    void OnDisable()
    {
        if (manualTriggerCheck || executed) return;
        targetParent.TargetCheck();
        executed = true;
    }
}
