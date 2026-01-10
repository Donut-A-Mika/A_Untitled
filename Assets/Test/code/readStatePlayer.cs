using UnityEngine;

public class readStatePlayer : MonoBehaviour
{
    public GameObject solit1;
    public GameObject solit2;
    public GameObject solit3;
    public GameObject solit4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //solt 1
        setUpGun(solit1, Playstate.gunslot1);
        setUpGun(solit2, Playstate.gunslot2);
        setUpGun(solit3, Playstate.gunslot3);
        setUpGun(solit4, Playstate.gunslot4);


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Player");
            Debug.Log("gunslot1" + Playstate.gunslot1.name);
            Debug.Log("gunslot2" + Playstate.gunslot2.name);
            Debug.Log("gunslot3" + Playstate.gunslot3.name);
            Debug.Log("gunslot4" + Playstate.gunslot4.name);
            //Debug.Log("robotType" + Playstate.robotType.name);
        }
    }
    void setUpGun(GameObject gunsoulid, GameObject gunperfab)
    {
        // 1. เช็คก่อนว่า Parameter ที่ส่งมามีค่าไหม (ป้องกัน Error)
        if (gunsoulid == null || gunperfab == null)
        {
            if (gunsoulid == null)
            {
                Debug.Log("gunsoulid == null");
            }
            if (gunperfab == null)
            {
                Debug.Log("gunperfab == null");
            }
            Debug.LogWarning("ข้อมูลไม่ครบ: กรุณาใส่ทั้งจุดวางปืนและ Prefab ปืน");
            return;
        }

        // 2. ลบลูกที่มีอยู่เดิมออกให้หมด
        foreach (Transform child in gunsoulid.transform)
        {
            // แนะนำให้ใช้ Destroy เฉยๆ ใน Play Mode
            Destroy(child.gameObject);
        }

        // 3. สร้างปืนใหม่เข้าไปเป็นลูก
        GameObject newChild = Instantiate(gunperfab, gunsoulid.transform);

        // 4. รีเซ็ตตำแหน่งและมุมหมุนให้ตรงตามตัวแม่
        newChild.transform.localPosition = Vector3.zero;
        newChild.transform.localRotation = Quaternion.identity; // เพิ่มบรรทัดนี้เพื่อให้ปืนไม่เอียง
    }
}
