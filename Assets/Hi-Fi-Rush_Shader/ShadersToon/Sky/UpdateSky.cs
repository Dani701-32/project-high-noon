using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UpdateSky : MonoBehaviour
{
    public Light sunLight;

    public Vector3 teste;


    //public override void OnNetworkSpawn()
    //{
    //    if(IsOwner)
    //    {
            
    //    }
    //    teste = sunLight.transform.forward;

    //    RenderSettings.skybox.SetVector("_MainLightDirection", teste);

    //    Debug.Log(teste);

    //    base.OnNetworkSpawn();
    //}
    //private void Start()
    //{
    //    teste = sunLight.transform.forward;

    //    RenderSettings.skybox.SetVector("_MainLightDirection", teste);
    //}
}
