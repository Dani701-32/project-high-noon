using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Match Variables")]
    public float matchDuration = 300f;
    public float currentTime = 0f;
    public TMP_Text textTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentTime = matchDuration;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerUi();
    }

    void UpdateTimerUi()
    {
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            textTimer.text = "00:00";
            return;
        }

        currentTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        textTimer.text = timerString;
    }
}
