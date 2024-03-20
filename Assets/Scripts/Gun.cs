using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private GameObject bulletPoint;

    [SerializeField]
    private float bulletSpeed;

    public void Shoot()
    {
        Debug.Log("Atirando");
        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletPoint.transform.position,
            bulletPoint.transform.rotation
        );
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bulletSpeed);
        Destroy(bullet, 2);
    }
}
