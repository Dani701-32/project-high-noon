using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerOnline : NetworkBehaviour
{
    [Header("Team data")]
    [SerializeField]
    private TeamData teamData;

    [Header("Toggleable objects")]
    [SerializeField]
    GameObject flagCarryObject;

    [SerializeField]
    GameObject flagCarryEffects;

    [SerializeField]
    GameObject model,
        playerCanvas;

    [SerializeField]
    private MovementOnline movementOnline;

    [SerializeField]
    private Camera _camera;
    Transform spawnPoint;

    [SerializeField, ReadOnly]
    bool _hasFlag;

    [Header("PlayerStatus")]
    [SerializeField, ReadOnly]
    private float health = 100f;

    [SerializeField, ReadOnly]
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>();
    public NetworkVariable<bool> canMove = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        teamData = MultiplayerManager.Instance.GetTeamData(this);
        transform.position = spawnPoint.position;
        if (!IsOwner)
            return;
        playerCanvas.SetActive(true);
        _camera.enabled = true;
        _camera.gameObject.GetComponent<AudioListener>().enabled = true;
        canMove.Value = true;
        health = 100f;
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (IsOwner)
        {
            playerCanvas.SetActive(true);
            
        }
        if(IsServer){
            canMove.Value = true;
        }
        transform.position = MultiplayerManager.Instance.defaultPos.position;
        if (teamData != null)
        {
            model.GetComponent<MeshRenderer>().material = teamData.teamEquipMaterial;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            networkHealth.Value = health;
        }
        else
        {
            health = networkHealth.Value;
            movementOnline.canMove = canMove.Value;
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

    public void SpawnPoint(Transform sp)
    {
        spawnPoint = sp;
        transform.position = sp.position;
        movementOnline.SetSpawn(sp);
    }

    private void FlagUpdate()
    {
        if (flagCarryObject != null)
            flagCarryObject.SetActive(!hasFlag);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(!hasFlag);
        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
    }

    [ServerRpc]
    public void Damage_ServerRpc()
    {
        Debug.Log("Dano");
    }

    [ServerRpc]
    public void Die_ServerRpc()
    {
        Debug.Log("Morreu");
        canMove.Value = false;
        model.SetActive(false);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        Die_ClientRpc();
        StartCoroutine("delaySpawn");
    }

    [ClientRpc]
    private void Die_ClientRpc()
    {
        Debug.Log("Morreu");
        model.SetActive(false);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        StartCoroutine("delaySpawn");
    }

    public IEnumerator delaySpawn()
    {
        yield return new WaitForSeconds(3);
        if (this != null)
        {
            model.SetActive(true);
            if (NetworkManager.Singleton.IsServer)
            {
                canMove.Value = true;
            }
            AcetivePlayer_ClientRpc();
        }
    }

    [ClientRpc]
    private void AcetivePlayer_ClientRpc()
    {
        model.SetActive(true);
    }
}
