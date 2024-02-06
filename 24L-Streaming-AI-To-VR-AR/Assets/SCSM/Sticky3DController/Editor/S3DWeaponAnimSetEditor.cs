using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(S3DWeaponAnimSet))]
    public class S3DWeaponAnimSetEditor : Editor
    {
        #region Custom Editor private variables
        private S3DWeaponAnimSet s3dWeaponAnimSet = null;
        private bool isStylesInitialised = false;
        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle miniLabelWrappedText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        private readonly static string modelElementName = "Model";

        private int s3dClipPairDeletePos = -1;
        private int s3dAnimActionDeletePos = -1;
        private int s3dAnimActionMoveDownPos = -1;

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Weapon Anim Set");
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of animation clip pairs used with one or more Sticky3D character Model IDs. They can also send weapon data to the animation controller on a character.");
        #endregion

        #region GUIContent - General
        private readonly static GUIContent isAimIKWhenNotAimingContent = new GUIContent("Aim IK When Not Aiming", "When the weapon is held by a character, it will always use Aim IK, even when it is not specifically being aimed.");
        private readonly static GUIContent isSkipParmVerifyContent = new GUIContent("Skip Parameter Verify", "Should anim action parameter name verification be skipped when the set is applied? Can be faster but will cause errors if parameter names don't exist in animator.");
        private readonly static GUIContent isAppliedToAllCharactersContent = new GUIContent("Apply to All Characters", "Should this be applied to all characters?");
        private readonly static GUIContent modelIDsContent = new GUIContent("Character Model IDs", "An array of Sticky3D Controller Module Model IDs. A Model ID (found on the Engage tab) groups characters with similar attributes together. E.g., Same rig or animation controller.");
        private readonly static GUIContent aimIKTurnDelayContent = new GUIContent("Aim IK Turn Delay", "The time, in seconds, to delay the character turning to face the target when aiming starts. This allows time to transition from a held animation, to an aiming animation.");
        private readonly static GUIContent aimIKFPWeaponOffsetContent = new GUIContent("Aim IK Weapon Offset", "The weapon local space offset from the first-person camera when aiming");
        private readonly static GUIContent aimIKFPNearClippingPlaneContent = new GUIContent("Aim IK Near Clipping", "The first person camera near clipping plane when aiming");
        private readonly static GUIContent animClipPairRevertDelayContent = new GUIContent("Clip Revert Delay", "The number of seconds to delay reverting animation clips for a character to their originals when the weapon is dropped.");
        private readonly static GUIContent isFreeLookWhenHeldContent = new GUIContent("Free Look When Held", "If a non-NPC character has Free Look enabled before the weapon is held (but not aimed), does Free Look remain enabled? Useful when a weapon is held in a relaxed pose not pointing forward.");
        private readonly static GUIContent heldTransitionDurationContent = new GUIContent("Held Transition Duration", "When grabbing a weapon, this is the time, in seconds, it takes the animation to play that transitions from not holding, to holding the weapon.");
        private readonly static GUIContent isAimTPUsingFPCameraContent = new GUIContent("TP Aim uses FP Camera", "When a weapon is aimed and held by a character in 3rd person, should it use the first-person camera?");
        private readonly static GUIContent reverseContent = new GUIContent("<->", "Switch the order of all the clips");
        private readonly static GUIContent resetContent = new GUIContent("Reset Identifiers", "This should be used after duplicating or copying a weapon anim set. It gives the various elements a unique identification (guidHash).");
        #endregion

        #region GUIContent - Weapon
        private readonly static GUIContent isWeaponSettingsOverrideContent = new GUIContent("Weapon Override", "Should the weapon settings be overridden when the character picks up this weapon?");
        private readonly static GUIContent isOnlyFireWhenAiming1Content = new GUIContent("Only Fire When Aiming 1", "Fire button 1 can only fire when the weapon is being aimed");
        private readonly static GUIContent isOnlyFireWhenAiming2Content = new GUIContent("Only Fire When Aiming 2", "Fire button 2 can only fire when the weapon is being aimed");

        #endregion

        #region GUIContent - Anim Action Ext
        private readonly static GUIContent aaWeaponContent = new GUIContent("Weapon");
        private readonly static GUIContent aaWeaponActionContent = new GUIContent(" Weapon Action", "This is the action that happens that causes the animation to take place. It helps you remember why you set it up.");
        private readonly static GUIContent aaParmTypeContent = new GUIContent(" Parameter Type", "The type of animation parameter, if any, used with this action");

        private readonly static GUIContent aaCharacterContent = new GUIContent("Character Animator");
        private readonly static GUIContent aaParmNameContent = new GUIContent(" Parameter Name", "The name of the parameter in the character animation controller.");
        private readonly static GUIContent aaIsExitActionContent = new GUIContent(" Is Exit Action", "Used for weapon actions like dropped, equipped, socketed or stashed to trigger an immediate exit from an Animation Layer while holding a weapon.");

        private readonly static GUIContent aaBoolValueContent = new GUIContent(" Bool Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatValueContent = new GUIContent(" Float Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaTriggerValueContent = new GUIContent(" Trigger Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaIntegerValueContent = new GUIContent(" Integer Value", "The realtime value from the weapon that will be sent to the model's animation controller");
        private readonly static GUIContent aaFloatMultiplierContent = new GUIContent(" Float Multiplier", "A value that is used to multiple or change the value of the float value being passed to the animation controller. Can speed up or slow down an animation.");
        private readonly static GUIContent aaFloatFixedValueContent = new GUIContent(" Fixed Value", "The fixed float value being passed to the animation controller.");
        private readonly static GUIContent aaBoolFixedValueContent = new GUIContent(" Fixed Value", "True or False being passed to the animation controller.");
        private readonly static GUIContent aaDampingContent = new GUIContent(" Damping", "The damping applied to help smooth transitions, especially with Blend Trees. Currently only used for floats. For quick transitions to the new float value use a low damping value, for the slower transitions use more damping.");
        private readonly static GUIContent aaIsInvertContent = new GUIContent(" Invert", "When the value is true, use false instead. When the value is false, use true instead. Not compatible with Toggle");
        private readonly static GUIContent aaIsToggleContent = new GUIContent(" Toggle", "Works with bool custom anim actions to toggle the existing parameter value in the animator controller. Not compatible with Invert or Reset After Use.");
        private readonly static GUIContent aaCustomValueContent = new GUIContent("User game code");
        private readonly static GUIContent aaIsResetCustomAfterUseContent = new GUIContent(" Reset After Use", "Works with bool custom anim actions to reset to false after it has been sent to the animator controller. Has no effect if Toggle is true. [Default: True]");

        #endregion

        #region Serialized Properties - General
        private SerializedProperty isAimIKWhenNotAimingProp;
        private SerializedProperty isSkipParmVerifyProp;
        private SerializedProperty isAppliedToAllCharactersProp;
        private SerializedProperty isFreeLookWhenHeldProp;
        private SerializedProperty heldTransitionDurationProp;
        private SerializedProperty isAimTPUsingFPCameraProp;
        private SerializedProperty stickyModelIDsProp;
        private SerializedProperty isModelsExpandedInEditorProp;
        private SerializedProperty animClipPairListProp;
        private SerializedProperty animClipPairProp;
        private SerializedProperty originalClipProp;
        private SerializedProperty replacementClipProp;
        private SerializedProperty aimIKTurnDelayProp;
        private SerializedProperty aimIKFPWeaponOffsetProp;
        private SerializedProperty aimIKFPNearClippingPlaneProp;
        private SerializedProperty animClipPairRevertDelayProp;

        private SerializedProperty isWeaponSettingsOverrideProp;
        private SerializedProperty isOnlyFireWhenAiming1Prop;
        private SerializedProperty isOnlyFireWhenAiming2Prop;

        private SerializedProperty s3dAAListProp;
        private SerializedProperty s3dAnimActionProp;
        private SerializedProperty s3dAAShowInEditorProp;
        private SerializedProperty s3dAAWeaponActionProp;
        private SerializedProperty s3dAAParamTypeProp;
        private SerializedProperty s3dAAParamNameProp;
        private SerializedProperty s3dAAParamHashCodeProp;
        private SerializedProperty s3dAAValueProp;
        private SerializedProperty s3dAAIsExitActionProp;
        private SerializedProperty s3dAAIsInvertProp;
        private SerializedProperty s3dAAIsToggleProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            s3dWeaponAnimSet = (S3DWeaponAnimSet)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            //// Used in Richtext labels
            //if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            //// Keep compiler happy - can remove this later if it isn't required
            //if (defaultTextColour.a > 0f) { }

            #region Find Properties - General
            stickyModelIDsProp = serializedObject.FindProperty("stickyModelIDs");
            isAimIKWhenNotAimingProp = serializedObject.FindProperty("isAimIKWhenNotAiming");
            isSkipParmVerifyProp = serializedObject.FindProperty("isSkipParmVerify");
            isAppliedToAllCharactersProp = serializedObject.FindProperty("isAppliedToAllCharacters");
            isFreeLookWhenHeldProp = serializedObject.FindProperty("isFreeLookWhenHeld");
            heldTransitionDurationProp = serializedObject.FindProperty("heldTransitionDuration");
            isAimTPUsingFPCameraProp = serializedObject.FindProperty("isAimTPUsingFPCamera");
            isModelsExpandedInEditorProp = serializedObject.FindProperty("isModelsExpandedInEditor");
            animClipPairRevertDelayProp = serializedObject.FindProperty("animClipPairRevertDelay");
            aimIKTurnDelayProp = serializedObject.FindProperty("aimIKTurnDelay");
            aimIKFPWeaponOffsetProp = serializedObject.FindProperty("aimIKFPWeaponOffset");
            aimIKFPNearClippingPlaneProp = serializedObject.FindProperty("aimIKFPNearClippingPlane");
            animClipPairListProp = serializedObject.FindProperty("animClipPairList");

            s3dAAListProp = serializedObject.FindProperty("animActionExtList");
            #endregion

            #region Find Properties - Weapon
            isWeaponSettingsOverrideProp = serializedObject.FindProperty("isWeaponSettingsOverride");
            isOnlyFireWhenAiming1Prop = serializedObject.FindProperty("isOnlyFireWhenAiming1");
            isOnlyFireWhenAiming2Prop = serializedObject.FindProperty("isOnlyFireWhenAiming2");
            #endregion

            isStylesInitialised = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draw the list of S3DAnimActionExts in the inspector
        /// </summary>
        private void DrawAnimActions()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            #region Check if anim action List is null
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (s3dWeaponAnimSet.animActionExtList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dWeaponAnimSet.animActionExtList = new List<S3DAnimActionExt>(6);
                EditorUtility.SetDirty(s3dWeaponAnimSet);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove Anim Actions
            s3dAnimActionDeletePos = -1;
            s3dAnimActionMoveDownPos = -1;
            int numAnimActions = s3dAAListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anim Actions", GUILayout.Width(100f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numAnimActions < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dWeaponAnimSet, "Add Anim Action");
                s3dWeaponAnimSet.animActionExtList.Add(new S3DAnimActionExt());
                EditorUtility.SetDirty(s3dWeaponAnimSet);

                // Read in the properties
                serializedObject.Update();

                numAnimActions = s3dAAListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numAnimActions > 0) { s3dAnimActionDeletePos = s3dAAListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Anim Action List

            for (int aaIdx = 0; aaIdx < numAnimActions; aaIdx++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                s3dAnimActionProp = s3dAAListProp.GetArrayElementAtIndex(aaIdx);

                #region Get Properties for the Animate Action
                s3dAAShowInEditorProp = s3dAnimActionProp.FindPropertyRelative("showInEditor");
                s3dAAWeaponActionProp = s3dAnimActionProp.FindPropertyRelative("weaponAction");
                s3dAAParamTypeProp = s3dAnimActionProp.FindPropertyRelative("parameterType");
                s3dAAParamNameProp = s3dAnimActionProp.FindPropertyRelative("parameterName");
                s3dAAParamHashCodeProp = s3dAnimActionProp.FindPropertyRelative("paramHashCode");
                s3dAAIsExitActionProp = s3dAnimActionProp.FindPropertyRelative("isExitAction");
                #endregion

                #region AnimAction Move/Insert/Delete buttons
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel += 1;
                s3dAAShowInEditorProp.boolValue = EditorGUILayout.Foldout(s3dAAShowInEditorProp.boolValue, "Animate Action " + (aaIdx + 1).ToString("00") + (s3dAAShowInEditorProp.boolValue ? "" : " - (" + (S3DAnimAction.WeaponAction)s3dAAWeaponActionProp.intValue + ")"));
                EditorGUI.indentLevel -= 1;

                // Move down button
                if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numAnimActions > 1) { s3dAnimActionMoveDownPos = aaIdx; }
                // Delete button
                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimActionDeletePos = aaIdx; }
                GUILayout.EndHorizontal();
                #endregion

                if (s3dAAShowInEditorProp.boolValue)
                {
                    EditorGUILayout.LabelField(aaWeaponContent);
                    StickyEditorHelper.DrawPropertyIndent(10f, s3dAAWeaponActionProp, aaWeaponActionContent, defaultEditorLabelWidth);
                    StickyEditorHelper.DrawPropertyIndent(10f, s3dAAParamTypeProp, aaParmTypeContent, defaultEditorLabelWidth);

                    EditorGUILayout.LabelField(aaCharacterContent);

                    EditorGUI.BeginChangeCheck();
                    StickyEditorHelper.DrawPropertyIndent(10f, s3dAAParamNameProp, aaParmNameContent, defaultEditorLabelWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (string.IsNullOrEmpty(s3dAAParamNameProp.stringValue))
                        {
                            s3dAAParamHashCodeProp.intValue = 0;
                        }
                        else
                        {
                            s3dAAParamHashCodeProp.intValue = Animator.StringToHash(s3dAAParamNameProp.stringValue);
                        }
                    }

                    StickyEditorHelper.DrawPropertyIndent(10f, s3dAAIsExitActionProp, aaIsExitActionContent, defaultEditorLabelWidth);

                    #region Bool
                    if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeBoolInt)
                    {
                        s3dAAIsInvertProp = s3dAnimActionProp.FindPropertyRelative("isInvert");
                        s3dAAIsToggleProp = s3dAnimActionProp.FindPropertyRelative("isToggle");

                        if (IsCustomAction(s3dAAWeaponActionProp))
                        {
                            EditorGUI.BeginChangeCheck();
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAAIsInvertProp, aaIsInvertContent, defaultEditorLabelWidth);
                            if (EditorGUI.EndChangeCheck() && s3dAAIsInvertProp.boolValue && s3dAAIsToggleProp.boolValue)
                            {
                                s3dAAIsToggleProp.boolValue = false;
                            }

                            if (!s3dAAIsInvertProp.boolValue)
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAAIsToggleProp, aaIsToggleContent, defaultEditorLabelWidth);
                            }

                            if (!s3dAAIsToggleProp.boolValue)
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("isResetCustomAfterUse"), aaIsResetCustomAfterUseContent, defaultEditorLabelWidth);
                            }
                            GUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(10f);
                            EditorGUILayout.LabelField(aaBoolValueContent, GUILayout.Width(defaultEditorLabelWidth - 14f));
                            EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                            GUILayout.EndHorizontal();
                        }
                        else if (!s3dAAIsExitActionProp.boolValue)
                        {
                            s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponBoolValue");
                            if (s3dAAValueProp.intValue != S3DAnimAction.ActionBoolValueFixedInt)
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAAIsInvertProp, aaIsInvertContent, defaultEditorLabelWidth);
                            }
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAAValueProp, aaBoolValueContent, defaultEditorLabelWidth);

                            // Is this a fixed bool value the user is sending to the animation controller?
                            if (s3dAAValueProp.intValue == S3DAnimAction.ActionBoolValueFixedInt)
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("fixedBoolValue"), aaBoolFixedValueContent, defaultEditorLabelWidth);
                            }
                        }
                    }
                    #endregion

                    #region Trigger
                    else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeTriggerInt)
                    {
                        if (IsCustomAction(s3dAAWeaponActionProp))
                        {
                            GUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(10f);
                            EditorGUILayout.LabelField(aaTriggerValueContent, GUILayout.Width(defaultEditorLabelWidth - 14f));
                            EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                            GUILayout.EndHorizontal();
                        }
                        else if (!s3dAAIsExitActionProp.boolValue)
                        {
                            s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponTriggerValue");
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAAValueProp, aaTriggerValueContent, defaultEditorLabelWidth);
                        }
                    }
                    #endregion

                    #region Float
                    else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeFloatInt)
                    {
                        if (IsCustomAction(s3dAAWeaponActionProp))
                        {
                            GUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(10f);
                            EditorGUILayout.LabelField(aaFloatValueContent, GUILayout.Width(defaultEditorLabelWidth - 14f));
                            EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                            GUILayout.EndHorizontal();
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent, defaultEditorLabelWidth);
                        }
                        else if (!s3dAAIsExitActionProp.boolValue)
                        {
                            s3dAAValueProp = s3dAnimActionProp.FindPropertyRelative("actionWeaponFloatValue");
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAAValueProp, aaFloatValueContent, defaultEditorLabelWidth);

                            // Is this a fixed float value the user is sending to the animation controller?
                            if (s3dAAValueProp.intValue == S3DAnimAction.ActionFloatValueFixedInt)
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("fixedFloatValue"), aaFloatFixedValueContent, defaultEditorLabelWidth);
                            }
                            else
                            {
                                StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("floatMultiplier"), aaFloatMultiplierContent, defaultEditorLabelWidth);
                            }
                            StickyEditorHelper.DrawPropertyIndent(10f, s3dAnimActionProp.FindPropertyRelative("damping"), aaDampingContent, defaultEditorLabelWidth);
                        }

                    }
                    #endregion

                    #region Integer
                    else if (s3dAAParamTypeProp.intValue == S3DAnimAction.ParameterTypeIntegerInt)
                    {
                        if (IsCustomAction(s3dAAWeaponActionProp))
                        {
                            GUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(10f);
                            EditorGUILayout.LabelField(aaIntegerValueContent, GUILayout.Width(defaultEditorLabelWidth - 14f));
                            EditorGUILayout.LabelField(aaCustomValueContent, GUILayout.MinWidth(defaultEditorFieldWidth - 8f));
                            GUILayout.EndHorizontal();
                        }
                        else if (!s3dAAIsExitActionProp.boolValue)
                        {
                            // Currently we don't have any integer realtime values in weapons for S3D
                            EditorGUILayout.BeginHorizontal();
                            StickyEditorHelper.DrawLabelIndent(12f, new GUIContent("None available"), defaultEditorLabelWidth);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    #endregion
                }
                GUILayout.EndVertical();
            }

            #endregion

            #region Delete/Move Anim Action
            if (s3dAnimActionDeletePos >= 0 || s3dAnimActionMoveDownPos >= 0)
            {
                GUI.FocusControl(null);
                // Don't permit multiple operations in the same pass
                if (s3dAnimActionMoveDownPos >= 0)
                {
                    // Move down one position, or wrap round to start of list
                    if (s3dAnimActionMoveDownPos < s3dAAListProp.arraySize - 1)
                    {
                        s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, s3dAnimActionMoveDownPos + 1);
                    }
                    else { s3dAAListProp.MoveArrayElement(s3dAnimActionMoveDownPos, 0); }

                    s3dAnimActionMoveDownPos = -1;
                }
                else if (s3dAnimActionDeletePos >= 0)
                {
                    s3dAAListProp.DeleteArrayElementAtIndex(s3dAnimActionDeletePos);
                    s3dAnimActionDeletePos = -1;
                }

                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dWeaponAnimSet);
                }
                GUIUtility.ExitGUI();
            }
            #endregion
        }

        /// <summary>
        /// Draw the list of animation clip pairs in the inspector
        /// </summary>
        private void DrawClipPairs()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            #region Check if animation clip pair List is null
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (s3dWeaponAnimSet.animClipPairList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dWeaponAnimSet.animClipPairList = new List<S3DAnimClipPair>(5);
                EditorUtility.SetDirty(s3dWeaponAnimSet);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove Anim Clip Pairs
            s3dClipPairDeletePos = -1;
            int numAnimClipPairs = animClipPairListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anim Clip Pairs", GUILayout.Width(100f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numAnimClipPairs < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dWeaponAnimSet, "Add Anim Clip Pair");
                s3dWeaponAnimSet.animClipPairList.Add(new S3DAnimClipPair());
                EditorUtility.SetDirty(s3dWeaponAnimSet);

                // Read in the properties
                serializedObject.Update();

                numAnimClipPairs = animClipPairListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numAnimClipPairs > 0) { s3dClipPairDeletePos = animClipPairListProp.arraySize - 1; }
            }

            if (GUILayout.Button(reverseContent, buttonCompact, GUILayout.Width(40f)))
            {
                for (int clipPairIdx = 0; clipPairIdx < numAnimClipPairs; clipPairIdx++)
                {
                    animClipPairProp = animClipPairListProp.GetArrayElementAtIndex(clipPairIdx);
                    if (animClipPairProp != null)
                    {
                        originalClipProp = animClipPairProp.FindPropertyRelative("originalClip");
                        replacementClipProp = animClipPairProp.FindPropertyRelative("replacementClip");

                        AnimationClip originalClip = (AnimationClip)originalClipProp.objectReferenceValue;

                        originalClipProp.objectReferenceValue = replacementClipProp.objectReferenceValue;
                        replacementClipProp.objectReferenceValue = originalClip;
                    }
                }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Anim Clip Pair List

            GUILayout.BeginHorizontal();
            float headerWidth = (defaultEditorLabelWidth + defaultEditorFieldWidth) * 0.5f;
            EditorGUILayout.LabelField("Num", GUILayout.Width(30f));
            EditorGUILayout.LabelField("Original", GUILayout.MinWidth(headerWidth));
            EditorGUILayout.LabelField("Replacement");
            GUILayout.EndHorizontal();

            for (int clipPairIdx = 0; clipPairIdx < numAnimClipPairs; clipPairIdx++)
            {
                animClipPairProp = animClipPairListProp.GetArrayElementAtIndex(clipPairIdx);
                if (animClipPairProp != null)
                {
                    GUILayout.BeginHorizontal();
                    //StickyEditorHelper.DrawLabelIndent(8f);
                    EditorGUILayout.LabelField((clipPairIdx + 1).ToString("00") + ".", GUILayout.Width(30f));

                    EditorGUILayout.PropertyField(animClipPairProp.FindPropertyRelative("originalClip"), GUIContent.none, GUILayout.MinWidth(headerWidth));
                    EditorGUILayout.PropertyField(animClipPairProp.FindPropertyRelative("replacementClip"), GUIContent.none);

                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dClipPairDeletePos = clipPairIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete Anim Clip Pair
            if (s3dClipPairDeletePos >= 0)
            {
                animClipPairListProp.DeleteArrayElementAtIndex(s3dClipPairDeletePos);
                s3dClipPairDeletePos = -1;

                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dWeaponAnimSet);
                }
                GUIUtility.ExitGUI();
            }
            #endregion
        }

        /// <summary>
        /// Draw the weapon override options in the inspector
        /// </summary>
        private void DrawWeaponSettingsOverride()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            EditorGUILayout.PropertyField(isWeaponSettingsOverrideProp, isWeaponSettingsOverrideContent);

            if (isWeaponSettingsOverrideProp.boolValue)
            {
                EditorGUILayout.PropertyField(isOnlyFireWhenAiming1Prop, isOnlyFireWhenAiming1Content);
                EditorGUILayout.PropertyField(isOnlyFireWhenAiming2Prop, isOnlyFireWhenAiming2Content);
            }
        }

        /// <summary>
        /// Is this a Custom "weapon" action? If so, it will get it's value from code.
        /// See stickyWeapon.AnimateCharacterCustomAction(..)
        /// </summary>
        /// <param name="weaponActionProp"></param>
        /// <returns></returns>
        protected bool IsCustomAction(SerializedProperty weaponActionProp)
        {
            return weaponActionProp.intValue == (int)S3DAnimAction.WeaponAction.Custom;
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

                miniLabelWrappedText = new GUIStyle(EditorStyles.miniLabel);
                miniLabelWrappedText.richText = true;
                miniLabelWrappedText.wordWrap = true;

                headingFieldRichText = new GUIStyle(EditorStyles.miniLabel);
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
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField(titleContent);
            EditorGUILayout.LabelField(headerContent, miniLabelWrappedText);
            GUILayout.EndVertical();
            #endregion

            // Read in all the properties
            serializedObject.Update();

            GUILayout.BeginVertical("HelpBox");

            #region General Settings
            EditorGUILayout.PropertyField(isSkipParmVerifyProp, isSkipParmVerifyContent);

            EditorGUILayout.PropertyField(isAppliedToAllCharactersProp, isAppliedToAllCharactersContent);
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            StickyEditorHelper.DrawArray(stickyModelIDsProp, isModelsExpandedInEditorProp, modelIDsContent, 80f, modelElementName, buttonCompact, foldoutStyleNoLabel, defaultEditorFieldWidth);
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(isAimIKWhenNotAimingProp, isAimIKWhenNotAimingContent);
            EditorGUILayout.PropertyField(aimIKTurnDelayProp, aimIKTurnDelayContent);
            EditorGUILayout.PropertyField(aimIKFPWeaponOffsetProp, aimIKFPWeaponOffsetContent);
            EditorGUILayout.PropertyField(aimIKFPNearClippingPlaneProp, aimIKFPNearClippingPlaneContent);
            EditorGUILayout.PropertyField(animClipPairRevertDelayProp, animClipPairRevertDelayContent);
            EditorGUILayout.PropertyField(isFreeLookWhenHeldProp, isFreeLookWhenHeldContent);
            EditorGUILayout.PropertyField(heldTransitionDurationProp, heldTransitionDurationContent);
            EditorGUILayout.PropertyField(isAimTPUsingFPCameraProp, isAimTPUsingFPCameraContent);
            #endregion

            DrawWeaponSettingsOverride();
            DrawClipPairs();
            DrawAnimActions();

            serializedObject.ApplyModifiedProperties();

            StickyEditorHelper.DrawHorizontalGap(6f);

            if (GUILayout.Button(resetContent, GUILayout.Width(120f)))
            {
                Undo.RecordObject(s3dWeaponAnimSet, "Reset Weapon Anim Set");
                s3dWeaponAnimSet.ResetIdentifiers();
                EditorUtility.SetDirty(s3dWeaponAnimSet);
            }

            GUILayout.EndVertical();  
        }

        #endregion
    }
}