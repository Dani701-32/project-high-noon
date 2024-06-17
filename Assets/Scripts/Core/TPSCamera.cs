using System;
using System.Collections;
using UnityEditor;
using UnityEngine;


// ReSharper disable Unity.InefficientPropertyAccess

public class TPSCamera : MonoBehaviour
{
    float pitch;
    float yaw;
    float angleDiff;
    Transform camTrans;
    IEnumerator waiting;
    TEMP_PlayerStats stats;
    Camera cam;

    [SerializeField]
    GameObject camParent;

    [SerializeField] KeyCode teclaDeFoco;

    [Header("Angles")]
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
    private bool matchIsOver;

    [SerializeField] Transform nearCamPos;
    [SerializeField] Transform farCamPos;
    [SerializeField] float fovChangeSpeed;

    void Awake()
    {
        // Eu não sei explicar que porra que tá rolando aqui mas normalmente começar o char em qualquer ângulo fora 0 no Y quebra tudo
        // Coloquei isso aqui e tudo foi resolvido. EU NÃO SEI O MOTIVO.
        transform.rotation = Quaternion.identity;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = OptionsManager.Instance.sensitivity;

        canMove = false;
        waiting = WaitForStart();
        StartCoroutine(waiting);
        
        stats = GetComponent<TEMP_PlayerStats>();
        cam = camParent.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        matchIsOver = (GameManager.Instance != null)? GameManager.Instance.MatchOver : MultiplayerManager.Instance.MatchOver;
        if (matchIsOver)
            return;
        if (canMove)
        {
            yaw += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            pitch += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, maxLookUpAngle, maxLookDownAngle);

            camParent.transform.localRotation = Quaternion.Euler(yaw, 0, 0);
            transform.rotation = Quaternion.Euler(0, pitch, 0);
            
            stats.focused = stats.grounded && Input.GetKey(teclaDeFoco);
            stats.focusInterp = Mathf.Clamp(stats.focusInterp + (stats.focused ? 1 : -1)*Time.deltaTime*fovChangeSpeed, 0, 1);
            
            cam.transform.localPosition =
                Vector3.Lerp(farCamPos.localPosition, nearCamPos.localPosition, stats.focusInterp);
        }
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        pitch = transform.eulerAngles.y;
        canMove = true;
    }
}
