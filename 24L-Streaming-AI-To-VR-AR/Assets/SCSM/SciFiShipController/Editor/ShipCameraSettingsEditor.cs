using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// The inspector editor for scriptable object ShipCameraModule settings.
    /// </summary>
    [CustomEditor(typeof(ShipCameraSettings))]
    public class ShipCameraSettingsEditor : Editor
    {
        #region Custom Editor private variables
        private ShipCameraSettings shipCameraSettings = null;
        private bool isStylesInitialised = false;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Ship Camera Settings");
        private readonly static GUIContent headerContent = new GUIContent("Contains settings for a ShipCameraModule.");
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

        private void OnEnable()
        {
            shipCameraSettings = (ShipCameraSettings)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties
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

            isStylesInitialised = false;
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

                isStylesInitialised = true;
            }

            #endregion

            #region Header Info and Buttons
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField(titleContent);
            EditorGUILayout.LabelField(headerContent, EditorStyles.miniLabel);
            GUILayout.EndVertical();
            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region General Settings
            bool isAimAtTarget = cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.AimAtTarget;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (isAimAtTarget)
            {
                bool isTargetPosLockorFastMoveSpeed = lockToTargetPosProp.boolValue || moveSpeedProp.floatValue > 10f;
                bool isTargetRotLockorFastTurnSpeed = lockToTargetRotProp.boolValue || turnSpeedProp.floatValue > 10f;
                bool isLateUpdate = updateTypeProp.intValue == (int)ShipCameraModule.CameraUpdateType.LateUpdate;

                if (isLateUpdate)
                {
                    // Don't post Position warning if Lock Camera Position is in use
                    if (isTargetPosLockorFastMoveSpeed && !(isAimAtTarget && lockCameraPosProp.boolValue))
                    {
                        EditorGUILayout.HelpBox(ShipCameraModuleEditor.targetPosMsgSCM, MessageType.Warning);
                    }
                    if (isTargetRotLockorFastTurnSpeed)
                    {
                        EditorGUILayout.HelpBox(ShipCameraModuleEditor.targetRotMsgSCM, MessageType.Warning);
                    }
                }
            }

            // Suggest using Target Rotation if user has Aim To Target selected.
            if (isAimAtTarget && targetOffsetCoordinatesProp.intValue == (int)ShipCameraModule.TargetOffsetCoordinates.CameraRotation)
            {
                EditorGUILayout.HelpBox(ShipCameraModuleEditor.aimIncompatibleMsgSCM, MessageType.Warning);
            }

            EditorGUILayout.PropertyField(targetOffsetCoordinatesProp, ShipCameraModuleEditor.targetOffsetCoordsContentSCM);

            if (isAimAtTarget)
            {
                EditorGUILayout.PropertyField(lockCameraPosProp, ShipCameraModuleEditor.lockCameraPosContentSCM);
            }

            // Lock to Target Position and offset are not required if the camera position is locked
            if (!(isAimAtTarget && lockCameraPosProp.boolValue))
            {
                EditorGUILayout.PropertyField(targetOffsetProp, ShipCameraModuleEditor.targetOffsetContentSCM);

                EditorGUILayout.PropertyField(lockToTargetPosProp, ShipCameraModuleEditor.lockToTargetPosContentSCM);

                if (!lockToTargetPosProp.boolValue)
                {
                    EditorGUILayout.PropertyField(moveSpeedProp, ShipCameraModuleEditor.moveSpeedContentSCM);

                    #region Offset Damping
                    EditorGUILayout.PropertyField(targetOffsetDampingProp, ShipCameraModuleEditor.targetOffsetDampingContentSCM);
                    
                    if (targetOffsetDampingProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dampingMaxPitchOffsetUpProp, ShipCameraModuleEditor.dampingMaxPitchOffsetUpContentSCM);
                        EditorGUILayout.PropertyField(dampingMaxPitchOffsetDownProp, ShipCameraModuleEditor.dampingMaxPitchOffsetDownContentSCM);
                        EditorGUILayout.PropertyField(dampingPitchRateProp, ShipCameraModuleEditor.dampingPitchRateContentSCM);
                        EditorGUILayout.PropertyField(dampingPitchGravityProp, ShipCameraModuleEditor.dampingPitchGravityContentSCM);

                        EditorGUILayout.PropertyField(dampingMaxYawOffsetLeftProp, ShipCameraModuleEditor.dampingMaxYawOffsetLeftContentSCM);
                        EditorGUILayout.PropertyField(dampingMaxYawOffsetRightProp, ShipCameraModuleEditor.dampingMaxYawOffsetRightContentSCM);
                        EditorGUILayout.PropertyField(dampingYawRateProp, ShipCameraModuleEditor.dampingYawRateContentSCM);
                        EditorGUILayout.PropertyField(dampingYawGravityProp, ShipCameraModuleEditor.dampingYawGravityContentSCM);

                        if (dampingMaxPitchOffsetUpProp.floatValue < targetOffsetProp.vector3Value.y || dampingMaxPitchOffsetDownProp.floatValue > targetOffsetProp.vector3Value.y)
                        {
                            EditorGUILayout.HelpBox(ShipCameraModuleEditor.targetDampingPitchMsgSCM, MessageType.Warning);
                        }
                        if (dampingMaxYawOffsetRightProp.floatValue < targetOffsetProp.vector3Value.x || dampingMaxYawOffsetLeftProp.floatValue > targetOffsetProp.vector3Value.x)
                        {
                            EditorGUILayout.HelpBox(ShipCameraModuleEditor.targetDampingYawMsgSCM, MessageType.Warning);
                        }
                    }
                    #endregion
                }
            }

            EditorGUILayout.PropertyField(lockToTargetRotProp, ShipCameraModuleEditor.lockToTargetRotContentSCM);
            if (!lockToTargetRotProp.boolValue)
            {
                EditorGUILayout.PropertyField(turnSpeedProp, ShipCameraModuleEditor.turnSpeedContentSCM);
            }

            EditorGUILayout.PropertyField(cameraRotationModeProp, ShipCameraModuleEditor.cameraRotationModeContentSCM);

            if (cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.FollowVelocity ||
                cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.TopDownFollowVelocity)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("followVelocityThreshold"), ShipCameraModuleEditor.followVelocityThresholdContentSCM);
            }

            if (cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.TopDownFollowTargetRotation &&
                cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.TopDownFollowVelocity &&
                cameraRotationModeProp.intValue != (int)ShipCameraModule.CameraRotationMode.Fixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("orientUpwards"), ShipCameraModuleEditor.orientUpwardsContentSCM);
            }

            if (cameraRotationModeProp.intValue == (int)ShipCameraModule.CameraRotationMode.Fixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraFixedRotation"), ShipCameraModuleEditor.cameraFixedRotationContentSCM);
            }
                
            EditorGUILayout.PropertyField(updateTypeProp, ShipCameraModuleEditor.updateTypeContentSCM);

            EditorGUILayout.PropertyField(maxShakeStrengthProp, ShipCameraModuleEditor.maxShakeStrengthContentSCM);

            if (maxShakeStrengthProp.floatValue > 0f)
            {
                EditorGUILayout.PropertyField(maxShakeDurationProp, ShipCameraModuleEditor.maxShakeDurationContentSCM);
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Clip Objects
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(clipObjectsProp, ShipCameraModuleEditor.clipObjectsContentSCM);

            if (clipObjectsProp.boolValue)
            {
                SSCEditorHelper.InTechPreview(false);

                EditorGUILayout.PropertyField(minClipMoveSpeedProp, ShipCameraModuleEditor.minClipMoveSpeedContentSCM);
                EditorGUILayout.PropertyField(clipMinDistanceProp, ShipCameraModuleEditor.clipMinDistanceContentSCM);

                EditorGUILayout.PropertyField(clipMinOffsetXProp, ShipCameraModuleEditor.clipMinOffsetXContentSCM);
                EditorGUILayout.PropertyField(clipMinOffsetYProp, ShipCameraModuleEditor.clipMinOffsetYContentSCM);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ShipCameraModuleEditor.clipObjectMaskContentSCM, GUILayout.Width(defaultEditorLabelWidth - 58f));
                if (GUILayout.Button(ShipCameraModuleEditor.resetBtnContentSCM, buttonCompact, GUILayout.MaxWidth(50f)))
                {
                    clipObjectMaskProp.intValue = (int)ShipCameraModule.DefaultClipObjectMask;
                }
                EditorGUILayout.PropertyField(clipObjectMaskProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                // When first added or if user attempts to set to Nothing, reset to defaults.
                if (clipObjectMaskProp.intValue == 0)
                {
                    clipObjectMaskProp.intValue = (int)ShipCameraModule.DefaultClipObjectMask;
                }
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Zoom Settings

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(isZoomEnabledProp, ShipCameraModuleEditor.isZoomEnabledContentSCM);

            if (isZoomEnabledProp.boolValue)
            {
                EditorGUILayout.PropertyField(unzoomDelayProp, ShipCameraModuleEditor.unzoomDelayContentSCM);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(zoomedInFoVProp, ShipCameraModuleEditor.zoomedInFoVContentSCM);
                if (EditorGUI.EndChangeCheck())
                {
                    if (zoomedInFoVProp.floatValue > unzoomedFoVProp.floatValue)
                    {
                        zoomedInFoVProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(unzoomedFoVProp, ShipCameraModuleEditor.unzoomedFoVContentSCM);
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
                EditorGUILayout.PropertyField(zoomedOutFovProp, ShipCameraModuleEditor.zoomedOutFoVContentSCM);
                if (EditorGUI.EndChangeCheck())
                {
                    if (zoomedOutFovProp.floatValue < unzoomedFoVProp.floatValue)
                    {
                        zoomedOutFovProp.floatValue = unzoomedFoVProp.floatValue;
                    }
                }

                EditorGUILayout.PropertyField(zoomDampingProp, ShipCameraModuleEditor.zoomDampingContentSCM);
            }

            EditorGUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}