using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPulseGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage;

    [Header("Glow Settings")]
    public Color normalColor = Color.white;
    public Color glowColor = new Color(1.2f, 1.2f, 1.2f); // สว่างขึ้น
    public float pulseSpeed = 2f; // ความเร็วกระพริบ

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.05f;
    public float scaleSpeed = 5f;

    private bool isHover = false;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetImage.color = normalColor;
    }

    void Update()
    {
        if (isHover)
        {
            // 🔥 ทำให้สี "กระพริบช้าๆ"
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            targetImage.color = Color.Lerp(normalColor, glowColor, t);

            // 🔍 ขยายปุ่มเล็กน้อยแบบลื่น
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * scaleMultiplier, Time.deltaTime * scaleSpeed);
        }
        else
        {
            // 🔙 กลับค่าปกติ
            targetImage.color = Color.Lerp(targetImage.color, normalColor, Time.deltaTime * 5f);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}
