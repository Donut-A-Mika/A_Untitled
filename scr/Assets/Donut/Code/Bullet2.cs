using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public float explosionForce = 20f;
    public float lifeTime = 5f;
    public GameObject explosionEffect;

    [Header("Layer Setup")]
    public LayerMask targetLayers;  // เลเยอร์ที่จะโดนดาเมจจากแรงระเบิด
    public LayerMask ignoreLayers;  // เลเยอร์ที่ชนแล้ว "ไม่ระเบิด" (ให้ข้ามไปเลย เช่น Player, Bullet)

    [Header("Audio")]
    public AudioClip explosionSound;
    private bool hasExploded = false;

    // เก็บค่า Normal และ Position เพื่อใช้ตอนระเบิด
    private Vector3 lastHitNormal = Vector3.up;
    private Vector3 lastHitPos;

    void Start()
    {
        lastHitPos = transform.position;
        Invoke(nameof(ExplodeFromTimer), lifeTime);
    }

    // กรณีระเบิดเพราะหมดเวลา (กลางอากาศ)
    void ExplodeFromTimer() => Explode(transform.position, Vector3.up);

    void OnTriggerEnter(Collider other)
    {
        // 1. ถ้าระเบิดไปแล้ว ให้หยุดการทำงานทันที
        if (hasExploded) return;

        // 2. เช็ค Layer ว่าสิ่งที่ชนอยู่ในหมวด ignoreLayers (เลเยอร์ที่ต้องละเว้น) หรือไม่
        // (เราใช้การคำนวณ Bitwise สลับบิตเพื่อเช็คว่า Layer ของสิ่งที่ชน ตรงกับ LayerMask ที่ตั้งไว้ไหม)
        if (((1 << other.gameObject.layer) & ignoreLayers) != 0) return;

        Vector3 hitPos = transform.position;
        Vector3 hitNormal = Vector3.up;

        // --- ใช้ Raycast เพื่อหาค่า Normal ของพื้นผิวที่ชน ---
        Ray ray = new Ray(transform.position - transform.forward, transform.forward);
        if (other.Raycast(ray, out RaycastHit hit, 2f))
        {
            hitPos = hit.point;
            hitNormal = hit.normal;
        }

        Explode(hitPos, hitNormal);
    }

    void Explode(Vector3 position, Vector3 normal)
    {
        if (hasExploded) return;
        hasExploded = true;

        // 1. เล่นเสียง
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, position, 1f);

        // 2. สร้างเอฟเฟกต์ตามทิศทาง Normal (หันหน้าออกจากกำแพง/ศัตรู)
        if (explosionEffect != null)
        {
            Quaternion rot = Quaternion.LookRotation(normal);
            Instantiate(explosionEffect, position, rot);
        }

        // 3. จัดการแรงระเบิดและความเสียหายรอบๆ
        Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius, targetLayers);
        foreach (Collider hit in hitColliders)
        {
            // ทำความเสียหาย
            Health h = hit.GetComponent<Health>();
            if (h != null) h.TakeDamage(damage);

            // ผลัก AI
            EnemyAI1 ai = hit.GetComponent<EnemyAI1>();
            if (ai != null && !ai.isDead)
            {
                Vector3 knockDir = (hit.transform.position - position).normalized;
                knockDir.y = 0.5f; // ให้กระดอนขึ้นเล็กน้อย
                ai.StartManualKnockback(knockDir, explosionForce);
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}