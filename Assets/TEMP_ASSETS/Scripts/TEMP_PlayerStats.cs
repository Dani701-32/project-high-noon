
using UnityEngine;

public class TEMP_PlayerStats : MonoBehaviour
{
    [Header("Team data")] 
    [SerializeField] TeamData team;
    [Header("Toggleable objects")]
    [SerializeField] GameObject flagCarryObject;
    [SerializeField] GameObject flagCarryEffects;
    //public int HP = 100;

    //private int HPMax = 100;
    [SerializeField, ReadOnly]
    bool _hasFlag;
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
        if(flagCarryObject != null) flagCarryObject.SetActive(!hasFlag);
        if(flagCarryEffects != null) flagCarryEffects.SetActive(!hasFlag);
    }
}
