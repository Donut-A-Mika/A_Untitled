using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 20f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // --- ส่วนที่แก้ไข: ป้องกันการชนตัวเองและพวกเดียวกัน ---
        // 1. ถ้าชนกับสิ่งที่มี Tag ว่า "Player" (ตัวเรา) ให้ข้ามไป
        // 2. ถ้าชนกับสิ่งที่มี Tag ว่า "Bullet" (กระสุนนัดอื่น) ให้ข้ามไป
        if (other.CompareTag("Player") || other.CompareTag("Bullet"))
        {
            return;
        }

        // ถ้าชนอะไรที่มีระบบเลือด
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // โดนอะไรก็ตามที่ไม่ใช่ Player หรือ Bullet ให้ทำลายกระสุนทิ้ง
        Destroy(gameObject);
    }
}