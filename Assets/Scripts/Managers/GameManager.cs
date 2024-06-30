using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    [Header("Controles da partida")]
    [SerializeField]
    private int currentPointRed = 0,
                currentPointBlue = 0;
    public int maxPoints = 3;

    [SerializeField]
    private FlagSpot flagSpot;
    private bool matchOver = false;
    
    [Header("Map Section")]
    public Transform spawnPoint;
    

    public bool MatchOver
    {
        get => matchOver;
        private set { matchOver = value; }
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("Game manager é nulo");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        MatchOver = false;
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
                UiManager.Instance.UpdatePointsUI(team.name, currentPointBlue);
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

    public void EndGame()
    {
        UiManager.Instance.EndMatch();
    }

    public void GameOver()
    {
        MatchOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine("ReturnMenu");
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
        Cleanup(); 
        SceneManager.LoadScene("Menu");
    }
    void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
}
