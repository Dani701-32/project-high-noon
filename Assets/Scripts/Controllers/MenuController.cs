using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        if(sceneName == "Menu"){
           
            Cleanup(); 
        }
        SceneManager.LoadScene(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
    void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
             NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
}
