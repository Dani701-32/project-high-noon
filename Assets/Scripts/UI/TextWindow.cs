using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextWindow : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] TextMeshProUGUI textBox;
    
    [TextArea(3,3)] public string[] texts;
    public UnityEvent dialogueEndEvents;

    int step;
    bool rolling;
    EventManager events;

    void Start()
    {
        events = EventManager.Instance;
    }

    void Update()
    {
        if (rolling) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            AdvanceText();
        }
    }

    public void StartText()
    {
        rolling = true;
        events.playerMove.canMove = false;
        events.playerGun.gunLocked = true;
        step = 0;
        textBox.text = texts[0];
        animator.SetBool("Active", true);
        Invoke("ReturnControl", 0.5f);  // rolling = false
    }

    public void EndText()
    {
        animator.SetBool("Active", false);
        rolling = true;
        events.playerMove.canMove = true;
        step = 0;
        Invoke("DelayedExecution", 0.5f);
    }

    public void AdvanceText()
    {
        step++;
        if (step >= texts.Length)
        {
            EndText();
            return;
        }
        textBox.text = texts[step];
    }

    void ReturnControl() { rolling = false; }

    void DelayedExecution()
    {
        if (!events.greaterGunLock) 
            events.playerGun.gunLocked = false;
        if (dialogueEndEvents != null)
            dialogueEndEvents.Invoke();
        dialogueEndEvents = null;
    }
}
