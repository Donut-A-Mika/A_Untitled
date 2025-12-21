using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public float damage = 25f;
    public float attackRange = 2f;
    public LayerMask enemyLayer; // กำหนด Layer ให้ศัตรูเพื่อไม่ให้ตีโดนตัวเอง

    public void Attack()
    {
        // สร้างวงกลมจำลองเพื่อเช็คว่ามีอะไรอยู่ในระยะโจมตีบ้าง
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            // เรียกใช้ระบบเลือดที่เราเขียนไว้ก่อนหน้านี้
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        { // คลิกซ้าย
            Attack();
        }
    }

    // วาดวงกลมใน Editor เพื่อให้เราเห็นระยะโจมตี
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
