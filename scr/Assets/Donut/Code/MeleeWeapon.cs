using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    public float damage = 25f;
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    // ฟังก์ชันนี้ต้องชื่อตรงกับใน Interface
    public void Attack()
    {
        Debug.Log("ใช้ดาบโจมตี!");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}