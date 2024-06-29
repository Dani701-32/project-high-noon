using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

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
    [SerializeField] private Camera _camera;
    Transform spawnPoint;
    [SerializeField, ReadOnly] bool _hasFlag;

    [Header("PlayerStatus")]
    [SerializeField, ReadOnly] private float health = 10f;
    [SerializeField, ReadOnly] private float maxHealth = 10f;
    [SerializeField] private Slider sliderHealth;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool isFocused;
    [ReadOnly] public bool isGrounded;
    public override void OnNetworkSpawn()
    {
        teamData = MultiplayerManager.Instance.GetTeamData(this);
        sliderHealth.maxValue = maxHealth;
        sliderHealth.value = health;
        model.GetComponent<SkinnedMeshRenderer>().material.color = teamData.teamColor; 
        if (!IsOwner)
            return;
        playerCanvas.SetActive(true);
        _camera.enabled = true;
        
        _camera.gameObject.GetComponent<AudioListener>().enabled = true;
        health = 100f;
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

        Damage_ServerRpc(damage);
    }

    [ServerRpc]
    private void Damage_ServerRpc(float damage)
    {
        health -= damage;
        sliderHealth.value = health;
        Damage_ClientRpc(health);
    }
    [ClientRpc]
    private void Damage_ClientRpc(float health)
    {
        this.health = health;
        sliderHealth.value = health;
    }

    [ServerRpc]
    public void Die_ServerRpc()
    {
        Debug.Log("Morreu");
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
            AcetivePlayer_ClientRpc();
        }
    }

    [ClientRpc]
    private void AcetivePlayer_ClientRpc()
    {
        model.SetActive(true);
    }
}
