using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerOnline : NetworkBehaviour
{
    [Header("Team data")]
    [SerializeField] private TeamData teamData;

    [Header("Toggleable objects")]
    [SerializeField] GameObject flagCarryObject;

    [SerializeField] GameObject flagCarryEffects;
    [SerializeField] GameObject model;

    [SerializeField] private TPSMovement tPSMovement;
    Transform spawnPoint; 

    [SerializeField, ReadOnly]
    bool _hasFlag;

    public override void OnNetworkSpawn()
    {
        
        teamData = MultiplayerManager.Instance.GetTeamData(this);
        transform.position = spawnPoint.position;
        base.OnNetworkSpawn();
    }


    private void Start()
    {
        
        transform.position = MultiplayerManager.Instance.defaultPos.position;
        if (teamData != null)
        {
            model.GetComponent<MeshRenderer>().material = teamData.teamEquipMaterial;
        }
    }
    private void Update() {
    }

    public bool hasFlag
    {
        get => _hasFlag;
        set
        {
            if (_hasFlag != value)
            {
                FlagUpdate();
            }
            _hasFlag = value;
        }
    }

    public TeamData GetTeam()
    {
        return teamData;
    }
    public void SpawnPoint(Transform sp)
    {
        this.spawnPoint = sp; 
        transform.position = sp.position;
        tPSMovement.SetSpawn(sp);
    }

    private void FlagUpdate()
    {
        if (flagCarryObject != null)
            flagCarryObject.SetActive(!hasFlag);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(!hasFlag);
        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
    }
}
