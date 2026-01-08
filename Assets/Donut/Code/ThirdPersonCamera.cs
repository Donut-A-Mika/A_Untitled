using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Transform cam; // Main Camera

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minPitch = -10f;
    public float maxPitch = 10f;
    public float smoothSpeed = 10f;

    [Header("Camera Distance")]
    public float defaultDistance = 5f;
    public float minDistance = 1.2f;

    [Header("Collision")]
    public LayerMask collisionMask;

    private float pitch;
    private float yaw;
    private Quaternion currentRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentRotation = transform.rotation;
    }

    void LateUpdate()
    {
        HandleRotation();
        HandleCameraCollision();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
        transform.rotation = currentRotation;

        // หมุนตัวละครเฉพาะแกน Y
        player.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleCameraCollision()
    {
        // จุดที่กล้องหมุนรอบ (ควรอยู่แถวหัว)
        Vector3 pivotPos = player.position + Vector3.up * 1.6f;

        Vector3 desiredDir = -transform.forward;
        float targetDistance = defaultDistance;

        RaycastHit hit;

        float sphereRadius = 0.3f; // ขนาดกันทะลุ

        if (Physics.SphereCast(
            pivotPos,
            sphereRadius,
            desiredDir,
            out hit,
            defaultDistance,
            collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, defaultDistance);
        }

        Vector3 finalCamPos = pivotPos + desiredDir * targetDistance;

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            finalCamPos,
            smoothSpeed * Time.deltaTime
        );
    }
}