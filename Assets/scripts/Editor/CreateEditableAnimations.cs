using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Editor
{
    /// <summary>
    /// Create new editable animation clips from sprite sequences
    /// This creates fresh .anim files that are NOT read-only
    /// </summary>
    public class CreateEditableAnimations : EditorWindow
    {
        private Object spriteFolder;
        private string animationName = "NewAnimation";
        private float frameRate = 12f;
        private bool loopAnimation = true;
        private string savePath = "Assets/Animations/Characters/Player";
        
        private List<Sprite> selectedSprites = new List<Sprite>();
        private Vector2 scrollPos;
        
        [MenuItem("Tools/Animation/Create Editable Animation from Sprites")]
        public static void ShowWindow()
        {
            var window = GetWindow<CreateEditableAnimations>("Create Editable Animation");
            window.minSize = new Vector2(500, 600);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create Editable Animation from Sprites", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "This tool creates NEW animation clips from sprite sequences.\n" +
                "The created animations will be fully editable (NOT read-only).\n\n" +
                "Steps:\n" +
                "1. Drag sprites to the list below (or use Auto-Load)\n" +
                "2. Set animation name and settings\n" +
                "3. Click 'Create Animation'\n" +
                "4. The new .anim file will be saved and ready to use!",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Settings
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            
            animationName = EditorGUILayout.TextField("Animation Name", animationName);
            frameRate = EditorGUILayout.FloatField("Frame Rate (FPS)", frameRate);
            loopAnimation = EditorGUILayout.Toggle("Loop Animation", loopAnimation);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            
            EditorGUILayout.Space(5);
            
            // Auto-load from folder
            EditorGUILayout.LabelField("Quick Load", EditorStyles.boldLabel);
            spriteFolder = EditorGUILayout.ObjectField("Sprite Folder/Asset", spriteFolder, typeof(Object), false);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto-Load Sprites from Folder"))
            {
                AutoLoadSprites();
            }
            if (GUILayout.Button("Clear Sprite List"))
            {
                selectedSprites.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Sprite list
            EditorGUILayout.LabelField($"Sprite Sequence ({selectedSprites.Count} sprites)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(
                "Drag & drop sprites here in the correct order.\n" +
                "The animation will play frames in this order.",
                MessageType.None
            );
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            
            // Display sprites
            for (int i = 0; i < selectedSprites.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                
                EditorGUILayout.LabelField($"Frame {i}", GUILayout.Width(60));
                selectedSprites[i] = (Sprite)EditorGUILayout.ObjectField(selectedSprites[i], typeof(Sprite), false);
                
                if (GUILayout.Button("▲", GUILayout.Width(30)) && i > 0)
                {
                    var temp = selectedSprites[i];
                    selectedSprites[i] = selectedSprites[i - 1];
                    selectedSprites[i - 1] = temp;
                }
                
                if (GUILayout.Button("▼", GUILayout.Width(30)) && i < selectedSprites.Count - 1)
                {
                    var temp = selectedSprites[i];
                    selectedSprites[i] = selectedSprites[i + 1];
                    selectedSprites[i + 1] = temp;
                }
                
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    selectedSprites.RemoveAt(i);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            // Add sprite button
            if (GUILayout.Button("+ Add Sprite Slot"))
            {
                selectedSprites.Add(null);
            }
            
            EditorGUILayout.Space(10);
            
            // Preview info
            if (selectedSprites.Count > 0)
            {
                float duration = selectedSprites.Count / frameRate;
                EditorGUILayout.HelpBox(
                    $"Preview:\n" +
                    $"Total Frames: {selectedSprites.Count}\n" +
                    $"Duration: {duration:F2} seconds\n" +
                    $"Frame Rate: {frameRate} FPS",
                    MessageType.None
                );
            }
            
            EditorGUILayout.Space(10);
            
            // Create button
            GUI.enabled = selectedSprites.Count > 0 && !string.IsNullOrEmpty(animationName);
            GUI.backgroundColor = Color.green;
            
            if (GUILayout.Button("Create Animation Clip", GUILayout.Height(40)))
            {
                CreateAnimationClip();
            }
            
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }
        
        private void AutoLoadSprites()
        {
            if (spriteFolder == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a sprite asset or folder first!", "OK");
                return;
            }
            
            selectedSprites.Clear();
            
            string path = AssetDatabase.GetAssetPath(spriteFolder);
            
            // Load sprites from the selected asset
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
            
            foreach (Object obj in sprites)
            {
                if (obj is Sprite sprite && !sprite.name.Contains("preview"))
                {
                    selectedSprites.Add(sprite);
                }
            }
            
            // Sort by name (usually Frame_0, Frame_1, etc.)
            selectedSprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            
            if (selectedSprites.Count > 0)
            {
                Debug.Log($"Auto-loaded {selectedSprites.Count} sprites from {path}");
            }
            else
            {
                EditorUtility.DisplayDialog("No Sprites Found", 
                    $"Could not find any sprites in the selected asset.\n\n" +
                    $"Make sure you selected a sprite sheet or sprite asset.", 
                    "OK");
            }
        }
        
        private void CreateAnimationClip()
        {
            // Remove null sprites
            selectedSprites.RemoveAll(s => s == null);
            
            if (selectedSprites.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No valid sprites selected!", "OK");
                return;
            }
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            // Create animation clip
            AnimationClip clip = new AnimationClip();
            clip.frameRate = frameRate;
            
            // Set loop
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loopAnimation;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            
            // Create keyframes
            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(SpriteRenderer);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";
            
            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[selectedSprites.Count];
            
            for (int i = 0; i < selectedSprites.Count; i++)
            {
                spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                spriteKeyFrames[i].time = i / frameRate;
                spriteKeyFrames[i].value = selectedSprites[i];
            }
            
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
            
            // Save the animation clip
            string fileName = $"{animationName}.anim";
            string fullPath = Path.Combine(savePath, fileName);
            
            // Check if file already exists
            if (File.Exists(fullPath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "File Exists",
                    $"Animation '{fileName}' already exists.\n\nOverwrite?",
                    "Yes", "Cancel"
                );
                
                if (!overwrite)
                    return;
            }
            
            AssetDatabase.CreateAsset(clip, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created animation
            EditorGUIUtility.PingObject(clip);
            Selection.activeObject = clip;
            
            EditorUtility.DisplayDialog(
                "Success!",
                $"Animation created successfully!\n\n" +
                $"Name: {animationName}\n" +
                $"Frames: {selectedSprites.Count}\n" +
                $"Duration: {selectedSprites.Count / frameRate:F2}s\n" +
                $"Loop: {loopAnimation}\n\n" +
                $"Saved to: {fullPath}\n\n" +
                $"This animation is fully editable!",
                "OK"
            );
            
            Debug.Log($"Created editable animation: {fullPath}");
        }
    }
    
    /// <summary>
    /// Duplicate read-only animations as editable copies
    /// </summary>
    public class DuplicateAsEditableAnimation
    {
        [MenuItem("Assets/Animation/Duplicate as Editable Copy", priority = 2002)]
        private static void DuplicateAsEditable()
        {
            foreach (Object obj in Selection.objects)
            {
                AnimationClip originalClip = obj as AnimationClip;
                if (originalClip != null)
                {
                    DuplicateClip(originalClip);
                }
            }
        }
        
        [MenuItem("Assets/Animation/Duplicate as Editable Copy", true)]
        private static bool ValidateDuplicateAsEditable()
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is AnimationClip)
                    return true;
            }
            return false;
        }
        
        private static void DuplicateClip(AnimationClip originalClip)
        {
            // Create a new animation clip
            AnimationClip newClip = new AnimationClip();
            newClip.frameRate = originalClip.frameRate;
            
            // Copy settings
            var settings = AnimationUtility.GetAnimationClipSettings(originalClip);
            AnimationUtility.SetAnimationClipSettings(newClip, settings);
            
            // Copy all curve bindings
            var curveBindings = AnimationUtility.GetCurveBindings(originalClip);
            foreach (var binding in curveBindings)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
                AnimationUtility.SetEditorCurve(newClip, binding, curve);
            }
            
            // Copy object reference curves (for sprite animations)
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(originalClip);
            foreach (var binding in objectBindings)
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(originalClip, binding);
                AnimationUtility.SetObjectReferenceCurve(newClip, binding, keyframes);
            }
            
            // Save the new clip
            string originalPath = AssetDatabase.GetAssetPath(originalClip);
            string directory = Path.GetDirectoryName(originalPath);
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string newPath = Path.Combine(directory, $"{fileName}_Editable.anim");
            
            // Make sure the path is unique
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
            
            AssetDatabase.CreateAsset(newClip, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new clip
            EditorGUIUtility.PingObject(newClip);
            Selection.activeObject = newClip;
            
            Debug.Log($"Created editable copy: {newPath}");
        }
    }
}
