using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2021 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(DestructModule))]
    public class DestructModuleEditor : Editor
    {
        #region Custom Editor private variables

        // Formatting and style variables
        private DestructModule destructModule = null;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        #endregion

        #region GUIContent
        private readonly static GUIContent headerContent = new GUIContent("<b>Destruct Module</b>\n\nThis module enables you to manage a prefab as it breaks into fragments.");
        private readonly static GUIContent isExplodeOnStartContent = new GUIContent("Explode on Start", "Should the explosion occur immediately the scene is started or the module is instantiated? This should be disabled if used with a pooling system.");
        private readonly static GUIContent usePoolingContent = new GUIContent("Use Pooling", "Use the Pooling system to manage create, re-use, and destroy destruct objects.");
        private readonly static GUIContent minPoolSizeContent = new GUIContent("Min Pool Size", "When using the Pooling system, this is the number of destruct objects kept in reserve for spawning and despawning.");
        private readonly static GUIContent maxPoolSizeContent = new GUIContent("Max Pool Size", "When using the Pooling system, this is the maximum number of destruct objects permitted in the scene at any one time.");
        private readonly static GUIContent isStartStaticContent = new GUIContent("Start Static", "Start in Static mode rather than Dynamic");
        private readonly static GUIContent isAddRigidBodiesEnabledContent = new GUIContent("Add Rigidbodies", "Add rigidbodies to the fragments in the prefab");
        private readonly static GUIContent isAddMeshCollidersEnabledContent = new GUIContent("Add Mesh Colliders", "Add mesh colliders to the fragments in the prefab");
        private readonly static GUIContent explosionRadiusContent = new GUIContent("Explosion Radius", "The default effective range, in metres, of the blast");
        private readonly static GUIContent explosionPowerContent = new GUIContent("Explosion Power", "The default power of the blast");
        private readonly static GUIContent unmovingVelocityContent = new GUIContent("Unmoving Velocity", "When the speed (in metres per second) in any direction of the fragment falls below this value, the fragment is considered to have stopped moving");
        private readonly static GUIContent maxTimeUnmovingContent = new GUIContent("Max Time Unmoving", "If a fragment has been unmoving for more than the maximum time, set the object as static");
        private readonly static GUIContent massContent = new GUIContent("Total Mass", "The total mass of all fragments in the prefab");
        private readonly static GUIContent isCalcMassByBoundsContent = new GUIContent("Calc Mass by Bounds", "This may be more accurate when there the is a lot of variation between the size of each fragment.");
        private readonly static GUIContent dragContent = new GUIContent("Drag", "The amount of drag the fragments have. A solid block of metal would be 0.001, while a feather would be 10.");
        private readonly static GUIContent angularDragContent = new GUIContent("Angular Drag", "The amount of angular drag the fragments have");
        private readonly static GUIContent useGravityContent = new GUIContent("Use Gravity", "Fragments are affected by gravity");

        private readonly static GUIContent interpolationContent = new GUIContent("Interpolation", "The rigidbody interpolation");
        private readonly static GUIContent collisionDetectionContent = new GUIContent("Collision Detection", "The rigidbody collision detection mode");

        private readonly static GUIContent despawnRulesContent = new GUIContent("Despawn Rules");
        private readonly static GUIContent despawnTimeContent = new GUIContent(" Despawn Time", "After this time (in seconds), the destruct object is automatically despawned or removed from the scene.");
        private readonly static GUIContent disableRigidbodyModeContent = new GUIContent("Disable Rigidbody Mode", "How the rigidbodies are disabled when considered static. See manual for details.");
        private readonly static GUIContent despawnConditionContent = new GUIContent(" Despawn Condition", "The conditions under which this object despawns");
        private readonly static GUIContent waitUntilNotRenderedContent = new GUIContent(" Wait until not Rendered", "Wait until the fragment is not being rendered by the camera before being despawned");
        private readonly static GUIContent waitUntilStaticContent = new GUIContent(" Wait until Static", "Wait until the fragment is set to static before being despawned");
        #endregion

        #region GUIConnent Debugging
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display destruct data from the component at runtime in the editor.");
        private readonly static GUIContent debugNotSetContent = new GUIContent("--", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugIsActivatedContent = new GUIContent("Is Activated?", "Is the destruct module currently in use?");
        private readonly static GUIContent debugIsDestructEnabledContent = new GUIContent("Is Destruct Enabled?", "Is the destruct module currently ready for use, or has it been paused?");
        private readonly static GUIContent debugEstimatedDespawnTimeContent = new GUIContent("Est. Despawn Time", "The estimated number of seconds before the object is despawned");
        #endregion

        #region Serialized Properties
        private SerializedProperty usePoolingProp;
        private SerializedProperty minPoolSizeProp;
        private SerializedProperty maxPoolSizeProp;
        private SerializedProperty despawnConditionProp;
        private SerializedProperty disableRigidbodyModeProp;
        private SerializedProperty isStartStaticProp;
        private SerializedProperty useGravityProp;

        private SerializedProperty waitUntilNotRenderedToDespawnProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            destructModule = (DestructModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            usePoolingProp = serializedObject.FindProperty("usePooling");
            isStartStaticProp = serializedObject.FindProperty("isStartStatic");
            useGravityProp = serializedObject.FindProperty("useGravity");
            despawnConditionProp = serializedObject.FindProperty("despawnCondition");
            disableRigidbodyModeProp = serializedObject.FindProperty("disableRigidbodyMode");

            waitUntilNotRenderedToDespawnProp = serializedObject.FindProperty("waitUntilNotRenderedToDespawn");

            #endregion
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (destructModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            destructModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            #endregion

            #region Configure Buttons and Styles

            // Set up rich text GUIStyles
            helpBoxRichText = new GUIStyle("HelpBox");
            helpBoxRichText.richText = true;

            labelFieldRichText = new GUIStyle("Label");
            labelFieldRichText.richText = true;

            buttonCompact = new GUIStyle("Button");
            buttonCompact.fontSize = 10;

            #endregion

            // Read in all the properties
            serializedObject.Update();

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            #region General Properties

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isExplodeOnStart"), isExplodeOnStartContent);

            EditorGUILayout.PropertyField(usePoolingProp, usePoolingContent);
            if (usePoolingProp.boolValue)
            {
                minPoolSizeProp = serializedObject.FindProperty("minPoolSize");
                maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
                EditorGUILayout.PropertyField(minPoolSizeProp, minPoolSizeContent);
                EditorGUILayout.PropertyField(maxPoolSizeProp, maxPoolSizeContent);
                if (minPoolSizeProp.intValue > maxPoolSizeProp.intValue) { maxPoolSizeProp.intValue = minPoolSizeProp.intValue; }
            }

            if (disableRigidbodyModeProp.intValue != (int)DestructModule.DisableRigidbodyMode.Destroy)
            {
                EditorGUILayout.PropertyField(isStartStaticProp, isStartStaticContent);
            }
            else if (isStartStaticProp.boolValue)
            {
                // isStartStatic is not compatible with DisableRigidbodyMode.Destroy
                isStartStaticProp.boolValue = false;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isAddRigidBodiesEnabled"), isAddRigidBodiesEnabledContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isAddMeshCollidersEnabled"), isAddMeshCollidersEnabledContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"), explosionRadiusContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPower"), explosionPowerContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unmovingVelocity"), unmovingVelocityContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxTimeUnmoving"), maxTimeUnmovingContent);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("mass"), massContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isCalcMassByBounds"), isCalcMassByBoundsContent);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("drag"), dragContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("angularDrag"), angularDragContent);
            EditorGUILayout.PropertyField(useGravityProp, useGravityContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interpolation"), interpolationContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionDetection"), collisionDetectionContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(disableRigidbodyModeProp, disableRigidbodyModeContent);
            if (EditorGUI.EndChangeCheck() && isStartStaticProp.boolValue && disableRigidbodyModeProp.intValue == (int)DestructModule.DisableRigidbodyMode.Destroy)
            {
                // isStartStatic is not compatible with DisableRigidbodyMode.Destroy
                isStartStaticProp.boolValue = false;
            }

            EditorGUILayout.LabelField(despawnRulesContent);

            EditorGUILayout.PropertyField(despawnConditionProp, despawnConditionContent);

            if (despawnConditionProp.intValue == (int)DestructModule.DespawnCondition.Time)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("despawnTime"), despawnTimeContent);
            }

            if (waitUntilNotRenderedToDespawnProp.boolValue)
            {
                SSCEditorHelper.NotImplemented();
            }
            EditorGUILayout.PropertyField(waitUntilNotRenderedToDespawnProp, waitUntilNotRenderedContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitUntilStaticToDespawn"), waitUntilStaticContent);

            EditorGUILayout.EndVertical();

            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && destructModule != null)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(destructModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsActivatedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(destructModule.IsActivated ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsDestructEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(destructModule.IsDestructEnabled ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                float estDespawnTime = destructModule.EstimatedDespawnTime;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugEstimatedDespawnTimeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(float.IsInfinity(estDespawnTime) ? "--" : estDespawnTime.ToString("0.0"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion

            destructModule.allowRepaint = true;

            //DrawDefaultInspector();
        }

        #endregion
    }
}