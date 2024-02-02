using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipCameraModule))]
    public class ShipCameraModuleEditor : Editor
    {
        #region Custom Editor private variables

        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private ShipCameraModule shipCameraModule = null;
        private bool isDebuggingEnabled = false;

        #endregion

        #region GUIContent - General

        private readonly static GUIContent headerContent = new GUIContent("<b>Ship Camera Module</b>\n\nThis module enables you to " +
            "implement camera behaviour on the object it is attached to.");

        internal readonly static GUIContent resetBtnContentSCM = new GUIContent("Reset", "Reset to default values");
        private readonly static GUIContent startOnInitialiseContent = new GUIContent("Start on Initialise", "Start the camera rendering when it is initialised");
        private readonly static GUIContent enableOnInitialiseContent = new GUIContent("Enable on Initialise", "Enable camera movement when it is initialised and Start on Initialise is on");
        private readonly static GUIContent targetShipContent = new GUIContent("Target Ship", "The target ship for this camera to " +
            "follow from the scene.");
        internal readonly static GUIContent targetOffsetCoordsContentSCM = new GUIContent("Target Offset Coords", "The coordinate " +
            "system used to interpret the target offset. " +
            "\nCameraRotation: The target offset is relative to the rotation of the camera." +
            "\nTargetRotation: The target offset is relative to the rotation of the target." +
            "\nTargetRotationFlat: The target offset is relative to the flat rotation of the target." +
            "\nWorld: The target offset is relative to the world coordinate system.");
        internal readonly static GUIContent targetOffsetContentSCM = new GUIContent("Target Offset", "The offset from the target (in " +
            "local space) for the camera to aim for.");
        private readonly static GUIContent targetOffsetBtnContent = new GUIContent("Align", "Set the offset to the current camera offset from the ship in the scene");
        internal readonly static GUIContent lockToTargetPosContentSCM = new GUIContent("Lock to Target Pos", "If enabled, the camera " +
            "will stay locked to the optimal camera position.");
        internal readonly static GUIContent moveSpeedContentSCM = new GUIContent("Move Speed", "How quickly the camera moves towards " +
            "the optimal camera position.");
        internal readonly static GUIContent lockToTargetRotContentSCM = new GUIContent("Lock to Target Rot", "If enabled, the camera " +
            "will stay locked to the optimal camera rotation.");
        internal readonly static GUIContent turnSpeedContentSCM = new GUIContent("Turn Speed", "How quickly the camera turns towards " +
            "the optimal camera rotation.");
        internal readonly static GUIContent lockCameraPosContentSCM = new GUIContent("Lock Camera Pos", "If enabled, the camera " +
            "will stay at the same position and rotate towards the target. Only available when Camera Rotation Mode is set to Aim To Target.");
        internal readonly static GUIContent targetOffsetDampingContentSCM = new GUIContent("Offset Damping", "Damp or modify the target position offset based upon the ship pitch and yaw inputs");
        internal readonly static GUIContent dampingMaxPitchOffsetUpContentSCM = new GUIContent(" Max Pitch Offset Up", "The damping maximum pitch Target Offset Up (y-axis)");
        internal readonly static GUIContent dampingMaxPitchOffsetDownContentSCM = new GUIContent(" Max Pitch Offset Down", "The damping maximum pitch Target Offset Down (y-axis). What is the lowest Target Offset Y value can be?");
        internal readonly static GUIContent dampingPitchRateContentSCM = new GUIContent(" Damping Pitch Rate", "The rate at which Target Offset Y is modified by ship pitch input. Higher values are more responsive.");
        internal readonly static GUIContent dampingPitchGravityContentSCM = new GUIContent(" Damping Pitch Gravity", "The rate at which the Target Offset Y returns to normal when there is no ship pitch input. Higher values are more responsive.");
        internal readonly static GUIContent dampingMaxYawOffsetLeftContentSCM = new GUIContent(" Max Yaw Offset Left", "The damping maximum yaw Target Offset left (x-axis)");
        internal readonly static GUIContent dampingMaxYawOffsetRightContentSCM = new GUIContent(" Max Yaw Offset Right", "The damping maximum yaw Target Offset right (x-axis). What is the lowest Target Offset X value can be?");
        internal readonly static GUIContent dampingYawRateContentSCM = new GUIContent(" Damping Yaw Rate", "The rate at which Target Offset X is modified by ship yaw input. Higher values are more responsive.");
        internal readonly static GUIContent dampingYawGravityContentSCM = new GUIContent(" Damping Yaw Gravity", "The rate at which the Target Offset X returns to normal when there is no ship yaw input. Higher values are more responsive.");
        internal readonly static GUIContent cameraRotationModeContentSCM = new GUIContent("Camera Rotation Mode", "How the camera " +
            "rotation is determined." +
            "\nFollowVelocity: The camera rotates to face in the direction the ship is moving in." +
            "\nFollowTargetRotation: The camera rotates to face the direction the ship is facing in." +
            "\nAimAtTarget: The camera rotates to face towards the ship itself." +
            "\nTopDownFollowVelocity: The camera faces downwards and rotates so that the top of the " +
            "screen is in the direction the ship is moving in." +
            "\nTopDownFollowTargetRotation: The camera faces downwards and rotates so that the top of the screen " +
            "is in the direction the ship is facing in." +
            "\nFixed: The camera rotation is fixed.");
        internal readonly static GUIContent followVelocityThresholdContentSCM = new GUIContent("Follow Velocity Threshold", "Below " +
            "this velocity (in metres per second) the forwards direction of the target will be followed instead of the velocity.");
        internal readonly static GUIContent orientUpwardsContentSCM = new GUIContent("Orient Upwards", "If enabled, the camera " +
            "will orient with respect to the world up direction rather than the target's up direction.");
        internal readonly static GUIContent cameraFixedRotationContentSCM = new GUIContent("Rotation", "The target rotation of the " +
            "camera in world space.");
        internal readonly static GUIContent updateTypeContentSCM = new GUIContent("Update Type", "When the camera " +
            "position/rotation is updated." +
            "\nFixedUpdate: The update occurs during FixedUpdate. Recommended for rigidbodies with Interpolation set to None." +
            "\nLateUpdate: The update occurs during LateUpdate. Recommended for rigidbodies with Interpolation set to Interpolate." +
            "\nAutomatic: When the update occurs is automatically determined.");

        internal readonly static GUIContent maxShakeStrengthContentSCM = new GUIContent("Max Shake Strength", "The maximum strength of the camera shake. Smaller numbers are typically better.");
        internal readonly static GUIContent maxShakeDurationContentSCM = new GUIContent("Max Shake Duration", "The maximum duration (in seconds) the camera will shake per incident.");

        #endregion

        #region GUIContent - Clip Objects
        internal readonly static GUIContent clipObjectsContentSCM = new GUIContent("Clip Objects", "Adjust the camera position to attempt to avoid the camera flying through objects between the ship and the camera. This has performance overhead, so disable if not needed.");
        internal readonly static GUIContent minClipMoveSpeedContentSCM = new GUIContent("Minimum Move Speed", "The minimum speed the camera will move to avoid flying through objects between the ship and the camera. High values make clipping more effective. Lower values will make it smoother.");
        internal readonly static GUIContent clipMinDistanceContentSCM = new GUIContent("Minimum Distance", "The minimum distance the camera can be from the ship (target) position. Set to spheric radius of ship if it has colliders that do not overlap ship position.");
        internal readonly static GUIContent clipMinOffsetXContentSCM = new GUIContent("Minimum Offset X", "The minimum offset on the x-axis the camera can be from the Ship (target) when object clipping. This should be less than or equal to the Target Offset X value.");
        internal readonly static GUIContent clipMinOffsetYContentSCM = new GUIContent("Minimum Offset Y", "The minimum offset on the y-axis the camera can be from the Ship (target) when object clipping. This should be less than or equal to the Target Offset Y value.");
        internal readonly static GUIContent clipObjectMaskContentSCM = new GUIContent("Clip Object Layers", "Only attempt to clip objects in the following Unity Layers");
        internal readonly static GUIContent clipEstMinDistanceBtnContentSCM = new GUIContent("Est.", "Estimate the minimum object distance from the ship (target) based on the radius of the ship plus 10 percent");

        #endregion

        #region GUIContent - Zoom
        internal readonly static GUIContent isZoomEnabledContentSCM = new GUIContent("Zoom", "Enable the ability to zoom the camera in or out");
        internal readonly static GUIContent zoomDurationContentSCM = new GUIContent("Zoom Duration", "The time, in seconds, to zoom fully in or out");
        internal readonly static GUIContent unzoomDelayContentSCM = new GUIContent("Unzoom Delay", "The delay, in seconds, before zoom starts to return to the non-zoomed position");
        internal readonly static GUIContent unzoomedFoVContentSCM = new GUIContent("Unzoomed FoV", "The camera field-of-view when no zoom is applied [Default 60]");
        internal readonly static GUIContent zoomedInFoVContentSCM = new GUIContent("Zoomed In FoV", "The camera field-of-view when fully zoomed in [Default 10]");
        internal readonly static GUIContent zoomedOutFoVContentSCM = new GUIContent("Zoomed Out FoV", "The camera field-of-view when fully zoomed out [Default 90]");
        internal readonly static GUIContent zoomDampingContentSCM = new GUIContent("Zoom Damping", "The amount of damping applied when starting or stopping camera zoom");

        #endregion

        #region GUIContent - Debugging
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display information about Ship Camera Module at runtime in the editor.");
        private readonly static GUIContent debugUpdateModeContent = new GUIContent("Update Mode");
        private readonly static GUIContent debugStartedContent = new GUIContent("Is Started (Rendering)");
        private readonly static GUIContent debugEnabledContent = new GUIContent("Is Enabled (Movement)");
        private readonly static GUIContent debugZoomAmountContent = new GUIContent("Zoom Amount");

        #endregion

        #region Static Strings
        internal readonly static string targetPosMsgSCM = "Requires FixedUpdate or no Target Pos Lock with a low Move Speed";
        internal readonly static string targetRotMsgSCM = "Requires FixedUpdate or no Target Rot Lock with a low Turn Speed";
        internal readonly static string aimIncompatibleMsgSCM = "Camera Rotation is not compatible with Aim At Target. Use Target Rotation instead";
        internal readonly static string targetDampingPitchMsgSCM = "Max Pitch Down should be <= Target Offset Y. Max Pitch Up should be >= Target Offset Y";
        internal readonly static string targetDampingYawMsgSCM = "Max Yaw Left should be <= Target Offset X. Max Yaw Right should be >= Target Offset X";
        internal readonly static string lockPosRotFixedUpdateMsgSCM = "Lock to Pos or Rot should be using LateUpdate to avoid lag.";

        #endregion

        #region Serialized Properties - General

        private SerializedProperty startOnInitialiseProp;
        private SerializedProperty enableOnInitialiseProp;
        private SerializedProperty lockToTargetPosProp;
        private SerializedProperty lockToTargetRotProp;
        private SerializedProperty lockCameraPosProp;
        private SerializedProperty targetOffsetCoordinatesProp;
        private SerializedProperty cameraRotationModeProp;
        private SerializedProperty updateTypeProp;
        private SerializedProperty moveSpeedProp;
        private SerializedProperty turnSpeedProp;
        private SerializedProperty targetProp;
        private SerializedProperty targetOffsetProp;
        private SerializedProperty targetOffsetDampingProp;
        private SerializedProperty dampingMaxPitchOffsetUpProp;
        private SerializedProperty dampingMaxPitchOffsetDownProp;
        private SerializedProperty dampingPitchRateProp;
        private SerializedProperty dampingPitchGravityProp;
        private SerializedProperty dampingMaxYawOffsetLeftProp;
        private SerializedProperty dampingMaxYawOffsetRightProp;
        private SerializedProperty dampingYawRateProp;
        private SerializedProperty dampingYawGravityProp;
        private SerializedProperty maxShakeStrengthProp;
        private SerializedProperty maxShakeDurationProp;
        #endregion

        #region Serialized Properties - Object Clipping
        private SerializedProperty clipObjectsProp;
        private SerializedProperty minClipMoveSpeedProp;
        private SerializedProperty clipMinDistanceProp;
        private SerializedProperty clipMinOffsetXProp;
        private SerializedProperty clipMinOffsetYProp;
        private SerializedProperty clipObjectMaskProp;
        #endregion

        #region Serialized Properties - Zoom
        private SerializedProperty isZoomEnabledProp;
        private SerializedProperty zoomDurationProp;
        private SerializedProperty unzoomDelayProp;
        private SerializedProperty unzoomedFoVProp;
        private SerializedProperty zoomedInFoVProp;
        private SerializedProperty zoomedOutFovProp;
        private SerializedProperty zoomDampingProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            shipCameraModule = (ShipCameraModule)target;
            defaultEditorLabelWidth = 175f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            startOnInitialiseProp = serializedObject.FindProperty("startOnInitialise");
            enableOnInitialiseProp = serializedObject.FindProperty("enableOnInitialise");
            cameraRotationModeProp = serializedObject.FindProperty("cameraRotationMode");
            targetOffsetCoordinatesProp = serializedObject.FindProperty("targetOffsetCoordinates");
            lockToTargetPosProp = serializedObject.FindProperty("lockToTargetPosition");
            lockToTargetRotProp = serializedObject.FindProperty("lockToTargetRotation");
            lockCameraPosProp = serializedObject.FindProperty("lockCameraPosition");
            moveSpeedProp = serializedObject.FindProperty("moveSpeed");
            turnSpeedProp = serializedObject.FindProperty("turnSpeed");
            updateTypeProp = serializedObject.FindProperty("updateType");
            targetProp = serializedObject.FindProperty("target");
            targetOffsetProp = serializedObject.FindProperty("targetOffset");
            targetOffsetDampingProp = serializedObject.FindProperty("targetOffsetDamping");
            dampingMaxPitchOffsetUpProp = serializedObject.FindProperty("dampingMaxPitchOffsetUp");
            dampingMaxPitchOffsetDownProp = serializedObject.FindProperty("dampingMaxPitchOffsetDown");
            dampingPitchRateProp = serializedObject.FindProperty("dampingPitchRate");
            dampingPitchGravityProp = serializedObject.FindProperty("dampingPitchGravity");
            dampingMaxYawOffsetLeftProp = serializedObject.FindProperty("dampingMaxYawOffsetLeft");
            dampingMaxYawOffsetRightProp = serializedObject.FindProperty("dampingMaxYawOffsetRight");
            dampingYawRateProp = serializedObject.FindProperty("dampingYawRate");
            dampingYawGravityProp = serializedObject.FindProperty("dampingYawGravity");
            maxShakeStrengthProp = serializedObject.FindProperty("maxShakeStrength");
            maxShakeDurationProp = serializedObject.FindProperty("maxShakeDuration");
            #endregion

            #region Find Properties - Clip Objects
            clipObjectsProp = serializedObject.FindProperty("clipObjects");
            clipMinDistanceProp = serializedObject.FindProperty("clipMinDistance");
            clipMinOffsetXProp = serializedObject.FindProperty("clipMinOffsetX");
            clipMinOffsetYProp = serializedObject.FindProperty("clipMinOffsetY");
            minClipMoveSpeedProp = serializedObject.FindProperty("minClipMoveSpeed");
            clipObjectMaskProp = serializedObject.FindProperty("clipObjectMask");
            #endregion

            #region Find Properties - Zoom
            isZoomEnabledProp = serializedObject.FindProperty("isZoomEnabled");
            zoomDurationProp = serializedObject.FindProperty("zoomDuration");
            unzoomDelayProp = serializedObject.FindProperty("unzoomDelay");
            unzoomedFoVProp = serializedObject.FindProperty("unzoomedFoV");
            zoomedInFoVProp = serializedObject.FindProperty("zoomedInFoV");
            zoomedOutFovProp = serializedObject.FindProperty("zoomedOutFoV");
            zoomDampingProp = serializedObject.FindProperty("zoomDamping");
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
            if (shipCameraModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draw the Zoom Settings in the inspector
        /// </summary>
        private void DrawZoomSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isZoomEnabledProp, isZoomEnabledContentSCM);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                if (isZoomEnabledProp.boolValue) { shipCameraModule.EnableZoom(); }
                else { shipCameraModule.DisableZoom(); }
            }
            
            if (isZoomEnabledProp.boolValue)
            {
                EditorGUILayout.PropertyField(zoomDurationProp, zoomDurationContentSCM);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(unzoomDelayProp, unzoomDelayContentSCM);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    shipCameraModule.SetUnzoomDelay(unzoomDelayProp.floatValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(zoomedInFoVProp, zoomedInFoVContentSCM);
                if (EditorGUI.EndChangeCheck())
                {
                    if (zoomedInFoVProp.floatValue > unzoomedFoVProp.floatValue)
                    {
                        zoomedInFoVProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                }
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(unzoomedFoVProp, unzoomedFoVContentSCM);
                if (EditorGUI.EndChangeCheck())
                {
                    if (zoomedInFoVProp.floatValue > unzoomedFoVProp.floatValue)
                    {
                        zoomedInFoVProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                    if (zoomedOutFovProp.floatValue < unzoomedFoVProp.floatValue)
                    {
                        zoomedOutFovProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(zoomedOutFovProp, zoomedOutFoVContentSCM);
                if (EditorGUI.EndChangeCheck())
                {
                    if (zoomedOutFovProp.floatValue < unzoomedFoVProp.floatValue)
                    {
                        zoomedOutFovProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                }

                EditorGUILayout.PropertyField(zoomDampingProp, zoomDampingContentSCM);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            shipCameraModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            float rightLabelWidth = 150f;

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

            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            // **********************************************************************************
            // ********** IF UPDATING OR CHANGING - ALSO UPDATE ShipCameraSettingsEditor ********
            // **********************************************************************************

            #region General Properties and Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool isAimAtTarget = cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.AimAtTarget;
            bool isLateUpdate = updateTypeProp.intValue == (int)ShipCameraModule.CameraUpdateType.LateUpdate;
            bool isFixedUpdate = updateTypeProp.intValue == (int)ShipCameraModule.CameraUpdateType.FixedUpdate;

            // Display warning if there is a potential for jittery movement (i.e. camera out of sync with player)
            if (isAimAtTarget)// || 
                //cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.FollowVelocity)
            {
                bool isTargetPosLockorFastMoveSpeed = lockToTargetPosProp.boolValue || moveSpeedProp.floatValue > 10f;
                bool isTargetRotLockorFastTurnSpeed = lockToTargetRotProp.boolValue || turnSpeedProp.floatValue > 10f;               

                if (!isLateUpdate && updateTypeProp.intValue == (int)ShipCameraModule.CameraUpdateType.Automatic)
                {
                    // Don't want to check for rigidbody every frame when playing in the editor
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        ShipControlModule targetSCM = targetProp.objectReferenceValue as ShipControlModule;

                        if (targetSCM != null)
                        {
                            Rigidbody rigidbody = targetSCM.GetComponent<Rigidbody>();
                            if (rigidbody != null) { isLateUpdate = rigidbody.interpolation != RigidbodyInterpolation.None; }
                        }
                    }
                }

                if (isLateUpdate)
                {
                    // Don't post Position warning if Lock Camera Position is in use
                    if (isTargetPosLockorFastMoveSpeed && !(isAimAtTarget && lockCameraPosProp.boolValue))
                    {
                        EditorGUILayout.HelpBox(targetPosMsgSCM, MessageType.Warning);
                    }
                    if (isTargetRotLockorFastTurnSpeed)
                    {
                        EditorGUILayout.HelpBox(targetRotMsgSCM, MessageType.Warning);
                    }
                }
            }

            // Suggest using Target Rotation if user has Aim To Target selected.
            if (isAimAtTarget && targetOffsetCoordinatesProp.intValue == (int)ShipCameraModule.TargetOffsetCoordinates.CameraRotation)
            {
                EditorGUILayout.HelpBox(aimIncompatibleMsgSCM, MessageType.Warning);
            }

            EditorGUILayout.PropertyField(startOnInitialiseProp, startOnInitialiseContent);
            EditorGUILayout.PropertyField(enableOnInitialiseProp, enableOnInitialiseContent);

            EditorGUILayout.PropertyField(targetProp, targetShipContent);
            EditorGUILayout.PropertyField(targetOffsetCoordinatesProp, targetOffsetCoordsContentSCM);

            if (isAimAtTarget)
            {
                EditorGUILayout.PropertyField(lockCameraPosProp, lockCameraPosContentSCM);
            }

            // Warning if FixedUpdate and lock to target position or rotation
            if (!isAimAtTarget && (lockToTargetPosProp.boolValue || lockToTargetRotProp.boolValue))
            {
                if (isFixedUpdate || shipCameraModule.IsCameraInFixedUpdate)
                {
                    EditorGUILayout.HelpBox(lockPosRotFixedUpdateMsgSCM, MessageType.Warning);
                }
            }

            // Lock to Target Position and offset are not required if the camera position is locked
            if (!(isAimAtTarget && lockCameraPosProp.boolValue))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(targetOffsetContentSCM, GUILayout.Width(defaultEditorLabelWidth - 60f));
                if (GUILayout.Button(targetOffsetBtnContent, buttonCompact, GUILayout.MaxWidth(50f)))
                {
                    if (targetProp.objectReferenceValue == null)
                    {
                        EditorUtility.DisplayDialog("Align Camera Target Offset", "Currently there is no Target Ship to align with", "Got it!");
                    }
                    else
                    {
                        Transform _targetTrfm = ((ShipControlModule)targetProp.objectReferenceValue).transform;

                        targetOffsetProp.vector3Value =  Quaternion.Inverse(_targetTrfm.rotation) * (shipCameraModule.transform.position - _targetTrfm.position);
                    }
                }
                EditorGUILayout.PropertyField(targetOffsetProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(lockToTargetPosProp, lockToTargetPosContentSCM);
                if (!lockToTargetPosProp.boolValue)
                {
                    EditorGUILayout.PropertyField(moveSpeedProp, moveSpeedContentSCM);

                    #region Offset Damping
                    EditorGUILayout.PropertyField(targetOffsetDampingProp, targetOffsetDampingContentSCM);
                    if (targetOffsetDampingProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dampingMaxPitchOffsetUpProp, dampingMaxPitchOffsetUpContentSCM);
                        EditorGUILayout.PropertyField(dampingMaxPitchOffsetDownProp, dampingMaxPitchOffsetDownContentSCM);
                        EditorGUILayout.PropertyField(dampingPitchRateProp, dampingPitchRateContentSCM);
                        EditorGUILayout.PropertyField(dampingPitchGravityProp, dampingPitchGravityContentSCM);

                        EditorGUILayout.PropertyField(dampingMaxYawOffsetLeftProp, dampingMaxYawOffsetLeftContentSCM);
                        EditorGUILayout.PropertyField(dampingMaxYawOffsetRightProp, dampingMaxYawOffsetRightContentSCM);
                        EditorGUILayout.PropertyField(dampingYawRateProp, dampingYawRateContentSCM);
                        EditorGUILayout.PropertyField(dampingYawGravityProp, dampingYawGravityContentSCM);

                        if (dampingMaxPitchOffsetUpProp.floatValue < targetOffsetProp.vector3Value.y || dampingMaxPitchOffsetDownProp.floatValue > targetOffsetProp.vector3Value.y)
                        {
                            EditorGUILayout.HelpBox(targetDampingPitchMsgSCM, MessageType.Warning);
                        }
                        if (dampingMaxYawOffsetRightProp.floatValue < targetOffsetProp.vector3Value.x || dampingMaxYawOffsetLeftProp.floatValue > targetOffsetProp.vector3Value.x)
                        {
                            EditorGUILayout.HelpBox(targetDampingYawMsgSCM, MessageType.Warning);
                        }
                    }
                    #endregion
                }
            }
          
            EditorGUILayout.PropertyField(lockToTargetRotProp, lockToTargetRotContentSCM);
            if (!lockToTargetRotProp.boolValue)
            {
                EditorGUILayout.PropertyField(turnSpeedProp, turnSpeedContentSCM);
            }

            EditorGUILayout.PropertyField(cameraRotationModeProp, cameraRotationModeContentSCM);

            if (cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.FollowVelocity ||
                cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.TopDownFollowVelocity)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("followVelocityThreshold"), followVelocityThresholdContentSCM);
            }

            if (cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.TopDownFollowTargetRotation &&
                cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.TopDownFollowVelocity &&
                cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.Fixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("orientUpwards"), orientUpwardsContentSCM);
            }

            if (cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.Fixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraFixedRotation"), cameraFixedRotationContentSCM);
            }
                
            EditorGUILayout.PropertyField(updateTypeProp, updateTypeContentSCM);

            EditorGUILayout.PropertyField(maxShakeStrengthProp, maxShakeStrengthContentSCM);

            if (maxShakeStrengthProp.floatValue > 0f)
            {
                EditorGUILayout.PropertyField(maxShakeDurationProp, maxShakeDurationContentSCM);
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Clip Objects
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(clipObjectsProp, clipObjectsContentSCM);
            if (clipObjectsProp.boolValue)
            {
                SSCEditorHelper.InTechPreview(false);

                EditorGUILayout.PropertyField(minClipMoveSpeedProp, minClipMoveSpeedContentSCM);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(clipMinDistanceContentSCM, GUILayout.Width(EditorGUIUtility.labelWidth - 58f));
                if (GUILayout.Button(clipEstMinDistanceBtnContentSCM, buttonCompact, GUILayout.Width(50f)))
                {
                    GUI.FocusControl(null);

                    if (shipCameraModule.target != null)
                    {
                        Transform _shipTransform = shipCameraModule.target.transform;
                        Vector3 _originalPos = _shipTransform.position;
                        Quaternion _originalRot = _shipTransform.rotation;

                        _shipTransform.position = Vector3.zero;
                        _shipTransform.rotation = Quaternion.identity;

                        Bounds shipBounds = SSCUtils.GetBounds(_shipTransform, false, true);

                        // Restore original settings
                        _shipTransform.position = _originalPos;
                        _shipTransform.rotation = _originalRot;

                        float maxDimension = Mathf.Max(new float[] { shipBounds.extents.x, shipBounds.extents.y, shipBounds.extents.z });

                        maxDimension = maxDimension > 1f ? maxDimension * 1.1f : 1f;

                        // Round to 2 decimal places
                        maxDimension = Mathf.RoundToInt(maxDimension * 100f) / 100f;

                        clipMinDistanceProp.floatValue = maxDimension;
                    }
                    else { clipMinDistanceProp.floatValue = 0f; }
                }
                EditorGUILayout.PropertyField(clipMinDistanceProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(clipMinOffsetXProp, clipMinOffsetXContentSCM);
                EditorGUILayout.PropertyField(clipMinOffsetYProp, clipMinOffsetYContentSCM);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(clipObjectMaskContentSCM, GUILayout.Width(defaultEditorLabelWidth - 58f));
                if (GUILayout.Button(resetBtnContentSCM, buttonCompact, GUILayout.MaxWidth(50f)))
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipCameraModule, "Reset Clip Mask");
                    shipCameraModule.ResetClipObjectSettings();
                    GUIUtility.ExitGUI();
                    return;
                }
                EditorGUILayout.PropertyField(clipObjectMaskProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                // When first added or if user attempts to set to Nothing, reset to defaults.
                if (clipObjectMaskProp.intValue == 0)
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipCameraModule, "Reset Clip Mask");
                    shipCameraModule.ResetClipObjectSettings();
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            EditorGUILayout.EndVertical();
            #endregion

            DrawZoomSettings();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);

            if (isDebuggingEnabled && shipCameraModule != null)
            {               
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugUpdateModeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(shipCameraModule.IsCameraInFixedUpdate ? "Fixed Update" : "Late Update", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugStartedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(shipCameraModule.IsCameraStarted ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugEnabledContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(shipCameraModule.IsCameraEnabled ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugZoomAmountContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(shipCameraModule.GetZoomAmount().ToString("0.0"), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            #endregion

            shipCameraModule.allowRepaint = true;
        }

        #endregion
    }
}
