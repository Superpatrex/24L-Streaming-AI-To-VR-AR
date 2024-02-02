using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyAnimReplacer))]
    [CanEditMultipleObjects]
    public class StickyAnimReplacerEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables

        private StickyAnimReplacer stickyAnimReplacer = null;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        //private bool isRefreshing = false;
        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        // Anim Clip Sets
        private int s3dAnimClipSetDeletePos = -1;

        #endregion

        #region GUIContent
        private readonly static GUIContent headerContent = new GUIContent("This component enables you to override or replace animation clips on character at runtime.");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise On Start", "If enabled, the " +
            "Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the zone through code.");

        private readonly static GUIContent animClipSetsContent = new GUIContent("Animation Clip Sets", "One or more Anim Clip Set scriptable objects that contain original and replacement animation clip pairs.");

        #endregion

        #region Properties
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty s3dAnimClipSetListProp;
        private SerializedProperty s3dAnimClipSetProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            stickyAnimReplacer = (StickyAnimReplacer)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            s3dAnimClipSetListProp = serializedObject.FindProperty("s3dAnimClipSetList");

            #endregion
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

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            #endregion

            serializedObject.Update();

            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);

            #region Override Animation Clips

            #region Add or Remove Anim Clip Pairs
            s3dAnimClipSetDeletePos = -1;
            int numAnimClipSets = s3dAnimClipSetListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(animClipSetsContent, GUILayout.Width(140f));

            // Limit to 20 sets of pairs - not sure why anyone would need more
            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numAnimClipSets < 21)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(stickyAnimReplacer, "Add Anim Clip Set");
                // Add an empty slot
                stickyAnimReplacer.AddAnimClipSet(null);
                isSceneModified = true;

                // Read in the properties
                serializedObject.Update();

                numAnimClipSets = s3dAnimClipSetListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numAnimClipSets > 0) { s3dAnimClipSetDeletePos = s3dAnimClipSetListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();
            #endregion

            #region Anim Clip Set List

            for (int clipSetIdx = 0; clipSetIdx < numAnimClipSets; clipSetIdx++)
            {
                s3dAnimClipSetProp = s3dAnimClipSetListProp.GetArrayElementAtIndex(clipSetIdx);
                if (s3dAnimClipSetProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (clipSetIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUILayout.PropertyField(s3dAnimClipSetProp, GUIContent.none);

                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimClipSetDeletePos = clipSetIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete Anim Clip Set
            if (s3dAnimClipSetDeletePos >= 0)
            {
                s3dAnimClipSetListProp.DeleteArrayElementAtIndex(s3dAnimClipSetDeletePos);
                s3dAnimClipSetDeletePos = -1;

                #if !UNITY_2019_3_OR_NEWER
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
