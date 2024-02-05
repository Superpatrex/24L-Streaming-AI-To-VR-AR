using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(SSCDoorAnimator))]
    public class SSCDoorAnimatorEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables

        private SSCDoorAnimator sscDoorAnimator = null;
        private float defaultEditorLabelWidth = 175f;

        #endregion

        #region GUIContent
        private readonly static GUIContent getBoolParmsBtnContent = new GUIContent("Get bool Parameters", "Get all the boolean parameters from the animation controller. If you see 'Animator is not playing an Animation Controller', go in an out of play mode.");
        private readonly static GUIContent getTriggerParmsBtnContent = new GUIContent("Get trigger Parameters", "Get all the trigger parameters from the animation controller. If you see 'Animator is not playing an Animation Controller', go in an out of play mode.");
        private readonly static GUIContent openingSpeedContent = new GUIContent("Opening Speed", "The Opening speed of the door. Min value 0.01, Max 10. Default 1");
        private readonly static GUIContent closingSpeedContent = new GUIContent("Closing Speed", "The Closing speed of the door. Min value 0.01, Max 10. Default 1");
        private readonly static GUIContent openParamNamesContent = new GUIContent("Open Parameter Names", "Array of bool or trigger Animimation Parameters to control one or more doors.");
        private readonly static GUIContent closeParamNamesContent = new GUIContent("Close Parameter Names", "Array of trigger Animimation Parameters to control one or more doors These must have the word Close in their names");
        private readonly static GUIContent parameterTypeContent = new GUIContent("Parameter Type", "Animation parameter types include boolean or triggers");
        private readonly static GUIContent isLockedStatusesContent = new GUIContent("Is Locked Statuses", "Array of isLocked statuses. There should be one for each door or sets of doors.");
        private readonly static GUIContent openingAudioClipContent = new GUIContent("Opening Audio Clip", "The audio clip that is played when the doors are opening.");
        private readonly static GUIContent closingAudioClipContent = new GUIContent("Closing Audio Clip", "The audio clip that is played when the doors are closing.");
        private readonly static GUIContent isLockedAudioClipContent = new GUIContent("Is Locked Audio Clip", "The audio clip that is played when the an attempt is made to open a door but the door is locked");
        private readonly static GUIContent openingClipVolumeContent = new GUIContent("Opening Clip Volume", "The relative volume of the Opening Audio Clip compared to the initial volume of the Audio Source");
        private readonly static GUIContent closingClipVolumeContent = new GUIContent("Closing Clip Volume", "The relative volume of the Closing Audio Clip compared to the initial volume of the Audio Source");
        private readonly static GUIContent isLockedClipVolumeContent = new GUIContent("Is Locked Clip Volume", "The relative volume of the Is Locked Audio Clip compared to the initial volume of the Audio Source");
        private readonly static GUIContent onOpeningContent = new GUIContent("On Opening", "These are triggered by a door when it starts to open");
        private readonly static GUIContent onClosingContent = new GUIContent("On Closing", "These are triggered by a door when it starts to close");
        #endregion

        #region Static Strings
        private readonly static string isLockedMismatchMsg = "The number of isLocked statuses does not match the number of doors or sets of doors being controlled. This is indicated by the number of Open Param Names.";
        #endregion

        #region Properties
        private SerializedProperty openingSpeedProp;
        private SerializedProperty closingSpeedProp;
        private SerializedProperty parameterTypeProp;
        private SerializedProperty openParamNamesProp;
        private SerializedProperty closeParamNamesProp;
        private SerializedProperty isLockedStatusesProp;
        private SerializedProperty openingAudioClipProp;
        private SerializedProperty closingAudioClipProp;
        private SerializedProperty isLockedAudioClipProp;
        private SerializedProperty openingClipVolumeProp;
        private SerializedProperty closingClipVolumeProp;
        private SerializedProperty isLockedClipVolumeProp;
        private SerializedProperty onOpeningProp;
        private SerializedProperty onClosingProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            sscDoorAnimator = (SSCDoorAnimator)target;

            openingSpeedProp = serializedObject.FindProperty("openingSpeed");
            closingSpeedProp = serializedObject.FindProperty("closingSpeed");
            parameterTypeProp = serializedObject.FindProperty("parameterType");
            openParamNamesProp = serializedObject.FindProperty("openParamNames");
            closeParamNamesProp = serializedObject.FindProperty("closeParamNames");
            isLockedStatusesProp = serializedObject.FindProperty("isLockedStatuses");
            openingAudioClipProp = serializedObject.FindProperty("openingAudioClip");
            closingAudioClipProp = serializedObject.FindProperty("closingAudioClip");
            isLockedAudioClipProp = serializedObject.FindProperty("isLockedAudioClip");
            openingClipVolumeProp = serializedObject.FindProperty("openingClipVolume");
            closingClipVolumeProp = serializedObject.FindProperty("closingClipVolume");
            isLockedClipVolumeProp = serializedObject.FindProperty("isLockedClipVolume");
            onOpeningProp = serializedObject.FindProperty("onOpening");
            onClosingProp = serializedObject.FindProperty("onClosing");
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;

            GUILayout.BeginVertical("HelpBox");

            if (sscDoorAnimator != null)
            {
                if (parameterTypeProp.intValue == (int)SSCDoorAnimator.ParmeterType.Bool)
                {
                    #region Get Bool Parms
                    if (GUILayout.Button(getBoolParmsBtnContent, GUILayout.Width(150f)))
                    {
                        GetParameterNames(SSCDoorAnimator.ParmeterType.Bool);
                    }
                    #endregion
                }
                else
                {
                    #region Get Trigger Parms
                    if (GUILayout.Button(getTriggerParmsBtnContent, GUILayout.Width(150f)))
                    {
                        GetParameterNames(SSCDoorAnimator.ParmeterType.Trigger);
                    }
                    #endregion
                }
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(openingSpeedProp, openingSpeedContent);
            EditorGUILayout.PropertyField(closingSpeedProp, closingSpeedContent);

            EditorGUILayout.PropertyField(parameterTypeProp, parameterTypeContent);

            SSCEditorHelper.DrawArray(openParamNamesProp, openParamNamesContent, defaultEditorLabelWidth, "Parameter");

            if (parameterTypeProp.intValue == (int)SSCDoorAnimator.ParmeterType.Trigger)
            {
                SSCEditorHelper.DrawArray(closeParamNamesProp, closeParamNamesContent, defaultEditorLabelWidth, "Parameter");
            }

            SSCEditorHelper.DrawArray(isLockedStatusesProp, isLockedStatusesContent, defaultEditorLabelWidth, "Door");

            if (isLockedStatusesProp.arraySize != openParamNamesProp.arraySize)
            {
                EditorGUILayout.HelpBox(isLockedMismatchMsg, MessageType.Warning);
            }

            EditorGUILayout.PropertyField(openingAudioClipProp, openingAudioClipContent);
            EditorGUILayout.PropertyField(openingClipVolumeProp, openingClipVolumeContent);
            EditorGUILayout.PropertyField(closingAudioClipProp, closingAudioClipContent);
            EditorGUILayout.PropertyField(closingClipVolumeProp, closingClipVolumeContent);
            EditorGUILayout.PropertyField(isLockedAudioClipProp, isLockedAudioClipContent);
            EditorGUILayout.PropertyField(isLockedClipVolumeProp, isLockedClipVolumeContent);

            GUILayout.EndVertical();

            GUILayoutUtility.GetRect(1f, 2f);

            #region Event Settings
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(onOpeningProp, onOpeningContent);
            EditorGUILayout.PropertyField(onClosingProp, onClosingContent);
            EditorGUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();

            //DrawDefaultInspector();
        }

        #endregion

        #region Private Methods

        private void GetParameterNames(SSCDoorAnimator.ParmeterType parmType)
        {
            Animator _animator = sscDoorAnimator.GetComponent<Animator>();

            if (_animator != null && _animator.runtimeAnimatorController != null && SSCUtils.IsAnimatorReady(_animator))
            {
                if (!_animator.isInitialized) { Debug.Log("Animator is not initialised. Go in and out of play mode to fix and ensure the gameobject is enabled"); return; }

                var parms = _animator.parameters;

                int numParms = parms == null ? 0 : parms.Length;
                List<string> openParmNames = new List<string>();
                List<string> closeParamNames = new List<string>();

                // Loop through all the parameters
                for (int paramIdx = 0; paramIdx < numParms; paramIdx++)
                {
                    AnimatorControllerParameter p = parms[paramIdx];

                    if (p.type == AnimatorControllerParameterType.Bool && parmType == SSCDoorAnimator.ParmeterType.Bool)
                    {
                        openParmNames.Add(p.name);
                    }
                    // Triggers can be open or close triggers in this component.
                    else if (p.type == AnimatorControllerParameterType.Trigger && parmType == SSCDoorAnimator.ParmeterType.Trigger)
                    {
                        if (p.name.ToLower().Contains("close"))
                        {
                            closeParamNames.Add(p.name);
                        }
                        else
                        {
                            openParmNames.Add(p.name);
                        }
                    }
                }

                int numOpenParms = openParmNames == null ? 0 : openParmNames.Count;
                int numCloseParms = closeParamNames == null ? 0 : closeParamNames.Count;

                if (numOpenParms > 0 || numCloseParms > 0)
                {
                    string msg = "This will replace existing array(s) Param Names with the ";
                    msg += parmType == SSCDoorAnimator.ParmeterType.Bool ? "bool " : "trigger ";
                    msg += "Parameters from the Animator Controller (" + _animator.runtimeAnimatorController.name + ").\n\nDo you wish to continue";

                    if (SSCEditorHelper.PromptForContinue("Get Params", msg))
                    {
                        Undo.RecordObject(sscDoorAnimator, "Get Parameters from Animator");

                        sscDoorAnimator.openParamNames = openParmNames.ToArray();
                        sscDoorAnimator.closeParamNames = closeParamNames.ToArray();

                        // Force persistence in the scene
                        serializedObject.Update();
                        float _origSpeed = openingSpeedProp.floatValue;
                        openingSpeedProp.floatValue += 0.01f;
                        openingSpeedProp.floatValue = _origSpeed;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                GUIUtility.ExitGUI();
            }
            else
            {
                EditorUtility.DisplayDialog("Get Parameters", "First configure an Animation Controller and add it to the Animator attached to this gameobject", "Got it!");
            }
        }

        #endregion
    }
}