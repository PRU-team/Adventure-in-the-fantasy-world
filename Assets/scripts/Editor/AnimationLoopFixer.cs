using UnityEngine;
using UnityEditor;
using System.IO;

namespace Editor
{
    /// <summary>
    /// Editor utility to fix animation loop settings for read-only animation clips
    /// Usage: Unity menu -> Tools -> Animation -> Fix Animation Loops
    /// </summary>
    public class AnimationLoopFixer : EditorWindow
    {
        private bool loopEnabled = true;
        private string searchFolder = "Assets/Animations";
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Animation/Fix Animation Loops")]
        public static void ShowWindow()
        {
            GetWindow<AnimationLoopFixer>("Animation Loop Fixer");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Animation Loop Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            loopEnabled = EditorGUILayout.Toggle("Enable Loop", loopEnabled);
            searchFolder = EditorGUILayout.TextField("Search Folder", searchFolder);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Fix All Animations in Folder", GUILayout.Height(30)))
            {
                FixAllAnimationsInFolder();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This tool will set the Loop property for all animation clips in the specified folder.\n\n" +
                "Note: This modifies the .anim files directly and may require Unity to reimport them.",
                MessageType.Info
            );
        }
        
        private void FixAllAnimationsInFolder()
        {
            if (!Directory.Exists(searchFolder))
            {
                EditorUtility.DisplayDialog("Error", $"Folder '{searchFolder}' does not exist!", "OK");
                return;
            }
            
            string[] animationFiles = Directory.GetFiles(searchFolder, "*.anim", SearchOption.AllDirectories);
            
            if (animationFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("No Animations Found", 
                    $"No animation files found in '{searchFolder}'", "OK");
                return;
            }
            
            int fixedCount = 0;
            int totalCount = animationFiles.Length;
            
            foreach (string animPath in animationFiles)
            {
                string relativePath = animPath.Replace("\\", "/");
                if (relativePath.StartsWith(Application.dataPath))
                {
                    relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
                }
                
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(relativePath);
                
                if (clip != null)
                {
                    // Get the serialized object to modify the clip
                    SerializedObject serializedClip = new SerializedObject(clip);
                    SerializedProperty loopTimeProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
                    
                    if (loopTimeProperty != null)
                    {
                        if (loopTimeProperty.boolValue != loopEnabled)
                        {
                            loopTimeProperty.boolValue = loopEnabled;
                            serializedClip.ApplyModifiedProperties();
                            EditorUtility.SetDirty(clip);
                            fixedCount++;
                        }
                    }
                }
            }
            
            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            EditorUtility.DisplayDialog("Complete", 
                $"Fixed {fixedCount} out of {totalCount} animation clips.\n" +
                $"Loop setting: {(loopEnabled ? "Enabled" : "Disabled")}", 
                "OK");
        }
    }
    
    /// <summary>
    /// Context menu option to fix loop for selected animation clips
    /// Usage: Right-click on animation file(s) in Project window -> Fix Animation Loop
    /// </summary>
    public class AnimationLoopContextMenu
    {
        [MenuItem("Assets/Animation/Enable Loop", true)]
        private static bool ValidateEnableLoop()
        {
            return Selection.activeObject is AnimationClip;
        }
        
        [MenuItem("Assets/Animation/Enable Loop")]
        private static void EnableLoop()
        {
            SetLoopForSelectedClips(true);
        }
        
        [MenuItem("Assets/Animation/Disable Loop", true)]
        private static bool ValidateDisableLoop()
        {
            return Selection.activeObject is AnimationClip;
        }
        
        [MenuItem("Assets/Animation/Disable Loop")]
        private static void DisableLoop()
        {
            SetLoopForSelectedClips(false);
        }
        
        private static void SetLoopForSelectedClips(bool loop)
        {
            int fixedCount = 0;
            
            foreach (Object obj in Selection.objects)
            {
                AnimationClip clip = obj as AnimationClip;
                if (clip != null)
                {
                    SerializedObject serializedClip = new SerializedObject(clip);
                    SerializedProperty loopTimeProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
                    
                    if (loopTimeProperty != null)
                    {
                        loopTimeProperty.boolValue = loop;
                        serializedClip.ApplyModifiedProperties();
                        EditorUtility.SetDirty(clip);
                        fixedCount++;
                    }
                }
            }
            
            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Set loop to {loop} for {fixedCount} animation clip(s)");
            }
        }
    }
}
