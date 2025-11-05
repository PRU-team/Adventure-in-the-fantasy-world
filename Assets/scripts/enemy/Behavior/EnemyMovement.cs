using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 3f; // How fast the enemy chases the player

    [Header("References")]
    private GameObject player;
    private Rigidbody2D rb2d;
    private bool playerInRange = false;

    private void Start()
    {
        // Find the player by tag
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Get the Rigidbody2D component
        rb2d = GetComponent<Rigidbody2D>();
        
        if (player == null)
        {
            Debug.LogError("❌ Player not found! Make sure the player has the 'Player' tag.");
            return;
        }
        
        if (rb2d == null)
        {
            Debug.LogError("❌ Rigidbody2D not found on enemy!");
            return;
        }

        // IMPORTANT: Freeze rotation to prevent spinning

        // Check if player has a collider
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("❌ Player doesn't have a Collider2D!");
        }

        // Check if this enemy has trigger colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        bool hasTrigger = false;
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log("✓ Found trigger collider on enemy");
                break;
            }
        }

        if (!hasTrigger)
        {
            Debug.LogWarning("⚠ No trigger collider found on enemy! Make sure CircleCollider2D has 'Is Trigger' checked");
        }

        Debug.Log("✓ Enemy initialized - Player found: " + player.name);
    }

    private void Update()
    {
        if (player == null || !playerInRange) 
        {
            // Stop moving if player is not in range
            rb2d.linearVelocity = Vector2.zero;
            return;
        }

        ChasePlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("🔔 Trigger detected: " + collision.gameObject.name);
        
        // Check if the player entered the detection circle
        if (collision.gameObject == player)
        {
            playerInRange = true;
            Debug.Log("✓ Player detected! Starting chase...");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("🔔 Trigger exited: " + collision.gameObject.name);
        
        // Check if the player left the detection circle
        if (collision.gameObject == player)
        {
            playerInRange = false;
            Debug.Log("✗ Player left detection range!");
        }
    }

    private void ChasePlayer()
    {
        // Calculate direction towards player
        Vector2 direction = (player.transform.position - transform.position).normalized;

        // Move the enemy towards the player
        rb2d.linearVelocity = direction * chaseSpeed;

        Debug.Log("➡ Chasing player - Direction: " + direction + " Speed: " + chaseSpeed);

        // Optional: Flip the sprite if you want the enemy to face the player
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            FlipSprite();
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        // Flip the sprite by inverting the x scale
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}
