using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagSpot : MonoBehaviour
{
    [SerializeField]
    bool isActive = true;

    void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with this object!");
            isActive = false;
            other.GetComponentInParent<TEMP_PlayerStats>().hasFlag = true;
            gameObject.SetActive(false);
        }
    }
}