using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Uimanage : MonoBehaviour
{
    public GameObject Player;
    public WeaponManager weaponManager; // ลาก WeaponManager มาใส่ที่นี่
    public TextMeshProUGUI HP;
    public Slider HPSlider;
    public Image fill;
    public Gradient gradient;
    public int MaxHp;
    [Header("Dash")]

    public Slider dashSlider;       // ลาก Slider UI มาใส่
    public Image fillImage;         // (Optional) ลากส่วน Fill ของ Slider มาใส่เพื่อเปลี่ยนสี


    [Header("UI Slots")]
    public RawImage activeWeaponSlot; // ช่องแสดงอาวุธที่ถืออยู่
    public RawImage backupWeaponSlot; // ช่องแสดงอาวุธที่เก็บไว้

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;
    public float blinkSpeed = 10f; // ความเร็วในการกระพริบ

    private Health hpComponent;
    private PlayerController playerCl;

    void Start()
    {
        SetMaxHP();
        playerCl = Player.GetComponent<PlayerController>();
        if (Player != null)
            hpComponent = Player.GetComponent<Health>();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateWeaponUI();
        UpdateAmmoUI(); 
        sliderHP();
        dashSlider.value = playerCl.GetDashCooldownRemaining();
    }

    void UpdateHealthUI()
    {
        if (hpComponent != null)
        {
           /* float currentHP = hpComponent.GetCurrentHealth();
            HP.text = "HP: " + currentHP.ToString("F0");*/
        }
    }

    void UpdateWeaponUI()
    {
        if (weaponManager == null) return;

        // ดึงข้อมูลอาวุธจาก WeaponManager (ใช้ Array ที่เราเก็บปืนไว้)
        GameObject gun1 = weaponManager.GetWeaponFromSlot(1);
        GameObject gun2 = weaponManager.GetWeaponFromSlot(2);

        // เช็คว่าตอนนี้ถือ Slot ไหนอยู่
        int activeSlot = weaponManager.GetCurrentSlotIndex();

        if (activeSlot == 1)
        {
            // ถ้าถือปืน 1: ช่อง Active = ปืน 1, ช่อง Backup = ปืน 2
            SetWeaponIcon(activeWeaponSlot, gun1);
            SetWeaponIcon(backupWeaponSlot, gun2);
        }
        else
        {
            // ถ้าถือปืน 2: ช่อง Active = ปืน 2, ช่อง Backup = ปืน 1
            SetWeaponIcon(activeWeaponSlot, gun2);
            SetWeaponIcon(backupWeaponSlot, gun1);
        }
    }
    void UpdateAmmoUI()
    {
        if (weaponManager == null || ammoText == null) return;

        GameObject activeWeapon = weaponManager.currentWeapon;

        if (activeWeapon != null)
        {
            RangedWeapon rw = activeWeapon.GetComponent<RangedWeapon>();

            if (rw != null)
            {
                // 1. แสดงตัวเลขกระสุน
                ammoText.text = rw.GetCurrentAmmo() + " / " + rw.GetMaxAmmo();

                // 2. ระบบกระพริบตอน Reload
                if (rw.IsReloading())
                {
                    // ใช้การเปิด-ปิด TextMeshPro ตามจังหวะเวลา
                    // Mathf.Repeat จะคืนค่า 0 ถึง 1 วนไปเรื่อยๆ 
                    // ถ้าค่าน้อยกว่า 0.5 ให้ปิด ถ้ามากกว่าให้เปิด (ทำให้เกิดการกระพริบ)
                    ammoText.enabled = (Mathf.Repeat(Time.time * blinkSpeed, 1f) > 0.5f);
                }
                else
                {
                    // ถ้าไม่ได้รีโหลด ต้องมั่นใจว่า Text เปิดอยู่ตลอด
                    ammoText.enabled = true;
                }
                if (rw.IsReloading())
                {
                    // สร้างค่า Sin Wave จาก 0 ถึง 1
                    float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
                    ammoText.alpha = alpha; // ปรับความโปร่งใสของ Text
                }
                else
                {
                    ammoText.alpha = 1f; // กลับมาสว่างเต็มร้อย
                }
            }
        }

    }
    public void sliderHP ()
    {
        if (hpComponent != null)
        {
            HPSlider.value = hpComponent.GetCurrentHealth();
            fill.color = gradient.Evaluate(HPSlider.normalizedValue);
        }
    }
    public void SetMaxHP ()
    {
        if (hpComponent != null)
        {
            HPSlider.maxValue = MaxHp;
            HPSlider.value = hpComponent.GetCurrentHealth();
            fill.color = gradient.Evaluate(1f);
        }
    }    

    // ฟังก์ชันช่วยดึงรูป Icon จากสคริปต์อาวุธ
    void SetWeaponIcon(RawImage targetImage, GameObject weaponObj)
    {
        if (weaponObj == null)
        {
            targetImage.enabled = false; // ถ้าไม่มีปืนให้ปิดภาพ
            return;
        }

        targetImage.enabled = true;

        // ดึง Icon จาก RangedWeapon หรือ MeleeWeapon (คุณต้องไปเพิ่มตัวแปร public Texture weaponIcon ในสคริปต์เหล่านั้นด้วย)
        RangedWeapon rw = weaponObj.GetComponent<RangedWeapon>();
        MeleeWeapon mw = weaponObj.GetComponent<MeleeWeapon>();

        if (rw != null) targetImage.texture = rw.weaponIcon;
        else if (mw != null) targetImage.texture = mw.weaponIcon;
    }
}