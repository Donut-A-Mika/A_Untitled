using UnityEngine;

public class FlyingBoss : MonoBehaviour
{
    [Header("Movement (Area & Speed)")]
    public Transform anchorObject;    // จุดศูนย์กลางการบิน (ถ้าว่างจะใช้จุดเริ่มของตัวเอง)
    public float roamRadius = 10f;     // รัศมีบินวน
    public float minHeight = 3f;      // สูงต่ำสุด
    public float maxHeight = 8f;      // สูงสูงสุด
    public float moveSpeed = 4f;      // ความเร็วการบิน

    [Header("Detection & Shooting")]
    public Transform player;
    public float detectionRange = 18f; // ระยะที่เริ่มเห็นผู้เล่น
    public float attackRange = 12f;    // ระยะที่จะเริ่มกดยิง
    public float fireRate = 2f;        // ยิงทุกๆ X วินาที

    [Header("References")]
    private EnemyRangedAttack rangedAttack; // เชื่อมกับสคริปต์ยิงที่คุณให้มา

    private Vector3 targetPosition;
    private float nextActionTime;
    private float fireTimer;
    private Vector3 centerPoint;

    void Start()
    {
        // ดึงสคริปต์การยิงมาไว้ใช้งาน
        rangedAttack = GetComponent<EnemyRangedAttack>();

        centerPoint = (anchorObject != null) ? anchorObject.position : transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        SetNewRandomTarget();
    }

    void Update()
    {
        HandleMovement();

        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            LookAtPlayer();
            HandleCombat();
        }
    }

    void HandleMovement()
    {
        // บินไปที่จุดหมายแบบ Linear
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // ถ้าถึงจุดหมาย หรือบินวนอยู่นานเกินไป ให้สุ่มจุดใหม่
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f || Time.time > nextActionTime)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        if (anchorObject != null) centerPoint = anchorObject.position;

        // สุ่มตำแหน่งในวงกลมรอบจุด Anchor + สุ่มความสูง
        Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
        float randomY = Random.Range(minHeight, maxHeight);

        targetPosition = new Vector3(centerPoint.x + randomCircle.x, centerPoint.y + randomY, centerPoint.z + randomCircle.y);

        // สุ่มเวลาที่จะเปลี่ยนจุดครั้งถัดไป (3-6 วินาที)
        nextActionTime = Time.time + Random.Range(3f, 6f);
    }

    void LookAtPlayer()
    {
        // หันหน้าไปหาผู้เล่น (แบบ Smooth)
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void HandleCombat()
    {
        // เช็คระยะยิง
        if (Vector3.Distance(transform.position, player.position) > attackRange) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / fireRate)
        {
            // ⭐ เรียกใช้ฟังก์ชันยิงจากสคริปต์ EnemyRangedAttack ของคุณ
            if (rangedAttack != null)
            {
                rangedAttack.ShootProjectile();
            }
            fireTimer = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // วาดขอบเขตการบินให้เห็นใน Scene View
        Gizmos.color = Color.yellow;
        Vector3 cp = (anchorObject != null) ? anchorObject.position : transform.position;
        Gizmos.DrawWireSphere(cp, roamRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}