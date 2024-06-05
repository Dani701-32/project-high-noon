using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentWeapon;
    bool auto;
    GameManager gameManager;

    void Start()
    {
        UpdateStats();
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        
        if (gameManager.MatchOver)
            return;
        if (auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
        {
            currentWeapon.Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.Reload();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!currentWeapon.gunLocked)
            {
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
