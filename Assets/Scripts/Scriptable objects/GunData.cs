using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "ScriptableObjects/Gun data", order = 2)]
public class GunData : ScriptableObject
{
    public string gunName;
    public float bulletSpeed;
    public float spread;
    public float shotCooldown;
    public float reloadTime;
    public int maxAmmo;
    public int clip;
    public bool autoFire;
}