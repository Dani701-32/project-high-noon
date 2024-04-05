using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "ScriptableObjects/Gun data", order = 2)]
public class GunData : ScriptableObject
{
    public string gunName;
    
    [Header("Spread")]
    public float maxSpread;
    public float minSpread;
    public float spreadIncrease;
    public float spreadRecovery;

    [Header("Bullet stats")] 
    public GameObject bulletPrefab;
    public float bulletSpeed = 10000;
    
    [Header("Generic data")]
    public float shotCooldown;
    public float reloadTime;
    public int maxAmmo;
    public int clip;
    public AudioClip[] firingSounds;
    
    [Header("Special")]
    public bool autoFire;
}