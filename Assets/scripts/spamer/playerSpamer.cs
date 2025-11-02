using UnityEngine;

/// <summary>
/// Player spawner that spawns/respawns the player when they die
/// Can be triggered to enable/disable spawning
/// </summary>
public class playerSpamer : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject playerPrefab; // Player prefab to spawn
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; // Spawn position
    [SerializeField] private float respawnDelay = 2f; // Delay before respawn

    [Header("References")]
    [SerializeField] private playerResourceManager playerResourceManager; // Player resource manager reference

    private GameObject currentPlayer;
    private bool spawningEnabled = true;
    private float deathTime = 0f;
    private bool waitingToRespawn = false;

    private void Start()
    {
        // If no player prefab assigned, try to find existing player in scene
        if (playerPrefab == null)
        {
            currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer == null)
            {
                Debug.LogError("Player prefab not assigned and no Player found in scene!");
            }
        }
        else
        {
            // Spawn initial player
            SpawnPlayer();
        }

        // Find player resource manager if not assigned
        if (playerResourceManager == null)
        {
            playerResourceManager = FindObjectOfType<playerResourceManager>();
            if (playerResourceManager != null)
            {
                playerResourceManager.OnPlayerDeath += OnPlayerDeath;
                Debug.Log("Player Resource Manager found and hooked up");
            }
            else
            {
                Debug.LogWarning("Player Resource Manager not found!");
            }
        }
        else
        {
            playerResourceManager.OnPlayerDeath += OnPlayerDeath;
        }
    }

    private void Update()
    {
        // Check if we should respawn
        if (waitingToRespawn && Time.time - deathTime >= respawnDelay)
        {
            if (spawningEnabled)
            {
                RespawnPlayer();
            }
        }
    }

    /// <summary>
    /// Spawns a new player at the spawn position
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned!");
            return;
        }

        // Instantiate player at spawn position
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"✓ Player spawned at {spawnPosition}");

        // Get player resource manager from spawned player
        if (playerResourceManager == null)
        {
            playerResourceManager = currentPlayer.GetComponent<playerResourceManager>();
            if (playerResourceManager != null)
            {
                playerResourceManager.OnPlayerDeath += OnPlayerDeath;
                Debug.Log("Player Resource Manager found on spawned player");
            }
        }
    }

    /// <summary>
    /// Called when player dies
    /// </summary>
    private void OnPlayerDeath()
    {
        if (!spawningEnabled)
        {
            Debug.Log("Player died but spawning is disabled");
            return;
        }

        Debug.Log($"Player died! Will respawn in {respawnDelay} seconds");
        waitingToRespawn = true;
        deathTime = Time.time;
    }

    /// <summary>
    /// Respawns the player
    /// </summary>
    private void RespawnPlayer()
    {
        Debug.Log("Respawning player...");
        waitingToRespawn = false;

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        SpawnPlayer();

        // Respawn the player resource manager
        if (playerResourceManager != null)
        {
            playerResourceManager.Respawn();
        }
    }

    /// <summary>
    /// Enable or disable player spawning
    /// </summary>
    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
        Debug.Log($"Player spawning {(enabled ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// Toggle spawning on/off
    /// </summary>
    public void ToggleSpawning()
    {
        SetSpawningEnabled(!spawningEnabled);
    }

    /// <summary>
    /// Force respawn the player immediately
    /// </summary>
    public void ForceRespawn()
    {
        Debug.Log("Force respawning player...");
        waitingToRespawn = false;
        RespawnPlayer();
    }

    /// <summary>
    /// Set the respawn position
    /// </summary>
    public void SetSpawnPosition(Vector3 newPosition)
    {
        spawnPosition = newPosition;
        Debug.Log($"Spawn position set to {newPosition}");
    }

    /// <summary>
    /// Check if spawning is currently enabled
    /// </summary>
    public bool IsSpawningEnabled()
    {
        return spawningEnabled;
    }

    /// <summary>
    /// Get current player instance
    /// </summary>
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

    private void OnDestroy()
    {
        // Unsubscribe from death event
        if (playerResourceManager != null)
        {
            playerResourceManager.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
