using System.Collections;
using UnityEngine;

public class Roller : MonoBehaviour
{
    [SerializeField] int framesToRollFor = 10;
    [SerializeField] RectTransform leftRoller;
    [SerializeField] RectTransform rightRoller;
    
    float height;
    int currNumber;
    bool working;
    IEnumerator leftRoll;
    IEnumerator rightRoll;
    void Start()
    {
        if (leftRoller == null || rightRoller == null)
        {
            this.enabled = false;
            return;
        }
        height = leftRoller.sizeDelta.y / 12;
    }

    public void AddPoint(int points)
    {
        if (working) return;
        int roll = Mathf.Clamp(points + currNumber, 0, 99);
        StartRoll(roll);
    }
    public void SetPoints(int points)
    {
        if (working) return;
        int roll = Mathf.Clamp(points, 0, 99);
        StartRoll(roll);
    }

    private void StartRoll(int target)
    {
        if (target == currNumber) return;
        
        string newTarget = target.ToString();
        if (newTarget.Length == 1) newTarget = "0" + newTarget;
        string individualNumber = currNumber.ToString();
        if (individualNumber.Length == 1) individualNumber = "0" + individualNumber;

        leftRoll = RollTo(leftRoller, newTarget[0] - '0', individualNumber[0] - '0');
        rightRoll = RollTo(rightRoller, newTarget[1] - '0', individualNumber[1] - '0');
        StartCoroutine(leftRoll);
        StartCoroutine(rightRoll);
        currNumber = target;
    }

    IEnumerator RollTo(RectTransform rect, int target, int myNumber)
    {
        int panic = 0;
        int roll = 0;
        int panicMax = 400;
        if (myNumber == 0 && target == 9) target = -1;
        if (myNumber == 9 && target == 0) target = 10;
        int plusMinus = myNumber > target ? -1 : 1;
        while (myNumber != target)
        {
            working = true;
            rect.anchoredPosition += Vector2.up * plusMinus * height/framesToRollFor;
            roll++;
            if (roll >= framesToRollFor)
            {
                myNumber += 1 * plusMinus;
                roll = 0;
            }
            panic++;
            if (panic >= panicMax)
            {
                Debug.LogWarning("Paniced!");
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        if (myNumber == 10) rect.anchoredPosition -= Vector2.up * height * 10;
        if (myNumber == -1) rect.anchoredPosition -= Vector2.down * height * 10;
        working = false;
    }
}
