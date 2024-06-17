using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AllTargetsGone : MonoBehaviour
{
    [SerializeField, ReadOnly] List<GameObject> targets;
    [SerializeField] UnityEvent targetsDestroyedEvents;

    void Start()
    {
        foreach (Transform child in gameObject.transform)
            if(child.gameObject != gameObject) targets.Add(child.gameObject);
    }

    public void TargetCheck()
    {
        int destroy = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i] || (targets[i] && !targets[i].activeInHierarchy))
                destroy++;
        }

        if (destroy == targets.Count)
        {
            targetsDestroyedEvents.Invoke();
            Invoke("DelayedDeactivate", 0.1f);
        }
    }

    void DelayedDeactivate()
    {
        if(gameObject.activeInHierarchy) gameObject.SetActive(false);
    }
}
