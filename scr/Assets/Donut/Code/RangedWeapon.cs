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
    // เพิ่มตัวแปรสำหรับตั้งค่าขนาดปืน
    public Vector3 weaponScale = Vector3.one;

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
    private bool isFiring = false;

    public void Attack()
    {
        if (Time.time < nextFireTime || isFiring) return;

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

    private void SingleFire(float interval)
    {
        ExecuteShot();
        nextFireTime = Time.time + interval;
    }

    private void AutoFire(float interval)
    {
        ExecuteShot();
        nextFireTime = Time.time + interval;
    }

    private IEnumerator BurstFireRoutine(float interval)
    {
        isFiring = true;
        float burstDelay = 0.08f;

        for (int i = 0; i < burstCount; i++)
        {
            ExecuteShot();
            yield return new WaitForSeconds(burstDelay);
        }

        nextFireTime = Time.time + interval;
        isFiring = false;
    }

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