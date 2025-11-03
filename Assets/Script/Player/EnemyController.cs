using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class EnemyController : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseSightRange = 12f;
[Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 5;

    [Header("References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Animator animator;

    private Enemy enemy;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private bool isChasing = false;
    private bool isDead = false;
    private Vector3 startPosition;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (animator == null) animator = GetComponent<Animator>();
        startPosition = transform.position;

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTarget = playerObj.transform;
        }

        damage = enemy != null ? enemy.GetCurrentDamage() : damage;

        enemy.OnEnemyDefeated += HandleDeath;
    }

    private void Update()
    {
        if (isDead || playerTarget == null) return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (!isChasing && distance <= detectionRange)
            isChasing = true;
        else if (isChasing && distance > loseSightRange)
        {
            isChasing = false;
            StopAllCoroutines();
        }

        if (isChasing)
            HandleChase(distance);
        else
            ReturnToStart();
    }

    private void HandleChase(float distance)
    {
        if (distance > attackRange)
            MoveToward(playerTarget.position);
        else
            TryAttack();
    }

    private void ReturnToStart()
    {
        float distToStart = Vector3.Distance(transform.position, startPosition);
        if (distToStart > 0.1f)
            MoveToward(startPosition);
        else if (animator != null)
            animator.SetBool("isMoving", false);
    }
    private void MoveToward(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown || isAttacking) return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetBool("isMoving", false);

        if (animator != null)
            animator.SetTrigger("Attack01");

        // Delay trước khi gây sát thương  
        yield return new WaitForSeconds(0.3f);

        // Trừ HP player qua GameManager  
        if (GameManager.Instance != null)
            GameManager.Instance.TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown - 0.3f);
        isAttacking = false;
    }

    public void OnHit(int damageTaken)
    {
        if (isDead) return;
        if (animator != null) animator.SetTrigger("Hurt");
    }

    private void HandleDeath(Enemy e)
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger("Death");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DieAfterDelay());
    }

    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
    }

    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }  

}
