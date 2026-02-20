using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public float explosionForce = 20f;
    public float lifeTime = 5f;
    public GameObject explosionEffect;

    [Header("Detection")]
    public LayerMask targetLayers; // อย่าลืมเลือก Layer ศัตรูใน Inspector

    private bool hasExploded = false; // ป้องกันการระเบิดซ้ำ

    void Start()
    {
        // สั่งให้ระเบิดทำงานเมื่อหมดอายุขัย (ถ้าไม่ชนอะไรเลย)
        Invoke(nameof(Explode), lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // ไม่ระเบิดใส่ Player หรือกระสุนด้วยกัน
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) return;

        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 1. สร้าง Effect (ถ้ามี)
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. ตรวจสอบ Object ในรัศมี
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayers);

        foreach (Collider hit in colliders)
        {
            // ทำความเสียหาย
            Health health = hit.GetComponent<Health>();
            if (health != null) health.TakeDamage(damage);

            // ผลักด้วยฟิสิกส์ (เรียกใช้สคริปต์ EnemyAI1 ที่เราทำไว้)
            EnemyAI1 enemyAI = hit.GetComponent<EnemyAI1>();
            if (enemyAI != null)
            {
                // คำนวณทิศทางจากจุดระเบิด -> ตัวศัตรู
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                direction.y = 0.5f; // ให้กระเด็นเสยขึ้นเล็กน้อย

                enemyAI.StartManualKnockback(direction.normalized, explosionForce);
            }
        }

        // 3. ทำลาย Object กระสุน
        Destroy(gameObject);
    }

    // --- ⭐ DEBUG GIZMOS ---
    // ส่วนนี้จะวาดวงกลมในหน้า Scene เพื่อให้คุณกะระยะระเบิดได้ง่ายๆ
    private void OnDrawGizmos()
    {
        // วาดวงกลมโปร่งแสงสีแดงรอบจุดระเบิด
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);

        // วาดเส้นขอบวงกลมสีแดงเข้ม
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}