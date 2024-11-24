using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using static Unity.Netcode.NetworkManager;
using Random = UnityEngine.Random;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class MultiplayerManager : NetworkBehaviour
{
    static MultiplayerManager _instance;
    public Transform[] spawnPointsRed,
        spawnPointsBlue;

    [SerializeField]
    private TeamData[] teamDatas;

    [SerializeField]
    public List<PlayerOnline> playersRed,
        playersBlue;
    public Transform defaultPos;

    [Header("Externos")]
    [SerializeField]
    GameObject cameraArea;

    [Header("Controles da partida")]
    [SerializeField]
    private int currentPointRed = 0;

    [SerializeField]
    private int currentPointBlue = 0;
    public int maxPoints = 3;

    [SerializeField]
    private FlagSpot flagSpot;
    private bool matchOver = false;
    public float matchDuration = 300f;
    public float currentTime = 0f;
    private bool gameStart = false;

    [Header("Online Variables")]
    public NetworkVariable<float> networkCurrentTime = new NetworkVariable<float>();

    int m_RoundRobinIndex = 0;
    [SerializeField] SpawnMethod m_SpawnMethod;
    [SerializeField] List<Vector3> m_SpawnPositions;

    public static MultiplayerManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("Multiplayer manager é nulo");
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
        // NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalWithRandomSpawnPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        switch (ConectionType.type)
        {
            default:
            case "host":

                Instance.StartGame();
                cameraArea.SetActive(false);
                NetworkManager.Singleton.StartHost();
                break;
            case "server":
                Instance.StartGame();
                cameraArea.SetActive(false);
                NetworkManager.Singleton.StartServer();
                break;
            case "client":
                Instance.StartGame();
                cameraArea.SetActive(false);
                Debug.Log(unityTransport.ConnectionData.Address);
                NetworkManager.Singleton.StartClient();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerUi();
        
    }

    public void StartGame()
    {
        currentTime = matchDuration;
        if (IsServer)
        {
            networkCurrentTime.Value = matchDuration;
        }

        gameStart = true;
    }

    void UpdateTimerUi()
    {
        if (!gameStart)
            return;
        if (IsServer)
        {
            currentTime -= Time.deltaTime;
            networkCurrentTime.Value = currentTime;
        }
    }

    public void GameOver()
    {
        gameStart = false;
        MatchOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine("ReturnMenu");
    }

    public TeamData GetTeamData(PlayerOnline newPlayer)
    {
        if (playersRed.Count == playersBlue.Count)
        {
            // newPlayer.SpawnPoint(spawnPointsBlue[0]);
            playersBlue.Add(newPlayer);
            return teamDatas[0];
        }
        // newPlayer.SpawnPoint(spawnPointsRed[0]);
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

    public void EndGame(bool fromHost = false)
    {
        if (IsServer)
        {
            currentTime = 15f;
            networkCurrentTime.Value = currentTime;
        }
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
        NetworkManager.Singleton.Shutdown();
        Cleanup();
        LobbyManager.Instance.LeaveLobby();
        SceneManager.LoadScene("Menu");
    }

    void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
    public Vector3 GetNextSpawnPosition(){
        switch (m_SpawnMethod)
        {
            case SpawnMethod.Random:
                var index = Random.Range(0, m_SpawnPositions.Count);
                return m_SpawnPositions[index];
            case SpawnMethod.RoundRobin:
                m_RoundRobinIndex = (m_RoundRobinIndex+1) % m_SpawnPositions.Count;
                return m_SpawnPositions[m_RoundRobinIndex];
            default:
                throw new NotImplementedException(); 
        }
    }
    
}

enum SpawnMethod
{
    Random = 0,
    RoundRobin = 1,
}
