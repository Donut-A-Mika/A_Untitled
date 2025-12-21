using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ใส่ Prefab จากโฟลเดอร์ Project ได้เลยสำหรับวิธีนี้
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public Transform weaponHoldPoint; // จุดที่จะให้กระสุนไปเกิด (เช่น มือ)

    private GameObject currentWeapon;

    void Start()
    {
        EquipWeapon(meleePrefab); // เริ่มเกมมาถือดาบ
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(meleePrefab);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(rangedPrefab);
    }

    void EquipWeapon(GameObject prefab)
    {
        // ลบอาวุธเก่าทิ้งก่อน
        if (currentWeapon != null) Destroy(currentWeapon);

        // สร้างอาวุธใหม่จาก Prefab และตั้งให้เป็นลูกของ weaponHoldPoint
        currentWeapon = Instantiate(prefab, weaponHoldPoint.position, weaponHoldPoint.rotation);
        currentWeapon.transform.SetParent(weaponHoldPoint);

        Debug.Log("Equipped: " + prefab.name);
    }
}