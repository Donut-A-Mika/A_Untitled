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
    void setUpGun (GameObject gunsoulid , GameObject gunperfab)
    {
        if (true)
        {
            foreach (Transform child in gunsoulid.transform)
            {

                Destroy(child.gameObject);
            }
        }
        if (gunperfab != null)
        {


            GameObject newChild = Instantiate(gunperfab, gunsoulid.transform);

            // รีเซ็ตตำแหน่งให้อยู่ตรงกลางของตัวแม่ (ถ้าต้องการ)
            newChild.transform.localPosition = Vector3.zero;
        }
    }
}
