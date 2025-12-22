using UnityEngine;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    public void Attack()
    {
        Debug.Log("ยิงปืน!");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }
    }
}
