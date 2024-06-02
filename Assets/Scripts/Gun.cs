using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class Gun : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject bulletPoint;

    [SerializeField]
    private GameObject aimSprite;

    [SerializeField]
    private GameObject accuracySprite;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private AudioSource shotSound;

    [SerializeField]
    GameObject playerObject;

    [Header("References - TEMPORARY")]
    [SerializeField]
    TextMeshProUGUI text_ammo;

    [SerializeField]
    TextMeshProUGUI text_reserve;

    [Header("Gun stats")]
    [SerializeField]
    GunData[] guns;

    [SerializeField, ReadOnly]
    int gunID;

    [SerializeField]
    float aimLeftRightTweak;

    // Aim stuff
    Vector3 aimPoint;
    Vector3 deviation;
    Ray ray;
    Ray center;
    RaycastHit hit;
    float aimDistance;
    float spread;

    // Updating gun stats
    [SerializeField, ReadOnly]
    int[] bulletsLoaded;

    [SerializeField, ReadOnly]
    int[] currentAmmo;
    bool[] inCooldown;
    bool[] isReloading;
    bool oneSound;
    public TeamData teamData;
    public char teamTag;
    public NetworkVariable<char> networkTeamTag = new NetworkVariable<char>();

    void Start()
    {
        bulletsLoaded = new int[guns.Length];
        currentAmmo = new int[guns.Length];
        inCooldown = new bool[guns.Length];
        isReloading = new bool[guns.Length];

        for (int i = 0; i < guns.Length; i++)
        {
            AcquireWeapon(i);
        }
        UpdateAmmo();
    }

    public void AcquireWeapon(int slot, bool swap = false)
    {
        if (!swap)
        {
            bulletsLoaded[slot] = guns[slot].clip;
            currentAmmo[slot] = guns[slot].maxAmmo;
        }
        spread = guns[slot].minSpread;
        oneSound = guns[slot].firingSounds.Length == 1;
        if (oneSound)
            shotSound.clip = guns[slot].firingSounds[0];
    }

    private void UpdateAmmo(bool updateReserve = true)
    {
        text_ammo.text = bulletsLoaded[gunID].ToString();
        if (updateReserve)
            text_reserve.text = currentAmmo[gunID].ToString();
    }

    [ServerRpc]
    private void UpdateAmmo_ServerRpc(bool updateReserve = true)
    {
        text_ammo.text = bulletsLoaded[gunID].ToString();
        if (updateReserve)
            text_reserve.text = currentAmmo[gunID].ToString();
        UpdateAmmo_ClientRpc(updateReserve);
    }

    [ClientRpc]
    private void UpdateAmmo_ClientRpc(bool updateReserve = true)
    {
        text_ammo.text = bulletsLoaded[gunID].ToString();
        if (updateReserve)
            text_reserve.text = currentAmmo[gunID].ToString();
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(guns[gunID].shotCooldown);
        inCooldown[gunID] = false;
    }

    public void Shoot()
    {
        if (isReloading[gunID])
            return;

        if (!inCooldown[gunID] && bulletsLoaded[gunID] > 0)
        {
            // Comece cooldown, se aplicável
            bulletsLoaded[gunID] -= 1;
            if (guns[gunID].shotCooldown > 0)
            {
                inCooldown[gunID] = true;
                StartCoroutine("Cooldown");
            }
            // Crie a bala
            Bullet bullet = Instantiate(
                guns[gunID].bulletPrefab,
                bulletPoint.transform.position,
                bulletPoint.transform.rotation
            );
            // Defina o spread com números aleatórios e acelere a bala com seu rigidbody. Destrua a bala após 2 segundos.
            deviation.x = Random.Range(-spread, spread) / 10;
            deviation.y = Random.Range(-spread, spread) / 10;
            bullet.rb.AddForce(
                (transform.forward + transform.right * deviation.x + transform.up * deviation.y)
                    * guns[gunID].bulletSpeed
            );
            bullet.owner = playerObject;
            Destroy(bullet.gameObject, 5);
            spread = Mathf.Min(spread + guns[gunID].spreadIncrease, guns[gunID].maxSpread);
            // Atualize nosso número de balas na UI e, se aplicável, toque o barulho de tiro com pitch aleatório
            UpdateAmmo(false);
            shotSound.pitch = Random.Range(0.9f, 1.1f);
            if (oneSound)
                shotSound.Play();
            else if (guns[gunID].firingSounds.Length > 0)
            {
                shotSound.clip = guns[gunID].firingSounds[
                    Random.Range(0, guns[gunID].firingSounds.Length)
                ];
                shotSound.Play();
            }
        }
    }

    [ServerRpc]
    public void Shoot_ServerRpc()
    {
        if (!IsServer)
            return;
        if (isReloading[gunID])
            return;

        if (!inCooldown[gunID] && bulletsLoaded[gunID] > 0)
        {
            // Comece cooldown, se aplicável
            bulletsLoaded[gunID] -= 1;
            if (guns[gunID].shotCooldown > 0)
            {
                inCooldown[gunID] = true;
                StartCoroutine("Cooldown");
            }
            // Crie a bala
            Bullet bullet = Instantiate(
                guns[gunID].bulletPrefab,
                bulletPoint.transform.position,
                bulletPoint.transform.rotation
            );
            NetworkObject netBullet = bullet.GetComponent<NetworkObject>();
            netBullet.Spawn();
            // Defina o spread com números aleatórios e acelere a bala com seu rigidbody. Destrua a bala após 2 segundos.
            deviation.x = Random.Range(-spread, spread) / 10;
            deviation.y = Random.Range(-spread, spread) / 10;
            bullet.rb.AddForce(
                (transform.forward + transform.right * deviation.x + transform.up * deviation.y)
                    * guns[gunID].bulletSpeed
            );
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.isKinematic = false;

            bullet.owner = playerObject;

            Destroy(bullet.gameObject, 5);

            spread = Mathf.Min(spread + guns[gunID].spreadIncrease, guns[gunID].maxSpread);
            // Atualize nosso número de balas na UI e, se aplicável, toque o barulho de tiro com pitch aleatório
            if (IsOwner)
            {
                UpdateAmmo_ServerRpc(false);
                if (teamData == null)
                {
                    teamData = playerObject.GetComponent<PlayerOnline>().GetTeam();
                }
                teamTag = teamData.teamTag;
                networkTeamTag.Value = teamTag;
            }
            else
            {
                teamTag = networkTeamTag.Value;
            }
            bullet.teamTag = teamTag;
            shotSound.pitch = Random.Range(0.9f, 1.1f);
            if (oneSound)
                shotSound.Play();
            else if (guns[gunID].firingSounds.Length > 0)
            {
                shotSound.clip = guns[gunID].firingSounds[
                    Random.Range(0, guns[gunID].firingSounds.Length)
                ];
                shotSound.Play();
            }
        }
    }

    public void Reload()
    {
        if (bulletsLoaded[gunID] == guns[gunID].clip)
            return;
        if (currentAmmo[gunID] <= 0)
        {
            currentAmmo[gunID] = 0;
            return;
        }

        int ammoAdded = bulletsLoaded[gunID];
        bulletsLoaded[gunID] =
            currentAmmo[gunID] - guns[gunID].clip >= 0
                ? guns[gunID].clip
                : bulletsLoaded[gunID] + currentAmmo[gunID];
        ammoAdded = bulletsLoaded[gunID] - ammoAdded;
        currentAmmo[gunID] -= ammoAdded;
        UpdateAmmo();
    }

    [ServerRpc]
    public void Reload_ServerRpc()
    {
        if (bulletsLoaded[gunID] == guns[gunID].clip)
            return;
        if (currentAmmo[gunID] <= 0)
        {
            currentAmmo[gunID] = 0;
            return;
        }

        int ammoAdded = bulletsLoaded[gunID];
        bulletsLoaded[gunID] =
            currentAmmo[gunID] - guns[gunID].clip >= 0
                ? guns[gunID].clip
                : bulletsLoaded[gunID] + currentAmmo[gunID];
        ammoAdded = bulletsLoaded[gunID] - ammoAdded;
        currentAmmo[gunID] -= ammoAdded;
        if (IsOwner)
        {
            UpdateAmmo_ServerRpc();
        }
    }

    void Update()
    {
        center = new Ray(cam.transform.position, cam.transform.forward);
        aimPoint = center.GetPoint(100);
        transform.parent.LookAt(aimPoint + Vector3.up * 3, Vector3.up);
        aimSprite.transform.position = center.GetPoint(8);

        accuracySprite.transform.localScale = Vector3.one * (spread + 0.25f);
    }

    void FixedUpdate()
    {
        if (spread > guns[gunID].minSpread)
            spread = Mathf.Max(spread - guns[gunID].spreadRecovery / 20, guns[gunID].minSpread);
    }

    public bool IsAuto()
    {
        if (guns[gunID])
            return guns[gunID].autoFire;
        else
        {
            Debug.LogError("Gun has no stats object attached");
            return false;
        }
    }

    public void SwapGun()
    {
        gunID++;
        if (gunID == guns.Length)
            gunID = 0;
        AcquireWeapon(gunID, true);
        UpdateAmmo();
    }

    [ServerRpc]
    public void SwapGun_ServerRpc()
    {
        gunID++;
        if (gunID == guns.Length)
            gunID = 0;
        AcquireWeapon(gunID, true);
        UpdateAmmo();
    }
}
