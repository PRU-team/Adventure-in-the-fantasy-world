using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// Force animation loops even for read-only or imported animation clips
    /// This works by modifying the ModelImporter settings
    /// </summary>
    public class AnimationLoopForcer : EditorWindow
    {
        private Vector2 scrollPos;
        private List<AnimationClipInfo> clips = new List<AnimationClipInfo>();
        
        private class AnimationClipInfo
        {
            public AnimationClip clip;
            public string path;
            public bool isLooping;
            public bool selected;
        }
        
        [MenuItem("Tools/Animation/Force Loop Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationLoopForcer>("Force Loop Settings");
            window.minSize = new Vector2(600, 400);
            window.RefreshClipList();
        }
        
        private void RefreshClipList()
        {
            clips.Clear();
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                
                if (clip != null && !clip.name.StartsWith("__preview__"))
                {
                    clips.Add(new AnimationClipInfo
                    {
                        clip = clip,
                        path = path,
                        isLooping = clip.isLooping,
                        selected = !clip.isLooping // Auto-select non-looping clips
                    });
                }
            }
            
            Debug.Log($"Found {clips.Count} animation clips");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Animation Loop Forcer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool forces loop settings even for read-only animations.\n" +
                "Select the clips you want to modify and click 'Force Enable Loop'.",
                MessageType.Info
            );
            
            EditorGUILayout.Space(5);
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Refresh List", GUILayout.Height(25)))
            {
                RefreshClipList();
            }
            
            if (GUILayout.Button("Select All", GUILayout.Height(25)))
            {
                foreach (var clip in clips)
                    clip.selected = true;
            }
            
            if (GUILayout.Button("Select None", GUILayout.Height(25)))
            {
                foreach (var clip in clips)
                    clip.selected = false;
            }
            
            if (GUILayout.Button("Select Non-Looping", GUILayout.Height(25)))
            {
                foreach (var clip in clips)
                    clip.selected = !clip.isLooping;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Force Enable Loop (Selected)", GUILayout.Height(35)))
            {
                ForceLoopSettings(true);
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Force Disable Loop (Selected)", GUILayout.Height(35)))
            {
                ForceLoopSettings(false);
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Clip list
            EditorGUILayout.LabelField($"Animation Clips ({clips.Count})", EditorStyles.boldLabel);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            foreach (var clipInfo in clips)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                
                clipInfo.selected = EditorGUILayout.Toggle(clipInfo.selected, GUILayout.Width(20));
                
                string loopStatus = clipInfo.isLooping ? "✓" : "✗";
                Color statusColor = clipInfo.isLooping ? Color.green : Color.red;
                
                GUI.color = statusColor;
                EditorGUILayout.LabelField(loopStatus, GUILayout.Width(20));
                GUI.color = Color.white;
                
                EditorGUILayout.LabelField(clipInfo.clip.name, GUILayout.MinWidth(200));
                EditorGUILayout.LabelField($"{clipInfo.clip.length:F2}s", GUILayout.Width(50));
                EditorGUILayout.LabelField(clipInfo.path, EditorStyles.miniLabel);
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void ForceLoopSettings(bool enableLoop)
        {
            var selectedClips = clips.FindAll(c => c.selected);
            
            if (selectedClips.Count == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select at least one animation clip.", "OK");
                return;
            }
            
            bool proceed = EditorUtility.DisplayDialog(
                "Confirm",
                $"Set loop={enableLoop} for {selectedClips.Count} animation clip(s)?",
                "Yes", "Cancel"
            );
            
            if (!proceed) return;
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (var clipInfo in selectedClips)
            {
                if (TrySetLoopForClip(clipInfo.clip, clipInfo.path, enableLoop))
                {
                    successCount++;
                    clipInfo.isLooping = enableLoop;
                }
                else
                {
                    failCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            RefreshClipList();
            
            EditorUtility.DisplayDialog(
                "Complete",
                $"Successfully modified: {successCount}\n" +
                $"Failed: {failCount}\n\n" +
                $"Loop setting: {enableLoop}",
                "OK"
            );
        }
        
        private bool TrySetLoopForClip(AnimationClip clip, string path, bool loop)
        {
            try
            {
                // Method 1: Direct SerializedObject modification
                SerializedObject serializedClip = new SerializedObject(clip);
                SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
                
                if (loopProperty != null)
                {
                    loopProperty.boolValue = loop;
                    serializedClip.ApplyModifiedProperties();
                    EditorUtility.SetDirty(clip);
                    
                    Debug.Log($"[Method 1] Set loop={loop} for: {clip.name}");
                    return true;
                }
                
                // Method 2: ModelImporter modification (for imported animations)
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
                    if (clipAnimations != null && clipAnimations.Length > 0)
                    {
                        for (int i = 0; i < clipAnimations.Length; i++)
                        {
                            if (clipAnimations[i].name == clip.name)
                            {
                                clipAnimations[i].loopTime = loop;
                            }
                        }
                        
                        importer.clipAnimations = clipAnimations;
                        importer.SaveAndReimport();
                        
                        Debug.Log($"[Method 2] Set loop={loop} for imported animation: {clip.name}");
                        return true;
                    }
                }
                
                // Method 3: Force by writing to AnimationClipSettings
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                
                Debug.Log($"[Method 3] Set loop={loop} for: {clip.name}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set loop for {clip.name}: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// Menu item to quickly enable loop for selected animation clips
    /// </summary>
    public class QuickAnimationLoopMenu
    {
        [MenuItem("Assets/Animation/Quick Enable Loop", priority = 2000)]
        private static void QuickEnableLoop()
        {
            QuickSetLoop(true);
        }
        
        [MenuItem("Assets/Animation/Quick Disable Loop", priority = 2001)]
        private static void QuickDisableLoop()
        {
            QuickSetLoop(false);
        }
        
        [MenuItem("Assets/Animation/Quick Enable Loop", true)]
        [MenuItem("Assets/Animation/Quick Disable Loop", true)]
        private static bool ValidateQuickLoop()
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is AnimationClip)
                    return true;
            }
            return false;
        }
        
        private static void QuickSetLoop(bool loop)
        {
            int count = 0;
            
            foreach (Object obj in Selection.objects)
            {
                AnimationClip clip = obj as AnimationClip;
                if (clip != null)
                {
                    // Try all methods
                    try
                    {
                        var settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = loop;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        EditorUtility.SetDirty(clip);
                        count++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to set loop for {clip.name}: {e.Message}");
                    }
                }
            }
            
            if (count > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Set loop={loop} for {count} animation(s)");
            }
        }
    }
}
