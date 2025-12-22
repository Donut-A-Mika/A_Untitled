using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public Transform weaponHoldPoint;
    public GameObject currentWeapon;

    void Start() { EquipWeapon(meleePrefab); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(meleePrefab);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(rangedPrefab);
    }

    // ฟังก์ชันนี้สำคัญมาก เพื่อให้ PlayerController มาถามว่า "ตอนนี้ถืออะไรอยู่"
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

    void EquipWeapon(GameObject prefab)
    {
        if (currentWeapon != null) Destroy(currentWeapon);
        currentWeapon = Instantiate(prefab, weaponHoldPoint.position, weaponHoldPoint.rotation);
        currentWeapon.transform.SetParent(weaponHoldPoint);
    }
}