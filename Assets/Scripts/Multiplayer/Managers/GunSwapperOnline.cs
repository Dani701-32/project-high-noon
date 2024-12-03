using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GunSwapperOnline : NetworkBehaviour
{
    [SerializeField] List<GameObject> guns;
    Dictionary<string, GameObject> gunsDictionary;
    [SerializeField] private GameObject currentGun;
    [SerializeField] private string tagBulletPoint = "BulletPoint";
    public GameObject bulletPoint;

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
        bulletPoint = GetChildWithTag(currentGun, tagBulletPoint);
    }

    public bool SwapToGun(string gunName)
    {
        gunName = gunName.ToLower();
        GameObject prev = currentGun;
        gunsDictionary.TryGetValue(gunName, out currentGun);
        if (currentGun)
        {
            bulletPoint = GetChildWithTag(currentGun, tagBulletPoint);
            prev.SetActive(false);
            currentGun.SetActive(true);
            return true;
        }
        currentGun = prev;
        Debug.Log("Arma n encontrada");
        return false;
    }

    private GameObject GetChildWithTag(GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }

        }
        return null;
    }
}
