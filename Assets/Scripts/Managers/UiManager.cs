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

    [SerializeField]
    Roller[] teamScores = new Roller[2];

    [Header("Screens")]
    public GameObject EndMatchScreen;

    [SerializeField]
    TMP_Text textStatus;

    [SerializeField]
    TMP_Text textTeam;

    GameManager gameManager;

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
        gameManager = GameManager.Instance;
        EndMatchScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.MatchOver)
        {
            UpdateTimerUi();
        }
    }

    void UpdateTimerUi()
    {
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            textTimer.text = "00:00";
            string result = gameManager.GetStatus();
            if (result != "Tie")
            {
                textStatus.text = "Victory";
                textTeam.text = result;
            }
            else
            {
                textStatus.text = "result";
                textTeam.text = "";
            }
            EndMatchScreen.SetActive(true);
            gameManager.GameOver();
            return;
        }

        currentTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        textTimer.text = timerString;
    }

    public void UpdatePointsUI(string team, int points)
    {
        switch (team)
        {
            default:
            case "Red":
                teamScores[0].SetPoints(points);
                break;
            case "blue":
                teamScores[1].SetPoints(points);
                break;
        }
    }

    public void EndMatch()
    {
        currentTime = 15f;
    }
}