using UnityEngine;

public class TPSMovement : MonoBehaviour
{
    float horiInput;
    float vertInput;
    Vector3 moveDir;
    Rigidbody rb;

    [SerializeField] Transform orientation;
    [SerializeField] TEMP_PlayerStats stats;
    Transform spawnPoint;

    [Header("Movement")]
    public float maxSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float extraGravityForce;
    public float focusSpeedDiv = 1;
    float speed;
    bool canJump;

    [Header("Ground check")]
    [SerializeField] float sphereRadius = 1;
    [SerializeField] Transform groundCheckPos;
    Collider[] gCol = new Collider[10];

    [Header("Debug")]
    public bool canMove = true;
    bool fixer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canJump = true;
        speed = maxSpeed;
    }

    void Update()
    {
        CheckGround();
        
        speed = Mathf.Lerp(maxSpeed, maxSpeed / focusSpeedDiv, stats.focusInterp);
        SpeedClamp();
        rb.drag = stats.grounded ? groundDrag : 0;
        if (transform.position.y < -1 && !fixer)
        {
            fixer = true;
            transform.position = spawnPoint.position;
            return;
        }

        if (!canMove || GameManager.Instance.MatchOver)
            return;
        
        InputUpdate();
    }

    void FixedUpdate()
    {
        if (!stats.grounded)
            rb.AddForce(-transform.up * extraGravityForce, ForceMode.Force);

        if (!canMove || GameManager.Instance.MatchOver)
            return;
        MovePlayer();
    }

    void InputUpdate()
    {
        horiInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && canJump && stats.grounded && !stats.focused)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
        moveDir = orientation.forward * vertInput + orientation.right * horiInput;
        if (stats.grounded)
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
        stats.grounded = found && canJump;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheckPos.position, sphereRadius/2);
    }
}
