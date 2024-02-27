using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class TPSCamera : MonoBehaviour
{
    float pitch;
    float yaw;
    float angleDiff;
    Camera myCamera;
    Transform camTrans;
    IEnumerator waiting;
    
    [SerializeField] GameObject camParent;
    [SerializeField] Transform orientation;
    [SerializeField] Transform body;
    
    [Header("Angles")]
    [Tooltip("Ângulo máximo antes do jogador começar a virar para acompanhar a câmera")]
    [SerializeField] float maxTurnAngle;
    [Tooltip("Ângulo máximo que o jogador pode olhar acima de si")]
    [SerializeField] float maxLookUpAngle = -55;
    [Tooltip("Ângulo máximo que o jogador pode olhar abaixo de si")]
    [SerializeField] float maxLookDownAngle = 50;
    [Tooltip("Velocidade de rotação da câmera")]
    public float sensibility = 1;
    
    [Header("Debug and temporary values")]
    [Tooltip("Mostrar informações de desenvolvimento")]
    [SerializeField] bool debugGizmos;
    [SerializeField] float gizmoHeight = 1;
    [Tooltip("O jogador está em jogo ou está ocupado com algo?")] // LEVAR ESSA VARIÁVEL PRO GAME MANAGER MAIS TARDE
    public bool canMove;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        canMove = false;
        waiting = WaitForStart();
        StartCoroutine(waiting);

        myCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (canMove)
        {
            yaw += -Input.GetAxis("Mouse Y") * sensibility * Time.deltaTime;
            pitch += Input.GetAxis("Mouse X") * sensibility * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, maxLookUpAngle, maxLookDownAngle);
            pitch = Mathf.Clamp(pitch, -maxTurnAngle - 0.5f, maxTurnAngle + 0.5f);
            
            camParent.transform.localRotation = Quaternion.Euler(yaw, pitch, 0);
            angleDiff = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, camParent.transform.rotation.eulerAngles.y);
            orientation.rotation = Quaternion.Euler(0, myCamera.transform.rotation.eulerAngles.y, 0);
            
            if ((angleDiff > maxTurnAngle || angleDiff < -maxTurnAngle) && Input.GetAxis("Mouse X") != 0)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
            }

            body.rotation = orientation.rotation;
        }
    }

    void OnDrawGizmos()
    {
        if (!debugGizmos) return;
        
        float gizmoDist = 1;
        float gizmoThickness = 8;
        
        Transform[] transforms = {transform, orientation};
        int i = 1;
        foreach (Transform trans in transforms)
        {
            Vector3 pos = trans.position + Vector3.up * gizmoHeight;
            Handles.DrawBezier(pos, pos + trans.forward * gizmoDist, pos, pos + trans.forward * gizmoDist, Color.blue * i, null, gizmoThickness);
            Handles.DrawBezier(pos, pos + trans.up * gizmoDist, pos, pos + trans.up * gizmoDist, Color.green * i, null, gizmoThickness);
            Handles.DrawBezier(pos, pos + trans.right * gizmoDist, pos, pos + trans.right * gizmoDist, Color.red * i, null, gizmoThickness);
            i++;
        }
        
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        canMove = true;
    }
}
