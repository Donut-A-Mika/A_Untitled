using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    public Image iconImage;
    public Sprite lockSprite;
    public Sprite notLockSprite;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        // หันหน้าเข้าหากล้องตลอดเวลา
        if (mainCam != null)
        {
            transform.LookAt(transform.position + mainCam.transform.forward);
        }
    }

    public void SetLockState(bool isLocked)
    {
        if (iconImage == null) return;
        iconImage.sprite = isLocked ? lockSprite : notLockSprite;
    }
}