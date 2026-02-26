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

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animatorPlayer == null)
            animatorPlayer = GetComponentInChildren<Animator>();

        lastPosition = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CheckGround();
        GetInput();
        RotatePlayerToCamera();

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

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void TryUseWeapon()
    {
        if (weaponSwitcher != null && weaponSwitcher.currentWeapon != null)
        {
            // 1. ดึง Component ที่เป็น IWeapon มาเพื่อสั่งโจมตี
            IWeapon weapon = weaponSwitcher.currentWeapon.GetComponent<IWeapon>();

            if (weapon != null)
            {
                // สั่งโจมตี (เรียกใช้ได้ทั้ง Melee และ Ranged)
                weapon.Attack();

                // 2. เช็คประเภทของ Script ที่แนบอยู่กับ Object อาวุธ
                // ถ้าอาวุธนั้นมีสคริปต์ชื่อ MeleeWeapon ให้เล่นอนิเมชั่น "attack"
                if (weaponSwitcher.currentWeapon.GetComponent<MeleeWeapon>() != null)
                {
                    if (animatorPlayer != null)
                    {
                        animatorPlayer.SetTrigger("attack");
                    }
                }
                // ถ้าเป็น RangedWeapon หรืออย่างอื่น (ตามเงื่อนไขของคุณ) จะไม่ทำอะไรต่อ 
                // ทำให้ไม่มีการเรียก SetTrigger("attack") ครับ
            }
        }
    }

    void RotatePlayerToCamera()
    {
        TargetLockSystem lockSystem = GetComponent<TargetLockSystem>();
        if (lockSystem != null && lockSystem.IsLocked) return;

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}