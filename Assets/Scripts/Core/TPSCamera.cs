using System.Collections;
using UnityEditor;
using UnityEngine;


// ReSharper disable Unity.InefficientPropertyAccess

public class TPSCamera : MonoBehaviour
{
    float pitch;
    float yaw;
    float angleDiff;
    Camera myCamera;
    Transform camTrans;
    IEnumerator waiting;

    [SerializeField]
    GameObject camParent;

    //[SerializeField] Transform body;

    [Header("Angles")]
    [Tooltip("Ângulo máximo antes do jogador começar a virar para acompanhar a câmera")]
    [SerializeField]
    float maxTurnAngle;

    [Tooltip("Ângulo máximo que o jogador pode olhar acima de si")]
    [SerializeField]
    float maxLookUpAngle = -55;

    [Tooltip("Ângulo máximo que o jogador pode olhar abaixo de si")]
    [SerializeField]
    float maxLookDownAngle = 50;

    [Tooltip("Velocidade de rotação da câmera")]
    public float sensitivity = 1;

    [Header("Debug and temporary values")]
    [Tooltip("O jogador está em jogo ou está ocupado com algo?")] // LEVAR ESSA VARIÁVEL PRO GAME MANAGER MAIS TARDE
    public bool canMove;
    private bool matchIsOver = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = OptionsManager.Instance.sensitivity;

        canMove = false;
        waiting = WaitForStart();
        StartCoroutine(waiting);

        myCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        matchIsOver = (GameManager.Instance != null)? GameManager.Instance.MatchOver : MultiplayerManager.Instance.MatchOver;
        if (matchIsOver)
            return;
        if (canMove)
        {
            yaw += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            pitch = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, maxLookUpAngle, maxLookDownAngle);
            pitch = Mathf.Clamp(pitch, -maxTurnAngle - 0.5f, maxTurnAngle + 0.5f);
            
            camParent.transform.localRotation = Quaternion.Euler(yaw, 0, 0);
            transform.Rotate(0, pitch, 0);
        }
    }

    void FixedUpdate()
    {
        if (!canMove || matchIsOver)
            return;
        //body.rotation = orientation.rotation;

        //angleDiff = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, camParent.transform.rotation.eulerAngles.y);
        //if ((angleDiff > maxTurnAngle || angleDiff < -maxTurnAngle) && Input.GetAxis("Mouse X") != 0)
        //{
        
        //}
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        canMove = true;
    }
}
