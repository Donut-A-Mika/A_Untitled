using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Outline outline;

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false;
    }
}
