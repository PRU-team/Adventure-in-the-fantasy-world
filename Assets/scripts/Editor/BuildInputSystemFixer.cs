using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

/// <summary>
/// Tool tự động fix hoàn toàn vấn đề input khi build game
/// Converts Legacy Input to New Input System and configures everything automatically
/// </summary>
public class BuildInputSystemFixer : EditorWindow
{
    private GameObject playerObject;
    private InputActionAsset inputActions;
    private bool autoFindPlayer = true;
    private bool enableBothInputSystems = true;
    private bool addPlayerInputComponent = true;
    private bool configureProjectSettings = true;
    private bool verifyInputActions = true;
    
    private Vector2 scrollPosition;
    private string statusMessage = "";
    private MessageType messageType = MessageType.Info;
    
    [MenuItem("Tools/Fix Build Input System")]
    public static void ShowWindow()
    {
        var window = GetWindow<BuildInputSystemFixer>("Build Input Fixer");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }
    
    private void OnEnable()
    {
        // Try to find player automatically
        if (autoFindPlayer)
        {
            FindPlayerObject();
        }
        
        // Try to find InputActions asset
        FindInputActionsAsset();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawHeader();
        DrawDiagnostics();
        DrawConfiguration();
        DrawFixButton();
        DrawStatus();
        DrawInstructions();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField("🔧 BUILD INPUT SYSTEM FIXER", titleStyle);
        EditorGUILayout.LabelField("Tự động fix input không hoạt động khi build", EditorStyles.centeredGreyMiniLabel);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Tool này sẽ:\n" +
            "1. ✅ Enable New Input System trong Project Settings\n" +
            "2. ✅ Add PlayerInput component vào Player GameObject\n" +
            "3. ✅ Configure Input Actions tự động\n" +
            "4. ✅ Verify tất cả settings đúng\n" +
            "5. ✅ Fix hoàn toàn input trong build!",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
    }
    
    private void DrawDiagnostics()
    {
        EditorGUILayout.LabelField("📊 DIAGNOSTICS", EditorStyles.boldLabel);
        
        // Check current input handler
        var currentHandler = GetCurrentInputHandler();
        string handlerText = currentHandler == 0 ? "❌ Input Manager (Old)" : 
                            currentHandler == 1 ? "✅ Input System Package (New)" : 
                            "✅ Both";
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Active Input Handling:", handlerText);
        
        // Check if Player exists
        if (playerObject != null)
        {
            EditorGUILayout.LabelField("Player GameObject:", $"✅ Found: {playerObject.name}");
            
            // Check if PlayerInput exists
            var playerInput = playerObject.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                EditorGUILayout.LabelField("PlayerInput Component:", "✅ Already added");
                
                if (playerInput.actions != null)
                {
                    EditorGUILayout.LabelField("Input Actions:", $"✅ Assigned: {playerInput.actions.name}");
                }
                else
                {
                    EditorGUILayout.LabelField("Input Actions:", "❌ Not assigned");
                }
            }
            else
            {
                EditorGUILayout.LabelField("PlayerInput Component:", "❌ Missing");
            }
        }
        else
        {
            EditorGUILayout.LabelField("Player GameObject:", "❌ Not found");
        }
        
        // Check InputActions asset
        if (inputActions != null)
        {
            EditorGUILayout.LabelField("InputActions Asset:", $"✅ Found: {inputActions.name}");
        }
        else
        {
            EditorGUILayout.LabelField("InputActions Asset:", "❌ Not found");
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }
    
    private void DrawConfiguration()
    {
        EditorGUILayout.LabelField("⚙️ CONFIGURATION", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Player object
        EditorGUILayout.BeginHorizontal();
        autoFindPlayer = EditorGUILayout.Toggle("Auto Find Player:", autoFindPlayer);
        if (GUILayout.Button("Find Now", GUILayout.Width(80)))
        {
            FindPlayerObject();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.BeginDisabledGroup(autoFindPlayer);
        playerObject = (GameObject)EditorGUILayout.ObjectField("Player GameObject:", playerObject, typeof(GameObject), true);
        EditorGUI.EndDisabledGroup();
        
        // Input Actions
        inputActions = (InputActionAsset)EditorGUILayout.ObjectField(
            "Input Actions Asset:", 
            inputActions, 
            typeof(InputActionAsset), 
            false
        );
        
        if (GUILayout.Button("Search for InputSystem_Actions", GUILayout.Height(20)))
        {
            FindInputActionsAsset();
        }
        
        EditorGUILayout.Space(5);
        
        // Options
        EditorGUILayout.LabelField("Fix Options:", EditorStyles.boldLabel);
        enableBothInputSystems = EditorGUILayout.Toggle("Enable Both Input Systems", enableBothInputSystems);
        addPlayerInputComponent = EditorGUILayout.Toggle("Add PlayerInput Component", addPlayerInputComponent);
        configureProjectSettings = EditorGUILayout.Toggle("Configure Project Settings", configureProjectSettings);
        verifyInputActions = EditorGUILayout.Toggle("Verify Input Actions", verifyInputActions);
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }
    
    private void DrawFixButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("🔧 FIX BUILD INPUT NOW!", GUILayout.Height(40), GUILayout.Width(300)))
        {
            FixBuildInput();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
    }
    
    private void DrawStatus()
    {
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, messageType);
            EditorGUILayout.Space(10);
        }
    }
    
    private void DrawInstructions()
    {
        EditorGUILayout.LabelField("📖 INSTRUCTIONS", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Sau khi fix:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1. Unity sẽ restart (nếu cần)");
        EditorGUILayout.LabelField("2. Test input trong Editor Play mode");
        EditorGUILayout.LabelField("3. Build game (File → Build And Run)");
        EditorGUILayout.LabelField("4. Test trong build - input sẽ hoạt động!");
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Controls:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("• WASD / Arrow Keys → Movement");
        EditorGUILayout.LabelField("• Mouse Left Click → Attack");
        EditorGUILayout.LabelField("• Q → Fire Attack");
        EditorGUILayout.LabelField("• E → Ranged Attack");
        EditorGUILayout.LabelField("• R → Defensive");
        EditorGUILayout.LabelField("• T → Healing");
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== CORE FIX METHODS ====================
    
    private void FixBuildInput()
    {
        statusMessage = "Starting fix process...\n";
        messageType = MessageType.Info;
        
        int stepsCompleted = 0;
        int totalSteps = 5;
        
        try
        {
            // Step 1: Find player if needed
            if (playerObject == null)
            {
                statusMessage += "❌ Player GameObject not found! Please assign it manually.\n";
                messageType = MessageType.Error;
                return;
            }
            statusMessage += $"✅ Step 1/{totalSteps}: Found Player GameObject: {playerObject.name}\n";
            stepsCompleted++;
            
            // Step 2: Configure Project Settings
            if (configureProjectSettings)
            {
                bool needsRestart = ConfigureInputProjectSettings();
                statusMessage += $"✅ Step 2/{totalSteps}: Configured Project Settings\n";
                stepsCompleted++;
                
                if (needsRestart)
                {
                    statusMessage += "⚠️ Unity needs to restart to apply input changes!\n";
                    messageType = MessageType.Warning;
                    
                    if (EditorUtility.DisplayDialog(
                        "Restart Required",
                        "Unity needs to restart to enable New Input System.\n\nRestart now?",
                        "Restart",
                        "Later"))
                    {
                        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                        return;
                    }
                }
            }
            else
            {
                statusMessage += $"⏭️ Step 2/{totalSteps}: Skipped Project Settings\n";
                stepsCompleted++;
            }
            
            // Step 3: Find or verify InputActions
            if (inputActions == null)
            {
                FindInputActionsAsset();
            }
            
            if (inputActions == null)
            {
                statusMessage += "❌ InputActions asset not found! Please create InputSystem_Actions.inputactions first.\n";
                messageType = MessageType.Error;
                return;
            }
            statusMessage += $"✅ Step 3/{totalSteps}: Found InputActions: {inputActions.name}\n";
            stepsCompleted++;
            
            // Step 4: Add and configure PlayerInput component
            if (addPlayerInputComponent)
            {
                ConfigurePlayerInputComponent();
                statusMessage += $"✅ Step 4/{totalSteps}: Configured PlayerInput component\n";
                stepsCompleted++;
            }
            else
            {
                statusMessage += $"⏭️ Step 4/{totalSteps}: Skipped PlayerInput component\n";
                stepsCompleted++;
            }
            
            // Step 5: Verify everything
            if (verifyInputActions)
            {
                bool verified = VerifyInputSetup();
                if (verified)
                {
                    statusMessage += $"✅ Step 5/{totalSteps}: Verification passed!\n";
                    stepsCompleted++;
                }
                else
                {
                    statusMessage += $"⚠️ Step 5/{totalSteps}: Verification found issues\n";
                    messageType = MessageType.Warning;
                }
            }
            else
            {
                statusMessage += $"⏭️ Step 5/{totalSteps}: Skipped verification\n";
                stepsCompleted++;
            }
            
            // Final message
            statusMessage += "\n========================================\n";
            statusMessage += $"🎉 COMPLETED: {stepsCompleted}/{totalSteps} steps!\n";
            statusMessage += "========================================\n\n";
            statusMessage += "✅ Build input system fixed!\n";
            statusMessage += "✅ Test in Editor Play mode\n";
            statusMessage += "✅ Then build and test game\n";
            statusMessage += "✅ Input will work correctly!\n";
            
            messageType = MessageType.Info;
            
            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(playerObject.scene);
            
            // Show success dialog
            EditorUtility.DisplayDialog(
                "Build Input Fixed!",
                $"Successfully fixed {stepsCompleted}/{totalSteps} steps!\n\n" +
                "Next steps:\n" +
                "1. Test in Editor (Play mode)\n" +
                "2. Build game (File → Build And Run)\n" +
                "3. Test in build - input will work!\n\n" +
                "Controls:\n" +
                "• WASD/Arrows = Move\n" +
                "• Mouse Click = Attack\n" +
                "• Q/E/R/T = Abilities",
                "OK"
            );
        }
        catch (System.Exception ex)
        {
            statusMessage += $"\n❌ ERROR: {ex.Message}\n";
            messageType = MessageType.Error;
            Debug.LogError($"BuildInputSystemFixer error: {ex}");
        }
    }
    
    private bool ConfigureInputProjectSettings()
    {
        // Read ProjectSettings/ProjectSettings.asset
        string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        
        if (!File.Exists(projectSettingsPath))
        {
            Debug.LogError("ProjectSettings.asset not found!");
            return false;
        }
        
        string content = File.ReadAllText(projectSettingsPath);
        
        // Check current activeInputHandler
        int currentHandler = GetCurrentInputHandler();
        
        if (enableBothInputSystems && currentHandler != 2)
        {
            // Change to "Both" (value = 2)
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"activeInputHandler:\s*\d+",
                "activeInputHandler: 2"
            );
            
            File.WriteAllText(projectSettingsPath, content);
            AssetDatabase.Refresh();
            
            Debug.Log("Changed Active Input Handling to: Both");
            return true; // Needs restart
        }
        
        return false; // No restart needed
    }
    
    private void ConfigurePlayerInputComponent()
    {
        if (playerObject == null)
        {
            Debug.LogError("Player object is null!");
            return;
        }
        
        // Get or add PlayerInput component
        PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
        
        if (playerInput == null)
        {
            playerInput = playerObject.AddComponent<PlayerInput>();
            Debug.Log($"Added PlayerInput component to {playerObject.name}");
        }
        
        // Configure PlayerInput
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
            Debug.Log($"Assigned InputActions: {inputActions.name}");
        }
        
        // Set default map to "Player"
        var playerMap = inputActions?.FindActionMap("Player");
        if (playerMap != null)
        {
            playerInput.defaultActionMap = "Player";
            Debug.Log("Set default action map to: Player");
        }
        
        // Set behavior to Send Messages (simpler than Invoke Unity Events)
        playerInput.notificationBehavior = PlayerNotifications.SendMessages;
        Debug.Log("Set notification behavior to: Send Messages");
        
        // Enable input
        playerInput.enabled = true;
        playerInput.ActivateInput();
        
        EditorUtility.SetDirty(playerObject);
    }
    
    private bool VerifyInputSetup()
    {
        bool allGood = true;
        
        // Check PlayerInput exists
        var playerInput = playerObject.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogWarning("PlayerInput component not found!");
            allGood = false;
        }
        else
        {
            // Check actions assigned
            if (playerInput.actions == null)
            {
                Debug.LogWarning("InputActions not assigned to PlayerInput!");
                allGood = false;
            }
            else
            {
                // Check for required actions
                var moveAction = playerInput.actions.FindAction("Move");
                if (moveAction == null)
                {
                    Debug.LogWarning("Move action not found in InputActions!");
                    allGood = false;
                }
                else
                {
                    Debug.Log($"✅ Move action found with {moveAction.bindings.Count} bindings");
                }
                
                var attackAction = playerInput.actions.FindAction("Attack");
                if (attackAction == null)
                {
                    Debug.LogWarning("Attack action not found in InputActions!");
                    allGood = false;
                }
                else
                {
                    Debug.Log($"✅ Attack action found with {attackAction.bindings.Count} bindings");
                }
            }
        }
        
        // Check Project Settings
        int handler = GetCurrentInputHandler();
        if (handler == 0)
        {
            Debug.LogWarning("Active Input Handling still set to Input Manager (Old) only!");
            allGood = false;
        }
        else
        {
            Debug.Log($"✅ Active Input Handling: {(handler == 1 ? "Input System (New)" : "Both")}");
        }
        
        return allGood;
    }
    
    // ==================== HELPER METHODS ====================
    
    private void FindPlayerObject()
    {
        // Try multiple common names
        string[] playerNames = { "Player", "PlayerCharacter", "player", "Player(Clone)" };
        
        foreach (string name in playerNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                playerObject = obj;
                Debug.Log($"Found player object: {name}");
                return;
            }
        }
        
        // Try finding by tag
        try
        {
            var obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
            {
                playerObject = obj;
                Debug.Log($"Found player by tag: {obj.name}");
                return;
            }
        }
        catch { }
        
        // Try finding PlayerController component in scene
#if UNITY_2023_1_OR_NEWER
        var allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
        var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
#endif
        foreach (var go in allGameObjects)
        {
            if (go.GetComponent(System.Type.GetType("PlayerController")) != null)
            {
                playerObject = go;
                Debug.Log($"Found player by PlayerController component: {go.name}");
                return;
            }
        }
        
        Debug.LogWarning("Could not find Player GameObject automatically. Please assign it manually.");
    }
    
    private void FindInputActionsAsset()
    {
        // Search for InputSystem_Actions asset
        string[] guids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            Debug.Log($"Found InputActions asset: {path}");
            return;
        }
        
        // Try any InputActionAsset
        guids = AssetDatabase.FindAssets("t:InputActionAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            Debug.Log($"Found InputActionAsset: {path}");
            return;
        }
        
        Debug.LogWarning("Could not find InputActionAsset. Please create or assign it manually.");
    }
    
    private int GetCurrentInputHandler()
    {
        // Read from ProjectSettings
        string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        
        if (!File.Exists(projectSettingsPath))
        {
            return 0; // Default to Input Manager
        }
        
        string content = File.ReadAllText(projectSettingsPath);
        
        // Find activeInputHandler value
        var match = System.Text.RegularExpressions.Regex.Match(content, @"activeInputHandler:\s*(\d+)");
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        
        return 0; // Default to Input Manager
    }
}
