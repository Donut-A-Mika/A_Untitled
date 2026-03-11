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

    private bool isGliding;
    private float glideTimer;

    [Header("Camera Flight Control")]
    public float flightVerticalSpeed = 4f;

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

    [Header("Animation & Weapons")]
    public Animator animatorPlayer;
    public WeaponManager weaponSwitcher;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;

    // --- ตัวแปรสำหรับเช็คตำแหน่ง (Position-based Velocity) ---
    private Vector3 lastPosition;
    private Vector3 actualVelocity;
    private float velocityXSmooth;
    private float velocityYSmooth;

    public TargetLockSystem lockSystem;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animatorPlayer == null)
            animatorPlayer = GetComponentInChildren<Animator>();

        // ⭐ FIX สำคัญ
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastPosition = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CheckGround();
        GetInput();

        CalculatePositionVelocity();
        UpdateAnimations();

        if (Input.GetButtonDown("Fire1")) TryUseWeapon();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) Jump();
        HandleGlide();

        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDash())
            StartCoroutine(Dash());
    }

    void FixedUpdate()
    {
        if (!isDashing) Move();
    }

    void CalculatePositionVelocity()
    {
        Vector3 distanceMoved = transform.position - lastPosition;
        distanceMoved.y = 0;

        if (Time.deltaTime > 0)
            actualVelocity = distanceMoved / Time.deltaTime;

        lastPosition = transform.position;
    }

    void UpdateAnimations()
    {
        if (animatorPlayer == null) return;

        Vector3 localVel = transform.InverseTransformDirection(actualVelocity);

        bool isMoving = actualVelocity.magnitude > 0.1f && isGrounded;
        animatorPlayer.SetBool("Isrun", isMoving);
        animatorPlayer.SetBool("Isjump", !isGrounded);

        float targetX = localVel.x / moveSpeed;
        float targetY = localVel.z / moveSpeed;

        velocityXSmooth = Mathf.Lerp(velocityXSmooth, targetX, Time.deltaTime * 10f);
        velocityYSmooth = Mathf.Lerp(velocityYSmooth, targetY, Time.deltaTime * 10f);

        animatorPlayer.SetFloat("velocityX", velocityXSmooth);
        animatorPlayer.SetFloat("velocityy", velocityYSmooth);
    }

    // ===== [ MELEE LUNGE - ส่วนที่เพิ่มมาเพื่อรองรับ MeleeWeapon ] =====
    public void LungeForward(float distance, float duration)
    {
        StartCoroutine(LungeRoutine(distance, duration));
    }

    IEnumerator LungeRoutine(float distance, float duration)
    {
        float timer = 0f;
        Vector3 dir = transform.forward;

        // ล็อกสถานะ Dashing ชั่วคราวเพื่อให้ FixedUpdate ไม่กวนแรงพุ่ง
        bool prevDashState = isDashing;
        isDashing = true;

        // ล้างแรงเดิมออกก่อนพุ่ง
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        while (timer < duration)
        {
            float speed = distance / duration;
            rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = prevDashState;
    }

    // =============================================================

    void GetInput()
    {
        if (cameraTransform == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        moveInput = (camForward * z + camRight * x).normalized;
    }
    void Move()
    {
        if (cameraTransform == null) return;

        Vector3 move = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        if (lockSystem == null || !lockSystem.IsLocked)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;

            if (camForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void HandleGlide()
    {
        bool holdingFly = Input.GetKey(KeyCode.Space);
        if (holdingFly && !isGrounded)
        {
            if (!isGliding) { isGliding = true; glideTimer = maxGlideTime; }
        }

        if (!isGliding) return;

        glideTimer -= Time.deltaTime;
        Vector3 velocity = rb.linearVelocity;

        if (holdingFly && glideTimer > 0f)
            velocity.y = flightVerticalSpeed;
        else
            velocity.y += Physics.gravity.y * glideGravityMultiplier * Time.deltaTime;

        rb.linearVelocity = velocity;

        if (isGrounded && !holdingFly) isGliding = false;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector3 dashDir = moveInput != Vector3.zero ? moveInput : transform.forward;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    public bool IsDashing() => isDashing;
    bool CanDash() => !isDashing && Time.time >= lastDashTime + dashCooldown;
    // --- ส่วนที่เพิ่มสำหรับ UI Slider ---

    // ส่งค่า 0 ถึง 1 (0 = กำลังคูลดาวน์, 1 = พร้อมใช้) สำหรับใส่ช่อง Slider.value
    public float GetDashCooldownNormalized()
    {
        if (isDashing) return 0f;

        float timeElapsed = Time.time - lastDashTime;
        float progress = timeElapsed / dashCooldown;
        return Mathf.Clamp01(progress);
    }

    // ส่งค่าเวลาที่เหลือเป็นวินาที (เช่น 0.5, 0.4...) เอาไว้โชว์เป็นตัวเลข Text
    public float GetDashCooldownRemaining()
    {
        float remaining = (lastDashTime + dashCooldown) - Time.time;
        return Mathf.Max(0, remaining);
    }

    // เช็คว่า Dash พร้อมใช้งานหรือไม่ (เอาไว้เปลี่ยนสี UI)
    public bool IsDashReady() => CanDash();
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void TryUseWeapon()
    {
        if (weaponSwitcher != null && weaponSwitcher.currentWeapon != null)
        {
            IWeapon weapon = weaponSwitcher.currentWeapon.GetComponent<IWeapon>();

            if (weapon != null)
            {
                weapon.Attack();

                if (weaponSwitcher.currentWeapon.GetComponent<MeleeWeapon>() != null)
                {
                    if (animatorPlayer != null)
                    {
                        animatorPlayer.SetTrigger("attack");
                    }
                }
            }
        }
    }
}