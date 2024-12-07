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
    private int countSpawnPoints = 0;



    public override void OnNetworkSpawn()
    {
        gunController = GetComponent<GunControllerOnline>();
        health = maxHealth;
        sliderHealth.maxValue = maxHealth;
        sliderHealth.value = health;
        movementOnline = gameObject.GetComponentInParent<MovementOnline>();
        bgPlayerName.SetActive(true);

        if (IsOwner)
        {
            gender = LobbyManager.Instance.GetGender();
            playerName = LobbyManager.Instance.GetPlayerName();
            SetCharacter_ServerRpc(gender, playerName);
            playerCanvas.SetActive(true);
            bgPlayerName.SetActive(false);
            _camera.gameObject.GetComponent<AudioListener>().enabled = true;
            if (IsHost)
                _camera.enabled = true;
            return;
        }
        if (!IsHost)
            RequestData_ServerRpc();


        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (IsOwner && teamData != null)
        {
            GetSpawn(teamData);
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

    // public void SpawnPoint(Transform sp)
    // {
    //     spawnPoint = sp;
    //     transform.position = sp.position;
    //     movementOnline.SetSpawn(sp);
    // }

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
                GetSpawn(teamData);
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
            GetSpawn(teamData);
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
        int index;
        if (teamData.teamId == 1)
        {
            countSpawnPoints = MultiplayerManager.Instance.spawnPointsBlue.Length;
            index = Random.Range(0, countSpawnPoints);

            movementOnline.transform.position = MultiplayerManager.Instance.spawnPointsBlue[index].position;
            movementOnline.transform.rotation = MultiplayerManager.Instance.spawnPointsBlue[index].rotation;
            return;
        }
        countSpawnPoints = MultiplayerManager.Instance.spawnPointsBlue.Length;
        index = Random.Range(0, countSpawnPoints);

        movementOnline.transform.position = MultiplayerManager.Instance.spawnPointsRed[index].position;
        movementOnline.transform.rotation = MultiplayerManager.Instance.spawnPointsRed[index].rotation;
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
    public void AddAmmo(int ammo)
    {
        gunController.AddAmmo(ammo);
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

        if (IsOwner)
        {
            Debug.Log("Teste");
            GetSpawn(teamData);
            ChangeWeapon(1);
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

    public void ShootRecoil(){
        inputShoot = Animator.StringToHash("shoot");
        animator.SetTrigger(inputShoot);
    }
}
