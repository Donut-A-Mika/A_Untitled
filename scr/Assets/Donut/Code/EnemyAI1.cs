using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI1 : MonoBehaviour
{
    [Header("Movement Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public float detectionRange = 10f;

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
    private BoxCollider boxCol; // ⭐ เปลี่ยนเป็น BoxCollider

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>(); // ⭐ ดึงค่า BoxCollider

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isKnockedBack || player == null || agent == null || !agent.enabled) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            agent.SetDestination(player.position);
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
                // กระเด็นแบบ 45 องศา (Forward + Up)
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
        agent.enabled = false;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);

        // วนลูปเช็คจนกว่าความเร็วจะต่ำและถึงพื้น
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

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (this != null)
        {
            agent.enabled = true;
            isKnockedBack = false;
        }
    }

    private bool CheckIfGrounded()
    {
        if (boxCol == null) return true;

        // ⭐ คำนวณระยะจากจุดศูนย์กลางลงไปที่ขอบล่างของ Box (size.y * lossyScale.y / 2)
        // บวกระยะเผื่อเล็กน้อย 0.1f เพื่อให้เช็คถึงพื้น
        float rayDistance = (boxCol.size.y * transform.lossyScale.y * 0.5f) + 0.1f;

        // ยิง Raycast ลงไปที่พื้น
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