using UnityEngine;
using System.Collections;
public class FlyingDiveEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float hoverHeight = 2.5f;
    public float hoverSpeed = 2f;
    public float diveSpeed = 18f;
    public float returnSpeed = 6f;

    [Header("Behavior")]
    public float detectionRange = 15f;
    public float attackCooldown = 3f;

    private Vector3 hoverPoint;
    private bool isDiving = false;
    private bool isReturning = false;
    private float cooldownTimer;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        hoverPoint = player.position + Vector3.up * hoverHeight;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!isDiving && !isReturning && distance <= detectionRange && cooldownTimer <= 0f)
        {
            StartDive();
        }

        if (isDiving)
        {
            DiveToPlayer();
        }
        else if (isReturning)
        {
            ReturnToHover();
        }
        else
        {
            Hover();
        }
    }

    // 🟢 ลอยเหนือหัวผู้เล่น
    void Hover()
    {
        hoverPoint = player.position + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(
            transform.position,
            hoverPoint,
            hoverSpeed * Time.deltaTime
        );
    }

    // 💥 เริ่มโฉบ
    void StartDive()
    {
        isDiving = true;
    }

    // 💥 โฉบเข้าหาผู้เล่น
    void DiveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * diveSpeed * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(dir);

        // 🔁 เมื่อโฉบถึงระดับใกล้พื้น → ให้ EnemyAttack จัดการเอง
        if (Vector3.Distance(transform.position, player.position) < 1.5f)
        {
            EndDive();
        }
    }

    // ⏸️ จบการโจมตี
    void EndDive()
    {
        isDiving = false;
        isReturning = true;
        cooldownTimer = attackCooldown;
    }

    // ⬆️ บินกลับขึ้นไป
    void ReturnToHover()
    {
        Vector3 target = player.position + Vector3.up * hoverHeight;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            returnSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.3f)
        {
            isReturning = false;
        }
    }
}