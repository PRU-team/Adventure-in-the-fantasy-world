using UnityEngine;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// Automatically sets loop to true for all animation clips when they are imported
    /// This solves the read-only problem at import time
    /// </summary>
    public class AnimationImportSettings : AssetPostprocessor
    {
        void OnPreprocessAnimation()
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (modelImporter != null)
            {
                // Enable loop for all animation clips on import
                modelImporter.animationType = ModelImporterAnimationType.Generic;
                
                // This will be applied when the asset is imported
                Debug.Log($"[AnimationImportSettings] Processing: {assetPath}");
            }
        }
        
        void OnPostprocessAnimation(GameObject root, AnimationClip clip)
        {
            // Set loop time to true for all imported animations
            if (clip != null)
            {
                SerializedObject serializedClip = new SerializedObject(clip);
                SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
                
                if (loopProperty != null)
                {
                    loopProperty.boolValue = true;
                    serializedClip.ApplyModifiedProperties();
                    Debug.Log($"[AnimationImportSettings] Set loop=true for: {clip.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Force reimport all animation files to apply loop settings
    /// Menu: Tools -> Animation -> Reimport All Animations
    /// </summary>
    public class AnimationReimporter
    {
        [MenuItem("Tools/Animation/Reimport All Animations")]
        public static void ReimportAllAnimations()
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Reimport Animations",
                "This will reimport ALL .anim files in the project and set them to loop.\n\n" +
                "This may take a while. Continue?",
                "Yes", "Cancel"
            );
            
            if (!proceed) return;
            
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
            int total = guids.Length;
            int count = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                EditorUtility.DisplayProgressBar(
                    "Reimporting Animations", 
                    $"Processing: {path}", 
                    (float)count / total
                );
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
            
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Complete", $"Reimported {count} animation clips.", "OK");
        }
        
        [MenuItem("Tools/Animation/Make Animations Writable")]
        public static void MakeAnimationsWritable()
        {
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
            int fixedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string systemPath = Application.dataPath.Replace("Assets", "") + path;
                
                try
                {
                    System.IO.FileAttributes attributes = System.IO.File.GetAttributes(systemPath);
                    
                    if ((attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                    {
                        // Remove read-only attribute
                        attributes &= ~System.IO.FileAttributes.ReadOnly;
                        System.IO.File.SetAttributes(systemPath, attributes);
                        fixedCount++;
                        Debug.Log($"Made writable: {path}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not modify attributes for {path}: {e.Message}");
                }
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", 
                $"Made {fixedCount} animation files writable.\n" +
                "You can now edit them in the Inspector.", 
                "OK");
        }
    }
}
