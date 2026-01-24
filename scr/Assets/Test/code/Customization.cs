using NUnit.Framework.Internal;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Customization : MonoBehaviour
{

    public GameObject[] customePart;

    public GameObject Gun1;
    public GameObject Gun2;
    public GameObject Gun3;
    public GameObject Gun4;

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
            Debug.Log("Player");
            Debug.Log("gunslot1" + Playstate.gunslot1.name);
            Debug.Log("gunslot2" + Playstate.gunslot2.name);
            Debug.Log("gunslot3" + Playstate.gunslot3.name);
            Debug.Log("gunslot4" + Playstate.gunslot4.name);
            //Debug.Log("robotType" + Playstate.robotType.name);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {

            lordSreen();
        }
    }
    // Update is called once per frame
    void previview(int slot)
    {
        GameObject parentObj = null;
        RawImage currentUI = null; // เพิ่มตัวแปรเพื่อเก็บ RawImage ที่จะเปลี่ยน
        int currentSlotIndex = 0;

        // เลือก Parent, Index และ UI ให้ตรงกับ Slot ที่ส่งมา
        switch (slot)
        {
            case 1:
                parentObj = Gun1;
                currentUI = targetRawImageslotGun1;
                currentSlotIndex = slotGun1;
                break;
            case 2:
                parentObj = Gun2;
                currentUI = targetRawImageslotGun2;
                currentSlotIndex = slotGun2;
                break;
            case 3:
                parentObj = Gun3;
                currentUI = targetRawImageslotGun3;
                currentSlotIndex = slotGun3;
                break;
            case 4:
                parentObj = Gun4;
                currentUI = targetRawImageslotGun4;
                currentSlotIndex = slotGun4;
                break;
        }

        if (parentObj == null || currentUI == null) return;

        // 1. ลบ Model เก่าใน Scene ออก
        foreach (Transform child in parentObj.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. ตรวจสอบข้อมูลใน Array
        if (customePart[currentSlotIndex] != null)
        {
            // 3. สร้าง Model ใหม่ใน Scene
            GameObject newChild = Instantiate(customePart[currentSlotIndex], parentObj.transform);
            newChild.transform.localPosition = Vector3.zero;

            // 4. ดึงสคริปต์ RangedWeapon จากตัวที่เพิ่งสร้าง
            RangedWeapon weaponScript = newChild.GetComponent<RangedWeapon>();

            if (weaponScript != null)
            {
                // เปลี่ยนรูปใน RawImage ของ Slot นั้นๆ ให้เป็น logo ของอาวุธ
                currentUI.texture = weaponScript.logo;
            }
            else
            {
                Debug.LogWarning(newChild.name + " ไม่มีสคริปต์ RangedWeapon!");
            }
        }
    }

    public void nextweaponslotGun1()
    {
        Debug.Log("slot 1 ");
        slotGun1++;
        if (slotGun1 == customePart.Length)
        {
            slotGun1 = 0;
        }
        previview(1);
    }
    public void nextweaponslotGun2()
    {
        Debug.Log("slot 2 ");
        slotGun2++;
        if (slotGun2 == customePart.Length)
        {
            slotGun2 = 0;
        }
        previview(2);
    }
    public void nextweaponslotGun3()
    {
        Debug.Log("slot 3 ");
        slotGun3++;
        if (slotGun3 == customePart.Length)
        {
            slotGun3 = 0;
        }
        previview(3);
    }
    public void nextweaponslotGun4()
    {
        Debug.Log("slot 4 ");
        slotGun4++;
        if (slotGun4 == customePart.Length)
        {
            slotGun4 = 0;
        }
        previview(4);
    }
    public void saveState()
    {
        if (customePart[slotGun1] != null)
        {
            Playstate.SaveToPlaystate(customePart[slotGun1], 1);
        }
        if (customePart[slotGun2] != null)
        {
            Playstate.SaveToPlaystate(customePart[slotGun2], 2);
        }
        if (customePart[slotGun3] != null)
        {
            Playstate.SaveToPlaystate(customePart[slotGun3], 3);
        }
        if (customePart[slotGun4] != null)
        {
            Playstate.SaveToPlaystate(customePart[slotGun4], 4);
        }

        Debug.Log("Player");
        if (Playstate.gunslot1 != null) Debug.Log("gunslot1: " + Playstate.gunslot1.name);
        if (Playstate.gunslot2 != null) Debug.Log("gunslot2: " + Playstate.gunslot2.name);
        if (Playstate.gunslot3 != null) Debug.Log("gunslot3: " + Playstate.gunslot3.name);
        if (Playstate.gunslot4 != null) Debug.Log("gunslot4: " + Playstate.gunslot4.name);
        //if (Playstate.robotType != null) Debug.Log("robotType: " + Playstate.robotType.name);
    }

    void lordSreen()
    {
        SceneManager.LoadScene("GameScene");
    }

}
