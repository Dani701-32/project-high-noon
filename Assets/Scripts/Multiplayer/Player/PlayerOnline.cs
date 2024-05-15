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

    [SerializeField, ReadOnly]
    bool _hasFlag;

    public override void OnNetworkSpawn()
    {
        teamData = MultiplayerManager.Instance.GetTeamData(this);

        base.OnNetworkSpawn();
    }


    private void Start()
    {
        if (teamData != null)
        {
            model.GetComponent<MeshRenderer>().material = teamData.teamEquipMaterial;
        }
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
    public void SpawnPoint(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        tPSMovement.SetSpawn(spawnPoint);
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
