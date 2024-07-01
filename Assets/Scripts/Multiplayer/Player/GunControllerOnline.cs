using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunControllerOnline : NetworkBehaviour
{
    [SerializeField, ReadOnly] private MultiplayerManager multiplayerManager;
    [SerializeField] private GameObject gunSpot; //Local da Arma
    [SerializeField] private GunOnline currentGun; //Script da Arma online
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
        if (!IsOwner || multiplayerManager.MatchOver) return;
        if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
        {
            currentGun.Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentGun.Reload();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentGun.SwapGun();
            UpdateStats();
        }

    }
    private void UpdateStats()
    {
        auto = currentGun.IsAuto();
    }
    
    public void RefillWeapons(){
        currentGun.Refill(); 
    }
}
