using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


// ReSharper disable Unity.InefficientPropertyAccess

public class TPSCamera : MonoBehaviour
{
    float pitch;
    float yaw;
    float angleDiff;
    Transform camTrans;
    IEnumerator waiting;
    PlayerStats stats;
    Camera cam;
    bool fullFocus;
    Vignette vignette;

    [SerializeField]
    GameObject camParent;

    [SerializeField] 
    GunSwapper gunSwapper;

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
    public bool canFocus = true;
    private bool matchIsOver;

    [SerializeField] Transform nearCamPos;
    [SerializeField] Transform farCamPos;
    [SerializeField] Transform scopeCamPos;
    Transform zoomTarget;
    [SerializeField] float fovChangeSpeed;
    [SerializeField] private Animator animator;
    private int inputAimHash = Animator.StringToHash("aim");
    private float yCam = 0f;

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
        
        stats = GetComponent<PlayerStats>();
        cam = camParent.GetComponentInChildren<Camera>();
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
        matchIsOver = (GameManager.Instance != null)? GameManager.Instance.MatchOver : MultiplayerManager.Instance.MatchOver;
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
            
            stats.focused = canFocus && stats.grounded && Input.GetKey(teclaDeFoco);
            stats.focusInterp = Mathf.Clamp(stats.focusInterp + (stats.focused ? 1 : -1)*Time.deltaTime*fovChangeSpeed, 0, 1);

            zoomTarget = stats.carryingScopedGun ? scopeCamPos : nearCamPos;
            cam.transform.localPosition =
                Vector3.Lerp(farCamPos.localPosition, zoomTarget.localPosition, stats.focusInterp);
        }
    }

    void FixedUpdate()
    {
        if (stats.focusInterp >= 0.9f && !fullFocus && stats.carryingScopedGun)
        {
            fullFocus = true;
            vignette.intensity.value = 0.4f;
            gunSwapper.currGun.SetActive(false);
        }

        if ((!stats.carryingScopedGun && fullFocus) || (stats.focusInterp < 0.9f && fullFocus))
        {
            fullFocus = false;
            vignette.intensity.value = 0.2f;
            gunSwapper.currGun.SetActive(true);
        }
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(0.5f);
        pitch = transform.eulerAngles.y;
        if(!EventManager.Instance.cameraLockOnStart)
            canMove = true;
    }
}
