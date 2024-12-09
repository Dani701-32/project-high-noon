using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    static OptionsManager _instance;

    [SerializeField] Slider sensiSlider;
    [SerializeField] Toggle dynCrossToggle;
    
    GameObject player;
    TPSCamera playerCam;
    CameraOnline onlineCam; 

    public bool dynamicAim = true;
    public float sensitivity = 50;

    public static OptionsManager Instance
    {
        get
        {
            if(_instance == null)
                Debug.LogError("Options manager é nulo");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        playerCam = FindObjectOfType<TPSCamera>();
        if (playerCam)
        {
            player = playerCam.gameObject;
            sensitivity = PlayerPrefs.GetFloat("SensitivitySetting", 500f);
        }
    }

    public void UpdateOptions()
    {
        
    }
}
