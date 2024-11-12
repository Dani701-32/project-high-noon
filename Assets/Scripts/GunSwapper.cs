using System.Collections.Generic;
using UnityEngine;

public class GunSwapper : MonoBehaviour
{
    [SerializeField] List<GameObject> guns;
    Dictionary<string, GameObject> gunDictionary;
    [SerializeField] GameObject currGun;

    void Start()
    {
        gunDictionary = new Dictionary<string, GameObject>();
        foreach (GameObject gun in guns)
        {
            gunDictionary.Add(gun.name.ToLower(), gun.gameObject);
            if (!currGun && gun.activeInHierarchy)
                currGun = gun;
        }
    }

    public bool SwapToGun(string newGun)
    {
        newGun = newGun.ToLower();
        GameObject prev = currGun;
        gunDictionary.TryGetValue(newGun, out currGun);
        if (currGun)
        {
            currGun.SetActive(true);
            prev.SetActive(false);
            return true;
        }
        currGun = prev;
        return false;
    }
}
