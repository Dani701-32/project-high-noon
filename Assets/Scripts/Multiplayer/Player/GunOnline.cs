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
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private GunSwapperOnline swapperOnline;
    [SerializeField] private PlayerOnline player;

    //Aim
    private Ray center;
    private float spread;
    private float focusSpread;
    private float focusCooldown;
    [SerializeField] private float aimLeftRightTweak;
    private float tweak;
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
    private bool[] inCoolDown;
    private bool[] isReloaing;
    private bool oneSound;

    public override void OnNetworkSpawn()
    {
        tweak = aimLeftRightTweak;
        bulletsLoaded = new int[guns.Length];
        currentAmmo = new int[guns.Length];
        inCoolDown = new bool[guns.Length];
        isReloaing = new bool[guns.Length];
        for (int i = 0; i < guns.Length; i++)
        {
            AcquireWeapon(i);
        }
        AcquireWeapon(0);
        if (IsOwner)
        {
            aimSprite.SetActive(true);
            UpdateAmmo_ServerRpc();
        }

        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) return;
        center = new Ray(cam.transform.position, cam.transform.forward);
        aimPoint = center.GetPoint(100);
        transform.parent.LookAt(aimPoint + Vector3.up * 3, Vector3.up);

        aimLeftRightTweak = player.scopeGun && player.isFocused ? 0 : tweak;
        aimSprite.transform.position = center.GetPoint(8) + aimSprite.transform.right * aimLeftRightTweak;

        accuracySprite.size = Vector2.one * Mathf.Max(0.25f, spread);

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
            UpdateAmmo_ServerRpc();
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
        if (guns[gunId].shotCooldown > 0)
        {
            inCoolDown[gunId] = true;
            StartCoroutine("Cooldown");
        }
        for (int i = 0; i < guns[gunId].bulletPerShot; i++)
        {
            //Criar Bala
            Bullet bullet = Instantiate(guns[gunId].bulletPrefab, bulletPoint.position, bulletPoint.rotation);
            //Spawna a bala nos clientes
            NetworkObject netBullet = bullet.GetComponent<NetworkObject>();
            netBullet.Spawn();
            bullet.teamId.Value = player.GetTeam().teamId;

            bullet.owner = player.gameObject;

            deviation.x = Random.Range(-spread, spread) / 10;
            deviation.y = Random.Range(-spread, spread) / 10;
            bullet.rb.AddForce(
                        (bulletPoint.transform.forward +
                        bulletPoint.transform.right * deviation.x +
                        bulletPoint.transform.up * deviation.y)
                        * guns[gunId].bulletSpeed,
                        ForceMode.VelocityChange
                    );

            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.isKinematic = false;

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
    }
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(guns[gunId].shotCooldown + focusCooldown);
        inCoolDown[gunId] = false;
        if (guns[gunId].autoReload)
        {
            Reload();
        }
    }

    [ServerRpc]
    private void Realod_ServerRpc()
    {
        if (bulletsLoaded[gunId] == guns[gunId].clip) return;
        if (currentAmmo[gunId] <= 0) currentAmmo[gunId] = 0;

        int ammoAdded = bulletsLoaded[gunId];
        bulletsLoaded[gunId] =
            currentAmmo[gunId] - guns[gunId].clip >= 0
                ? guns[gunId].clip
                : bulletsLoaded[gunId] + currentAmmo[gunId];
        ammoAdded = bulletsLoaded[gunId] - ammoAdded;
        currentAmmo[gunId] -= ammoAdded;
    }
    [ServerRpc]
    private void SwapGun_ServerRpc()
    {
        gunId++;
        if (gunId == guns.Length)
        {
            gunId = 0;
            if (!guns[gunId]) return;
        }
        if (!guns[gunId]) return;
        Debug.Log("GunOnline - Server");
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
        Debug.Log("GunOnline - Client");
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
        }
        if (IsOwner)
        {
            aimSprite.SetActive(true);
            UpdateAmmo_ServerRpc();
        }
    }

    [ServerRpc]
    public void AddAmmo_ServerRpc(int ammo)
    {
        currentAmmo[gunId] += ammo * guns[gunId].bulletsInAmmoBox;
        currentAmmo[gunId] = Mathf.Min(currentAmmo[gunId], guns[gunId].maxAmmo);
        UpdateAmmo_ServerRpc();
        AddAmmo_ClientRpc(currentAmmo[gunId]);
    }
    [ClientRpc]
    private void AddAmmo_ClientRpc(int ammo)
    {
        currentAmmo[gunId] = ammo;
    }
}
