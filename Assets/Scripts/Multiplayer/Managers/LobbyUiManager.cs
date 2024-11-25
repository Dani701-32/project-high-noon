using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUiManager : MonoBehaviour
{
    static LobbyUiManager _instance;
    [SerializeField] private LobbyManager lobbyManager;

    [Header("Listar Lobbies")]
    [SerializeField]
    GameObject listLobbyScreen;

    [SerializeField]
    GameObject prefabLobbyItem;

    [SerializeField]
    Transform bodyList;

    [SerializeField, ReadOnly]
    List<LobbyItem> listLobbies;

    [Header("Criar Lobby")]
    [SerializeField]
    GameObject createLobbyPopUp;

    [SerializeField]
    TMP_InputField inputLobbyName;

    [SerializeField]
    TMP_InputField inputPlayerName;

    [SerializeField]
    TMP_InputField inputLobbPlayerCap;

    [Header("Abri Lobby")]
    [SerializeField]
    GameObject openLobbyPopUp;

    [SerializeField]
    GameObject prefabPlayerItem;

    [SerializeField]
    Transform bodyListPlayers;

    [SerializeField, ReadOnly]
    List<PlayerItem> listPlayers;

    [SerializeField]
    TMP_Text textNameLobbyOpen;

    [SerializeField]
    TMP_Text textLobbyOpenPlayers;

    [Header("Abri Lobby")]
    [SerializeField]
    GameObject joinLobbyScreen;

    [SerializeField]
    TMP_InputField inputLobbyJoinName;

    [SerializeField]
    private GameObject btnStartGame;

    [SerializeField, ReadOnly]
    private string joinLobbyId;

    [SerializeField, ReadOnly]
    private string relayCode;

    [SerializeField, ReadOnly]
    public bool lobbyIsOpen = false;

    [SerializeField, ReadOnly]
    private bool isLobbyHost = false;
    [SerializeField, ReadOnly ] private bool isLobbyList = false; 
    private float lobbyTimer = 1f; 
    private float lobbyMaxTimer = 5f; 

    public static LobbyUiManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("Lobby Ui Manager é nulo");
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
        
    }
    void Update()
    {
        if(isLobbyList){
            lobbyTimer -= Time.deltaTime;
            if (lobbyTimer < 0f)
            {
                lobbyTimer = lobbyMaxTimer;
                RefreshList(); 
            }
        }
    }

    private void Start()
    {
        lobbyManager = LobbyManager.Instance;
        foreach (Transform child in bodyList)
        {
            Destroy(child);
        }
        foreach (Transform child in bodyListPlayers)
        {
            Destroy(child);
        }
        btnStartGame.SetActive(false);
    }

    //Listar Lobbies
    public void OpenListLobbies()
    {
        RefreshList();
        isLobbyList = true; 
        listLobbyScreen.SetActive(true);
    }

    public void CloseListLobbies()
    {
        listLobbyScreen.SetActive(false);
        isLobbyList = false; 
    }

    public void RefreshList()
    {
        if (listLobbies.Count > 0)
        {
            foreach (LobbyItem listItem in listLobbies)
            {
                Destroy(listItem.gameObject);
            }
            listLobbies.Clear();
        }
        foreach (Transform child in bodyList)
        {
            Destroy(child);
        }
        lobbyManager.ListLobbies();
    }

    public void AddLobby(string id, string nameLobby, int countPlayers, int maxPlayers)
    {
        GameObject lobbyObject = Instantiate(prefabLobbyItem, bodyList);
        LobbyItem lobbyItem = lobbyObject.GetComponent<LobbyItem>();

        lobbyItem.SetLobby(id, nameLobby, countPlayers, maxPlayers);

        listLobbies.Add(lobbyItem);
    }

    //Entrar em lobby
    public void OpenPopUpJoin(string id)
    {
        joinLobbyScreen.SetActive(true);
        CloseListLobbies();
        joinLobbyId = id;
    }

    public void JoinLobby()
    {
        joinLobbyScreen.SetActive(false);
        string namePlayer = inputLobbyJoinName.text;
        lobbyManager.JoinLobby(joinLobbyId, namePlayer);
    }

    //Create Lobby
    public void OpenCreateLobby()
    {
        createLobbyPopUp.SetActive(true);
    }

    public void CloseCreateLobby()
    {
        inputLobbyName.text = "";
        inputLobbPlayerCap.text = "";
        createLobbyPopUp.SetActive(false);
    }

    public void CreateLobby()
    {
        string playerName = inputPlayerName.text ?? "player" + Random.Range(10, 100);
        string lobbyName = inputLobbyName.text ?? "lobby" + Random.Range(1, 100);
        int lobbyPlayerCap = int.Parse(inputLobbPlayerCap.text == ""? "4" : inputLobbPlayerCap.text);
        isLobbyHost = true;
        if(lobbyPlayerCap > 8){
            lobbyPlayerCap = 8;
        }
        lobbyManager.CreateLobby(lobbyName, lobbyPlayerCap, playerName);
    }

    //Abrir Lobby
    public void OpenLobby(string nameLobby, int numPlayers, int maxPlayers, string gameMode)
    {
        CloseListLobbies();
        textNameLobbyOpen.text = $"{nameLobby}: {gameMode}";
        textLobbyOpenPlayers.text = $"{numPlayers}/{maxPlayers}";
        RefreshPlayer();
        lobbyIsOpen = true;
        openLobbyPopUp.SetActive(true);
        if (isLobbyHost)
        {
            btnStartGame.SetActive(true);
        }
    }

    public void UpdateLobby(string nameLobby, int numPlayers, int maxPlayers, string gameMode)
    {
        textNameLobbyOpen.text = $"{nameLobby}: {gameMode}";
        textLobbyOpenPlayers.text = $"{numPlayers}/{maxPlayers}";
        RefreshPlayer();
    }

    public void RefreshPlayer()
    {
        if (listPlayers.Count > 0)
        {
            foreach (PlayerItem playerItem in listPlayers)
            {
                if (playerItem != null)
                    Destroy(playerItem.gameObject);
            }
            listPlayers.Clear();
        }
        foreach (Transform child in bodyListPlayers)
        {
            Destroy(child);
        }
    }

    public void LeaveLobby()
    {
        if (isLobbyHost)
        {
            isLobbyHost = false;
            btnStartGame.SetActive(false);
        }
        lobbyIsOpen = false;
        openLobbyPopUp.SetActive(false);
        lobbyManager.LeaveLobby();
        isLobbyList = true;
        RefreshList();
        listLobbyScreen.SetActive(true);
    }

    public void AddPlayer(string playerId, string playerName)
    {
        GameObject playerObject = Instantiate(prefabPlayerItem, bodyListPlayers);
        PlayerItem playerItem = playerObject.GetComponent<PlayerItem>();

        playerItem.SetPlayer(playerId, playerName);
        listPlayers.Add(playerItem);
    }

    public void StartMatch()
    {
        btnStartGame.SetActive(false);
        lobbyManager.StartMatch();
    }

    public bool IsLobbyHost
    {
        get { return isLobbyHost; }
        private set { isLobbyHost = value; }
    }
}
