
using UnityEngine;
using UnityEngine.InputSystem;

public class playerControler : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down; // Default direction for sprite
    private Rigidbody2D rb;
    private Animator animator;
    public InputAction moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f; // Prevent sliding due to gravity in top-down games
        // Setup InputAction if not assigned in Inspector
        if (moveAction == null)
        {
            moveAction = new InputAction(type: InputActionType.Value, binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.Enable();
        }
        else
        {
            moveAction.Enable();
        }
    }

    void Update()
    {
        // Get input for movement from new Input System
        movement = moveAction.ReadValue<Vector2>().normalized;
        
        // Track last direction for animations
        if (movement != Vector2.zero)
        {
            lastDirection = movement;
        }
        
        // Update animator parameters
        if (animator != null)
        {
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
            animator.SetBool("isMoving", movement != Vector2.zero);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}
