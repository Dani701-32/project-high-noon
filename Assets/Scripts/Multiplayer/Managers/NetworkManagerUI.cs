using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button btnHost;

    [SerializeField]
    private Button btnServer;

    [SerializeField]
    private Button btnClient;

    [Header("User input")]
    [SerializeField]
    TMP_InputField inputIp;
    private UnityTransport unityTransport;
    List<string> defaultInputs = new List<string> { "ip", "port", "localhost", "" };

    public GameObject camTeste, canvas; 

    private void Start()
    {
        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        btnHost.onClick.AddListener(() =>
        {
            ConectionType.type = "host"; 
            camTeste.SetActive(false);
            canvas.SetActive(false);
            NetworkManager.Singleton.StartHost();
        });
        btnServer.onClick.AddListener(() =>
        {
            ConectionType.type = "server"; 
            camTeste.SetActive(false);
            canvas.SetActive(false);
            NetworkManager.Singleton.StartServer();
        });
        btnClient.onClick.AddListener(() =>
        {
            //Verifica se algum dado foi mandado para o servidor
            if (defaultInputs.Contains(inputIp.text.ToLower()) || inputIp.text.ToLower() == null)
                inputIp.text = "127.0.0.1";
            //Se conecta com o servidor desejado
            unityTransport.ConnectionData.Address = inputIp.text;
            ConectionType.type = "client"; 
            camTeste.SetActive(false);
            canvas.SetActive(false);
            NetworkManager.Singleton.StartClient();
        });
    }
}

public class ConectionType
{
    public static string type;
}
