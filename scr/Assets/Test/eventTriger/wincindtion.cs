using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class wincindtion : MonoBehaviour
{



    [Header("List of Enemies")]
    public List<GameObject> enemies = new List<GameObject>();
    [Header("UI text Disply")]
    public string CountingText = "";
    public string finishCountingText = "";
    public string Sreenloding;

    [Header("UI Reference")]
    public Trigerevent uiSystem; // ลากตัวกลางข้อความมาใส่

    


    private void Start()
    {
        
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
                uiSystem.DisplayNewMessage($"{CountingText}");

            }
            else
            {
                StartLoading(Sreenloding);
            }
            
        }
    }
    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // คำนวณความคืบหน้า (0 ถึง 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log("Loading progress: " + (progress * 100) + "%");

            yield return null; // รอเฟรมถัดไป
        }
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
