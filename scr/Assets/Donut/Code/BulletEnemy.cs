using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float damage = 15f;
    public float lifeTime = 20f;

    [Header("Effects")]
    public GameObject impactEffect; // ลาก Prefab เอฟเฟกต์ระเบิดมาใส่
    public AudioClip impactSound;   // ลากไฟล์เสียงกระทบมาใส่

    private GameObject shooter;      // เก็บข้อมูลว่าใครเป็นคนยิง

    // ฟังก์ชันสำหรับตั้งค่าคนยิง (เรียกใช้จากสคริปต์ที่ยิงกระสุนออกมา)
    public void SetShooter(GameObject owner)
    {
        shooter = owner;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าชน "ตัวเอง" หรือ "คนยิง" หรือไม่
        if (other.gameObject == shooter || other.gameObject == gameObject) return;

        // 2. ถ้าชน Layer "Enemy" (พวกเดียวกัน) ให้ทะลุผ่านไปเลย
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

        // 3. ถ้าชน Layer "Bullet" (ชนกระสุนด้วยกันเอง) ให้ข้ามไป
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet")) return;

        // --- ทำงานเมื่อชนเป้าหมายที่ถูกต้อง ---

        // 4. สร้างเอฟเฟกต์กระทบ (Impact Effect)
        if (impactEffect != null)
        {
            // สร้างเอฟเฟกต์ที่จุดชน และให้หันไปทิศทางตรงข้ามที่กระสุนวิ่งมา
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // 5. เล่นเสียงกระทบ
        if (impactSound != null)
        {
            // ใช้ PlayClipAtPoint เพื่อให้เสียงเล่นจบแม้กระสุนจะถูกทำลายไปแล้ว
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }

        // 6. ลดเลือดเป้าหมาย
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 7. ทำลายกระสุนทิ้ง
        Destroy(gameObject);
    }
}