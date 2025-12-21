using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
    public GameObject bulletPrefab; // เช็คว่าลากไฟล์กระสุนมาใส่ในช่องนี้หรือยัง
    public Transform firePoint;     // เช็คว่าลากจุดปลายกระบอกปืนมาใส่หรือยัง
    public float bulletForce = 20f;

    void Update()
    {
        // ต้องมีส่วนนี้เพื่อให้ปืน "ฟัง" คำสั่งคลิกเมาส์
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // สร้างกระสุน
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // สั่งให้กระสุนพุ่งไป
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
            }

            Debug.Log("ยิงกระสุนออกไปแล้ว!");
        }
        else
        {
            Debug.LogWarning("ลืมใส่ Bullet Prefab หรือ Fire Point หรือเปล่า?");
        }
    }
}