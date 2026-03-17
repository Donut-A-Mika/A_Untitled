using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    public GameObject projectilePrefab; // ใส่ Prefab ที่มีสคริปต์ BulletEnemy
    public Transform firePoint;        // จุดที่กระสุนออกจากปืน
    public float projectileSpeed = 15f;
    public float damage = 10f;          // ค่าพลังโจมตีที่จะส่งให้กระสุน

    public void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // ⭐ เชื่อมโยงกับสคริปต์ BulletEnemy ของคุณ
            BulletEnemy bulletScript = bullet.GetComponent<BulletEnemy>();
            if (bulletScript != null)
            {
                bulletScript.damage = this.damage; // ส่งค่า damage ที่ตั้งจากตัวศัตรูไปให้กระสุน
            }

            Rigidbody rbB = bullet.GetComponent<Rigidbody>();
            if (rbB != null)
            {
                rbB.linearVelocity = firePoint.forward * projectileSpeed;
            }
        }
    }
}