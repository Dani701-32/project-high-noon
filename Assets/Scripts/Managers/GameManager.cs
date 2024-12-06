using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    [Header("Controles da partida [CTF]")]
    [SerializeField]
    private int currentPointRed = 0,
                currentPointBlue = 0;
    public int maxPoints = 3;

    [SerializeField]
    private FlagSpot flagSpot;
    private bool matchOver = false;

    [Header("Controles da partida [Survival]")]
    [SerializeField] TextMeshProUGUI survivalTimer;
    [SerializeField] GameObject pauseScreen;
    public Slider HPSlider;
    [SerializeField] bool timerOn;
    public int goldSeconds;
    public Sprite goldMedal;
    public int silverSeconds;
    public Sprite silverMedal;
    public int bronzeSeconds;
    public Sprite bronzeMedal;
    [SerializeField] GameObject gameEndScore;
    [SerializeField] TextMeshProUGUI gameEndTime;
    [SerializeField] Image gameEndMedal;
    float timer;

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
        timerOn = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine("ReturnMenu");
    }

    public void SurvivalOver()
    {
        int finalTime = Mathf.FloorToInt(timer);
        survivalTimer.transform.parent.gameObject.SetActive(false);

        if (finalTime >= goldSeconds)
        {
            gameEndMedal.sprite = goldMedal;
        } else if (finalTime >= silverSeconds)
        {
            gameEndMedal.sprite = silverMedal;
        }else if (finalTime >= bronzeSeconds)
        {
            gameEndMedal.sprite = bronzeMedal;
        }
        finalTime /= 60;
        int seconds = Mathf.FloorToInt(timer - finalTime * 60);
        gameEndTime.text = $"{finalTime:00}:{seconds:00}";
        gameEndScore.SetActive(true);
        timer = 0;
        EventManager events = EventManager.Instance;
        events.ToggleGreaterLock(true);
        events.ToggleCamera(false);
        events.ToggleMovement(false);
        events.ForceReleaseMouse();
        Time.timeScale = 0;
    }

    public string GetStatus()
    {
        if (timerOn)
            return "Survival";
        if (currentPointRed == maxPoints || currentPointRed > currentPointBlue)
            return "Red Team";
        if (currentPointBlue == maxPoints || currentPointRed < currentPointBlue)
            return "Blue Team";
        return "Tie";
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

    public void SetSurvivalTimer(bool set) { timerOn = set; }
    void Update()
    {
        if (timerOn)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);

            survivalTimer.text = $"{minutes:00}:{seconds:00}";

            if (Input.GetKeyDown(KeyCode.Escape) && pauseScreen && !pauseScreen.activeInHierarchy)
                PauseGame();
        }

        if (timer > 0 && !timerOn)
        {
            SurvivalOver();
        }
    }

    void PauseGame()
    {
        EventManager events = EventManager.Instance;
        if (events)
        {
            events.ToggleCamera(false);
            events.ToggleMovement(false);
            events.ToggleGreaterLock(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        AudioSource aud = pauseScreen.GetComponent<AudioSource>();
        if(aud)
            aud.Play();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        EventManager events = EventManager.Instance;
        if (events)
        {
            events.ToggleCamera(true);
            events.ToggleMovement(true);
            events.ToggleGreaterLock(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseScreen.SetActive(false);
    }
    
    public void ResumeTime() { Time.timeScale = 1; }
}
