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

    [Header("Retreat Settings")]
    public float retreatTriggerRange = 1.5f;   // ระยะที่ทำให้ถอย
    public float retreatForce = 8f;            // แรงถอย
    public float retreatDuration = 0.3f;       // เวลาถอย
    public float retreatCooldown = 2f;         // คูลดาวน์ถอย

    private float lastRetreatTime = -10f;
    private bool isRetreating = false;


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

        // ✅ คำนวณระยะก่อนใช้
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ===== RETREAT SYSTEM =====
        if (distanceToPlayer <= retreatTriggerRange
            && Time.time >= lastRetreatTime + retreatCooldown
            && !isRetreating
            && !isAttacking
            && !isKnockedBack)
        {
            StartCoroutine(Retreat());
            return;
        }

        // 1. เช็คสถานะการโจมตี
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool isPlayingAttack = stateInfo.IsName("Attack");

        bool attackFinished = !isPlayingAttack ||
            (stateInfo.normalizedTime >= 1.0f && !anim.IsInTransition(0));

        agent.nextPosition = transform.position;

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange && !EnemyAttack.isSomeoneAttacking)
            {
                agent.ResetPath();
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

                if (!EnemyAttack.isSomeoneAttacking)
                {
                   // anim.SetBool("isAttack", true);
                }
            }
            else
            {
                if (attackFinished)
                {
                    //anim.SetBool("isAttack", false);
                    agent.SetDestination(player.position);
                }
            }
        }
        else
        {
            if (attackFinished)
            {
                agent.ResetPath();
                anim.SetBool("isAttack", false);
            }
        }

        if (!isKnockedBack)
        {
            anim.SetBool("isHit", false);
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // เช็คอีกครั้งว่าปัจจุบันกำลังเล่นอนิเมชั่น Attack อยู่หรือไม่
        bool isActuallyAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");

        // วิ่งได้ก็ต่อเมื่อ: มีความเร็ว และ ไม่ได้อยู่ในสถานะโจมตี
        bool shouldRun = horizontalVel.magnitude > 0.1f && !isActuallyAttacking;

        anim.SetBool("isRunning", shouldRun);
        anim.SetBool("isDead", isDead);
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack || isAttacking || isRetreating
    || player == null || agent == null || !agent.enabled || !agent.hasPath)
            return;

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
    IEnumerator Retreat()
    {
        isRetreating = true;
        lastRetreatTime = Time.time;

        if (agent.isActiveAndEnabled)
            agent.ResetPath();

        // หยุดความเร็วเดิม
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        Vector3 retreatDir = (transform.position - player.position).normalized;
        retreatDir.y = 0;

        float timer = 0f;

        while (timer < retreatDuration)
        {
            rb.linearVelocity = new Vector3(
                retreatDir.x * retreatForce,
                rb.linearVelocity.y,
                retreatDir.z * retreatForce
            );

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        isRetreating = false;
    }
    private bool CheckIfGrounded()
    {
        if (boxCol == null) return true;
        float rayDistance = (boxCol.size.y * transform.lossyScale.y * 0.5f) + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);
    }
}