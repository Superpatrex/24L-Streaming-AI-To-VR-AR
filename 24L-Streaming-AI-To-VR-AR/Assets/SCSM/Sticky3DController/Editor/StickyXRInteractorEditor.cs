using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyXRInteractor))]
    public class StickyXRInteractorEditor : Editor
    {
        #region Custom Editor private variables
        private StickyXRInteractor stickyXRInteractor;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private GUIStyle toggleCompactButtonStyleToggled = null;
        private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private bool isDebuggingShowLocationVolume = false;

        private string tempDisplayTargetName = string.Empty;

        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This enables VR hands on a Sticky3D character to interact with interactive-enabled objects in the scene");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent generalSettingsContent = new GUIContent("General Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky XR Interactor component is enabled through code.");
        private readonly static GUIContent stickyControlModuleContent = new GUIContent(" Sticky Control Module", "The Sticky3D character this interactor is a child of");
        private readonly static GUIContent beamStartWidthContent = new GUIContent(" Beam Width", "The width of the beam or ray displayed in the scene");
        private readonly static GUIContent interactorTypeContent = new GUIContent(" Interactor Type", "Left or right hand");
        private readonly static GUIContent lookModeContent = new GUIContent(" Look Mode", "The method the interactor uses to interact with objects in the scene");
        private readonly static GUIContent maxDistanceContent = new GUIContent(" Max Distance", "Maximum distance the interactor can see ahead");
        private readonly static GUIContent defaultBeamGradientContent = new GUIContent(" Default Beam Colour", "The default colour gradient of the StickyXRInterctor beam");
        private readonly static GUIContent activeBeamGradientContent = new GUIContent(" Active Beam Colour", "The colour gradient of the StickyXRInteractor beam when it is interacting with an object in the scene");
        #endregion

        #region GUIContent - Interactive
        private readonly static GUIContent interactiveSettingsContent = new GUIContent("Interactive Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent palmTransformContent = new GUIContent(" Palm Transform", "This needs to be a child transform of the hand. The z-axis (forward) should point in the direction of the palm is facing. This is the palm “normal”. The x-axis (right) is towards the direction of the index finger and, for the right hand, the y-axis (up) is in a thumbs-up direction.");
        private readonly static GUIContent isPointToGrabContent = new GUIContent(" Point to Grab", "When LookMode is Interactive, the Target sprite or beam can be pointed at a grabbable interactive-enabled object.");
        private readonly static GUIContent isPointToTouchContent = new GUIContent(" Point to Touch", "When LookMode is Interactive, the Target sprite or beam can be pointed at a touchable interactive-enabled object.");
        private readonly static GUIContent throwStrengthContent = new GUIContent(" Throw Strength", "When an interactive-enabled object is dropped, the velocity of the hand is applied to it. The strength of the throw, applies more or less velocity to the object. This enables your character to have more strength in one hand that the other.");
        private readonly static GUIContent interactiveTargetSpriteContent = new GUIContent(" Target Sprite", "The sprite that will be used instead of the pointer beam when the Point Mode is Target.");
        private readonly static GUIContent defaultTargetColourContent = new GUIContent(" Default Target Colour", "The default colour of the pointer reticle when the Point Mode is Target");
        private readonly static GUIContent activeTargetColourContent = new GUIContent(" Active Target Colour", "The colour of the StickyXRInteractor Target when it is interacting with an object in the scene");
        private readonly static GUIContent targetOffsetDistanceNContent = new GUIContent(" Target Offset", "When the Point Mode is Target, this is the normalised distance the Target sprite is moved toward the hand away from the object or obstacle to help prevent clipping.");
        private readonly static GUIContent interactivePointModeContent = new GUIContent(" Point Mode", "The visual pointing mode used with a LookMode of Interactive.");
        private readonly static GUIContent isPointWeaponHeldContent = new GUIContent(" Point if Weapon Held", "Is pointing permitted when a weapon is held? By default, this is disabled so that the same button can be configured for both pointing and firing a weapon. For example, a controller trigger.");

        #endregion

        #region GUIContent - Teleport
        private readonly static GUIContent teleportSettingsContent = new GUIContent("Teleport Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent beamStartCurveContent = new GUIContent(" Beam Start Curve", "The normalised point along the beam at which the curve begins when the LookMode is Teleport.");
        private readonly static GUIContent teleportReticleContent = new GUIContent(" Teleport Reticle", "This is the prefab that is instantiated in the scene and is enabled and disabled during Teleport activities.");
        private readonly static GUIContent isShowTeleportReticleNormalContent = new GUIContent(" Show Reticle Normal", "Should the teleporter reticle show the destination ground normal?");
        private readonly static GUIContent isAutoDisableTeleporterContent = new GUIContent(" Auto Disable Teleporter", "Is teleporting disabled after the character is teleported to a new location?");
        private readonly static GUIContent isTeleportObstacleCheckContent = new GUIContent(" Teleport Obstacle Check", "When teleporting, check if the character would fit into the space above the location. Currently does not check for S3D characters at that location.");

        #endregion

        #region GUIContent - Debug General
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugNotSetContent = new GUIContent("--", "not set");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        #endregion

        #region GUIContent - Debug Teleporting
        private readonly static GUIContent debugIsTeleporterEnabledContent = new GUIContent(" Is Teleporter On?");
        private readonly static GUIContent debugTeleportLocationContent = new GUIContent("  Teleportation Location");
        private readonly static GUIContent debugIsTeleportLocationValidContent = new GUIContent("  Is Location Valid?");
        private readonly static GUIContent debugShowLocationVolumeContent = new GUIContent("  Show Location Volume", "Draw the approximate volume of the character in the scene at the Teleporter location");
        #endregion

        #region GUIContent - Debug Interactive
        private readonly static GUIContent debugIsLookInteractiveEnabledContent = new GUIContent(" Is Interactive On?");
        private readonly static GUIContent debugIsLookingAtInteractiveContent = new GUIContent("  Looking At");
        private readonly static GUIContent debugIsLookingAtPointContent = new GUIContent("  Focusing On");
        private readonly static GUIContent debugNumSelectedInteractiveContent = new GUIContent(" Selected Interactives");

        #endregion

        #region Serialized Properties - General
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty stickyControlModuleProp;
        private SerializedProperty beamStartWidthProp;
        private SerializedProperty interactorTypeProp;
        private SerializedProperty lookModeProp;
        private SerializedProperty maxDistanceProp;        
        private SerializedProperty defaultBeamGradientProp;
        private SerializedProperty activeBeamGradientProp;
        #endregion

        #region Serialized Properties - Interactive
        private SerializedProperty showInteractiveSettingsInEditorProp;
        private SerializedProperty palmTransformProp;
        private SerializedProperty isPointToGrabProp;
        private SerializedProperty isPointToTouchProp;
        private SerializedProperty throwStrengthProp;
        private SerializedProperty interactiveTargetSpriteProp;
        private SerializedProperty defaultTargetColourProp;
        private SerializedProperty activeTargetColourProp;
        private SerializedProperty targetOffsetDistanceNProp;
        private SerializedProperty interactivePointModeProp;
        private SerializedProperty isPointWeaponHeldProp;

        #endregion

        #region Serialized Properties - Teleport
        private SerializedProperty showTeleportSettingsInEditorProp;
        private SerializedProperty beamStartCurveProp;
        private SerializedProperty teleportReticleProp;
        private SerializedProperty isShowTeleportReticleNormalProp;
        private SerializedProperty isAutoDisableTeleporterProp;
        private SerializedProperty isTeleportObstacleCheckProp;

        #endregion

        #region Events

        private void OnEnable()
        {
            stickyXRInteractor = (StickyXRInteractor)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;

            #region Find Properties - General
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            stickyControlModuleProp = serializedObject.FindProperty("stickyControlModule");
            interactorTypeProp = serializedObject.FindProperty("interactorType");
            lookModeProp = serializedObject.FindProperty("lookMode");
            beamStartWidthProp = serializedObject.FindProperty("beamStartWidth");
            maxDistanceProp = serializedObject.FindProperty("maxDistance");
            
            defaultBeamGradientProp = serializedObject.FindProperty("defaultBeamGradient");
            activeBeamGradientProp = serializedObject.FindProperty("activeBeamGradient");
            #endregion

            #region Find Properties - Interactive
            showInteractiveSettingsInEditorProp = serializedObject.FindProperty("showInteractiveSettingsInEditor");
            palmTransformProp = serializedObject.FindProperty("palmTransform");
            isPointToGrabProp = serializedObject.FindProperty("isPointToGrab");
            isPointToTouchProp = serializedObject.FindProperty("isPointToTouch");
            throwStrengthProp = serializedObject.FindProperty("throwStrength");
            interactiveTargetSpriteProp = serializedObject.FindProperty("interactiveTargetSprite");
            defaultTargetColourProp = serializedObject.FindProperty("defaultTargetColour");
            activeTargetColourProp = serializedObject.FindProperty("activeTargetColour");
            targetOffsetDistanceNProp = serializedObject.FindProperty("targetOffsetDistanceN");
            interactivePointModeProp = serializedObject.FindProperty("interactivePointMode");
            isPointWeaponHeldProp = serializedObject.FindProperty("isPointWeaponHeld");
            #endregion

            #region Find Properties - Teleport
            showTeleportSettingsInEditorProp = serializedObject.FindProperty("showTeleportSettingsInEditor");
            beamStartCurveProp = serializedObject.FindProperty("beamStartCurve");
            teleportReticleProp = serializedObject.FindProperty("teleportReticle");
            isShowTeleportReticleNormalProp = serializedObject.FindProperty("isShowTeleportReticleNormal");
            isAutoDisableTeleporterProp = serializedObject.FindProperty("isAutoDisableTeleporter");
            isTeleportObstacleCheckProp = serializedObject.FindProperty("isTeleportObstacleCheck");
            #endregion
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneGUI;
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// </summary>
        void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (stickyXRInteractor.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This lets us modify and display things in the scene view
        /// </summary>
        /// <param name="sv"></param>
        private void SceneGUI(SceneView sv)
        {
            if (stickyXRInteractor != null && stickyXRInteractor.gameObject.activeInHierarchy)
            {
                #region When Debugging is enabled
                if (isDebuggingEnabled && isDebuggingShowLocationVolume)
                {
                    StickyControlModule stickyControlModule = stickyXRInteractor.stickyControlModule;
                    if (stickyControlModule != null)
                    {
                        // Use local space rather than world space for the volume wire cube.
                        // This allows the handle to rotate with the character.
                        using (new Handles.DrawingScope(stickyXRInteractor.IsTeleportLocationValid ? Color.yellow : Color.red, stickyControlModule.transform.localToWorldMatrix))
                        {
                            // Get the teleport location in local space
                            Vector3 _locationOffset = stickyControlModule.GetLocalPosition(stickyXRInteractor.TeleportLocation);

                            // Centre the volume based on the pivot point of the character
                            _locationOffset.y += stickyControlModule.pivotToCentreOffsetY;

                            Handles.DrawWireCube(_locationOffset, stickyControlModule.ScaledSize);

                        }
                    }
                }
                #endregion
            }
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            stickyXRInteractor.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            #region Configure Buttons and Styles

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

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            #endregion

            StickyEditorHelper.InTechPreview(true);

            #region General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showGeneralSettingsInEditorProp, generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGeneralSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
                EditorGUILayout.PropertyField(stickyControlModuleProp, stickyControlModuleContent);
                EditorGUILayout.PropertyField(beamStartWidthProp, beamStartWidthContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(interactorTypeProp, interactorTypeContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetInteractorType((StickyXRInteractor.InteractorType)interactorTypeProp.intValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lookModeProp, lookModeContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetLookMode((StickyXRInteractor.LookMode)lookModeProp.intValue);
                }

                EditorGUILayout.PropertyField(maxDistanceProp, maxDistanceContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(defaultBeamGradientProp, defaultBeamGradientContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetDefaultBeamGradient(StickyEditorHelper.GetGradient(defaultBeamGradientProp));
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(activeBeamGradientProp, activeBeamGradientContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetActiveBeamGradient(StickyEditorHelper.GetGradient(activeBeamGradientProp));
                }
            }

            EditorGUILayout.EndVertical();

            //StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            #endregion

            #region Interactive Settings

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showInteractiveSettingsInEditorProp, interactiveSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showInteractiveSettingsInEditorProp.boolValue)
            {
                StickyControlModule stickyControlModule = (StickyControlModule)stickyControlModuleProp.objectReferenceValue;

                if (stickyControlModule != null && !stickyControlModule.IsLookInteractiveEnabled &&
                    (StickyXRInteractor.LookMode)lookModeProp.intValue == StickyXRInteractor.LookMode.Interactive)
                {
                    EditorGUILayout.HelpBox("Interactive is not enabled on the Sticky Control Module Look tab", MessageType.Warning);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(palmTransformProp, palmTransformContent);
                if (EditorGUI.EndChangeCheck() && palmTransformProp.objectReferenceValue != null)
                {
                    if (!stickyXRInteractor.SetHandPalmTransform((Transform)palmTransformProp.objectReferenceValue))
                    {
                        palmTransformProp.objectReferenceValue = null;
                    }
                }

                EditorGUILayout.PropertyField(isPointToGrabProp, isPointToGrabContent);
                EditorGUILayout.PropertyField(isPointToTouchProp, isPointToTouchContent);
                EditorGUILayout.PropertyField(throwStrengthProp, throwStrengthContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(interactiveTargetSpriteProp, interactiveTargetSpriteContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetTargetSprite((Sprite)interactiveTargetSpriteProp.objectReferenceValue);
                }

                EditorGUILayout.PropertyField(defaultTargetColourProp, defaultTargetColourContent);
                EditorGUILayout.PropertyField(activeTargetColourProp, activeTargetColourContent);
                EditorGUILayout.PropertyField(targetOffsetDistanceNProp, targetOffsetDistanceNContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(interactivePointModeProp, interactivePointModeContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyXRInteractor.SetInteractivePointMode((StickyXRInteractor.InteractivePointMode)interactivePointModeProp.intValue);
                }

                EditorGUILayout.PropertyField(isPointWeaponHeldProp, isPointWeaponHeldContent);
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Teleport Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showTeleportSettingsInEditorProp, teleportSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showTeleportSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(beamStartCurveProp, beamStartCurveContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(teleportReticleProp, teleportReticleContent);
                if (EditorGUI.EndChangeCheck() && teleportReticleProp.objectReferenceValue != null)
                {
                    if (!StickyEditorHelper.IsPrefabAsset((GameObject)teleportReticleProp.objectReferenceValue))
                    {
                        teleportReticleProp.objectReferenceValue = null;
                        Debug.LogWarning("WARNING: Sticky XR Interactor - Teleport Reticle prefabs must be prefabs from the Project folder.");
                    }
                }

                EditorGUILayout.PropertyField(isShowTeleportReticleNormalProp, isShowTeleportReticleNormalContent);
                EditorGUILayout.PropertyField(isAutoDisableTeleporterProp, isAutoDisableTeleporterContent);
                EditorGUILayout.PropertyField(isTeleportObstacleCheckProp, isTeleportObstacleCheckContent);
            }
            EditorGUILayout.EndVertical();
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyXRInteractor != null)
            {
                float rightLabelWidth = 175f;

                #region Debug - General
                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyXRInteractor.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

                #region Debug - Interactive

                #region Debug - Look Interactive
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsLookInteractiveEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyXRInteractor.IsInteractiveEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsLookingAtInteractiveContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(!stickyXRInteractor.IsInteractiveEnabled || stickyXRInteractor.GetLookingAtInteractiveId() == StickyInteractive.NoID ? "--" : stickyXRInteractor.GetLookingAtInteractive().name, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsLookingAtPointContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(StickyEditorHelper.GetVector3Text(stickyXRInteractor.GetLookingAtPoint, 1), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

                #endregion

                #region Debug - Teleporting
                bool isTeleporterEnabled = stickyXRInteractor.IsTeleportEnabled;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsTeleporterEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(isTeleporterEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                isDebuggingShowLocationVolume = EditorGUILayout.Toggle(debugShowLocationVolumeContent, isDebuggingShowLocationVolume);

                Vector3 teleportLocation = stickyXRInteractor.TeleportLocation;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTeleportLocationContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(teleportLocation == Vector3.zero ? "--" : StickyEditorHelper.GetVector3Text(teleportLocation, 1), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsTeleportLocationValidContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyXRInteractor.IsTeleportLocationValid ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                #endregion

            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyXRInteractor.allowRepaint = true;
        }

        #endregion
    }
}
