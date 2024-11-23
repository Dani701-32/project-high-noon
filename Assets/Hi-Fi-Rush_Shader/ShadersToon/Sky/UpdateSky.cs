using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UpdateSky : MonoBehaviour
{
    public Light sunLight;

    [SerializeField] private Material skyboxMat;

    void Start()
    {
        skyboxMat.SetVector(name = "_MainLightDirection", sunLight.transform.forward);
    }

    void Update()
    {
        

    }
}
