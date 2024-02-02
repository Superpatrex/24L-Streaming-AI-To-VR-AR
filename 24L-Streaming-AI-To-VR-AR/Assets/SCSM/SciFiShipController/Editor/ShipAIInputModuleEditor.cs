using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipAIInputModule))]
    [CanEditMultipleObjects]
    public class ShipAIInputModuleEditor : Editor
    {
        #region Custom Editor private variables
        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private bool isShowShipSpeedEnabled = false;
        private bool isShowShipInput = false;
        private ShipInput debugShipInput = null;
        private ShipAIInputModule shipAIInputModule = null;

        #endregion

        #region GUIContent - General
        private readonly static GUIContent headerContent = new GUIContent("<b>Ship AI Input Module</b>\n\nThis module sends input to the Ship Control Module attached to this gameobject.");
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent("Initialise On Awake", "If enabled, the Initialise() will be called as soon as Awake() runs. This should be disabled if you are instantiating the ShipAIInputModule through code.");
        private readonly static GUIContent isEnableAIOnInitialiseContent = new GUIContent("Enable AI On Initialise", "Will the main update loop perform calculations and send input to the ship as soon as it is initialised? [Default: ON]");
        private readonly static GUIContent movementAlgorithmContent = new GUIContent("Movement Algorithm", "The algorithm used for calculating AI movement.");
        private readonly static GUIContent obstacleAvoidanceQualityContent = new GUIContent("Obstacle Avoidance Quality", "The quality of obstacle avoidance for this AI ship. Lower quality settings will improve performance.");
        private readonly static GUIContent obstacleLayerMaskContent = new GUIContent("Obstacle Layer Mask", "Layermask determining which layers will be detected as obstacles when raycasted against. Exclude layers that you don't want the AI ship to try and avoid using obstacle avoidance.");
        private readonly static GUIContent raycastStartOffsetZContent = new GUIContent("Raycast Start Offset", "If the ship has colliders that do not overlap the centre of the ship, use this with Obstacle Avoidance. This moves the raycasts forward from the centre of the ship to prevent it seeing its own colliders. 0 = OFF");
        private readonly static GUIContent pathFollowingQualityContent = new GUIContent("Path Following Quality", "The quality of path following for this AI ship. Lower quality settings will improve performance.");
        private readonly static GUIContent maxSpeedContent = new GUIContent("Max Speed", "The max speed for the ship in metres per second.");
        private readonly static GUIContent shipRadiusContent = new GUIContent("Ship Radius", "The supposed radius of the ship (approximated as a sphere) used for obstacle avoidance.");
        private readonly static GUIContent calcShipRadiusBtnContent = new GUIContent("Est.", "Estimate the radius of the ship");
        private readonly static GUIContent targetingAccuracyContent = new GUIContent("Targeting Accuracy", "The accuracy of the ship at shooting at a target. A value of 1 is perfect accuracy, while a value of 0 is the lowest accuracy.");
        private readonly static GUIContent maxBankAngleContent = new GUIContent("Max Bank Angle", "The max angle (in degrees) the ship should bank at while turning.");
        private readonly static GUIContent maxBankTurnAngleContent = new GUIContent("Max Bank Turn Angle", "The turning angle (in degrees) to the target position at which the AI will bank at the maxBankAngle. Lower values will result in the AI banking at a steeper angle for lower turning angles.");
        private readonly static GUIContent maxPitchAngleContent = new GUIContent("Max Pitch Angle", "The maximum pitch angle (in degrees) that the AI is able to use to pitch towards the target position.");
        private readonly static GUIContent turnPitchThresholdContent = new GUIContent("Turn Pitch Threshold", "Only use pitch to steer when the ship is within the threshold (in degrees) of the correct yaw/roll angle.");
        private readonly static GUIContent rollBiasContent = new GUIContent("Roll Bias", "When turning, will the ship favour yaw (i.e. turning using yaw then pitching) or roll (i.e. turning using roll then pitching) to achieve the turn? Lower values will favour yaw while higher values will favour roll.");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display the data being set in the Ship AI Input Module at runtime in the editor.");
        private readonly static GUIContent debugCurrentStateContent = new GUIContent("Current State");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugIsAIEnabledContent = new GUIContent("Is AI Enabled?");
        private readonly static GUIContent debugDesiredLocalVelocityContent = new GUIContent("Desired Local Velocity");
        private readonly static GUIContent debugCurrentLocalVelocityContent = new GUIContent("Current Local Velocity");
        private readonly static GUIContent debugCurrentLocationContent = new GUIContent("Current Location");
        private readonly static GUIContent debugTargetShipContent = new GUIContent("Target Ship");
        private readonly static GUIContent debugTargetShipIdContent = new GUIContent("  Ship Id");
        private readonly static GUIContent debugTargetFactionIdContent = new GUIContent("  Faction Id");
        private readonly static GUIContent debugTargetSquadronIdContent = new GUIContent("  Squadron Id");
        private readonly static GUIContent debugTargetShipPositionContent = new GUIContent("  Position");
        private readonly static GUIContent debugTargetPathContent = new GUIContent("Target Path");
        private readonly static GUIContent debugTargetPathNameContent = new GUIContent("  Path Name");
        private readonly static GUIContent debugTargetPathIndexContent = new GUIContent("  Path Index");
        private readonly static GUIContent debugTargetPathLocationPositionContent = new GUIContent("  Location Position");
        private readonly static GUIContent debugTargetLocationContent = new GUIContent("Target Location");
        private readonly static GUIContent debugTargetLocationNameContent = new GUIContent("  Location Name");
        private readonly static GUIContent debugTargetLocationPositionContent = new GUIContent("  Location Position");
        private readonly static GUIContent debugTargetPositionContent = new GUIContent("Target Position");
        private readonly static GUIContent debugNotSetContent = new GUIContent("-", "not set");
        private readonly static GUIContent debugIsShipSpeedShownContent = new GUIContent("Ship Speed", "");
        private readonly static GUIContent debugShipSpeedContent = new GUIContent("Ship Speed km/h", "");
        private readonly static GUIContent debugShowShipInputContent = new GUIContent("Show Ship Input", "This is the current input sent from the AI module to the ship");
        private readonly static GUIContent debugShowShipInputHorizontalContent = new GUIContent("  Horizontal");
        private readonly static GUIContent debugShowShipInputVerticalContent = new GUIContent("  Vertical");
        private readonly static GUIContent debugShowShipInputLongitudinalContent = new GUIContent("  Longitudinal");
        private readonly static GUIContent debugShowShipInputPitchContent = new GUIContent("  Pitch");
        private readonly static GUIContent debugShowShipInputYawContent = new GUIContent("  Yaw");
        private readonly static GUIContent debugShowShipInputRollContent = new GUIContent("  Roll");
        private readonly static GUIContent debugShowShipInputPrimaryFireContent = new GUIContent("  Primary Fire");
        private readonly static GUIContent debugShowShipInputSecondaryFireContent = new GUIContent("  Secondary Fire");
        private readonly static GUIContent debugShowShipInputDockingContent = new GUIContent("  Docking");
        #endregion GUIContent - Debug

        #region Serialized Properties

        private SerializedProperty shipAIInputModuleProp;
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty isEnableAIOnInitialiseProp;
        private SerializedProperty movementAlgorithmProp;
        private SerializedProperty obstacleAvoidanceQualityProp;
        private SerializedProperty obstacleLayerMaskProp;
        private SerializedProperty raycastStartOffsetZProp;
        private SerializedProperty pathFollowingQualityProp;
        private SerializedProperty maxSpeedProp;
        private SerializedProperty shipRadiusProp;

        #endregion

        #region Event Methods

        private void OnEnable()
        {
            shipAIInputModule = (ShipAIInputModule)target;

            //#if UNITY_2019_1_OR_NEWER
            //SceneView.duringSceneGui -= SceneGUI;
            //SceneView.duringSceneGui += SceneGUI;
            //#else
            //SceneView.onSceneGUIDelegate -= SceneGUI;
            //SceneView.onSceneGUIDelegate += SceneGUI;
            //#endif

            //Tools.hidden = true;

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }
            if (string.IsNullOrEmpty(txtColourName)) { }

            // Reset guistyles to avoid issues - forces reinitialisation of button styles etc
            helpBoxRichText = null;
            labelFieldRichText = null;
            buttonCompact = null;
            foldoutStyleNoLabel = null;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;

            defaultEditorLabelWidth = 185f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region FindProperties

            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            isEnableAIOnInitialiseProp = serializedObject.FindProperty("isEnableAIOnInitialise");
            movementAlgorithmProp = serializedObject.FindProperty("movementAlgorithm");
            obstacleAvoidanceQualityProp = serializedObject.FindProperty("obstacleAvoidanceQuality");
            obstacleLayerMaskProp = serializedObject.FindProperty("obstacleLayerMask");
            raycastStartOffsetZProp = serializedObject.FindProperty("raycastStartOffsetZ");
            pathFollowingQualityProp = serializedObject.FindProperty("pathFollowingQuality");
            maxSpeedProp = serializedObject.FindProperty("maxSpeed");
            shipRadiusProp = serializedObject.FindProperty("shipRadius");

            #endregion
        }

        private void OnDisable()
        {
            //Tools.hidden = false;
            //Tools.current = Tool.Move;

            //#if UNITY_2019_1_OR_NEWER
            //SceneView.duringSceneGui -= SceneGUI;
            //#else
            //SceneView.onSceneGUIDelegate -= SceneGUI;
            //#endif
        }

        private void OnDestroy()
        {
            //Tools.hidden = false;
            //Tools.current = Tool.Move;
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows items like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise

            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            #endregion

            #region Configure Buttons and Styles
            // Set up rich text GUIStyles
            if (helpBoxRichText == null)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;
            }

            if (labelFieldRichText == null)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;
            }

            if (buttonCompact == null)
            {
                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;
            }

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }

            // Set up the toggle buttons styles
            if (toggleCompactButtonStyleNormal == null)
            {
                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            #endregion

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            #region Properties
            EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
            EditorGUILayout.PropertyField(isEnableAIOnInitialiseProp, isEnableAIOnInitialiseContent);
            EditorGUILayout.PropertyField(movementAlgorithmProp, movementAlgorithmContent);
            EditorGUILayout.PropertyField(obstacleAvoidanceQualityProp, obstacleAvoidanceQualityContent);
            EditorGUILayout.PropertyField(obstacleLayerMaskProp, obstacleLayerMaskContent);
            EditorGUILayout.PropertyField(raycastStartOffsetZProp, raycastStartOffsetZContent);
            EditorGUILayout.PropertyField(pathFollowingQualityProp, pathFollowingQualityContent);
            EditorGUILayout.PropertyField(maxSpeedProp, maxSpeedContent);
            if (maxSpeedProp.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("The ship will not be able to move forward unless max speed > 0", MessageType.Warning);
            }
            EditorGUILayout.BeginHorizontal();
            #if UNITY_2019_3_OR_NEWER
            EditorGUILayout.LabelField(shipRadiusContent, GUILayout.Width(EditorGUIUtility.labelWidth - 44f));
            #else
            EditorGUILayout.LabelField(shipRadiusContent, GUILayout.Width(EditorGUIUtility.labelWidth - 48f));
            #endif
            if (GUILayout.Button(calcShipRadiusBtnContent, buttonCompact, GUILayout.Width(40f)))
            {
                Transform _shipTransform = shipAIInputModule.transform;
                Vector3 _originalPos = _shipTransform.position;
                Quaternion _originalRot = _shipTransform.rotation;

                _shipTransform.position = Vector3.zero;
                _shipTransform.rotation = Quaternion.identity;

                Bounds shipBounds = SSCUtils.GetBounds(_shipTransform, false, true);

                // Restore original settings
                _shipTransform.position = _originalPos;
                _shipTransform.rotation = _originalRot;

                float maxDimension = Mathf.Max(new float[] { shipBounds.extents.x, shipBounds.extents.y, shipBounds.extents.z });

                shipRadiusProp.floatValue = maxDimension > 1f ? maxDimension : 1f;
            }
            EditorGUILayout.PropertyField(shipRadiusProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetingAccuracy"), targetingAccuracyContent);

            if (movementAlgorithmProp.intValue == (int)ShipAIInputModule.AIMovementAlgorithm.PlanarFlight ||
                movementAlgorithmProp.intValue == (int)ShipAIInputModule.AIMovementAlgorithm.PlanarFlightBanking)
            {
                if (movementAlgorithmProp.intValue == (int)ShipAIInputModule.AIMovementAlgorithm.PlanarFlightBanking)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxBankAngle"), maxBankAngleContent);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxBankTurnAngle"), maxBankTurnAngleContent);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPitchAngle"), maxPitchAngleContent);
            }
            else if (movementAlgorithmProp.intValue == (int)ShipAIInputModule.AIMovementAlgorithm.Full3DFlight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("turnPitchThreshold"), turnPitchThresholdContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rollBias"), rollBiasContent);
            }
            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && shipAIInputModule != null)
            {
                AIState currentState = AIState.GetState(shipAIInputModule.GetState());

                // Increase from 150 to 160 in v1.4.0 to cater for target position with 2 dec places
                float rightLabelWidth = 160f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipAIInputModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsAIEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipAIInputModule.IsAIEnabled ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCurrentStateContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth-3f));
                if (currentState != null) { EditorGUILayout.LabelField(currentState.name, GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugDesiredLocalVelocityContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth-3f));
                EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(shipAIInputModule.DesiredLocalVelocity,3), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCurrentLocalVelocityContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(shipAIInputModule.CurrentLocalVelocity,3), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                if (shipAIInputModule.IsInitialised)
                {
                    // This could be a little slow each frame
                    Ship ship = shipAIInputModule.GetShip;
                    if (ship != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(debugCurrentLocationContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(ship.TransformPosition,2), GUILayout.MaxWidth(rightLabelWidth));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                isShowShipSpeedEnabled = EditorGUILayout.Toggle(debugIsShipSpeedShownContent, isShowShipSpeedEnabled);
                if (isShowShipSpeedEnabled)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShipSpeedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    if (shipAIInputModule.IsInitialised)
                    {
                        ShipControlModule localShipControlModule = shipAIInputModule.GetShipControlModule;
                        if (localShipControlModule != null && localShipControlModule.IsInitialised)
                        {
                            // z-axis of Local space velocity. Convert to km/h
                            float shipVelocity = localShipControlModule.shipInstance.LocalVelocity.z;

                            EditorGUILayout.LabelField((shipVelocity*3.6f).ToString("0.0") + " (m/s " + shipVelocity.ToString("0.0") + ")", GUILayout.MaxWidth(rightLabelWidth));
                        }
                        else
                        {
                            EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth));
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth));
                    }
                    EditorGUILayout.EndHorizontal();
                }

                Ship targetShip = shipAIInputModule.GetTargetShip();
                LocationData targetLocationData = shipAIInputModule.GetTargetLocation();
                PathData targetPathData = shipAIInputModule.GetTargetPath();

                // Target Ship
                bool isTargetShip = targetShip != null;

                EditorGUILayout.LabelField(debugTargetShipContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetShip) { EditorGUILayout.LabelField(targetShip.shipId.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetFactionIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetShip) { EditorGUILayout.LabelField(targetShip.factionId.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetSquadronIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetShip) { EditorGUILayout.LabelField(targetShip.squadronId.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetShip) { EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(targetShip.TransformPosition,1), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                // Target Path
                bool isTargetPath = targetPathData != null;
                EditorGUILayout.LabelField(debugTargetPathContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetPathNameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetPath) { EditorGUILayout.LabelField(targetPathData.name.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                int pathLocationIndex = isTargetPath ? shipAIInputModule.GetCurrentTargetPathLocationIndex() : -1;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetPathIndexContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetPath && pathLocationIndex >= 0) { EditorGUILayout.LabelField(pathLocationIndex.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetPathLocationPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetPath && pathLocationIndex >= 0) { EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(targetPathData.pathLocationDataList[pathLocationIndex].locationData.position, 2), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                // Target Location
                bool isTargetLocation = targetLocationData != null;

                EditorGUILayout.LabelField(debugTargetLocationContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetLocationNameContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetLocation) { EditorGUILayout.LabelField(targetLocationData.name.ToString(), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetLocationPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                if (isTargetLocation) { EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(targetLocationData.position,2), GUILayout.MaxWidth(rightLabelWidth)); }
                else { EditorGUILayout.LabelField(debugNotSetContent, GUILayout.MaxWidth(rightLabelWidth)); }
                EditorGUILayout.EndHorizontal();

                // Target Position
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetPositionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(shipAIInputModule.GetTargetPosition(), 2), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                // Ship Input (OFF BY DEFAULT)
                isShowShipInput = EditorGUILayout.Toggle(debugShowShipInputContent, isShowShipInput);

                if (isShowShipInput) { debugShipInput = shipAIInputModule.GetShipInput; }

                if (isShowShipInput && debugShipInput != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputHorizontalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.horizontal.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputVerticalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.vertical.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputLongitudinalContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.longitudinal.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputPitchContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.pitch.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputYawContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.yaw.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputRollContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.roll.ToString("0.000"), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputPrimaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.primaryFire.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputSecondaryFireContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.secondaryFire.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(debugShowShipInputDockingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                    EditorGUILayout.LabelField(debugShipInput.dock.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            #endregion

            //DrawDefaultInspector();
        }

        #endregion
    }
}