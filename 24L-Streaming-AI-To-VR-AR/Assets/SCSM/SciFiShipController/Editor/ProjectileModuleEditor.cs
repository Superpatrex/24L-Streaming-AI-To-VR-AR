using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ProjectileModule))]
    public class ProjectileModuleEditor : Editor
    {
        #region Custom Editor private variables

        private ProjectileModule projectileModule;

        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        #endregion

        #region GUIContent

        private readonly static GUIContent headerContent = new GUIContent("<b>Projectile Module</b>\n\nThis module enables you to implement projectile behaviour on the object it is attached to.");
        private readonly static GUIContent startSpeedContent = new GUIContent("Start Speed", "The starting speed of the projectile when it is launched.");
        private readonly static GUIContent useGravityContent = new GUIContent("Use Gravity", "Whether gravity is applied to the projectile.");
        private readonly static GUIContent damageTypeContent = new GUIContent("Damage Type", "The type of damage the projectile does when hitting a ship. " +
            "The amount of damage dealt to a ship upon collision is dependent on the ship's resistance to this damage type. If the " +
            "damage type is set to Default, the ship's damage multipliers are ignored i.e. the damage amount is unchanged.");
        private readonly static GUIContent damageAmountContent = new GUIContent("Damage Amount", "The amount of damage the projectile does on collision with a ship or object. NOTE: Non-ship objects need a DamageReceiver component.");
        private readonly static GUIContent collisionLayerMaskContent = new GUIContent("Collision Mask", "The layer mask used for collision testing for this projectile. Default: Everything");
        private readonly static GUIContent useECSContent = new GUIContent("Use DOTS", "Use Data-Oriented Technology Stack which uses the Entity Component System and Job System to create and destroy projectiles. Has no effect if Unity 2019.1, ECS, and Jobs is not installed.");
        private readonly static GUIContent usePoolingContent = new GUIContent("Use Pooling", "Use the Pooling system to manage create, re-use, and destroy projectiles.");
        private readonly static GUIContent minPoolSizeContent = new GUIContent("Min Pool Size", "When using the Pooling system, this is the number of projectile objects kept in reserve for spawning and despawning.");
        private readonly static GUIContent maxPoolSizeContent = new GUIContent("Max Pool Size", "When using the Pooling system, this is the maximum number of projectiles permitted in the scene at any one time.");
        private readonly static GUIContent despawnTimeContent = new GUIContent("Despawn Time", "If the projectile has not collided with something before this time (in seconds), it is automatically despawned or removed from the scene.");
        private readonly static GUIContent isKinematicGuideToTargetContent = new GUIContent("Guide to Target", "Rather than being fire and forget, is this projectile guided to a target with kinematics?");
        private readonly static GUIContent guidedMaxTurnSpeedContent = new GUIContent("Guided Max Turn Speed", "The max turning speed in degrees per second for a guided projectile.");
        private readonly static GUIContent effectsObjectContent = new GUIContent("Effects Object", "The particle and/or sound effect prefab that will be instantiated when the projectile hits something and is destroyed. This does not fire when the projectile is automatically despawned.");
        private readonly static GUIContent shieldEffectsObjectContent = new GUIContent("Shield Effects Object", "The particle and/or sound effect prefab that will be instantiated, instead of the regular Effects Object, when the projectile hits a shielded ship. This does not fire when the projectile is automatically despawned.");
        private readonly static GUIContent muzzleFXObjectContent = new GUIContent("Muzzle FX Object", "The particle and/or sound effect prefab that will be instantiated when the projectile is fired from a weapon.");
        private readonly static GUIContent muzzleFXOffsetContent = new GUIContent("Muzzle FX Offset", "The distance in local space that the muzzle Effects Object should be instantiated from the weapon firing point. Typically only the z-axis will be used.");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugSourceShipIdContent = new GUIContent("Source Ship Id", "The ShipId of the ship that fired this projectile");
        private readonly static GUIContent debugSourceSquadronIdContent = new GUIContent("Source Squadron Id", "The Squadron which the ship belonged to when it fired the projectile");
        private readonly static GUIContent debugEstimatedRangeContent = new GUIContent("Estimated Range", "The estimated range (in metres) of this projectile assuming it travels at a constant velocity");
        private readonly static GUIContent debugVelocityContent = new GUIContent("Current Velocity", "Current velocity of the projectile.");
        private readonly static GUIContent debugTargetShipContent = new GUIContent("Target Ship", "If a ship is being targeted, will return its name. If it is being targeted but is NULL, will assume destroyed.");
        private readonly static GUIContent debugTargetShipDamageRegionContent = new GUIContent("Target Damage Region", "If a ship's damage region is being targeted, will return its name. If it is being targeted but the ship is NULL, will assume destroyed.");
        private readonly static GUIContent debugTargetGameObjectContent = new GUIContent("Target GameObject", "If a gameobject is being targeted, will return its name");
        #endregion

        #region Serialized Properties

        private SerializedProperty useECSProp;
        private SerializedProperty usePoolingProp;
        private SerializedProperty minPoolSizeProp;
        private SerializedProperty maxPoolSizeProp;
        private SerializedProperty isKinematicGuideToTargetProp;
        private SerializedProperty effectsObjectProp;
        private SerializedProperty shieldEffectsObjectProp;
        private SerializedProperty muzzleEffectsObjectProp;
        private SerializedProperty collisionLayerMaskProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            projectileModule = (ProjectileModule)target;

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties

            collisionLayerMaskProp = serializedObject.FindProperty("collisionLayerMask");
            useECSProp = serializedObject.FindProperty("useECS");
            usePoolingProp = serializedObject.FindProperty("usePooling");
            effectsObjectProp = serializedObject.FindProperty("effectsObject");
            shieldEffectsObjectProp = serializedObject.FindProperty("shieldEffectsObject");
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

            EditorGUILayout.PropertyField(serializedObject.FindProperty("startSpeed"), startSpeedContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useGravity"), useGravityContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageType"), damageTypeContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageAmount"), damageAmountContent);
            EditorGUILayout.PropertyField(collisionLayerMaskProp, collisionLayerMaskContent);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useECSProp, useECSContent);
            // ECS and Pooling are mutually exclusive
            if (EditorGUI.EndChangeCheck() && usePoolingProp.boolValue && useECSProp.boolValue) { usePoolingProp.boolValue = false; }
            #if !SSC_ENTITIES
            if (useECSProp.boolValue) { EditorGUILayout.HelpBox("Entity Component System is only supported on 2019.1 or newer. Consult Help or Get Support to install the correct packages.", MessageType.Error); }
            #endif
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(usePoolingProp, usePoolingContent);
            // ECS and Pooling are mutually exclusive
            if (EditorGUI.EndChangeCheck() && usePoolingProp.boolValue && useECSProp.boolValue) { useECSProp.boolValue = false; }
            if (usePoolingProp.boolValue)
            {
                minPoolSizeProp = serializedObject.FindProperty("minPoolSize");
                maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
                EditorGUILayout.PropertyField(minPoolSizeProp, minPoolSizeContent);
                EditorGUILayout.PropertyField(maxPoolSizeProp, maxPoolSizeContent);
                if (minPoolSizeProp.intValue > maxPoolSizeProp.intValue) { maxPoolSizeProp.intValue = minPoolSizeProp.intValue; }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("despawnTime"), despawnTimeContent);
            if (!useECSProp.boolValue)
            {
                isKinematicGuideToTargetProp = serializedObject.FindProperty("isKinematicGuideToTarget");
                EditorGUILayout.PropertyField(isKinematicGuideToTargetProp, isKinematicGuideToTargetContent);
                if (isKinematicGuideToTargetProp.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("guidedMaxTurnSpeed"), guidedMaxTurnSpeedContent);
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(effectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(effectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(shieldEffectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(shieldEffectsObjectProp, GUIContent.none);
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
            if (projectileModule.GetComponentInChildren<Collider>() != null)
            {
                EditorGUILayout.HelpBox("Projectiles should not have colliders attached. Collision detection currently occurs via " +
                    "raycasting to improve performance.", MessageType.Error);
            }

            // Tell users not to add rigidbodies
            if (projectileModule.GetComponentInChildren<Rigidbody>() != null)
            {
                EditorGUILayout.HelpBox("Projectiles should not have rigidbodies attached. Position is currently updated manually to improve performance.", MessageType.Error);
            }

            EditorGUILayout.EndVertical();

            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);

            if (isDebuggingEnabled && projectileModule != null)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceShipIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.sourceShipId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceSquadronIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.sourceSquadronId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugEstimatedRangeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.estimatedRange.ToString("0.0"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugVelocityContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.Velocity.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.TargetShipName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipDamageRegionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.TargetShipDamageRegionName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetGameObjectContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(projectileModule.TargetGameObjectName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            #endregion
        }

        #endregion
    }
}
