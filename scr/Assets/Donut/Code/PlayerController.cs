using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip dashSound;
    public AudioClip jumpSound;
    [SerializeField] private float animeX;
    [SerializeField] private float animeY;

    [System.Serializable]
    public class EffectSlot
    {
        public string effectName; // ตั้งชื่อให้เรียกง่าย
        public GameObject vfxPrefab;
        public Transform spawnPoint;
        public bool attachToPlayer = true;

        [Header("Settings")]
        public bool isLooping = false; // ถ้า true จะไม่ลบเอง จนกว่าจะเรียกซ้ำ (Toggle)
        public bool autoDestroy = true; // ถ้า true จะลบ Object ทิ้งเมื่อหมดเวลา (สำหรับ One-shot)
        public float duration = 2f;    // เวลาที่แสดงผล

        [HideInInspector] public GameObject spawnedInstance;
    }

    [Header("Visual Effects")]
    public List<EffectSlot> effectsList;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animatorPlayer == null)
            animatorPlayer = GetComponentInChildren<Animator>();

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

    // --- ส่วนระบบเอฟเฟคที่ปรับปรุงใหม่ (Toggle & Child Object) ---
    public void PlayEffect(int index)
    {
        if (index < 0 || index >= effectsList.Count) return;

        EffectSlot slot = effectsList[index];
        if (slot.vfxPrefab == null) return;

        // ถ้าเป็นแบบ Looping (Toggle เปิด-ปิด)
        if (slot.isLooping)
        {
            if (slot.spawnedInstance != null)
            {
                // ถ้ามีตัวตนอยู่แล้ว (เปิดอยู่) ให้ทำลายทิ้ง (เป็นการปิด)
                Destroy(slot.spawnedInstance);
                slot.spawnedInstance = null;
            }
            else
            {
                // ถ้ายังไม่มี ให้สร้างขึ้นมา
                SpawnEffectInstance(slot);
            }
        }
        else // แบบ One-shot (เกิดแล้วหายไปเอง)
        {
            GameObject vfx = SpawnEffectInstance(slot);
            if (slot.autoDestroy)
            {
                Destroy(vfx, slot.duration);
            }
        }
    }

    private GameObject SpawnEffectInstance(EffectSlot slot)
    {
        // กำหนดตำแหน่งและจุดเกิด
        Vector3 pos = slot.spawnPoint != null ? slot.spawnPoint.position : transform.position;
        Quaternion rot = slot.spawnPoint != null ? slot.spawnPoint.rotation : transform.rotation;

        // กำหนด Parent ถ้าต้องการ Attach (เป็น Obj ลูก)
        Transform parentObj = null;
        if (slot.attachToPlayer)
        {
            parentObj = slot.spawnPoint != null ? slot.spawnPoint : transform;
        }

        // สร้าง Object
        GameObject vfx = Instantiate(slot.vfxPrefab, pos, rot, parentObj);

        // เก็บ Instance ไว้เผื่อการทำ Toggle หรืออ้างอิงภายหลัง
        slot.spawnedInstance = vfx;

        return vfx;
    }

    // --- ฟังก์ชันเดิมคงไว้ทั้งหมด ---
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
        float targetX = 0f;
        float targetY = 0f;

        if (actualVelocity.magnitude > 0.1f)
        {
            targetX = localVel.x > 0.1f ? 1f : (localVel.x < -0.1f ? -1f : 0f);
            targetY = localVel.z > 0.1f ? 1f : (localVel.z < -0.1f ? -1f : 0f);
        }

        float smoothTime = 12f;
        velocityXSmooth = Mathf.Lerp(velocityXSmooth, targetX, Time.deltaTime * smoothTime);
        velocityYSmooth = Mathf.Lerp(velocityYSmooth, targetY, Time.deltaTime * smoothTime);

        animatorPlayer.SetBool("Isjump", !isGrounded);
        animatorPlayer.SetFloat("velocityX", velocityXSmooth);
        animatorPlayer.SetFloat("velocityy", velocityYSmooth);
    }

    public void LungeForward(float distance, float duration)
    {
        StartCoroutine(LungeRoutine(distance, duration));
    }

    IEnumerator LungeRoutine(float distance, float duration)
    {
        float timer = 0f;
        Vector3 dir = transform.forward;
        bool prevDashState = isDashing;
        isDashing = true;
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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        if (audioSource && jumpSound) audioSource.PlayOneShot(jumpSound);
        PlayEffect(0);
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
        if (audioSource && dashSound) audioSource.PlayOneShot(dashSound);
        Vector3 dashDir = moveInput != Vector3.zero ? moveInput : transform.forward;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    public bool IsDashing() => isDashing;
    bool CanDash() => !isDashing && Time.time >= lastDashTime + dashCooldown;

    public float GetDashCooldownNormalized()
    {
        if (isDashing) return 0f;
        float timeElapsed = Time.time - lastDashTime;
        return Mathf.Clamp01(timeElapsed / dashCooldown);
    }

    public float GetDashCooldownRemaining()
    {
        float remaining = (lastDashTime + dashCooldown) - Time.time;
        return Mathf.Max(0, remaining);
    }

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
                    if (animatorPlayer != null) animatorPlayer.SetTrigger("attack");
                }
            }
        }
    }
}