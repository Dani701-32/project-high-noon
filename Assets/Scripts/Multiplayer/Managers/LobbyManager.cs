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
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float lobbyUpdateTimer;
    private float lobbyUpdateTimerMax = 1.1f;
    private string KEY_GAME_MODE = "GameMode";
    private string KEY_START_GAME = "GameStarted";

    // Start is called before the first frame update
    async void Start()
    {
        lobbyUiManager = LobbyUiManager.Instance;
        //Iniciando a parte asincrona da Unity
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Player Id {AuthenticationService.Instance.PlayerId}");
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Update is called once per frame
    void Update()
    {
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
            Debug.Log("Star Match");

            string relayCode = await CreateRealy();
            Debug.Log(relayCode);

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

            ConectionType.type = "host";
            SceneManager.LoadScene("TesteMultiplayer");
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
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
            SceneManager.LoadScene("TesteMultiplayer");
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
                joinedLobby = null;
            }
        }
    }

    //Cria um Lobby
    public async void CreateLobby(string nameLobby, int playerCount, string playerName)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(playerName),
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
            Debug.Log($"Lobby Encontrados:{queryResponse.Results.Count} results");
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

    public async void JoinLobby(string id, string playerName)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer(playerName)
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
            await LobbyService.Instance.RemovePlayerAsync(
                joinedLobby.Id,
                AuthenticationService.Instance.PlayerId
            );
        }
        catch (LobbyServiceException error)
        {
            Debug.Log(error);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log(lobby.Players.Count);
        foreach (Player player in lobby.Players)
        {
            lobbyUiManager.AddPlayer(player.Id, player.Data["PlayerName"].Value);
        }
    }

    private Player GetPlayer(string playerName)
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)
                }
            }
        };
        return player;
    }
}
