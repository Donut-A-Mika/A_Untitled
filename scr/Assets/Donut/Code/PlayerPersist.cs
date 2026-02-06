using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

    void Awake()
    {
        // ถ้ามี Instance อยู่แล้ว ทำลายตัวนี้ทิ้ง
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // เก็บ Instance และไม่ให้หายเมื่อเปลี่ยน Scene
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe event เมื่อเปลี่ยน Scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // ยกเลิก Subscribe เมื่อถูกทำลาย
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ถ้าเข้า GameScene (หรือ Test Scene)
        if (scene.name == "Test" || scene.name == "GameScene")
        {
            SetupPlayerInGame();
        }
    }

    void SetupPlayerInGame()
    {
        // หา Spawn Point ในเกม (ถ้ามี)
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
        }
        else
        {
            // ถ้าไม่มี Spawn Point ให้วางที่ตำแหน่งเริ่มต้น
            transform.position = new Vector3(0, 1, 0);
        }

        // เปิดใช้งาน Component ที่ต้องการในเกม
        EnableGameplayComponents();
    }

    void EnableGameplayComponents()
    {
        // เปิด PlayerController
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        // ปิด Customization Script (ไม่ต้องใช้ในเกม)
        Customization custom = GetComponent<Customization>();
        if (custom != null) custom.enabled = false;
    }
}