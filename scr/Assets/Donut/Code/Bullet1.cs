using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;
    public float knockbackForce = 15f;
    public float lifeTime = 5f;

    [Header("Visual Effects")]
    public GameObject hitEffectPrefab; // เอ็ฟเฟกต์ตอนกระสุนชน (เช่น ฝุ่นหรือประกายไฟ)

    [Header("Sound Settings")]
    public AudioClip hitSound;         // เสียงตอนกระสุนชน

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // ข้ามถ้าเป็นพวกเดียวกัน
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) return;

        // --- ⭐ ส่วนที่เพิ่มเข้ามา: เอ็ฟเฟกต์และเสียง ---

        // 1. เล่นเสียงกระทบ (สร้าง -> เล่น -> ลบตัวเองอัตโนมัติ)
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f);
        }

        // 2. สร้างเอ็ฟเฟกต์กระทบ (ถ้ามี)
        if (hitEffectPrefab != null)
        {
            // สร้างเอ็ฟเฟกต์ ณ จุดที่ชน และทำลายทิ้งใน 1 วินาที
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // --- ส่วนเดิม ---

        // 3. จัดการเรื่องความเสียหาย
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 4. ทำลายตัวเอง
        Destroy(gameObject);
    }
}