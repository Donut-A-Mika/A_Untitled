using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("List of Enemies")]
    public List<GameObject> enemies = new List<GameObject>();

    [Header("UI Reference")]
    public Trigerevent uiSystem; // ลากตัวกลางข้อความมาใส่

    /// <summary>
    /// ฟังก์ชันสำหรับนับศัตรูที่ยังไม่ถูกทำลาย
    /// </summary>
    public int GetRemainingEnemyCount()
    {
        // ลบข้อมูลใน List ที่กลายเป็น Null (ถูกทำลายไปแล้ว) ออกให้หมด
        enemies.RemoveAll(item => item == null);

        // คืนค่าจำนวนที่เหลืออยู่จริงๆ
        return enemies.Count;
    }

    /// <summary>
    /// สั่งให้แสดงผลจำนวนศัตรูไปที่ UI
    /// </summary>
    public void UpdateUIWithCount()
    {
        int count = GetRemainingEnemyCount();

        if (uiSystem != null)
        {
            if (count > 0)
            {
                uiSystem.DisplayNewMessage($"เหลือศัตรูอีก {count} ตัว");
            }
            else
            {
                uiSystem.DisplayNewMessage("กำจัดศัตรูหมดแล้ว!");
            }
        }
    }

    // ตัวอย่าง: ถ้าอยากให้เช็คทุกครั้งที่เดินเข้าพื้นที่ (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateUIWithCount();
        }
    }
}