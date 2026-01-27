using UnityEngine;
using System.Collections;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    // --- ตั้งค่าพื้นฐาน ---
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    // --- ระบบโหมดการยิง ---
    public enum FireMode { Single, Auto, Burst } // สร้างตัวเลือก Dropdown
    [Header("Weapon Settings")]
    public FireMode currentMode = FireMode.Single;

    [Tooltip("จำนวนนัดต่อวินาที (สำหรับ Auto)")]
    public float fireRate = 0.2f;

    [Tooltip("จำนวนนัดที่ยิงออกไปในโหมด Burst")]
    public int burstCount = 3;

    private float nextFireTime = 0f;
    private bool isFiring = false;

    // Interface Method
    public void Attack()
    {
        // ตรวจสอบ Cooldown พื้นฐานก่อนยิง
        if (Time.time < nextFireTime) return;

        switch (currentMode)
        {
            case FireMode.Single:
                SingleFire();
                break;
            case FireMode.Auto:
                AutoFire();
                break;
            case FireMode.Burst:
                StartCoroutine(BurstFireRoutine());
                break;
        }
    }

    // --- แยก Method ตามโหมดการยิง ---

    private void SingleFire()
    {
        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    private void AutoFire()
    {
        // สำหรับ Auto มักจะใช้การเช็คปุ่มค้างจาก Script Controller 
        // แต่ใน Method นี้เราจะจัดการเรื่อง Cooldown ให้
        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    private void BurstFire()
    {
        // เรียกใช้ผ่าน Coroutine เพื่อให้มีการเว้นจังหวะระหว่างนัดใน 1 ชุด
        StartCoroutine(BurstFireRoutine());
    }

    private IEnumerator BurstFireRoutine()
    {
        nextFireTime = Time.time + fireRate + (burstCount * 0.1f); // กันการกดยิงรัวเกินไปขณะ Burst
        for (int i = 0; i < burstCount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(0.1f); // ระยะห่างระหว่างกระสุนในโหมด Burst
        }
    }

    // --- Method กลางสำหรับการสร้างกระสุน ---
    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Debug.Log($"ยิงปืน! โหมด: {currentMode}");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }
    }
}