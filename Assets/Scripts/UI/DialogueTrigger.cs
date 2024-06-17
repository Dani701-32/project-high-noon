using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] [TextArea(3,3)] string[] texts;
    [SerializeField] UnityEvent events;
    [SerializeField] bool vanishOnTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TextWindow tx = EventManager.Instance.canvasTextWindow;
        if (tx != null)
        {
            tx.texts = texts;
            tx.dialogueEndEvents = events;
            tx.StartText();
            if(vanishOnTrigger)
                gameObject.SetActive(false);
        }
            
    }
}
