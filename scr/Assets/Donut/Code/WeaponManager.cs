using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Hold Points")]
    public Transform weaponSlot1;
    public Transform weaponSlot2;

    [Header("Current Weapon")]
    public GameObject currentWeapon;
    private int currentSlotIndex = 1;

    private GameObject[] equippedWeapons = new GameObject[4];

    void Start()
    {
        LoadWeaponsFromPlaystate();
        SwitchToSlot(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(2);
    }

    void LoadWeaponsFromPlaystate()
    {
        if (Playstate.gunslot1 != null) EquipWeaponToSlot(Playstate.gunslot1, 1);
        if (Playstate.gunslot2 != null) EquipWeaponToSlot(Playstate.gunslot2, 2);
    }

    void EquipWeaponToSlot(GameObject weaponPrefab, int slotNumber)
    {
        Transform targetSlot = GetSlotTransform(slotNumber);
        if (targetSlot == null) return;

        if (equippedWeapons[slotNumber - 1] != null)
        {
            Destroy(equippedWeapons[slotNumber - 1]);
        }

        // 1. สร้างปืนออกมา
        GameObject newWeapon = Instantiate(weaponPrefab, targetSlot);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        // 2. ดึงค่า Scale จากสคริปต์ RangedWeapon มาใช้
        RangedWeapon weaponScript = newWeapon.GetComponent<RangedWeapon>();
        if (weaponScript != null)
        {
            // ใช้ขนาดที่บันทึกไว้ในตัวปืน (Prefab)
            newWeapon.transform.localScale = weaponScript.weaponScale;
        }
        else
        {
            // ถ้าหาไม่เจอ ให้ใช้ขนาดมาตรฐาน 1, 1, 1
            newWeapon.transform.localScale = Vector3.one;
            Debug.LogWarning($"{newWeapon.name} ไม่มีสคริปต์ RangedWeapon จึงใช้ Scale 1");
        }

        newWeapon.SetActive(false);
        equippedWeapons[slotNumber - 1] = newWeapon;
    }

    void SwitchToSlot(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > equippedWeapons.Length) return;

        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            if (equippedWeapons[i] != null) equippedWeapons[i].SetActive(false);
        }

        currentSlotIndex = slotNumber;
        GameObject selectedWeapon = equippedWeapons[slotNumber - 1];

        if (selectedWeapon != null)
        {
            selectedWeapon.SetActive(true);
            currentWeapon = selectedWeapon;
        }
        else
        {
            currentWeapon = null;
        }
    }

    Transform GetSlotTransform(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1: return weaponSlot1;
            case 2: return weaponSlot2;
            default: return null;
        }
    }
}