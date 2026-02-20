using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI1 : MonoBehaviour
{
    [Header("Movement Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public float detectionRange = 10f;

    [Header("Physics Movement")]
    public float moveForce = 20f; // แรงที่ใช้ดันตัวละครเดิน
    public float maxSpeed = 5f;   // ความเร็วสูงสุดเวลาเดินปกติ
    public float rotationSpeed = 10f; // ความเร็วในการหันหน้า

    [Header("Knockback Settings")]
    public float minImpactForceToKnockback = 5f;
    public float chainReactionMultiplier = 0.8f;
    public float exitKnockbackSpeed = 0.5f;
    public LayerMask groundLayer;

    [Header("Cooldown Settings")]
    public float knockbackCooldown = 1.0f;
    private float lastKnockbackTime = -10f;

    public bool isKnockedBack = false;
    private Rigidbody rb;
    private BoxCollider boxCol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();

        if (rb != null)
        {
            // ⭐ เปลี่ยนเป็น false เสมอ เพราะเราจะใช้ Physics ขับเคลื่อน 100%
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (agent != null)
        {
            // ⭐ ปิดไม่ให้ NavMesh ขยับ Transform เอง เราจะใช้มันแค่หาเส้นทาง
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isKnockedBack || player == null || agent == null || !agent.enabled) return;

        // ⭐ บังคับให้ตำแหน่งในใจของ NavMesh ตรงกับตำแหน่งจริงของ Rigidbody เสมอ
        agent.nextPosition = transform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            // กำหนดเป้าหมายให้ NavMesh คำนวณทาง
            agent.SetDestination(player.position);
        }
    }

    // ⭐ ใช้ FixedUpdate สำหรับจัดการ Physics (AddForce)
    void FixedUpdate()
    {
        if (isKnockedBack || player == null || agent == null || !agent.enabled || !agent.hasPath) return;

        // หาจุดเลี้ยวถัดไป (Steering Target) ที่ NavMesh คำนวณมาให้
        Vector3 targetDirection = (agent.steeringTarget - transform.position).normalized;
        targetDirection.y = 0; // ล็อกแกน Y ไม่ให้พยายามมุดดินหรือบิน

        // 1. ใส่แรงผลัก (AddForce) ไปยังทิศทางนั้น
        rb.AddForce(targetDirection * moveForce, ForceMode.Force);

        // 2. จำกัดความเร็ว (Speed Limit) เพื่อไม่ให้ลื่นและพุ่งเร็วเกินไป
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
        }

        // 3. หันหน้าไปในทิศทางที่เดิน (หมุนด้วย Physics)
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastKnockbackTime + knockbackCooldown) return;

        if (other.CompareTag("Bullet"))
        {
            Bullet1 bullet = other.GetComponent<Bullet1>();
            if (bullet != null)
            {
                Vector3 knockbackDir = (other.transform.forward + Vector3.up).normalized;
                StartManualKnockback(knockbackDir, bullet.knockbackForce);
            }
            return;
        }

        Rigidbody otherRb = other.GetComponent<Rigidbody>();
        if (otherRb != null)
        {
            float otherSpeed = otherRb.linearVelocity.magnitude;
            if (otherSpeed >= minImpactForceToKnockback)
            {
                Vector3 horizontalDir = (transform.position - other.transform.position).normalized;
                Vector3 knockbackDir = (horizontalDir + Vector3.up).normalized;

                float finalForce = otherSpeed * chainReactionMultiplier;
                StartManualKnockback(knockbackDir, finalForce);
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

        // ปิด Agent ชั่วคราวไม่ให้กวนการกระเด็น
        if (agent.isActiveAndEnabled) agent.enabled = false;

        // เคลียร์ความเร็วเก่าก่อนกระเด็น
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            bool isLowSpeed = rb.linearVelocity.magnitude <= exitKnockbackSpeed;
            bool isGrounded = CheckIfGrounded();

            if (isLowSpeed && isGrounded)
            {
                break;
            }
            yield return null;
        }

        // เคลียร์ความเร็วให้สนิทเมื่อตกถึงพื้น
        rb.linearVelocity = Vector3.zero;

        if (this != null)
        {
            // อัปเดตตำแหน่งกลับไปที่ Agent ก่อนเปิดใช้งาน
            agent.nextPosition = transform.position;
            agent.enabled = true;
            isKnockedBack = false;
        }
    }

    private bool CheckIfGrounded()
    {
        if (boxCol == null) return true;

        float rayDistance = (boxCol.size.y * transform.lossyScale.y * 0.5f) + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (boxCol != null)
        {
            Gizmos.color = Color.yellow;
            float rayDistance = (boxCol.size.y * transform.lossyScale.y * 0.5f) + 0.1f;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
        }
    }
}