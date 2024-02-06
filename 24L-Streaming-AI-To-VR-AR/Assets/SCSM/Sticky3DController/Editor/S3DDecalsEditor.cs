using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The inspector editor for scriptable object list of decal modules.
    /// </summary>
    [CustomEditor(typeof(S3DDecals))]
    public class S3DDecalsEditor : Editor
    {
        #region Custom Editor private variables
        private S3DDecals s3dDecals = null;
        private bool isStylesInitialised = false;
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

        private int s3dDeletePos = -1;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of Sticky Decal Modules typically used with projectile or beam module prefabs");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty decalModuleListProp;
        private SerializedProperty stickyDecalModuleProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            s3dDecals = (S3DDecals)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties - General
            decalModuleListProp = serializedObject.FindProperty("decalModuleList");
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
            if (s3dDecals.decalModuleList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dDecals.decalModuleList = new List<StickyDecalModule>(10);
                EditorUtility.SetDirty(s3dDecals);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove types
            s3dDeletePos = -1;
            int numDecals = decalModuleListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sticky Decal Modules", GUILayout.Width(140f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numDecals < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dDecals, "Add Decal");
                //s3dDecals.decalModuleList.Add(new StickyDecalModule());
                s3dDecals.decalModuleList.Add(null);
                EditorUtility.SetDirty(s3dDecals);

                // Read in the properties
                serializedObject.Update();

                numDecals = decalModuleListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numDecals > 0) { s3dDeletePos = decalModuleListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region StickyDecalModule List

            for (int stypeIdx = 0; stypeIdx < numDecals; stypeIdx++)
            {
                stickyDecalModuleProp = decalModuleListProp.GetArrayElementAtIndex(stypeIdx);
                if (stickyDecalModuleProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (stypeIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUILayout.PropertyField(stickyDecalModuleProp, GUIContent.none);

                    //EditorGUILayout.PropertyField(surfaceTypeProp, GUIContent.none);
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDeletePos = stypeIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete StickyDealModule
            if (s3dDeletePos >= 0)
            {
                decalModuleListProp.DeleteArrayElementAtIndex(s3dDeletePos);
                s3dDeletePos = -1;

                #if UNITY_2019_3_OR_NEWER
                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dDecals);
                }
                GUIUtility.ExitGUI();
                #endif
            }
            #endregion

            GUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}