using UnityEngine;
using UnityEngine.InputSystem;

public class attackController : MonoBehaviour
{
    [System.Serializable]
    public class AttackType
    {
        public string name = "Attack";
        public float damage = 10;
        public float cooldown = 0.5f;
        public float range = 1.5f;
        public float knockback = 5f;
        public string animationTrigger = "attack";
        public KeyCode inputKey = KeyCode.Space;
    }

    [Header("Attack Types")]
    public AttackType lightAttack = new AttackType { name = "Light", damage = 5, cooldown = 0.3f, range = 1f, animationTrigger = "attackLight" };
    public AttackType heavyAttack = new AttackType { name = "Heavy", damage = 15, cooldown = 1f, range = 1.5f, knockback = 10f, animationTrigger = "attackHeavy" };
    public AttackType rangeAttack = new AttackType { name = "Range", damage = 8, cooldown = 0.6f, range = 5f, animationTrigger = "attackRange" };

    [Header("Layers")]
    public LayerMask enemyLayer;
    
    private float lastLightAttackTime = 0f;
    private float lastHeavyAttackTime = 0f;
    private float lastRangeAttackTime = 0f;
    
    private Animator animator;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private InputAction rangeAttackAction;
    
    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetupAttackInputs();
    }

    void SetupAttackInputs()
    {
        // Light Attack (Left Mouse / Gamepad X)
        lightAttackAction = new InputAction(type: InputActionType.Button);
        lightAttackAction.AddBinding("<Mouse>/leftButton");
        lightAttackAction.AddBinding("<Gamepad>/buttonNorth"); // Y button
        lightAttackAction.Enable();

        // Heavy Attack (Right Mouse / Gamepad Y)
        heavyAttackAction = new InputAction(type: InputActionType.Button);
        heavyAttackAction.AddBinding("<Mouse>/rightButton");
        heavyAttackAction.AddBinding("<Gamepad>/buttonEast"); // B button
        heavyAttackAction.Enable();

        // Range Attack (Space / Gamepad A)
        rangeAttackAction = new InputAction(type: InputActionType.Button);
        rangeAttackAction.AddBinding("<Keyboard>/space");
        rangeAttackAction.AddBinding("<Gamepad>/buttonSouth"); // A button
        rangeAttackAction.Enable();
    }

    void Update()
    {
        // Check for attack inputs
        if (lightAttackAction.WasPressedThisFrame())
        {
            TryAttack(lightAttack, ref lastLightAttackTime);
        }
        else if (heavyAttackAction.WasPressedThisFrame())
        {
            TryAttack(heavyAttack, ref lastHeavyAttackTime);
        }
        else if (rangeAttackAction.WasPressedThisFrame())
        {
            TryAttack(rangeAttack, ref lastRangeAttackTime);
        }
    }

    public void SetLastDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            lastDirection = direction;
        }
    }

    void TryAttack(AttackType attack, ref float lastAttackTime)
    {
        // Check if attack is off cooldown
        if (Time.time - lastAttackTime < attack.cooldown)
            return;

        lastAttackTime = Time.time;
        PerformAttack(attack);
    }

    void PerformAttack(AttackType attack)
    {
        Debug.Log($"Performing {attack.name} Attack!");

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger(attack.animationTrigger);
        }

        // Detect enemies in attack range
        DetectEnemies(attack);
    }

    void DetectEnemies(AttackType attack)
    {
        // Calculate attack position in front of player
        Vector3 attackPosition = transform.position + (Vector3)lastDirection * (attack.range * 0.5f);

        // Create a circle to detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attack.range,
            enemyLayer
        );

        // Deal damage to all enemies hit
        foreach (Collider2D enemy in hitEnemies)
        {
            // TODO: Call enemy.GetComponent<Enemy>().TakeDamage(attack.damage, lastDirection, attack.knockback);
            Debug.Log($"{attack.name} Hit enemy: {enemy.gameObject.name} for {attack.damage} damage");
        }
    }

    // Draw attack ranges in editor for debugging
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Vector3 attackPos = transform.position + (Vector3)lastDirection * 0.5f;

        // Light Attack (Green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPos, lightAttack.range);

        // Heavy Attack (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, heavyAttack.range);

        // Range Attack (Blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPos, rangeAttack.range);
    }

    void OnDestroy()
    {
        if (lightAttackAction != null)
            lightAttackAction.Dispose();
        if (heavyAttackAction != null)
            heavyAttackAction.Dispose();
        if (rangeAttackAction != null)
            rangeAttackAction.Dispose();
    }
}
