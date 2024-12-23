using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using TMPro;

public class GunOnline : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private GameObject aimSprite;
    [SerializeField] private SpriteRenderer accuracySprite;
    [SerializeField] private Camera cam;
    [Header("Sound")]
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private AudioSource reloadSound;
    [SerializeField] private GunSwapperOnline swapperOnline;
    [SerializeField] private PlayerOnline player;

    //Aim
    private Ray center;
    private RaycastHit hit;
    private float spread;
    private float focusSpread;
    private float focusCooldown;
    [SerializeField] private float distanceUni = 8;
    private Vector3 aimPoint;
    private Vector3 deviation;

    [Header("UI References")]
    [SerializeField] private TMP_Text textAmmo;
    [SerializeField] private TMP_Text textReserve;

    [Header("Gun stats")]
    [SerializeField] private GunData[] guns;
    [SerializeField, ReadOnly] private int gunId = 0;
    [SerializeField, ReadOnly] private int[] bulletsLoaded;
    [SerializeField, ReadOnly] private int[] currentAmmo;
    [SerializeField, ReadOnly] private bool[] inCoolDown;
    [SerializeField, ReadOnly] private bool[] isReloaing;
    private bool oneSound;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    private void Start()
    {
        swapperOnline = player.swapperOnline;

        bulletsLoaded = new int[guns.Length];
        currentAmmo = new int[guns.Length];
        inCoolDown = new bool[guns.Length];
        isReloaing = new bool[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            AcquireWeapon(i);
        }
        AcquireWeapon(0);
        player.ChangeWeapon(guns[gunId].animId);

        if (IsOwner)
        {
            aimSprite.SetActive(true);
            UpdateAmmo_ServerRpc();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) return;
        if (swapperOnline != null)
        {
            bulletPoint = swapperOnline.bulletPoint.transform;
            center = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(center, out hit))
            {
                aimPoint = hit.point;
            }
            else
            {
                aimPoint = center.GetPoint(100);
            }
            float dist = Vector3.Distance(bulletPoint.position, aimPoint);
            bulletPoint.LookAt(aimPoint, Vector3.up);

            aimSprite.transform.position = dist > Vector3.Distance(bulletPoint.position, center.GetPoint(distanceUni)) ? center.GetPoint(distanceUni) : aimPoint;
            aimSprite.transform.position -= Vector3.up / 15;
            accuracySprite.size = Vector2.one * Mathf.Max(0.25f, spread);
        }

    }
    private void FixedUpdate()
    {
        if (Math.Abs(spread - (guns[gunId].minSpread - focusSpread)) > 0.01f)
        {
            spread = Mathf.Max(spread - guns[gunId].spreadRecovery / 20, guns[gunId].minSpread - focusSpread);
        }
        bool focused = player.isFocused;
        focusCooldown = focused ? guns[gunId].focusShotCooldownExtra : 0;
        focusSpread = focused ? guns[gunId].focusSpreadDecrease : 0;
    }

    public void AcquireWeapon(int slot, bool swap = false)
    {
        if (!swap)
        {
            bulletsLoaded[slot] = guns[slot].clip;
            currentAmmo[slot] = guns[slot].startingAmmo;
        }
        spread = guns[slot].minSpread;
        oneSound = guns[slot].firingSounds.Length == 1;
        if (oneSound && slot == gunId)
        {
            shotSound.clip = guns[slot].firingSounds[0];
        }
        player.scopeGun = guns[slot].scopeView;
        reloadSound.clip = guns[slot].reloadSound;
    }
    [ServerRpc]
    private void UpdateAmmo_ServerRpc(bool updateReserve = true)
    {
        textAmmo.text = bulletsLoaded[gunId].ToString();
        if (updateReserve)
        {
            textReserve.text = currentAmmo[gunId].ToString();
        }
        UpdateAmmo_ClientRpc(bulletsLoaded[gunId], currentAmmo[gunId], gunId);
    }
    [ClientRpc]
    private void UpdateAmmo_ClientRpc(int ammo, int reserve, int gunId)
    {
        bulletsLoaded[gunId] = ammo;
        currentAmmo[gunId] = reserve;
        textAmmo.text = ammo.ToString();
        textReserve.text = reserve.ToString();

    }
    public bool IsAuto()
    {
        if (guns[gunId])
        {
            return guns[gunId].autoFire;
        }
        Debug.Log("Gun has no stats object attached");
        return false;
    }

    public void Shoot()
    {
        if (IsOwner)
        {
            Shoot_ServerRpc();
            UpdateAmmo_ServerRpc(false);
        }
    }
    public void Reload()
    {
        if (IsOwner)
        {
            Realod_ServerRpc();
        }
    }

    public void SwapGun()
    {
        if (IsOwner)
        {
            Debug.Log("GunOnline");
            SwapGun_ServerRpc();
            UpdateAmmo_ServerRpc();
        }
    }

    [ServerRpc]
    private void Shoot_ServerRpc()
    {
        if (isReloaing[gunId]) return;
        if (inCoolDown[gunId] || bulletsLoaded[gunId] <= 0) return;

        bulletsLoaded[gunId] -= 1;
        //Aplica cooldown se necessario
        if (guns[gunId].shotCooldown > 0 && IsOwner)
        {
            inCoolDown[gunId] = true;
            StartCoroutine("Cooldown");
        }
        for (int i = 0; i < guns[gunId].bulletPerShot; i++)
        {
            GameObject sourc = player.scopeGun && player.isFocused ? cam.gameObject : bulletPoint.gameObject;
            //Criar Bala
            Bullet bullet = Instantiate(
                guns[gunId].bulletPrefab,
                bulletPoint.position,
                bulletPoint.rotation
            );
            //Spawna a bala nos clientes
            NetworkObject netBullet = bullet.GetComponent<NetworkObject>();
            netBullet.Spawn();

            bullet.teamId.Value = player.GetTeam().teamId;
            bullet.damageOn.Value = guns[gunId].bulletDamage;

            bullet.owner = player.gameObject;

            deviation.x = Random.Range(-spread, spread) / 30;
            deviation.y = Random.Range(-spread, spread) / 30;
            bullet.rb.isKinematic = false;

            bullet.rb.AddForce(
                        (sourc.transform.forward +
                        sourc.transform.right * deviation.x +
                        sourc.transform.up * deviation.y)
                        * guns[gunId].bulletSpeed,
                        ForceMode.VelocityChange
                    );
            Destroy(bullet.gameObject, 5);
        }


        spread = Mathf.Min(spread + guns[gunId].spreadIncrease, guns[gunId].maxSpread);

        shotSound.pitch = Random.Range(0.9f, 1.1f);
        if (oneSound)
            shotSound.Play();
        else if (guns[gunId].firingSounds.Length > 0)
        {
            shotSound.clip = guns[gunId].firingSounds[
                Random.Range(0, guns[gunId].firingSounds.Length)
            ];
            shotSound.Play();
        }
        Shoot_ClientRpc();
    }

    [ClientRpc]
    private void Shoot_ClientRpc()
    {

        shotSound.pitch = Random.Range(0.9f, 1.1f);
        if (oneSound)
            shotSound.Play();
        else if (guns[gunId].firingSounds.Length > 0)
        {
            shotSound.clip = guns[gunId].firingSounds[
                Random.Range(0, guns[gunId].firingSounds.Length)
            ];
            shotSound.Play();
        }

        if (!IsOwner) return;
        //Aplica cooldown se necessario
        if (guns[gunId].shotCooldown > 0)
        {
            inCoolDown[gunId] = true;
            StartCoroutine("Cooldown");
        }
        spread = Mathf.Min(spread + guns[gunId].spreadIncrease, guns[gunId].maxSpread);


    }
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(guns[gunId].shotCooldown + focusCooldown);
        inCoolDown[gunId] = false;
        if (bulletsLoaded[gunId] <= 0 && !isReloaing[gunId] && IsOwner)
            Reload();
    }

    [ServerRpc]
    private void Realod_ServerRpc()
    {
        StartCoroutine(ReloadTimer());
        Realod_ClientRpc();
    }
    [ClientRpc]
    private void Realod_ClientRpc()
    {
        StartCoroutine(ReloadTimer());
    }

    private IEnumerator ReloadTimer()
    {
        if (bulletsLoaded[gunId] == guns[gunId].clip || isReloaing[gunId]) yield break;
        if (currentAmmo[gunId] <= 0)
        {
            currentAmmo[gunId] = 0;
            yield break;
        }
        reloadSound.Play();
        isReloaing[gunId] = true;
        if (IsOwner)
            player.ReloadWeapon(guns[gunId].gunID, isReloaing[gunId]);
        yield return new WaitForSeconds(guns[gunId].reloadTime);
        isReloaing[gunId] = false;

        int ammoAdded = bulletsLoaded[gunId];
        bulletsLoaded[gunId] =
            currentAmmo[gunId] - guns[gunId].clip >= 0
                ? guns[gunId].clip
                : bulletsLoaded[gunId] + currentAmmo[gunId];
        ammoAdded = bulletsLoaded[gunId] - ammoAdded;
        currentAmmo[gunId] -= ammoAdded;
        if (IsOwner)
        {
            player.ReloadWeapon(guns[gunId].gunID, isReloaing[gunId]);
            UpdateAmmo_ServerRpc();
        }
    }


    [ServerRpc]
    private void SwapGun_ServerRpc()
    {
        if (isReloaing[gunId])
        {
            return;
        }
        gunId++;
        if (gunId == guns.Length)
        {
            gunId = 0;
            if (!guns[gunId]) return;
        }
        if (!guns[gunId]) return;
        // player.ChangeWeapon(guns[gunId].animId);

        SwapGun_ClientRpc(gunId);
        AcquireWeapon(gunId, true);
        if (!swapperOnline.SwapToGun(guns[gunId].gunName))
        {
            Debug.LogError("Tried to swap to a gun model that does not exist in the prefab's gun holder");
        };
    }
    [ClientRpc]
    private void SwapGun_ClientRpc(int id)
    {
        gunId = id;
        player.ChangeWeapon(guns[gunId].animId);
        AcquireWeapon(gunId, true);
        if (!swapperOnline.SwapToGun(guns[gunId].gunName))
        {
            Debug.LogError("Tried to swap to a gun model that does not exist in the prefab's gun holder");
        };
    }

    public void Refill()
    {
        bulletsLoaded = new int[guns.Length];
        currentAmmo = new int[guns.Length];
        inCoolDown = new bool[guns.Length];
        isReloaing = new bool[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            AcquireWeapon(i);
            Refil_ServerRpc(i, bulletsLoaded[i], currentAmmo[i]);
        }
        Refil_ServerRpc(gunId, bulletsLoaded[gunId], currentAmmo[gunId]);
        if (IsOwner)
        {
            aimSprite.SetActive(true);
        }
    }
    [ServerRpc]
    private void Refil_ServerRpc(int index, int bulletsLoaded, int currentAmmo)
    {
        this.bulletsLoaded[index] = bulletsLoaded;
        this.currentAmmo[index] = currentAmmo;
        textAmmo.text = bulletsLoaded.ToString();
        textReserve.text = currentAmmo.ToString();

        UpdateAmmo_ClientRpc(bulletsLoaded, currentAmmo, index);
    }

    public void AddAmmo()
    {
        if (IsOwner)
        {
            AddAmmo_ServerRpc();
        }
    }

    [ServerRpc]
    public void AddAmmo_ServerRpc()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            currentAmmo[i] += guns[i].bulletsInAmmoBox;
            if (currentAmmo[i] > guns[i].maxAmmo)
            {
                currentAmmo[i] = guns[i].maxAmmo;
            }
            if (i == gunId)
                AddAmmo_ClientRpc(i, currentAmmo[i]);
        }
        textReserve.text = currentAmmo[gunId].ToString();
    }
    [ClientRpc]
    private void AddAmmo_ClientRpc(int id, int ammo)
    {
        currentAmmo[id] = ammo;
        textReserve.text = currentAmmo[id].ToString();
    }

    public void SetSwapper(GunSwapperOnline swapperOnline)
    {
        this.swapperOnline = swapperOnline;
    }
}
