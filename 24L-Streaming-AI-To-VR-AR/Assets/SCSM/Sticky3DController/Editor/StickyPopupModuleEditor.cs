using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyPopupModule class
    /// </summary>
    [CustomEditor(typeof(StickyPopupModule))]
    public class StickyPopupModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyPopupModule stickyPopupModule = null;

        private int s3dMsgMoveDownPos = -1;
        private int s3dMsgInsertPos = -1;
        private int s3dMsgDeletePos = -1;

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A popup canvas-based selectable menu that uses the Sticky Manager's pooling system.");
        #endregion

        #region GUIContent - Popup
        private readonly static GUIContent isPauseWeaponsFiringContent = new GUIContent(" Pause Weapons Firing", "If a character is assigned to the popup, any held weapons will not fire while the popup is shown.");
        private readonly static GUIContent unPauseWeaponsFiringDelayContent = new GUIContent(" Unpause Weapons Delay", "When the popup is closed and it was initiated from a character (directly or indirectly), delaying the unpausing of weapons by a small amount can help to prevent unintentional weapon firing. Smaller delays are typically better. Try 0.1 seconds to start with and increase if required.");
        private readonly static GUIContent useButtonColourForTextContent = new GUIContent(" Button Colour For Text", "This can be useful when button has no background image and the Text should change colour based on button transition tint colours");
        private readonly static GUIContent isMessagesExpandedContent = new GUIContent(" Messages");

        private readonly static GUIContent pmMessageNameSettingsContent = new GUIContent(" Message Name", "The name or description of the message. This can be used to identify the message. It is not displayed in the popup.");
        private readonly static GUIContent pmMessageLabelStringContent = new GUIContent(" Message Label Text", "The text to display in the message label. It can include RichText markup. e.g., <b>Bold Text</b>");
        private readonly static GUIContent pmMessageValueStringContent = new GUIContent(" Message Value Text", "The text to display in the message. It can include RichText markup. e.g., <b>Bold Text</b>");
        private readonly static GUIContent pmShowMessageSettingsContent = new GUIContent(" Show Message", "Show the message on the popup. [Has no effect outside play mode]");
        private readonly static GUIContent pmMessagePanelContent = new GUIContent(" Message Panel", "The UI Panel that holds the label and value RectTransforms");
        private readonly static GUIContent pmMessageLabelPanelContent = new GUIContent(" Message Label Panel", " The UI Panel for the message label");
        private readonly static GUIContent pmMessageValuePanelContent = new GUIContent(" Message Value Panel", " The UI Panel for the message value text");

        #endregion

        #region Serialized Properties - Popup
        private SerializedProperty isPauseWeaponsFiringProp;
        private SerializedProperty unPauseWeaponsFiringDelayProp;
        private SerializedProperty useButtonColourForTextProp;
        private SerializedProperty isMessagesExpandedProp;
        private SerializedProperty popupMessageListProp;
        private SerializedProperty popupMessageProp;
        private SerializedProperty pMsgShowInEditorProp;
        private SerializedProperty pMsgShowMessageProp;
        private SerializedProperty pMsgMessageNameProp;
        private SerializedProperty pMsgMessageLabelStringProp;
        private SerializedProperty pMsgMessageValueStringProp;
        private SerializedProperty pMsgMessagePanelProp;
        private SerializedProperty pMsgLabelPanelProp;
        private SerializedProperty pMsgValuePanelProp;
        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyPopupModule = (StickyPopupModule)target;

            #region Find Properties - Popup
            useButtonColourForTextProp = serializedObject.FindProperty("useButtonColourForText");
            isPauseWeaponsFiringProp = serializedObject.FindProperty("isPauseWeaponsFiring");
            unPauseWeaponsFiringDelayProp = serializedObject.FindProperty("unPauseWeaponsFiringDelay");
            isMessagesExpandedProp = serializedObject.FindProperty("isMessagesExpanded");
            popupMessageListProp = serializedObject.FindProperty("popupMessageList");
            #endregion
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Draw the general popup settings in the inspector
        /// </summary>
        protected void DrawPopupGeneralSettings()
        {
            EditorGUILayout.PropertyField(useButtonColourForTextProp, useButtonColourForTextContent);
            EditorGUILayout.PropertyField(isPauseWeaponsFiringProp, isPauseWeaponsFiringContent);
            EditorGUILayout.PropertyField(unPauseWeaponsFiringDelayProp, unPauseWeaponsFiringDelayContent);
        }

        /// <summary>
        /// Draw settings for a message in the inspector
        /// </summary>
        /// <param name="messageIndex"></param>
        /// <param name="numberOfMessages"></param>
        protected void DrawMessage(int messageIndex, int numberOfMessages)
        {
            EditorGUILayout.BeginVertical("HelpBox");

            popupMessageProp = popupMessageListProp.GetArrayElementAtIndex(messageIndex);

            #region Find Display Message Properties
            pMsgShowInEditorProp = popupMessageProp.FindPropertyRelative("showInEditor");
            pMsgShowMessageProp = popupMessageProp.FindPropertyRelative("showMessage");
            pMsgMessageNameProp = popupMessageProp.FindPropertyRelative("messageName");
            pMsgMessageLabelStringProp = popupMessageProp.FindPropertyRelative("messageLabelString");
            pMsgMessageValueStringProp = popupMessageProp.FindPropertyRelative("messageValueString");
            pMsgMessagePanelProp = popupMessageProp.FindPropertyRelative("messagePanel");
            pMsgLabelPanelProp = popupMessageProp.FindPropertyRelative("labelPanel");
            pMsgValuePanelProp = popupMessageProp.FindPropertyRelative("valuePanel");

            #endregion


            GUILayout.BeginHorizontal();

            StickyEditorHelper.DrawS3DFoldout(pMsgShowInEditorProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
            EditorGUILayout.LabelField((messageIndex + 1).ToString("00 ") + pMsgMessageNameProp.stringValue);
            // Move down button
            if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numberOfMessages > 1) { s3dMsgMoveDownPos = messageIndex; }
            // Create duplicate button
            if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dMsgInsertPos = messageIndex; }
            // Delete button
            if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dMsgDeletePos = messageIndex; }
            GUILayout.EndHorizontal();

            if (pMsgShowInEditorProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMsgShowMessageProp, pmShowMessageSettingsContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (pMsgShowMessageProp.boolValue) { stickyPopupModule.ShowMessage(stickyPopupModule.popupMessageList[messageIndex]); }
                    else { stickyPopupModule.HideMessage(stickyPopupModule.popupMessageList[messageIndex]); }
                    serializedObject.Update();
                }

                // String fields
                EditorGUILayout.PropertyField(pMsgMessageNameProp, pmMessageNameSettingsContent);
                EditorGUILayout.PropertyField(pMsgMessageLabelStringProp, pmMessageLabelStringContent);
                EditorGUILayout.PropertyField(pMsgMessageValueStringProp, pmMessageValueStringContent);

                // RectTransform panels
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMsgMessagePanelProp, pmMessagePanelContent);
                if (EditorGUI.EndChangeCheck() && pMsgMessagePanelProp.objectReferenceValue != null)
                {
                    if (!((RectTransform)pMsgMessagePanelProp.objectReferenceValue).transform.IsChildOf(stickyPopupModule.transform))
                    {
                        pMsgMessagePanelProp.objectReferenceValue = null;
                        Debug.LogWarning("The Message Panel (" + pMsgMessageNameProp.stringValue + ") must be a child of the StickyPopupModule gameobject or part of the prefab on " + stickyPopupModule.name);
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMsgLabelPanelProp, pmMessageLabelPanelContent);
                if (EditorGUI.EndChangeCheck() && pMsgLabelPanelProp.objectReferenceValue != null)
                {
                    if (!((RectTransform)pMsgLabelPanelProp.objectReferenceValue).transform.IsChildOf(stickyPopupModule.transform))
                    {
                        pMsgLabelPanelProp.objectReferenceValue = null;
                        Debug.LogWarning("The Label Panel (" + pMsgMessageNameProp.stringValue + ") must be a child of the StickyPopupModule gameobject or part of the prefab on " + stickyPopupModule.name);
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMsgValuePanelProp, pmMessageValuePanelContent);
                if (EditorGUI.EndChangeCheck() && pMsgValuePanelProp.objectReferenceValue != null)
                {
                    if (!((RectTransform)pMsgValuePanelProp.objectReferenceValue).transform.IsChildOf(stickyPopupModule.transform))
                    {
                        pMsgValuePanelProp.objectReferenceValue = null;
                        Debug.LogWarning("The Value Panel (" + pMsgMessageNameProp.stringValue + ") must be a child of the StickyPopupModule gameobject or part of the prefab on " + stickyPopupModule.name);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the list of messages in the inspector
        /// </summary>
        protected void DrawMessageSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            StickyEditorHelper.DrawS3DFoldout(isMessagesExpandedProp, isMessagesExpandedContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

            if (isMessagesExpandedProp.boolValue)
            {
                #region Add-Remove Messages

                int numMessages = popupMessageListProp.arraySize;

                // Reset button variables
                s3dMsgMoveDownPos = -1;
                s3dMsgInsertPos = -1;
                s3dMsgDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                isMessagesExpandedProp.boolValue = EditorGUILayout.Foldout(isMessagesExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(stickyPopupModule.popupMessageList, isMessagesExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("Messages: " + numMessages.ToString("00"), labelFieldRichText);

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    // Ensure we have a non-null list so undo works correctly.
                    if (stickyPopupModule.popupMessageList == null) { stickyPopupModule.popupMessageList = new List<S3DPopupMessage>(); }
                    Undo.RecordObject(stickyPopupModule, "Add Popup Message");
                    stickyPopupModule.AddMessage(new S3DPopupMessage());
                    ExpandList(stickyPopupModule.popupMessageList, false);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numMessages = popupMessageListProp.arraySize;
                    if (numMessages > 1)
                    {
                        // Force new Damage Region to be serialized in scene
                        popupMessageProp = popupMessageListProp.GetArrayElementAtIndex(numMessages - 1);
                        pMsgShowInEditorProp = popupMessageProp.FindPropertyRelative("showInEditor");
                        pMsgShowInEditorProp.boolValue = !pMsgShowInEditorProp.boolValue;
                        // Show the new Message
                        pMsgShowInEditorProp.boolValue = true;
                    }
                }

                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numMessages > 0) { s3dMsgDeletePos = popupMessageListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                for (int msgIdx = 0; msgIdx < numMessages; msgIdx++)
                {
                    DrawMessage(msgIdx, numMessages);
                }

                #region Move/Insert/Delete Messages

                if (s3dMsgDeletePos >= 0 || s3dMsgInsertPos >= 0 || s3dMsgMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (s3dMsgMoveDownPos >= 0)
                    {
                        if (popupMessageListProp.arraySize > 1)
                        {
                            // Move down one position, or wrap round to 1nd position in list
                            if (s3dMsgMoveDownPos < popupMessageListProp.arraySize - 1)
                            {
                                popupMessageListProp.MoveArrayElement(s3dMsgMoveDownPos, s3dMsgMoveDownPos + 1);
                            }
                            else { popupMessageListProp.MoveArrayElement(s3dMsgMoveDownPos, 0); }
                        }
                        s3dMsgMoveDownPos = -1;
                    }
                    else if (s3dMsgInsertPos >= 0)
                    {
                        // NOTE: Undo doesn't work with Insert.

                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        S3DPopupMessage insertedPopupMessage = new S3DPopupMessage(stickyPopupModule.popupMessageList[s3dMsgInsertPos]);
                        insertedPopupMessage.showInEditor = true;
                        // Generate a new hashcode for the duplicated message
                        insertedPopupMessage.guidHash = S3DMath.GetHashCodeFromGuid();

                        stickyPopupModule.popupMessageList.Insert(s3dMsgInsertPos, insertedPopupMessage);

                        // Read all properties from the poupup module
                        serializedObject.Update();

                        // Hide original message
                        popupMessageListProp.GetArrayElementAtIndex(s3dMsgInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        pMsgShowInEditorProp = popupMessageListProp.GetArrayElementAtIndex(s3dMsgInsertPos).FindPropertyRelative("showInEditor");

                        // Force new action to be serialized in scene
                        pMsgShowInEditorProp.boolValue = !pMsgShowInEditorProp.boolValue;

                        // Show inserted duplicate message
                        pMsgShowInEditorProp.boolValue = true;

                        s3dMsgInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (s3dMsgDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = s3dMsgDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Message " + (s3dMsgDeletePos + 1) + "?", "Message " + (s3dMsgDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Message from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            popupMessageListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            s3dMsgDeletePos = -1;
                        }
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Expand (show) or collapse (hide) all items in a list in the editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList<T>(List<T> componentList, bool isExpanded)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(S3DPopupMessage))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DPopupMessage).showInEditor = isExpanded;
                    }
                }
            }
        }

        #endregion

        #region DrawBaseInspector

        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyPopupModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            DrawBaseSettings();

            DrawPopupGeneralSettings();            

            DrawMessageSettings();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickyPopupModule.allowRepaint = true;
        }

        #endregion
    }
}