using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown graphicsDropdown;
    public Toggle toggleFullScreen; 

    Resolution[] resolutions;

    private void GetResolutions()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        bool selected = false;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (PlayerPrefs.HasKey("ResolutionIndex") && PlayerPrefs.GetInt("ResolutionIndex") == i)
            {
                selected = true;
                currentResolutionIndex = i; // Resolução salva nos PlayerPrefs
            }
            else if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height && !selected)
            {
                currentResolutionIndex = i; // Resolução atual da tela
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void Start()
    {
        //Chama a fun��o que controla as resolu��es de cada monitor
        GetResolutions();

        //Come�a no m�dio pra todos
        
        int savedQuality = PlayerPrefs.GetInt("QualitySetting", 1);
        QualitySettings.SetQualityLevel(savedQuality);
        graphicsDropdown.value = savedQuality;

        //Come�a fora da tela cheia
        bool isFullScreen = PlayerPrefs.GetInt("FullScreen", 0) == 1;
        toggleFullScreen.isOn = isFullScreen;
        Screen.fullScreen = isFullScreen;

        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        if (savedResolutionIndex >= 0 && savedResolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[savedResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            resolutionDropdown.value = savedResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }
    public void SetQuality(int qualityindex)
    {
        QualitySettings.SetQualityLevel(qualityindex);
        PlayerPrefs.SetInt("QualitySetting", qualityindex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
    }

    public void SetResolution(int resolutionindex)
    {
        Resolution resolution = resolutions[resolutionindex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionindex);
    }
}
