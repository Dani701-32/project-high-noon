using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUiManager : MonoBehaviour
{
    private LobbyManager lobbyManager;

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
    TMP_InputField inputLobbPlayerCap;

    [Header("Abri Lobby")]
    [SerializeField]
    GameObject openLobbyPopUp;

    [SerializeField]
    TMP_Text textNameLobbyOpen;

    [SerializeField]
    TMP_Text textLobbyOpenPlayers;

    private void Start()
    {
        lobbyManager = GetComponent<LobbyManager>();
    }

    //Listar Lobbies
    public void OpenListLobbies()
    {
        RefreshList(); 
        listLobbyScreen.SetActive(true);
        
    }

    public void RefreshList()
    {
        if (listLobbies.Count > 0){
            Debug.Log("Refressing...");
            foreach (LobbyItem listItem in listLobbies)
            {
                Destroy(listItem.gameObject);
            }
            listLobbies.Clear();
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
        string lobbyName = inputLobbyName.text;
        int lobbyPlayerCap = int.Parse(inputLobbPlayerCap.text);
        lobbyManager.CreateLobby(lobbyName, lobbyPlayerCap);
    }

    //Abrir Lobby
    public void OpenLobby(string nameLobby, int numPlayers, int maxPlayers)
    {
        textNameLobbyOpen.text = nameLobby;
        textLobbyOpenPlayers.text = $"{numPlayers}/{maxPlayers}";
        openLobbyPopUp.SetActive(true);
    }
}
