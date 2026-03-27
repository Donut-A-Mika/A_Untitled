using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 50f; // ดึงตัวแปรความเร็วจากโค้ด 1 มาใช้สำหรับการเคลื่อนที่แบบ Raycast
    public float lifeTime = 20f;

    [Header("Combat Settings")]
    public float damage = 15f;
    public LayerMask hitLayers; // ดึงมาจากโค้ด 1 (สามารถไปตั้งค่าใน Inspector ให้ชนเฉพาะเลเยอร์ที่ต้องการได้)

    [Header("Effects")]
    public GameObject impactEffect;
    public AudioClip impactSound;

    private GameObject shooter;

    // ฟังก์ชันสำหรับตั้งค่าคนยิง
    public void SetShooter(GameObject owner)
    {
        shooter = owner;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. คำนวณระยะทางที่กระสุนจะเคลื่อนที่ในเฟรมนี้ (จากโค้ด 1)
        float moveDistance = speed * Time.deltaTime;
        Vector3 direction = transform.forward;

        // 2. ยิง Raycast แบบทะลุ (RaycastAll) เผื่อในกรณีที่มันไปโดน "คนยิง" หรือ "พวกเดียวกัน" ก่อน
        // มันจะได้ทะลุไปเช็คการชนเป้าหมายที่อยู่ด้านหลังต่อได้เลย
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, moveDistance, hitLayers);

        // เรียงลำดับสิ่งที่ชนจากระยะใกล้สุด ไปไกลสุด
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool hitValidTarget = false;

        foreach (RaycastHit hit in hits)
        {
            // ตรวจสอบข้อยกเว้นต่างๆ (จากโค้ด 2)
            if (hit.collider.gameObject == shooter || hit.collider.gameObject == gameObject) continue; // ข้ามตัวเองและคนยิง
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) continue;             // ข้ามพวกเดียวกัน
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Bullet")) continue;            // ข้ามกระสุนด้วยกัน

            // ถ้าผ่านมาถึงจุดนี้ได้ แสดงว่าชนเป้าหมายที่ถูกต้อง!
            OnHit(hit);
            hitValidTarget = true;
            break; // สั่งหยุดเช็ค เพราะเราชนเป้าหมายแรกที่ถูกต้องแล้ว
        }

        // 3. ถ้าไม่ชนอะไรที่เป็นเป้าหมายเลย ให้เคลื่อนที่ไปข้างหน้าตามปกติ
        if (!hitValidTarget)
        {
            transform.Translate(Vector3.forward * moveDistance);
        }
    }

    void OnHit(RaycastHit hit)
    {
        // --- จัดการเอฟเฟกต์ (หันตาม Normal ของจุดที่ชนเป๊ะๆ จากโค้ด 1) ---
        if (impactEffect != null)
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            GameObject effect = Instantiate(impactEffect, hit.point, rot);
            Destroy(effect, 1f);
        }

        // --- เล่นเสียงที่จุดตกกระทบ (จากโค้ด 1) ---
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, hit.point, 1f);
        }

        // --- ส่งความเสียหาย ---
        Health health = hit.collider.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // หากชนเป้าหมายที่ถูกต้องแล้ว ให้ทำลายกระสุนทิ้งทันที
        Destroy(gameObject);
    }
}