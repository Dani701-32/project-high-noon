using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private LobbyUiManager lobbyUiManager;
    private Lobby hostLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;

    // Start is called before the first frame update
    async void Start()
    {
        lobbyUiManager = GetComponent<LobbyUiManager>();
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
    public async void CreateLobby(string nameLobby, int playerCount)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(nameLobby, playerCount);
            hostLobby = lobby;

            lobbyUiManager.CloseCreateLobby();
            lobbyUiManager.OpenLobby(lobby.Name, lobby.Players.Count, lobby.MaxPlayers);
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
}
