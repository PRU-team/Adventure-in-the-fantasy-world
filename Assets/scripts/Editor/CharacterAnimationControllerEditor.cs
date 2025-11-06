using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for CharacterAnimationController with sprite flip visualization
/// </summary>
#if UNITY_EDITOR
namespace Character
{
    [CustomEditor(typeof(CharacterAnimationController))]
    public class CharacterAnimationControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty enableAutoFlipProp;
        private SerializedProperty defaultFacingRightProp;
        
        private void OnEnable()
        {
            enableAutoFlipProp = serializedObject.FindProperty("enableAutoFlip");
            defaultFacingRightProp = serializedObject.FindProperty("defaultFacingRight");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            CharacterAnimationController controller = (CharacterAnimationController)target;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Character Animation Controller", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "This controller automatically handles character animations and sprite flipping.\n\n" +
                "Enable Auto Flip: Automatically flip sprite when moving left/right.\n" +
                "Default Facing Right: Set this based on how your sprite is drawn by default.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Sprite Flip Settings
            EditorGUILayout.LabelField("Sprite Flip Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.PropertyField(enableAutoFlipProp, new GUIContent("Enable Auto Flip"));
            
            if (enableAutoFlipProp.boolValue)
            {
                EditorGUILayout.PropertyField(defaultFacingRightProp, new GUIContent("Default Facing Right"));
                
                EditorGUILayout.Space(5);
                
                // Visual helper
                EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                
                GUI.color = defaultFacingRightProp.boolValue ? Color.green : Color.gray;
                if (GUILayout.Button("→ Facing Right", GUILayout.Height(30)))
                {
                    defaultFacingRightProp.boolValue = true;
                }
                
                GUI.color = !defaultFacingRightProp.boolValue ? Color.green : Color.gray;
                if (GUILayout.Button("← Facing Left", GUILayout.Height(30)))
                {
                    defaultFacingRightProp.boolValue = false;
                }
                
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    defaultFacingRightProp.boolValue 
                        ? "Your sprite faces RIGHT by default.\nWhen moving left, sprite will flip." 
                        : "Your sprite faces LEFT by default.\nWhen moving right, sprite will flip.",
                    MessageType.None
                );
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Testing tools (only in play mode)
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Test Controls (Play Mode)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("← Test Left"))
                {
                    controller.ChangeDirection(Enums.Direction.Left);
                }
                
                if (GUILayout.Button("→ Test Right"))
                {
                    controller.ChangeDirection(Enums.Direction.Right);
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("↑ Test Up"))
                {
                    controller.ChangeDirection(Enums.Direction.Up);
                }
                
                if (GUILayout.Button("↓ Test Down"))
                {
                    controller.ChangeDirection(Enums.Direction.Down);
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("Test Attack"))
                {
                    controller.StartAttack();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
            
            // Component check
            var spriteRenderer = controller.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = controller.GetComponentInChildren<SpriteRenderer>();
            }
            
            if (spriteRenderer == null && enableAutoFlipProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ No SpriteRenderer found!\n" +
                    "Auto-flip requires a SpriteRenderer component on this GameObject or its children.",
                    MessageType.Warning
                );
                
                if (GUILayout.Button("Add SpriteRenderer"))
                {
                    controller.gameObject.AddComponent<SpriteRenderer>();
                }
            }
            
            var animator = controller.GetComponent<Animator>();
            if (animator == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ No Animator found!\n" +
                    "This component requires an Animator.",
                    MessageType.Error
                );
                
                if (GUILayout.Button("Add Animator"))
                {
                    controller.gameObject.AddComponent<Animator>();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
