using System.Collections;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public float rotateSpeed = 10f;

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

    public Animator animatorPlayer;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;

    [Header("Weapon System")]
    public GameObject currentWeaponObject; // อาวุธที่ติดตั้งอยู่ปัจจุบัน
    private IWeapon currentWeaponInterface;
    public WeaponManager weaponSwitcher;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (animatorPlayer != null)
        {
            animatorPlayer = GetComponent<Animator>();
        }
        Cursor.lockState = CursorLockMode.Locked; // ล็อกเมาส์ไว้กึ่งกลางจอ
        Cursor.visible = false;                   // ซ่อนรูปเคอร์เซอร์
    }

    void Update()
    {
        CheckGround();
        GetInput();
        RotatePlayerToCamera();

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

        if (Input.GetButton("Fire1"))
        {
            TryUseWeapon();
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

        Vector3 velocity = rb.linearVelocity;

        if (Input.GetKey(KeyCode.Space))
        {
            // ✅ กดค้าง = ลอยขึ้น
            velocity.y = flightVerticalSpeed;
        }
        else
        {
            // ✅ ปล่อย = ค่อย ๆ ร่อนลง
            velocity.y += Physics.gravity.y * glideGravityMultiplier * Time.deltaTime;
        }

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
    void TryUseWeapon()
    {
        if (weaponSwitcher != null && weaponSwitcher.currentWeapon != null)
        {
            IWeapon weapon = weaponSwitcher.currentWeapon.GetComponent<IWeapon>();
            if (weapon != null)
            {
                Debug.Log("สั่งโจมตีไปที่: " + weaponSwitcher.currentWeapon.name);
                weapon.Attack();
                if (animatorPlayer != null)
                {
                    animatorPlayer.SetTrigger("attack");
                }
            }
            else
            {
                Debug.LogError("อาวุธที่ถืออยู่ไม่มี Script ที่เป็น IWeapon!");
            }
        }
        else
        {
            Debug.LogWarning("ไม่มีอาวุธติดตั้งอยู่ หรือลืมลาก WeaponManager ใส่ Player");
        }
    }
    void TryAttack()
    {
        // เช็คเงื่อนไขพิเศษ เช่น ห้ามยิงขณะ Dash
        if (isDashing) return;

        // หา Component อาวุธในวัตถุที่ถืออยู่
        currentWeaponInterface = currentWeaponObject.GetComponent<IWeapon>();

        if (currentWeaponInterface != null)
        {
            currentWeaponInterface.Attack();
        }
    }

    // ===== MELEE LUNGE =====
    public void LungeForward(float distance, float duration)
    {
        StartCoroutine(LungeRoutine(distance, duration));
    }

    IEnumerator LungeRoutine(float distance, float duration)
    {
        float timer = 0f;
        Vector3 dir = transform.forward;

        // ปิดการควบคุมเดินชั่วคราว
        bool prevDashState = isDashing;
        isDashing = true;

        // ล้างแรงเดิม
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        while (timer < duration)
        {
            // ตั้งความเร็วตรง ๆ จะชัวร์กว่า AddForce
            float speed = distance / duration;
            rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = prevDashState;
    }
    void RotatePlayerToCamera()
    {
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f; // ❗ ตัดการเงย/ก้ม
        camForward.Normalize();

        if (camForward.magnitude < 0.1f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}
