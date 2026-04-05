using UnityEngine;

public class setmucis : MonoBehaviour
{
    public GameObject Refobj;
    private GameObject Target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    // Update is called once per frame
    void Update()
    {
        if (Refobj.activeSelf)
        {
           gameObject.SetActive(false);
        }
    }
}
