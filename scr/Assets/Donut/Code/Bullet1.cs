using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    public float damage = 10f;
    public float knockbackForce = 15f;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // ข้ามถ้าเป็นพวกเดียวกัน
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) return;

        // 1. จัดการเรื่องความเสียหาย
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 2. ทำลายตัวเอง (EnemyAI จะดึงค่า Knockback ไปใช้ในจังหวะเดียวกันนี้)
        Destroy(gameObject);
    }
}