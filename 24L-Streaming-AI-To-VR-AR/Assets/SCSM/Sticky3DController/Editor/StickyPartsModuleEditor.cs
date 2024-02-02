using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyPartsModule))]
    public class StickyPartsModuleEditor : Editor
    {
        #region Custom Editor private variables
        private StickyPartsModule stickyPartsModule;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        private int s3dPartMoveDownPos = -1;
        private int s3dPartInsertPos = -1;
        private int s3dPartDeletePos = -1;
        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to configure individual parts on a character");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky Parts Module is enabled through code.");
        #endregion

        #region GUIContent - Parts
        private readonly static GUIContent partTransformContent = new GUIContent(" Part Transform", "The child transform on the character for this part");
        private readonly static GUIContent penableOnStartContent = new GUIContent(" Enable on Start", "Should the part be enabled (or disabled) when the module is initialised?");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty s3dPartListProp;
        private SerializedProperty isS3DPartListExpandedProp;
        private SerializedProperty s3dPartProp;
        private SerializedProperty s3dPShowInEditorProp;
        private SerializedProperty s3dPartTransformProp;
        private SerializedProperty s3dPenableOnStartProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            stickyPartsModule = (StickyPartsModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Used in Richtext labels
            //if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            //if (defaultTextColour.a > 0f) { }

            #region Find Properties - General
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            s3dPartListProp = serializedObject.FindProperty("s3dPartList");
            isS3DPartListExpandedProp = serializedObject.FindProperty("isS3DPartListExpanded");
            #endregion
        }

        #endregion

        #region Private Methods

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

                if (compType == typeof(S3DPart))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as S3DPart).showInEditor = isExpanded;
                    }
                }
            }
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

                isStylesInitialised = true;
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            #region General Settings
            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
            #endregion

            #region Parts
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            #region Check if s3dPartsList is null
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (stickyPartsModule.s3dPartList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                stickyPartsModule.s3dPartList = new List<S3DPart>(10);
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add-Remove S3DParts
            int numParts = s3dPartListProp.arraySize;

            // Reset button variables
            s3dPartMoveDownPos = -1;
            s3dPartInsertPos = -1;
            s3dPartDeletePos = -1;

            GUILayout.BeginHorizontal();

            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            EditorGUI.BeginChangeCheck();
            isS3DPartListExpandedProp.boolValue = EditorGUILayout.Foldout(isS3DPartListExpandedProp.boolValue, "", foldoutStyleNoLabel);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ExpandList(stickyPartsModule.s3dPartList, isS3DPartListExpandedProp.boolValue);
                // Read in the properties
                serializedObject.Update();
            }
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            EditorGUILayout.LabelField("Parts: " + numParts.ToString("00"), labelFieldRichText);

            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(stickyPartsModule, "Add Part");
                stickyPartsModule.s3dPartList.Add(new S3DPart());
                ExpandList(stickyPartsModule.s3dPartList, false);
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();

                numParts = s3dPartListProp.arraySize;
                if (numParts > 0)
                {
                    // Force new AnimAction to be serialized in scene
                    s3dPartProp = s3dPartListProp.GetArrayElementAtIndex(numParts - 1);
                    s3dPShowInEditorProp = s3dPartProp.FindPropertyRelative("showInEditor");
                    s3dPShowInEditorProp.boolValue = !s3dPShowInEditorProp.boolValue;
                    // Show the new Part
                    s3dPShowInEditorProp.boolValue = true;
                }
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
            {
                if (numParts > 0) { s3dPartDeletePos = s3dPartListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();
            #endregion

            #region Parts List
            numParts = s3dPartListProp.arraySize;

            for (int ptIdx = 0; ptIdx < numParts; ptIdx++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                s3dPartProp = s3dPartListProp.GetArrayElementAtIndex(ptIdx);

                #region Get Properties for the Part
                s3dPShowInEditorProp = s3dPartProp.FindPropertyRelative("showInEditor");
                s3dPartTransformProp = s3dPartProp.FindPropertyRelative("partTransform");
                #endregion

                #region Part Move/Insert/Delete buttons
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel += 1;
                s3dPShowInEditorProp.boolValue = EditorGUILayout.Foldout(s3dPShowInEditorProp.boolValue, "Part " + (ptIdx + 1).ToString("00") + (!s3dPShowInEditorProp.boolValue && s3dPartTransformProp.objectReferenceValue != null ? " - " + s3dPartTransformProp.objectReferenceValue.name : ""));
                EditorGUI.indentLevel -= 1;

                // Move down button
                if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numParts > 1) { s3dPartMoveDownPos = ptIdx; }
                // Create duplicate button
                if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { s3dPartInsertPos = ptIdx; }
                // Delete button
                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dPartDeletePos = ptIdx; }
                GUILayout.EndHorizontal();
                #endregion

                if (s3dPShowInEditorProp.boolValue)
                {
                    #region General Part Properties
                    
                    s3dPenableOnStartProp = s3dPartProp.FindPropertyRelative("enableOnStart");

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(s3dPartTransformProp, partTransformContent);
                    if (EditorGUI.EndChangeCheck() && s3dPartTransformProp.objectReferenceValue != null)
                    {
                        if (!((Transform)s3dPartTransformProp.objectReferenceValue).IsChildOf(stickyPartsModule.transform))
                        {
                            s3dPartTransformProp.objectReferenceValue = null;
                            Debug.LogWarning("The part transform must be a child of the parent Sticky3D Parts Module gameobject or part of the prefab.");
                        }
                    }

                    EditorGUILayout.PropertyField(s3dPenableOnStartProp, penableOnStartContent);

                    #endregion
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Move/Insert/Delete S3DPart
            if (s3dPartDeletePos >= 0 || s3dPartInsertPos >= 0 || s3dPartMoveDownPos >= 0)
            {
                GUI.FocusControl(null);
                // Don't permit multiple operations in the same pass
                if (s3dPartMoveDownPos >= 0)
                {
                    // Move down one position, or wrap round to start of list
                    if (s3dPartMoveDownPos < s3dPartListProp.arraySize - 1)
                    {
                        s3dPartListProp.MoveArrayElement(s3dPartMoveDownPos, s3dPartMoveDownPos + 1);
                    }
                    else { s3dPartListProp.MoveArrayElement(s3dPartMoveDownPos, 0); }

                    s3dPartMoveDownPos = -1;
                }
                else if (s3dPartInsertPos >= 0)
                {
                    // NOTE: Undo doesn't work with Insert.

                    // Apply property changes before potential list changes
                    serializedObject.ApplyModifiedProperties();

                    S3DPart insertedPart = new S3DPart(stickyPartsModule.s3dPartList[s3dPartInsertPos]);
                    insertedPart.showInEditor = true;
                    // Generate a new hashcode for the duplicated Part
                    insertedPart.guidHash = S3DMath.GetHashCodeFromGuid();

                    stickyPartsModule.s3dPartList.Insert(s3dPartInsertPos, insertedPart);

                    // Read all properties from the Sticky Parts Module
                    serializedObject.Update();

                    // Hide original Part
                    s3dPartListProp.GetArrayElementAtIndex(s3dPartInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                    s3dPShowInEditorProp = s3dPartListProp.GetArrayElementAtIndex(s3dPartInsertPos).FindPropertyRelative("showInEditor");

                    // Force new action to be serialized in scene
                    s3dPShowInEditorProp.boolValue = !s3dPShowInEditorProp.boolValue;

                    // Show inserted duplicate Part
                    s3dPShowInEditorProp.boolValue = true;

                    s3dPartInsertPos = -1;

                    isSceneModified = true;
                }
                else if (s3dPartDeletePos >= 0)
                {
                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                    int _deleteIndex = s3dPartDeletePos;

                    if (EditorUtility.DisplayDialog("Delete Part " + (s3dPartDeletePos + 1) + "?", "Part " + (s3dPartDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Part from the list and cannot be undone.", "Delete Now", "Cancel"))
                    {
                        s3dPartListProp.DeleteArrayElementAtIndex(_deleteIndex);
                        s3dPartDeletePos = -1;
                    }
                }

                #if UNITY_2019_3_OR_NEWER
                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                GUIUtility.ExitGUI();
                #endif
            }
            #endregion

            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion
        }

        #endregion
    }
}