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
    [SerializeField] GameObject flagCarryObject;
    [SerializeField] GameObject flagCarryEffects;

    [SerializeField]
    GameObject model,
        playerCanvas;
    [Header("Player Model")]
    [SerializeField] private GameObject malePlayer;
    [SerializeField] private GameObject femalePlayer;
    [SerializeField] private List<SkinnedMeshRenderer> malePlayerParts;
    [SerializeField] private List<SkinnedMeshRenderer> femalePlayerParts;
    [SerializeField] private GameObject maleGunHolder;
    [SerializeField] private GameObject femaleGunHolder;
    [Header("Controllers")]
    [SerializeField, ReadOnly] private MovementOnline movementOnline;
    [SerializeField, ReadOnly] private GunControllerOnline gunController;
    public GunSwapperOnline swapperOnline;
    [Header("Animator")]
    [SerializeField] private RuntimeAnimatorController maleAnimController;
    [SerializeField] private RuntimeAnimatorController femaleAnimController;
    [SerializeField] private Avatar maleAvatar;
    [SerializeField] private Avatar femaleAvatar;
    private int inputShoot;
    public Animator animator;
    [SerializeField] private Camera _camera;
    Transform spawnPoint;
    [SerializeField, ReadOnly] bool _hasFlag;
    public GameObject gunHolder;

    [Header("PlayerStatus")]
    [SerializeField] private float health = 3f;
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private Slider sliderHealth;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool isFocused;
    [ReadOnly] public bool isGrounded;
    [ReadOnly] public bool scopeGun;
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public string gender;

    [Header("Player UI")]
    public string playerName;
    [SerializeField, ReadOnly] private bool overlaySet = false;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject bgPlayerName;
    [SerializeField] private GameObject pauseScreen;
    public bool isPaused = false;
    [Header("Player Spawn Points")]
    [SerializeField] private bool firstSpawn = false;
    public List<Transform> playerSpawns;
    public Vector3 spawnPosition = new Vector3(14f, 1.5f, 20.5f);

    private void Awake()
    {
        gunController = GetComponent<GunControllerOnline>();
        movementOnline = gameObject.GetComponentInParent<MovementOnline>();
        health = maxHealth;
        health = maxHealth;
        sliderHealth.maxValue = maxHealth;
        sliderHealth.value = health;
        bgPlayerName.SetActive(true);
        playerSpawns = new List<Transform>();

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gender = LobbyManager.Instance.GetGender();
            playerName = LobbyManager.Instance.GetPlayerName();
            SetCharacter_ServerRpc(gender, playerName);
            playerCanvas.SetActive(true);
            bgPlayerName.SetActive(false);
            _camera.gameObject.GetComponent<AudioListener>().enabled = true;
            if (IsHost)
            {
                _camera.enabled = true;
            }
        }
        if (!IsHost)
            RequestData_ServerRpc();


        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (IsOwner && playerSpawns.Count > 0)
        {
            SetSpawnPosition();
        }
    }

    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName))
        {
            SetOverlay();
            overlaySet = true;
        }
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                PauseGame(isPaused);
            }
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

    public void SetOverlay()
    {
        playerNameText.text = playerName;
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
        Damage_ServerRpc(damage);
    }

    [ServerRpc]
    private void Damage_ServerRpc(float damage)
    {
        if (isDead.Value) return;
        health -= damage;
        Damage_ClientRpc(health);

        sliderHealth.value = health;
        if (health <= 0)
        {
            if (IsOwner)
            {
                isDead.Value = true;
                Die_ServerRpc();
            }
        }

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
                isDead.Value = true;
                Die_ServerRpc();
            }
        }
    }

    [ServerRpc]
    public void Die_ServerRpc()
    {
        model.SetActive(false);
        bgPlayerName.SetActive(false);
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
                SetSpawnPosition();
                movementOnline.enabled = true;
                gunController.RefillWeapons();
            }
            model.SetActive(true);
            if (!IsOwner)
            {
                bgPlayerName.SetActive(true);
            }
            AcetivePlayer_ClientRpc();
        }
    }

    [ClientRpc]
    private void AcetivePlayer_ClientRpc()
    {
        health = maxHealth;
        if (IsOwner)
        {
            isDead.Value = false;
            // transform.position = MultiplayerManager.Instance.GetNextSpawnPosition();
            // transform.rotation = spawnPoint.rotation;
            SetSpawnPosition();
            sliderHealth.value = health;
            movementOnline.enabled = true;
            gunController.RefillWeapons();
            if (!IsOwner)
            {
                bgPlayerName.SetActive(true);
            }
        }
        model.SetActive(true);
    }

    private void ReturnBanner()
    {
        if (!hasFlag) return;
        hasFlag = false;
        MultiplayerManager.Instance.ActivateFlag();
        if (flagCarryObject != null)
            flagCarryObject.SetActive(false);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(false);
    }

    private void GetSpawn(TeamData teamData)
    {
        // int index;
        // if (teamData.teamId == 1)
        // {
        // countSpawnPoints = MultiplayerManager.Instance.spawnPointsBlue.Length;
        foreach (Transform spawnPoint in teamData.teamId == 1 ? MultiplayerManager.Instance.spawnPointsBlue : MultiplayerManager.Instance.spawnPointsRed)
        {
            playerSpawns.Add(spawnPoint);
        }

        // index = Random.Range(0, countSpawnPoints);

        // movementOnline.transform.position = MultiplayerManager.Instance.spawnPointsBlue[index].position;
        // movementOnline.transform.rotation = MultiplayerManager.Instance.spawnPointsBlue[index].rotation;

        // }
        // countSpawnPoints = MultiplayerManager.Instance.spawnPointsRed.Length;
        // index = Random.Range(0, countSpawnPoints);

        // movementOnline.transform.position = MultiplayerManager.Instance.spawnPointsRed[index].position;
        // movementOnline.transform.rotation = MultiplayerManager.Instance.spawnPointsRed[index].rotation;
    }
    private void SetSpawnPosition()
    {
        int index = Random.Range(0, playerSpawns.Count);
        Debug.Log(index + " -- " + playerSpawns.Count + " --- " + teamData != null);
        transform.position = playerSpawns[index].position;
        transform.rotation = playerSpawns[index].rotation;
    }
    public void PauseGame(bool status)
    {
        movementOnline.enabled = !status;
        // playerHUD.SetActive(!status);
        pauseScreen.SetActive(status);
        if (status)
        {
            // UiManager.Instance.PauseGame();
        }
        Cursor.visible = status;
        Cursor.lockState = status ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void LeaveLobby()
    {
        if (IsOwner)
        {
            LobbyManager.Instance.LeaveLobby();
        }
    }
    public void ChangeWeapon(int id)
    {
        movementOnline.ChangeWeapon(id);
    }
    public void ReloadWeapon(int idWeapon, bool reload)
    {
        movementOnline.ReloadWeapon(idWeapon, reload);
    }
    public void AddAmmo()
    {
        gunController.AddAmmo();
    }
    [ServerRpc]
    private void SetCharacter_ServerRpc(string gender, string playerName)
    {

        teamData = MultiplayerManager.Instance.GetTeamData(this);
        this.playerName = playerName;
        if (gender == "male")
        {
            animator.avatar = maleAvatar;
            animator.runtimeAnimatorController = maleAnimController;
            malePlayer.SetActive(true);
            femalePlayer.SetActive(false);
            gunHolder = maleGunHolder;
            foreach (SkinnedMeshRenderer part in malePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }
        else
        {
            animator.avatar = femaleAvatar;
            animator.runtimeAnimatorController = femaleAnimController;
            femalePlayer.SetActive(true);
            malePlayer.SetActive(false);
            gunHolder = femaleGunHolder;
            foreach (SkinnedMeshRenderer part in femalePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }

        movementOnline.LoadAnimator(this.animator);
        if (IsOwner)
        {
            Debug.Log("Team Data: " + teamData.teamName);
            GetSpawn(teamData);
            Debug.Log("Player Spawns: " + playerSpawns.Count);
            SetSpawnPosition();
        }
        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
        playerNameText.color = teamData.teamColor;
        swapperOnline = gunHolder.GetComponent<GunSwapperOnline>();
        SetCharacter_ClientRpc(gender, teamData.teamTag, playerName);
        gunController.currentGun.SetSwapper(swapperOnline);
    }
    [ClientRpc]
    private void SetCharacter_ClientRpc(string gender, char teamTag, string playerName)
    {
        this.teamData = MultiplayerManager.Instance.GetTeamData(teamTag, gender);
        this.playerName = playerName;
        this.gender = gender;

        if (gender == "male")
        {
            animator.avatar = maleAvatar;
            animator.runtimeAnimatorController = maleAnimController;
            malePlayer.SetActive(true);
            model = malePlayer;
            femalePlayer.SetActive(false);
            gunHolder = maleGunHolder;
            foreach (SkinnedMeshRenderer part in malePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }
        else
        {
            animator.avatar = femaleAvatar;
            animator.runtimeAnimatorController = femaleAnimController;
            femalePlayer.SetActive(true);
            model = femalePlayer;
            malePlayer.SetActive(false);
            gunHolder = femaleGunHolder;
            foreach (SkinnedMeshRenderer part in femalePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }
        movementOnline.LoadAnimator(this.animator);

        if (IsOwner && !IsHost)
        {
            Debug.Log("Team Data: " + teamData.teamName);
            GetSpawn(teamData);
            Debug.Log("Player Spawns: " + playerSpawns.Count);
            ChangeWeapon(1);
            SetSpawnPosition();
            _camera.enabled = true;
        }
        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
        playerNameText.color = teamData.teamColor;
        swapperOnline = gunHolder.GetComponent<GunSwapperOnline>();
        gunController.currentGun.SetSwapper(swapperOnline);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RequestData_ServerRpc()
    {
        SendData_ClientRpc(gender, teamData.teamTag, playerName);
    }
    [ClientRpc]
    private void SendData_ClientRpc(string gender, char teamTag, string playerName)
    {
        // Atualize o personagem com os dados recebidos
        this.playerName = playerName;
        this.teamData = MultiplayerManager.Instance.GetTeamData(teamTag, gender);
        this.gender = gender;

        if (gender == "male")
        {
            animator.avatar = maleAvatar;
            animator.runtimeAnimatorController = maleAnimController;
            malePlayer.SetActive(true);
            model = malePlayer;
            femalePlayer.SetActive(false);
            gunHolder = maleGunHolder;
            foreach (SkinnedMeshRenderer part in malePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }
        else
        {
            animator.avatar = femaleAvatar;
            animator.runtimeAnimatorController = femaleAnimController;
            femalePlayer.SetActive(true);
            model = femalePlayer;
            malePlayer.SetActive(false);
            gunHolder = femaleGunHolder;
            foreach (SkinnedMeshRenderer part in femalePlayerParts)
            {
                part.material = teamData.teamEquipMaterial;
            }
        }

        flagCarryObject.GetComponent<MeshRenderer>().material.color = teamData.teamColor;
        playerNameText.color = teamData.teamColor;
        swapperOnline = gunHolder.GetComponent<GunSwapperOnline>();
        gunController.currentGun.SetSwapper(swapperOnline);

    }

    public void ShootRecoil()
    {
        inputShoot = Animator.StringToHash("shoot");
        animator.SetTrigger(inputShoot);
    }
}
