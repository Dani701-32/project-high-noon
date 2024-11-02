using System.Collections;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
public class CameraOnline : NetworkBehaviour
{
    float pitch;
    float yaw;
    Camera myCamera;
    IEnumerator waiting;
    bool fullFocus;
    [SerializeField, ReadOnly] Vignette vignette;
    [SerializeField] private GameObject camParent;
    [SerializeField] private GameObject gunHolder;
    [SerializeField] KeyCode keyAim;

    [Header("Angles")]

    [Tooltip("Ângulo máximo que o jogador pode olhar acima de si")]
    [SerializeField] private float maxLookUpAngle = -55;

    [Tooltip("Ângulo máximo que o jogador pode olhar abaixo de si")]
    [SerializeField] private float maxLookDownAngle = 50;

    [Tooltip("Velocidade de rotação da câmera")]
    public float sensitivity = 1;

    [Header("Debug and temporary values")]
    [Tooltip("O jogador está em jogo ou está ocupado com algo?")] // LEVAR ESSA VARIÁVEL PRO GAME MANAGER MAIS TARDE
    public bool canMove;
    public bool canFocus = true;
    private bool matchIsOver = false;

    [SerializeField] Transform nearCamPos;
    [SerializeField] Transform farCamPos;
    [SerializeField] Transform scopeCamPos;
    private Transform zoomTarget;
    [SerializeField] float fovChangeSpeed;
    private PlayerOnline player;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private int inputAimHash = Animator.StringToHash("aim");
    private float yCam = 0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = OptionsManager.Instance.sensitivity;

        canMove = false;
        waiting = WaitForStart();
        StartCoroutine(waiting);

        player = GetComponent<PlayerOnline>();
        myCamera = camParent.GetComponentInChildren<Camera>();

        if (!vignette)
        {
            Volume vol = GameObject.Find("Global Volume").GetComponent<Volume>();
            if(vol)
                vol.profile.TryGet(out vignette);
            else
                Debug.LogError("Componente de nome 'Global Volume' não pode ser encontrado na cena!");
        }
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
            yCam = (yaw - maxLookDownAngle) / (maxLookUpAngle - maxLookDownAngle) * 2f - 1f;

            animator.SetFloat(inputAimHash, yCam);

            camParent.transform.localRotation = Quaternion.Euler(yaw, 0, 0);
            transform.rotation = Quaternion.Euler(0, pitch, 0);

            player.isFocused = canFocus && player.isGrounded && Input.GetKey(keyAim);
            player.focusInterp = Mathf.Clamp(player.focusInterp + (player.isFocused ? 1 : -1) * Time.deltaTime * fovChangeSpeed, 0, 1);

            zoomTarget = player.scopeGun ? scopeCamPos : nearCamPos;

            myCamera.transform.localPosition = Vector3.Lerp(farCamPos.localPosition, zoomTarget.localPosition, player.focusInterp);
            
        }
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (player.focusInterp >= 0.9f && !fullFocus && player.scopeGun)
        {
            fullFocus = true;
            vignette.intensity.value = 0.4f;
            gunHolder.SetActive(false);
        }

        if (fullFocus && (!player.scopeGun || player.focusInterp < 0.9f))
        {
            fullFocus = false;
            vignette.intensity.value = 0.2f;
            gunHolder.SetActive(true);
        }
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        pitch = transform.eulerAngles.y;
        canMove = true;
    }
}
