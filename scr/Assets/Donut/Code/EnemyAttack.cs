using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.0f;
    public Animator anim;

    private float nextAttackTime;
    private Transform player;
    private Health playerHealth;

    // ⭐ ตัวควบคุมคิวโจมตี
    public static EnemyAttack currentAttacker = null;
    public static bool isSomeoneAttacking = false;

    private bool isMyTurn = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ถ้ายังไม่มีใครตี ให้ตัวนี้เป็นคนตี
        if (currentAttacker == null && distanceToPlayer <= attackRange)
        {
            currentAttacker = this;
            isMyTurn = true;
        }

        // ถ้าเป็นเทิร์นของตัวนี้
        if (isMyTurn && currentAttacker == this)
        {
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                Attack();

                nextAttackTime = Time.time + attackCooldown;

                // ⭐ ส่งเทิร์นให้ตัวอื่น
                isMyTurn = false;
                currentAttacker = null;
            }
        }
        else
        {
            anim.SetBool("isAttack", false);
        }
    }

    void Attack()
    {
        if (isSomeoneAttacking) return;
        anim.SetBool("isAttack", true);
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isSomeoneAttacking = true;

        Debug.Log(name + " โจมตีผู้เล่น!");
        playerHealth.TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown);

        isSomeoneAttacking = false;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}