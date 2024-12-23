using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MovementOnline : NetworkBehaviour
{
    float horiInput;
    float vertInput;
    Vector3 moveDir;
    Rigidbody rb;

    [SerializeField] Transform orientation;
    Transform spawnPoint;

    [Header("Movement")]
    public float maxSpeed;
    float speed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float extraGravityForce;
    public float focusSpeedDiv = 1;
    [ReadOnly] public float focusInterp;
    bool canJump;
    public bool focused;

    [Header("Ground check")]
    public float playerHeight;
    public LayerMask groundLayer;
    [SerializeField] Vector3 boxCastDimensions = Vector3.zero;
    [SerializeField] Transform groundCheckPos;
    Collider[] gCol = new Collider[10];
    [Header("Players")]
    public PlayerOnline player;
    [Header("Animator")]
    [SerializeField] private Animator animator;
    private int inputxHash;
    private int inputYHash;
    private int boolGround;
    private int inputJump;
    private int inputWeapon;
    private int inputReload;
    private int inputGunId;

    [Header("Debug")]
    public bool canMove = true;
    private bool fixer = false;
    private bool matchOver = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();
            canJump = true;
            // transform.position = MultiplayerManager.Instance.defaultPos.position;
            speed = maxSpeed;
        }

        base.OnNetworkSpawn();
    }
    private void Start()
    {
        if(animator == null && player.animator  != null){
            animator = player.animator;
            inputxHash = Animator.StringToHash("X");
            inputYHash = Animator.StringToHash("Y");
            boolGround = Animator.StringToHash("isGround");
            inputJump = Animator.StringToHash("jump");
            inputWeapon = Animator.StringToHash("Weapon");
            inputReload = Animator.StringToHash("Reload");
            inputGunId = Animator.StringToHash("GunID");
        }
    }

    void Update()
    {
        //Apenas o dono pode movimentar o player
        if (!IsOwner) return;

        CheckGround();

        speed = Mathf.Lerp(maxSpeed, maxSpeed / focusSpeedDiv, player.focusInterp);
        SpeedClamp();
        rb.drag = player.isGrounded ? groundDrag : 0;
        if (transform.position.y < -1 && !fixer)
        {
            fixer = true;
            // transform.position = spawnPoint.position;
            return;
        }

        if (!canMove || MultiplayerManager.Instance.MatchOver)
        {
            animator.SetFloat(inputxHash, 0);
            animator.SetFloat(inputYHash, 0);
            return;
        }

        InputUpdate();
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            matchOver = MultiplayerManager.Instance.MatchOver;
            animator.SetBool(boolGround, player.isGrounded);
            if (!player.isGrounded)
                rb.AddForce(-transform.up * extraGravityForce, ForceMode.Force);

            if (!canMove || MultiplayerManager.Instance.MatchOver)
                return;
            MovePlayer();

        }


    }

    void InputUpdate()
    {
        if (IsOwner)
        {
            horiInput = Input.GetAxisRaw("Horizontal");
            vertInput = Input.GetAxisRaw("Vertical");

            animator.SetFloat(inputxHash, horiInput);
            animator.SetFloat(inputYHash, vertInput);

            if (Input.GetKeyDown(KeyCode.Space) && canJump && player.isGrounded && !player.isFocused)
            {
                canJump = false;
                Jump();
                animator.SetTrigger(inputJump);
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
    }

    void MovePlayer()
    {
        if (!IsOwner) return;
        moveDir = orientation.forward * vertInput + orientation.right * horiInput;
        if (player.isGrounded)
            rb.AddForce(moveDir.normalized * speed, ForceMode.VelocityChange);
        else
            rb.AddForce(moveDir.normalized * speed * airMultiplier, ForceMode.VelocityChange);
    }

    void SpeedClamp()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > speed)
        {
            Vector3 limit = flatVel.normalized * speed;
            rb.velocity = new Vector3(limit.x, rb.velocity.y, limit.z);
        }
    }

    void Jump()
    {
        rb.drag = 0;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    void ResetJump()
    {
        canJump = true;
    }
    public void SetSpawn(Transform transformSpawn)
    {
        spawnPoint = transformSpawn;
    }
    void CheckGround()
    {
        int numCol = Physics.OverlapBoxNonAlloc(groundCheckPos.position, boxCastDimensions, gCol);
        bool found = false;
        for (int i = 0; i < numCol; i++)
        {
            if (gCol[i].gameObject.layer == 3)
            {
                found = true;
                break;
            }
        }
        player.isGrounded = found && canJump;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawCube(groundCheckPos.position, boxCastDimensions);
    }
    public void ChangeWeapon(int idWeapon)
    {
        animator.SetInteger(inputWeapon, idWeapon);
    }
    public void ReloadWeapon(int idWeapon, bool reload)
    {
        animator.SetInteger(inputGunId, idWeapon);
        animator.SetBool(inputReload, reload);
    }

    public void LoadAnimator(Animator animator){
        this.animator = animator;
        inputxHash = Animator.StringToHash("X");
        inputYHash = Animator.StringToHash("Y");
        boolGround = Animator.StringToHash("isGround");
        inputJump = Animator.StringToHash("jump");
        inputWeapon = Animator.StringToHash("Weapon");
        inputReload = Animator.StringToHash("Reload");
        inputGunId = Animator.StringToHash("GunID");
    }
}
