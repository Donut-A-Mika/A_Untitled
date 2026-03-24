using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 50f;
    public float lifeTime = 5f;

    [Header("Combat Settings")]
    public float damage = 10f;
    public LayerMask hitLayers; // ติ๊กเลือก Layer ที่กระสุนจะชน (เช่น Default, Enemy)

    [Header("Visual & Sound")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. คำนวณระยะทางที่กระสุนจะเคลื่อนที่ในเฟรมนี้
        float moveDistance = speed * Time.deltaTime;
        Vector3 direction = transform.forward;

        // 2. ยิง Raycast ไปข้างหน้าตามระยะที่จะเคลื่อนที่
        // วิธีนี้จะไม่มีทางทะลุ เพราะเราเช็คเส้นทางก่อนที่โมเดลจะย้ายไปจริง
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance, hitLayers))
        {
            OnHit(hit);
        }
        else
        {
            // ถ้าไม่ชนอะไรเลย ให้เคลื่อนที่ไปข้างหน้าตามปกติ
            transform.Translate(Vector3.forward * moveDistance);
        }
    }

    void OnHit(RaycastHit hit)
    {
        // --- จัดการเอฟเฟกต์ (หันตาม Normal ของจุดที่ชนเป๊ะๆ) ---
        if (hitEffectPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, rot);
            Destroy(effect, 1f);
        }

        // --- เล่นเสียง ---
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hit.point, 1f);
        }

        // --- ส่งความเสียหาย ---
        Health health = hit.collider.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // หากชนแล้ว ให้ทำลายกระสุนทิ้งทันที
        Destroy(gameObject);
    }
}