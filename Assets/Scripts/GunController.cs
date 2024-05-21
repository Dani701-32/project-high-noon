using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunController : NetworkBehaviour
{
    [SerializeField]
    private GameObject gunSpot;

    [SerializeField]
    private Gun currentWeapon;
    bool auto;
    bool matchIsOver = false;

    MultiplayerManager multiplayerManager;
    GameManager gameManager;

    void Start()
    {
        if (IsOwner)
        {
            UpdateStats();
        }
        gameManager = GameManager.Instance ?? null;
        multiplayerManager = MultiplayerManager.Instance ?? null;
    }

    void Update()
    {
        // if (IsServer)
        // {
            if (IsOwner)
            {
                if (multiplayerManager.MatchOver)
                    return;
                if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Shoot RPC");
                    currentWeapon.Shoot_ServerRpc();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log("Realod RPC");
                    currentWeapon.Reload_ServerRpc();
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    print("got input");
                    currentWeapon.SwapGun_ServerRpc();
                    UpdateStats();
                }
            }
        // }
        // else
        // {
            // if (gameManager.MatchOver)
            //     return;
            // if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
            // {
            //     currentWeapon.Shoot();
            // }

            // if (Input.GetKeyDown(KeyCode.R))
            // {
            //     currentWeapon.Reload();
            // }

            // if (Input.GetKeyDown(KeyCode.Q))
            // {
            //     print("got input");
            //     currentWeapon.SwapGun();
            //     UpdateStats();
            // }
        // }
    }

    void UpdateStats()
    {
        auto = currentWeapon.IsAuto();
    }
}
