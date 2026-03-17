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
        // 1. ถ้าชนสิ่งที่เป็น "ศัตรู" เหมือนกัน ให้ข้ามไป (ไม่ทำลายกระสุน)
        // สมมติว่าศัตรูใช้ Layer "Enemy" หรือคุณจะเช็ค Tag ก็ได้
        if (other.CompareTag("Enemy")) return;

        // 2. ถ้าชนอะไรที่มีระบบเลือด (Health)
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 3. ทำลายกระสุนทิ้งเมื่อชนกำแพง พื้น หรือผู้เล่น
        // ยกเว้นกระสุนด้วยกันเอง (ป้องกันกระสุนชนกันกลางอากาศ)
        if (!other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}