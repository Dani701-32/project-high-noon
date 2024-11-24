using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FlagSpot : NetworkBehaviour
{
    [SerializeField]
    bool isActive = true;

    [SerializeField]
    private GameObject flagModel;

    public void ActiveFlag()
    {
        isActive = true;
        flagModel.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with this object!");
            isActive = false;
            PlayerOnline playerOnline = other.GetComponentInParent<PlayerOnline>();
            if(playerOnline){
                playerOnline.hasFlag = true; 
            }else{

                other.GetComponentInParent<PlayerStats>().hasFlag = true;
            }
            flagModel.SetActive(false);
        }
    }
}
