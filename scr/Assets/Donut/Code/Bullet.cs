using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 20f; // ให้กระสุนทำลายตัวเองถ้าไม่โดนอะไรเลย

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

        // โดนอะไรก็ตามให้ทำลายกระสุนทิ้ง
        Destroy(gameObject);
    }
}
