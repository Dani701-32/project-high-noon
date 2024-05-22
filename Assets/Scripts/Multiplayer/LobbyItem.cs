using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField, ReadOnly]
    string id;

    [SerializeField, ReadOnly]
    string nameLobby;

    [SerializeField, ReadOnly]
    int countPlayers;

    [SerializeField, ReadOnly]
    int maxPlayers;

    [SerializeField, ReadOnly]
    string gameMode = "Capture the Flag";

    [SerializeField]
    private TMP_Text textName;

    [SerializeField]
    private TMP_Text textPlayers;

    [SerializeField]
    private TMP_Text textGameMode;

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
    }
}
