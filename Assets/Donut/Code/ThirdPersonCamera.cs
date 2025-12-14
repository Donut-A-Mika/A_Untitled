using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public float mouseSensitivity = 3f;
    public float minPitch = -10f;   // ก้ม
    public float maxPitch = 10f;    // เงย

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private float pitch;
    private float yaw;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        // ✅ จำกัดมุมก้มเงย
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // หมุน Pivot (ไม่ใช่ตัวกล้อง)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // หมุนตัวละครเฉพาะแกน Y
        player.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

}
