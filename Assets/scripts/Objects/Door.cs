using System.Collections;
using SaveScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Objects
{
    public class Door : MonoBehaviour
    {
        public bool isOpen;
        public string nextScene = "";
        public Animator crossFade;
        private bool isLoading = false; // Prevent multiple coroutine calls

        private void Awake()
        {
            isOpen = false;
            isLoading = false;
            var spriteChanger = GetComponent<DoorSpriteChanger>();
            if (spriteChanger != null) spriteChanger.closeDoor();
        }

        public void Update()
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount <= 0)
            {
                isOpen = true;
                var spriteChanger = GetComponent<DoorSpriteChanger>();
                if (spriteChanger != null) spriteChanger.openDoor();
            }
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player") && isOpen && !isLoading)
            {
                if (string.IsNullOrEmpty(nextScene))
                {
                    GameObject gameStateObj = GameObject.Find("GameStateController");
                    if (gameStateObj == null)
                    {
                        Debug.LogError("GameStateController not found in scene!");
                        return;
                    }

                    GameStateController gameStateController = gameStateObj.GetComponent<GameStateController>().GetInstance();
                    if (gameStateController == null)
                    {
                        Debug.LogError("GameStateController instance is null!");
                        return;
                    }

                    gameStateController.SaveCombatStats();
                    gameStateController.isTransition = true;
                    gameStateController.nextLevel++;

                    if (gameStateController.levels == null || gameStateController.nextLevel >= gameStateController.levels.Length)
                    {
                        Debug.LogError($"Invalid level index: {gameStateController.nextLevel} / {gameStateController.levels?.Length ?? 0}");
                        return;
                    }

                    nextScene = gameStateController.levels[gameStateController.nextLevel];
                }

                if (!string.IsNullOrEmpty(nextScene))
                {
                    isLoading = true; // Prevent multiple loads
                    StartCoroutine(LoadNextLevel(nextScene));
                }
                else
                {
                    Debug.LogError("Next scene is empty or null!");
                }
            }
        }

        IEnumerator LoadNextLevel(string levelName)
        {
            if (crossFade != null)
            {
                crossFade.SetTrigger("Start");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.LogWarning("CrossFade animator not assigned! Loading scene immediately.");
            }

            if (!string.IsNullOrEmpty(levelName))
            {
                // Try to load by scene name (without path)
                Debug.Log($"Attempting to load scene: {levelName}");

                // Check if scene exists in build settings
                int sceneIndex = SceneManager.GetSceneByName(levelName).buildIndex;
                if (sceneIndex == -1)
                {
                    // Try with different path prefixes based on scene name
                    string scenePath;
                    if (levelName.Contains("Menu") || levelName.Contains("Screen"))
                    {
                        scenePath = "Scenes/Menus/" + levelName;
                    }
                    else
                    {
                        scenePath = "Scenes/Rooms/" + levelName;
                    }
                    Debug.LogWarning($"Scene '{levelName}' not found, trying path: {scenePath}");
                    SceneManager.LoadScene(scenePath);
                }
                else
                {
                    SceneManager.LoadScene(levelName);
                }
            }
            else
            {
                Debug.LogError("Cannot load scene: levelName is null or empty!");
                isLoading = false; // Reset if failed
            }
        }
    }
}