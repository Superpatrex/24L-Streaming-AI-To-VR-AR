using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyDynamicModule class
    /// </summary>
    [CustomEditor(typeof(StickyDynamicModule))]
    public class StickyDynamicModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyDynamicModule stickyDynamicModule = null;
        private bool isDebuggingEnabled = false;
        #endregion

        #region Static Strings
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A poolable physical object in your scene that can react to changing gravity situations.");
        #endregion

        #region GUIContent - Dynamic Object
        private readonly static GUIContent isStartStaticContent = new GUIContent(" Start Static", "Start in Static mode rather than Dynamic");
        private readonly static GUIContent unmovingVelocityContent = new GUIContent(" Unmoving Velocity", "When the speed (in metres per second) in any direction falls below this value, the object is considered to have stopped moving");
        private readonly static GUIContent maxTimeUnmovingContent = new GUIContent(" Max Time Unmoving", "If a object has been unmoving for more than the maximum time, set the object as static");

        private readonly static GUIContent despawnRulesContent = new GUIContent(" Despawn Rules");
        private readonly static GUIContent disableRBodyModeContent = new GUIContent(" Disable Rigidbody Mode", "How the rigidbody is disabled when considered static. See manual for details.");
        private readonly static GUIContent despawnConditionContent = new GUIContent(" Despawn Condition", "The conditions under which this object despawns");
        private readonly static GUIContent waitUntilNotRenderedContent = new GUIContent(" Wait until not Rendered", "Wait until the object is not being rendered by the camera before being despawned");
        private readonly static GUIContent waitUntilStaticContent = new GUIContent(" Wait until Static", "Wait until the object is set to static before being despawned");

        #endregion

        #region GUIContent - Rigidbody and Gravity

        private readonly static GUIContent rbodySettingsContent = new GUIContent(" Rigidbody Settings");
        private readonly static GUIContent massContent = new GUIContent(" Mass", "The mass of the object");
        private readonly static GUIContent dragContent = new GUIContent(" Drag", "The amount of drag the object has. A solid block of metal would be 0.001, while a feather would be 10.");
        private readonly static GUIContent angularDragContent = new GUIContent(" Angular Drag", "The amount of angular drag the object has");

        private readonly static GUIContent gravitySettingsContent = new GUIContent(" Gravity Settings");
        private readonly static GUIContent isUseGravityContent = new GUIContent(" Use Gravity", "The object is affected by gravity");
        private readonly static GUIContent gravityModeContent = new GUIContent(" Gravity Mode", "The method used to determine in which direction gravity is acting.");
        private readonly static GUIContent gravitationalAccelerationContent = new GUIContent(" Gravity", "The gravitational acceleration, in metres per second per second, that acts downward for the dynamic object");
        private readonly static GUIContent gravityDirectionContent = new GUIContent(" Gravity Direction", "The world space direction that gravity acts upon the dynamic object when GravityMode is Direction.");

        private readonly static GUIContent interpolationContent = new GUIContent(" Interpolation", "The rigidbody interpolation");
        private readonly static GUIContent collisionDetectionContent = new GUIContent(" Collision Detection", "The rigidbody collision detection mode");
        private readonly static GUIContent initialReferenceFrameContent = new GUIContent(" Initial Reference Frame", "Initial or default reference frame transform the object will stick to when Use Gravity is enabled and Gravity Mode is ReferenceFrame.");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugNotSetContent = new GUIContent("--", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugIsActivatedContent = new GUIContent("Is Activated?", "Is the dynamic module currently in use?");
        private readonly static GUIContent debugIsModuleEnabledContent = new GUIContent("Is Module Enabled?", "Is the dynamic module currently ready for use, or has it been paused?");
        private readonly static GUIContent debugIsObjectVisibleContent = new GUIContent("Is Object Visible?", "Is the dynamic object visible to the main camera?");
        private readonly static GUIContent debugDynamicStateContent = new GUIContent("Dynamic State", "Is the object in the Dynamic or Static state?");
        private readonly static GUIContent debugEstimatedDespawnTimeContent = new GUIContent("Est. Despawn Time", "The estimated number of seconds before the object is despawned");
        private readonly static GUIContent debugSpeedContent = new GUIContent("Speed (m/s)", "The estimated number of seconds before the object is despawned");
        protected readonly static GUIContent debugCurrentReferenceFrameContent = new GUIContent("Current Reference Frame");
        #endregion

        #region Serialized Properties - Dynamic Object
        private SerializedProperty disableRBodyModeProp;
        private SerializedProperty despawnConditionProp;
        private SerializedProperty maxTimeUnmovingProp;
        private SerializedProperty unmovingVelocityProp;
        private SerializedProperty isStartStaticProp;

        private SerializedProperty waitUntilNotRenderedToDespawnProp;
        private SerializedProperty waitUntilStaticToDespawnProp;

        #endregion

        #region Serialized Properties - Gravity

        private SerializedProperty isUseGravityProp;
        private SerializedProperty gravityModeProp;
        private SerializedProperty gravitationalAccelerationProp;
        private SerializedProperty gravityDirectionProp;
        private SerializedProperty massProp;
        private SerializedProperty dragProp;
        private SerializedProperty angularDragProp;
        private SerializedProperty interpolationProp;
        private SerializedProperty collisionDetectionProp;
        private SerializedProperty initialReferenceFrameProp;

        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyDynamicModule = (StickyDynamicModule)target;

            #region Find Properties - Dynamic Objects
            disableRBodyModeProp = serializedObject.FindProperty("disableRBodyMode");
            despawnConditionProp = serializedObject.FindProperty("despawnCondition");
            maxTimeUnmovingProp = serializedObject.FindProperty("maxTimeUnmoving");
            unmovingVelocityProp = serializedObject.FindProperty("unmovingVelocity");
            isStartStaticProp = serializedObject.FindProperty("isStartStatic");

            isUseGravityProp = serializedObject.FindProperty("isUseGravity");
            gravityModeProp = serializedObject.FindProperty("gravityMode");
            gravitationalAccelerationProp = serializedObject.FindProperty("gravitationalAcceleration");
            gravityDirectionProp = serializedObject.FindProperty("gravityDirection");

            massProp = serializedObject.FindProperty("mass");
            dragProp = serializedObject.FindProperty("drag");
            angularDragProp = serializedObject.FindProperty("angularDrag");
            interpolationProp = serializedObject.FindProperty("interpolation");
            collisionDetectionProp = serializedObject.FindProperty("collisionDetection");
            initialReferenceFrameProp = serializedObject.FindProperty("initialReferenceFrame");

            waitUntilNotRenderedToDespawnProp = serializedObject.FindProperty("waitUntilNotRenderedToDespawn");
            waitUntilStaticToDespawnProp = serializedObject.FindProperty("waitUntilStaticToDespawn");

            #endregion
        }

        #endregion

        #region Private and Protected Methods

        /// <summary>
        /// Draw current reference frame information in the inspector
        /// </summary>
        /// <param name="rightLabelWidth"></param>
        protected void DrawDebugReferenceFrame(float rightLabelWidth)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugCurrentReferenceFrameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
            EditorGUILayout.LabelField(stickyDynamicModule.CurrentReferenceFrame == null ? "--" : stickyDynamicModule.CurrentReferenceFrame.name, GUILayout.MaxWidth(rightLabelWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the despawn rules in the inspector
        /// </summary>
        protected void DrawDespawnRules()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            EditorGUILayout.LabelField(despawnRulesContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(despawnConditionProp, despawnConditionContent);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyDynamicModule.DespawnCondition = (StickyManager.DespawnCondition)despawnConditionProp.intValue;
                serializedObject.Update();

                // Turn off despawn time when Despawn Condition is not Time.
                despawnTimeProp.floatValue = despawnConditionProp.intValue != (int)StickyManager.DespawnCondition.Time ? -1f : 3f;
            }

            if (despawnConditionProp.intValue == (int)StickyManager.DespawnCondition.Time)
            {
                EditorGUILayout.PropertyField(despawnTimeProp, despawnTimeContent);
            }

            if (waitUntilNotRenderedToDespawnProp.boolValue)
            {
                StickyEditorHelper.NotImplemented();
            }
            EditorGUILayout.PropertyField(waitUntilNotRenderedToDespawnProp, waitUntilNotRenderedContent);
            EditorGUILayout.PropertyField(waitUntilStaticToDespawnProp, waitUntilStaticContent);
        }

        /// <summary>
        /// Draw the gravity settings in the inspector
        /// </summary>
        protected void DrawGravitySettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            EditorGUILayout.LabelField(gravitySettingsContent);

            EditorGUILayout.PropertyField(isUseGravityProp, isUseGravityContent);
            EditorGUILayout.PropertyField(gravityModeProp, gravityModeContent);

            if (gravityModeProp.intValue != StickyManager.GravityModeUnityInt)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(gravitationalAccelerationProp, gravitationalAccelerationContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyDynamicModule.GravitationalAcceleration = gravitationalAccelerationProp.floatValue;
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(gravityDirectionProp, gravityDirectionContent);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    stickyDynamicModule.GravityDirection = gravityDirectionProp.vector3Value;
                    serializedObject.Update();
                }
            }
            
            if (gravityModeProp.intValue == StickyManager.GravityModeRefFrameInt)
            {
                EditorGUILayout.PropertyField(initialReferenceFrameProp, initialReferenceFrameContent);
            }
        }

        /// <summary>
        /// Draw (or hide) the IsStatic option in the inspector
        /// </summary>
        protected void DrawIsStatic()
        {
            if (disableRBodyModeProp.intValue != (int)StickyManager.DisableRBodyMode.Destroy)
            {
                EditorGUILayout.PropertyField(isStartStaticProp, isStartStaticContent);
            }
            else if (isStartStaticProp.boolValue)
            {
                // isStartStatic is not compatible with DisableRBodyMode.Destroy
                isStartStaticProp.boolValue = false;
            }
        }

        /// <summary>
        /// Draw the rigid body settings in the inspector
        /// </summary>
        protected void DrawRBodySettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            EditorGUILayout.LabelField(rbodySettingsContent);

            EditorGUILayout.PropertyField(massProp, massContent);
            EditorGUILayout.PropertyField(dragProp, dragContent);
            EditorGUILayout.PropertyField(angularDragProp, angularDragContent);
            EditorGUILayout.PropertyField(interpolationProp, interpolationContent);
            EditorGUILayout.PropertyField(collisionDetectionProp, collisionDetectionContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(disableRBodyModeProp, disableRBodyModeContent);
            if (EditorGUI.EndChangeCheck() && isStartStaticProp.boolValue && disableRBodyModeProp.intValue == (int)StickyManager.DisableRBodyMode.Destroy)
            {
                // isStartStatic is not compatible with DisableRBodyMode.Destroy
                isStartStaticProp.boolValue = false;
            }
        }

        /// <summary>
        /// Draw the Unmoving options in the inspector
        /// </summary>
        protected void DrawUnmoving()
        {
            EditorGUILayout.PropertyField(unmovingVelocityProp, unmovingVelocityContent);
            EditorGUILayout.PropertyField(maxTimeUnmovingProp, maxTimeUnmovingContent);
        }

        #endregion

        #region DrawBaseInspector

        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyDynamicModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            // Draw the common generic object settings without the despawn time
            DrawMinMaxPoolSize();
            DrawIsReparented();

            DrawIsStatic();
            DrawUnmoving();
            DrawRBodySettings();
            DrawGravitySettings();
            DrawDespawnRules();

            //EditorGUILayout.PropertyField(speedProp, speedContent);

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyDynamicModule != null)
            {
                float rightLabelWidth = 150f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.IsDynamicModuleInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsModuleEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.IsModuleEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsActivatedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.IsDynamicModuleActivated ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugDynamicStateContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.IsDynamic ? "Dynamic" : "Static", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsObjectVisibleContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.IsObjectVisibleToCamera ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                float estDespawnTime = stickyDynamicModule.EstimatedDespawnTime;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugEstimatedDespawnTimeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(float.IsInfinity(estDespawnTime) ? "--" : estDespawnTime.ToString("0.0"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyDynamicModule.DynamicObjectSpeed.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                DrawDebugReferenceFrame(rightLabelWidth);
            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyDynamicModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyDynamicModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Dynamic Object")]
        public static StickyDynamicModule CreateStickyDynamicObject()
        {
            StickyDynamicModule stickyDynamicModule = null;

            // Create a new gameobject
            GameObject stickyDynamicObj = new GameObject("StickyDynamicObject");

            if (stickyDynamicObj != null)
            {
                stickyDynamicModule = stickyDynamicObj.AddComponent<StickyDynamicModule>();

                if (stickyDynamicModule != null)
                {
                    stickyDynamicModule.isReparented = true;
                }
            }

            return stickyDynamicModule;
        }

        #endregion
    }
}