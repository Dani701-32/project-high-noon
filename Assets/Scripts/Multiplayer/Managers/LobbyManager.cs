using System.Collections;
using System.Collections.Generic;
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

    private async void CreateRealy()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(relayServerData);
        }
        catch (RelayServiceException error)
        {
            Debug.Log(error);
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
                    joinedLobby.Data["GameMode"].Value
                );
                PrintPlayers(joinedLobby);
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
                        "GameMode",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            "CaptureTheFlag",
                            DataObject.IndexOptions.S1 //Define CaptureTheFlag como campo S1 para ser pesquisado
                        )
                    }
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
                hostLobby.Data["GameMode"].Value
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
                joinedLobby.Data["GameMode"].Value
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
            Debug.Log(player.Id);
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
