using UnityEngine;
using System.Collections.Generic;

public class TargetingSystem : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetLayer;
    public float detectionRadius = 30f;
    public Camera playerCamera;

    [Header("Input Keys")]
    public KeyCode switchKey = KeyCode.Tab;
    public KeyCode lockKey = KeyCode.E;

    public static GameObject SelectedTarget { get; private set; }
    public static bool IsLockedOn { get; private set; }

    private List<GameObject> targetsInSight = new List<GameObject>();
    private int currentTargetIndex = 0;
    private TargetIndicator activeIndicator;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        UpdateVisibleTargets();
        HandleInput();
        UpdateIndicatorVisuals();
    }

    // ⭐ หัวใจหลัก: เช็คว่าศัตรูอยู่ในรัศมี และ "กล้องมองเห็น" จริงๆ หรือไม่
    void UpdateVisibleTargets()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        targetsInSight.Clear();

        foreach (var col in cols)
        {
            if (IsInViewport(col.gameObject))
            {
                targetsInSight.Add(col.gameObject);
            }
        }

        // จัดการเรื่อง Index หากเป้าหมายหายไป
        if (targetsInSight.Count == 0)
        {
            ClearTarget();
        }
        else
        {
            currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, targetsInSight.Count - 1);
            SelectedTarget = targetsInSight[currentTargetIndex];
        }
    }

    // ฟังก์ชันเช็คว่าวัตถุอยู่ใน Viewport ของกล้องหรือไม่
    bool IsInViewport(GameObject target)
    {
        Vector3 viewPoint = playerCamera.WorldToViewportPoint(target.transform.position);
        // เช็คว่า x, y อยู่ระหว่าง 0-1 และ z > 0 (อยู่ข้างหน้ากล้อง)
        return viewPoint.z > 0 && viewPoint.x > 0 && viewPoint.x < 1 && viewPoint.y > 0 && viewPoint.y < 1;
    }

    void HandleInput()
    {
        if (targetsInSight.Count == 0) return;

        if (Input.GetKeyDown(switchKey))
        {
            currentTargetIndex = (currentTargetIndex + 1) % targetsInSight.Count;
            IsLockedOn = false; // ปลดล็อกเมื่อสลับเป้า
        }

        if (Input.GetKeyDown(lockKey))
        {
            IsLockedOn = !IsLockedOn;
        }
    }

    void UpdateIndicatorVisuals()
    {
        // 1. ปิด Indicator ของศัตรูทุกตัวในระยะก่อน (เพื่อความชัวร์)
        // หรือใช้วิธีเก็บ Cache ของตัวเก่าไว้ปิดก็ได้เพื่อ Performance

        if (SelectedTarget != null)
        {
            TargetIndicator indicator = SelectedTarget.GetComponentInChildren<TargetIndicator>(true);
            if (indicator != null)
            {
                // โชว์เฉพาะตัวที่เลือก
                if (activeIndicator != null && activeIndicator != indicator)
                    activeIndicator.gameObject.SetActive(false);

                indicator.gameObject.SetActive(true);
                indicator.SetLockState(IsLockedOn);
                activeIndicator = indicator;
            }
        }
    }

    void ClearTarget()
    {
        if (activeIndicator != null) activeIndicator.gameObject.SetActive(false);
        SelectedTarget = null;
        activeIndicator = null;
        IsLockedOn = false;
    }
}