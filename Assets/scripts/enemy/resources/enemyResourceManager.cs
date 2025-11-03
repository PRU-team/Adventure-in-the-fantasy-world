using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Enemy stat configuration - scales with player level
/// </summary>
[System.Serializable]
public class EnemyStats
{
    public string enemyType = "Goblin";
    public int baseHP = 20;
    public int baseDamage = 5;
    public int baseXPReward = 50;
    public int baseGoldReward = 10;
    public float difficultyMultiplier = 1.0f; // Scales with player level

    // Current runtime stats
    [HideInInspector] public int currentHP;

    public EnemyStats Clone()
    {
        return new EnemyStats
        {
            enemyType = this.enemyType,
            baseHP = this.baseHP,
            baseDamage = this.baseDamage,
            baseXPReward = this.baseXPReward,
            baseGoldReward = this.baseGoldReward,
            difficultyMultiplier = this.difficultyMultiplier,
            currentHP = this.baseHP
        };
    }
}

/// <summary>
/// Base enemy component - attached to enemy prefabs
/// </summary>
public class Enemy : MonoBehaviour
{
    public EnemyStats stats;

    public event Action<Enemy> OnEnemyDefeated;

    public void Initialize(EnemyStats newStats)
    {
        stats = newStats.Clone();
        stats.currentHP = stats.baseHP;
    }

    public void TakeDamage(int damage)
    {
        stats.currentHP -= damage;
        Debug.Log($"{stats.enemyType} took {damage} damage. HP: {stats.currentHP}");

        if (stats.currentHP <= 0)
        {
            Defeat();
        }
    }

    public void Defeat()
    {
        Debug.Log($"{stats.enemyType} defeated! XP: {stats.baseXPReward}, Gold: {stats.baseGoldReward}");
        OnEnemyDefeated?.Invoke(this);
        gameObject.SetActive(false);
    }

    public int GetCurrentDamage()
    {
        return Mathf.Max(1, Mathf.RoundToInt(stats.baseDamage * stats.difficultyMultiplier));
    }
}

/// <summary>
/// Central enemy resource manager - object pool + integration with GameManager
/// </summary>
public class enemyResourceManager : MonoBehaviour
{
    [SerializeField] private List<EnemyStats> enemyStatTemplates = new List<EnemyStats>();
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float difficultyScalingFactor = 0.1f; // +10% per player level
    
    private Dictionary<string, Queue<GameObject>> enemyPool = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, EnemyStats> statTemplates = new Dictionary<string, EnemyStats>();
    private static enemyResourceManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeStatTemplates();
        InitializePool();
    }

    /// <summary>
    /// Cache enemy stat templates for quick access
    /// </summary>
    private void InitializeStatTemplates()
    {
        foreach (EnemyStats stats in enemyStatTemplates)
        {
            statTemplates[stats.enemyType] = stats;
        }
    }

    /// <summary>
    /// Initialize the object pool for enemies
    /// </summary>
    private void InitializePool()
    {
        foreach (GameObject prefab in enemyPrefabs)
        {
            string enemyName = prefab.name;
            enemyPool[enemyName] = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject enemy = Instantiate(prefab);
                enemy.SetActive(false);
                enemy.transform.SetParent(transform);
                enemyPool[enemyName].Enqueue(enemy);
            }
        }
    }

    /// <summary>
    /// Calculate difficulty multiplier based on player level
    /// </summary>
    private float CalculateDifficultyMultiplier()
    {
        if (GameManager.Instance == null) return 1.0f;
        
        int playerLevel = GameManager.Instance.Player.level;
        return 1.0f + (playerLevel - 1) * difficultyScalingFactor;
    }

    /// <summary>
    /// Get an enemy from the pool with scaled stats
    /// </summary>
    public GameObject GetEnemy(string enemyType, Vector3 position)
    {
        if (!enemyPool.ContainsKey(enemyType))
        {
            Debug.LogWarning($"Enemy type '{enemyType}' not found in pool");
            return null;
        }

        if (!statTemplates.ContainsKey(enemyType))
        {
            Debug.LogWarning($"Enemy stats for '{enemyType}' not configured");
            return null;
        }

        GameObject enemyGO;

        if (enemyPool[enemyType].Count > 0)
        {
            enemyGO = enemyPool[enemyType].Dequeue();
        }
        else
        {
            // Create new enemy if pool is empty
            GameObject prefab = enemyPrefabs.Find(p => p.name == enemyType);
            enemyGO = Instantiate(prefab);
        }

        // Setup enemy stats and position
        Enemy enemyComponent = enemyGO.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            EnemyStats scaledStats = statTemplates[enemyType].Clone();
            scaledStats.difficultyMultiplier = CalculateDifficultyMultiplier();

            // Scale rewards and HP based on difficulty
            scaledStats.baseHP = Mathf.RoundToInt(scaledStats.baseHP * scaledStats.difficultyMultiplier);
            scaledStats.baseXPReward = Mathf.RoundToInt(scaledStats.baseXPReward * scaledStats.difficultyMultiplier);
            scaledStats.baseGoldReward = Mathf.RoundToInt(scaledStats.baseGoldReward * scaledStats.difficultyMultiplier);

            enemyComponent.Initialize(scaledStats);

            // Subscribe to defeat event for rewards
            enemyComponent.OnEnemyDefeated -= OnEnemyDefeated;
            enemyComponent.OnEnemyDefeated += OnEnemyDefeated;
        }

        enemyGO.transform.position = position;
        enemyGO.SetActive(true);
        return enemyGO;
    }

    /// <summary>
    /// Handle enemy defeat - award XP and gold to player
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddXP(enemy.stats.baseXPReward);
            GameManager.Instance.ModifyGold(enemy.stats.baseGoldReward);
            Debug.Log($"Player gained {enemy.stats.baseXPReward} XP and {enemy.stats.baseGoldReward} gold!");
        }

        // Return to pool after a brief delay
        ReturnEnemyToPool(enemy.gameObject, enemy.stats.enemyType);
    }

    /// <summary>
    /// Return an enemy to the pool
    /// </summary>
    public void ReturnEnemy(string enemyType, GameObject enemy)
    {
        if (!enemyPool.ContainsKey(enemyType))
        {
            Debug.LogWarning($"Enemy type '{enemyType}' not found in pool");
            Destroy(enemy);
            return;
        }

        enemy.SetActive(false);
        enemyPool[enemyType].Enqueue(enemy);
    }

    /// <summary>
    /// Internal method to return enemy to pool
    /// </summary>
    private void ReturnEnemyToPool(GameObject enemy, string enemyType)
    {
        ReturnEnemy(enemyType, enemy);
    }

    /// <summary>
    /// Get the singleton instance
    /// </summary>
    public static enemyResourceManager GetInstance()
    {
        return instance;
    }
}
