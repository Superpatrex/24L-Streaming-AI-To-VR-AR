using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyInteractorBridge))]
    public class StickyInteractorBridgeEditor : Editor
    {
        #region Custom Editor private variables
        private StickyInteractorBridge stickyInteractorBridge;
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

        // Parameters for left or right hand VR animator
        private List<S3DAnimParm> animParamsFloatVRList = null;

        private string[] animParamFloatVRNames = null;

        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This component enables a non-Sticky3D object or hand to interact with Sticky Interactive objects.");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent generalSettingsContent = new GUIContent("General Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent interactiveSettingsContent = new GUIContent("Interactive Settings", "In Unity 2019+ you may need to click slightly to the right of the arrow");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky Interactor Bridge component is enabled through code.");
        private readonly static GUIContent isLeftHandContent = new GUIContent(" Left Hand", "Is this the left hand?");
        #endregion

        #region GUIContent - Activate
        private readonly static GUIContent isCanActivateContent = new GUIContent(" Can Activate", "Can this item activate an interactive-enabled Activable object?");
        #endregion

        #region GUIContent - Grab
        private readonly static GUIContent isCanGrabContent = new GUIContent(" Can Grab", "Can this item grab an interactive-enabled Grabbable object?");
        private readonly static GUIContent palmTransformContent = new GUIContent("  Palm Transform", "The z-axis (forward) should point in the direction of the palm is facing. This is the palm “normal”. The x-axis (right) is towards the direction of the index finger and, for the right hand, the y-axis (up) is in a thumbs-up direction.");
        private readonly static GUIContent isAutoDropContent = new GUIContent("  Auto Drop", "Automatically drop (let go of) an object that goes outside the hand collider range.");
        private readonly static GUIContent isAutoGrabContent = new GUIContent("  Auto Grab", "The component will attempt to automatically grab an interactive-enabled Grabbable object when it is in range.");

        #endregion

        #region GUIContent - Animate Hand VR

        private readonly static GUIContent isHandVRSettingsContent = new GUIContent("Hand Animate Settings");
        private readonly static GUIContent handVRAnimatorContent = new GUIContent(" Hand Animator", "The animator used to animate a Virtual Reality left or right hand");

        private readonly static GUIContent handVRGripParmNameContent = new GUIContent("  Grip Parameter", "The (float) parameter name from the animation controller for the grip action");
        private readonly static GUIContent handVRTriggerParmNameContent = new GUIContent("  Trigger Parameter", "The (float) parameter name from the animation controller for the trigger action");

        private readonly static GUIContent aaNoneContent = new GUIContent(" None Available", "No data available");
        #endregion

        #region GUIContent - Touch
        private readonly static GUIContent isCanTouchContent = new GUIContent(" Can Touch", "Can this item touch an interactive-enabled Touchable object?");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty showInteractiveSettingsInEditorProp;
        private SerializedProperty initialiseOnStartProp;
        #endregion

        #region Serialized Properties - Activate
        private SerializedProperty isCanActivateProp;
        #endregion

        #region Serialized Properties - Grab
        private SerializedProperty isCanGrabProp;
        private SerializedProperty palmTransformProp;
        private SerializedProperty isAutoDropProp;
        private SerializedProperty isAutoGrabProp;

        #endregion

        #region Serialized Properties - Hand VR

        private SerializedProperty isHandVRExpandedProp;
        private SerializedProperty handVRAnimatorProp;
        private SerializedProperty isLeftHandProp;
        private SerializedProperty s3dAnimActionProp;
        private SerializedProperty s3dAAParamHashCodeProp;

        #endregion

        #region Serialized Properties - Touch
        private SerializedProperty isCanTouchProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            stickyInteractorBridge = (StickyInteractorBridge)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            #region Find Properties - General
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor");
            showInteractiveSettingsInEditorProp = serializedObject.FindProperty("showInteractiveSettingsInEditor");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            #endregion

            #region Find Properties - Activate
            isCanActivateProp = serializedObject.FindProperty("isCanActivate");
            #endregion

            #region Find Properties - Grab
            isCanGrabProp = serializedObject.FindProperty("isCanGrab");
            palmTransformProp = serializedObject.FindProperty("palmTransform");
            isAutoGrabProp = serializedObject.FindProperty("isAutoGrab");
            isAutoDropProp = serializedObject.FindProperty("isAutoDrop");

            #endregion

            #region Find Properties - Hand VR
            isHandVRExpandedProp = serializedObject.FindProperty("isHandVRExpanded");
            handVRAnimatorProp = serializedObject.FindProperty("handAnimator");
            isLeftHandProp = serializedObject.FindProperty("isLeftHand");
            #endregion

            #region Find Properties - Touch
            isCanTouchProp = serializedObject.FindProperty("isCanTouch");
            #endregion

            stickyInteractorBridge.ValidateAnimActions();
            RefreshVRAnimatorParameters();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draw the hand vr settings in the inspector
        /// </summary>
        private void DrawHandVRSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(isHandVRExpandedProp, isHandVRSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isHandVRExpandedProp.boolValue)
            {
                stickyInteractorBridge.ValidateAnimActions();

                #if SCSM_XR && SSC_UIS
                #else
                EditorGUILayout.HelpBox("Hand VR requires Unity 2020.3+ and Unity XR - See Help for details", MessageType.Error);
                #endif

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(handVRAnimatorProp, handVRAnimatorContent);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshVRAnimatorParameters();
                }

                if (handVRAnimatorProp.objectReferenceValue != null)
                {
                    s3dAnimActionProp = serializedObject.FindProperty("handVRGrip");
                    s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                    DrawParameterSelection(animParamsFloatVRList, animParamFloatVRNames, s3dAAParamHashCodeProp, handVRGripParmNameContent);

                    s3dAnimActionProp = serializedObject.FindProperty("handVRTrigger");
                    s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");

                    DrawParameterSelection(animParamsFloatVRList, animParamFloatVRNames, s3dAAParamHashCodeProp, handVRTriggerParmNameContent);
                }
            }

            EditorGUILayout.EndVertical();
        }


        /// <summary>
        /// Draw a drop down list of animator parameters
        /// </summary>
        /// <param name="paramList"></param>
        /// <param name="paramNames"></param>
        /// <param name="hashCodeProperty"></param>
        /// <param name="labelContent"></param>
        private void DrawParameterSelection(List<S3DAnimParm> paramList, string[] paramNames, SerializedProperty hashCodeProperty, GUIContent labelContent)
        {
            if (paramNames == null) { RefreshVRAnimatorParameters(); }

            if (paramNames == null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(aaNoneContent, GUILayout.Width(defaultEditorLabelWidth - 8f));
                GUILayout.EndHorizontal();
            }
            else
            {
                int paramIdx = paramList.FindIndex(p => p.hashCode == hashCodeProperty.intValue);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelContent, GUILayout.Width(defaultEditorLabelWidth - 1f));

                EditorGUI.BeginChangeCheck();
                paramIdx = EditorGUILayout.Popup(paramIdx, paramNames);
                if (EditorGUI.EndChangeCheck())
                {
                    // The parameter list and the name array should be in synch. See RefreshAnimatorParameters()
                    if (paramIdx < paramList.Count)
                    {
                        hashCodeProperty.intValue = paramList[paramIdx].hashCode;
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Fetch the current paramaters from the left or right hand VR animators
        /// </summary>
        private void RefreshVRAnimatorParameters()
        {
            if (animParamsFloatVRList == null) { animParamsFloatVRList = new List<S3DAnimParm>(10); }
            else { animParamsFloatVRList.Clear(); }

            if (stickyInteractorBridge.handAnimator != null)
            {
                // Synch the list of parameters from the animator controller with the array of names used in the editor

                // Populate the lists from the animator
                S3DAnimParm.GetParameterList(stickyInteractorBridge.handAnimator, animParamsFloatVRList, S3DAnimAction.ParameterType.Float);

                // Get arrays of names for use in the popup controls
                animParamFloatVRNames = S3DAnimParm.GetParameterNames(animParamsFloatVRList);
            }
            else
            {
                animParamFloatVRNames = null;
            }
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            stickyInteractorBridge.allowRepaint = false;
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
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            EditorGUILayout.EndVertical();
            #endregion

            StickyEditorHelper.InTechPreview(true);

            #region General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showGeneralSettingsInEditorProp, generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showGeneralSettingsInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
                EditorGUILayout.PropertyField(isLeftHandProp, isLeftHandContent);
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Interactive Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawS3DFoldout(showInteractiveSettingsInEditorProp, interactiveSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (showInteractiveSettingsInEditorProp.boolValue)
            {
                #region Activate
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isCanActivateProp, isCanActivateContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractorBridge.SetIsCanActivate(isCanActivateProp.boolValue);
                }
                #endregion

                #region Grab
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isCanGrabProp, isCanGrabContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractorBridge.SetIsCanGrab(isCanGrabProp.boolValue);
                }

                if (isCanGrabProp.boolValue)
                {
                    EditorGUILayout.PropertyField(isAutoDropProp, isAutoDropContent);
                    EditorGUILayout.PropertyField(isAutoGrabProp, isAutoGrabContent);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(palmTransformProp, palmTransformContent);
                    if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                    {
                        if (!stickyInteractorBridge.SetHandPalmTransform((Transform)palmTransformProp.objectReferenceValue))
                        {
                            palmTransformProp.objectReferenceValue = null;
                        }
                    }
                }
                #endregion

                #region Touch
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isCanTouchProp, isCanTouchContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    stickyInteractorBridge.SetIsCanTouch(isCanTouchProp.boolValue);
                }
                #endregion
            }


            EditorGUILayout.EndVertical();
            #endregion

            DrawHandVRSettings();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion

            stickyInteractorBridge.allowRepaint = true;
        }

        #endregion
    }
}