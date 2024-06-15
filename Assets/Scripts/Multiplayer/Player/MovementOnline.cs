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

    [SerializeField]
    Transform orientation;
    Transform spawnPoint;

     [Header("Movement")]
    public float maxSpeed;
    public float speed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float extraGravityForce;
    public float focusSpeedDiv = 1;
    public float focusInterp;
    bool canJump;
    public bool focused;

    [Header("Ground check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;

    [Header("Animator")]
    [SerializeField, ReadOnly] private Animator animator;
    private int inputxHash = Animator.StringToHash("X");
    private int inputYHash = Animator.StringToHash("Y");
    [Header("Ground check")]
    [SerializeField] float sphereRadius = 1;
    [SerializeField] Transform groundCheckPos;
    Collider[] gCol = new Collider[10];

    [Header("Debug")]
    public bool canMove = true;
    private bool fixer = false;
    private bool matchOver = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            canJump = true;
            transform.position = MultiplayerManager.Instance.defaultPos.position; 
        }

        base.OnNetworkSpawn();
    }

    void Start()
    {
        transform.position = MultiplayerManager.Instance.defaultPos.position; 
    }

    void Update()
    {
        //Apenas o dono pode movimentar o player
        if (!IsOwner) return;

        CheckGround(); 

        speed = Mathf.Lerp(maxSpeed, maxSpeed / focusSpeedDiv, focusInterp);
        SpeedClamp();
        rb.drag = grounded ? groundDrag : 0;
        if (transform.position.y < -1 && !fixer)
        {
            fixer = true;
            transform.position = spawnPoint.position;
            return;
        }

        if (!canMove || MultiplayerManager.Instance.MatchOver)
            return;
        
        InputUpdate();
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            matchOver = MultiplayerManager.Instance.MatchOver;

            if (!grounded)
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

            if (Input.GetKeyDown(KeyCode.Space) && canJump && grounded && !focused)
            {
                canJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
    }

    void MovePlayer()
    {
        if (!IsOwner) return;
        moveDir = orientation.forward * vertInput + orientation.right * horiInput;
        if (grounded)
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
        int numCol = Physics.OverlapSphereNonAlloc(groundCheckPos.position, sphereRadius, gCol);
        bool found = false;
        for (int i = 0; i < numCol; i++)
        {
            if (gCol[i].gameObject.layer == 3)
            {
                found = true;
                break;
            }
        }
        grounded = found && canJump;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheckPos.position, sphereRadius/2);
    }
}
