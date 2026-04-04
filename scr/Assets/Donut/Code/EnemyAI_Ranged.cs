using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic; // จำเป็นสำหรับการใช้ List

public class EnemyAI_Ranged : MonoBehaviour
{
    [Header("Distance Settings")]
    public float detectionRange = 20f;
    public float stopDistance = 12f;
    public float retreatDistance = 7f;

    [Header("Timer Settings")]
    public float attackCooldown = 2f;
    private float lastAttackTime = -10f;
    private bool isPerformingAction = false;

    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 4f;
    public float rotationSpeed = 10f;

    // --- ส่วนของ Knockback ที่เพิ่มเข้ามา ---
    [Header("Knockback Settings")]
    public float knockbackDuration = 0.5f;
    public float knockbackThreshold = 0.5f;
    public EnemyState postKnockbackState = EnemyState.Retreat; // หลังโดนตีให้ถอยตั้งหลัก
    public bool useHitAnimation = true;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip alertSFX;
    public AudioClip shootSFX;
    public AudioClip hitSFX;
    public AudioClip deathSFX;
    public AudioClip footstepSFX;
    public float footstepInterval = 0.4f; // เพิ่มระยะห่างเสียงเท้า
    private float nextFootstepTime;
    private bool hasAlerted = false;

    [Header("Components")]
    public EnemyState currentState = EnemyState.Idle;
    private NavMeshAgent agent;
    private Rigidbody rb;
    public Animator anim;
    public Transform player;
    private EnemyRangedAttack attackScript;

    public bool isDead = false;

    // ==========================================
    // --- ระบบ Visual Effects (นำมาจาก Player) ---
    // ==========================================
    [System.Serializable]
    public class EffectSlot
    {
        public string effectName;
        public GameObject vfxPrefab;
        public Transform spawnPoint;
        public bool attachToEnemy = true;

        [Header("Settings")]
        public bool isLooping = false;
        public bool autoDestroy = true;
        public float duration = 2f;

        [HideInInspector] public GameObject spawnedInstance;
    }

    [Header("Visual Effects")]
    public List<EffectSlot> effectsList;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        attackScript = GetComponent<EnemyRangedAttack>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (agent != null) { agent.updatePosition = false; agent.updateRotation = false; }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Health hp = GetComponent<Health>();
        if (hp != null) hp.onDeath += Die;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (agent.isActiveAndEnabled) agent.nextPosition = transform.position;

        // ถ้าโดน Knockback หรือกำลังทำ Action สำคัญ ให้หยุดรอ
        if (isPerformingAction || currentState == EnemyState.Knockback) return;

        if (dist <= stopDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackSequence());
            return;
        }

        if (dist < retreatDistance)
        {
            currentState = EnemyState.Retreat;
        }
        else if (dist <= detectionRange && dist > stopDistance)
        {
            if (!hasAlerted)
            {
                PlaySound(alertSFX);
                PlayEffect(0); // <--- [VFX Index 0] เรียกเอฟเฟคตอนตกใจ/เจอผู้เล่น
                hasAlerted = true;
            }
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
            hasAlerted = false;
        }

        if (currentState != EnemyState.Attack) LookAtPlayer();
        UpdateAnimation();
        HandleFootsteps();
    }

    // --- ฟังก์ชัน StartManualKnockback สำหรับเรียกใช้ภายนอก ---
    public void StartManualKnockback(Vector3 dir, float force)
    {
        if (isDead) return;

        StopAllCoroutines(); // หยุดการยิงทันทีถ้าโดนตีก่อน
        isPerformingAction = false;
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    IEnumerator KnockbackRoutine(Vector3 dir, float force)
    {
        currentState = EnemyState.Knockback;
        PlaySound(hitSFX); // เล่นเสียงเจ็บ
        PlayEffect(2); // <--- [VFX Index 2] เรียกเอฟเฟคเลือดสาด หรือประกายไฟตอนโดนตี

        if (useHitAnimation && anim != null) anim.SetBool("isHit", true);

        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        // รอจนกว่าจะหยุดกระเด็น
        while (rb.linearVelocity.magnitude > knockbackThreshold)
        {
            yield return null;
        }

        if (useHitAnimation && anim != null) anim.SetBool("isHit", false);

        agent.enabled = true;
        currentState = postKnockbackState; // โดยปกติศัตรูยิงจะถอยหลัง (Retreat) เพื่อรักษาระยะ
    }

    IEnumerator AttackSequence()
    {
        isPerformingAction = true;
        currentState = EnemyState.Attack;

        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        if (anim != null)
        {
            anim.SetTrigger("doAttack");
            yield return null;

            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            bool hasShot = false;

            while (stateInfo.normalizedTime < 1.0f)
            {
                // ถ้าโดน Knockback กลางคัน ให้หยุด Coroutine นี้ทันที
                if (currentState == EnemyState.Knockback) yield break;

                stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (!hasShot && stateInfo.normalizedTime >= 0.3f)
                {
                    if (attackScript != null && !isDead)
                    {
                        attackScript.ShootProjectile();
                        PlaySound(shootSFX);
                        PlayEffect(1); // <--- [VFX Index 1] เรียกเอฟเฟคประกายไฟตอนยิงกระสุน
                    }
                    hasShot = true;
                }
                yield return null;
            }
        }
        else
        {
            if (attackScript != null)
            {
                attackScript.ShootProjectile();
                PlaySound(shootSFX);
                PlayEffect(1); // <--- [VFX Index 1] กรณีไม่มี Animator
            }
            yield return new WaitForSeconds(0.5f);
        }

        lastAttackTime = Time.time;
        isPerformingAction = false;
        if (agent != null && agent.isActiveAndEnabled) agent.isStopped = false;
    }

    // ==========================================
    // --- ฟังก์ชันจัดการ Visual Effects ---
    // ==========================================
    public void PlayEffect(int index)
    {
        if (index < 0 || index >= effectsList.Count) return;

        EffectSlot slot = effectsList[index];
        if (slot.vfxPrefab == null) return;

        if (slot.isLooping)
        {
            if (slot.spawnedInstance != null)
            {
                Destroy(slot.spawnedInstance);
                slot.spawnedInstance = null;
            }
            else
            {
                SpawnEffectInstance(slot);
            }
        }
        else
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
        Vector3 pos = slot.spawnPoint != null ? slot.spawnPoint.position : transform.position;
        Quaternion rot = slot.spawnPoint != null ? slot.spawnPoint.rotation : transform.rotation;

        Transform parentObj = null;
        if (slot.attachToEnemy)
        {
            parentObj = slot.spawnPoint != null ? slot.spawnPoint : transform;
        }

        GameObject vfx = Instantiate(slot.vfxPrefab, pos, rot, parentObj);
        slot.spawnedInstance = vfx;

        return vfx;
    }

    // ... (ส่วนที่เหลือ: LookAtPlayer, UpdateAnimation, FixedUpdate, Die, PlaySound, HandleFootsteps) ...
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

    private void UpdateAnimation()
    {
        if (anim == null) return;
        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        anim.SetBool("isRunning", speed > 0.2f);
    }

    void FixedUpdate()
    {
        if (isDead || isPerformingAction || currentState == EnemyState.Idle || currentState == EnemyState.Attack || currentState == EnemyState.Knockback) return;
        Vector3 targetPos = (currentState == EnemyState.Chase) ? player.position : transform.position + (transform.position - player.position).normalized * 5f;
        agent.SetDestination(targetPos);
        if (agent.hasPath)
        {
            Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
            targetDir.y = 0;
            rb.AddForce(targetDir * moveForce, ForceMode.Force);
            Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (hVel.magnitude > maxSpeed) rb.linearVelocity = hVel.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        if (audioSource != null) audioSource.Stop(); // หยุดเสียงเท้า/เสียงอื่นๆ ทันที
        PlaySound(deathSFX);

        PlayEffect(3);

        if (agent != null) agent.enabled = false;
        if (anim != null) anim.SetBool("isDead", true);
        if (rb != null) rb.isKinematic = true;
        this.enabled = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        // ป้องกันการเล่นเสียงซ้ำในเฟรมเดียวกัน (เช่น โดนดาเมจหลายครั้งพร้อมกัน)
        if (clip == hitSFX && audioSource.isPlaying && audioSource.clip == hitSFX) return;

        audioSource.PlayOneShot(clip);
    }
    private void HandleFootsteps()
    {
        // คำนวณความเร็วแนวราบ
        float speed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;

        // เล่นเสียงเท้าตามระยะเวลา (Interval) แทนการเช็ค isPlaying อย่างเดียว
        if (speed > 0.5f && Time.time >= nextFootstepTime && footstepSFX != null)
        {
            // ใช้ PlayOneShot เพื่อไม่ให้กวนเสียงอื่น และกำหนด Volume เล็กลงได้
            audioSource.PlayOneShot(footstepSFX, 0.5f);
            nextFootstepTime = Time.time + footstepInterval;
        }
    }
}