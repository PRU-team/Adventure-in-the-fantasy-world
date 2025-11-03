using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enemy spawner that spawns enemy prefabs when the player camera is in range
/// </summary>
public class enemySpamer : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab; // Enemy prefab to spawn
    [SerializeField] private Vector3[] spawnPositions; // Relative spawn positions
    [SerializeField] private float activationRange = 50f; // Range at which spawner activates (INCREASED)
    [SerializeField] private float despawnRange = 100f; // Range at which enemies disappear (INCREASED)
    [SerializeField] private float spawnCooldown = 2f; // Cooldown between spawns
    [SerializeField] private int maxEnemies = 5; // Max enemies at once

    [Header("References")]
    [SerializeField] private Transform playerCamera; // Player's camera transform

    private float lastSpawnTime = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool inRangeLastFrame = false;

    private void Start()
    {
        // Find player camera if not assigned
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
                Debug.Log("Found main camera: " + mainCamera.name);
            }
            else
            {
                Debug.LogError("No main camera found! Assign camera manually in inspector.");
            }
        }

        // Create default spawn positions around spawner if none exist ( no spam lệch y=-1.9)
        if (spawnPositions.Length == 0)
        {
            spawnPositions = new Vector3[] 
            { 
                Vector3.zero,           // Center
                new Vector3(1, 0, 0),   // Right
                new Vector3(-1, 0, 0),  // Left
                new Vector3(0, 1, 0),   // Up
                new Vector3(0, -1, 0)   // Down
            };
            Debug.Log("Created default spawn positions around spawner");
        }

        Debug.Log($"Enemy Spawner initialized. Activation Range: {activationRange}, Despawn Range: {despawnRange}");
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera not found!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerCamera.position);
        
        // Debug distance only when entering/leaving range
        if (distanceToPlayer <= activationRange && !inRangeLastFrame)
        {
            Debug.Log($"✓ Player entered spawn range! Distance: {distanceToPlayer}m, Range: {activationRange}m");
            inRangeLastFrame = true;
        }
        else if (distanceToPlayer > activationRange && inRangeLastFrame)
        {
            Debug.Log($"✗ Player left spawn range! Distance: {distanceToPlayer}m, Range: {activationRange}m");
            inRangeLastFrame = false;
        }

        // Spawn enemies only if player is in range
        if (distanceToPlayer <= activationRange)
        {
            TrySpawnEnemies();
        }

        // Hide/show sprites based on distance
        UpdateEnemyVisibility();
        
        // Clean up dead enemies
        CleanupDeadEnemies();
    }

    /// <summary>
    /// Checks if spawned enemies are out of range and hides/shows their sprite renderer
    /// </summary>
    private void CheckAndDespawnEnemies()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            GameObject enemy = spawnedEnemies[i];
            
            if (enemy == null || !enemy.activeInHierarchy)
                continue;

            // Check distance from player camera
            float distanceFromPlayer = Vector3.Distance(enemy.transform.position, playerCamera.position);
            
            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Hide sprite if out of range, show if in range
                spriteRenderer.enabled = (distanceFromPlayer <= despawnRange);
            }
        }
    }

    /// <summary>
    /// Attempts to spawn enemies based on cooldown and spawn limits
    /// </summary>
    private void TrySpawnEnemies()
    {
        if (Time.time - lastSpawnTime < spawnCooldown)
            return;

        if (spawnedEnemies.Count >= maxEnemies)
        {
            Debug.Log($"Max enemies reached: {spawnedEnemies.Count}/{maxEnemies}");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned! Go to inspector and drag the enemy prefab here.");
            return;
        }

        // Spawn one enemy at a random spawn position
        Vector3 spawnPos = transform.TransformPoint(spawnPositions[Random.Range(0, spawnPositions.Length)]);
        Debug.Log($"Attempting to spawn at position: {spawnPos}");
        
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Enemy instantiated: {newEnemy.name}");
        
        // Ensure sprite renderer is enabled
        SpriteRenderer sr = newEnemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            Debug.Log($"✓ Spawned enemy with visible sprite at {spawnPos}");
        }
        else
        {
            Debug.LogWarning("Enemy prefab has no SpriteRenderer component!");
        }
        
        // Make sure the enemy is active
        newEnemy.SetActive(true);
        Debug.Log($"Enemy SetActive(true): {newEnemy.activeInHierarchy}");
        
        spawnedEnemies.Add(newEnemy);
        lastSpawnTime = Time.time;
        Debug.Log($"✓ SPAWNED! Enemy count: {spawnedEnemies.Count}/{maxEnemies}");
    }

    /// <summary>
    /// Updates visibility of enemies based on distance from camera
    /// </summary>
    private void UpdateEnemyVisibility()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            GameObject enemy = spawnedEnemies[i];
            
            if (enemy == null)
                continue;

            float distanceFromPlayer = Vector3.Distance(enemy.transform.position, playerCamera.position);
            
            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = (distanceFromPlayer <= despawnRange);
            }
        }
    }

    /// <summary>
    /// Removes dead/destroyed enemies from the list
    /// </summary>
    private void CleanupDeadEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Visualize the activation range in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw activation range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Draw spawn positions
        Gizmos.color = Color.green;
        foreach (Vector3 pos in spawnPositions)
        {
            Vector3 worldPosition = transform.TransformPoint(pos);
            Gizmos.DrawWireCube(worldPosition, Vector3.one * 0.5f);
        }
    }
}
