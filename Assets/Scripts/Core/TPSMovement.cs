using UnityEngine;
using Unity.Netcode;
public class TPSMovement : NetworkBehaviour
{
    float horiInput;
    float vertInput;
    Vector3 moveDir;
    Rigidbody rb;

    [SerializeField]
    Transform orientation;
    Transform spawnPoint;

    [Header("Movement")]
    public float speed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float extraGravityForce;
    bool canJump;

    [Header("Ground check")]
    public float playerHeight;
    public LayerMask groundLayer;
    bool grounded;

    [Header("Debug")]
    public bool canMove = true;
    private bool fixer = false;
    private bool matchOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canJump = true;
    }

    void Update()
    {
        grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            groundLayer
        );
        SpeedClamp();
        rb.drag = grounded ? groundDrag : 0;
        if (transform.position.y < -1 && !fixer)
        {
            Debug.Log("Teste");
            fixer = true;
            transform.position = spawnPoint.position;
            return;
        }

        if (!canMove || matchOver)
            return;
        InputUpdate();
    }

    void FixedUpdate()
    {
        matchOver = IsServer ? MultiplayerManager.Instance.MatchOver : GameManager.Instance.MatchOver;
        if (!grounded)
            rb.AddForce(-transform.up * extraGravityForce, ForceMode.Force);

        if (!canMove || matchOver)
            return;
        MovePlayer();
    }

    void InputUpdate()
    {
        horiInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && canJump && grounded)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
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
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        canJump = true;
    }
    public void SetSpawn(Transform transformSpawn)
    {
        spawnPoint = transformSpawn;
    }
}
