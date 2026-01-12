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
        // 1. àªç¤¡èÍ¹ÇèÒ Parameter ·ÕèÊè§ÁÒÁÕ¤èÒäËÁ (»éÍ§¡Ñ¹ Error)
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
            Debug.LogWarning("¢éÍÁÙÅäÁè¤Ãº: ¡ÃØ³ÒãÊè·Ñé§¨Ø´ÇÒ§»×¹áÅÐ Prefab »×¹");
            return;
        }

        // 2. ÅºÅÙ¡·ÕèÁÕÍÂÙèà´ÔÁÍÍ¡ãËéËÁ´
        foreach (Transform child in gunsoulid.transform)
        {
            // á¹Ð¹ÓãËéãªé Destroy à©Âæ ã¹ Play Mode
            Destroy(child.gameObject);
        }

        // 3. ÊÃéÒ§»×¹ãËÁèà¢éÒä»à»ç¹ÅÙ¡
        GameObject newChild = Instantiate(gunperfab, gunsoulid.transform);

        // 4. ÃÕà«çµµÓáË¹è§áÅÐÁØÁËÁØ¹ãËéµÃ§µÒÁµÑÇáÁè
        newChild.transform.localPosition = Vector3.zero;
        //newChild.transform.localScale = Vector3.one;
        //newChild.transform.localRotation = Quaternion.identity; // à¾ÔèÁºÃÃ·Ñ´¹Õéà¾×èÍãËé»×¹äÁèàÍÕÂ§
    }
}
