using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    [Header("Controles da partida")]
    [SerializeField]
    private int currentPointRed = 0,
        currentPointBlue = 0;
    public int maxPoints = 3;

    [SerializeField]
    private FlagSpot flagSpot;
    private bool matchOver = false;
    public bool MatchOver
    {
        get => matchOver;
        private set { matchOver = value; }
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Game manager é nulo");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        MatchOver = false;
    }

    public void AddPoint(TeamData team)
    {
        switch (team.teamName)
        {
            default:
            case "Red":
                currentPointRed++;
                UiManager.Instance.UpdatePointsUI(team.teamName, currentPointRed);
                break;
            case "Blue":
                currentPointBlue++;
                UiManager.Instance.UpdatePointsUI(team.name, currentPointBlue);
                break;
        }
        if (currentPointRed >= maxPoints || currentPointBlue >= maxPoints)
        {
            EndGame();
            return;
        }
        ActivateFlag();
    }

    public void ActivateFlag()
    {
        flagSpot.ActiveFlag();
    }

    public void EndGame()
    {
        MatchOver = true;
        UiManager.Instance.EndMatch();
    }
}
