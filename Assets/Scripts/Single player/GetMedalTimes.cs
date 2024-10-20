using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetMedalTimes : MonoBehaviour
{
    [SerializeField] Image[] medals = new Image[3];
    [SerializeField] TextMeshProUGUI[] times = new TextMeshProUGUI[3];
    void Start()
    {
        if (!gameObject.activeInHierarchy || !GameManager.Instance)
            return;
        GameManager GM = GameManager.Instance;
        
        medals[0].sprite = GM.goldMedal;
        int minutes = Mathf.FloorToInt(GM.goldSeconds / 60F);
        int seconds = Mathf.FloorToInt(GM.goldSeconds - minutes * 60);
        times[0].text = $"{minutes:0}:{seconds:00}";
        
        medals[1].sprite = GM.silverMedal;
        minutes = Mathf.FloorToInt(GM.silverSeconds / 60F);
        seconds = Mathf.FloorToInt(GM.silverSeconds - minutes * 60);
        times[1].text = $"{minutes:0}:{seconds:00}";
        
        medals[2].sprite = GM.bronzeMedal;
        minutes = Mathf.FloorToInt(GM.bronzeSeconds / 60F);
        seconds = Mathf.FloorToInt(GM.bronzeSeconds - minutes * 60);
        times[2].text = $"{minutes:0}:{seconds:00}";
    }
    
}
