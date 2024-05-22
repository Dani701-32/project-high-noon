using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private LobbyUiManager lobbyUiManager;
    private Lobby hostLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;

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

    //Cria um Lobby
    public async void CreateLobby(string nameLobby, int playerCount, string playerName)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(playerName)
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                nameLobby,
                playerCount,
                createLobbyOptions
            );
            hostLobby = lobby;

            lobbyUiManager.CloseCreateLobby();
            lobbyUiManager.OpenLobby(hostLobby.Name, hostLobby.Players.Count, hostLobby.MaxPlayers);
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

    public async void JoinLobby(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer("testeName")
            };
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByIdOptions);
            lobbyUiManager.OpenLobby(
                joinedLobby.Name,
                joinedLobby.Players.Count,
                joinedLobby.MaxPlayers
            );
            PrintPlayers(joinedLobby);
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
