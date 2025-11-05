using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DatabasesScripts
{
    [System.Serializable]
    public class Character
    {
        public int characterId;
        public string characterName;
        public float characterMoveSpeed;
        public float characterHealth;
        public float characterAttackDamage;
        public float characterAttackRange;
        public float characterAttackCooldown;
    }

    [System.Serializable]
    public class CharacterData
    {
        public Character[] characters;
    }

    public class PlayerDatabaseConn
    {
        private CharacterData characterData;
        private Character playerCharacter;
        private int playerCharacterId;
        
        public PlayerDatabaseConn()
        {
            // Load character data from JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Characters");
            characterData = JsonUtility.FromJson<CharacterData>(jsonFile.text);
            
            // Find the PlayerCharacter
            foreach (Character character in characterData.characters)
            {
                if (character.characterName == "PlayerCharacter")
                {
                    playerCharacter = character;
                    playerCharacterId = character.characterId;
                    break;
                }
            }
            
            if (playerCharacter == null)
            {
                Debug.LogError("PlayerCharacter not found in Characters.json");
            }
        }

        public void SetMoveSpeed(float newMoveSpeed)
        {
            if (playerCharacter != null)
            {
                playerCharacter.characterMoveSpeed = newMoveSpeed;
            }
        }

        public int GETPlayerCharacterId()
        {
            return playerCharacterId;
        }

        public float GETPlayerMoveSpeed()
        {
            if (playerCharacter != null)
            {
                return playerCharacter.characterMoveSpeed;
            }
            return 0f;
        }

        public string GETDbPath()
        {
            return "Resources/Characters.json";
        }

        public float GETPlayerHealth()
        {
            if (playerCharacter != null)
            {
                return playerCharacter.characterHealth;
            }
            return 0f;
        }

        public float GETPlayerAttackDamage()
        {
            if (playerCharacter != null)
            {
                return playerCharacter.characterAttackDamage;
            }
            return 0f;
        }
        
        public float GETPlayerAttackRange()
        {
            if (playerCharacter != null)
            {
                return playerCharacter.characterAttackRange;
            }
            return 0f;
        }
        
        public float GETPlayerAttackCooldown()
        {
            if (playerCharacter != null)
            {
                return playerCharacter.characterAttackCooldown;
            }
            return 0f;
        }
    }
}