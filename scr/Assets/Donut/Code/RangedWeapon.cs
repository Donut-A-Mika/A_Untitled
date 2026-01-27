using UnityEngine;
using System.Collections;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    // --- ตั้งค่าพื้นฐาน (ห้ามลบ) ---
    [Header("Basic Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public Texture2D logo;

    // --- ระบบโหมดการยิง (Fire Mode) ---
    public enum FireMode { Single, Auto, Burst }
    [Header("Fire Mode Settings")]
    public FireMode currentMode = FireMode.Single;

    [Tooltip("อัตราการยิง: จำนวนนัดต่อ 1 วินาที")]
    public float fireRate = 5f;

    [Tooltip("จำนวนครั้งที่จะยิงออกไปในโหมด Burst")]
    public int burstCount = 3;

    // --- ระบบลูกซอง (Shotgun Spread) ---
    [Header("Shotgun Settings")]
    public bool useShotgunSpread = false;
    public int pelletsCount = 8;
    [Range(0f, 0.5f)] public float spreadAmount = 0.1f;

    private float nextFireTime = 0f;
    private bool isFiring = false; // นำมาใช้เช็คสถานะการยิง (โดยเฉพาะ Burst)

    // Interface Method: เรียกใช้ใน Update() ของ PlayerController
    public void Attack()
    {
        // 1. ถ้ายังไม่ถึงเวลาหน่วง (Cooldown) หรือกำลังยิง Burst ค้างอยู่ ให้หยุดทำงาน
        if (Time.time < nextFireTime || isFiring) return;

        // 2. คำนวณช่วงว่างระหว่างนัด (Interval)
        float fireInterval = 1f / Mathf.Max(fireRate, 0.01f);

        switch (currentMode)
        {
            case FireMode.Single:
                SingleFire(fireInterval);
                break;
            case FireMode.Auto:
                AutoFire(fireInterval);
                break;
            case FireMode.Burst:
                StartCoroutine(BurstFireRoutine(fireInterval));
                break;
        }
    }

    // --- แยก Method ตามโหมดการยิง ---

    private void SingleFire(float interval)
    {
        ExecuteShot();
        nextFireTime = Time.time + interval;
    }

    private void AutoFire(float interval)
    {
        // โหมด Auto จะทำงานเหมือน Single แต่เมื่อเรียกใน Update() ต่อเนื่อง จะยิงรัวตาม interval
        ExecuteShot();
        nextFireTime = Time.time + interval;
    }

    private IEnumerator BurstFireRoutine(float interval)
    {
        isFiring = true; // เริ่มการยิง Burst (ล็อคไม่ให้ Attack() ซ้อนกัน)

        float burstDelay = 0.08f;

        for (int i = 0; i < burstCount; i++)
        {
            ExecuteShot();
            yield return new WaitForSeconds(burstDelay);
        }

        // ตั้งเวลา Cooldown หลังจากยิงครบชุด และปลดล็อค isFiring
        nextFireTime = Time.time + interval;
        isFiring = false;
    }

    // --- Method ตัดสินใจรูปแบบกระสุน ---
    private void ExecuteShot()
    {
        if (useShotgunSpread)
        {
            for (int i = 0; i < pelletsCount; i++)
            {
                CreateAndFireBullet(true);
            }
        }
        else
        {
            CreateAndFireBullet(false);
        }
    }

    // --- Method สร้างกระสุน ---
    private void CreateAndFireBullet(bool applySpread)
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 shootDirection = firePoint.forward;

        if (applySpread)
        {
            Vector2 spread = Random.insideUnitCircle * spreadAmount;
            shootDirection += firePoint.right * spread.x + firePoint.up * spread.y;
            shootDirection.Normalize();
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(shootDirection * bulletForce, ForceMode.Impulse);
        }
    }
}