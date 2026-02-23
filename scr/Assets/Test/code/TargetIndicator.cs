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
        // ปิดการแสดงผลเริ่มต้น
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // ทำหน้าที่แค่หันหน้าเข้าหากล้อง ไม่มีการสั่งหมุนกล้องเด็ดขาด
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