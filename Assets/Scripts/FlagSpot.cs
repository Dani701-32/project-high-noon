using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagSpot : MonoBehaviour
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

                other.GetComponentInParent<TEMP_PlayerStats>().hasFlag = true;
            }
            flagModel.SetActive(false);
        }
    }
}
