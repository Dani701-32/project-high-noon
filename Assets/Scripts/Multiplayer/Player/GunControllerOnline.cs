using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunControllerOnline : NetworkBehaviour
{
    [SerializeField] private PlayerOnline player;
    [SerializeField, ReadOnly] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameObject gunSpot; //Local da Arma
    public GunOnline currentGun; //Script da Arma online
    [SerializeField] private GameObject currentGunModel; //Modelo atual do player
    private bool auto;

    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        multiplayerManager = MultiplayerManager.Instance != null ? MultiplayerManager.Instance : null;
        if (IsOwner)
        {
            UpdateStats();
        }
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || multiplayerManager.MatchOver || player.isPaused) return;
        if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0) && !player.isDead.Value)
        {
            currentGun.Shoot();
            player.ShootRecoil();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentGun.Reload();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("GunController");
            currentGun.SwapGun();
            UpdateStats();
        }

    }
    private void UpdateStats()
    {
        auto = currentGun.IsAuto();
    }

    public void RefillWeapons()
    {
        currentGun.Refill();
    }
    public void AddAmmo(int ammo)
    {
        if (IsOwner)
        {
            currentGun.AddAmmo_ServerRpc(ammo);
        }
    }
}
