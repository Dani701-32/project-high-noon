using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MultiplayerManager : NetworkBehaviour
{
    static MultiplayerManager _instance;
    [SerializeField] private Transform[] spawnPointsRed, spawnPointsBlue;
    [SerializeField] private TeamData[] teamDatas;
    [SerializeField] private List<PlayerOnline> playersRed, playersBlue;
    public Transform defaultPos;
    [Header("Controles da partida")]
    [SerializeField] private int currentPointRed = 0;
    [SerializeField] private int currentPointBlue = 0;
    public int maxPoints = 3;
    [SerializeField] private FlagSpot flagSpot;
    private bool matchOver = false;
    public static MultiplayerManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Multiplayer manager é nulo");
            return _instance;
        }
    }
    public bool MatchOver
    {
        get => matchOver;
        private set { matchOver = value; }
    }
    private void Awake()
    {
        _instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void GameOver()
    {
        MatchOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine("ReturnMenu");
    }

    public TeamData GetTeamData(PlayerOnline newPlayer)
    {
        if (playersRed.Count == playersBlue.Count)
        {
            newPlayer.SpawnPoint(spawnPointsBlue[0]);
            playersBlue.Add(newPlayer);
            return teamDatas[0];
        }
        newPlayer.SpawnPoint(spawnPointsRed[0]);
        playersRed.Add(newPlayer);
        return teamDatas[1];
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
                UiManager.Instance.UpdatePointsUI(team.teamName, currentPointBlue);
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
        UiManager.Instance.EndMatch();
    }
    public string GetStatus()
    {
        if (currentPointRed == maxPoints || currentPointRed > currentPointBlue)
        {
            return "Red Team";
        }
        else if (currentPointBlue == maxPoints || currentPointRed < currentPointBlue)
        {
            return "Blue Team";
        }
        else
        {
            return "Tie";
        }
    }
    IEnumerator ReturnMenu()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Menu");
    }

}
