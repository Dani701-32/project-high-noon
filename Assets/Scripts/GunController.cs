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
        if (IsOwner)
        {

            matchIsOver =  gameManager == null ? multiplayerManager.MatchOver : gameManager.MatchOver;
            if (matchIsOver)
                return;
            if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
            {
                // if(IsServer){
                    currentWeapon.Shoot_ServerRpc(); 
                // }else{
                    // currentWeapon.Shoot();
                // }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.Reload();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                print("got input");
                currentWeapon.SwapGun();
                UpdateStats();
            }
        }
    }

    void UpdateStats()
    {
        auto = currentWeapon.IsAuto();
    }
}
