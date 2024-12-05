using UnityEngine;

public class PlayerStats : MonoBehaviour
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
    [ReadOnly] public int HP;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool carryingScopedGun;
    [SerializeField] GameObject model;

    [SerializeField]
    private int HPMax = 6;
    [SerializeField, ReadOnly]
    bool _hasFlag;
    [ReadOnly]
    public bool focused;
    [ReadOnly] 
    public bool grounded;

    EventManager events;

    private void Start()
    {
        HP = Mathf.Max(HPMax, 1);
        events = EventManager.Instance;
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

    public void Damage(int damage)
    {
        HP -= damage;
        GameManager manager = GameManager.Instance;
        if (manager && manager.HPSlider)
            manager.HPSlider.value = HP;
        if (HP <= 0)
            Death();
    }

    private void FlagUpdate()
    {
        if (flagCarryObject != null)
            flagCarryObject.SetActive(!hasFlag);
        if (flagCarryEffects != null)
            flagCarryEffects.SetActive(!hasFlag);
        flagCarryObject.GetComponent<MeshRenderer>().material.color = team.teamColor;
    }

    void Death()
    {
        GameManager manager = GameManager.Instance;
        if (manager.GetStatus().Equals("Survival"))
            manager.SetSurvivalTimer(false);
    }
}
