using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The inspector editor for scriptable object list of effects modules.
    /// </summary>
    [CustomEditor(typeof(S3DEffectsSet))]
    public class S3DEffectsSetEditor : Editor
    {
        #region Custom Editor private variables
        private S3DEffectsSet s3dEffectsSet = null;
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

        private int s3dDeletePos = -1;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of Sticky Effects Modules typically used when you want a collection of similar particle and/or Sound FX");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty effectsModuleListProp;
        private SerializedProperty stickyEffectsModuleProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            s3dEffectsSet = (S3DEffectsSet)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties - General
            effectsModuleListProp = serializedObject.FindProperty("effectsModuleList");
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
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region General Settings
            GUILayout.BeginVertical("HelpBox");

            #region Check if type List is null
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (s3dEffectsSet.effectsModuleList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dEffectsSet.effectsModuleList = new List<StickyEffectsModule>(10);
                EditorUtility.SetDirty(s3dEffectsSet);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove types
            s3dDeletePos = -1;
            int numEffects = effectsModuleListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sticky Effects Modules", GUILayout.Width(140f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numEffects < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dEffectsSet, "Add Effects Slot");
                s3dEffectsSet.effectsModuleList.Add(null);
                EditorUtility.SetDirty(s3dEffectsSet);

                // Read in the properties
                serializedObject.Update();

                numEffects = effectsModuleListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numEffects > 0) { s3dDeletePos = effectsModuleListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region StickyEffectsModule List

            for (int stypeIdx = 0; stypeIdx < numEffects; stypeIdx++)
            {
                stickyEffectsModuleProp = effectsModuleListProp.GetArrayElementAtIndex(stypeIdx);
                if (stickyEffectsModuleProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (stypeIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUILayout.PropertyField(stickyEffectsModuleProp, GUIContent.none);

                    //EditorGUILayout.PropertyField(surfaceTypeProp, GUIContent.none);
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDeletePos = stypeIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete StickyEffectsModule
            if (s3dDeletePos >= 0)
            {
                effectsModuleListProp.DeleteArrayElementAtIndex(s3dDeletePos);
                s3dDeletePos = -1;

                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dEffectsSet);
                }
                GUIUtility.ExitGUI();
            }
            #endregion

            GUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}