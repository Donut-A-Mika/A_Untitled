using UnityEngine;
using TMPro; // สำคัญมาก: ต้องมี Namespace นี้เพื่อเรียกใช้งาน TMP

public class Trigerevent : MonoBehaviour
{
    // ตัวแปรสำหรับลาก TextMesh Pro (GUI หรือ 3D) มาใส่ใน Inspector
    public TMP_Text textDisplay;
    public string newMessage = "ตรวจพบการชน!";

    // ทำงานเมื่อมี Object เข้ามาในขอบเขต Trigger
    private void OnTriggerEnter(Collider other)
    {
        // แนะนำให้เช็ค Tag ของสิ่งที่มาชน (เช่น "Player")
        if (other.CompareTag("Player"))
        {
            UpdateText();
        }
    }

    void UpdateText()
    {
        if (textDisplay != null)
        {
            textDisplay.text = newMessage;

            // แถม: ถ้าอยากเปลี่ยนสีด้วย
            // textDisplay.color = Color.red;

            Debug.Log("TextMesh Pro Updated!");
        }
    }
}