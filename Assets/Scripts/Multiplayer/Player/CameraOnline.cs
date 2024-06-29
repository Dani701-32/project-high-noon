using System.Collections;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
public class CameraOnline : NetworkBehaviour
{
    float pitch;
    float yaw;
    float angleDiff;
    Transform camTrans;
    Camera myCamera;
    IEnumerator waiting;
    [SerializeField] private GameObject camParent;
    [SerializeField] private Transform orientation;
    [SerializeField] KeyCode key;
    [Header("Angles")]
    [Tooltip("Ângulo máximo antes do jogador começar a virar para acompanhar a câmera")]
    [SerializeField] private float maxTurnAngle;

    [Tooltip("Ângulo máximo que o jogador pode olhar acima de si")]
    [SerializeField] private float maxLookUpAngle = -55;

    [Tooltip("Ângulo máximo que o jogador pode olhar abaixo de si")]
    [SerializeField]private float maxLookDownAngle = 50;

    [Tooltip("Velocidade de rotação da câmera")]
    public float sensitivity = 1;

    [Header("Debug and temporary values")]
    [Tooltip("O jogador está em jogo ou está ocupado com algo?")] // LEVAR ESSA VARIÁVEL PRO GAME MANAGER MAIS TARDE
    public bool canMove;
    public bool canFocus = true;
    private bool matchIsOver = false;

    [SerializeField] Transform nearCamPos;
    [SerializeField] Transform farCamPos;
    [SerializeField] float fovChangeSpeed;
    [SerializeField, ReadOnly] private PlayerOnline player;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = OptionsManager.Instance.sensitivity;

        canMove = false;
        waiting = WaitForStart();
        StartCoroutine(waiting);

        myCamera = camParent.GetComponentInChildren<Camera>();
        player = GetComponent<PlayerOnline>();
    }

    void Update()
    {
        if (!IsOwner) return;
        matchIsOver = MultiplayerManager.Instance.MatchOver;
        if (matchIsOver)
            return;
        if (canMove)
        {
            yaw += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            pitch += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, maxLookUpAngle, maxLookDownAngle);
            // pitch = Mathf.Clamp(pitch, -maxTurnAngle - 0.5f, maxTurnAngle + 0.5f);

            camParent.transform.localRotation = Quaternion.Euler(yaw, pitch, 0);
            transform.rotation = Quaternion.Euler(0, pitch, 0);
            // orientation.rotation = Quaternion.Euler(0, myCamera.transform.rotation.eulerAngles.y, 0);

            player.isFocused = canFocus && player.isGrounded && Input.GetKey(key); 
            player.focusInterp = Mathf.Clamp(player.focusInterp + (player.isFocused ? 1 : -1)*Time.deltaTime*fovChangeSpeed, 0, 1);
            
            myCamera.transform.localPosition = Vector3.Lerp(farCamPos.localPosition, nearCamPos.localPosition, player.focusInterp);

            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivity * 0.011f, 0);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float gizmoDist = 1;
        float gizmoThickness = 8;

        Transform[] transforms = { transform, orientation };
        int i = 1;
        foreach (Transform trans in transforms)
        {
            Vector3 pos = trans.position + Vector3.up;
            Handles.DrawBezier(
                pos,
                pos + trans.forward * gizmoDist,
                pos,
                pos + trans.forward * gizmoDist,
                Color.blue * i,
                null,
                gizmoThickness
            );
            Handles.DrawBezier(
                pos,
                pos + trans.up * gizmoDist,
                pos,
                pos + trans.up * gizmoDist,
                Color.green * i,
                null,
                gizmoThickness
            );
            Handles.DrawBezier(
                pos,
                pos + trans.right * gizmoDist,
                pos,
                pos + trans.right * gizmoDist,
                Color.red * i,
                null,
                gizmoThickness
            );
            i++;
        }
    }
#endif

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        pitch = transform.eulerAngles.y;
        canMove = true;
    }
}
