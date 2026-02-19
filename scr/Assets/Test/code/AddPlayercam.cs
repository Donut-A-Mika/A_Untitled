using UnityEngine;
using Unity.Cinemachine; // ตรวจสอบว่าใช้ Unity.Cinemachine (สำหรับเวอร์ชันใหม่)

public class CinemachineTargetSetter : MonoBehaviour
{
    [Header("Settings")]
    public CinemachineCamera vcam;      // ลาก Component Cinemachine Camera มาใส่ในช่องนี้
    public string targetTag = "Player"; // ชื่อ Tag ของเป้าหมาย

    void Start()
    {
        // 1. ถ้าไม่ได้ลาก vcam มาใส่ใน Inspector ให้พยายามหาจาก Object นี้เอง
        if (vcam == null)
        {
            vcam = GetComponent<CinemachineCamera>();
        }

        // 2. เรียกใช้งานฟังก์ชันเปลี่ยนเป้าหมาย
        SetCameraTarget();
    }

    public void SetCameraTarget()
    {
        // ค้นหา Object ตาม Tag
        GameObject target = GameObject.FindWithTag(targetTag);

        /*if (target != null && vcam != null)
        {
            // สำหรับ Cinemachine 3.0+ การเซต 'Tracking Target' ใน Inspector 
            // จะต้องเซตผ่านค่า Follow และ LookAt ในโค้ดครับ
            vcam.Follow = target.transform;
            vcam.LookAt = target.transform;

            Debug.Log($"[Cinemachine] ตั้งค่าเป้าหมายสำเร็จ: {target.name}");
        }
        else
        {
            if (target == null) Debug.LogError($"[Cinemachine] ไม่พบ Object ที่มี Tag: {targetTag}");
            if (vcam == null) Debug.LogError("[Cinemachine] ไม่ได้ระบุ CinemachineCamera ใน Script");
        }*/
    }
}