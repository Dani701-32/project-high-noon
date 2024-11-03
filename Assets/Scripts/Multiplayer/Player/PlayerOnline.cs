using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;

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
        playerCanvas, hat;

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
    [ReadOnly] public bool scopeGun;

    [Header("Player UI")]
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField, ReadOnly] private string playName;
    [SerializeField, ReadOnly] private bool overlaySet = false;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject bgPlayerName;

    private int countSpawnPoints = 0;


    public override void OnNetworkSpawn()
    {
        gunController = GetComponent<GunControllerOnline>();
        movementOnline = GetComponent<MovementOnline>();
        teamData = MultiplayerManager.Instance.GetTeamData(this);
        health = maxHealth;
        sliderHealth.maxValue = maxHealth;
        sliderHealth.value = health;
        bgPlayerName.SetActive(true);
        hat.GetComponent<SkinnedMeshRenderer>().material.color = teamData.teamColor;
        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
        if (!IsOwner)
            return;
        playerCanvas.SetActive(true);
        _camera.enabled = true;
        bgPlayerName.SetActive(false);
        _camera.gameObject.GetComponent<AudioListener>().enabled = true;
        playerName.Value = LobbyManager.Instance.GetPlayerName();


        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (IsOwner)
        {
            GetSpawn(teamData);
        }
    }

    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverlay();
            overlaySet = true;
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

    // public void SpawnPoint(Transform sp)
    // {
    //     spawnPoint = sp;
    //     transform.position = sp.position;
    //     movementOnline.SetSpawn(sp);
    // }

    public void SetOverlay()
    {
        playerNameText.text = playerName.Value;
        playName = playerName.Value.ToString();
    }

    private void FlagUpdate()
    {
        if (flagCarryObject != null)
            flagCarryObject.SetActive(!hasFlag);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(!hasFlag);

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
        ReturnBanner();
        StartCoroutine("delaySpawn");
    }

    [ClientRpc]
    private void Die_ClientRpc()
    {

        Debug.Log("Morreu");
        model.SetActive(false);
        ReturnBanner();
        if (IsOwner)
        {
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
                GetSpawn(teamData);
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
            // transform.position = MultiplayerManager.Instance.GetNextSpawnPosition();
            // transform.rotation = spawnPoint.rotation;
            GetSpawn(teamData);
            sliderHealth.value = health;
            movementOnline.enabled = true;
            gunController.RefillWeapons();
        }
        model.SetActive(true);
    }

    private void ReturnBanner()
    {
        hasFlag = false;
        MultiplayerManager.Instance.ActivateFlag();
        if (flagCarryObject != null)
            flagCarryObject.SetActive(false);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(false);
    }

    private void GetSpawn(TeamData teamData)
    {
        int index;
        if (teamData.teamId == 1)
        {
            countSpawnPoints = MultiplayerManager.Instance.spawnPointsBlue.Length;
            index = Random.Range(0, countSpawnPoints);
            
            transform.position = MultiplayerManager.Instance.spawnPointsBlue[index].position;
            transform.rotation = MultiplayerManager.Instance.spawnPointsBlue[index].rotation;
            return;
        }
        countSpawnPoints = MultiplayerManager.Instance.spawnPointsBlue.Length;
        index = Random.Range(0, countSpawnPoints);

        transform.position = MultiplayerManager.Instance.spawnPointsRed[index].position;
        transform.rotation = MultiplayerManager.Instance.spawnPointsRed[index].rotation;
    }
}
