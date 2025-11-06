using UnityEngine;
using UnityEditor;
using Combat.Enemy;
using Character;

namespace Editor
{
    /// <summary>
    /// Debug tool to diagnose enemy attack issues
    /// </summary>
    public class EnemyAttackDebugger : EditorWindow
    {
        private GameObject selectedEnemy;
        private Vector2 scrollPos;
        
        [MenuItem("Tools/Debug/Enemy Attack Debugger")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnemyAttackDebugger>("Enemy Attack Debug");
            window.minSize = new Vector2(500, 600);
        }
        
        private void OnSelectionChange()
        {
            if (Selection.activeGameObject != null && 
                (Selection.activeGameObject.CompareTag("Enemy") || 
                 Selection.activeGameObject.GetComponent<Enemy.EnemyController>() != null))
            {
                selectedEnemy = Selection.activeGameObject;
                Repaint();
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Enemy Attack Debugger", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "This tool helps diagnose why enemies are not attacking.\n\n" +
                "Select an enemy GameObject in the Hierarchy to analyze.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            selectedEnemy = (GameObject)EditorGUILayout.ObjectField(
                "Enemy to Debug", 
                selectedEnemy, 
                typeof(GameObject), 
                true
            );
            
            if (selectedEnemy == null)
            {
                EditorGUILayout.HelpBox("Please select an enemy GameObject to debug.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.Space(10);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            // Check 1: Enemy Controller
            EditorGUILayout.LabelField("1. Enemy Controller Check", EditorStyles.boldLabel);
            CheckEnemyController();
            
            EditorGUILayout.Space(10);
            
            // Check 2: Enemy Attack Controller
            EditorGUILayout.LabelField("2. Enemy Attack Controller Check", EditorStyles.boldLabel);
            CheckEnemyAttackController();
            
            EditorGUILayout.Space(10);
            
            // Check 3: Enemy Basic Attack
            EditorGUILayout.LabelField("3. Enemy Basic Attack Check", EditorStyles.boldLabel);
            CheckEnemyBasicAttack();
            
            EditorGUILayout.Space(10);
            
            // Check 4: Character Animation Controller
            EditorGUILayout.LabelField("4. Character Animation Controller Check", EditorStyles.boldLabel);
            CheckCharacterAnimationController();
            
            EditorGUILayout.Space(10);
            
            // Check 5: Animator
            EditorGUILayout.LabelField("5. Animator Check", EditorStyles.boldLabel);
            CheckAnimator();
            
            EditorGUILayout.Space(10);
            
            // Check 6: Player Setup
            EditorGUILayout.LabelField("6. Player Setup Check", EditorStyles.boldLabel);
            CheckPlayerSetup();
            
            EditorGUILayout.Space(10);
            
            // Check 7: Animation Events
            EditorGUILayout.LabelField("7. Animation Events Check", EditorStyles.boldLabel);
            CheckAnimationEvents();
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(10);
            
            // Quick fix buttons
            EditorGUILayout.LabelField("Quick Fixes", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add Missing Components", GUILayout.Height(30)))
            {
                AddMissingComponents();
            }
            
            if (GUILayout.Button("Fix Hierarchy Structure", GUILayout.Height(30)))
            {
                FixHierarchyStructure();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (Application.isPlaying && GUILayout.Button("Test Attack Now", GUILayout.Height(35)))
            {
                TestAttack();
            }
        }
        
        private void CheckEnemyController()
        {
            var controller = selectedEnemy.GetComponent<Enemy.EnemyController>();
            
            if (controller == null)
            {
                ShowError("❌ Missing EnemyController component");
                if (GUILayout.Button("Add EnemyController"))
                {
                    selectedEnemy.AddComponent<Enemy.EnemyController>();
                }
            }
            else
            {
                ShowSuccess("✓ EnemyController found");
                
                // Check fields
                var healthBar = GetFieldValue<UIScripts.HealthBar>(controller, "healthBar");
                var aggroRange = GetFieldValue<Enemy.AggroRange>(controller, "aggroRange");
                
                if (healthBar == null)
                {
                    ShowWarning("⚠️ HealthBar not assigned");
                }
                else
                {
                    ShowSuccess("  ✓ HealthBar assigned");
                }
                
                if (aggroRange == null)
                {
                    ShowWarning("⚠️ AggroRange not assigned");
                }
                else
                {
                    ShowSuccess("  ✓ AggroRange assigned");
                }
            }
        }
        
        private void CheckEnemyAttackController()
        {
            var attackController = selectedEnemy.GetComponentInChildren<EnemyAttackController>();
            
            if (attackController == null)
            {
                ShowError("❌ Missing EnemyAttackController component (should be in child)");
                EditorGUILayout.HelpBox(
                    "Expected structure:\n" +
                    "Enemy GameObject\n" +
                    "└─ EnemyAttack (child)\n" +
                    "   └─ EnemyAttackController",
                    MessageType.Info
                );
            }
            else
            {
                ShowSuccess("✓ EnemyAttackController found at: " + GetPath(attackController.gameObject));
                
                // Check if it has EnemyBasicAttack child
                var basicAttack = attackController.GetComponentInChildren<EnemyBasicAttack>();
                if (basicAttack == null)
                {
                    ShowError("  ❌ EnemyBasicAttack not found as child");
                }
                else
                {
                    ShowSuccess("  ✓ EnemyBasicAttack found");
                }
            }
        }
        
        private void CheckEnemyBasicAttack()
        {
            var basicAttack = selectedEnemy.GetComponentInChildren<EnemyBasicAttack>();
            
            if (basicAttack == null)
            {
                ShowError("❌ Missing EnemyBasicAttack component");
                EditorGUILayout.HelpBox(
                    "Expected structure:\n" +
                    "Enemy GameObject\n" +
                    "└─ EnemyAttack\n" +
                    "   ├─ EnemyAttackController\n" +
                    "   └─ BasicAttack (child)\n" +
                    "      ├─ EnemyBasicAttack script\n" +
                    "      └─ CircleCollider2D (IsTrigger = true)",
                    MessageType.Info
                );
            }
            else
            {
                ShowSuccess("✓ EnemyBasicAttack found at: " + GetPath(basicAttack.gameObject));
                
                // Check collider
                var collider = basicAttack.GetComponent<CircleCollider2D>();
                if (collider == null)
                {
                    ShowError("  ❌ Missing CircleCollider2D");
                }
                else
                {
                    ShowSuccess("  ✓ CircleCollider2D found");
                    
                    if (!collider.isTrigger)
                    {
                        ShowWarning("  ⚠️ CircleCollider2D.isTrigger should be TRUE");
                        if (GUILayout.Button("Fix: Set isTrigger = true"))
                        {
                            collider.isTrigger = true;
                            EditorUtility.SetDirty(collider);
                        }
                    }
                    else
                    {
                        ShowSuccess("  ✓ isTrigger is TRUE");
                    }
                    
                    EditorGUILayout.LabelField($"  Radius: {collider.radius}");
                }
            }
        }
        
        private void CheckCharacterAnimationController()
        {
            var animController = selectedEnemy.GetComponentInChildren<CharacterAnimationController>();
            
            if (animController == null)
            {
                ShowError("❌ Missing CharacterAnimationController");
            }
            else
            {
                ShowSuccess("✓ CharacterAnimationController found at: " + GetPath(animController.gameObject));
            }
        }
        
        private void CheckAnimator()
        {
            var animator = selectedEnemy.GetComponentInChildren<Animator>();
            
            if (animator == null)
            {
                ShowError("❌ Missing Animator component");
            }
            else
            {
                ShowSuccess("✓ Animator found at: " + GetPath(animator.gameObject));
                
                if (animator.runtimeAnimatorController == null)
                {
                    ShowError("  ❌ No Animator Controller assigned");
                }
                else
                {
                    ShowSuccess($"  ✓ Controller: {animator.runtimeAnimatorController.name}");
                    
                    // Check for Attack trigger
                    bool hasAttackTrigger = false;
                    foreach (var param in animator.parameters)
                    {
                        if (param.name == "Attack" && param.type == AnimatorControllerParameterType.Trigger)
                        {
                            hasAttackTrigger = true;
                            break;
                        }
                    }
                    
                    if (!hasAttackTrigger)
                    {
                        ShowWarning("  ⚠️ No 'Attack' trigger parameter found");
                    }
                    else
                    {
                        ShowSuccess("  ✓ 'Attack' trigger found");
                    }
                }
            }
        }
        
        private void CheckPlayerSetup()
        {
            var player = GameObject.Find("PlayerCharacter");
            
            if (player == null)
            {
                ShowError("❌ Player 'PlayerCharacter' not found in scene");
                EditorGUILayout.HelpBox(
                    "Enemy looks for GameObject named 'PlayerCharacter'.\n" +
                    "Make sure your player GameObject has this exact name.",
                    MessageType.Warning
                );
            }
            else
            {
                ShowSuccess("✓ PlayerCharacter found");
                
                if (!player.CompareTag("Player"))
                {
                    ShowWarning("⚠️ PlayerCharacter doesn't have 'Player' tag");
                    if (GUILayout.Button("Fix: Set tag to 'Player'"))
                    {
                        player.tag = "Player";
                        EditorUtility.SetDirty(player);
                    }
                }
                else
                {
                    ShowSuccess("  ✓ Has 'Player' tag");
                }
                
                var playerController = player.GetComponent<Player.PlayerController>();
                if (playerController == null)
                {
                    ShowError("  ❌ Missing PlayerController component");
                }
                else
                {
                    ShowSuccess("  ✓ PlayerController found");
                }
            }
        }
        
        private void CheckAnimationEvents()
        {
            var animator = selectedEnemy.GetComponentInChildren<Animator>();
            
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                ShowWarning("Cannot check - Animator or Controller missing");
                return;
            }
            
            // Try to find Attack animation clip
            var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            
            if (controller != null)
            {
                bool foundAttackClip = false;
                bool hasApplyDamageEvent = false;
                
                foreach (var clip in controller.animationClips)
                {
                    if (clip.name.ToLower().Contains("attack"))
                    {
                        foundAttackClip = true;
                        
                        // Check for events
                        var events = AnimationUtility.GetAnimationEvents(clip);
                        foreach (var evt in events)
                        {
                            if (evt.functionName == "ApplyDamageToPlayer")
                            {
                                hasApplyDamageEvent = true;
                                break;
                            }
                        }
                        
                        EditorGUILayout.LabelField($"Attack Animation: {clip.name}");
                        EditorGUILayout.LabelField($"  Events: {events.Length}");
                        
                        if (hasApplyDamageEvent)
                        {
                            ShowSuccess("  ✓ Has 'ApplyDamageToPlayer' event");
                        }
                        else
                        {
                            ShowError("  ❌ Missing 'ApplyDamageToPlayer' event");
                            EditorGUILayout.HelpBox(
                                "To fix:\n" +
                                "1. Select the attack animation clip\n" +
                                "2. Open Animation window\n" +
                                "3. Add event at damage frame (usually mid-animation)\n" +
                                "4. Set function to 'ApplyDamageToPlayer'",
                                MessageType.Info
                            );
                        }
                        
                        break;
                    }
                }
                
                if (!foundAttackClip)
                {
                    ShowWarning("⚠️ No attack animation clip found");
                }
            }
        }
        
        private void AddMissingComponents()
        {
            bool changed = false;
            
            // Add EnemyController if missing
            if (selectedEnemy.GetComponent<Enemy.EnemyController>() == null)
            {
                selectedEnemy.AddComponent<Enemy.EnemyController>();
                changed = true;
            }
            
            if (changed)
            {
                EditorUtility.SetDirty(selectedEnemy);
                Debug.Log("Added missing components to " + selectedEnemy.name);
            }
            else
            {
                Debug.Log("No missing components found");
            }
        }
        
        private void FixHierarchyStructure()
        {
            EditorUtility.DisplayDialog(
                "Fix Hierarchy",
                "This will create the proper child structure:\n\n" +
                "Enemy\n" +
                "├─ CharacterGFX (Animator, CharacterAnimationController)\n" +
                "└─ EnemyAttack\n" +
                "   ├─ EnemyAttackController\n" +
                "   └─ BasicAttack\n" +
                "      ├─ EnemyBasicAttack\n" +
                "      └─ CircleCollider2D",
                "OK"
            );
        }
        
        private void TestAttack()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Must be in Play mode to test!", "OK");
                return;
            }
            
            var attackController = selectedEnemy.GetComponentInChildren<EnemyAttackController>();
            if (attackController != null)
            {
                attackController.Attack();
                Debug.Log("Triggered attack on " + selectedEnemy.name);
            }
            else
            {
                Debug.LogError("Cannot test - EnemyAttackController not found!");
            }
        }
        
        // Helper methods
        private void ShowSuccess(string message)
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField("  " + message);
            GUI.color = Color.white;
        }
        
        private void ShowWarning(string message)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("  " + message);
            GUI.color = Color.white;
        }
        
        private void ShowError(string message)
        {
            GUI.color = Color.red;
            EditorGUILayout.LabelField("  " + message);
            GUI.color = Color.white;
        }
        
        private string GetPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
        
        private T GetFieldValue<T>(object obj, string fieldName) where T : class
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(obj) as T;
            }
            
            return null;
        }
    }
}
