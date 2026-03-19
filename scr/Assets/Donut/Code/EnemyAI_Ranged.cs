using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// ใช้ Enum เดิมเพื่อให้ทำงานร่วมกับระบบอื่นได้
public enum EnemyRangedState { Idle, Chase, Attack, Retreat, Knockback, Dead }

public class EnemyAI_Ranged : MonoBehaviour
{
    [Header("Distance Settings")]
    public float detectionRange = 20f;
    public float stopDistance = 12f;      // ระยะที่ศัตรูจะหยุดยืนยิง
    public float retreatDistance = 7f;     // ระยะที่ศัตรูจะเริ่มวิ่งหนีถ้าผู้เล่นใกล้เกินไป

    [Header("Timer Settings")]
    public float attackCooldown = 2f;
    private float lastAttackTime = -10f;
    private bool isPerformingAction = false;

    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("Components")]
    public EnemyState currentState = EnemyState.Idle;
    private NavMeshAgent agent;
    private Rigidbody rb;
    public Animator anim;
    public Transform player;

    // อ้างอิงสคริปต์ยิง
    private EnemyRangedAttack attackScript;

    public bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        attackScript = GetComponent<EnemyRangedAttack>();

        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (agent != null) { agent.updatePosition = false; agent.updateRotation = false; }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ดึงสคริปต์ Health ที่คุณให้มามาใช้งาน
        Health hp = GetComponent<Health>();
        if (hp != null)
        {
            hp.onDeath += Die; // เมื่อเลือดหมด ให้เรียกฟังก์ชัน Die ในนี้
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (agent.isActiveAndEnabled) agent.nextPosition = transform.position;

        // ถ้ากำลังโจมตีหรือโดน Knockback อยู่ ไม่ต้องคำนวณ State ใหม่
        if (isPerformingAction || currentState == EnemyState.Knockback) return;

        // --- ระบบตัดสินใจ (Decision Making) ---

        // 1. ถ้าอยู่ในระยะยิง และ Cooldown พร้อม => ยิงทันที!
        if (dist <= stopDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackSequence());
            return; // ออกจาก Update เพื่อไปรัน Coroutine โจมตี
        }

        // 2. ถ้าใกล้เกินไป => ถอยหนี (Kiting)
        if (dist < retreatDistance)
        {
            currentState = EnemyState.Retreat;
        }
        // 3. ถ้าอยู่นอกระยะยิงแต่ยังเห็นตัว => ไล่ตาม
        else if (dist <= detectionRange && dist > stopDistance)
        {
            currentState = EnemyState.Chase;
        }
        // 4. นอกนั้น => ยืนนิ่ง
        else
        {
            currentState = EnemyState.Idle;
        }

        if (currentState != EnemyState.Attack) LookAtPlayer();
        UpdateAnimation();
    }

    void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    IEnumerator AttackSequence()
    {
        isPerformingAction = true;
        currentState = EnemyState.Attack;

        // หยุดเคลื่อนที่ทันที
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        if (anim != null)
        {
            anim.SetTrigger("doAttack");

            // รอ 1 เฟรมเพื่อให้ Animator เปลี่ยนสถานะ
            yield return null;

            // ดึงข้อมูลสถานะแอนิเมชัน (ตรวจสอบว่าชื่อ State ใน Animator ตรงกับคำว่า "Attack" หรือไม่)
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            bool hasShot = false;

            // วนลูปจนกว่าแอนิเมชันจะเล่นจบ (NormalizedTime >= 1.0)
            while (stateInfo.normalizedTime < 1.0f)
            {
                stateInfo = anim.GetCurrentAnimatorStateInfo(0);

                // จังหวะปล่อยกระสุน (สมมติว่าปล่อยที่ 30% ของแอนิเมชัน)
                if (!hasShot && stateInfo.normalizedTime >= 0.3f)
                {
                    if (attackScript != null && !isDead)
                    {
                        attackScript.ShootProjectile();
                    }
                    hasShot = true;
                }

                yield return null;
            }
        }
        else
        {
            // กรณีไม่มี Animator ให้ยิงแล้วรอ Cooldown สั้นๆ
            if (attackScript != null) attackScript.ShootProjectile();
            yield return new WaitForSeconds(0.5f);
        }

        lastAttackTime = Time.time;
        isPerformingAction = false; // ปลดล็อกให้ Update กลับมาทำงานต่อได้
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = false;
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        anim.SetBool("isRunning", speed > 0.2f);
    }

    void FixedUpdate()
    {
        if (isDead || isPerformingAction || currentState == EnemyState.Idle || currentState == EnemyState.Attack) return;

        Vector3 targetPos = transform.position;

        if (currentState == EnemyState.Chase)
        {
            targetPos = player.position;
        }
        else if (currentState == EnemyState.Retreat)
        {
            // คำนวณจุดถอยหนีออกจากผู้เล่น
            Vector3 dirFromPlayer = (transform.position - player.position).normalized;
            targetPos = transform.position + dirFromPlayer * 5f;
        }

        agent.SetDestination(targetPos);
        if (agent.hasPath)
        {
            Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
            targetDir.y = 0;
            rb.AddForce(targetDir * moveForce, ForceMode.Force);

            // จำกัดความเร็วสูงสุด
            Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (hVel.magnitude > maxSpeed)
                rb.linearVelocity = hVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        if (agent != null) agent.enabled = false;
        if (anim != null) anim.SetBool("isDead", true);
        if (rb != null) rb.isKinematic = true;

        this.enabled = false; // ปิด AI สคริปต์นี้
    }
}