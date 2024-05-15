using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    static MultiplayerManager _instance;
    [SerializeField] private Transform[] spawnPointsRed, spawnPointsBlue;
    [SerializeField] private TeamData[] teamDatas;
    [SerializeField] private List<PlayerOnline> playersRed, playersBlue;
    public static MultiplayerManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Multiplayer manager é nulo");
            return _instance;
        }
    }
    private void Awake()
    {
        _instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public TeamData GetTeamData(PlayerOnline newPlayer)
    {
        if (playersRed.Count == playersBlue.Count)
        {
            newPlayer.SpawnPoint(spawnPointsBlue[0]);
            playersBlue.Add(newPlayer);
            return teamDatas[0];
        }
        newPlayer.SpawnPoint(spawnPointsRed[0]);
        playersRed.Add(newPlayer);
        return teamDatas[1];
    }

}
