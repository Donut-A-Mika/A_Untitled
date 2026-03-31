using UnityEngine;
using System.Collections;

public class FlyingBoss : MonoBehaviour
{
    public enum BossState { Normal, Dashing, AoEAttack }
    public BossState currentState = BossState.Normal;

    [Header("Movement Settings (Roaming)")]
    public Transform anchorObject;
    public float roamRadius = 10f;
    public float minHeight = 4f;
    public float moveSpeed = 4f;     // ⭐ ความเร็วตอนบินไปมาปกติ

    [Header("Dash Attack Settings")]
    public float dashSpeed = 22f;    // ⭐ ความเร็วตอนพุ่งโจมตี (แยกอิสระ)
    public float dashTurnSpeed = 15f; // ความเร็วในการหันหน้าตอนพุ่ง

    [Header("Combat Settings (Normal)")]
    public Transform player;
    public float detectionRange = 18f;
    public float attackRange = 12f;
    [Tooltip("จำนวนนัดต่อวินาที")]
    public float fireRate = 2f;

    [Header("Phase Control")]
    public float timeBetweenDash = 12f;
    public float delayAfterExplosion = 2f;

    private EnemyRangedAttack rangedAttack;
    private Vector3 targetPosition;
    private float fireTimer, phaseTimer, nextActionTime;

    [Header("Audio Settings")]
    public AudioSource bossVoiceSource;
    public AudioClip dashWarningSound;
    public AudioClip chargeExplosionSound;

    void Start()
    {
        rangedAttack = GetComponent<EnemyRangedAttack>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetNewRandomTarget();
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Normal: UpdateNormalPhase(); break;
            case BossState.Dashing: UpdateDashPhase(); break;
        }
    }

    void UpdateNormalPhase()
    {
        // ⭐ ใช้ moveSpeed สำหรับการบินวนปกติ
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f || Time.time > nextActionTime)
            SetNewRandomTarget();

        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            LookAt(player.position, 5f);

            fireTimer += Time.deltaTime;
            if (fireTimer >= 1f / fireRate)
            {
                if (Vector3.Distance(transform.position, player.position) <= attackRange)
                {
                    rangedAttack.ShootProjectile();
                }
                fireTimer = 0;
            }
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= timeBetweenDash && player != null) StartDash();
    }

    void StartDash()
    {
        currentState = BossState.Dashing;
        if (bossVoiceSource != null && dashWarningSound != null)
            bossVoiceSource.PlayOneShot(dashWarningSound);

        float anchorY = (anchorObject != null) ? anchorObject.position.y : 0f;
        Vector3 dashTarget = player.position;
        dashTarget.y = Mathf.Max(dashTarget.y, anchorY + minHeight);
        targetPosition = dashTarget;
    }

    void UpdateDashPhase()
    {
        // ⭐ ใช้ dashTurnSpeed เพื่อให้หันหน้าไปหาจุดพุ่งได้ไวขึ้น
        LookAt(targetPosition, dashTurnSpeed);

        // ⭐ ใช้ dashSpeed สำหรับจังหวะพุ่งเข้าใส่เป้าหมายเท่านั้น
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, dashSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        currentState = BossState.AoEAttack;
        if (bossVoiceSource != null && chargeExplosionSound != null)
            bossVoiceSource.PlayOneShot(chargeExplosionSound);

        yield return new WaitForSeconds(0.2f);

        if (rangedAttack != null)
            rangedAttack.PerformAreaExplosion();

        yield return new WaitForSeconds(delayAfterExplosion);

        phaseTimer = 0;
        SetNewRandomTarget();
        currentState = BossState.Normal;
    }

    void LookAt(Vector3 pos, float speed)
    {
        Vector3 dir = (pos - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * speed);
    }

    void SetNewRandomTarget()
    {
        Vector3 center = (anchorObject != null) ? anchorObject.position : transform.position;
        Vector2 circle = Random.insideUnitCircle * roamRadius;
        targetPosition = new Vector3(center.x + circle.x, center.y + Random.Range(minHeight, minHeight + 3f), center.z + circle.y);
        nextActionTime = Time.time + Random.Range(3f, 5f);
    }
}