using UnityEngine;
using System.Collections.Generic;

public class TargetingSystem : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask targetLayer;
    public float maxDistance = 50f;
    public Camera playerCamera;

    [Header("Input Keys")]
    public KeyCode switchKey = KeyCode.Tab;
    public KeyCode lockKey = KeyCode.E;

    // ข้อมูลให้สคริปต์อื่นเข้าถึง
    public static GameObject SelectedTarget { get; private set; }
    public static bool IsLockedOn { get; private set; }

    [SerializeField] private List<GameObject> visibleTargets = new List<GameObject>();
    private int currentTargetIndex = 0;
    private TargetIndicator activeIndicator;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        UpdateVisibleList();
        HandleInput();
        UpdateIndicatorVisuals();
    }

    void UpdateVisibleList()
    {
        // 1. หาวัตถุทั้งหมดใน Layer ที่กำหนดภายในรัศมี
        Collider[] allPotentialTargets = Physics.OverlapSphere(transform.position, maxDistance, targetLayer);

        GameObject previousTarget = SelectedTarget;
        visibleTargets.Clear();

        foreach (var col in allPotentialTargets)
        {
            // 2. เช็คว่าอยู่ในมุมมองกล้องหรือไม่ (Viewport Check)
            if (IsInViewport(col.gameObject))
            {
                // 3. (Optional) เช็ค Raycast เพื่อดูว่ามีกำแพงบังหรือไม่
                if (HasLineOfSight(col.gameObject))
                {
                    visibleTargets.Add(col.gameObject);
                }
            }
        }

        // จัดการ Index กรณีเป้าหมายหลุดจอหรือหายไป
        if (visibleTargets.Count == 0)
        {
            ClearTarget();
        }
        else
        {
            // พยายามรักษาเป้าหมายเดิมไว้ถ้ามันยังอยู่ในจอ
            if (previousTarget != null && visibleTargets.Contains(previousTarget))
            {
                currentTargetIndex = visibleTargets.IndexOf(previousTarget);
            }
            else
            {
                currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, visibleTargets.Count - 1);
            }

            SelectedTarget = visibleTargets[currentTargetIndex];
        }
    }

    // ฟังก์ชันเช็คว่าอยู่ในขอบเขตหน้าจอหรือไม่
    bool IsInViewport(GameObject target)
    {
        Vector3 viewPoint = playerCamera.WorldToViewportPoint(target.transform.position);
        return viewPoint.z > 0 && viewPoint.x > 0 && viewPoint.x < 1 && viewPoint.y > 0 && viewPoint.y < 1;
    }

    // ฟังก์ชันเช็คว่ามีอะไรบังสายตาหรือไม่ (Line of Sight)
    bool HasLineOfSight(GameObject target)
    {
        RaycastHit hit;
        Vector3 direction = target.transform.position - playerCamera.transform.position;
        if (Physics.Raycast(playerCamera.transform.position, direction, out hit, maxDistance))
        {
            if (hit.collider.gameObject == target) return true;
        }
        return false;
    }

    void HandleInput()
    {
        if (visibleTargets.Count <= 1) return;

        if (Input.GetKeyDown(switchKey))
        {
            currentTargetIndex = (currentTargetIndex + 1) % visibleTargets.Count;
            IsLockedOn = false;
        }

        if (Input.GetKeyDown(lockKey))
        {
            IsLockedOn = !IsLockedOn;
        }
    }

    void UpdateIndicatorVisuals()
    {
        // ปิด Indicator เก่าก่อนถ้าจำเป็น
        if (SelectedTarget == null && activeIndicator != null)
        {
            activeIndicator.gameObject.SetActive(false);
            activeIndicator = null;
        }

        if (SelectedTarget != null)
        {
            TargetIndicator indicator = SelectedTarget.GetComponentInChildren<TargetIndicator>(true);
            if (indicator != null)
            {
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