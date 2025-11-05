using System;
using Enums;
using UnityEngine;

namespace DatabasesScripts
{
    [System.Serializable]
    public class Ability
    {
        public int abilityId;
        public string abilityName;
        public string abilityTypeName;
        public float abilityCooldown;
        public float attackDamage;
        public float abilityRange;
        public float projectileSpeed;
        public int damageReduction;
        public float abilityDuration;
        public float healingAmount;
        public string abilityKeyCode;
    }

    [System.Serializable]
    public class AbilitiesData
    {
        public Ability[] abilities;
    }

    public class AbilitiesDatabaseConn
    {
        private AbilitiesData abilitiesData;
        private Ability ability;
        private string abilityName = "";

        public AbilitiesDatabaseConn(string abilityName)
        {
            this.abilityName = abilityName;
            
            // Load abilities data from JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Abilities");
            abilitiesData = JsonUtility.FromJson<AbilitiesData>(jsonFile.text);
            
            // Find the ability by name
            foreach (Ability a in abilitiesData.abilities)
            {
                if (a.abilityName == abilityName)
                {
                    ability = a;
                    break;
                }
            }
            
            if (ability == null)
            {
                Debug.LogError("Ability '" + abilityName + "' not found in Abilities.json");
            }
        }

        public AbilityType GETAbilityType()
        {
            if (ability != null)
            {
                return AbilityTypeFromString(ability.abilityTypeName);
            }
            return AbilityType.NotFound;
        }

        public AbilityType AbilityTypeFromString(string abilityTypeName)
        {
            switch (abilityTypeName)
            {
                case "MeleeAttack":
                    return AbilityType.MeleeAttack;
                case "RangedAttack":
                    return AbilityType.RangedAttack;
                case "DefensiveAbility":
                    return AbilityType.DefensiveAbility;
                case "HealingAbility":
                    return AbilityType.HealingAbility;
            }

            return AbilityType.NotFound;
        }

        public string GETAbiltyName()
        {
            return abilityName;
        }

        public float GETAbilityCooldown()
        {
            if (ability != null)
            {
                return ability.abilityCooldown;
            }
            return 0f;
        }
        
        public float GETAbilityAttackDamage()
        {
            if (ability != null)
            {
                return ability.attackDamage;
            }
            return 0f;
        }

        public float GETAbilityAttackRange()
        {
            if (ability != null)
            {
                return ability.abilityRange;
            }
            return 0f;
        }
        
        public float GETProjectileSpeed()
        {
            if (ability != null)
            {
                return ability.projectileSpeed;
            }
            return 0f;
        }
        
        public int GETAbilityDamageReduction()
        {
            if (ability != null)
            {
                return ability.damageReduction;
            }
            return 0;
        }
        
        public float GETAbilityDuration()
        {
            if (ability != null)
            {
                return ability.abilityDuration;
            }
            return 0f;
        }
        
        public float GETAbilityHealingAmount()
        {
            if (ability != null)
            {
                return ability.healingAmount;
            }
            return 0f;
        }

        public string GETAbilityKeyCode()
        {
            if (ability != null)
            {
                return ability.abilityKeyCode;
            }
            return "";
        }
    }
}