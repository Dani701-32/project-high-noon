using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject bulletPoint;
    [SerializeField]
    private GameObject aimSprite;
    [SerializeField]
    private GameObject accuracySprite;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private AudioSource shotSound;

    [Header("Gun stats")] 
    [SerializeField] 
    private GunData stats;

    [Header("Misc")]
    [SerializeField]
    private float aimLeftRightTweak;

    // Aim stuff
    Vector3 aimPoint;
    Vector3 center;
    Vector3 deviation;
    Ray ray;
    RaycastHit hit;
    float aimDistance;
    float spread;

    // Updating gun stats
    [SerializeField, ReadOnly] int bulletsLoaded;
    [SerializeField, ReadOnly] int currentAmmo;
    bool inCooldown;
    bool isReloading;
    bool oneSound;

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
        spread = stats.minSpread;
        oneSound = stats.firingSounds.Length == 1;
        if (oneSound)
            shotSound.clip = stats.firingSounds[0];
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
                stats.bulletPrefab,
                bulletPoint.transform.position,
                bulletPoint.transform.rotation
            );
            deviation.x = Random.Range(-spread, spread)/10;
            deviation.y = Random.Range(-spread, spread)/10;
            bullet.GetComponent<Rigidbody>().AddForce((transform.forward + transform.right*deviation.x + transform.up*deviation.y) * stats.bulletSpeed);
            Destroy(bullet, 2);
            spread = Mathf.Min(spread + stats.spreadIncrease, stats.maxSpread);
            shotSound.pitch = Random.Range(0.9f, 1.1f);
            if (oneSound)
                shotSound.Play();
            else if (stats.firingSounds.Length > 0)
            {
                shotSound.clip = stats.firingSounds[Random.Range(0, stats.firingSounds.Length)];
                shotSound.Play();
            }
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
        
        accuracySprite.transform.localScale = Vector3.one * (spread + 0.25f);
    }

    void FixedUpdate()
    {
        if (spread > stats.minSpread)
            spread = Mathf.Max(spread - stats.spreadRecovery, stats.minSpread);
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
