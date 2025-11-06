using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Quick helper to assign Animator Controllers to GameObjects
    /// </summary>
    public class AnimatorControllerHelper : EditorWindow
    {
        private RuntimeAnimatorController controllerToAssign;
        private List<GameObject> selectedObjects = new List<GameObject>();
        
        [MenuItem("Tools/Animation/Assign Controller to GameObjects")]
        public static void ShowWindow()
        {
            GetWindow<AnimatorControllerHelper>("Assign Animator Controller");
        }
        
        private void OnEnable()
        {
            RefreshSelection();
        }
        
        private void OnSelectionChange()
        {
            RefreshSelection();
            Repaint();
        }
        
        private void RefreshSelection()
        {
            selectedObjects.Clear();
            
            foreach (var obj in Selection.gameObjects)
            {
                if (obj != null)
                    selectedObjects.Add(obj);
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Assign Animator Controller", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Select GameObjects in the Hierarchy/Scene, then assign an Animator Controller.\n\n" +
                "This will:\n" +
                "• Add Animator component if missing\n" +
                "• Assign the selected controller\n" +
                "• Configure runtime settings",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Controller selection
            EditorGUILayout.LabelField("Controller to Assign", EditorStyles.boldLabel);
            controllerToAssign = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                "Animator Controller",
                controllerToAssign,
                typeof(RuntimeAnimatorController),
                false
            );
            
            EditorGUILayout.Space(10);
            
            // Selected objects
            EditorGUILayout.LabelField($"Selected GameObjects ({selectedObjects.Count})", EditorStyles.boldLabel);
            
            if (selectedObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("No GameObjects selected. Please select objects in the Hierarchy.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                foreach (var obj in selectedObjects)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.LabelField(obj.name);
                    
                    var animator = obj.GetComponent<Animator>();
                    if (animator != null)
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("Has Animator", EditorStyles.miniLabel, GUILayout.Width(100));
                        
                        if (animator.runtimeAnimatorController != null)
                        {
                            EditorGUILayout.LabelField($"[{animator.runtimeAnimatorController.name}]", 
                                EditorStyles.miniLabel, GUILayout.Width(150));
                        }
                        else
                        {
                            GUI.color = Color.yellow;
                            EditorGUILayout.LabelField("[No Controller]", EditorStyles.miniLabel, GUILayout.Width(100));
                        }
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("Missing Animator", EditorStyles.miniLabel, GUILayout.Width(100));
                    }
                    
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
            
            // Assign button
            GUI.enabled = controllerToAssign != null && selectedObjects.Count > 0;
            GUI.backgroundColor = Color.green;
            
            if (GUILayout.Button("Assign Controller to Selected Objects", GUILayout.Height(40)))
            {
                AssignControllerToObjects();
            }
            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }
        
        private void AssignControllerToObjects()
        {
            int successCount = 0;
            
            foreach (var obj in selectedObjects)
            {
                if (obj == null) continue;
                
                // Get or add Animator component
                var animator = obj.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = obj.AddComponent<Animator>();
                    Debug.Log($"Added Animator component to: {obj.name}");
                }
                
                // Assign controller
                animator.runtimeAnimatorController = controllerToAssign;
                
                // Configure animator settings
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                
                EditorUtility.SetDirty(obj);
                successCount++;
                
                Debug.Log($"Assigned controller '{controllerToAssign.name}' to: {obj.name}");
            }
            
            EditorUtility.DisplayDialog(
                "Success!",
                $"Assigned controller '{controllerToAssign.name}' to {successCount} GameObject(s).",
                "OK"
            );
        }
    }
    
    /// <summary>
    /// Context menu items for quick animator operations
    /// </summary>
    public class AnimatorContextMenu
    {
        [MenuItem("GameObject/Animation/Add Animator Component", false, 0)]
        private static void AddAnimatorComponent()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.GetComponent<Animator>() == null)
                {
                    obj.AddComponent<Animator>();
                    EditorUtility.SetDirty(obj);
                    Debug.Log($"Added Animator to: {obj.name}");
                }
            }
        }
        
        [MenuItem("GameObject/Animation/Add Animator Component", true)]
        private static bool ValidateAddAnimatorComponent()
        {
            return Selection.gameObjects.Length > 0;
        }
        
        [MenuItem("GameObject/Animation/Open Animator Window", false, 1)]
        private static void OpenAnimatorWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
        }
        
        [MenuItem("GameObject/Animation/Open Animation Window", false, 2)]
        private static void OpenAnimationWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
        }
    }
}
