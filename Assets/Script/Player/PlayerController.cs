using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerController tích h?p t?n công, cooldown, attack range, và trigger animation
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayers;
    public int damage = 10;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleAttackInput();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleMovementInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        anim.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    void HandleAttackInput()
    {
        // T?n công th??ng - J
        if (Input.GetKeyDown(KeyCode.J) && CanAttack())
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack01");
        }

        // T?n công ??c bi?t 1 - K
        if (Input.GetKeyDown(KeyCode.K) && CanAttack())
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack02");
        }

        // T?n công ??c bi?t 2 - L
        if (Input.GetKeyDown(KeyCode.L) && CanAttack())
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack03");
        }
    }


    bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    void PerformAttack()
    {
        // L?y t?t c? Enemy trong range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
