using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GenericTrigger : MonoBehaviour
{
    [SerializeField] float waitTime;
    [SerializeField] UnityEvent events;
    [SerializeField] bool vanishOnTrigger = true;
    bool running;

    void Start() { waitTime = Mathf.Max(0, waitTime); }

    void OnTriggerEnter(Collider other)
    {
        if (running) return;
        if (!other.CompareTag("Player")) return;
        if (waitTime > 0)
            StartCoroutine(StartDelayedEvent());
        else
        {
            events.Invoke();
            if (vanishOnTrigger)
                gameObject.SetActive(false);
        }
    }

    IEnumerator StartDelayedEvent()
    {
        running = true;
        yield return new WaitForSeconds(waitTime);
        events.Invoke();
        if (vanishOnTrigger)
            gameObject.SetActive(false);
        else
            running = false;
    }
}
