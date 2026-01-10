using NUnit.Framework.Internal;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Customization : MonoBehaviour
{

    public GameObject[] customePart;

    public GameObject Gun1;
    public GameObject Gun2;
    public GameObject Gun3;
    public GameObject Gun4;

    

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
        switch (slot)
        {
            case 1:
                Debug.Log("switch slotGun1 now ", customePart[slotGun1]);
                if (true)
                { 
                    foreach (Transform child in Gun1.transform)
                    {

                        Destroy(child.gameObject);
                    }
                }
                if (customePart[slotGun1] != null)
                {


                    GameObject newChild = Instantiate(customePart[slotGun1], Gun1.transform);

                    // รีเซ็ตตำแหน่งให้อยู่ตรงกลางของตัวแม่ (ถ้าต้องการ)
                    newChild.transform.localPosition = Vector3.zero;
                }
                break;
            case 2:
                Debug.Log("switch slotGun2", customePart[slotGun2]);
                if (true)
                {
                    foreach (Transform child in Gun2.transform)
                    {

                        Destroy(child.gameObject);
                    }
                }
                if (customePart[slotGun2] != null)
                {


                    GameObject newChild = Instantiate(customePart[slotGun2], Gun2.transform);

                    // รีเซ็ตตำแหน่งให้อยู่ตรงกลางของตัวแม่ (ถ้าต้องการ)
                    newChild.transform.localPosition = Vector3.zero;
                }
                break;
            case 3:
                Debug.Log("switch slotGun3", customePart[slotGun3]);
                if (true)
                {
                    foreach (Transform child in Gun3.transform)
                    {

                        Destroy(child.gameObject);
                    }
                }
                if (customePart[slotGun3] != null)
                {


                    GameObject newChild = Instantiate(customePart[slotGun3], Gun3.transform);

                    // รีเซ็ตตำแหน่งให้อยู่ตรงกลางของตัวแม่ (ถ้าต้องการ)
                    newChild.transform.localPosition = Vector3.zero;
                }
                break;
            case 4:
                Debug.Log("switch slotGun4", customePart[slotGun4]);
                if (true)
                {
                    foreach (Transform child in Gun4.transform)
                    {

                        Destroy(child.gameObject);
                    }
                }
                if (customePart[slotGun4] != null)
                {


                    GameObject newChild = Instantiate(customePart[slotGun4], Gun4.transform);

                    // รีเซ็ตตำแหน่งให้อยู่ตรงกลางของตัวแม่ (ถ้าต้องการ)
                    newChild.transform.localPosition = Vector3.zero;
                }
                break;
            default:
                Debug.Log("Default case (no match)");
                break;
        }
    }

    public void nextweaponslotGun1 ()
    {
        Debug.Log("slot 1 ");
        slotGun1++;
        if (slotGun1 == customePart.Length )
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
        SceneManager.LoadScene("Test");
    }

}
