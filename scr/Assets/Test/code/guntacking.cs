using UnityEngine;

public class AimingSystem : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;

    [Header("Rotation Settings")]
    [Tooltip("ใส่ปืนหรือวัตถุที่ต้องการให้หันตามเป้าเล็งได้มากกว่า 1 อย่าง")]
    public Transform[] objectsToRotate; // เปลี่ยนจากอันเดียวเป็น Array

    public float rayRange = 100f;
    public LayerMask hitLayers;

    [Header("Debug Settings")]
    public bool showDebugLines = true;
    public Color debugLineColor = Color.red;
    public Vector3 currentHitPoint;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;
        UpdateAiming();
    }

    private void UpdateAiming()
    {
        // 1. ยิง Ray จากกลางจอกล้อง
        Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, rayRange, hitLayers))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(rayRange);
        }

        currentHitPoint = targetPoint;

        // 2. สั่งให้วัตถุทุกชิ้นใน Array หันไปที่เป้าหมาย
        if (objectsToRotate != null && objectsToRotate.Length > 0)
        {
            foreach (Transform obj in objectsToRotate)
            {
                if (obj != null)
                {
                    RotateObject(obj, targetPoint);

                    // 3. วาดเส้น Debug จากวัตถุแต่ละชิ้นไปยังจุดเป้าหมาย
                    if (showDebugLines)
                    {
                        Debug.DrawLine(obj.position, targetPoint, debugLineColor);
                    }
                }
            }
        }

        // เส้น Debug จากกล้องไปยังเป้าหมาย (สีเขียว)
        if (showDebugLines)
        {
            Debug.DrawLine(ray.origin, targetPoint, Color.green);
        }
    }

    private void RotateObject(Transform obj, Vector3 target)
    {
        Vector3 direction = target - obj.position;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // ปรับหมุนทันที
            obj.rotation = targetRotation;

            // หรือถ้าต้องการให้นุ่มนวล (Smooth) ให้ใช้ Slerp:
            // obj.rotation = Quaternion.Slerp(obj.rotation, targetRotation, Time.deltaTime * 20f);
        }
    }
}