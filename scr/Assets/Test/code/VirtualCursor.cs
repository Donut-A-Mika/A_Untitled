using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VirtualCursor : MonoBehaviour
{
    [Header("Settings")]
    public float cursorSpeed = 1000f;
    public RectTransform canvasRect;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    public Image cursorVisual; // ลาก Image ของตัว Cursor มาใส่ที่นี่

    private RectTransform cursorRect;
    private InputSystem_Actions inputActions;
    private bool isUsingGamepad = false;

    private void Awake()
    {
        cursorRect = GetComponent<RectTransform>();
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update()
    {
        CheckInputDevice();

        if (isUsingGamepad)
        {
            HandleGamepadMovement();
            if (inputActions.Player.Attack.WasPressedThisFrame())
            {
                HandleClick();
            }
        }
    }

    void CheckInputDevice()
    {
        // 1. เช็ค Gamepad: ดูว่ามีการขยับ Stick หรือมีการกดปุ่มใดๆ หรือไม่
        if (Gamepad.current != null)
        {
            // เช็คการขยับของ Left Stick หรือ Right Stick
            bool stickMoved = Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
                             Gamepad.current.rightStick.ReadValue().magnitude > 0.1f;

            // เช็คว่ามีการกดปุ่มใดๆ ในเฟรมนี้หรือไม่ (ใช้คำสั่ง wasUpdatedThisFrame ของ Device)
            if (stickMoved || Gamepad.current.wasUpdatedThisFrame)
            {
                SetGamepadMode(true);
            }
        }

        // 2. เช็ค Mouse: ถ้ามีการขยับหรือคลิก ให้สลับกลับมาใช้เมาส์ปกติ
        if (Mouse.current != null)
        {
            if (Mouse.current.delta.ReadValue().magnitude > 0.1f || Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetGamepadMode(false);
            }
        }
    }

    void SetGamepadMode(bool useGamepad)
    {
        isUsingGamepad = useGamepad;

        if (useGamepad)
        {
            // --- กรณีใช้จอย ---
            if (cursorVisual != null) cursorVisual.enabled = true; // โชว์รูปเคอร์เซอร์จอย

            Cursor.visible = false; // ซ่อนเมาส์จริง
            Cursor.lockState = CursorLockMode.Locked; // ล็อกเมาส์จริงไว้กลางจอ (กันเมาส์หลุดไปจอ 2)
        }
        else
        {
            // --- กรณีกลับมาใช้เมาส์ ---
            if (cursorVisual != null) cursorVisual.enabled = false; // ซ่อนรูปเคอร์เซอร์จอย

            Cursor.visible = true; // โชว์เมาส์จริง
            Cursor.lockState = CursorLockMode.None; // ปลดล็อกเมาส์ให้เลื่อนได้อิสระ

            // ย้ายตำแหน่ง Virtual Cursor ไปที่จุดที่เมาส์อยู่ล่าสุด เพื่อความต่อเนื่อง
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, null, out Vector2 localPoint);
                cursorRect.anchoredPosition = localPoint;
            }
        }
    }

    void HandleGamepadMovement()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            Vector2 newPos = cursorRect.anchoredPosition + (moveInput * cursorSpeed * Time.deltaTime);

            newPos.x = Mathf.Clamp(newPos.x, -canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2);
            newPos.y = Mathf.Clamp(newPos.y, -canvasRect.sizeDelta.y / 2, canvasRect.sizeDelta.y / 2);

            cursorRect.anchoredPosition = newPos;
        }
    }

    void HandleClick()
    {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = RectTransformUtility.WorldToScreenPoint(null, transform.position);

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            // เช็ค Tag "UI"
            if (result.gameObject.CompareTag("UI") || (result.gameObject.transform.parent != null && result.gameObject.transform.parent.CompareTag("UI")))
            {
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
                break;
            }
        }
    }
}