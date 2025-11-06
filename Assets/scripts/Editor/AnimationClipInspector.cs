using UnityEngine;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// Inspector for viewing and debugging animation clip settings
    /// Shows detailed info about loop settings and provides quick fix buttons
    /// </summary>
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            AnimationClip clip = (AnimationClip)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Debug Info", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Clip Name:", clip.name);
            EditorGUILayout.LabelField("Length:", $"{clip.length:F2} seconds");
            EditorGUILayout.LabelField("Frame Rate:", $"{clip.frameRate} fps");
            EditorGUILayout.LabelField("Loop:", clip.isLooping ? "✓ YES" : "✗ NO");
            EditorGUILayout.LabelField("Legacy:", clip.legacy ? "Yes" : "No");
            
            EditorGUILayout.Space();
            
            // Quick fix buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Enable Loop", GUILayout.Height(25)))
            {
                SetClipLoop(clip, true);
            }
            
            if (GUILayout.Button("Disable Loop", GUILayout.Height(25)))
            {
                SetClipLoop(clip, false);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show additional settings
            SerializedObject so = new SerializedObject(clip);
            SerializedProperty settings = so.FindProperty("m_AnimationClipSettings");
            
            if (settings != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clip Settings (Read-Only View)", EditorStyles.boldLabel);
                
                SerializedProperty loopTime = settings.FindPropertyRelative("m_LoopTime");
                SerializedProperty loopBlend = settings.FindPropertyRelative("m_LoopBlend");
                SerializedProperty cycleOffset = settings.FindPropertyRelative("m_CycleOffset");
                
                if (loopTime != null)
                    EditorGUILayout.LabelField("  Loop Time:", loopTime.boolValue.ToString());
                if (loopBlend != null)
                    EditorGUILayout.LabelField("  Loop Blend:", loopBlend.boolValue.ToString());
                if (cycleOffset != null)
                    EditorGUILayout.LabelField("  Cycle Offset:", cycleOffset.floatValue.ToString("F2"));
            }
        }
        
        private void SetClipLoop(AnimationClip clip, bool loop)
        {
            SerializedObject serializedClip = new SerializedObject(clip);
            SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
            
            if (loopProperty != null)
            {
                loopProperty.boolValue = loop;
                serializedClip.ApplyModifiedProperties();
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"Set loop={loop} for animation: {clip.name}");
                EditorUtility.DisplayDialog("Success", 
                    $"Animation '{clip.name}' loop set to: {loop}", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find loop property!", 
                    "OK");
            }
        }
    }
}
