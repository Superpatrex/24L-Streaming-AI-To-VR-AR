using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2021 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(DestructibleObjectModule))]
    public class DestructibleObjectModuleEditor : Editor
    {
        #region Custom Editor private variables
        private DestructibleObjectModule destructibleObjectModule = null;

        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        // Debugging
        private bool isDebuggingEnabled = false;
        #endregion

        #region GUIContent - General
        private readonly static GUIContent headerContent = new GUIContent("This module can be used to take damage and trigger a DestructModule when the health of the object reaches 0.");

        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Destructible Object Module is enabled through code.");

        private readonly static GUIContent damageStartHealthContent = new GUIContent("Starting Health", "How much 'health' the object has initially.");

        private readonly static GUIContent useShieldingContent = new GUIContent("Use Shielding", "Whether this object uses shielding. Up until a point, shielding protects the object from damage");
        private readonly static GUIContent shieldingDamageThresholdContent = new GUIContent(" Damage Threshold", "Damage below this value will not affect the shield or the object's health while the shield is still active " +
            "(i.e. until the shield has absorbed damage more than or equal to the shielding Amount value from damage events above the " +
            "damage threshold).");
        private readonly static GUIContent shieldingAmountContent = new GUIContent(" Shield Amount", "How much damage the shield can absorb before " +
            "it ceases to protect the object from damage.");
        private readonly static GUIContent shieldingRechargeRateContent = new GUIContent(" Recharge Rate", "The rate per second that a shield will recharge (default = 0)");
        private readonly static GUIContent shieldingRechargeDelayContent = new GUIContent(" Recharge Delay", "The delay, in seconds, between when damage occurs to a shield and it begins to recharge.");

        private readonly static GUIContent destructObjectContent = new GUIContent("Destruct Object", "The destruct prefab that breaks into fragments when the object is destroyed.");
        private readonly static GUIContent destructOffsetContent = new GUIContent("Destruct Offset", "The offset in the forward direction, from the objects gameobject, that the destruct module is instantiated");

        private readonly static GUIContent effectsObjectContent = new GUIContent("Effects Object", "The particle and/or sound effect prefab that will be instantiated when the object is destroyed.");
        private readonly static GUIContent effectsOffsetContent = new GUIContent("Effects Offset", "The offset in the forward direction, from the objects gameobject, that the destruction effect is instantiated");

        private readonly static GUIContent useDamageMultipliersContent = new GUIContent("Use Damage Multipliers", "Whether damage type multipliers are used when calculating damage from projectiles.");
        private readonly static GUIContent typeADamageMultiplierContent = new GUIContent(" Damage Type A", "The relative amount of damage a Type A projectile will inflict on the object.");
        private readonly static GUIContent typeBDamageMultiplierContent = new GUIContent(" Damage Type B", "The relative amount of damage a Type B projectile will inflict on the object.");
        private readonly static GUIContent typeCDamageMultiplierContent = new GUIContent(" Damage Type C", "The relative amount of damage a Type C projectile will inflict on the object.");
        private readonly static GUIContent typeDDamageMultiplierContent = new GUIContent(" Damage Type D", "The relative amount of damage a Type D projectile will inflict on the object.");
        private readonly static GUIContent typeEDamageMultiplierContent = new GUIContent(" Damage Type E", "The relative amount of damage a Type E projectile will inflict on the object.");
        private readonly static GUIContent typeFDamageMultiplierContent = new GUIContent(" Damage Type F", "The relative amount of damage a Type F projectile will inflict on the object.");

        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        private readonly static GUIContent gotoDestructsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Destructs folder");

        // Identity
        private readonly static GUIContent factionIdContent = new GUIContent("Faction Id", "The faction or alliance the object belongs to. This can be used to identify if a object is friend or foe.  Neutral = 0.");
        private readonly static GUIContent squadronIdContent = new GUIContent("Squadron Id", "Although normally representing a squadron of ships, this can be used on a gameobjects to group it with other things in your scene.");
        private readonly static GUIContent isRadarEnabledContent = new GUIContent("Visible to Radar", "Is this object visible to the radar system?");
        private readonly static GUIContent radarBlipSizeContent = new GUIContent("Radar Blip Size", "The relative size of the blip on the radar mini-map.");

        #endregion

        #region GUIContent - Debugging
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display the data about the Destructible Object Module component at runtime in the editor.");
        //private readonly static GUIContent debugNotSetContent = new GUIContent("-", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugHealthContent = new GUIContent("Health");
        private readonly static GUIContent debugUseShieldContent = new GUIContent("Use Shield?");
        private readonly static GUIContent debugShieldContent = new GUIContent("Shield");
        #endregion

        #region Serialized Properties
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty startingHealthProp;
        private SerializedProperty useShieldingProp;
        private SerializedProperty shieldingDamageThresholdProp;
        private SerializedProperty shieldingAmountProp;
        private SerializedProperty shieldingRechargeRateProp;
        private SerializedProperty shieldingRechargeDelayProp;
        private SerializedProperty destructionEffectsObjectProp;
        private SerializedProperty destructionEffectsOffsetProp;
        private SerializedProperty destructObjectProp;
        private SerializedProperty destructOffsetProp;
        private SerializedProperty useDamageMultipliersProp;
        private SerializedProperty factionIdProp;
        private SerializedProperty squadronIdProp;
        private SerializedProperty isRadarEnabledProp;
        private SerializedProperty radarBlipSizeProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            destructibleObjectModule = (DestructibleObjectModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            startingHealthProp = serializedObject.FindProperty("startingHealth");
            useShieldingProp = serializedObject.FindProperty("useShielding");
            shieldingDamageThresholdProp = serializedObject.FindProperty("shieldingDamageThreshold");
            shieldingAmountProp = serializedObject.FindProperty("shieldingAmount");
            shieldingRechargeRateProp = serializedObject.FindProperty("shieldingRechargeRate");
            shieldingRechargeDelayProp = serializedObject.FindProperty("shieldingRechargeDelay");
            destructionEffectsObjectProp = serializedObject.FindProperty("destructionEffectsObject");
            destructionEffectsOffsetProp = serializedObject.FindProperty("destructionEffectsOffset");
            destructObjectProp = serializedObject.FindProperty("destructObject");
            destructOffsetProp = serializedObject.FindProperty("destructObjectOffset");
            useDamageMultipliersProp = serializedObject.FindProperty("useDamageMultipliers");
            factionIdProp = serializedObject.FindProperty("factionId");
            squadronIdProp = serializedObject.FindProperty("squadronId");
            isRadarEnabledProp = serializedObject.FindProperty("isRadarEnabled");
            radarBlipSizeProp = serializedObject.FindProperty("radarBlipSize");

            #endregion
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draw the array of damage multipliers for the object.
        /// Includes support for Editor Undo/Redo.
        /// </summary>
        private void DrawDamageMultipliers()
        {
            // Apply property changes
            serializedObject.ApplyModifiedProperties();
            // Make sure that the damage multipliers array is the correct size
            destructibleObjectModule.VerifyMultiplierArray();
            // Read in the properties
            serializedObject.Update();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
            for (int dTypeIdx = 0; dTypeIdx < 6; dTypeIdx++)
            {
                ProjectileModule.DamageType thisDamageType = ProjectileModule.DamageType.Default;
                GUIContent thisDamageTypeGUIContent = typeADamageMultiplierContent;
                switch (dTypeIdx)
                {
                    case 0:
                        thisDamageType = ProjectileModule.DamageType.TypeA;
                        thisDamageTypeGUIContent = typeADamageMultiplierContent;
                        break;
                    case 1:
                        thisDamageType = ProjectileModule.DamageType.TypeB;
                        thisDamageTypeGUIContent = typeBDamageMultiplierContent;
                        break;
                    case 2:
                        thisDamageType = ProjectileModule.DamageType.TypeC;
                        thisDamageTypeGUIContent = typeCDamageMultiplierContent;
                        break;
                    case 3:
                        thisDamageType = ProjectileModule.DamageType.TypeD;
                        thisDamageTypeGUIContent = typeDDamageMultiplierContent;
                        break;
                    case 4:
                        thisDamageType = ProjectileModule.DamageType.TypeE;
                        thisDamageTypeGUIContent = typeEDamageMultiplierContent;
                        break;
                    case 5:
                        thisDamageType = ProjectileModule.DamageType.TypeF;
                        thisDamageTypeGUIContent = typeFDamageMultiplierContent;
                        break;
                }

                float thisDamageMultiplier = destructibleObjectModule.GetDamageMultiplier(thisDamageType);
                EditorGUI.BeginChangeCheck();
                thisDamageMultiplier = EditorGUILayout.FloatField(thisDamageTypeGUIContent, thisDamageMultiplier);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(destructibleObjectModule, "Modify Damage " + thisDamageType.ToString());
                    destructibleObjectModule.SetDamageMultiplier(thisDamageType, thisDamageMultiplier);
                }
            }
            // Read in the properties
            serializedObject.Update();
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

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            #region General Settings

            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);

            EditorGUILayout.PropertyField(startingHealthProp, damageStartHealthContent);
            EditorGUILayout.PropertyField(useShieldingProp, useShieldingContent);
            if (useShieldingProp.boolValue)
            {
                EditorGUILayout.PropertyField(shieldingDamageThresholdProp, shieldingDamageThresholdContent);
                EditorGUILayout.PropertyField(shieldingAmountProp, shieldingAmountContent);

                EditorGUILayout.PropertyField(shieldingRechargeRateProp, shieldingRechargeRateContent);
                if (shieldingRechargeRateProp.floatValue > 0f)
                {
                    EditorGUILayout.PropertyField(shieldingRechargeDelayProp, shieldingRechargeDelayContent);
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(effectsObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(destructionEffectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(destructionEffectsOffsetProp, effectsOffsetContent);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(destructObjectContent, GUILayout.Width(EditorGUIUtility.labelWidth - 28f));
            if (GUILayout.Button(gotoDestructsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.destructFolder, false, true); }
            EditorGUILayout.PropertyField(destructObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(destructOffsetProp, destructOffsetContent);

            #endregion

            #region Damage Multipliers
            EditorGUILayout.PropertyField(useDamageMultipliersProp, useDamageMultipliersContent);

            if (useDamageMultipliersProp.boolValue)
            {
                DrawDamageMultipliers();
            }
            #endregion

            #region Identity

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(factionIdProp, factionIdContent);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                destructibleObjectModule.SetFactionId(factionIdProp.intValue);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(squadronIdProp, squadronIdContent);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                destructibleObjectModule.SetSquadronId(squadronIdProp.intValue);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isRadarEnabledProp, isRadarEnabledContent);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                if (isRadarEnabledProp.boolValue) { destructibleObjectModule.EnableRadar(); }
                else { destructibleObjectModule.DisableRadar(); }
            }

            EditorGUILayout.PropertyField(radarBlipSizeProp, radarBlipSizeContent);

            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            //DrawDefaultInspector();

            #region Debug Mode
            // NOTE: This is NOT performance optimised - can create GC issues and other performance overhead.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && destructibleObjectModule != null)
            {
                SSCEditorHelper.PerformanceImpact();

                float rightLabelWidth = 150f;
                bool isModuleInitialised = destructibleObjectModule.IsInitialised;

                #region Debugging - General

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isModuleInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

                #region Health

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugHealthContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isModuleInitialised ? destructibleObjectModule.HealthNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugUseShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(destructibleObjectModule.useShielding ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugShieldContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isModuleInitialised ? destructibleObjectModule.ShieldNormalised.ToString("0.0 %") : "---", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        #endregion
    }
}