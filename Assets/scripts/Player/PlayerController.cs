using System;
using Character;
using Combat.Player;
using DatabasesScripts;
using Enemy;
using Enums;
using SaveScripts;
using UIScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        // Input values
        private Vector2 moveInput;
        private bool attackInput;
        private bool fireAttackInput;
        private bool rangedAttackInput;
        private bool defensiveInput;
        private bool healingInput;
        private CharacterMovement characterMovement;
        private PlayerDatabaseConn DBConn;
        private CharacterStats characterStats;
        private PlayerAttackController playerAttackController;
        private bool canMove = true;
        private bool m_IsSwimming = false;
        private float m_NextAttack = 0f;
        public HealthBar healthBar;
        private bool isDead = false;
        
        
        private float m_NextFireAttack = 0f;
        public AbilityUICooldownController fireCooldown;
        private float m_NextRangedAttack = 0f;
        public AbilityUICooldownController rangedCooldown;
        private float m_NextDefensiveAbility = 0f;
        public AbilityUICooldownController defensiveCooldown;
        private float m_NextHealingAbility = 0f;
        public AbilityUICooldownController healingCooldown;

        private GameStateController gameStateController;
        
        // Cache for performance optimization
        private Rigidbody2D cachedRigidbody;
        private CharacterAnimationController cachedAnimController;
        
        // Helper method to convert string to KeyCode
        private KeyCode StringToKeyCode(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return KeyCode.None;
            }
            
            try
            {
                // Convert single lowercase character to uppercase for KeyCode enum
                // e.g., "q" -> "Q", "e" -> "E"
                string keyUpper = key.ToUpper();
                
                // Handle special keys
                if (keyUpper == "*")
                {
                    return KeyCode.Asterisk;
                }
                
                // Parse the KeyCode enum
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), keyUpper, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not parse KeyCode from string: '{key}'. Error: {ex.Message}");
                return KeyCode.None;
            }
        }
        
        private void Awake()
        {
            gameStateController = GameObject.Find("GameStateController").GetComponent<GameStateController>().GetInstance();
            
            // Cache components for better performance
            cachedRigidbody = GetComponent<Rigidbody2D>();
            cachedAnimController = GetComponentInChildren<CharacterAnimationController>();
            
            // Reset center of mass to (0, 0)
            cachedRigidbody.centerOfMass = Vector2.zero;
            
            characterMovement = GetComponent<CharacterMovement>();
            characterMovement.SetRigidBody2D(cachedRigidbody);
            characterMovement.SetCharacterAnimationController(cachedAnimController);
            DBConn = new PlayerDatabaseConn();
            characterStats = new CharacterStats(DBConn);
            healthBar.SetMaxHealth(characterStats.GETHealth());
            
            SetUpPlayerAttackController();
            
            if (gameStateController != null && gameStateController.isLoadedFromSave)
            {
                characterStats.SetHealth(gameStateController.playerHealth);
                m_NextFireAttack = Time.time + gameStateController.fireCooldown;
                fireCooldown.StartCoroutine(fireCooldown.CooldownFillTime(gameStateController.fireCooldown));
                m_NextRangedAttack = Time.time + gameStateController.windCooldown;
                rangedCooldown.StartCoroutine(rangedCooldown.CooldownFillTime(gameStateController.windCooldown));
                m_NextDefensiveAbility = Time.time + gameStateController.earthCooldown;
                defensiveCooldown.StartCoroutine(defensiveCooldown.CooldownFillTime(gameStateController.earthCooldown));
                m_NextHealingAbility = Time.time + gameStateController.waterCooldown;
                healingCooldown.StartCoroutine(healingCooldown.CooldownFillTime(gameStateController.waterCooldown));
                healthBar.SetHealth(gameStateController.playerHealth);
            }
            if (gameStateController != null && gameStateController.isTransition)
            {
                characterStats.SetHealth(gameStateController.playerHealth);
                playerAttackController.setFireAttackDamage(gameStateController.fireDamage);
                playerAttackController.setRangedAttackDamage(gameStateController.windDamage);
                playerAttackController.setDefenseDamageReduction(gameStateController.earthDamageReduction);
                playerAttackController.setHealingAmount(gameStateController.waterHealingAmount);
                healthBar.SetHealth(gameStateController.playerHealth);
            }
            isDead = false;
        }

        private void FixedUpdate()
        {
            Move();
            if (!m_IsSwimming)
            {
                UseAttackAbilities();
                UseDefensiveAbilities();
            }
        }

        private void Move()
        {
            float horizontalSpeed = moveInput.x;
            float verticalSpeed = moveInput.y;
            float moveSpeed = characterStats.GETMoveSpeed();
            Vector2 force;
            Direction direction;

            if(canMove){
                // Create movement vector  
                Vector2 movement = new Vector2(horizontalSpeed, verticalSpeed);
                
                // Normalize to prevent faster diagonal movement
                if (movement.magnitude > 1f)
                {
                    movement.Normalize();
                }
                
                force = movement * (moveSpeed * Time.deltaTime);
                direction = characterMovement.GETDirectionFromVector(force);
            }
            else
            {
                force = new Vector2(0f,0f);
                direction = Direction.Idle;
            }

            if (m_IsSwimming)
            {
                force.x = force.x * 0.7f;
                force.y = force.y * 0.7f;
            }
            
            characterMovement.SetCharacterVelocity(force);
            characterMovement.SetCharacterDirection(direction);
            characterMovement.SetIsCharacterSwimming(m_IsSwimming);
        }

        private void SetUpPlayerAttackController()
        {
            playerAttackController = GetComponentInChildren<PlayerAttackController>();
            playerAttackController.SetAttackRange(characterStats.GETAttackRange());
            playerAttackController.SetBasicAttackDamage(characterStats.GETAttackDamage());
            playerAttackController.SetUpFireAttack();
            fireCooldown.SetCooldown(playerAttackController.GETFireAttackCooldown());
            playerAttackController.SetUpRangedAttack();
            rangedCooldown.SetCooldown(playerAttackController.GETRangedAttackCooldown());
            playerAttackController.SetUpDefensiveAbility();
            defensiveCooldown.SetCooldown(playerAttackController.GETDefensiveAbilityCooldown());
            playerAttackController.SetUpHealingAbility();
            healingCooldown.SetCooldown(playerAttackController.GETHealingAbilityCooldown());
        }

        private void UseAttackAbilities()
        {
            if (attackInput && Time.time >= m_NextAttack)
            {
                playerAttackController.Attack();
                m_NextAttack = Time.time + characterStats.GETAttackCooldown();
                attackInput = false; // Reset after use
            }
            else if(fireAttackInput && Time.time >= m_NextFireAttack)
            {
                playerAttackController.FireAttack();
                m_NextFireAttack = Time.time + playerAttackController.GETFireAttackCooldown();
                fireCooldown.StartCoroutine("CooldownFill");
                fireAttackInput = false; // Reset after use
            }
            else if(rangedAttackInput && Time.time >= m_NextRangedAttack)
            {
                playerAttackController.RangedAttack();
                m_NextRangedAttack = Time.time + playerAttackController.GETRangedAttackCooldown();
                rangedCooldown.StartCoroutine("CooldownFill");
                rangedAttackInput = false; // Reset after use
            }
        }

        private void UseDefensiveAbilities()
        {
            if (defensiveInput && Time.time >= m_NextDefensiveAbility)
            {
                playerAttackController.DefensiveAbility();
                m_NextDefensiveAbility = Time.time + playerAttackController.GETDefensiveAbilityCooldown();
                defensiveCooldown.StartCoroutine("CooldownFill");
                defensiveInput = false; // Reset after use
            }
            else if (healingInput && Time.time >= m_NextHealingAbility)
            {
                playerAttackController.Heal();
                m_NextHealingAbility = Time.time + playerAttackController.GETHealingAbilityCooldown();
                healingCooldown.StartCoroutine("CooldownFill");
                healingInput = false; // Reset after use
            }
        }

        public void BuffFireAttackDamage(float damage)
        {
            playerAttackController.BuffFireAttackDamage(damage);
        }

        public void BuffRangedAttackDamage(float damage)
        {
            playerAttackController.BuffRangedAttackDamage(damage);
        }

        public void BuffDefensiveAbilityDamageReduction(float amount)
        {
            playerAttackController.BuffDefensiveDamageReduction(amount);
        }

        public void BuffHealingAbilityAmount(float amount)
        {
            playerAttackController.BuffHealingAbilityAmount(amount);
        }

        public void FreezePosition()
        {
            canMove = false;
            cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void UnfreezePosition()
        {
            canMove = true;
            cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void TakeDamage(float damage)
        {
            if (playerAttackController.IsDefensiveAbilityActive())
            {
                damage -= damage * (playerAttackController.GETDefensiveAbilityDmgReduction() / 100);
            }
            characterStats.TakeDamage(damage);
            healthBar.TakeDamage(damage);
            if (characterStats.GETHealth() <= Mathf.Epsilon && !isDead)
            {
                Debug.Log("Player Died");
                isDead = true;
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in enemies)
                {
                    if(enemy.GetComponent<EnemyController>() != null) enemy.GetComponent<EnemyController>().PlayerIsDead();
                    else enemy.GetComponent<DeathBossController>().PlayerIsDead();
                }
                Destroy(gameObject);
                SceneManager.LoadScene("Scenes/Menus/DeathScreen");
            }
        }

        public void HealPlayer()
        {
            float healingAmount = playerAttackController.GETHealingAmount();
            characterStats.Heal(healingAmount);
            healthBar.Heal(healingAmount);
        }

        public void SetIsSwimming(bool isCharacterSwimming)
        {
            m_IsSwimming = isCharacterSwimming;
        }
        
        public bool GETIsSwimming()
        {
            return m_IsSwimming;
        }

        public float GETPlayerHealth()
        {
            return characterStats.GETHealth();
        }

        public float getFireCooldown()
        {
            return m_NextFireAttack - Time.time;
        }

        public float getFireDamage()
        {
            return playerAttackController.getFireAttackDamage();
        }

        public float getWindCooldown()
        {
            return m_NextRangedAttack - Time.time;
        }

        public float getWindDamage()
        {
            return playerAttackController.getRangedAttackDamage();
        }

        public float getEarthCooldown()
        {
            return m_NextDefensiveAbility - Time.time;
        }

        public float getEarthDamageReduction()
        {
            return playerAttackController.getDefensiveDamageReduction();
        }

        public float getWaterCooldown()
        {
            return m_NextHealingAbility - Time.time;
        }

        public float getWaterHealingAmount()
        {
            return playerAttackController.GETHealingAmount();
        }

        // ===== NEW INPUT SYSTEM CALLBACKS =====
        // These methods are called automatically by PlayerInput component
        
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                attackInput = true;
            }
        }

        // For abilities, map keyboard keys in Unity:
        // Q -> Fire Attack, E -> Ranged Attack, R -> Defensive, T -> Healing
        
        // Fallback for ability keys - you need to map these in Inspector
        private void Update()
        {
            // Check for ability keys (Q, E, R, T)
            // This is fallback in case Input Actions don't have these mapped
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.wasPressedThisFrame)
                {
                    fireAttackInput = true;
                }
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    rangedAttackInput = true;
                }
                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    defensiveInput = true;
                }
                if (Keyboard.current.tKey.wasPressedThisFrame)
                {
                    healingInput = true;
                }
            }
        }
    }
}