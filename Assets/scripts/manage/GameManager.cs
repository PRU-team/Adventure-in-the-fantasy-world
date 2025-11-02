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
/// Central game manager for a simple RPG: singleton, player stats, XP/leveling, HP/gold management, save/load.
/// Attach to a persistent GameObject in your initial scene (mark DontDestroyOnLoad in Awake).
/// </summary>
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Player")]
	public PlayerStats Player = new PlayerStats();

	// Event fired when player stats change (useful for UI binding)
	public event Action<PlayerStats> OnPlayerStatsChanged;

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
	}

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
		// allow multiple level-ups if XP is large
		while (Player.currentXP >= Player.XPToNextLevel)
		{
			Player.currentXP -= Player.XPToNextLevel;
			Player.level++;
			ApplyLevelBenefits();
		}
	}

	private void ApplyLevelBenefits()
	{
		// simple progression rules - tweak to taste
		Player.maxHP += 10;
		Player.currentHP = Player.maxHP;
		Player.strength += 2;
		Player.defense += 1;
		Debug.Log($"Leveled up! Now level {Player.level}");
	}
	#endregion

	#region HP / Combat Helpers
	public void TakeDamage(int rawDamage)
	{
		int damage = Mathf.Max(0, rawDamage - Player.defense);
		Player.currentHP -= damage;
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
		Debug.Log("Player died. Performing simple respawn.");
		// Simple death handling: restore to full health and penalize gold
		Player.currentHP = Player.maxHP;
		Player.gold = Mathf.Max(0, Player.gold / 2);
		OnPlayerStatsChanged?.Invoke(Player);
		Save();
	}
	#endregion

	#region Economy
	public void ModifyGold(int amount)
	{
		Player.gold += amount;
		if (Player.gold < 0) Player.gold = 0;
		OnPlayerStatsChanged?.Invoke(Player);
		Save();
	}
	#endregion

	#region Save / Load (PlayerPrefs)
	public void Save()
	{
		PlayerPrefs.SetString("player_name", Player.playerName ?? "Hero");
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
