using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState { Idle, Chase, Attack, Retreat, Knockback, Dead }

public class EnemyAI1 : MonoBehaviour
{
    [Header("Distance Settings")]
    public float detectionRange = 15f;
    public float attackRange = 2.2f;
    public float retreatDistance = 8f;
    public float stopRetreatRange = 1f;

    [Header("Timer Settings")]
    public float attackCooldown = 3f;      // เวลาพักก่อนจะเริ่มไล่ใหม่
    public float attackStandTime = 1.0f;   // ⭐ เวลายืนนิ่งเพื่อโจมตี/ค้างท่า ก่อนจะเริ่มถอย
    private float lastAttackTime = -10f;
    private bool isPerformingAction = false; // ล็อคคิวไม่ให้คำนวณซ้อน

    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("State Machine")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Components")]
    public NavMeshAgent agent;
    private Rigidbody rb;
    public Animator anim;
    public Transform player;
    public LayerMask groundLayer;

    private bool isRegistered = false;
    public bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (agent != null) { agent.updatePosition = false; agent.updateRotation = false; }
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (agent.isActiveAndEnabled) agent.nextPosition = transform.position;

        if (!isRegistered && dist <= detectionRange) isRegistered = true;
        if (!isRegistered) return;

        // ระบบป้องกันตัวประชิด (สวนกลับ)
        if (dist <= attackRange && currentState != EnemyState.Attack && currentState != EnemyState.Knockback && !isPerformingAction)
        {
            StartCoroutine(AttackSequence());
            return;
        }

        // ถ้ากำลังทำ Action สำคัญ (ตีหรือรอถอย) ให้หยุด Logic อื่น
        if (isPerformingAction || currentState == EnemyState.Knockback) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (Time.time >= lastAttackTime + attackCooldown) currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                if (dist <= attackRange) StartCoroutine(AttackSequence());
                else agent.SetDestination(player.position);
                break;

            case EnemyState.Retreat:
                HandleRetreat(dist);
                break;
        }

        UpdateAnimation();
    }

    // ⭐ Coroutine จัดลำดับ: โจมตี -> ยืนนิ่ง -> ถอย
    IEnumerator AttackSequence()
    {
        isPerformingAction = true;
        currentState = EnemyState.Attack;

        // 1. หยุดนิ่งและสั่งโจมตี
        agent.isStopped = true;
        rb.linearVelocity = Vector3.zero;
        if (anim != null) anim.SetTrigger("doAttack");

        // 2. ยืนนิ่งค้างไว้ตามเวลาที่กำหนด (เช่น รอให้อนิเมชั่นเล่นถึงจังหวะฟัน)
        yield return new WaitForSeconds(attackStandTime);

        // 3. เริ่มเข้าสู่สถานะถอย
        lastAttackTime = Time.time;
        currentState = EnemyState.Retreat;
        agent.isStopped = false;
        isPerformingAction = false;
    }

    private void HandleRetreat(float dist)
    {
        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        if (dirFromPlayer == Vector3.zero) dirFromPlayer = -transform.forward;

        Vector3 retreatPos = player.position + (dirFromPlayer * retreatDistance);
        agent.SetDestination(retreatPos);

        // ถ้าถอยถึงระยะที่กำหนด ให้เข้าสู่โหมดรอ (Idle)
        if (dist >= retreatDistance - stopRetreatRange)
        {
            currentState = EnemyState.Idle;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void FixedUpdate()
    {
        // จะขยับด้วย Force เฉพาะตอนไล่ (Chase) หรือตอนถอย (Retreat) เท่านั้น
        bool canMove = (currentState == EnemyState.Chase || currentState == EnemyState.Retreat);
        if (isDead || isPerformingAction || !canMove || !agent.hasPath) return;

        Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
        targetDir.y = 0;

        rb.AddForce(targetDir * moveForce, ForceMode.Force);

        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
            rb.linearVelocity = hVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;

        if (targetDir != Vector3.zero)
        {
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.fixedDeltaTime * rotationSpeed));
        }
    }

    public void StartManualKnockback(Vector3 dir, float force)
    {
        if (!isDead)
        {
            StopAllCoroutines();
            isPerformingAction = false;
            StartCoroutine(KnockbackRoutine(dir, force));
        }
    }

    IEnumerator KnockbackRoutine(Vector3 dir, float force)
    {
        currentState = EnemyState.Knockback;
        if (anim != null) anim.SetTrigger("isHit");
        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);
        while (rb.linearVelocity.magnitude > 0.5f) yield return null;

        agent.enabled = true;
        currentState = EnemyState.Retreat; // หลังโดนยิงให้ถอยก่อน
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        if (agent != null) agent.enabled = false;
        if (anim != null) anim.SetBool("isDead", true);
        rb.isKinematic = true;
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        anim.SetBool("isRunning", speed > 0.2f && currentState != EnemyState.Attack);
    }
}