using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GenericTrigger : MonoBehaviour
{
    [SerializeField] float waitTimeForEventStart;
    [SerializeField] UnityEvent events;
    [SerializeField] bool vanishOnTrigger = true;
    [SerializeField] GameObject[] objectsToHideOnVanish;
    [SerializeField] float respawnAfterSeconds;
    
    bool running;
    Collider trigger;

    void Start()
    {
        trigger = GetComponent<Collider>();
        waitTimeForEventStart = Mathf.Max(0, waitTimeForEventStart);
    }

    void End()
    {
        trigger.enabled = false;
        foreach (GameObject obj in objectsToHideOnVanish)
            obj.SetActive(false);
        if (respawnAfterSeconds > 0)
            StartCoroutine(WaitThenRespawn());
    }

    void OnTriggerEnter(Collider other)
    {
        if (running) return;
        if (!other.CompareTag("Player")) return;
        if (waitTimeForEventStart > 0)
            StartCoroutine(StartDelayedEvent());
        else
        {
            events.Invoke();
            if (vanishOnTrigger)
                End();
        }
    }

    IEnumerator StartDelayedEvent()
    {
        running = true;
        yield return new WaitForSeconds(waitTimeForEventStart);
        events.Invoke();
        if (vanishOnTrigger)
            End();
        else
            running = false;
    }

    IEnumerator WaitThenRespawn()
    {
        yield return new WaitForSeconds(respawnAfterSeconds);
        trigger.enabled = true;
        foreach (GameObject obj in objectsToHideOnVanish)
            obj.SetActive(true);
    }
}
