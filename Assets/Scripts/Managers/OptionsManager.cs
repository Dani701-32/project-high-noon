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

    public bool dynamicAim = true;
    public float sensitivity = 500;

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
        }
    }

    public void UpdateOptions()
    {
        
    }
}