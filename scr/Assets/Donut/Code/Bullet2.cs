using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public float explosionForce = 20f;
    public float lifeTime = 5f;
    public GameObject explosionEffect;
    public LayerMask targetLayers;
    public AudioClip explosionSound;
    private bool hasExploded = false;

    void Start() => Invoke(nameof(Explode), lifeTime);

    void OnTriggerEnter(Collider other)
    {
        if (!hasExploded && !other.CompareTag("Player") && !other.CompareTag("Bullet")) Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayers);
        foreach (Collider hit in hitColliders)
        {
            // ทำความเสียหาย (ต้องการสคริปต์ Health ที่ตัวศัตรู)
            // Health h = hit.GetComponent<Health>();
            // if (h != null) h.TakeDamage(damage);

            EnemyAI1 ai = hit.GetComponent<EnemyAI1>();
            if (ai != null && !ai.isDead)
            {
                
                Vector3 knockDir = (hit.transform.position - transform.position).normalized;
                knockDir.y = 0.5f;
                ai.StartManualKnockback(knockDir, explosionForce);
            }
        }
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}