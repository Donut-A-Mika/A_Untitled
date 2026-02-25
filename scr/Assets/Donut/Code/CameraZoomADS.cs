using Unity.Cinemachine;
using UnityEngine;
public class CameraZoomADS : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public PlayerController player; // 👈 ลาก Player มาใส่

    [Header("Zoom Setting")]
    public float normalFOV = 50f;
    public float adsFOV = 30f;
    public float dashFOV = 65f;     // 👈 ซูมออกตอน Dash
    public float zoomSpeed = 10f;

    void Update()
    {
        bool isAiming = Input.GetMouseButton(1);
        bool isDashing = player != null && player.IsDashing(); // 👈 เช็คจาก Player

        float targetFOV;

        if (isDashing)
            targetFOV = dashFOV;      // 🔥 Dash priority สูงสุด
        else if (isAiming)
            targetFOV = adsFOV;
        else
            targetFOV = normalFOV;

        var lens = cineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(
            lens.FieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
        cineCam.Lens = lens;
    }
}