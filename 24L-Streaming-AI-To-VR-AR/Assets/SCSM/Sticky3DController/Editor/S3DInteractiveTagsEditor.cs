using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(S3DInteractiveTags))]
    public class S3DInteractiveTagsEditor : Editor
    {
        #region Editor Variables
        private Color separatorColor = new Color();
        private bool isStylesInitialised = false;
        private GUIStyle labelFieldRichText;
        private GUIStyle miniLabelWrappedText;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Interactive Tags");
        private readonly static GUIContent headerContent = new GUIContent("Contains an array of 32 common interactive object tags. Typically, only one set is required for all interactive objects. They are used to determine compatibility with things like StickySockets or character Equip Points. The text is only used to help you remember what each number from 1-32 represent. Start from top and do not leave gaps between tags. If there are gaps or the array is re-ordered, items in your project will need to be updated.");
        #endregion

        #region Serialized Properties
        private SerializedProperty interactiveTagsProp;
        private SerializedProperty interactiveTagProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            interactiveTagsProp = serializedObject.FindProperty("tags");

            isStylesInitialised = false;
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Configure Buttons and Styles

            // Set up rich text GUIStyles
            if (!isStylesInitialised)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;

                miniLabelWrappedText = new GUIStyle(EditorStyles.miniLabel);
                miniLabelWrappedText.richText = true;
                miniLabelWrappedText.wordWrap = true;

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

            #region General Settings
            GUILayout.BeginVertical("HelpBox");

            // Always draw the default tag
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tag 01", GUILayout.Width(80f));
            EditorGUILayout.LabelField("Default");
            GUILayout.EndHorizontal();

            for (int arrayIdx = 1; arrayIdx < interactiveTagsProp.arraySize; arrayIdx++)
            {
                interactiveTagProp = interactiveTagsProp.GetArrayElementAtIndex(arrayIdx);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tag " + (arrayIdx+1).ToString("00"), GUILayout.Width(80f));
                EditorGUILayout.PropertyField(interactiveTagProp, GUIContent.none);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}