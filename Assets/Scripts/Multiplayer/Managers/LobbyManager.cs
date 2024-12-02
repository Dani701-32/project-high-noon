using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private LobbyUiManager lobbyUiManager;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private Lobby currentLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float lobbyUpdateTimer;
    private float lobbyUpdateTimerMax = 1.1f;
    private string KEY_GAME_MODE = "GameMode";
    private string KEY_START_GAME = "GameStarted";
    private string KEY_CHARACTER = "Character";

    [SerializeField, ReadOnly]
    private string playerId = "";
    public static LobbyManager Instance { get; private set; }
    [SerializeField, ReadOnly]
    private bool isMenu = true;


    private void Awake()
    {
        // Garante que este objeto persista entre cenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        lobbyUiManager = LobbyUiManager.Instance;
        isMenu = true;
        //Iniciando a parte asincrona da Unity
        var options = new InitializationOptions();
        await UnityServices.InitializeAsync(options);
        playerId = AuthenticationService.Instance.PlayerId;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Update is called once per frame
    void Update()
    {
        isMenu = SceneManager.GetActiveScene().name == "Menu";
        if (isMenu && lobbyUiManager == null)
        {
            lobbyUiManager = LobbyUiManager.Instance;

        }
        HandleLobbyHeartbeat();
        if (lobbyUiManager.lobbyIsOpen)
        {
            HanddleLobbyPollForUpdates();
        }
    }

    public async void StartMatch()
    {
        try
        {
            string relayCode = await CreateRealy();

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(
                joinedLobby.Id,
                new UpdateLobbyOptions
                {

                    Data = new Dictionary<string, DataObject>
                    {

                        {
                            KEY_START_GAME,
                            new DataObject(DataObject.VisibilityOptions.Member, relayCode)
                        }
                    }
                }
            );

            joinedLobby = lobby;
            hostLobby = joinedLobby;
            lobbyUiManager = null;
            isMenu = false;

            ConectionType.type = "host";
            StartCoroutine(LoadMatchAsync());
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }
    IEnumerator LoadMatchAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MultiplayerCTF");
        // SceneManager.LoadScene("TesteMultiplayer");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private async Task<string> CreateRealy()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(relayServerData);

            return joinCode;
        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
            return null;
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(
                joinCode
            );

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(relayServerData);

            ConectionType.type = "client";
            // SceneManager.LoadScene("TesteMultiplayer");
            StartCoroutine(LoadMatchAsync());
        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HanddleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                lobbyUiManager.UpdateLobby(
                    joinedLobby.Name,
                    joinedLobby.Players.Count,
                    joinedLobby.MaxPlayers,
                    joinedLobby.Data[KEY_GAME_MODE].Value
                );
                PrintPlayers(joinedLobby);
            }
            if (joinedLobby.Data[KEY_START_GAME].Value != "0")
            {
                if (!lobbyUiManager.IsLobbyHost)
                {
                    JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                }
                currentLobby = joinedLobby;
                joinedLobby = null;
            }
        }
    }

    //Cria um Lobby
    public async void CreateLobby(string nameLobby, int playerCount, string playerName, string gender)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(playerName, gender),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        KEY_GAME_MODE,
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            "CaptureTheFlag",
                            DataObject.IndexOptions.S1 //Define CaptureTheFlag como campo S1 para ser pesquisado
                        )
                    },
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                nameLobby,
                playerCount,
                createLobbyOptions
            );
            hostLobby = lobby;
            joinedLobby = hostLobby;

            lobbyUiManager.CloseCreateLobby();
            lobbyUiManager.OpenLobby(
                hostLobby.Name,
                hostLobby.Players.Count,
                hostLobby.MaxPlayers,
                hostLobby.Data[KEY_GAME_MODE].Value
            );

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            if (queryResponse.Results.Count == 0)
            {
                Debug.Log("Nenhum Lobby encontrado");
                return;
            }

            foreach (Lobby lobby in queryResponse.Results)
            {
                lobbyUiManager.AddLobby(
                    lobby.Id,
                    lobby.Name,
                    lobby.Players.Count,
                    lobby.MaxPlayers
                );
            }
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    public async void JoinLobby(string id, string playerName, string gender)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer(playerName, gender)
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByIdOptions);
            joinedLobby = lobby;
            lobbyUiManager.OpenLobby(
                joinedLobby.Name,
                joinedLobby.Players.Count,
                joinedLobby.MaxPlayers,
                joinedLobby.Data[KEY_GAME_MODE].Value
            );
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                MultiplayerManager.Instance.EndGame(true);
            }
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    currentLobby.Id,
                    AuthenticationService.Instance.PlayerId
                );
                NetworkManager.Singleton.Shutdown();

                SceneManager.LoadScene("Menu");
            }
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            lobbyUiManager.AddPlayer(player.Id, player.Data["PlayerName"].Value);
        }
    }

    private Player GetPlayer(string playerName, string gender)
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                },
                {
                    KEY_CHARACTER  ,
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, gender)
                }
            }
        };
        return player;
    }

    private string GetPlayerName(Lobby lobby)
    {
        string playerName = "";
        foreach (Player player in lobby.Players)
        {

            if (player.Id == playerId)
            {
                playerName = player.Data["PlayerName"].Value;

                break;
            }
        }
        return playerName;
    }

    public string GetPlayerName()
    {
        string namePlayer;
        if (currentLobby != null)
        {
            namePlayer = GetPlayerName(currentLobby);
        }
        else
        {
            namePlayer = GetPlayerName(hostLobby);
        }
        return namePlayer;
    }

    public string GetGender(){
        string gender = "";
        foreach (Player player in currentLobby.Players)
        {

            if (player.Id == playerId)
            {
                gender = player.Data[KEY_CHARACTER].Value;

                break;
            }
        }
        return gender;
    }

}
