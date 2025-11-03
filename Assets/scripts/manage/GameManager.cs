using System;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    public string playerName = "Hero";
    public int level = 1;
    public int currentXP = 0;
    public int maxHP = 100;
    public int currentHP = 100;
    public int strength = 10;
    public int defense = 5;
    public int gold = 0;

    public int XPToNextLevel => level * 100;
}

/// <summary>
/// Central Game Manager for a simple RPG.
/// - Manages player stats (HP, XP, level, gold)
/// - Handles leveling, combat damage, saving/loading.
/// - Singleton accessible via GameManager.Instance.
/// Attach this to a persistent GameObject in your initial scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    public PlayerStats Player = new PlayerStats();
    [SerializeField] private Transform playerTransform; // Cached reference for enemies

    // Fired whenever player stats change (for UI or systems)
    public event Action<PlayerStats> OnPlayerStatsChanged;

    #region Singleton Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Load();
        CachePlayerTransform();
    }

    private void CachePlayerTransform()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log($"GameManager: Cached player transform from {playerObj.name}");
            }
        }
    }

    public Transform PlayerTransform => playerTransform;
    #endregion

    #region XP / Leveling
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        Player.currentXP += amount;
        CheckLevelUp();
        OnPlayerStatsChanged?.Invoke(Player);
        Save();
    }

    private void CheckLevelUp()
    {
        while (Player.currentXP >= Player.XPToNextLevel)
        {
            Player.currentXP -= Player.XPToNextLevel;
            Player.level++;
            ApplyLevelBenefits();
        }
    }

    private void ApplyLevelBenefits()
    {
        // Simple progression rules
        Player.maxHP += 10;
        Player.currentHP = Player.maxHP;
        Player.strength += 2;
        Player.defense += 1;

        Debug.Log($"[LEVEL UP] {Player.playerName} reached level {Player.level}!");
    }
    #endregion

    #region HP / Combat
    public void TakeDamage(int rawDamage)
    {
        int damage = Mathf.Max(0, rawDamage - Player.defense);
        Player.currentHP -= damage;

        Debug.Log($"Player took {damage} damage. HP: {Player.currentHP}/{Player.maxHP}");

        if (Player.currentHP <= 0)
        {
            Player.currentHP = 0;
            HandlePlayerDeath();
        }

        OnPlayerStatsChanged?.Invoke(Player);
        Save();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        Player.currentHP = Mathf.Min(Player.maxHP, Player.currentHP + amount);
        OnPlayerStatsChanged?.Invoke(Player);
        Save();
    }

    private void HandlePlayerDeath()
    {
        Debug.LogWarning("💀 Player died. Respawning...");

        // Respawn logic — you can extend this with checkpoints or reload scene
        Player.currentHP = Player.maxHP;
        Player.gold = Mathf.Max(0, Player.gold / 2);

        OnPlayerStatsChanged?.Invoke(Player);
        Save();
    }
    #endregion

    #region Economy
    public void ModifyGold(int amount)
    {
        Player.gold = Mathf.Max(0, Player.gold + amount);
        OnPlayerStatsChanged?.Invoke(Player);
        Save();
    }
    #endregion

    #region Save / Load (PlayerPrefs)
    public void Save()
    {
        PlayerPrefs.SetString("player_name", Player.playerName);
        PlayerPrefs.SetInt("player_level", Player.level);
        PlayerPrefs.SetInt("player_xp", Player.currentXP);
        PlayerPrefs.SetInt("player_maxhp", Player.maxHP);
        PlayerPrefs.SetInt("player_hp", Player.currentHP);
        PlayerPrefs.SetInt("player_str", Player.strength);
        PlayerPrefs.SetInt("player_def", Player.defense);
        PlayerPrefs.SetInt("player_gold", Player.gold);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        Player.playerName = PlayerPrefs.GetString("player_name", Player.playerName);
        Player.level = PlayerPrefs.GetInt("player_level", Player.level);
        Player.currentXP = PlayerPrefs.GetInt("player_xp", Player.currentXP);
        Player.maxHP = PlayerPrefs.GetInt("player_maxhp", Player.maxHP);
        Player.currentHP = PlayerPrefs.GetInt("player_hp", Player.currentHP);
        Player.strength = PlayerPrefs.GetInt("player_str", Player.strength);
        Player.defense = PlayerPrefs.GetInt("player_def", Player.defense);
        Player.gold = PlayerPrefs.GetInt("player_gold", Player.gold);

        OnPlayerStatsChanged?.Invoke(Player);
    }

    public void ResetProgress()
    {
        Player = new PlayerStats();
        Save();
        OnPlayerStatsChanged?.Invoke(Player);
    }
    #endregion
}
