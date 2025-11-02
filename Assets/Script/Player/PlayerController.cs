using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 mouseDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovementInput();
        HandleMouseDirection();
        HandleAttackInput();
    }

    void FixedUpdate()
    {
        // Di chuy?n m??t mà v?i Rigidbody2D
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleMovementInput()
    {
        // L?y input tr?c
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Chu?n hóa ?? tránh ?i nhanh khi ?i chéo
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        // C?p nh?t Animator
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        anim.SetFloat("Speed", moveInput.sqrMagnitude); // Quan tr?ng: ?i?u khi?n Idle <-> Walk
    }

    void HandleMouseDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseDir = (mouseWorldPos - transform.position).normalized;

        // L?u h??ng cu?i cùng ?? dùng khi ??ng yên (Idle theo h??ng chu?t)
        anim.SetFloat("LastMoveX", mouseDir.x);
        anim.SetFloat("LastMoveY", mouseDir.y);
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

    // Hàm ki?m tra cooldown
    private bool CanAttack()
    {
        return Time.time - lastAttackTime > attackCooldown;
    }
}