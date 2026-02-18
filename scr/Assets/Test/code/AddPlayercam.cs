using UnityEngine;
using Unity.Cinemachine; 

public class CinemachineTargetSetter : MonoBehaviour
{
    [Header("Settings")]
    public CinemachineCamera vcam;      
    public string targetTag = "Player"; 

    void Start()
    {
        if (vcam == null)
        {
            vcam = GetComponent<CinemachineCamera>();
        }
        SetCameraTarget();
    }

    public void SetCameraTarget()
    {
        // ค้นหา Object ตาม Tag
        GameObject target = GameObject.FindWithTag(targetTag);

        if (target != null && vcam != null)
        {
            vcam.Follow = target.transform;
            vcam.LookAt = target.transform;

            Debug.Log($"[Cinemachine]จ: {target.name}");
        }
        else
        {
            if (target == null) Debug.LogError($"[Cinemachine]Tag: {targetTag}");
            if (vcam == null) Debug.LogError("[Cinemachine] CinemachineCamera ใน Script");
        }
    }
}