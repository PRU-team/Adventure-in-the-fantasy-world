using System;
using UnityEngine;

namespace DatabasesScripts
{
    public class EnemyDatabaseConn
    {
        private CharacterData characterData;
        private Character enemyCharacter;
        private int enemyCharacterId;

        public EnemyDatabaseConn(string enemyName)
        {
            // Load character data from JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Characters");
            characterData = JsonUtility.FromJson<CharacterData>(jsonFile.text);
            
            // Find the enemy character by name
            foreach (Character character in characterData.characters)
            {
                if (character.characterName == enemyName)
                {
                    enemyCharacter = character;
                    enemyCharacterId = character.characterId;
                    break;
                }
            }
            
            if (enemyCharacter == null)
            {
                Debug.LogError("Enemy '" + enemyName + "' not found in Characters.json");
            }
        }

        public int GETEnemyCharacterId()
        {
            return enemyCharacterId;
        }

        public float GETEnemyMoveSpeed()
        {
            if (enemyCharacter != null)
            {
                return enemyCharacter.characterMoveSpeed;
            }
            return 0f;
        }
        
        public void SetMoveSpeed(float newMoveSpeed)
        {
            if (enemyCharacter != null)
            {
                enemyCharacter.characterMoveSpeed = newMoveSpeed;
            }
        }
        
        public float GETEnemyHealth()
        {
            if (enemyCharacter != null)
            {
                return enemyCharacter.characterHealth;
            }
            return 0f;
        }

        public float GETEnemyAttackDamage()
        {
            if (enemyCharacter != null)
            {
                return enemyCharacter.characterAttackDamage;
            }
            return 0f;
        }
        
        public float GETEnemyAttackRange()
        {
            if (enemyCharacter != null)
            {
                return enemyCharacter.characterAttackRange;
            }
            return 0f;
        }
        
        public float GETEnemyAttackCooldown()
        {
            if (enemyCharacter != null)
            {
                return enemyCharacter.characterAttackCooldown;
            }
            return 0f;
        }
    }
}