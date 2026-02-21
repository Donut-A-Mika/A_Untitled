using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class TargetLockSystem : MonoBehaviour
{
    [Header("Reference")]
    public Transform player;
    public CinemachineCamera cineCam;

    [Header("Target Setting")]
    public float lockRange = 20f;
    public LayerMask enemyLayer;

    private Transform currentTarget;
    private GameObject focusPoint;

    void Start()
    {
        focusPoint = new GameObject("CameraFocusPoint");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(2)) // กดเมาส์กลาง
        {
            if (currentTarget == null)
                LockTarget();
            else
                UnlockTarget();
        }

        if (currentTarget != null)
            UpdateFocusPoint();
    }

    void LockTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(player.position, lockRange, enemyLayer);

        float closestDist = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;
            cineCam.Follow = focusPoint.transform;
            cineCam.LookAt = focusPoint.transform;
        }
    }

    void UnlockTarget()
    {
        currentTarget = null;
        cineCam.Follow = player;
        cineCam.LookAt = player;
    }

    void UpdateFocusPoint()
    {
        Vector3 midPoint = (player.position + currentTarget.position) * 0.5f;
        focusPoint.transform.position = midPoint;
    }
}