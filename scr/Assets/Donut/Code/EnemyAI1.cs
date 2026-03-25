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
    public float attackStandTime = 1.0f;
    private float lastAttackTime = -10f;
    private bool isPerformingAction = false;

    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 10f;

    // --- ส่วนที่เพิ่ม/แก้ไขสำหรับ Knockback ---
    [Header("Knockback Settings")]
    [Tooltip("ระยะเวลาขั้นต่ำที่ตัวละครจะติดสถานะ Knockback (วินาที)")]
    public float knockbackDuration = 0.5f; 
    [Tooltip("ความเร็วที่เหลืออยู่เท่าไหร่ถึงจะยอมให้กลับไปเดินได้ (ยิ่งน้อยยิ่งต้องรอให้นิ่งจริง)")]
    public float knockbackThreshold = 0.5f;
    [Tooltip("หลังจาก Knockback เสร็จ จะให้เปลี่ยนไป State ไหน")]
    public EnemyState postKnockbackState = EnemyState.Retreat;
    public bool useHitAnimation = true;
    // ---------------------------------------

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

        if (agent.isActiveAndEnabled) agent.nextPosition = transform.position;

        if (isPerformingAction || currentState == EnemyState.Knockback) return;

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackSequence());
        }
        else if (dist <= detectionRange)
        {
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

        if (agent.isActiveAndEnabled) agent.isStopped = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        if (anim != null) anim.SetTrigger("doAttack");

        yield return new WaitForSeconds(attackStandTime);

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

        if (dist >= retreatDistance - stopRetreatRange)
        {
            currentState = EnemyState.Idle;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isPerformingAction || currentState == EnemyState.Knockback) return;

        bool canMove = (currentState == EnemyState.Chase || currentState == EnemyState.Retreat);
        if (!canMove || !agent.hasPath) return;

        Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
        targetDir.y = 0;

        rb.AddForce(targetDir * moveForce, ForceMode.Force);

        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (hVel.magnitude > maxSpeed)
        {
            rb.linearVelocity = hVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        if (targetDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    public void StartManualKnockback(Vector3 dir, float force)
    {
        if (isDead) return;

        StopAllCoroutines(); 
        isPerformingAction = false;
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    IEnumerator KnockbackRoutine(Vector3 dir, float force)
    {
        currentState = EnemyState.Knockback;

        // 1. เปิด bool isHit เป็น true เพื่อเริ่มเล่นท่าโดนตี
        if (useHitAnimation && anim != null)
        {
            anim.SetBool("isHit", true);
        }

        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        // รอตามเวลา Stun ที่ตั้งไว้
        yield return new WaitForSeconds(knockbackDuration);

        // รอจนกว่าความเร็วจะนิ่ง (Threshold)
        while (rb.linearVelocity.magnitude > knockbackThreshold)
        {
            yield return null;
        }

        // 2. ปิด bool isHit เป็น false เมื่อฟื้นตัวเสร็จแล้ว
        if (useHitAnimation && anim != null)
        {
            anim.SetBool("isHit", false);
        }

        agent.enabled = true;
        currentState = postKnockbackState;
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