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

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
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

        // Dash/Sprint system
        private bool isDashing = false;
        private float dashDuration = 0.5f; // Duration in seconds
        private float dashSpeedMultiplier = 2f; // Speed multiplier during dash
        private float dashCooldown = 2f; // Cooldown between dashes
        private float nextDashTime = 0f;
        private AudioSource audioSource;
        // God Mode Cheat Code System
        private bool isGodMode = false;
        private int cheatCodeStep = 0; // Tracks current step in cheat sequence
        // Cheat sequence: Spacebar -> T -> U -> H -> T
        private readonly KeyCode[] cheatSequence = { KeyCode.Space, KeyCode.T, KeyCode.U, KeyCode.H, KeyCode.T };

        private GameStateController gameStateController;
        [Header("Player Audio Clips")]
        public AudioClip attackClip;
        public AudioClip dashClip;
        public AudioClip hurtClip;
        public AudioClip deathClip;
        [Header("Player Audio Clips")]
        public AudioClip stepClip;
        private float stepCooldown = 0.3f; // thời gian giữa hai bước chân
        private float nextStepTime = 0f;
        /// <summary>
        /// Safely converts a string key name to Unity's KeyCode enum.
        /// Handles both uppercase and lowercase inputs with error handling.
        /// </summary>
        private KeyCode StringToKeyCode(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                Debug.LogWarning("KeyCode string is null or empty. Returning KeyCode.None.");
                return KeyCode.None;
            }

            try
            {
                // Try to parse the string to KeyCode enum (case-insensitive)
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString, true);
            }
            catch (System.ArgumentException)
            {
                Debug.LogError($"Invalid KeyCode string: '{keyString}'. Returning KeyCode.None.");
                return KeyCode.None;
            }
        }
        
        private void Awake()
        {
            gameStateController = GameObject.Find("GameStateController").GetComponent<GameStateController>().GetInstance();
            
            characterMovement = GetComponent<CharacterMovement>();
            characterMovement.SetRigidBody2D(GetComponent<Rigidbody2D>());
            characterMovement.SetCharacterAnimationController(GetComponentInChildren<CharacterAnimationController>());
            DBConn = new PlayerDatabaseConn();
            characterStats = new CharacterStats(DBConn);
            healthBar.SetMaxHealth(characterStats.GETHealth());
            
            // Ensure player can move at start
            canMove = true;
            Debug.Log($"PlayerController Awake - canMove set to: {canMove}");
            
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
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            isDead = false;
        }

        private void Update()
        {
            // Check for dash input
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                TryDash();
            }
            
            // Check for cheat code input
            CheckCheatCode();
            
            // Input should be read in Update, not FixedUpdate, to avoid missing input events
            if (!m_IsSwimming)
            {
                UseAttackAbilities();
                UseDefensiveAbilities();
            }
        }

        private void FixedUpdate()
        {
            // Physics updates should remain in FixedUpdate
            Move();
        }

        private void Move()
        {
            float horizontalSpeed = Input.GetAxisRaw("Horizontal");
            float verticalSpeed = Input.GetAxisRaw("Vertical");
            float moveSpeed = characterStats.GETMoveSpeed();
            
            // Apply dash speed multiplier if dashing
            if (isDashing)
            {
                moveSpeed *= dashSpeedMultiplier;
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Debug logging to verify input is being received
            if (horizontalSpeed != 0 || verticalSpeed != 0)
            {
                Debug.Log($"Input detected - H: {horizontalSpeed}, V: {verticalSpeed}, CanMove: {canMove}, Dashing: {isDashing}");
            }
            #endif
            Vector2 force;
            Direction direction;

            if(canMove){
                force = new Vector2(horizontalSpeed, verticalSpeed) * (moveSpeed * Time.deltaTime);
                direction = characterMovement.GETDirectionFromVector(force);
            }
            else
            {
                force = new Vector2(0f,0f);
                direction = Direction.Idle;
            }
            if (canMove && (horizontalSpeed != 0 || verticalSpeed != 0))
            {
                if (Time.time >= nextStepTime && stepClip != null)
                {
                    audioSource.PlayOneShot(stepClip);
                    nextStepTime = Time.time + stepCooldown;
                }
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

        /// <summary>
        /// Attempts to activate dash if cooldown has expired.
        /// Dash increases movement speed by dashSpeedMultiplier for dashDuration seconds.
        /// </summary>
        private void TryDash()
        {
            if (Time.time >= nextDashTime && canMove && !isDashing)
            {
                StartDash();
            }
        }

        /// <summary>
        /// Starts the dash effect and schedules it to stop after dashDuration.
        /// </summary>
        private void StartDash()
        {
            isDashing = true;
            nextDashTime = Time.time + dashCooldown;
            
            Debug.Log($"Dash started! Speed multiplier: {dashSpeedMultiplier}x for {dashDuration}s");
            if (dashClip != null) audioSource.PlayOneShot(dashClip);
            // Schedule dash to stop after dashDuration
            Invoke(nameof(StopDash), dashDuration);
        }

        /// <summary>
        /// Stops the dash effect, returning movement speed to normal.
        /// </summary>
        private void StopDash()
        {
            isDashing = false;
            Debug.Log("Dash ended. Speed returned to normal.");
        }

        /// <summary>
        /// Checks for cheat code input sequence: Spacebar -> T -> U -> H -> T
        /// If any wrong key is pressed, resets the sequence.
        /// </summary>
        private void CheckCheatCode()
        {
            // Check if any key was pressed this frame
            if (Input.anyKeyDown)
            {
                // Check if the correct key in the sequence was pressed
                if (cheatCodeStep < cheatSequence.Length && Input.GetKeyDown(cheatSequence[cheatCodeStep]))
                {
                    cheatCodeStep++;
                    Debug.Log($"Cheat code progress: {cheatCodeStep}/{cheatSequence.Length}");
                    
                    // If sequence is complete, activate god mode
                    if (cheatCodeStep >= cheatSequence.Length)
                    {
                        ActivateGodMode();
                        cheatCodeStep = 0; // Reset for next use
                    }
                }
                else
                {
                    // Wrong key pressed or key pressed when not in sequence
                    // Only reset if we were actually in the middle of entering the code
                    bool wrongKeyInSequence = false;
                    
                    // Check if any key other than the expected one was pressed
                    for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
                    {
                        if (Input.GetKeyDown(key) && key != cheatSequence[cheatCodeStep])
                        {
                            wrongKeyInSequence = true;
                            break;
                        }
                    }
                    
                    if (Input.GetKeyDown(KeyCode.Space) && cheatSequence[cheatCodeStep] != KeyCode.Space)
                    {
                        wrongKeyInSequence = true;
                    }
                    
                    if (wrongKeyInSequence && cheatCodeStep > 0)
                    {
                        Debug.Log("Wrong key! Cheat code reset.");
                        cheatCodeStep = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Toggles god mode on/off. When active, player takes no damage.
        /// </summary>
        private void ActivateGodMode()
        {
            isGodMode = !isGodMode;
            
            if (isGodMode)
            {
                Debug.Log("🛡️ GOD MODE ACTIVATED! Player is now invincible!");
                // Optional: Add visual feedback here (e.g., glow effect, particle system)
            }
            else
            {
                Debug.Log("GOD MODE DEACTIVATED. Player can take damage again.");
            }
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
            if (Input.GetMouseButton(0) && Time.time >= m_NextAttack)
            {
                playerAttackController.Attack();
                audioSource.PlayOneShot(attackClip);
                m_NextAttack = Time.time + characterStats.GETAttackCooldown();
            }
            else if(Input.GetKey(StringToKeyCode(playerAttackController.GETFireAttackKeyCode())) && Time.time >= m_NextFireAttack)
            {
                playerAttackController.FireAttack();
                m_NextFireAttack = Time.time + playerAttackController.GETFireAttackCooldown();
                fireCooldown.StartCoroutine("CooldownFill");
            }
            else if(Input.GetKey(StringToKeyCode(playerAttackController.GETRangedAttackKeyCode())) && Time.time >= m_NextRangedAttack)
            {
                playerAttackController.RangedAttack();
                m_NextRangedAttack = Time.time + playerAttackController.GETRangedAttackCooldown();
                rangedCooldown.StartCoroutine("CooldownFill");
            }
        }

        private void UseDefensiveAbilities()
        {
            if (Input.GetKey(StringToKeyCode(playerAttackController.GETDefensiveAbilityKeyCode())) && Time.time >= m_NextDefensiveAbility)
            {
                playerAttackController.DefensiveAbility();
                m_NextDefensiveAbility = Time.time + playerAttackController.GETDefensiveAbilityCooldown();
                defensiveCooldown.StartCoroutine("CooldownFill");
            }
            else if (Input.GetKey(StringToKeyCode(playerAttackController.GETHealingAbilityKeyCode())) && Time.time >= m_NextHealingAbility)
            {
                playerAttackController.Heal();
                m_NextHealingAbility = Time.time + playerAttackController.GETHealingAbilityCooldown();
                healingCooldown.StartCoroutine("CooldownFill");
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
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            Debug.Log("Player FROZEN - canMove: false");
        }

        public void UnfreezePosition()
        {
            canMove = true;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("Player UNFROZEN - canMove: true");
        }

        public void TakeDamage(float damage)
        {
            // God mode check - ignore all damage if active
            if (isGodMode)
            {
                Debug.Log($"🛡️ God Mode: Blocked {damage} damage!");
                return;
            }
            
            if (playerAttackController.IsDefensiveAbilityActive())
            {
                damage -= damage * (playerAttackController.GETDefensiveAbilityDmgReduction() / 100);
            }
            characterStats.TakeDamage(damage);
            healthBar.TakeDamage(damage);
            if (hurtClip != null) audioSource.PlayOneShot(hurtClip);
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
                if (deathClip != null) audioSource.PlayOneShot(deathClip);
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
    }
}