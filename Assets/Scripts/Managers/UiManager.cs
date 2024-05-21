using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class UiManager : NetworkBehaviour
{
    public static UiManager Instance;

    [Header("Match Variables")]
    public float matchDuration = 240f;
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

    [SerializeField]
    MultiplayerManager multiplayerManager;
    bool matchIsOver = false;

    public override void OnNetworkSpawn()
    {
        multiplayerManager = MultiplayerManager.Instance;
    }

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
        multiplayerManager = MultiplayerManager.Instance ?? null;
        gameManager = GameManager.Instance ?? null;
        currentTime = matchDuration;
        EndMatchScreen.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        matchIsOver = gameManager == null ? multiplayerManager.MatchOver : gameManager.MatchOver;
        if (multiplayerManager)
        {
            if (IsServer)
            {
                currentTime = multiplayerManager.currentTime;
            }
            else
            {
                currentTime = multiplayerManager.networkCurrentTime.Value;
            }
        }
        if (!matchIsOver)
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

            string result =  gameManager == null ? multiplayerManager.GetStatus() : gameManager.GetStatus();
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
            if (IsServer ||  gameManager == null)
            {
                multiplayerManager.GameOver();
            }
            else
            {
                gameManager.GameOver();
            }

            return;
        }
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
            case "Blue":
                teamScores[1].SetPoints(points);
                break;
        }
    }

    public void EndMatch()
    {
        currentTime = 15f;
    }
}
