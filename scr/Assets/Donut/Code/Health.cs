using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Action onDeath;   // ⭐ เพิ่มบรรทัดนี้

    [Header("Hit Flash Setting")]
    public bool enableHitFlash = true;      // ✅ เปิด/ปิดเอฟเฟกต์เปลี่ยนสี
    public Color hitColor = Color.red;      // สีตอนโดนตี
    public float flashTime = 0.1f;           // ระยะเวลาที่เปลี่ยนสี

    private Renderer rend;
    private Color originalColor;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;

        // ดึง Renderer
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " เลือดเหลือ: " + currentHealth);

        // 🔴 เล่นเอฟเฟกต์เปลี่ยนสีถ้าเปิดใช้งาน
        if (enableHitFlash && rend != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(HitFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        rend.material.color = hitColor;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = originalColor;
    }

    void Die()
    {
        onDeath?.Invoke();   // ⭐ แจ้งว่า “ตายแล้ว”
        Destroy(gameObject);
        Debug.Log(gameObject.name + "Dead");
    }
}