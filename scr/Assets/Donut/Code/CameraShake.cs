using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    CinemachineBasicMultiChannelPerlin noise;

    Coroutine shakeRoutine;

    void Awake()
    {
        Instance = this;

        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("ไม่พบ CinemachineBasicMultiChannelPerlin บนกล้อง");
        }
    }

    public void Shake(float intensity, float time)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeCoroutine(intensity, time));
    }

    IEnumerator ShakeCoroutine(float intensity, float time)
    {
        noise.AmplitudeGain = intensity;

        yield return new WaitForSeconds(time);

        noise.AmplitudeGain = 0f;
    }
}