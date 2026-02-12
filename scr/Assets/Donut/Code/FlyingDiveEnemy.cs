using System.Collections;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
public class FlyingDiveEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float hoverHeight = 2.5f;
    public float hoverSpeed = 2f;
    public float diveSpeed = 18f;
    public float returnSpeed = 6f;

    [Header("Detection")]
    public float detectionRange = 15f;     // ระยะเริ่มสนใจผู้เล่น
    public float loseRange = 22f;          // ระยะเลิกไล่ (ต้องมากกว่า detection)

    [Header("Behavior")]
    public float attackCooldown = 3f;

    private Vector3 startPoint;
    private Vector3 hoverPoint;

    private bool playerDetected = false;
    private bool isDiving = false;
    private bool isReturning = false;

    private float cooldownTimer;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        startPoint = transform.position;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ✅ ตรวจจับผู้เล่น
        if (!playerDetected && distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
        }

        // ❌ ผู้เล่นหนีไกล → เลิกสนใจ
        if (playerDetected && distanceToPlayer >= loseRange)
        {
            playerDetected = false;
            isDiving = false;
            isReturning = true;
        }

        // 🎯 โหมดพฤติกรรม
        if (!playerDetected)
        {
            IdleHover();
            return;
        }

        if (!isDiving && !isReturning && cooldownTimer <= 0f)
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
            HoverAbovePlayer();
        }
    }

    // 💤 อยู่จุดเดิม
    void IdleHover()
    {
        Vector3 idlePoint = startPoint + Vector3.up * hoverHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            idlePoint,
            hoverSpeed * Time.deltaTime
        );
    }

    // 🟢 ลอยเหนือหัวผู้เล่น
    void HoverAbovePlayer()
    {
        hoverPoint = player.position + Vector3.up * hoverHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            hoverPoint,
            hoverSpeed * Time.deltaTime
        );

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }

    void StartDive()
    {
        isDiving = true;
    }

    void DiveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * diveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        if (Vector3.Distance(transform.position, player.position) < 1.5f)
        {
            EndDive();
        }
    }

    void EndDive()
    {
        isDiving = false;
        isReturning = true;
        cooldownTimer = attackCooldown;
    }

    void ReturnToHover()
    {
        Vector3 target;

        if (playerDetected)
            target = player.position + Vector3.up * hoverHeight;
        else
            target = startPoint + Vector3.up * hoverHeight;

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
