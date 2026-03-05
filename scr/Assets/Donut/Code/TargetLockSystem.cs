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

    [Header("Weapon")]
    public Transform weaponHolder;

    private Transform currentTarget;
    public bool IsLocked => currentTarget != null;

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (IsLocked) UnlockTarget();
            else LockNearestTarget();
        }

        // ⭐ ถ้า target ถูกทำลาย ให้ปลด lock
        if (currentTarget == null)
        {
            UnlockTarget();
            return;
        }

        if (IsLocked)
        {
            RotatePlayerToTarget();
            RotateCameraToTarget();
            RotateWeaponToTarget(); // ⭐ เพิ่มตรงนี้
        }
    }

    void LockNearestTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(player.position, lockRange, enemyLayer);

        float closest = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider e in enemies)
        {
            if (e == null) continue;

            float dist = Vector3.Distance(player.position, e.transform.position);

            if (dist < closest)
            {
                closest = dist;
                nearest = e.transform;
            }
        }

        if (nearest == null) return;

        currentTarget = nearest;

        if (cineCam != null)
            cineCam.LookAt = currentTarget;
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
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - player.position;
        dir.y = 0f;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        player.rotation = Quaternion.Slerp(
            player.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    void RotateCameraToTarget()
    {
        if (currentTarget == null || cameraFollowPivot == null) return;

        Vector3 dir = currentTarget.position - cameraFollowPivot.position;

        if (dir == Vector3.zero) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        cameraFollowPivot.rotation = Quaternion.Slerp(
            cameraFollowPivot.rotation,
            lookRot,
            rotateSpeed * Time.deltaTime
        );
    }

    void RotateWeaponToTarget()
    {
        if (currentTarget == null || weaponHolder == null) return;

        Vector3 dir = currentTarget.position - weaponHolder.position;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        weaponHolder.rotation = Quaternion.Slerp(
            weaponHolder.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

}