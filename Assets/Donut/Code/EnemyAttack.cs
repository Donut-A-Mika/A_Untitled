using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 10f;          // ความแรงในการโจมตี
    public float attackRange = 1.5f;   // ระยะที่ศัตรูจะเริ่มตี
    public float attackCooldown = 1.0f; // ตีหนึ่งครั้งแล้วต้องรอกี่วินาที

    private float nextAttackTime;
    private Transform player;
    private Health playerHealth;

    void Start()
    {
        // หาตัวผู้เล่นและดึง Component เลือดมาเก็บไว้
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        // เช็คระยะห่างระหว่างศัตรูกับผู้เล่น
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ถ้าใกล้พอ และ ถึงเวลาที่โจมตีได้อีกครั้ง
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        Debug.Log("ศัตรูโจมตีผู้เล่น!");
        playerHealth.TakeDamage(damage);

        // ตรงนี้สามารถใส่โค้ดเล่น Animation โจมตีของศัตรูเพิ่มได้ในอนาคต
    }

    // วาดวงกลมระยะโจมตีในหน้า Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}