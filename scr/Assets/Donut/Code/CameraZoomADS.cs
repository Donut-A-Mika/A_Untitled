using Unity.Cinemachine;
using UnityEngine;

public class CameraZoomADS : MonoBehaviour
{
    public CinemachineCamera cineCam;

    [Header("Zoom Setting")]
    public float normalFOV = 50f;
    public float adsFOV = 30f;
    public float zoomSpeed = 10f;

    void Update()
    {
        bool isAiming = Input.GetMouseButton(1); // ???????????

        float targetFOV = isAiming ? adsFOV : normalFOV;

        var lens = cineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(
            lens.FieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );

        cineCam.Lens = lens;
    }
}

