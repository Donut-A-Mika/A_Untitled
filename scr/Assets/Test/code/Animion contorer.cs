using UnityEngine;

public class Animioncontorer : MonoBehaviour
{
    public GameObject Player;
    private Animator animatorPlayer;
    private Rigidbody playerRigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animatorPlayer = GetComponent<Animator>();
        if (Player != null)
        {
            playerRigidbody = Player.GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRigidbody != null)
        {
            // �Ҥ������� (Magnitude ��͢�Ҵ�ͧ Vector ��������)
            float speed = playerRigidbody.linearVelocity.magnitude;

            // �觤�Ҥ����������� Animator (��������� Animator ��駪��� Parameter ��� "Speed")
            if (speed >= 5)
            {
                animatorPlayer.SetBool("Isrun", true);
            }
            else
            {
                animatorPlayer.SetBool("Isrun", false);
            }
            
        }
    }
}
