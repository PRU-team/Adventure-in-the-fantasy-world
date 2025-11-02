using UnityEngine;
using System;

public class playerResourceManager : MonoBehaviour
{
    [System.Serializable]
    public class Resource
    {
        public float maxValue = 100f;
        public float currentValue;
        public float regenRate = 5f; // Per second
        public float regenDelay = 0.5f; // Delay before regen starts
        
        private float timeSinceLastUsed = 0f;

        public Resource()
        {
            currentValue = maxValue;
            timeSinceLastUsed = regenDelay;
        }

        public void Initialize()
        {
            currentValue = maxValue;
            timeSinceLastUsed = regenDelay;
        }

        public bool TryConsume(float amount)
        {
            if (currentValue >= amount)
            {
                currentValue -= amount;
                timeSinceLastUsed = 0f;
                return true;
            }
            return false;
        }

        public void Regenerate(float deltaTime)
        {
            timeSinceLastUsed += deltaTime;
            
            if (timeSinceLastUsed >= regenDelay && currentValue < maxValue)
            {
                currentValue = Mathf.Min(currentValue + regenRate * deltaTime, maxValue);
            }
        }

        public void Add(float amount)
        {
            currentValue = Mathf.Min(currentValue + amount, maxValue);
        }

        public float GetPercentage()
        {
            return currentValue / maxValue;
        }
    }

    [Header("Health")]
    public Resource health = new Resource { maxValue = 100f, regenRate = 0f };

    [Header("Mana")]
    public Resource mana = new Resource { maxValue = 50f, regenRate = 10f, regenDelay = 1f };

    [Header("Stamina")]
    public Resource stamina = new Resource { maxValue = 100f, regenRate = 15f, regenDelay = 0.5f };

    [Header("Experience")]
    public int experience = 0;
    public int level = 1;
    public int experienceToNextLevel = 100;

    // Events
    public event Action<float> OnHealthChanged;
    public event Action<float> OnManaChanged;
    public event Action<float> OnStaminaChanged;
    public event Action<int> OnLevelUp;
    public event Action OnPlayerDeath;

    private bool isAlive = true;

    void Start()
    {
        health.Initialize();
        mana.Initialize();
        stamina.Initialize();
    }

    void Update()
    {
        if (!isAlive)
            return;

        // Regenerate resources
        mana.Regenerate(Time.deltaTime);
        stamina.Regenerate(Time.deltaTime);

        // Invoke events
        OnHealthChanged?.Invoke(health.GetPercentage());
        OnManaChanged?.Invoke(mana.GetPercentage());
        OnStaminaChanged?.Invoke(stamina.GetPercentage());
    }

    // ===== HEALTH =====
    public void TakeDamage(float damageAmount)
    {
        if (!isAlive)
            return;

        health.currentValue -= damageAmount;
        health.currentValue = Mathf.Max(health.currentValue, 0f);

        Debug.Log($"Player took {damageAmount} damage. Health: {health.currentValue}/{health.maxValue}");

        if (health.currentValue <= 0f)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (!isAlive)
            return;

        health.Add(healAmount);
        Debug.Log($"Player healed for {healAmount}. Health: {health.currentValue}/{health.maxValue}");
    }

    // ===== MANA =====
    public bool TryConsumeMana(float amount)
    {
        if (mana.TryConsume(amount))
        {
            Debug.Log($"Consumed {amount} mana. Mana: {mana.currentValue}/{mana.maxValue}");
            return true;
        }
        Debug.Log($"Not enough mana! Need {amount}, have {mana.currentValue}");
        return false;
    }

    public void RestoreMana(float amount)
    {
        mana.Add(amount);
        Debug.Log($"Restored {amount} mana. Mana: {mana.currentValue}/{mana.maxValue}");
    }

    // ===== STAMINA =====
    public bool TryConsumeStamina(float amount)
    {
        if (stamina.TryConsume(amount))
        {
            Debug.Log($"Consumed {amount} stamina. Stamina: {stamina.currentValue}/{stamina.maxValue}");
            return true;
        }
        Debug.Log($"Not enough stamina! Need {amount}, have {stamina.currentValue}");
        return false;
    }

    public void RestoreStamina(float amount)
    {
        stamina.Add(amount);
        Debug.Log($"Restored {amount} stamina. Stamina: {stamina.currentValue}/{stamina.maxValue}");
    }

    // ===== EXPERIENCE & LEVELING =====
    public void AddExperience(int amount)
    {
        experience += amount;
        Debug.Log($"Gained {amount} experience. Total: {experience}/{experienceToNextLevel}");

        while (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel = (int)(experienceToNextLevel * 1.1f); // Increase requirement by 10%

        // Restore resources on level up
        health.Initialize();
        mana.Initialize();
        stamina.Initialize();

        Debug.Log($"Level Up! You are now level {level}");
        OnLevelUp?.Invoke(level);
    }

    // ===== DEATH =====
    void Die()
    {
        isAlive = false;
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();
    }

    public void Respawn()
    {
        isAlive = true;
        health.Initialize();
        mana.Initialize();
        stamina.Initialize();
        Debug.Log("Player respawned!");
    }

    // ===== GETTERS =====
    public float GetHealthPercentage() => health.GetPercentage();
    public float GetManaPercentage() => mana.GetPercentage();
    public float GetStaminaPercentage() => stamina.GetPercentage();
    public bool IsAlive() => isAlive;
    public float GetCurrentHealth() => health.currentValue;
    public float GetMaxHealth() => health.maxValue;
}
