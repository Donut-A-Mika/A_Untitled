using UnityEngine;
using System.Collections;

public class GunShoot : MonoBehaviour
{
    [Header("Shoot Settings")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.2f;

    [Header("References")]
    public Transform firePoint;
    public LayerMask hitLayers;

    private float nextFireTime;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        // Debug เส้นยิง
        Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 0.5f);

        if (Physics.Raycast(ray, out hit, range, hitLayers))
        {
            Debug.Log("ยิงโดน: " + hit.collider.name);

            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}