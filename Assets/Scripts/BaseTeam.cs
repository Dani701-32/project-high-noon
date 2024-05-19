using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTeam : MonoBehaviour
{
    [SerializeField]
    TeamData teamData;

    [SerializeField]
    private GameObject particles;

    [SerializeField]
    private GameObject baseObj;

    // Start is called before the first frame update
    void Start()
    {
        if (teamData != null)
        {
            var mainModule = particles.GetComponent<ParticleSystem>().main;
            mainModule.startColor = teamData.teamColor;
            baseObj.GetComponent<MeshRenderer>().material = teamData.teamEquipMaterial;
        }
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOnline playerOnline = other.GetComponentInParent<PlayerOnline>();
            if (playerOnline)
            {
                if (playerOnline.GetTeam().teamName == teamData.teamName && playerOnline.hasFlag)
                {
                    playerOnline.hasFlag = false;
                    MultiplayerManager.Instance.AddPoint(teamData); 
                }
            }
            else
            {

                TEMP_PlayerStats playerStats = other.GetComponentInParent<TEMP_PlayerStats>();
                if (playerStats.GetTeam().teamName == teamData.teamName && playerStats.hasFlag)
                {
                    playerStats.hasFlag = false;
                    GameManager.Instance.AddPoint(teamData);
                }
            }

        }
    }
}
