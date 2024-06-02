using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    private LobbyUiManager lobbyUiManager;

    [SerializeField, ReadOnly]
    private string id;
    private string nameLobby;
    private int countPlayers;
    private int maxPlayers;

    [SerializeField, ReadOnly]
    private string gameMode = "Capture the Flag";

    [SerializeField]
    private TMP_Text textName;

    [SerializeField]
    private TMP_Text textPlayers;

    [SerializeField]
    private TMP_Text textGameMode;
    [SerializeField] private GameObject buttonJoin; 

    private void Start()
    {
        lobbyUiManager = LobbyUiManager.Instance;
    }

    public void SetLobby(string id, string nameLobby, int countPlayers, int maxPlayers)
    {
        this.id = id;
        this.nameLobby = nameLobby;
        this.countPlayers = countPlayers;
        this.maxPlayers = maxPlayers;
        SetUI();
    }

    private void SetUI()
    {
        textName.text = nameLobby;
        textPlayers.text = $"{countPlayers}/{maxPlayers}";
        textGameMode.text = gameMode;
         buttonJoin.SetActive(countPlayers < maxPlayers);
    }

    public void JoinLobby()
    {
        lobbyUiManager.OpenPopUpJoin(id);
    }
}
