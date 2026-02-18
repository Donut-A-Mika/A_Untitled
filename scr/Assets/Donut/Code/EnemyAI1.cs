using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public float detectionRange = 10f; // ระยะที่ศัตรูจะมองเห็นเรา

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // หาตัวผู้เล่นโดยใช้ Tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // คำนวณระยะห่างระหว่างศัตรูกับผู้เล่น
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // สั่งให้เดินตามผู้เล่น
            agent.SetDestination(player.position);
        }
    }
}