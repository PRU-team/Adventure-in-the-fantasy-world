using UnityEngine;
using UnityEditor;
using Enemy;
using Combat.Enemy;
using Combat.Player;
using Player;
using Character;

/// <summary>
/// Comprehensive Combat System Debugger
/// Kiểm tra toàn bộ hệ thống chiến đấu giữa Player và Enemy
/// 
/// Sử dụng: Tools → Debug → Combat System Debugger
/// </summary>
public class CombatSystemDebugger : EditorWindow
{
    private GameObject selectedEnemy;
    private GameObject selectedPlayer;
    private Vector2 scrollPos;
    
    private string enemyStatus = "";
    private string playerStatus = "";
    
    [MenuItem("Tools/Debug/Combat System Debugger")]
    public static void ShowWindow()
    {
        GetWindow<CombatSystemDebugger>("Combat System Debugger");
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.LabelField("COMBAT SYSTEM DEBUGGER", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Object selection
        EditorGUILayout.LabelField("1. Chọn đối tượng để kiểm tra:", EditorStyles.boldLabel);
        selectedEnemy = (GameObject)EditorGUILayout.ObjectField("Enemy (Skeleton):", selectedEnemy, typeof(GameObject), true);
        selectedPlayer = (GameObject)EditorGUILayout.ObjectField("Player:", selectedPlayer, typeof(GameObject), true);
        
        EditorGUILayout.Space(10);
        
        // Auto-find buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-Find Enemy"))
        {
            AutoFindEnemy();
        }
        if (GUILayout.Button("Auto-Find Player"))
        {
            AutoFindPlayer();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Debug buttons
        if (GUILayout.Button("🔍 CHECK ALL - Kiểm tra toàn bộ hệ thống", GUILayout.Height(40)))
        {
            CheckAll();
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Enemy Attack System"))
        {
            CheckEnemyAttackSystem();
        }
        if (GUILayout.Button("Check Player Attack System"))
        {
            CheckPlayerAttackSystem();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Results display
        if (!string.IsNullOrEmpty(enemyStatus))
        {
            EditorGUILayout.LabelField("🗡️ ENEMY ATTACK STATUS:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(enemyStatus, MessageType.Info);
            EditorGUILayout.Space(5);
        }
        
        if (!string.IsNullOrEmpty(playerStatus))
        {
            EditorGUILayout.LabelField("⚔️ PLAYER ATTACK STATUS:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(playerStatus, MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void AutoFindEnemy()
    {
        // Try to find Skeleton enemy
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("Skeleton") && obj.GetComponent<EnemyController>() != null)
            {
                selectedEnemy = obj;
                Debug.Log($"[CombatDebugger] Found enemy: {obj.name}");
                return;
            }
        }
        Debug.LogWarning("[CombatDebugger] Cannot find Skeleton enemy!");
    }
    
    private void AutoFindPlayer()
    {
        selectedPlayer = GameObject.Find("Player");
        if (selectedPlayer == null)
        {
            selectedPlayer = GameObject.Find("PlayerCharacter");
        }
        
        if (selectedPlayer != null)
        {
            Debug.Log($"[CombatDebugger] Found player: {selectedPlayer.name}");
        }
        else
        {
            Debug.LogWarning("[CombatDebugger] Cannot find Player!");
        }
    }
    
    private void CheckAll()
    {
        if (selectedEnemy == null || selectedPlayer == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Vui lòng chọn Enemy và Player trước!\n\nSử dụng nút Auto-Find hoặc kéo thả GameObject vào ô.", 
                "OK");
            return;
        }
        
        CheckEnemyAttackSystem();
        EditorGUILayout.Space(10);
        CheckPlayerAttackSystem();
    }
    
    private void CheckEnemyAttackSystem()
    {
        if (selectedEnemy == null)
        {
            enemyStatus = "❌ Chưa chọn Enemy!";
            return;
        }
        
        enemyStatus = "=== ENEMY ATTACK SYSTEM ===\n\n";
        bool hasErrors = false;
        
        // 1. Check EnemyController
        var enemyController = selectedEnemy.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            enemyStatus += "❌ CRITICAL: Thiếu EnemyController component!\n";
            hasErrors = true;
        }
        else
        {
            enemyStatus += "✅ EnemyController: OK\n";
        }
        
        // 2. Check EnemyAttackController
        var enemyAttackController = selectedEnemy.GetComponentInChildren<EnemyAttackController>();
        if (enemyAttackController == null)
        {
            enemyStatus += "❌ CRITICAL: Thiếu EnemyAttackController (child object)!\n";
            hasErrors = true;
        }
        else
        {
            enemyStatus += "✅ EnemyAttackController: OK\n";
            
            // 3. Check EnemyBasicAttack
            var enemyBasicAttack = enemyAttackController.GetComponentInChildren<EnemyBasicAttack>();
            if (enemyBasicAttack == null)
            {
                enemyStatus += "❌ CRITICAL: Thiếu EnemyBasicAttack (child object)!\n";
                hasErrors = true;
            }
            else
            {
                enemyStatus += "✅ EnemyBasicAttack: OK\n";
                
                // 4. Check CircleCollider2D
                var circleCollider = enemyBasicAttack.GetComponent<CircleCollider2D>();
                if (circleCollider == null)
                {
                    enemyStatus += "❌ CRITICAL: Thiếu CircleCollider2D trên BasicAttack!\n";
                    hasErrors = true;
                }
                else
                {
                    enemyStatus += $"✅ CircleCollider2D: radius={circleCollider.radius}\n";
                    
                    if (!circleCollider.isTrigger)
                    {
                        enemyStatus += "❌ ERROR: CircleCollider2D.isTrigger = FALSE! (phải là TRUE)\n";
                        hasErrors = true;
                    }
                    else
                    {
                        enemyStatus += "✅ CircleCollider2D.isTrigger: TRUE\n";
                    }
                }
            }
        }
        
        // 5. Check Animation Controller
        var animController = selectedEnemy.GetComponentInChildren<CharacterAnimationController>();
        if (animController == null)
        {
            enemyStatus += "❌ CRITICAL: Thiếu CharacterAnimationController!\n";
            hasErrors = true;
        }
        else
        {
            enemyStatus += "✅ CharacterAnimationController: OK\n";
            
            var animator = animController.GetComponent<Animator>();
            if (animator == null)
            {
                enemyStatus += "❌ CRITICAL: Thiếu Animator component!\n";
                hasErrors = true;
            }
            else
            {
                enemyStatus += $"✅ Animator: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL")}\n";
                
                // Check for Attack animation
                if (animator.runtimeAnimatorController != null)
                {
                    var clips = animator.runtimeAnimatorController.animationClips;
                    bool hasAttackAnim = false;
                    foreach (var clip in clips)
                    {
                        if (clip.name.Contains("Attack"))
                        {
                            hasAttackAnim = true;
                            
                            // Check animation events
                            var events = clip.events;
                            bool hasApplyDamageEvent = false;
                            foreach (var evt in events)
                            {
                                if (evt.functionName == "ApplyDamageToPlayer" || evt.functionName == "ApplyDamage")
                                {
                                    hasApplyDamageEvent = true;
                                    enemyStatus += $"✅ Animation Event '{evt.functionName}' found at time {evt.time}\n";
                                }
                            }
                            
                            if (!hasApplyDamageEvent)
                            {
                                enemyStatus += $"❌ WARNING: Animation '{clip.name}' không có Animation Event 'ApplyDamageToPlayer'!\n";
                                enemyStatus += "   → Thêm Animation Event vào frame giữa animation để gây sát thương\n";
                                hasErrors = true;
                            }
                        }
                    }
                    
                    if (!hasAttackAnim)
                    {
                        enemyStatus += "❌ WARNING: Không tìm thấy Attack animation!\n";
                        hasErrors = true;
                    }
                }
            }
        }
        
        // 6. Check Player reference
        if (selectedPlayer != null)
        {
            var playerController = selectedPlayer.GetComponent<PlayerController>();
            if (playerController == null)
            {
                enemyStatus += "❌ ERROR: Player không có PlayerController component!\n";
                hasErrors = true;
            }
            else
            {
                enemyStatus += "✅ Player.PlayerController: OK\n";
                enemyStatus += $"   → Player Tag: {selectedPlayer.tag}\n";
                
                if (selectedPlayer.tag != "Player")
                {
                    enemyStatus += "❌ WARNING: Player tag không phải 'Player'!\n";
                    hasErrors = true;
                }
            }
        }
        
        // 7. Check AggroRange
        var aggroRange = selectedEnemy.GetComponent<Enemy.AggroRange>();
        if (aggroRange == null)
        {
            enemyStatus += "❌ WARNING: Thiếu AggroRange component!\n";
            hasErrors = true;
        }
        else
        {
            enemyStatus += "✅ AggroRange: OK\n";
        }
        
        // Summary
        enemyStatus += "\n=== SUMMARY ===\n";
        if (hasErrors)
        {
            enemyStatus += "❌ CÓ LỖI - Xem chi tiết ở trên\n";
            enemyStatus += "\nCÁC BƯỚC SỬA:\n";
            enemyStatus += "1. Đảm bảo CircleCollider2D.isTrigger = TRUE trên BasicAttack\n";
            enemyStatus += "2. Thêm Animation Event 'ApplyDamageToPlayer' vào Skeleton_Attack animation\n";
            enemyStatus += "3. Đảm bảo Player tag = 'Player'\n";
        }
        else
        {
            enemyStatus += "✅ TẤT CẢ OK - Enemy attack system hoạt động bình thường!\n";
        }
        
        Debug.Log("[CombatDebugger] Enemy check completed:\n" + enemyStatus);
    }
    
    private void CheckPlayerAttackSystem()
    {
        if (selectedPlayer == null)
        {
            playerStatus = "❌ Chưa chọn Player!";
            return;
        }
        
        playerStatus = "=== PLAYER ATTACK SYSTEM ===\n\n";
        bool hasErrors = false;
        
        // 1. Check PlayerController
        var playerController = selectedPlayer.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerStatus += "❌ CRITICAL: Thiếu PlayerController component!\n";
            hasErrors = true;
        }
        else
        {
            playerStatus += "✅ PlayerController: OK\n";
        }
        
        // 2. Check PlayerAttackController
        var playerAttackController = selectedPlayer.GetComponentInChildren<PlayerAttackController>();
        if (playerAttackController == null)
        {
            playerStatus += "❌ CRITICAL: Thiếu PlayerAttackController (child object)!\n";
            hasErrors = true;
        }
        else
        {
            playerStatus += "✅ PlayerAttackController: OK\n";
            
            // 3. Check AttackHitbox (BasicAttack child)
            Transform basicAttackTransform = selectedPlayer.transform.Find("BasicAttack");
            if (basicAttackTransform == null)
            {
                playerStatus += "❌ CRITICAL: Thiếu child object 'BasicAttack'!\n";
                hasErrors = true;
            }
            else
            {
                var attackHitbox = basicAttackTransform.GetComponent<AttackHitbox>();
                if (attackHitbox == null)
                {
                    playerStatus += "❌ CRITICAL: BasicAttack thiếu AttackHitbox component!\n";
                    hasErrors = true;
                }
                else
                {
                    playerStatus += "✅ AttackHitbox: OK\n";
                    
                    // Check CircleCollider2D
                    var circleCollider = attackHitbox.GetComponent<CircleCollider2D>();
                    if (circleCollider == null)
                    {
                        playerStatus += "❌ CRITICAL: BasicAttack thiếu CircleCollider2D!\n";
                        hasErrors = true;
                    }
                    else
                    {
                        playerStatus += $"✅ CircleCollider2D: radius={circleCollider.radius}\n";
                        
                        if (!circleCollider.isTrigger)
                        {
                            playerStatus += "❌ ERROR: CircleCollider2D.isTrigger = FALSE! (phải là TRUE)\n";
                            hasErrors = true;
                        }
                        else
                        {
                            playerStatus += "✅ CircleCollider2D.isTrigger: TRUE\n";
                        }
                    }
                }
            }
        }
        
        // 4. Check Animation
        var animController = selectedPlayer.GetComponentInChildren<CharacterAnimationController>();
        if (animController == null)
        {
            playerStatus += "❌ CRITICAL: Thiếu CharacterAnimationController!\n";
            hasErrors = true;
        }
        else
        {
            playerStatus += "✅ CharacterAnimationController: OK\n";
            
            var animator = animController.GetComponent<Animator>();
            if (animator == null)
            {
                playerStatus += "❌ CRITICAL: Thiếu Animator component!\n";
                hasErrors = true;
            }
            else
            {
                playerStatus += $"✅ Animator: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL")}\n";
                
                // Check for Attack animation
                if (animator.runtimeAnimatorController != null)
                {
                    var clips = animator.runtimeAnimatorController.animationClips;
                    bool hasAttackAnim = false;
                    foreach (var clip in clips)
                    {
                        if (clip.name.Contains("Attack"))
                        {
                            hasAttackAnim = true;
                            
                            // Check animation events
                            var events = clip.events;
                            bool hasApplyDamageEvent = false;
                            foreach (var evt in events)
                            {
                                if (evt.functionName == "ApplyDamage")
                                {
                                    hasApplyDamageEvent = true;
                                    playerStatus += $"✅ Animation Event '{evt.functionName}' found at time {evt.time}\n";
                                }
                            }
                            
                            if (!hasApplyDamageEvent)
                            {
                                playerStatus += $"❌ WARNING: Animation '{clip.name}' không có Animation Event 'ApplyDamage'!\n";
                                playerStatus += "   → Thêm Animation Event 'ApplyDamage' với string parameter 'BasicAttack'\n";
                                hasErrors = true;
                            }
                        }
                    }
                    
                    if (!hasAttackAnim)
                    {
                        playerStatus += "❌ WARNING: Không tìm thấy Attack animation!\n";
                        hasErrors = true;
                    }
                }
            }
        }
        
        // 5. Check Enemy reference
        if (selectedEnemy != null)
        {
            playerStatus += $"\n✅ Enemy to test: {selectedEnemy.name}\n";
            playerStatus += $"   → Enemy Tag: {selectedEnemy.tag}\n";
            
            if (selectedEnemy.tag != "Enemy")
            {
                playerStatus += "❌ ERROR: Enemy tag không phải 'Enemy'!\n";
                hasErrors = true;
            }
            
            // Check if enemy can take damage
            var enemyController = selectedEnemy.GetComponent<EnemyController>();
            var bossController = selectedEnemy.GetComponent<DeathBossController>();
            
            if (enemyController == null && bossController == null)
            {
                playerStatus += "❌ ERROR: Enemy không có EnemyController hoặc DeathBossController!\n";
                hasErrors = true;
            }
            else
            {
                playerStatus += "✅ Enemy có thể nhận damage (TakeDamage method)\n";
            }
        }
        
        // Summary
        playerStatus += "\n=== SUMMARY ===\n";
        if (hasErrors)
        {
            playerStatus += "❌ CÓ LỖI - Xem chi tiết ở trên\n";
            playerStatus += "\nCÁC BƯỚC SỬA:\n";
            playerStatus += "1. Đảm bảo CircleCollider2D.isTrigger = TRUE trên BasicAttack\n";
            playerStatus += "2. Thêm Animation Event 'ApplyDamage' với parameter 'BasicAttack' vào Player attack animation\n";
            playerStatus += "3. Đảm bảo Enemy tag = 'Enemy'\n";
        }
        else
        {
            playerStatus += "✅ TẤT CẢ OK - Player attack system hoạt động bình thường!\n";
        }
        
        Debug.Log("[CombatDebugger] Player check completed:\n" + playerStatus);
    }
}
