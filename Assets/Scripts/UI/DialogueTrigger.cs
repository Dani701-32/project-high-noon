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
        if (tx != null || texts.Length == 0)
        {
            if (texts.Length > 0)
            {
                tx.texts = texts;
                tx.StartText();
                tx.dialogueEndEvents = events;
                if(vanishOnTrigger)
                    gameObject.SetActive(false);
            }
            else
            {
                events.Invoke();
                if(vanishOnTrigger)
                    gameObject.SetActive(false);
            }
        }
            
    }
}
