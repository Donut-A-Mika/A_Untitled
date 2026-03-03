using Unity.Cinemachine;
using UnityEngine;

public class HardLockSystem : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform cameraPivot; // ตัว Follow ของ Cinemachine
    public CinemachineCamera cineCam;

    [Header("Target Settings")]
    public float lockRange = 25f;
    public LayerMask enemyLayer;

    [Header("Rotation")]
    public float rotateSpeed = 8f;

    private Transform currentTarget;

    public Transform CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;

    void Update()
    {
        AutoLockNearest();

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(player.position, currentTarget.position);
            if (dist > lockRange)
            {
                currentTarget = null;
                cineCam.LookAt = cameraPivot;
                return;
            }

            RotatePlayer();
            RotateCamera();
        }
    }

    void AutoLockNearest()
    {
        Collider[] enemies = Physics.OverlapSphere(player.position, lockRange, enemyLayer);

        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider enemy in enemies)
        {
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = enemy.transform;
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest;
        }
    }

    void RotatePlayer()
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

    void RotateCamera()
    {
        if (cameraPivot == null) return;

        Vector3 dir = currentTarget.position - player.position;
        dir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        cameraPivot.rotation = Quaternion.Slerp(
            cameraPivot.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}