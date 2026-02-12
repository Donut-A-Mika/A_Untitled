using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Target")]
    public float detectRange = 15f;
    private Transform player;

    [Header("Parts")]
    public Transform headYaw;      // หมุนซ้าย-ขวา
    public Transform firePoint;    // เงย-ก้ม + ยิง

    [Header("Rotation")]
    public float yawSpeed = 5f;
    public float pitchSpeed = 5f;
    public float minPitch = -10f;
    public float maxPitch = 30f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float bulletSpeed = 25f;

    private float nextFireTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectRange) return;

        RotateYaw();
        RotatePitch();

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    // ---------- หมุนซ้าย-ขวา ----------
    void RotateYaw()
    {
        Vector3 dir = player.position - headYaw.position;
        dir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        headYaw.rotation = Quaternion.Slerp(
            headYaw.rotation,
            targetRot,
            yawSpeed * Time.deltaTime
        );
    }

    // ---------- เงย-ก้มเฉพาะ FirePoint ----------
    void RotatePitch()
    {
        Vector3 dir = player.position - firePoint.position;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        Vector3 localEuler = targetRot.eulerAngles;

        // แปลงค่าองศาให้ไม่เกิน 180
        float pitch = localEuler.x;
        if (pitch > 180) pitch -= 360;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        firePoint.localRotation = Quaternion.Euler(
            pitch,
            0f,
            0f
        );
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}