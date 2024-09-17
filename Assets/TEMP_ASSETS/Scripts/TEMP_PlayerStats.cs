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

    [Header("Stats")]
    [ReadOnly] public int HP = 100;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool carryingScopedGun;
    [SerializeField] GameObject model;

    [SerializeField]
    private int HPMax = 100;
    [SerializeField, ReadOnly]
    bool _hasFlag;
    [ReadOnly]
    public bool focused;
    [ReadOnly] 
    public bool grounded;

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

    public TeamData GetTeam()
    {
        return team;
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
