using UnityEngine;

public class TEMP_PlayerStats : MonoBehaviour
{
    [Header("Team data")]
    [SerializeField]
    TeamData team;

    [Header("Toggleable objects")]
    [SerializeField]
    GameObject flagCarryObject;

    [SerializeField]
    GameObject flagCarryEffects;

    //public int HP = 100;
    [SerializeField]
    GameObject model;

    //private int HPMax = 100;
    [SerializeField, ReadOnly]
    bool _hasFlag;

    private void Start()
    {
        if (team != null)
        {
            model.GetComponent<MeshRenderer>().material = team.teamEquipMaterial;
        }
    }

    public bool hasFlag
    {
        get => _hasFlag;
        set
        {
            if (_hasFlag != value)
            {
                FlagUpdate();
            }
            _hasFlag = value;
        }
    }

    private void FlagUpdate()
    {
        if (flagCarryObject != null)
            flagCarryObject.SetActive(!hasFlag);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(!hasFlag);
        flagCarryObject.GetComponent<MeshRenderer>().material.color = team.teamColor;
    }
}
