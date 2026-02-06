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
