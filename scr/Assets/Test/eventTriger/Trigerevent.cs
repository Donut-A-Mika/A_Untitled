using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class Trigerevent : MonoBehaviour
{
    [Header("Display Target")]
    [Tooltip("ลาก TextMeshPro ที่ต้องการให้แสดงผลมาใส่ตรงนี้ (หรือปล่อยว่างไว้ถ้าสคริปต์อื่นจะเป็นคนจัดการ)")]
    public TMP_Text textDisplay;
    public AudioClip texteffet = null;    
    private AudioSource texteffetAudioSource;

    [Header("Self-Trigger Settings")]
    [Tooltip("ถ้าติ๊กถูก: เมื่อ Player มาชน จะใช้ข้อความข้างล่างนี้แสดงทันที")]
    public bool actAsTrigger = true;
    public string myMessage = "ยินดีต้อนรับสู่โซนอันตราย!";

    [Header("Behavior Settings")]
    public bool clearOnExit = true;
    public bool OneTimeTrigger = false;


    /// <summary>
    /// ฟังก์ชันกลาง: สำหรับรับข้อความจากที่อื่น หรือส่งข้อความจากตัวเอง
    /// </summary>
    /// 
    private void Start()
    {
        texteffetAudioSource =  GetComponent<AudioSource>();
    }
    public void DisplayNewMessage(string message)
    {
        if (textDisplay != null)
        {
            textDisplay.text = message;
            Debug.Log($"[Trigerevent] Displaying: {message}");
        }
        else
        {
            Debug.LogWarning("ยังไม่ได้ใส่ textDisplay ใน Inspector!");
        }
    }

    // --- ส่วนของการทำงานเป็น Trigger เอง ---
    private void OnTriggerEnter(Collider other)
    {
        if (actAsTrigger && other.CompareTag("Player"))
        {
            // ส่งข้อความที่ตั้งไว้ในตัวแปร myMessage ของตัวเอง
            DisplayNewMessage(myMessage);
        }
        if (texteffet)
        {
            texteffetAudioSource.PlayOneShot(texteffet);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (clearOnExit && other.CompareTag("Player"))
        {
            DisplayNewMessage(""); // ล้างข้อความเมื่อเดินออก
        }
        if (OneTimeTrigger)
        {
            Destroy(gameObject);
        }
    }
}