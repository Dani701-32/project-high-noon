using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

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

    [SerializeField, ReadOnly] private MovementOnline movementOnline;
    [SerializeField, ReadOnly] private GunControllerOnline gunController;
    [SerializeField] private Camera _camera;
    Transform spawnPoint;
    [SerializeField, ReadOnly] bool _hasFlag;

    [Header("PlayerStatus")]
    [SerializeField] private float health = 3f;
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private Slider sliderHealth;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool isFocused;
    [ReadOnly] public bool isGrounded;

    [Header("Player UI")]
    [SerializeField, ReadOnly] private string playerName = ""; 
    [SerializeField] private TMP_Text playerNameText; 
    [SerializeField] private GameObject bgPlayerName;
    
    public override void OnNetworkSpawn()
    {
        gunController = GetComponent<GunControllerOnline>();
        movementOnline = GetComponent<MovementOnline>();
        teamData = MultiplayerManager.Instance.GetTeamData(this);
        health = maxHealth;
        sliderHealth.maxValue = maxHealth;
        sliderHealth.value = health;
        model.GetComponent<SkinnedMeshRenderer>().material.color = teamData.teamColor;
        if (!IsOwner)
            return;
        playerCanvas.SetActive(true);
        _camera.enabled = true;
        if(IsOwner){
            bgPlayerName.SetActive(false);
            playerName = LobbyManager.Instance.GetPlayerName();
            UpdatePlayerNameServerRpc(playerName);
        }

        _camera.gameObject.GetComponent<AudioListener>().enabled = true;
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (IsOwner)
        {
            playerCanvas.SetActive(true);
            
        }
    }

    private void Update()
    {
        if (IsOwner && playerName == "")
        {
            bgPlayerName.SetActive(false);
            playerName = LobbyManager.Instance.GetPlayerName();
            UpdatePlayerNameServerRpc(playerName);
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
    public void Damage(float damage)
    {
        if (!IsOwner) return;
        Debug.Log("Teste");
        Damage_ServerRpc(damage);
    }

    [ServerRpc]
    private void Damage_ServerRpc(float damage)
    {
        health -= damage;
        if (IsOwner)
        {
            sliderHealth.value = health;
            if (health <= 0)
            {
                Die_ServerRpc();
            }
        }
        Damage_ClientRpc(health);
    }
    [ClientRpc]
    private void Damage_ClientRpc(float health)
    {
        this.health = health;
        if (IsOwner)
        {
            sliderHealth.value = health;
            if (health <= 0)
            {
                Die_ServerRpc();
            }
        }
    }

    [ServerRpc]
    public void Die_ServerRpc()
    {
        Debug.Log("Morreu");
        model.SetActive(false);
        if (IsOwner)
        {
            movementOnline.enabled = false;
        }
        Die_ClientRpc();
        StartCoroutine("delaySpawn");
    }

    [ClientRpc]
    private void Die_ClientRpc()
    {

        Debug.Log("Morreu");
        model.SetActive(false);
        if(IsOwner){
            movementOnline.enabled = false;
        }
    }

    public IEnumerator delaySpawn()
    {
        yield return new WaitForSeconds(3);
        if (this != null)
        {
            health = maxHealth;
            if (IsOwner)
            {
                sliderHealth.value = health;
                transform.position = MultiplayerManager.Instance.GetNextSpawnPosition();
                transform.rotation = spawnPoint.rotation;
                movementOnline.enabled = true;
                gunController.RefillWeapons(); 
            }

            model.SetActive(true);
            AcetivePlayer_ClientRpc();
        }
    }

    [ClientRpc]
    private void AcetivePlayer_ClientRpc()
    {
        health = maxHealth;
        if (IsOwner)
        {
            transform.position = MultiplayerManager.Instance.GetNextSpawnPosition();
            transform.rotation = spawnPoint.rotation;
            sliderHealth.value = health;
            movementOnline.enabled = true;
            gunController.RefillWeapons(); 
        }
        model.SetActive(true);
    }
    [ServerRpc]
    private void UpdatePlayerNameServerRpc(string pName){
        Debug.Log("Nome do jogador recebido no cliente: " + pName);
        UpdatePlayerNameClientRpc(pName);
    }
    [ClientRpc]
    private void UpdatePlayerNameClientRpc(string pName)
    {
        playerName = pName;
        playerNameText.text = pName;
        bgPlayerName.SetActive(!IsOwner);
    }
}
