using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 15f;
    public float damage = 10f;

    [Header("Spread Settings")]
    [Range(0f, 50f)]
    public float spreadAngle = 5f;      // องศาการกระจาย (0 คือตรงเป๊ะ)
    public int bulletsPerShot = 1;      // จำนวนกระสุนที่ยิงออกมาพร้อมกัน

    public void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // ใช้ Loop เพื่อรองรับการยิงหลายนัดพร้อมกัน (เช่น ลูกซอง)
            for (int i = 0; i < bulletsPerShot; i++)
            {
                // 1. คำนวณการกระจายแบบสุ่ม (Random Spread)
                float scatterX = Random.Range(-spreadAngle, spreadAngle);
                float scatterY = Random.Range(-spreadAngle, spreadAngle);

                // นำค่าสุ่มมาผสมกับ Rotation เดิมของกระบอกปืน
                Quaternion spreadRotation = Quaternion.Euler(scatterX, scatterY, 0);
                Quaternion finalRotation = firePoint.rotation * spreadRotation;

                // 2. สร้างกระสุนด้วย Rotation ที่คำนวณใหม่
                GameObject bullet = Instantiate(projectilePrefab, firePoint.position, finalRotation);

                // ⭐ เชื่อมโยงกับสคริปต์ BulletEnemy (Context เดิม)
                BulletEnemy bulletScript = bullet.GetComponent<BulletEnemy>();
                if (bulletScript != null)
                {
                    bulletScript.damage = this.damage;
                }

                // 3. กำหนดความเร็วตามทิศทางที่กระจายแล้ว
                Rigidbody rbB = bullet.GetComponent<Rigidbody>();
                if (rbB != null)
                {
                    // ยิงออกไปตามแนว Forward ของกระสุนที่หมุนแล้ว
                    rbB.linearVelocity = finalRotation * Vector3.forward * projectileSpeed;
                }
            }
        }
    }
}