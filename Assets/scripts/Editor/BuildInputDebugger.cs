using UnityEngine;
using UnityEditor;

/// <summary>
/// Build Input Debugger
/// Kiểm tra và fix vấn đề input không hoạt động khi build game
/// 
/// Sử dụng: Tools → Debug → Build Input Debugger
/// </summary>
public class BuildInputDebugger : EditorWindow
{
    private Vector2 scrollPos;
    private string debugLog = "";
    
    [MenuItem("Tools/Debug/Build Input Debugger")]
    public static void ShowWindow()
    {
        GetWindow<BuildInputDebugger>("Build Input Debugger");
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.LabelField("BUILD INPUT DEBUGGER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tool này kiểm tra tại sao input không hoạt động khi build game.\n\n" +
            "Các vấn đề thường gặp:\n" +
            "• Input Manager chưa được config\n" +
            "• Cursor bị lock\n" +
            "• Run in Background disabled\n" +
            "• Input System package conflicts",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("🔍 CHECK ALL ISSUES", GUILayout.Height(40)))
        {
            CheckAll();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Fix Input Axes"))
        {
            FixInputAxes();
        }
        if (GUILayout.Button("Fix Build Settings"))
        {
            FixBuildSettings();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(debugLog))
        {
            EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(debugLog, GUILayout.Height(400));
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CheckAll()
    {
        debugLog = "=== BUILD INPUT DEBUG ===\n\n";
        
        // 1. Check Input Manager axes
        debugLog += "1. INPUT MANAGER AXES:\n";
        SerializedObject inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
        SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");
        
        bool hasHorizontal = false;
        bool hasVertical = false;
        bool hasMouse0 = false;
        
        for (int i = 0; i < axesProperty.arraySize; i++)
        {
            SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
            string name = axis.FindPropertyRelative("m_Name").stringValue;
            
            if (name == "Horizontal") hasHorizontal = true;
            if (name == "Vertical") hasVertical = true;
            if (name == "Fire1") hasMouse0 = true;
        }
        
        if (hasHorizontal) debugLog += "   ✅ Horizontal axis found\n";
        else debugLog += "   ❌ CRITICAL: Missing Horizontal axis!\n";
        
        if (hasVertical) debugLog += "   ✅ Vertical axis found\n";
        else debugLog += "   ❌ CRITICAL: Missing Vertical axis!\n";
        
        if (hasMouse0) debugLog += "   ✅ Fire1 (Mouse) axis found\n";
        else debugLog += "   ❌ CRITICAL: Missing Fire1 axis!\n";
        
        debugLog += "\n";
        
        // 2. Check Player Settings
        debugLog += "2. PLAYER SETTINGS:\n";
        
        // Run in Background
        if (PlayerSettings.runInBackground)
        {
            debugLog += "   ✅ Run In Background: ENABLED\n";
        }
        else
        {
            debugLog += "   ❌ WARNING: Run In Background: DISABLED\n";
            debugLog += "      (Game may not receive input when not focused)\n";
        }
        
        // Fullscreen Mode
        debugLog += $"   Fullscreen Mode: {PlayerSettings.fullScreenMode}\n";
        
        // Default Screen Width/Height
        debugLog += $"   Default Resolution: {PlayerSettings.defaultScreenWidth}x{PlayerSettings.defaultScreenHeight}\n";
        
        debugLog += "\n";
        
        // 3. Check Active Input Handling
        debugLog += "3. INPUT SYSTEM:\n";
        
        #if ENABLE_INPUT_SYSTEM
        debugLog += "   ✅ New Input System: ENABLED\n";
        #else
        debugLog += "   ℹ️  New Input System: NOT ENABLED\n";
        #endif
        
        #if ENABLE_LEGACY_INPUT_MANAGER
        debugLog += "   ✅ Legacy Input Manager: ENABLED\n";
        #else
        debugLog += "   ❌ Legacy Input Manager: DISABLED\n";
        #endif
        
        // Check if using Input.GetAxisRaw (old system)
        debugLog += "   ℹ️  PlayerController uses Input.GetAxisRaw() (Legacy)\n";
        debugLog += "   → Requires Legacy Input Manager to be enabled\n";
        
        debugLog += "\n";
        
        // 4. Check Build Settings
        debugLog += "4. BUILD SETTINGS:\n";
        
        var scenes = EditorBuildSettings.scenes;
        debugLog += $"   Scenes in Build: {scenes.Length}\n";
        
        int enabledScenes = 0;
        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes++;
                debugLog += $"      ✅ {System.IO.Path.GetFileNameWithoutExtension(scene.path)}\n";
            }
        }
        
        if (enabledScenes == 0)
        {
            debugLog += "   ❌ CRITICAL: No scenes enabled in build!\n";
        }
        
        debugLog += "\n";
        
        // 5. Check Cursor State
        debugLog += "5. CURSOR & FOCUS:\n";
        debugLog += "   ℹ️  Check these at runtime:\n";
        debugLog += "      - Cursor.lockState (should be CursorLockMode.None for normal gameplay)\n";
        debugLog += "      - Cursor.visible (should be true)\n";
        debugLog += "      - Application.isFocused (should be true when window active)\n";
        
        debugLog += "\n";
        
        // Summary
        debugLog += "=== SUMMARY ===\n";
        
        int criticalIssues = 0;
        if (!hasHorizontal || !hasVertical) criticalIssues++;
        if (enabledScenes == 0) criticalIssues++;
        
        #if !ENABLE_LEGACY_INPUT_MANAGER
        criticalIssues++;
        debugLog += "❌ CRITICAL: Legacy Input Manager is DISABLED!\n";
        debugLog += "   Your code uses Input.GetAxisRaw() which requires Legacy Input Manager.\n\n";
        #endif
        
        if (criticalIssues > 0)
        {
            debugLog += $"\n❌ FOUND {criticalIssues} CRITICAL ISSUES!\n\n";
            debugLog += "FIX STEPS:\n";
            debugLog += "1. Click 'Fix Input Axes' to add missing axes\n";
            debugLog += "2. Click 'Fix Build Settings' to enable run in background\n";
            debugLog += "3. Go to: Edit → Project Settings → Player → Active Input Handling\n";
            debugLog += "   → Set to 'Both' or 'Input Manager (Old)'\n";
            debugLog += "4. Add scenes to build: File → Build Settings → Add Open Scenes\n";
            debugLog += "5. Rebuild and test\n";
        }
        else
        {
            debugLog += "✅ NO CRITICAL ISSUES FOUND!\n\n";
            debugLog += "If input still doesn't work in build:\n";
            debugLog += "1. Check console for errors in build\n";
            debugLog += "2. Test if PlayerController script is active\n";
            debugLog += "3. Check if cursor is locked\n";
            debugLog += "4. Verify scene is properly loaded\n";
        }
        
        Debug.Log("[BuildInputDebugger]\n" + debugLog);
    }
    
    private void FixInputAxes()
    {
        debugLog = "=== FIXING INPUT AXES ===\n\n";
        
        SerializedObject inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
        SerializedProperty axesProperty = inputManager.FindProperty("m_Axes");
        
        // Check and add Horizontal if missing
        bool hasHorizontal = false;
        for (int i = 0; i < axesProperty.arraySize; i++)
        {
            SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
            string name = axis.FindPropertyRelative("m_Name").stringValue;
            if (name == "Horizontal") hasHorizontal = true;
        }
        
        if (!hasHorizontal)
        {
            // Add Horizontal axis
            axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
            SerializedProperty newAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            SetupHorizontalAxis(newAxis);
            debugLog += "✅ Added Horizontal axis\n";
        }
        else
        {
            debugLog += "ℹ️  Horizontal axis already exists\n";
        }
        
        // Check and add Vertical if missing
        bool hasVertical = false;
        for (int i = 0; i < axesProperty.arraySize; i++)
        {
            SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
            string name = axis.FindPropertyRelative("m_Name").stringValue;
            if (name == "Vertical") hasVertical = true;
        }
        
        if (!hasVertical)
        {
            axesProperty.InsertArrayElementAtIndex(axesProperty.arraySize);
            SerializedProperty newAxis = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            SetupVerticalAxis(newAxis);
            debugLog += "✅ Added Vertical axis\n";
        }
        else
        {
            debugLog += "ℹ️  Vertical axis already exists\n";
        }
        
        inputManager.ApplyModifiedProperties();
        
        debugLog += "\n✅ Input axes setup complete!\n";
        debugLog += "\nNEXT STEP:\n";
        debugLog += "Go to: Edit → Project Settings → Player → Active Input Handling\n";
        debugLog += "Set to: 'Both' or 'Input Manager (Old)'\n";
        
        Debug.Log("[BuildInputDebugger] Fixed input axes");
    }
    
    private void SetupHorizontalAxis(SerializedProperty axis)
    {
        axis.FindPropertyRelative("m_Name").stringValue = "Horizontal";
        axis.FindPropertyRelative("descriptiveName").stringValue = "";
        axis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
        axis.FindPropertyRelative("negativeButton").stringValue = "left";
        axis.FindPropertyRelative("positiveButton").stringValue = "right";
        axis.FindPropertyRelative("altNegativeButton").stringValue = "a";
        axis.FindPropertyRelative("altPositiveButton").stringValue = "d";
        axis.FindPropertyRelative("gravity").floatValue = 3;
        axis.FindPropertyRelative("dead").floatValue = 0.001f;
        axis.FindPropertyRelative("sensitivity").floatValue = 3;
        axis.FindPropertyRelative("snap").boolValue = true;
        axis.FindPropertyRelative("invert").boolValue = false;
        axis.FindPropertyRelative("type").intValue = 0; // Key or Mouse Button
        axis.FindPropertyRelative("axis").intValue = 0;
        axis.FindPropertyRelative("joyNum").intValue = 0;
    }
    
    private void SetupVerticalAxis(SerializedProperty axis)
    {
        axis.FindPropertyRelative("m_Name").stringValue = "Vertical";
        axis.FindPropertyRelative("descriptiveName").stringValue = "";
        axis.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
        axis.FindPropertyRelative("negativeButton").stringValue = "down";
        axis.FindPropertyRelative("positiveButton").stringValue = "up";
        axis.FindPropertyRelative("altNegativeButton").stringValue = "s";
        axis.FindPropertyRelative("altPositiveButton").stringValue = "w";
        axis.FindPropertyRelative("gravity").floatValue = 3;
        axis.FindPropertyRelative("dead").floatValue = 0.001f;
        axis.FindPropertyRelative("sensitivity").floatValue = 3;
        axis.FindPropertyRelative("snap").boolValue = true;
        axis.FindPropertyRelative("invert").boolValue = false;
        axis.FindPropertyRelative("type").intValue = 0;
        axis.FindPropertyRelative("axis").intValue = 0;
        axis.FindPropertyRelative("joyNum").intValue = 0;
    }
    
    private void FixBuildSettings()
    {
        debugLog = "=== FIXING BUILD SETTINGS ===\n\n";
        
        PlayerSettings.runInBackground = true;
        debugLog += "✅ Enabled 'Run In Background'\n";
        
        debugLog += "\n✅ Build settings fixed!\n";
        debugLog += "\nREMAINING STEPS:\n";
        debugLog += "1. Go to: Edit → Project Settings → Player\n";
        debugLog += "2. Scroll to 'Active Input Handling'\n";
        debugLog += "3. Set to: 'Both' or 'Input Manager (Old)'\n";
        debugLog += "4. Restart Unity Editor\n";
        debugLog += "5. Rebuild game\n";
        
        Debug.Log("[BuildInputDebugger] Fixed build settings");
        EditorUtility.DisplayDialog("Settings Fixed", 
            "Build settings have been updated!\n\n" +
            "IMPORTANT: You must also set Active Input Handling:\n" +
            "Edit → Project Settings → Player → Active Input Handling\n" +
            "Set to: 'Both' or 'Input Manager (Old)'\n\n" +
            "Then restart Unity and rebuild.",
            "OK");
    }
}
