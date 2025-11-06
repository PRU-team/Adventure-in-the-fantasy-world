using System;
using Combat.Enemy;
using Combat.Player;
using Enemy;
using UnityEngine;
using Enums;
using Player;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Character
{
    public class CharacterAnimationController : MonoBehaviour
    {
        private float horizontalSpeed = 0f;
        private float verticalSpeed = 0f;
        private bool isIdle = true;
        private Animator characterGfx;
        private SpriteRenderer spriteRenderer;
        
        [Header("Sprite Flip Settings")]
        [Tooltip("Enable automatic sprite flipping when moving left/right")]
        public bool enableAutoFlip = true;
        
        [Tooltip("If true, sprite faces right by default. If false, sprite faces left by default.")]
        public bool defaultFacingRight = true;

        public void Awake()
        {
            characterGfx = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // If no SpriteRenderer on this object, try to find it in children
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            // Set animator to always update (helps with animation loops)
            characterGfx.keepAnimatorStateOnDisable = true;
        }

        public void LateUpdate()
        {
            // Force CharacterGFX to stay at (0, 0, 0) relative to parent
            // This prevents animation from moving the sprite
            if (transform.localPosition != Vector3.zero)
            {
                Debug.Log($"CharacterGFX position was {transform.localPosition}, resetting to zero");
                transform.localPosition = Vector3.zero;
            }
        }

        public string GetCurrentAnimation()
        {
            return characterGfx.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        }

        public void ChangeDirection(Direction direction){
            switch (direction)
            {
                case Direction.Down:
                    horizontalSpeed = 0f;
                    verticalSpeed = -1f;
                    isIdle = false;
                    break;
                case Direction.Up:
                    horizontalSpeed = 0f;
                    verticalSpeed = 1f;
                    isIdle = false;
                    break;
                case Direction.Right:
                    horizontalSpeed = 1f;
                    verticalSpeed = 0f;
                    isIdle = false;
                    break;
                case Direction.Left:
                    horizontalSpeed = -1f;
                    verticalSpeed = 0f;
                    isIdle = false;
                    break;
                default:
                    horizontalSpeed = 0f;
                    verticalSpeed = 0f;
                    isIdle = true;
                    break;
            }
        
            // Auto-flip sprite based on horizontal direction
            if (enableAutoFlip && spriteRenderer != null)
            {
                FlipSprite(horizontalSpeed);
            }
        
            characterGfx.SetFloat("HorizontalSpeed", horizontalSpeed);
            characterGfx.SetFloat("VerticalSpeed", verticalSpeed);
            characterGfx.SetBool("isIdle", isIdle);
        }
        
        /// <summary>
        /// Flips the sprite based on horizontal movement direction
        /// </summary>
        /// <param name="horizontalDirection">Positive = right, Negative = left, Zero = no change</param>
        private void FlipSprite(float horizontalDirection)
        {
            if (horizontalDirection > 0) // Moving right
            {
                spriteRenderer.flipX = !defaultFacingRight;
            }
            else if (horizontalDirection < 0) // Moving left
            {
                spriteRenderer.flipX = defaultFacingRight;
            }
            // If horizontalDirection == 0, keep current facing direction
        }

        public void CharacterSwim(bool isSwimming)
        {
            characterGfx.SetBool("isSwimming", isSwimming);
        }

        public void StartAttack()
        {
            characterGfx.SetTrigger("Attack");
        }

        public void StartFireAttack()
        {
            characterGfx.SetTrigger("FireAttack");
        }

        public void StartRangedAttack()
        {
            characterGfx.SetTrigger("RangedAttack");
        }

        public void StartDefensiveAbility()
        {
            characterGfx.SetTrigger("DefensiveAbility");
        }

        public void StartHealingAbility()
        {
            characterGfx.SetTrigger("HealingAbility");
        }

        public void TakeHit()
        {
            characterGfx.SetTrigger("Hit");
        }

        public void CharacterDeath()
        {
            characterGfx.SetTrigger("Death");
        }

        public void ApplyDamageToEnemy()
        {
            transform.parent.gameObject.GetComponentInChildren<PlayerAttackController>().ApplyDamage("BasicAttack");
        }

        public void ApplyFireDamageToEnemy()
        {
            transform.parent.gameObject.GetComponentInChildren<PlayerAttackController>().ApplyDamage("FireAttack");
        }

        public void ApplyDamageToPlayer()
        {
            transform.parent.gameObject.GetComponentInChildren<EnemyAttackController>().ApplyDamage();
        }

        public void HealPlayer()
        {
            transform.parent.gameObject.GetComponent<PlayerController>().HealPlayer();
        }

        public void FreezePlayerPosition()
        {
            GetComponentInParent<PlayerController>().FreezePosition();
        }
        
        public void FreezeEnemyPosition()
        {
            GetComponentInParent<EnemyController>().FreezePosition();
        }
        
        public void FreezeEnemyBossPosition()
        {
            GetComponentInParent<DeathBossController>().FreezePosition();
        }

        public void UnfreezePlayerPosition()
        {
            GetComponentInParent<PlayerController>().UnfreezePosition();
        }
        
        public void UnfreezeEnemyPosition()
        {
            GetComponentInParent<EnemyController>().UnfreezePosition();
        }
        
        public void UnfreezeEnemyBossPosition()
        {
            GetComponentInParent<DeathBossController>().UnfreezePosition();
        }

        public void ResetScale()
        {
            GetComponentInParent<EnemyController>().ResetScale();
        }

        public void DestroyObject()
        {
            Destroy(transform.parent.gameObject);
        }

        public void DestroyBoss()
        {
            transform.parent.gameObject.GetComponent<DeathBossController>().Death();
        }

        public void BossAttack(string trigger)
        {
            characterGfx.SetTrigger(trigger);
        }

        public void Summon()
        {
            GetComponentInParent<DeathBossController>().Summon();
        }
    }
}
