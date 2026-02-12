using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Customization : MonoBehaviour
{
    [Header("Part Settings")]
    public GameObject[] customePart;      // เก็บ Prefab ปืน
    public Vector3[] partScales;         // เก็บขนาด (Scale) ของปืนแต่ละอัน (ลำดับต้องตรงกับด้านบน)

    [Header("Spawn Points")]
    public GameObject Gun1;
    public GameObject Gun2;
    public GameObject Gun3;
    public GameObject Gun4;

    [Header("UI Display")]
    public RawImage targetRawImageslotGun1;
    public RawImage targetRawImageslotGun2;
    public RawImage targetRawImageslotGun3;
    public RawImage targetRawImageslotGun4;

    private int slotGun1 = 0;
    private int slotGun2 = 0;
    private int slotGun3 = 0;
    private int slotGun4 = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Debug เช็คว่า Playstate เก็บค่าอะไรไว้
            Debug.Log("gunslot1: " + (Playstate.gunslot1 != null ? Playstate.gunslot1.name : "Empty"));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            lordSreen();
        }
    }

    void previview(int slot)
    {
        GameObject parentObj = null;
        RawImage currentUI = null;
        int currentSlotIndex = 0;

        switch (slot)
        {
            case 1: parentObj = Gun1; currentUI = targetRawImageslotGun1; currentSlotIndex = slotGun1; break;
            case 2: parentObj = Gun2; currentUI = targetRawImageslotGun2; currentSlotIndex = slotGun2; break;
            case 3: parentObj = Gun3; currentUI = targetRawImageslotGun3; currentSlotIndex = slotGun3; break;
            case 4: parentObj = Gun4; currentUI = targetRawImageslotGun4; currentSlotIndex = slotGun4; break;
        }

        if (parentObj == null || currentUI == null) return;

        // 1. ลบปืนเก่า
        foreach (Transform child in parentObj.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. สร้างปืนใหม่
        if (customePart[currentSlotIndex] != null)
        {
            GameObject newChild = Instantiate(customePart[currentSlotIndex], parentObj.transform);
            newChild.transform.localPosition = Vector3.zero;
            newChild.transform.localRotation = Quaternion.identity;

            // --- ส่วนการตั้งขนาดจากสคริปต์นี้โดยตรง ---
            // ตรวจสอบว่าเราได้ตั้งค่าใน Array partScales ไว้ไหม
            if (currentSlotIndex < partScales.Length)
            {
                newChild.transform.localScale = partScales[currentSlotIndex];
            }
            else
            {
                // ถ้าลืมตั้งค่าใน Inspector ให้ใช้ขนาด 1,1,1 เป็นพื้นฐาน
                newChild.transform.localScale = Vector3.one;
            }

            // ดึง Logo มาโชว์ใน UI (ใช้จาก RangedWeapon เดิมที่มีอยู่)
            RangedWeapon weaponScript = newChild.GetComponent<RangedWeapon>();
            if (weaponScript != null && weaponScript.logo != null)
            {
                currentUI.texture = weaponScript.logo;
            }
        }
    }

    #region Button Events
    public void nextweaponslotGun1() { slotGun1 = (slotGun1 + 1) % customePart.Length; previview(1); }
    public void nextweaponslotGun2() { slotGun2 = (slotGun2 + 1) % customePart.Length; previview(2); }
    public void nextweaponslotGun3() { slotGun3 = (slotGun3 + 1) % customePart.Length; previview(3); }
    public void nextweaponslotGun4() { slotGun4 = (slotGun4 + 1) % customePart.Length; previview(4); }

    public void saveState()
    {
        if (customePart[slotGun1] != null) Playstate.SaveToPlaystate(customePart[slotGun1], 1);
        if (customePart[slotGun2] != null) Playstate.SaveToPlaystate(customePart[slotGun2], 2);
        if (customePart[slotGun3] != null) Playstate.SaveToPlaystate(customePart[slotGun3], 3);
        if (customePart[slotGun4] != null) Playstate.SaveToPlaystate(customePart[slotGun4], 4);
        Debug.Log("Saved all weapon slots.");
    }

    void lordSreen() { SceneManager.LoadScene("GameScene"); }
    #endregion
}