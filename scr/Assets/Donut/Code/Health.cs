using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Action onDeath;

    [Header("Hit Flash Setting")]
    public bool enableHitFlash = true;
    public Color hitColor = Color.red;
    public float flashTime = 0.1f;

    private Renderer rend;
    private Color originalColor;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            // ใช้ material.color เพื่อเก็บสีเริ่มต้น
            originalColor = rend.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} เลือดเหลือ: {currentHealth}");

        if (enableHitFlash && rend != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
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
        onDeath?.Invoke();
        Debug.Log(gameObject.name + " Dead");
        Destroy(gameObject);
    }
}