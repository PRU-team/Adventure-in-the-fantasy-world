using UnityEngine;

namespace Testing
{
    /// <summary>
    /// Test script to verify that KeyCode conversion is working properly.
    /// This ensures that input will work correctly in both Editor and Build modes.
    /// </summary>
    public class InputSystemTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== Input System Test Started ===");
            TestKeyCodeConversion();
            Debug.Log("=== Input System Test Completed ===");
        }

        private void TestKeyCodeConversion()
        {
            // Test all the ability keycodes
            string[] testKeys = { "Q", "E", "F", "G", "q", "e", "f", "g" };
            
            foreach (string key in testKeys)
            {
                try
                {
                    KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), key, true);
                    Debug.Log($"✓ Key '{key}' successfully converted to KeyCode.{keyCode}");
                }
                catch (System.ArgumentException)
                {
                    Debug.LogError($"✗ Failed to convert key '{key}' to KeyCode");
                }
            }
        }

        private void Update()
        {
            // Real-time input testing
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Fire Attack key (Q) pressed - Input system working!");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Ranged Attack key (E) pressed - Input system working!");
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Defensive Ability key (F) pressed - Input system working!");
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("Healing Ability key (G) pressed - Input system working!");
            }
        }
    }
}
