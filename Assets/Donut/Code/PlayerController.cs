using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Jump & Glide")]
    public float jumpForce = 7f;
    public float glideGravityMultiplier = 0.3f;
    public float maxGlideTime = 2f;

    private bool hasDoubleJumped;
    private bool isGliding;
    private float glideTimer;

    [Header("Camera Flight Control")]
    public float flightVerticalSpeed = 4f;
    public float maxUpAngle = 60f;
    public float maxDownAngle = -45f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Dash")]
    public float dashForce = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    private bool isDashing;
    private float lastDashTime;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckGround();
        GetInput();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
                hasDoubleJumped = false;
            }
            else
            {
                if (isGliding)
                {
                    CancelGlide();     // ✅ กดยกเลิกบิน
                }
                else if (!hasDoubleJumped)
                {
                    StartGlide();     // เริ่มบิน
                }
            }
        }

        HandleGlide();

        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDash())
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Move();
        }
    }

    bool CanDash()
    {
        return !isDashing && Time.time >= lastDashTime + dashCooldown;
    }

    void GetInput()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        moveInput = (camForward * z + camRight * x).normalized;
    }

    void Move()
    {
        Vector3 move = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    void HandleGlide()
    {
        if (!isGliding) return;

        glideTimer -= Time.deltaTime;
        if (glideTimer <= 0)
        {
            isGliding = false;
            return;
        }

        Camera cam = Camera.main;

        // มุมก้มเงยของกล้อง
        float pitch = cam.transform.eulerAngles.x;
        if (pitch > 180) pitch -= 360; // แปลงเป็น -180 ถึง 180

        // จำกัดมุม
        float clampedPitch = Mathf.Clamp(pitch, maxDownAngle, maxUpAngle);

        // แปลงมุม → แรงขึ้นลง
        float verticalInput = -clampedPitch / maxUpAngle;

        Vector3 velocity = rb.linearVelocity;

        // คุมการบินขึ้นลงตามกล้อง
        velocity.y = verticalInput * flightVerticalSpeed;

        // ยังมีแรงร่อนอยู่
        velocity += Vector3.up * Physics.gravity.y * (1 - glideGravityMultiplier) * Time.deltaTime;

        rb.linearVelocity = velocity;
    }
    void StartGlide()
    {
        hasDoubleJumped = true;
        isGliding = true;
        glideTimer = maxGlideTime;
    }
    void CancelGlide()
    {
        isGliding = false;
        glideTimer = 0f;
    }
    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector3 dashDir = moveInput;

        if (dashDir == Vector3.zero)
        {
            dashDir = Camera.main.transform.forward;
            dashDir.y = 0;
            dashDir.Normalize();
        }

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            isGliding = false;
            hasDoubleJumped = false;
        }
    }
}
