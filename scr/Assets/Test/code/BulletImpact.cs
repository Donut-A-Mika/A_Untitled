using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public GameObject impactEffect; // เอฟเฟกต์ฝุ่น/ระเบิดตอนชน

    void OnCollisionEnter(Collision collision)
    {
        if (impactEffect != null)
        {
            // สร้างเอฟเฟกต์ตรงจุดที่ชนและหันหน้าไปตามแนวที่ชน (Normal)
            ContactPoint contact = collision.contacts[0];
            GameObject effect = Instantiate(impactEffect, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(effect, 1f);
        }
        Destroy(gameObject); // ทำลายลูกกระสุน
    }
}