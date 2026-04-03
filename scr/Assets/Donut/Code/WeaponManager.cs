using UnityEngine;
using UnityEngine.InputSystem; // ต้องเพิ่มอันนี้

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Hold Points")]
    public Transform weaponSlot1;
    public Transform weaponSlot2;
    public Transform backSlot;

    [Header("Current Weapon")]
    public GameObject currentWeapon;
    private int currentSlotIndex = 1;

    private GameObject[] equippedWeapons = new GameObject[4];

    // --- เพิ่มตัวแปรสำหรับ New Input System ---
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        LoadWeaponsFromPlaystate();
        SwitchToSlot(1);
    }

    void Update()
    {
        // ใช้ Action "Previous" แทนการกดเลข 1
        if (inputActions.Player.Previous.WasPressedThisFrame())
        {
            SwitchToSlot(1);
        }

        // ใช้ Action "Next" แทนการกดเลข 2
        if (inputActions.Player.Next.WasPressedThisFrame())
        {
            SwitchToSlot(2);
        }
    }

    // --- ฟังก์ชันเดิมคงไว้ทั้งหมด ---

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

        GameObject newWeapon = Instantiate(weaponPrefab, targetSlot);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        equippedWeapons[slotNumber - 1] = newWeapon;
    }

    public void SwitchToSlot(int slotNumber) // เปลี่ยนเป็น public เพื่อให้สคริปต์อื่นเรียกได้ถ้าจำเป็น
    {
        if (slotNumber < 1 || slotNumber > equippedWeapons.Length) return;

        currentSlotIndex = slotNumber;

        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            GameObject weapon = equippedWeapons[i];
            if (weapon == null) continue;

            bool isActiveWeapon = (i == slotNumber - 1);

            if (isActiveWeapon)
            {
                MoveWeaponToHand(weapon, GetSlotTransform(slotNumber));
                EnableWeaponUse(weapon, true);
                currentWeapon = weapon;
            }
            else
            {
                MoveWeaponToBack(weapon);
                EnableWeaponUse(weapon, false);
            }
        }
    }

    void MoveWeaponToHand(GameObject weapon, Transform handSlot)
    {
        weapon.transform.SetParent(handSlot);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        ApplyCorrectScale(weapon);
    }

    void MoveWeaponToBack(GameObject weapon)
    {
        weapon.transform.SetParent(backSlot);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    void ApplyCorrectScale(GameObject weapon)
    {
        RangedWeapon rw = weapon.GetComponent<RangedWeapon>();
        MeleeWeapon mw = weapon.GetComponent<MeleeWeapon>();

        if (rw != null)
            weapon.transform.localScale = rw.weaponScale;
        else if (mw != null)
            weapon.transform.localScale = mw.weaponScale;
    }

    void EnableWeaponUse(GameObject weapon, bool enable)
    {
        RangedWeapon rw = weapon.GetComponent<RangedWeapon>();
        if (rw != null) rw.enabled = enable;

        MeleeWeapon mw = weapon.GetComponent<MeleeWeapon>();
        if (mw != null) mw.enabled = enable;
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

    public GameObject GetWeaponFromSlot(int slot)
    {
        if (slot < 1 || slot > equippedWeapons.Length) return null;
        return equippedWeapons[slot - 1];
    }

    public int GetCurrentSlotIndex()
    {
        return currentSlotIndex;
    }
}