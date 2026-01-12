using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Hold Points")]
    public Transform weaponSlot1; // มือที่ 1
    public Transform weaponSlot2; // มือที่ 2
    public Transform weaponSlot3; // มือที่ 3
    public Transform weaponSlot4; // มือที่ 4

    [Header("Current Weapon")]
    public GameObject currentWeapon;
    private int currentSlotIndex = 1; // เริ่มที่มือ 1

    private GameObject[] equippedWeapons = new GameObject[4]; // เก็บอาวุธที่ติดตั้งแล้ว

    void Start()
    {
        LoadWeaponsFromPlaystate();
        SwitchToSlot(1); // เริ่มด้วยมือที่ 1
    }

    void Update()
    {
        // สลับมือด้วย 1, 2, 3, 4
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToSlot(4);
    }

    void LoadWeaponsFromPlaystate()
    {
        // ติดอาวุธทั้ง 4 มือจาก Playstate
        if (Playstate.gunslot1 != null) EquipWeaponToSlot(Playstate.gunslot1, 1);
        if (Playstate.gunslot2 != null) EquipWeaponToSlot(Playstate.gunslot2, 2);
        if (Playstate.gunslot3 != null) EquipWeaponToSlot(Playstate.gunslot3, 3);
        if (Playstate.gunslot4 != null) EquipWeaponToSlot(Playstate.gunslot4, 4);
    }

    void EquipWeaponToSlot(GameObject weaponPrefab, int slotNumber)
    {
        Transform targetSlot = GetSlotTransform(slotNumber);
        if (targetSlot == null)
        {
            Debug.LogError($"Weapon Slot {slotNumber} ไม่ได้ตั้งค่าใน Inspector!");
            return;
        }

        // ลบอาวุธเก่าออก (ถ้ามี)
        if (equippedWeapons[slotNumber - 1] != null)
        {
            Destroy(equippedWeapons[slotNumber - 1]);
        }

        // สร้างอาวุธใหม่
        GameObject newWeapon = Instantiate(weaponPrefab, targetSlot);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.SetActive(false); // ซ่อนไว้ก่อน

        equippedWeapons[slotNumber - 1] = newWeapon;

        Debug.Log($"Equipped {weaponPrefab.name} to Slot {slotNumber}");
    }

    void SwitchToSlot(int slotNumber)
    {
        // ซ่อนอาวุธทั้งหมด
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] != null)
            {
                equippedWeapons[i].SetActive(false);
            }
        }

        // แสดงอาวุธที่เลือก
        currentSlotIndex = slotNumber;
        GameObject selectedWeapon = equippedWeapons[slotNumber - 1];

        if (selectedWeapon != null)
        {
            selectedWeapon.SetActive(true);
            currentWeapon = selectedWeapon;
            Debug.Log($"Switched to Slot {slotNumber}: {selectedWeapon.name}");
        }
        else
        {
            currentWeapon = null;
            Debug.LogWarning($"Slot {slotNumber} ไม่มีอาวุธ!");
        }
    }

    Transform GetSlotTransform(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1: return weaponSlot1;
            case 2: return weaponSlot2;
            case 3: return weaponSlot3;
            case 4: return weaponSlot4;
            default: return null;
        }
    }

    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }
}