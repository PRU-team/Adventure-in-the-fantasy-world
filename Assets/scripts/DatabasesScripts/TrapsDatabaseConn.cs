using UnityEngine;

namespace DatabasesScripts
{
    [System.Serializable]
    public class Trap
    {
        public int trapId;
        public string trapName;
        public float trapCooldown;
        public float trapDuration;
        public float trapDamage;
        public float trapRange;
        public float trapProjectileSpeed;
    }

    [System.Serializable]
    public class TrapsData
    {
        public Trap[] traps;
    }

    public class TrapsDatabaseConn
    {
        private TrapsData trapsData;
        private Trap trap;

        public TrapsDatabaseConn(string trapName)
        {
            // Load traps data from JSON
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Traps");
            trapsData = JsonUtility.FromJson<TrapsData>(jsonFile.text);
            
            // Find the trap by name
            foreach (Trap t in trapsData.traps)
            {
                if (t.trapName == trapName)
                {
                    trap = t;
                    break;
                }
            }
            
            if (trap == null)
            {
                Debug.LogError("Trap '" + trapName + "' not found in Traps.json");
            }
        }

        public int GETTrapId()
        {
            if (trap != null)
            {
                return trap.trapId;
            }
            return 0;
        }

        public float GETTrapCooldown()
        {
            if (trap != null)
            {
                return trap.trapCooldown;
            }
            return 0f;
        }
        
        public float GETTrapDuration()
        {
            if (trap != null)
            {
                return trap.trapDuration;
            }
            return 0f;
        }
        
        public float GETTrapDamage()
        {
            if (trap != null)
            {
                return trap.trapDamage;
            }
            return 0f;
        }
        
        public float GETTrapRange()
        {
            if (trap != null)
            {
                return trap.trapRange;
            }
            return 0f;
        }
        
        public float GETTrapProjectileSpeed()
        {
            if (trap != null)
            {
                return trap.trapProjectileSpeed;
            }
            return 0f;
        }
    }
}