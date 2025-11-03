using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayers;
    public int damage = 10;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;

    private playerResourceManager prm;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        prm = GetComponent<playerResourceManager>();
        if (prm == null)
            Debug.LogError("playerResourceManager missing on Player!");
    }

    void Update()
    {
        HandleMovement();
        HandleAttackInput();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleMovement()
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
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack01");
            DealDamage();
        }
    }

    void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.OnHit(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (prm != null)
        {
            prm.TakeDamage(damageAmount);
            anim.SetTrigger("Hurt");
        }
    }
}