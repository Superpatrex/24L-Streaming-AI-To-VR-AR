using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipWarpModule))]
    public class ShipWarpModuleEditor : Editor
    {
        #region Custom Editor protected variables
        // These are visible to inherited classes
        protected ShipWarpModule shipWarpModule;
        protected bool isStylesInitialised = false;
        protected bool isSceneModified = false;
        protected string labelText;
        protected GUIStyle labelFieldRichText;
        protected GUIStyle headingFieldRichText;
        protected GUIStyle helpBoxRichText;
        protected GUIStyle buttonCompact;
        protected GUIStyle foldoutStyleNoLabel;
        protected GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        protected GUIStyle toggleCompactButtonStyleToggled = null;
        protected Color separatorColor = new Color();
        protected float defaultEditorLabelWidth = 0f;
        protected float defaultEditorFieldWidth = 0f;
        protected bool isDebuggingEnabled = false;

        protected int cameraSettingsDeletePos = -1;
        #endregion

        #region SceneView Variables

        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A warp drive-like effect that works with a Ship Control Module, Ship Camera Module, and particle systems. Place on its own gameobject in the scene.");
        private readonly static GUIContent[] tabTexts = { new GUIContent("General"), new GUIContent("Ship"), new GUIContent("Camera"), new GUIContent("FX"), new GUIContent("Events") };

        #endregion

        #region GUIContent - General
        protected readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.");
        protected readonly static GUIContent allowCustomInputsContent = new GUIContent(" Allow Custom Inputs", "Allow the user of Custom Player inputs during warp");
        protected readonly static GUIContent offsetFromShipContent = new GUIContent(" Offset from Ship", "The offset, in local space, from warp fx is from the position of the ship");
        protected readonly static GUIContent maxWarpDurationContent = new GUIContent(" Max Warp Duration", "If greater than zero, the time, in seconds, that warp will automatically disengage.");
        protected readonly static GUIContent nightSkyColourContent = new GUIContent(" Night Sky Colour", "The colour of the night sky");
        protected readonly static GUIContent envAmbientSourceContent = new GUIContent(" Ambient Source", "The source of the ambient light. Colour, Gradient or Skybox.");
        protected readonly static GUIContent overrideAmbientColourContent = new GUIContent(" Override Ambient Colour", "By default the ambient sky colour will be set to the Night Sky Colour");
        protected readonly static GUIContent ambientSkyColourContent = new GUIContent(" Ambient Sky Colour", "If overriding the ambient colour, this is the ambient sky colour");
        #endregion

        #region GUIContent - Ship
        protected readonly static GUIContent shipControlModuleContent = new GUIContent(" Ship Control Module", "The module from the scene used to control the player ship.");
        protected readonly static GUIContent shipForwardThrustContent = new GUIContent(" Forward Thrust", "The amount of proportional thrust to apply to forward thrusters when warp is engaged.");

        protected readonly static GUIContent maxShakeStrengthContent = new GUIContent(" Max Shake Strength", "The maximum strength of the ship shake. Smaller numbers are better.");
        protected readonly static GUIContent maxShakeDurationContent = new GUIContent(" Max Shake Duration", "The maximum duration, in seconds. the ship will shake per incident.");
        protected readonly static GUIContent minShakeIntervalContent = new GUIContent(" Min Shake Interval", "The minimum interval, in seconds, between ship shake incidents.");
        protected readonly static GUIContent maxShakeIntervalContent = new GUIContent(" Max Shake Interval", "The maximum interval, in seconds, between ship shake incidents.");

        protected readonly static GUIContent maxShipPitchDownContent = new GUIContent(" Max Pitch Down", "The maximum angle, in degrees, the ship can pitch down.");
        protected readonly static GUIContent maxShipPitchUpContent = new GUIContent(" Max Pitch Up", "The maximum angle, in degrees, the ship can pitch up.");
        protected readonly static GUIContent maxShipPitchDurationContent = new GUIContent(" Max Pitch Duration", "The maximum time, in seconds, the ship will take to pitch up and down.");
        protected readonly static GUIContent shipPitchCurveContent = new GUIContent(" Pitch Curve", "The curve used to evaluate the amount of pitch over the pitch duration of each pitch incident.");

        protected readonly static GUIContent maxShipRollAngleContent = new GUIContent(" Max Roll Angle", "The maximum angle, in degrees, the ship can roll left or right.");
        protected readonly static GUIContent maxShipRollDurationContent = new GUIContent(" Max Roll Duration", "The maximum time, in seconds, the ship will take to roll from left to right.");
        protected readonly static GUIContent shipRollCurveContent = new GUIContent(" Roll Curve", "The curve used to evaluate the amount of roll over the roll duration of each roll incident.");
        #endregion

        #region GUIContent - Camera
        protected readonly static GUIContent shipCameraModuleContent = new GUIContent(" Ship Camera Module", "The module used to control the player ship camera");
        protected readonly static GUIContent isApplyCameraSettingsOnEngageContent = new GUIContent(" Apply Settings on Engage", "If there are optional camera settings configured, apply the first one when warp is engaged.");
        protected readonly static GUIContent cameraSettingsContent = new GUIContent(" Optional Camera Settings");

        #endregion

        #region GUIContent - FX
        protected readonly static GUIContent innerParticleSystemContent = new GUIContent(" Inner Particle System", "The child particle system used to generate the inner or centre particles for the FX");
        protected readonly static GUIContent outerParticleSystemContent = new GUIContent(" Outer Particle System", "The child particle system used to generate the outer particles for the FX");
        protected readonly static GUIContent isSoundFXPausedContent = new GUIContent(" Pause Sound FX", "Are the sound effects currently paused. New new sounds will play until it is unpaused.");
        protected readonly static GUIContent sscSoundFXSetContent = new GUIContent(" Sound FX Set", "A set of SoundFX that are randomly selected while warp is engaged.");
        protected readonly static GUIContent maxSoundIntervalContent = new GUIContent(" Max Sound Interval", "The maximum interval, in seconds, between sound fx when warp is engaged.");
        protected readonly static GUIContent soundFXOffsetContent = new GUIContent(" Sound FX Offset", "The local space relative offset from the ship used when instantiating Sound FX.");
        protected readonly static GUIContent isSoundIntervalRandomisedContent = new GUIContent(" Randomise Sound Volume", "Is the volume randomised between 50 percent of the EffectsModule default volume, and the default volume?");

        #endregion

        #region GUIContent - Events
        protected readonly static GUIContent onChangeCameraSettingsContent = new GUIContent("On Change Camera Settings");
        protected readonly static GUIContent onPreEngageWarpContent = new GUIContent("On Pre Engage Warp");
        protected readonly static GUIContent onPostEngageWarpContent = new GUIContent("On Post Engage Warp");
        protected readonly static GUIContent onPreDisengageWarpContent = new GUIContent("On Pre Disengage Warp");
        protected readonly static GUIContent onPostDisengageWarpContent = new GUIContent("On Post Disengage Warp");


        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to display the data about the ShipWarpModule component at runtime in the editor.");
        //private readonly static GUIContent debugNotSetContent = new GUIContent("-", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugIsWarpEngagedContent = new GUIContent(" Is Warp Engaged?");
        private readonly static GUIContent debugCurrentCameraSettingsIndexContent = new GUIContent(" Camera Settings");

        #endregion

        #region Serialized Properties - General
        protected SerializedProperty selectedTabIntProp;
        protected SerializedProperty initialiseOnStartProp;
        protected SerializedProperty allowCustomInputsProp;
        protected SerializedProperty offsetFromShipProp;
        protected SerializedProperty maxWarpDurationProp;
        protected SerializedProperty nightSkyColourProp;
        protected SerializedProperty envAmbientSourceProp;
        protected SerializedProperty overrideAmbientColourProp;
        protected SerializedProperty ambientSkyColourProp;

        #endregion

        #region Serialized Properties - Ship
        protected SerializedProperty shipControlModuleProp;
        protected SerializedProperty shipForwardThrustProp;
        protected SerializedProperty maxShakeStrengthProp;
        protected SerializedProperty maxShakeDurationProp;
        protected SerializedProperty minShakeIntervalProp;
        protected SerializedProperty maxShakeIntervalProp;
        protected SerializedProperty maxShipPitchDownProp;
        protected SerializedProperty maxShipPitchUpProp;
        protected SerializedProperty maxShipPitchDurationProp;
        protected SerializedProperty shipPitchCurveProp;
        protected SerializedProperty maxShipRollAngleProp;
        protected SerializedProperty maxShipRollDurationProp;
        protected SerializedProperty shipRollCurveProp;

        #endregion

        #region Serialized Properties - Camera
        protected SerializedProperty shipCameraModuleProp;
        protected SerializedProperty isApplyCameraSettingsOnEngageProp;
        protected SerializedProperty shipCameraSettingsListProp;
        protected SerializedProperty shipCameraSettingsProp;

        #endregion

        #region Serialized Properties - FX
        protected SerializedProperty innerParticleSystemProp;
        protected SerializedProperty outerParticleSystemProp;
        protected SerializedProperty isSoundFXPausedProp;
        protected SerializedProperty sscSoundFXSetProp;
        protected SerializedProperty maxSoundIntervalProp;
        protected SerializedProperty soundFXOffsetProp;
        protected SerializedProperty isSoundIntervalRandomisedProp;

        #endregion

        #region Serialized Properties - Events
        protected SerializedProperty onChangeCameraSettingsProp;
        protected SerializedProperty onPreEngageWarpProp;
        protected SerializedProperty onPostEngageWarpProp;
        protected SerializedProperty onPreDisengageWarpProp;
        protected SerializedProperty onPostDisengageWarpProp;
        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            shipWarpModule = (ShipWarpModule)target;

            defaultEditorLabelWidth = 185f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            allowCustomInputsProp = serializedObject.FindProperty("allowCustomInputs");
            offsetFromShipProp = serializedObject.FindProperty("offsetFromShip");
            maxWarpDurationProp = serializedObject.FindProperty("maxWarpDuration");
            nightSkyColourProp = serializedObject.FindProperty("nightSkyColour");
            envAmbientSourceProp = serializedObject.FindProperty("envAmbientSource");
            overrideAmbientColourProp = serializedObject.FindProperty("overrideAmbientColour");
            ambientSkyColourProp = serializedObject.FindProperty("ambientSkyColour");

            #endregion

            #region Find Properties - Ship
            shipControlModuleProp = serializedObject.FindProperty("shipControlModule");
            shipForwardThrustProp = serializedObject.FindProperty("shipForwardThrust");
            maxShakeStrengthProp = serializedObject.FindProperty("maxShakeStrength");
            maxShakeDurationProp = serializedObject.FindProperty("maxShakeDuration");
            minShakeIntervalProp = serializedObject.FindProperty("minShakeInterval");
            maxShakeIntervalProp = serializedObject.FindProperty("maxShakeInterval");
            maxShipPitchDownProp = serializedObject.FindProperty("maxShipPitchDown");
            maxShipPitchUpProp = serializedObject.FindProperty("maxShipPitchUp");
            maxShipPitchDurationProp = serializedObject.FindProperty("maxShipPitchDuration");
            shipPitchCurveProp = serializedObject.FindProperty("shipPitchCurve");
            maxShipRollAngleProp = serializedObject.FindProperty("maxShipRollAngle");
            maxShipRollDurationProp = serializedObject.FindProperty("maxShipRollDuration");
            shipRollCurveProp = serializedObject.FindProperty("shipRollCurve");
            #endregion

            #region Find Properties - Camera
            shipCameraModuleProp = serializedObject.FindProperty("shipCameraModule");
            isApplyCameraSettingsOnEngageProp = serializedObject.FindProperty("isApplyCameraSettingsOnEngage");
            shipCameraSettingsListProp = serializedObject.FindProperty("shipCameraSettingsList");
            #endregion

            #region Find Properties - FX
            innerParticleSystemProp = serializedObject.FindProperty("innerParticleSystem");
            outerParticleSystemProp = serializedObject.FindProperty("outerParticleSystem");
            isSoundFXPausedProp = serializedObject.FindProperty("isSoundFXPaused");
            sscSoundFXSetProp = serializedObject.FindProperty("sscSoundFXSet");
            maxSoundIntervalProp = serializedObject.FindProperty("maxSoundInterval");
            soundFXOffsetProp = serializedObject.FindProperty("soundFXOffset");
            isSoundIntervalRandomisedProp = serializedObject.FindProperty("isSoundIntervalRandomised");
            #endregion

            #region Find Properties - Events
            onChangeCameraSettingsProp = serializedObject.FindProperty("onChangeCameraSettings");
            onPreEngageWarpProp = serializedObject.FindProperty("onPreEngageWarp");
            onPostEngageWarpProp = serializedObject.FindProperty("onPostEngageWarp");
            onPreDisengageWarpProp = serializedObject.FindProperty("onPreDisengageWarp");
            onPostDisengageWarpProp = serializedObject.FindProperty("onPostDisengageWarp");
            #endregion
        }

        #endregion

        #region Private and Protected Methods


        /// <summary>
        /// Set up the buttons and styles used in OnInspectorGUI.
        /// Call this near the top of OnInspectorGUI.
        /// </summary>
        protected void ConfigureButtonsAndStyles()
        {
            // Set up rich text GUIStyles
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;

                headingFieldRichText = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                headingFieldRichText.richText = true;
                headingFieldRichText.normal.textColor = helpBoxRichText.normal.textColor;

                // Overide default styles
                EditorStyles.foldout.fontStyle = FontStyle.Bold;

                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;

                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;

                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;

                isStylesInitialised = true;
            }
        }

        /// <summary>
        /// Draw enable or disable debugging in the inspector
        /// </summary>
        protected void DrawDebugToggle()
        {
            isDebuggingEnabled = EditorGUILayout.Toggle(SSCEditorHelper.debugModeIndent1Content, isDebuggingEnabled);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// This function overides what is normally seen in the inspector window
        /// This allows stuff like buttons to be drawn there
        /// </summary>
        protected virtual void DrawBaseInspector()
        {
            #region Initialise
            shipWarpModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Headers and Info Buttons
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SSCEditorHelper.DrawSSCGetHelpButtons(buttonCompact);
            DrawToolBar(tabTexts);
            EditorGUILayout.EndVertical();
            #endregion

            SSCEditorHelper.InTechPreview(false);

            EditorGUILayout.BeginVertical("HelpBox");

            #region General Settings
            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
                EditorGUILayout.PropertyField(allowCustomInputsProp, allowCustomInputsContent);
                EditorGUILayout.PropertyField(offsetFromShipProp, offsetFromShipContent);
                EditorGUILayout.PropertyField(maxWarpDurationProp, maxWarpDurationContent);
                EditorGUILayout.PropertyField(nightSkyColourProp, nightSkyColourContent);
                EditorGUILayout.PropertyField(envAmbientSourceProp, envAmbientSourceContent);
                EditorGUILayout.PropertyField(overrideAmbientColourProp, overrideAmbientColourContent);
                EditorGUILayout.PropertyField(ambientSkyColourProp, ambientSkyColourContent);
            }
            #endregion

            #region Ship Settings
            else if (selectedTabIntProp.intValue == 1)
            {
                DrawShipSettings();
            }
            #endregion

            #region Camera Settings
            else if (selectedTabIntProp.intValue == 2)
            {
                DrawCameraSettings();
            }
            #endregion

            #region FX Settings
            else if (selectedTabIntProp.intValue == 3)
            {
                DrawParticleSettings();
                DrawSoundFXSettings();
            }
            #endregion

            #region Event Settings
            else if (selectedTabIntProp.intValue == 4)
            {
                DrawEventSettings();
            }

            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            shipWarpModule.allowRepaint = true;

            #region Debug Mode
            DrawDebugSettings();
            #endregion
        }

        /// <summary>
        /// Draw the event settings in the inspector
        /// </summary>
        protected virtual void DrawEventSettings()
        {
            EditorGUILayout.PropertyField(onChangeCameraSettingsProp, onChangeCameraSettingsContent);
            EditorGUILayout.PropertyField(onPreEngageWarpProp, onPreEngageWarpContent);
            EditorGUILayout.PropertyField(onPostEngageWarpProp, onPostEngageWarpContent);
            EditorGUILayout.PropertyField(onPreDisengageWarpProp, onPreDisengageWarpContent);
            EditorGUILayout.PropertyField(onPostDisengageWarpProp, onPostDisengageWarpContent);
        }

        /// <summary>
        /// Draw the settings for the particle systems in the inspector
        /// </summary>
        protected virtual void DrawParticleSettings()
        {
            EditorGUILayout.PropertyField(innerParticleSystemProp, innerParticleSystemContent);
            EditorGUILayout.PropertyField(outerParticleSystemProp, outerParticleSystemContent);
        }

        /// <summary>
        /// Draw the settings for the camera in the inspector
        /// </summary>
        protected virtual void DrawCameraSettings()
        {
            EditorGUILayout.PropertyField(shipCameraModuleProp, shipCameraModuleContent);
            EditorGUILayout.PropertyField(isApplyCameraSettingsOnEngageProp, isApplyCameraSettingsOnEngageContent);

            cameraSettingsDeletePos = -1;
            int numCamSettings = shipCameraSettingsListProp.arraySize;

            #region Add or Remove Camera Settings
            SSCEditorHelper.DrawSSCHorizontalGap(4f);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(cameraSettingsContent);

            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
            {
                shipCameraSettingsListProp.arraySize++;
                numCamSettings = shipCameraSettingsListProp.arraySize;
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
            {
                if (numCamSettings > 0) { cameraSettingsDeletePos = shipCameraSettingsListProp.arraySize - 1; }
            }
            GUILayout.EndHorizontal();

            #endregion

            #region Camera Settings List

            for (int csIdx = 0; csIdx < numCamSettings; csIdx++)
            {
                shipCameraSettingsProp = shipCameraSettingsListProp.GetArrayElementAtIndex(csIdx);

                if (shipCameraSettingsProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (csIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUILayout.PropertyField(shipCameraSettingsProp, GUIContent.none);

                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { cameraSettingsDeletePos = csIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete Camera Settings
            if (cameraSettingsDeletePos >= 0)
            {
                shipCameraSettingsListProp.DeleteArrayElementAtIndex(cameraSettingsDeletePos);
                cameraSettingsDeletePos = -1;

                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                GUIUtility.ExitGUI();
            }
            #endregion
        }

        protected virtual void DrawDebugSettings()
        {
            // NOTE: This is NOT performance optimised - can create GC issues and other performance overhead.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && shipWarpModule != null)
            {
                #region Debugging - General

                SSCEditorHelper.PerformanceImpact();

                float rightLabelWidth = 150f;
                bool isWarpInitialised = shipWarpModule.IsInitialised;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isWarpInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsWarpEngagedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipWarpModule.IsWarpEngaged ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

                #region Debugging - Camera

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCurrentCameraSettingsIndexContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipWarpModule.CurrentCameraSettingsIndex < 0 ? "Not Set" : (shipWarpModule.CurrentCameraSettingsIndex+1).ToString("00"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the settings for the ship in the inspector
        /// </summary>
        protected virtual void DrawShipSettings()
        {
            EditorGUILayout.PropertyField(shipControlModuleProp, shipControlModuleContent);

            #region Thrusters
            EditorGUILayout.PropertyField(shipForwardThrustProp, shipForwardThrustContent);
            #endregion

            #region Shake
            SSCEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(maxShakeStrengthProp, maxShakeStrengthContent);
            EditorGUILayout.PropertyField(maxShakeDurationProp, maxShakeDurationContent);            

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(minShakeIntervalProp, minShakeIntervalContent);
            if (EditorGUI.EndChangeCheck() && maxShakeIntervalProp.floatValue < minShakeIntervalProp.floatValue)
            {
                maxShakeIntervalProp.floatValue = minShakeIntervalProp.floatValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxShakeIntervalProp, maxShakeIntervalContent);
            if (EditorGUI.EndChangeCheck() && maxShakeIntervalProp.floatValue < minShakeIntervalProp.floatValue)
            {
                minShakeIntervalProp.floatValue = maxShakeIntervalProp.floatValue;
            }
            #endregion

            #region Pitch
            SSCEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(maxShipPitchDownProp, maxShipPitchDownContent);
            EditorGUILayout.PropertyField(maxShipPitchUpProp, maxShipPitchUpContent);
            EditorGUILayout.PropertyField(maxShipPitchDurationProp, maxShipPitchDurationContent);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(shipPitchCurveContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(SSCEditorHelper.btnResetContent, buttonCompact, GUILayout.Width(20f)))
            {
                Undo.RecordObject(shipWarpModule, "Reset Pitch Curve");
                serializedObject.ApplyModifiedProperties();
                shipWarpModule.SetShipPitchCurve(ShipWarpModule.GetDefaultPitchCurve());
                serializedObject.Update();
            }
            EditorGUILayout.PropertyField(shipPitchCurveProp, GUIContent.none);
            GUILayout.EndVertical();
            #endregion

            #region Roll
            SSCEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(maxShipRollAngleProp, maxShipRollAngleContent);
            EditorGUILayout.PropertyField(maxShipRollDurationProp, maxShipRollDurationContent);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(shipRollCurveContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(SSCEditorHelper.btnResetContent, buttonCompact, GUILayout.Width(20f)))
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(shipWarpModule, "Reset Roll Curve");
                shipWarpModule.SetShipRollCurve(ShipWarpModule.GetDefaultRollCurve());
                serializedObject.Update();
            }
            EditorGUILayout.PropertyField(shipRollCurveProp, GUIContent.none);
            #endregion

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the settings for sound fx in the inspector
        /// </summary>
        protected virtual void DrawSoundFXSettings()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isSoundFXPausedProp, isSoundFXPausedContent);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                if (isSoundFXPausedProp.boolValue) { shipWarpModule.PauseSoundFX(); }
                else { shipWarpModule.UnpauseSoundFX(); }
                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(sscSoundFXSetProp, sscSoundFXSetContent);
            EditorGUILayout.PropertyField(maxSoundIntervalProp, maxSoundIntervalContent);
            EditorGUILayout.PropertyField(soundFXOffsetProp, soundFXOffsetContent);
            EditorGUILayout.PropertyField(isSoundIntervalRandomisedProp, isSoundIntervalRandomisedContent);
        }

        /// <summary>
        /// Draw the toolbar using the supplied array of tab text.
        /// </summary>
        /// <param name="tabGUIContent"></param>
        protected virtual void DrawToolBar(GUIContent[] tabGUIContent)
        {
            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabGUIContent);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }
        }

        #endregion

        #region OnInspectorGUI

        public override void OnInspectorGUI()
        {
            DrawBaseInspector();
        }

        #endregion
    }
}