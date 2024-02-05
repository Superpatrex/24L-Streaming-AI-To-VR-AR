using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The inspector editor for scriptable object array of S3DMagType
    /// </summary>
    [CustomEditor(typeof(S3DMagTypes))]
    public class S3DMagTypesEditor : Editor
    {
        #region Editor Variables
        private Color separatorColor = new Color();
        private bool isStylesInitialised = false;
        private GUIStyle labelFieldRichText;
        private GUIStyle miniLabelWrappedText;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Mag Types");
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of 26 magazine (clip) types. Magazines of the same type will fit the same weapons.");
        #endregion

        #region GUIContent
        private readonly static GUIContent nameContent = new GUIContent("Type Name");
        #endregion

        #region Serialized Properties
        private SerializedProperty magTypesProp;
        private SerializedProperty magProp;
        private SerializedProperty magTypeProp;
        private SerializedProperty magTypeNameProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            magTypesProp = serializedObject.FindProperty("magTypes");

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

            EditorGUI.indentLevel++;

            for (int arrayIdx = 0; arrayIdx < magTypesProp.arraySize; arrayIdx++)
            {
                magProp = magTypesProp.GetArrayElementAtIndex(arrayIdx);

                magTypeProp = magProp.FindPropertyRelative("magType");
                magTypeNameProp = magProp.FindPropertyRelative("magTypeName");

                EditorGUILayout.LabelField("Type " + (S3DMagType.MagType)magTypeProp.intValue, GUILayout.Width(80f));

                EditorGUILayout.PropertyField(magTypeNameProp, nameContent);

                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
            }
            EditorGUI.indentLevel--;

            GUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();

        }
        #endregion
    }
}