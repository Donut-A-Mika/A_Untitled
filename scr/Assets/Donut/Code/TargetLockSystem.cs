using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class TargetLockSystem : MonoBehaviour
{
    public Transform player;

    [Header("Cinemachine")]
    public CinemachineCamera cineCam; // กล้องที่ใช้
    public Transform cameraFollowPivot; // pivot ที่กล้องตาม

    [Header("Targeting")]
    public float lockRange = 15f;
    public LayerMask enemyLayer;

    [Header("Rotation")]
    public float rotateSpeed = 10f;

    private Transform currentTarget;
    public bool IsLocked => currentTarget != null;

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (IsLocked) UnlockTarget();
            else LockNearestTarget();
        }

        if (IsLocked)
        {
            RotatePlayerToTarget();
            RotateCameraToTarget();
        }
    }

    void LockNearestTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(player.position, lockRange, enemyLayer);

        float closest = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider e in enemies)
        {
            float dist = Vector3.Distance(player.position, e.transform.position);
            if (dist < closest)
            {
                closest = dist;
                nearest = e.transform;
            }
        }

        currentTarget = nearest;

        if (cineCam != null && currentTarget != null)
        {
            cineCam.LookAt = currentTarget; // ⭐ สำคัญมาก
        }
    }

    void UnlockTarget()
    {
        currentTarget = null;

        if (cineCam != null)
        {
            cineCam.LookAt = cameraFollowPivot; // กลับไปมอง player
        }
    }

    void RotatePlayerToTarget()
    {
        Vector3 dir = currentTarget.position - player.position;
        dir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        player.rotation = Quaternion.Slerp(
            player.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    void RotateCameraToTarget()
    {
        if (cameraFollowPivot == null) return;

        Vector3 dir = currentTarget.position - cameraFollowPivot.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        cameraFollowPivot.rotation = Quaternion.Slerp(
            cameraFollowPivot.rotation,
            lookRot,
            rotateSpeed * Time.deltaTime
        );
    }
}