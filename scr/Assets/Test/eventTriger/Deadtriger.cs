using UnityEngine;

public class Deadtriger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าวัตถุที่มาโดนมี Tag เป็น "Player" หรือไม่
        if (other.CompareTag("Player"))
        {
            // ดึงสคริปต์ Health ออกมาจากตัว Player
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                // ลดเลือดเท่ากับค่าเลือดปัจจุบัน (เพื่อให้เหลือ 0 พอดี)
                playerHealth.TakeDamage(playerHealth.currentHealth);
            }
        }
        else
        {
            // 2. ถ้าไม่ใช่ Player ให้ทำลายวัตถุนั้นทิ้งทันที
            // ป้องกันการทำลายพื้นหรือวัตถุที่ไม่มีสคริปต์สำคัญ (ถ้าต้องการ)
            if (other.gameObject.GetComponent<Health>() != null || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}