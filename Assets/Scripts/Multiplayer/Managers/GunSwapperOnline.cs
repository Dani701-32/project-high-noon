using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSwapperOnline : MonoBehaviour
{
    [SerializeField] List<GameObject> guns;
    Dictionary<string, GameObject> gunsDictionary;
    [SerializeField] private GameObject currentGun;

    // Start is called before the first frame update
    private void Awake()
    {
        gunsDictionary = new Dictionary<string, GameObject>();
        foreach (GameObject gun in guns)
        {
            gunsDictionary.Add(gun.name.ToLower(), gun.gameObject);
            if (!currentGun && gun.activeInHierarchy)
            {
                currentGun = gun;
            }
        }
    }

    public bool SwapToGun(string gunName)
    {
        gunName = gunName.ToLower();
        GameObject prev = currentGun;
        gunsDictionary.TryGetValue(gunName, out currentGun);
        if (currentGun)
        {
            prev.SetActive(false);
            currentGun.SetActive(true);
            return true;
        }
        currentGun = prev;
        Debug.Log("Arma n encontrada");
        return false;
    }
}
