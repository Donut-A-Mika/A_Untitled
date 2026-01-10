using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " เลือดเหลือ: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // ใส่ Effect ตายตรงนี้ เช่น เล่น Animation หรือลบ Object ออก
        Destroy(gameObject);
        Debug.Log(gameObject.name + " ตายแล้ว!");
    }
}
