using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Quick Fix Tool for Combat System
/// Tự động sửa các vấn đề phổ biến trong hệ thống combat
/// 
/// Sử dụng: Tools → Fix → Auto-Fix Combat System
/// </summary>
public class CombatSystemQuickFix : EditorWindow
{
    [MenuItem("Tools/Fix/Auto-Fix Combat System")]
    public static void ShowWindow()
    {
        var window = GetWindow<CombatSystemQuickFix>("Combat Quick Fix");
        window.minSize = new Vector2(400, 300);
    }
    
    private Vector2 scrollPos;
    private string fixLog = "";
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.LabelField("COMBAT SYSTEM QUICK FIX", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Tool này sẽ tự động sửa các vấn đề phổ biến:\n" +
            "• Set CircleCollider2D.isTrigger = TRUE cho tất cả attack hitbox\n" +
            "• Set Player tag cho Player GameObject\n" +
            "• Set Enemy tag cho tất cả Enemy GameObjects\n" +
            "• Fix missing components",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("🔧 AUTO-FIX ALL", GUILayout.Height(40)))
        {
            AutoFixAll();
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Individual Fixes:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Fix Enemy Attack Colliders"))
        {
            FixEnemyAttackColliders();
        }
        
        if (GUILayout.Button("Fix Player Attack Colliders"))
        {
            FixPlayerAttackColliders();
        }
        
        if (GUILayout.Button("Fix Tags"))
        {
            FixTags();
        }
        
        EditorGUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(fixLog))
        {
            EditorGUILayout.LabelField("Fix Log:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(fixLog, GUILayout.Height(200));
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void AutoFixAll()
    {
        fixLog = "=== AUTO-FIX ALL STARTED ===\n\n";
        
        FixEnemyAttackColliders();
        fixLog += "\n";
        FixPlayerAttackColliders();
        fixLog += "\n";
        FixTags();
        
        fixLog += "\n=== AUTO-FIX COMPLETED ===\n";
        fixLog += "✅ Đã fix xong! Nhớ Save Scene (Ctrl+S)\n";
        
        Debug.Log("[CombatQuickFix] " + fixLog);
        EditorUtility.DisplayDialog("Success", "Auto-fix completed! Check the log for details.\n\nNhớ Save Scene!", "OK");
    }
    
    private void FixEnemyAttackColliders()
    {
        fixLog += "--- Fixing Enemy Attack Colliders ---\n";
        int fixCount = 0;
        
        // Find all enemies
        var enemies = FindObjectsByType<Enemy.EnemyController>(FindObjectsSortMode.None);
        var bosses = FindObjectsByType<Enemy.DeathBossController>(FindObjectsSortMode.None);
        
        foreach (var enemy in enemies)
        {
            var result = FixEnemyCollider(enemy.gameObject);
            fixLog += result;
            if (result.Contains("✅")) fixCount++;
        }
        
        foreach (var boss in bosses)
        {
            var result = FixEnemyCollider(boss.gameObject);
            fixLog += result;
            if (result.Contains("✅")) fixCount++;
        }
        
        fixLog += $"\n✅ Fixed {fixCount} enemy attack colliders\n";
    }
    
    private string FixEnemyCollider(GameObject enemy)
    {
        string log = $"Checking {enemy.name}:\n";
        
        // Find BasicAttack child
        Transform basicAttack = null;
        var attackController = enemy.GetComponentInChildren<Combat.Enemy.EnemyAttackController>();
        if (attackController != null)
        {
            var basicAttackComponent = attackController.GetComponentInChildren<Combat.Enemy.EnemyBasicAttack>();
            if (basicAttackComponent != null)
            {
                basicAttack = basicAttackComponent.transform;
            }
        }
        
        if (basicAttack == null)
        {
            log += "  ⚠️ Cannot find BasicAttack child - skip\n";
            return log;
        }
        
        // Check and fix CircleCollider2D
        var collider = basicAttack.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            log += "  ❌ Missing CircleCollider2D - adding...\n";
            collider = basicAttack.gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 1.5f;
            log += "  ✅ Added CircleCollider2D with radius 1.5\n";
        }
        
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            log += "  ✅ Set isTrigger = TRUE\n";
            EditorUtility.SetDirty(collider);
        }
        else
        {
            log += "  ℹ️ isTrigger already TRUE\n";
        }
        
        return log;
    }
    
    private void FixPlayerAttackColliders()
    {
        fixLog += "--- Fixing Player Attack Colliders ---\n";
        
        // Find player
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.Find("PlayerCharacter");
        }
        
        if (player == null)
        {
            fixLog += "❌ Cannot find Player GameObject!\n";
            return;
        }
        
        fixLog += $"Found player: {player.name}\n";
        
        // Find BasicAttack child
        Transform basicAttack = player.transform.Find("BasicAttack");
        if (basicAttack == null)
        {
            fixLog += "⚠️ Cannot find BasicAttack child - skip\n";
            return;
        }
        
        // Check and fix CircleCollider2D
        var collider = basicAttack.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            fixLog += "❌ Missing CircleCollider2D - adding...\n";
            collider = basicAttack.gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 2.0f;
            fixLog += "✅ Added CircleCollider2D with radius 2.0\n";
        }
        
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            fixLog += "✅ Set isTrigger = TRUE\n";
            EditorUtility.SetDirty(collider);
        }
        else
        {
            fixLog += "ℹ️ isTrigger already TRUE\n";
        }
        
        // Check AttackHitbox component
        var attackHitbox = basicAttack.GetComponent<Combat.Player.AttackHitbox>();
        if (attackHitbox == null)
        {
            fixLog += "⚠️ Missing AttackHitbox component - adding...\n";
            basicAttack.gameObject.AddComponent<Combat.Player.AttackHitbox>();
            fixLog += "✅ Added AttackHitbox component\n";
            EditorUtility.SetDirty(basicAttack.gameObject);
        }
        else
        {
            fixLog += "ℹ️ AttackHitbox already exists\n";
        }
    }
    
    private void FixTags()
    {
        fixLog += "--- Fixing Tags ---\n";
        
        // Ensure tags exist
        EnsureTagExists("Player");
        EnsureTagExists("Enemy");
        
        // Fix player tag
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.Find("PlayerCharacter");
        }
        
        if (player != null)
        {
            if (player.tag != "Player")
            {
                player.tag = "Player";
                fixLog += $"✅ Set {player.name} tag to 'Player'\n";
                EditorUtility.SetDirty(player);
            }
            else
            {
                fixLog += $"ℹ️ {player.name} tag already 'Player'\n";
            }
        }
        
        // Fix enemy tags
        var enemies = FindObjectsByType<Enemy.EnemyController>(FindObjectsSortMode.None);
        var bosses = FindObjectsByType<Enemy.DeathBossController>(FindObjectsSortMode.None);
        
        int enemyCount = 0;
        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.tag != "Enemy")
            {
                enemy.gameObject.tag = "Enemy";
                fixLog += $"✅ Set {enemy.gameObject.name} tag to 'Enemy'\n";
                EditorUtility.SetDirty(enemy.gameObject);
                enemyCount++;
            }
        }
        
        foreach (var boss in bosses)
        {
            if (boss.gameObject.tag != "Enemy")
            {
                boss.gameObject.tag = "Enemy";
                fixLog += $"✅ Set {boss.gameObject.name} tag to 'Enemy'\n";
                EditorUtility.SetDirty(boss.gameObject);
                enemyCount++;
            }
        }
        
        if (enemyCount == 0)
        {
            fixLog += "ℹ️ All enemies already have 'Enemy' tag\n";
        }
        else
        {
            fixLog += $"✅ Fixed {enemyCount} enemy tags\n";
        }
    }
    
    private void EnsureTagExists(string tagName)
    {
        // Check if tag exists
        try
        {
            GameObject.FindGameObjectWithTag(tagName);
        }
        catch
        {
            // Tag doesn't exist, need to create it
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            // Check if already exists in array
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName))
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
                newTag.stringValue = tagName;
                tagManager.ApplyModifiedProperties();
                fixLog += $"✅ Created tag '{tagName}'\n";
            }
        }
    }
}
