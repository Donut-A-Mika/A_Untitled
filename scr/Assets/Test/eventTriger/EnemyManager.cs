using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("List of Enemies")]
    public List<GameObject> enemies = new List<GameObject>();
    [Header("UI text Disply")]
    public string CountingText = "";
    public string finishCountingText = "";
    public GameObject Setsenario;

    [Header("UI Reference")]
    public Trigerevent uiSystem; // ลากตัวกลางข้อความมาใส่

    [Header("Door")]
    public GameObject Door;
    private Animator anim;
    [SerializeField] private bool door;

    private bool ative = false;


    private void Start()
    {
        anim = Door.GetComponent<Animator>();
    }
    /// <summary>
    /// ฟังก์ชันสำหรับนับศัตรูที่ยังไม่ถูกทำลาย
    /// </summary>
    public int GetRemainingEnemyCount()
    {
        // ลบข้อมูลใน List ที่กลายเป็น Null (ถูกทำลายไปแล้ว) ออกให้หมด
        enemies.RemoveAll(item => item == null);

        // คืนค่าจำนวนที่เหลืออยู่จริงๆ
        return enemies.Count;
    }
    private void FixedUpdate()
    {
        if (ative)
        {


            UpdateUIWithCount();
        }
    }

    /// <summary>
    /// สั่งให้แสดงผลจำนวนศัตรูไปที่ UI
    /// </summary>
    public void UpdateUIWithCount()
    {
        int count = GetRemainingEnemyCount();

        if (uiSystem != null)
        {
            if (count > 0)
            {
                uiSystem.DisplayNewMessage($"{CountingText} {count} ");
                
            }
            else
            {
                uiSystem.DisplayNewMessage(finishCountingText);

                door = true;
                if (Setsenario)
                {
                    Setsenario.SetActive(true);
                }
            }
            anim.SetBool("isOpen", door);
        }
        ative = true;
    }

    // ตัวอย่าง: ถ้าอยากให้เช็คทุกครั้งที่เดินเข้าพื้นที่ (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateUIWithCount();
        }
    }
}