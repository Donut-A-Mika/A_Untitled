using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float damage = 15f;
    public float lifeTime = 20f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. ถ้าชนสิ่งที่เป็น "ศัตรู" (ใช้ Layer แทน Tag)
        // ตรวจสอบว่าสิ่งที่ชนอยู่ใน Layer "Enemy" หรือไม่
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

        // 2. ถ้าชนอะไรที่มีระบบเลือด
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 3. ทำลายกระสุนเมื่อชนสิ่งที่ไม่ใช่กระสุนด้วยกันเอง
        // เปลี่ยนจาก other.CompareTag("Bullet") เป็นเช็ค Layer "Bullet"
        if (other.gameObject.layer != LayerMask.NameToLayer("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}