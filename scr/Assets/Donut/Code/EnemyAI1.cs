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
    public float attackCooldown = 3f;
    public float attackStandTime = 1.0f;   // เวลายืนนิ่งหลังโจมตี
    private float lastAttackTime = -10f;
    private bool isPerformingAction = false;

    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("State Machine")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Components")]
    private NavMeshAgent agent;
    private Rigidbody rb;
    public Animator anim;
    private Transform player;

    public bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;

        // ให้ NavMeshAgent คำนวณทางอย่างเดียว ไม่ต้องคุมตัวละครเอง
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // ซิงค์ตำแหน่ง Agent เข้ากับ Rigidbody ตลอดเวลา
        if (agent.isActiveAndEnabled) agent.nextPosition = transform.position;

        // ถ้ากำลังติด Knockback หรือทำ Action โจมตีอยู่ ไม่ต้องคำนวณ State อื่น
        if (isPerformingAction || currentState == EnemyState.Knockback) return;

        // --- Logic การเปลี่ยนสถานะ ---
        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackSequence());
        }
        else if (dist <= detectionRange)
        {
            // ถ้าไม่อยู่ในระยะโจมตี และไม่ได้กำลังถอย ให้ไล่ตาม
            if (currentState != EnemyState.Retreat)
            {
                currentState = EnemyState.Chase;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            currentState = EnemyState.Idle;
        }

        // กรณีพิเศษ: ถ้ากำลังถอย ให้จัดการผ่านฟังก์ชันเฉพาะ
        if (currentState == EnemyState.Retreat)
        {
            HandleRetreat(dist);
        }

        UpdateAnimation();
    }

    IEnumerator AttackSequence()
    {
        isPerformingAction = true;
        currentState = EnemyState.Attack;

        // 1. หยุดนิ่ง
        if (agent.isActiveAndEnabled) agent.isStopped = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        // 2. สั่งอนิเมชั่น
        if (anim != null) anim.SetTrigger("doAttack");

        // 3. ยืนนิ่งตามเวลาที่กำหนด
        yield return new WaitForSeconds(attackStandTime);

        // 4. เข้าสู่สถานะถอยหลังโจมตีเสร็จ
        lastAttackTime = Time.time;
        if (agent.isActiveAndEnabled) agent.isStopped = false;

        currentState = EnemyState.Retreat;
        isPerformingAction = false;
    }

    private void HandleRetreat(float dist)
    {
        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        if (dirFromPlayer == Vector3.zero) dirFromPlayer = -transform.forward;

        Vector3 retreatPos = player.position + (dirFromPlayer * retreatDistance);
        agent.SetDestination(retreatPos);

        // ถ้าถอยมาไกลพอแล้ว ให้กลับไป Idle เพื่อรอ Chase ใหม่ตาม Cooldown
        if (dist >= retreatDistance - stopRetreatRange)
        {
            currentState = EnemyState.Idle;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isPerformingAction || currentState == EnemyState.Knockback) return;

        // เคลื่อนที่ด้วย Force เฉพาะตอน Chase หรือ Retreat
        bool canMove = (currentState == EnemyState.Chase || currentState == EnemyState.Retreat);
        if (!canMove || !agent.hasPath) return;

        Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
        targetDir.y = 0;

        // ใส่แรงผลัก
        rb.AddForce(targetDir * moveForce, ForceMode.Force);

        // ควบคุมความเร็วสูงสุด (Speed Limit)
        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            rb.linearVelocity = hVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        // หมุนหน้าไปทางที่จะเดิน
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    public void StartManualKnockback(Vector3 dir, float force)
    {
        if (isDead) return;

        StopAllCoroutines(); // หยุดการโจมตีหรือการถอยชั่วคราว
        isPerformingAction = false;
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    IEnumerator KnockbackRoutine(Vector3 dir, float force)
    {
        currentState = EnemyState.Knockback;
        if (anim != null) anim.SetTrigger("isHit");

        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f); // ระยะเวลาที่เสียหลัก

        // รอจนกว่าความเร็วจะนิ่งพอ
        while (rb.linearVelocity.magnitude > 0.5f) yield return null;

        agent.enabled = true;
        currentState = EnemyState.Retreat; // หลังโดนตี ให้พยายามถอยตั้งหลักก่อน
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        if (agent != null) agent.enabled = false;
        if (anim != null) anim.SetBool("isDead", true);

        rb.isKinematic = true; // หยุดฟิสิกส์ทั้งหมด
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        // วัดความเร็วราบ (X, Z) เพื่อส่งค่าให้ Animator
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        anim.SetBool("isRunning", speed > 0.2f && currentState != EnemyState.Attack);
    }
}