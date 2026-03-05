using Unity.Cinemachine;
using UnityEngine;
public class CameraZoomADS : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public PlayerController player;

    public float normalFOV = 50f;
    public float adsFOV = 30f;
    public float dashFOV = 65f;
    public float zoomSpeed = 10f;

    void Start()
    {
        if (cineCam == null)
            cineCam = FindFirstObjectByType<CinemachineCamera>();

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (cineCam == null) return;

        bool isAiming = Input.GetMouseButton(1);
        bool isDashing = player != null && player.IsDashing();

        float targetFOV = normalFOV;

        if (isDashing)
            targetFOV = dashFOV;
        else if (isAiming)
            targetFOV = adsFOV;

        var lens = cineCam.Lens;

        lens.FieldOfView = Mathf.Lerp(
            lens.FieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );

        cineCam.Lens = lens;
    }
}