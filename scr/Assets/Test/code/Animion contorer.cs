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
        
    }
}
