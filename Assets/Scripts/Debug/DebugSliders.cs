
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DebugSliders : MonoBehaviour
{
    //TPSMovement move;
    TPSCamera cam;
    Slider slider;
    
    [SerializeField] Text sliderText;
    [SerializeField] GameObject player;
    void Start()
    {
        //move = player.GetComponent<TPSMovement>();
        cam = player.GetComponent<TPSCamera>();
        slider = GetComponent<Slider>();
    }

    public void SetCamSpeed()
    {
        sliderText.text = slider.value.ToSafeString();
    }
}
