using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private GameObject gunSpot; 
    [SerializeField] private Gun currentWeapon;
    bool auto;


    void Start()
    {
        UpdateStats();
    }
 
    void Update()
    {
        if(auto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0)){
            currentWeapon.Shoot();
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

    void UpdateStats()
    {
        auto = currentWeapon.IsAuto();
    }

   
}
