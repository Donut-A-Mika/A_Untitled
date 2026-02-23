using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI1 : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent agent;
    private Rigidbody rb;
    private BoxCollider boxCol;
    public Animator anim;

    [Header("Movement Settings")]
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 2f;

    [Header("Physics Movement")]
    public float moveForce = 20f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Knockback Settings")]
    public float minImpactForceToKnockback = 5f;
    public float chainReactionMultiplier = 0.8f;
    public float exitKnockbackSpeed = 0.5f;
    public LayerMask groundLayer;

    [Header("Cooldown Settings")]
    public float knockbackCooldown = 1.0f;
    private float lastKnockbackTime = -10f;

    public bool isKnockedBack = false;
    public bool isDead = false;
    private bool isAttacking = false; // ตัวแปรเช็คว่ากำลังอยู่ในอนิเมชั่นโจมตีหรือไม่

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
       

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || isKnockedBack || player == null || agent == null || !agent.enabled)
        {
            UpdateAnimation();
            return;
        }

        // เช็คว่า Animator กำลังเล่นท่าโจมตีอยู่หรือไม่ (สมมติว่า State โจมตีชื่อ "Attack")
        // ถ้าโจมตีอยู่ ให้หยุดการเคลื่อนที่ทั้งหมด
        isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
                      anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;

        if (isAttacking)
        {
            agent.ResetPath();
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // หยุดตัวทันที
            UpdateAnimation();
            return;
        }

        agent.nextPosition = transform.position;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                // เข้าสู่ระยะโจมตี: สั่งให้หยุดเดินและเริ่มโจมตี
                agent.ResetPath();
                anim.SetBool("isAttack", true);
            }
            else
            {
                // ระยะไล่ตาม: สั่งให้เดินและปิดการโจมตี
                agent.SetDestination(player.position);
                anim.SetBool("isAttack", false);
            }
        }
        else
        {
            agent.ResetPath();
            anim.SetBool("isAttack", false);
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        // เช็คความเร็วจาก Rigidbody 
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // เงื่อนไข: ถ้าความเร็ว > 0 และไม่ได้กำลังโจมตี ให้เล่น isRunning
        bool shouldRun = horizontalVel.magnitude > 0.1f && !isAttacking;

        anim.SetBool("isRunning", shouldRun);
        anim.SetBool("isDead", isDead);
    }

    void FixedUpdate()
    {
        // ถ้าตาย, โดนเด้ง, หรือ "กำลังโจมตีอยู่" ห้ามใส่แรงเดิน
        if (isDead || isKnockedBack || isAttacking || player == null || agent == null || !agent.enabled || !agent.hasPath) return;

        Vector3 targetDirection = (agent.steeringTarget - transform.position).normalized;
        targetDirection.y = 0;

        rb.AddForce(targetDirection * moveForce, ForceMode.Force);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
        }

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    // --- ส่วนที่เหลือ (OnTriggerEnter, ApplyKnockback) คงเดิมเหมือนโค้ดก่อนหน้า ---
    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (Time.time < lastKnockbackTime + knockbackCooldown) return;

        if (other.CompareTag("Bullet"))
        {
            if (anim != null) anim.SetTrigger("isHit");
            Bullet1 bullet = other.GetComponent<Bullet1>();
            if (bullet != null)
            {
                Vector3 knockbackDir = (other.transform.forward + Vector3.up).normalized;
                StartManualKnockback(knockbackDir, bullet.knockbackForce);
            }
        }
    }

    public void StartManualKnockback(Vector3 direction, float force)
    {
        if (Time.time >= lastKnockbackTime + knockbackCooldown && gameObject.activeInHierarchy)
        {
            lastKnockbackTime = Time.time;
            StopAllCoroutines();
            StartCoroutine(ApplyKnockback(direction, force));
        }
    }

    IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        isKnockedBack = true;
        if (anim != null) anim.SetBool("isHit", true);
        if (agent.isActiveAndEnabled) agent.enabled = false;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            bool isLowSpeed = rb.linearVelocity.magnitude <= exitKnockbackSpeed;
            bool isGrounded = CheckIfGrounded();
            if (isLowSpeed && isGrounded) break;
            yield return null;
        }
        rb.linearVelocity = Vector3.zero;
        if (this != null)
        {
            if (anim != null) anim.SetBool("isHit", false);
            agent.nextPosition = transform.position;
            agent.enabled = true;
            isKnockedBack = false;
        }
    }

    private bool CheckIfGrounded()
    {
        if (boxCol == null) return true;
        float rayDistance = (boxCol.size.y * transform.lossyScale.y * 0.5f) + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);
    }
}