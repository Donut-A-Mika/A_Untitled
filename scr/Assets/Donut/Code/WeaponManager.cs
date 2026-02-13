using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Hold Points")]
    public Transform weaponSlot1;
    public Transform weaponSlot2;
    public Transform backSlot; // ⭐ จุดติดปืนสำรอง

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

        GameObject newWeapon = Instantiate(weaponPrefab, targetSlot);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        RangedWeapon weaponScript = newWeapon.GetComponent<RangedWeapon>();
        if (weaponScript != null)
            newWeapon.transform.localScale = weaponScript.weaponScale;
        else
            newWeapon.transform.localScale = Vector3.one;

        equippedWeapons[slotNumber - 1] = newWeapon;
    }

    void SwitchToSlot(int slotNumber)
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
    }

    void MoveWeaponToBack(GameObject weapon)
    {
        weapon.transform.SetParent(backSlot);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    void EnableWeaponUse(GameObject weapon, bool enable)
    {
        RangedWeapon rw = weapon.GetComponent<RangedWeapon>();
        if (rw != null) rw.enabled = enable;
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