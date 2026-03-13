using UnityEngine;
using System.Collections;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    [Header("Basic Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public Texture2D logo;
    public Vector3 weaponScale = Vector3.one;

    [Header("VFX Settings")]
    public GameObject muzzleFlashPrefab;    // เอฟเฟกต์ไฟปลายกระบอก
    public GameObject shellPrefab;          // Prefab ปลอกกระสุน
    public Transform shellEjectionPoint;    // จุดที่ปลอกกระสุนกระเด็นออก
    public float destroyEffectDelay = 1f;   // ระยะเวลาลบเอ็ฟเฟกต์ออกจาก Scene
    // ⭐ ระบบกระสุน
    [Header("Ammo System")]
    public int magazineSize = 30;
    public float reloadTime = 2f;

    private int currentAmmo;
    private bool isReloading = false;

    public enum FireMode { Single, Auto, Burst }

    [Header("Fire Mode Settings")]
    public FireMode currentMode = FireMode.Single;

    public float fireRate = 5f;
    public int burstCount = 3;

    [Header("Shotgun Settings")]
    public bool useShotgunSpread = false;
    public int pelletsCount = 8;
    [Range(0f, 0.5f)] public float spreadAmount = 0.1f;

    private float nextFireTime = 0f;
    private bool isFiring = false;

    public Texture weaponIcon;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    void Start()
    {
        currentAmmo = magazineSize;
    }

    void Update()
    {
        // ⭐ กด R เพื่อรีโหลด
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    public void Attack()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            if (audioSource && emptySound)
                audioSource.PlayOneShot(emptySound);

            StartCoroutine(Reload());
            return;
        }

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
    private void PlayVFX()
    {
        // 1. สร้างไฟปลายกระบอก
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint); // ให้ขยับตามปืน
            Destroy(flash, destroyEffectDelay);
        }

        // 2. ดีดปลอกกระสุน
        if (shellPrefab != null && shellEjectionPoint != null)
        {
            GameObject shell = Instantiate(shellPrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            if (shellRb != null)
            {
                // ดีดปลอกออกไปทางขวาของจุดดีด
                shellRb.AddForce(shellEjectionPoint.right * 5f, ForceMode.Impulse);
                shellRb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }
            Destroy(shell, 2f); // ปลอกกระสุนอยู่ 2 วิแล้วหายไป
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
            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
                break;
            }

            ExecuteShot();
            yield return new WaitForSeconds(burstDelay);
        }

        nextFireTime = Time.time + interval;
        isFiring = false;
    }
    // เพิ่มฟังก์ชันนี้ใน RangedWeapon.cs
    public bool IsReloading()
    {
        return isReloading;
    }
    private void ExecuteShot()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        PlayVFX();
        // ⭐ เล่นเสียงยิง
        if (audioSource && shootSound)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(shootSound);
        }

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

        Debug.Log("Ammo: " + currentAmmo + "/" + magazineSize);
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
    // เพิ่มฟังก์ชันเหล่านี้ใน RangedWeapon.cs
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return magazineSize;
    }
    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        Debug.Log("Reloading...");

        // ⭐ เล่นเสียงรีโหลด
        if (audioSource && reloadSound)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        Debug.Log("Reload Complete");
    }
}