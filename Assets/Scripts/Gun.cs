using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject bulletPoint;
    [SerializeField]
    private GameObject aimSprite;
    [SerializeField]
    private Camera cam;

    [Header("Gun stats")] 
    [SerializeField] 
    private GunData stats;

    [Header("Misc")]
    [SerializeField]
    private float aimLeftRightTweak;

    // Aim stuff
    Vector3 aimPoint;
    Vector3 center;
    Ray ray;
    RaycastHit hit;
    float aimDistance;
    Vector3 deviation;

    // Updating gun stats
    [SerializeField, ReadOnly] int bulletsLoaded;
    [SerializeField, ReadOnly] int currentAmmo;
    bool inCooldown;
    bool isReloading;

    void Start()
    {
        center = Vector3.one / 2;
        center.x += aimLeftRightTweak/100;
        
        AcquireWeapon();
    }

    public void AcquireWeapon()
    {
        bulletsLoaded = stats.clip;
        currentAmmo = stats.maxAmmo;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(stats.shotCooldown);
        inCooldown = false;
    }

    public void Shoot()
    {
        if (isReloading) return;
        
        if (!inCooldown && bulletsLoaded > 0)
        {
            bulletsLoaded -= 1;
            if (stats.shotCooldown > 0)
            {
                inCooldown = true;
                StartCoroutine("Cooldown");
            }
            GameObject bullet = Instantiate(
                bulletPrefab,
                bulletPoint.transform.position,
                bulletPoint.transform.rotation
            );
            deviation.x = Random.Range(-stats.spread, stats.spread);
            deviation.y = Random.Range(-stats.spread, stats.spread);
            bullet.GetComponent<Rigidbody>().AddForce((transform.forward + deviation/10) * stats.bulletSpeed);
            Destroy(bullet, 2);
        }
    }

    public void Reload()
    {
        if (bulletsLoaded == stats.clip) return;
        if (currentAmmo <= 0)
        {
            currentAmmo = 0;
            return;
        }

        int oldAmmo = bulletsLoaded;
        bulletsLoaded = currentAmmo - stats.clip >= 0 ? stats.clip : currentAmmo;
        currentAmmo -= Math.Max(stats.clip - oldAmmo, 0);
    }

    void Update()
    {
        center.z = cam.farClipPlane;
        aimPoint = cam.ViewportToWorldPoint(center);
        transform.parent.LookAt(aimPoint, Vector3.up);
        
        ray = new Ray(transform.position, aimPoint);
        Physics.Raycast(ray, out hit, 8, 1 << 3);
        aimDistance = hit.distance != 0 ? Mathf.Max(hit.distance, 3) : 8;
        aimSprite.transform.position = ray.GetPoint(aimDistance);
    }

    public bool isAuto()
    {
        if (stats)
            return stats.autoFire;
        else
        {
            Debug.LogError("Gun has no stats object attached");
            return false;
        }
    }
}
