using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2020 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(BeamModule))]
    public class BeamModuleEditor : Editor
    {
        #region Custom Editor private variables

        private BeamModule beamModule;

        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        #endregion

        #region Static Strings
        private readonly static string effectsDespawnWarning = "The Despawn Time of the Effects Object is less than the Max Burst Duration.";
        #endregion

        #region GUIContent
        private readonly static GUIContent headerContent = new GUIContent("<b>Beam Module</b>\n\nThis module enables you to implement beam, ray or laser-type behaviour on the object it is attached to.");

        private readonly static GUIContent damageTypeContent = new GUIContent("Damage Type", "The type of damage the beam does when hitting a ship. " +
            "The amount of damage dealt to a ship upon collision is dependent on the ship's resistance to this damage type. If the " +
            "damage type is set to Default, the ship's damage multipliers are ignored i.e. the damage amount is unchanged.");
        private readonly static GUIContent damageRateContent = new GUIContent("Damage Rate", "The amount of damage this beam does, per second, to the ship or object it hits. NOTE: Non-ship objects need a DamageReceiver component.");

        private readonly static GUIContent speedContent = new GUIContent("Speed", "The speed the beam travels in metres per second");
        private readonly static GUIContent usePoolingContent = new GUIContent("Use Pooling", "Use the Pooling system to manage create, re-use, and destroy beams.");
        private readonly static GUIContent minPoolSizeContent = new GUIContent("Min Pool Size", "When using the Pooling system, this is the number of beam objects kept in reserve for spawning and despawning.");
        private readonly static GUIContent maxPoolSizeContent = new GUIContent("Max Pool Size", "When using the Pooling system, this is the maximum number of beams permitted in the scene at any one time.");
        private readonly static GUIContent beamStartWidthContent = new GUIContent("Beam Width", "The (visual) width of the beam in metres");
        private readonly static GUIContent minBurstDurationContent = new GUIContent("Min Burst Duration", "The minimum amount of time, in seconds, the beam must be active");
        private readonly static GUIContent maxBurstDurationContent = new GUIContent("Max Burst Duration", "The maximum amount of time, in seconds, the beam can be active in a single burst");
        private readonly static GUIContent dischargeDurationContent = new GUIContent("Discharge Duration", "The time (in seconds) it takes a single beam to discharge the beam weapon from full charge");

        private readonly static GUIContent effectsObjectContent = new GUIContent("Effects Object", "The particle and/or sound effect prefab that will be instantiated when the beam hits something. This does not fire when the beam is automatically despawned. For beams, this would typically be a looping effect.");
        private readonly static GUIContent muzzleFXObjectContent = new GUIContent("Muzzle FX Object", "The particle and/or sound effect prefab that will be instantiated when the beam is fired from a weapon.");
        private readonly static GUIContent muzzleFXOffsetContent = new GUIContent("Muzzle FX Offset", "The distance in local space that the muzzle Effects Object should be instantiated from the weapon firing point. Typically only the z-axis will be used.");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugSourceShipIdContent = new GUIContent("Source Ship Id", "The ShipId of the ship that fired this beam");
        private readonly static GUIContent debugSourceSquadronIdContent = new GUIContent("Source Squadron Id", "The Squadron which the ship belonged to when it fired the beam");
        #endregion

        #region Serialized Properties
        private SerializedProperty usePoolingProp;
        private SerializedProperty minPoolSizeProp;
        private SerializedProperty maxPoolSizeProp;
        private SerializedProperty minBurstDurationProp;
        private SerializedProperty maxBurstDurationProp;
        private SerializedProperty dischargeDurationProp;
        private SerializedProperty effectsObjectProp;
        private SerializedProperty muzzleEffectsObjectProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            beamModule = (BeamModule)target;

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            usePoolingProp = serializedObject.FindProperty("usePooling");
            minBurstDurationProp = serializedObject.FindProperty("minBurstDuration");
            maxBurstDurationProp = serializedObject.FindProperty("maxBurstDuration");
            dischargeDurationProp = serializedObject.FindProperty("dischargeDuration");
            effectsObjectProp = serializedObject.FindProperty("effectsObject");
            muzzleEffectsObjectProp = serializedObject.FindProperty("muzzleEffectsObject");

            #endregion
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
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

            #region General Settings

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), speedContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageType"), damageTypeContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageRate"), damageRateContent);
            EditorGUILayout.PropertyField(usePoolingProp, usePoolingContent);

            if (usePoolingProp.boolValue)
            {
                minPoolSizeProp = serializedObject.FindProperty("minPoolSize");
                maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
                EditorGUILayout.PropertyField(minPoolSizeProp, minPoolSizeContent);
                EditorGUILayout.PropertyField(maxPoolSizeProp, maxPoolSizeContent);
                if (minPoolSizeProp.intValue > maxPoolSizeProp.intValue) { maxPoolSizeProp.intValue = minPoolSizeProp.intValue; }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("beamStartWidth"), beamStartWidthContent);

            EditorGUILayout.PropertyField(minBurstDurationProp, minBurstDurationContent);
            EditorGUILayout.PropertyField(maxBurstDurationProp, maxBurstDurationContent);
            if (minBurstDurationProp.floatValue > maxBurstDurationProp.floatValue) { maxBurstDurationProp.floatValue = minBurstDurationProp.floatValue; }

            if (dischargeDurationProp.floatValue < 0.1f) { dischargeDurationProp.floatValue = 0.1f; }

            EditorGUILayout.PropertyField(dischargeDurationProp, dischargeDurationContent);

            if (beamModule.effectsObject != null)
            {
                if (beamModule.effectsObject.despawnTime < beamModule.maxBurstDuration)
                {
                    EditorGUILayout.HelpBox(effectsDespawnWarning, MessageType.Info);
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(effectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(effectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(muzzleFXObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(muzzleEffectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            if (muzzleEffectsObjectProp.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleEffectsOffset"), muzzleFXOffsetContent);
            }

            // Tell users not to add colliders
            if (beamModule.GetComponentInChildren<Collider>() != null)
            {
                EditorGUILayout.HelpBox("Beams should not have colliders attached. Collision detection currently occurs via " +
                    "raycasting to improve performance.", MessageType.Error);
            }

            // Tell users not to add rigidbodies
            if (beamModule.GetComponentInChildren<Rigidbody>() != null)
            {
                EditorGUILayout.HelpBox("Beams should not have rigidbodies attached. Position is currently updated manually to improve performance.", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && beamModule != null)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceShipIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(beamModule.sourceShipId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceSquadronIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(beamModule.sourceSquadronId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion

        }
        #endregion
    }
}
