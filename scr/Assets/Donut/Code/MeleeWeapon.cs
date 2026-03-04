using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    public Vector3 weaponScale = new Vector3(1, 1, 1);

    public float damage = 25f;
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    [Header("Lunge")]
    public float lungeDistance = 1.5f;
    public float lungeDuration = 0.15f;

    [Header("Cooldown")]
    public float attackCooldown = 1f;

    private float lastAttackTime;
    private PlayerController player;
    public Texture weaponIcon; // เอารูปภาพ Preview ของปืนมาใส่ใน Inspector ของ Prefab ปืน

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void Attack()
    {
        // ⛔ ยังไม่ถึงเวลาตี
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        // 💥 ทำดาเมจ
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Health>(out Health health))
                health.TakeDamage(damage);
        }

        // 🚀 พุ่งไปข้างหน้า
        if (player != null)
            player.LungeForward(lungeDistance, lungeDuration);
    }
}