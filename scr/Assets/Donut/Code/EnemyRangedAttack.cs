using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 15f;
    public float damage = 10f;

    [Header("Normal Shot Spread")]
    [Range(0f, 50f)] public float spreadAngle = 5f;
    public int bulletsPerShot = 1;

    [Header("AoE Explosion Settings")]
    public GameObject[] explosionVFXList;
    public Vector3 vfxScale = Vector3.one;
    public float explosionRadius = 7f;
    public float knockbackForce = 25f;
    public LayerMask playerLayer;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip shootSound;      // เสียงเวลายิงปกติ
    public AudioClip explosionSound;  // เสียงเวลา AoE ระเบิด

    public void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float scatterX = Random.Range(-spreadAngle, spreadAngle);
            float scatterY = Random.Range(-spreadAngle, spreadAngle);
            Quaternion finalRotation = firePoint.rotation * Quaternion.Euler(scatterX, scatterY, 0);
            SpawnBullet(finalRotation);
        }
    }

    public void PerformAreaExplosion()
    {
        // 1. เล่นเสียงระเบิด
        if (audioSource != null && explosionSound != null)
            audioSource.PlayOneShot(explosionSound);

        // 2. สร้าง VFX ทั้งหมดและปรับขนาด
        if (explosionVFXList != null)
        {
            foreach (GameObject vfxPrefab in explosionVFXList)
            {
                if (vfxPrefab != null)
                {
                    GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
                    vfx.transform.localScale = vfxScale;
                }
            }
        }

        // 3. ตรวจสอบเป้าหมายในรัศมีระเบิด (เชื่อมต่อกับสคริปต์ Health)
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);
        foreach (Collider target in hitTargets)
        {
            // ⭐ ส่ง Damage ไปยังสคริปต์ Health ของคุณ
            if (target.TryGetComponent(out Health targetHealth))
            {
                targetHealth.TakeDamage(damage); // หรือปรับเป็น damage * 2f ถ้าอยากให้ระเบิดแรงกว่าปกติ
            }

            // ⭐ แรงผลัก (Knockback)
            if (target.TryGetComponent(out Rigidbody rb))
            {
                Vector3 pushDir = (target.transform.position - transform.position).normalized;
                pushDir.y = 0.5f; // ให้กระดอนขึ้นเล็กน้อย
                rb.AddForce(pushDir * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    private void SpawnBullet(Quaternion rotation)
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, rotation);

        // ⭐ ส่งค่า Damage ไปยังสคริปต์ที่ติดอยู่กับกระสุน (ถ้ามี)
        if (bullet.TryGetComponent(out BulletEnemy b))
        {
            b.damage = this.damage;
        }

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = (rotation * Vector3.forward) * projectileSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}