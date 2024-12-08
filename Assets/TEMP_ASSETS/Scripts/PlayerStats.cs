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
    [ReadOnly] public float HP;
    [SerializeField] private float HPMax = 6;
    [SerializeField, Range(0.01f, 0.15f)] private float percentHealth;
    [SerializeField, ReadOnly] private float currentHealthTimer = 0f;
    [SerializeField, ReadOnly] private float maxHealthTimer = 3f;
    [SerializeField, Range(0.01f, 1f)] private float timerRegen = 1f;
    private float timer;
    [ReadOnly] public float focusInterp;
    [ReadOnly] public bool carryingScopedGun;
    [SerializeField] GameObject model;

    [SerializeField, ReadOnly]
    bool _hasFlag;
    [ReadOnly]
    public bool focused;
    [ReadOnly]
    public bool grounded;
    EventManager events;
    GameManager manager;

    private void Start()
    {
        HP = Mathf.Max(HPMax, 1);
        events = EventManager.Instance;
        manager = GameManager.Instance;
        if (team != null)
        {
            model.GetComponent<MeshRenderer>().material = team.teamEquipMaterial;
        }
    }
    void Update()
    {
        if (HP < HPMax)
        {
            currentHealthTimer -= Time.deltaTime;
            if (currentHealthTimer <= 0f)
            {
                currentHealthTimer = 0f;
                timer += Time.deltaTime;
                if (timer >= timerRegen)
                {
                    RegenHealth();
                    manager.HPSlider.value = HP;
                    timer = 0f;
                }

            }
        }
    }
    private void RegenHealth()
    {
        float percent = percentHealth * HPMax;
        HP = Mathf.Min(HP + percent, HPMax);
        if (HP == HPMax)
            currentHealthTimer = maxHealthTimer;

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
        currentHealthTimer = maxHealthTimer;
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
