using UnityEngine;
using UnityEngine.InputSystem; // จำเป็นต้องใช้

public class GamepadDetector : MonoBehaviour
{
    [Header("Settings")]
    public GameObject objectToActivate; // ลาก Object ที่ต้องการเปิด/ปิดมาใส่ที่นี่

    void Update()
    {
        // ตรวจสอบว่ามี Gamepad เชื่อมต่ออยู่หรือไม่
        if (Gamepad.current != null)
        {
            // ถ้ามีจอย ให้ Active Object
            if (objectToActivate != null && !objectToActivate.activeSelf)
            {
                objectToActivate.SetActive(true);
                Debug.Log("Gamepad Connected: Object Activated");
            }
        }
        else
        {
            // ถ้าไม่มีการใช้จอย (ถอดปลั๊กออก) ให้ Deactivate Object (หรือจะไม่ทำอะไรก็ได้ตามโจทย์)
            if (objectToActivate != null && objectToActivate.activeSelf)
            {
                objectToActivate.SetActive(false);
                Debug.Log("Gamepad Disconnected: Object Deactivated");
            }
        }
    }
}