using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 100f;
    public float lifeTime = 5f;

    [Header("Combat")]
    public float damage = 20f;
    public LayerMask hitLayers;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    private Vector3 lastPosition;

    void Start()
    {
        // กำหนดจุดเริ่มต้นคือตำแหน่งที่กระสุนเกิด
        lastPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. คำนวณจุดที่จะไปในเฟรมนี้
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.deltaTime;

        // 2. เช็คการชนทันทีระหว่างจุดเก่า (lastPosition) และจุดใหม่ (nextPosition)
        if (Physics.Linecast(lastPosition, nextPosition, out RaycastHit hit, hitLayers))
        {
            HandleHit(hit);
            return;
        }

        // 3. ถ้าไม่ชนอะไรเลย ให้อัปเดตตำแหน่ง
        lastPosition = transform.position;
        transform.position = nextPosition;
    }

    void HandleHit(RaycastHit hit)
    {
        transform.position = hit.point;

        if (hitEffectPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(hit.normal);
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, rot);
            Destroy(effect, 1f);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hit.point, 1f);
        }

        if (hit.collider.TryGetComponent<Health>(out Health h))
        {
            h.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}