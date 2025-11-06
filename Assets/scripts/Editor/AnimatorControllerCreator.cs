using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

namespace Editor
{
    /// <summary>
    /// Create and configure Animator Controllers with animations
    /// Automatically sets up states, transitions, and parameters
    /// </summary>
    public class AnimatorControllerCreator : EditorWindow
    {
        private string controllerName = "NewAnimatorController";
        private string savePath = "Assets/Animations/Characters";
        private RuntimeAnimatorController existingController;
        
        private AnimationClip idleAnimation;
        private AnimationClip walkUpAnimation;
        private AnimationClip walkDownAnimation;
        private AnimationClip walkLeftAnimation;
        private AnimationClip walkRightAnimation;
        private AnimationClip attackAnimation;
        private AnimationClip attack2Animation;
        private AnimationClip attack3Animation;
        private AnimationClip hurtAnimation;
        private AnimationClip deathAnimation;
        
        private bool createMovementStates = true;
        private bool createCombatStates = true;
        private bool autoCreateParameters = true;
        
        private Vector2 scrollPos;
        
        [MenuItem("Tools/Animation/Create Animator Controller")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimatorControllerCreator>("Create Animator Controller");
            window.minSize = new Vector2(500, 700);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Animator Controller Creator", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "This tool creates a new Animator Controller or modifies an existing one.\n\n" +
                "Features:\n" +
                "• Create controller from scratch\n" +
                "• Add states for Idle, Walk (4 directions), Attack, Hurt, Death\n" +
                "• Automatically create parameters (HorizontalSpeed, VerticalSpeed, isIdle, etc.)\n" +
                "• Set up transitions with proper conditions\n" +
                "• Compatible with your CharacterAnimationController.cs script",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            // Basic Settings
            EditorGUILayout.LabelField("Controller Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Or modify existing controller:", EditorStyles.miniLabel);
            existingController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                "Existing Controller", 
                existingController, 
                typeof(RuntimeAnimatorController), 
                false
            );
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            createMovementStates = EditorGUILayout.Toggle("Create Movement States", createMovementStates);
            createCombatStates = EditorGUILayout.Toggle("Create Combat States", createCombatStates);
            autoCreateParameters = EditorGUILayout.Toggle("Auto-Create Parameters", autoCreateParameters);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Animation Clips
            EditorGUILayout.LabelField("Animation Clips", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Assign animation clips (optional - can be set later)", MessageType.None);
            
            // Idle & Movement
            if (createMovementStates)
            {
                EditorGUILayout.LabelField("Movement Animations", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                idleAnimation = (AnimationClip)EditorGUILayout.ObjectField("Idle", idleAnimation, typeof(AnimationClip), false);
                walkUpAnimation = (AnimationClip)EditorGUILayout.ObjectField("Walk Up", walkUpAnimation, typeof(AnimationClip), false);
                walkDownAnimation = (AnimationClip)EditorGUILayout.ObjectField("Walk Down", walkDownAnimation, typeof(AnimationClip), false);
                walkLeftAnimation = (AnimationClip)EditorGUILayout.ObjectField("Walk Left", walkLeftAnimation, typeof(AnimationClip), false);
                walkRightAnimation = (AnimationClip)EditorGUILayout.ObjectField("Walk Right", walkRightAnimation, typeof(AnimationClip), false);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            // Combat
            if (createCombatStates)
            {
                EditorGUILayout.LabelField("Combat Animations", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                attackAnimation = (AnimationClip)EditorGUILayout.ObjectField("Attack 1", attackAnimation, typeof(AnimationClip), false);
                attack2Animation = (AnimationClip)EditorGUILayout.ObjectField("Attack 2", attack2Animation, typeof(AnimationClip), false);
                attack3Animation = (AnimationClip)EditorGUILayout.ObjectField("Attack 3", attack3Animation, typeof(AnimationClip), false);
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Reactions", EditorStyles.miniLabel);
                
                hurtAnimation = (AnimationClip)EditorGUILayout.ObjectField("Hurt", hurtAnimation, typeof(AnimationClip), false);
                deathAnimation = (AnimationClip)EditorGUILayout.ObjectField("Death", deathAnimation, typeof(AnimationClip), false);
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(10);
            
            // Auto-assign button
            if (GUILayout.Button("Auto-Assign Animations from Selection", GUILayout.Height(30)))
            {
                AutoAssignAnimations();
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.EndScrollView();
            
            // Create button
            GUI.backgroundColor = Color.green;
            
            string buttonText = existingController != null ? "Update Existing Controller" : "Create New Controller";
            
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                if (existingController != null)
                {
                    UpdateExistingController();
                }
                else
                {
                    CreateNewController();
                }
            }
            
            GUI.backgroundColor = Color.white;
        }
        
        private void AutoAssignAnimations()
        {
            foreach (Object obj in Selection.objects)
            {
                AnimationClip clip = obj as AnimationClip;
                if (clip == null) continue;
                
                string name = clip.name.ToLower();
                
                // Try to match by name
                if (name.Contains("idle"))
                    idleAnimation = clip;
                else if (name.Contains("walk") || name.Contains("run"))
                {
                    if (name.Contains("up"))
                        walkUpAnimation = clip;
                    else if (name.Contains("down"))
                        walkDownAnimation = clip;
                    else if (name.Contains("left"))
                        walkLeftAnimation = clip;
                    else if (name.Contains("right"))
                        walkRightAnimation = clip;
                }
                else if (name.Contains("attack"))
                {
                    if (name.Contains("3") || name.Contains("03") || name.Contains("three"))
                        attack3Animation = clip;
                    else if (name.Contains("2") || name.Contains("02") || name.Contains("two"))
                        attack2Animation = clip;
                    else if (name.Contains("1") || name.Contains("01") || name.Contains("one") || attackAnimation == null)
                        attackAnimation = clip;
                }
                else if (name.Contains("hurt") || name.Contains("hit"))
                    hurtAnimation = clip;
                else if (name.Contains("death") || name.Contains("die"))
                    deathAnimation = clip;
            }
            
            Debug.Log("Auto-assigned animations from selection");
        }
        
        private void CreateNewController()
        {
            if (string.IsNullOrEmpty(controllerName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a controller name!", "OK");
                return;
            }
            
            // Create directory if needed
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            // Create the animator controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                Path.Combine(savePath, $"{controllerName}.controller")
            );
            
            // Set up the controller
            SetupAnimatorController(controller);
            
            // Select the created controller
            EditorGUIUtility.PingObject(controller);
            Selection.activeObject = controller;
            
            EditorUtility.DisplayDialog(
                "Success!",
                $"Animator Controller created successfully!\n\n" +
                $"Name: {controllerName}\n" +
                $"Path: {savePath}\n\n" +
                $"The controller is ready to use with your CharacterAnimationController.cs script.",
                "OK"
            );
            
            Debug.Log($"Created Animator Controller: {controller.name}");
        }
        
        private void UpdateExistingController()
        {
            AnimatorController controller = existingController as AnimatorController;
            
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected controller is not a valid AnimatorController!", "OK");
                return;
            }
            
            bool proceed = EditorUtility.DisplayDialog(
                "Update Controller",
                $"This will add/update states and parameters in:\n{controller.name}\n\nContinue?",
                "Yes", "Cancel"
            );
            
            if (!proceed) return;
            
            SetupAnimatorController(controller);
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Success!", "Controller updated successfully!", "OK");
        }
        
        private void SetupAnimatorController(AnimatorController controller)
        {
            // Get or create base layer
            AnimatorControllerLayer baseLayer = controller.layers.Length > 0 
                ? controller.layers[0] 
                : controller.layers[0];
            
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            
            // Create parameters
            if (autoCreateParameters)
            {
                CreateParameter(controller, "HorizontalSpeed", AnimatorControllerParameterType.Float);
                CreateParameter(controller, "VerticalSpeed", AnimatorControllerParameterType.Float);
                CreateParameter(controller, "isIdle", AnimatorControllerParameterType.Bool);
                CreateParameter(controller, "isSwimming", AnimatorControllerParameterType.Bool);
                CreateParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "FireAttack", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "RangedAttack", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "DefensiveAbility", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "HealingAbility", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);
                CreateParameter(controller, "Death", AnimatorControllerParameterType.Trigger);
            }
            
            // Create states
            AnimatorState idleState = null;
            
            if (createMovementStates)
            {
                // Idle state (default state)
                idleState = CreateOrGetState(stateMachine, "Idle", idleAnimation);
                stateMachine.defaultState = idleState;
                idleState.writeDefaultValues = false;
                
                // Movement states
                var walkUpState = CreateOrGetState(stateMachine, "Walk_Up", walkUpAnimation);
                var walkDownState = CreateOrGetState(stateMachine, "Walk_Down", walkDownAnimation);
                var walkLeftState = CreateOrGetState(stateMachine, "Walk_Left", walkLeftAnimation);
                var walkRightState = CreateOrGetState(stateMachine, "Walk_Right", walkRightAnimation);
                
                // Set up blend tree for movement (alternative approach)
                CreateMovementBlendTree(controller, stateMachine, idleState);
            }
            
            if (createCombatStates && idleState != null)
            {
                // Combat states
                var attackState = CreateOrGetState(stateMachine, "Attack", attackAnimation);
                var attack2State = CreateOrGetState(stateMachine, "Attack_2", attack2Animation);
                var attack3State = CreateOrGetState(stateMachine, "Attack_3", attack3Animation);
                var hurtState = CreateOrGetState(stateMachine, "Hurt", hurtAnimation);
                var deathState = CreateOrGetState(stateMachine, "Death", deathAnimation);
                
                // Attack 1 transitions
                if (attackState != null)
                {
                    var toAttack = idleState.AddTransition(attackState);
                    toAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
                    toAttack.hasExitTime = false;
                    toAttack.duration = 0.1f;
                    
                    var fromAttack = attackState.AddTransition(idleState);
                    fromAttack.hasExitTime = true;
                    fromAttack.exitTime = 0.9f;
                    fromAttack.duration = 0.1f;
                }
                
                // Attack 2 transitions (Fire Attack)
                if (attack2State != null)
                {
                    var toAttack2 = idleState.AddTransition(attack2State);
                    toAttack2.AddCondition(AnimatorConditionMode.If, 0, "FireAttack");
                    toAttack2.hasExitTime = false;
                    toAttack2.duration = 0.1f;
                    
                    var fromAttack2 = attack2State.AddTransition(idleState);
                    fromAttack2.hasExitTime = true;
                    fromAttack2.exitTime = 0.9f;
                    fromAttack2.duration = 0.1f;
                }
                
                // Attack 3 transitions (Ranged Attack)
                if (attack3State != null)
                {
                    var toAttack3 = idleState.AddTransition(attack3State);
                    toAttack3.AddCondition(AnimatorConditionMode.If, 0, "RangedAttack");
                    toAttack3.hasExitTime = false;
                    toAttack3.duration = 0.1f;
                    
                    var fromAttack3 = attack3State.AddTransition(idleState);
                    fromAttack3.hasExitTime = true;
                    fromAttack3.exitTime = 0.9f;
                    fromAttack3.duration = 0.1f;
                }
                
                // Hurt transition
                if (hurtState != null)
                {
                    var toHurt = CreateAnyStateTransition(stateMachine, hurtState);
                    toHurt.AddCondition(AnimatorConditionMode.If, 0, "Hit");
                    toHurt.hasExitTime = false;
                    toHurt.duration = 0.05f;
                    
                    var fromHurt = hurtState.AddTransition(idleState);
                    fromHurt.hasExitTime = true;
                    fromHurt.exitTime = 0.9f;
                    fromHurt.duration = 0.1f;
                }
                
                // Death transition
                if (deathState != null)
                {
                    var toDeath = CreateAnyStateTransition(stateMachine, deathState);
                    toDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
                    toDeath.hasExitTime = false;
                    toDeath.duration = 0.1f;
                }
            }
            
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            // Check if parameter already exists
            foreach (var param in controller.parameters)
            {
                if (param.name == name)
                    return;
            }
            
            controller.AddParameter(name, type);
            Debug.Log($"Created parameter: {name} ({type})");
        }
        
        private AnimatorState CreateOrGetState(AnimatorStateMachine stateMachine, string name, AnimationClip clip)
        {
            // Check if state already exists
            foreach (var state in stateMachine.states)
            {
                if (state.state.name == name)
                {
                    if (clip != null)
                        state.state.motion = clip;
                    return state.state;
                }
            }
            
            // Create new state
            var newState = stateMachine.AddState(name);
            if (clip != null)
            {
                newState.motion = clip;
                Debug.Log($"Created state: {name} with animation: {clip.name}");
            }
            else
            {
                Debug.Log($"Created state: {name} (no animation assigned)");
            }
            
            return newState;
        }
        
        private AnimatorStateTransition CreateAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState targetState)
        {
            // Check if transition already exists
            foreach (var transition in stateMachine.anyStateTransitions)
            {
                if (transition.destinationState == targetState)
                    return transition;
            }
            
            return stateMachine.AddAnyStateTransition(targetState);
        }
        
        private void CreateMovementBlendTree(AnimatorController controller, AnimatorStateMachine stateMachine, AnimatorState idleState)
        {
            // This creates a blend tree for smoother 4-directional movement
            // You can enable this for more advanced movement
            
            // For now, we'll use simple state-based approach
            // If you want blend tree, uncomment and modify this section
            
            /*
            var blendTreeState = stateMachine.AddState("Movement");
            var blendTree = new BlendTree();
            blendTree.name = "Movement Blend Tree";
            blendTree.blendType = BlendTreeType.SimpleDirectional2D;
            blendTree.blendParameter = "HorizontalSpeed";
            blendTree.blendParameterY = "VerticalSpeed";
            
            // Add motions to blend tree
            blendTree.AddChild(idleAnimation, new Vector2(0, 0));
            blendTree.AddChild(walkUpAnimation, new Vector2(0, 1));
            blendTree.AddChild(walkDownAnimation, new Vector2(0, -1));
            blendTree.AddChild(walkLeftAnimation, new Vector2(-1, 0));
            blendTree.AddChild(walkRightAnimation, new Vector2(1, 0));
            
            blendTreeState.motion = blendTree;
            */
        }
    }
}
