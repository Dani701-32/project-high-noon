using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerOnline : NetworkBehaviour
{
    [SerializeField] private TPSMovement tPSMovement;
    [SerializeField] Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoint = GameManager.Instance.spawnPoint;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        GetComponent<TPSMovement>().SetOrientation(spawnPoint);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
