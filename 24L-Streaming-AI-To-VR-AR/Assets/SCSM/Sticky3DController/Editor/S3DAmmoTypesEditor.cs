using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The inspector editor for scriptable object array of S3DAmmo
    /// </summary>
    [CustomEditor(typeof(S3DAmmoTypes))]
    public class S3DAmmoTypesEditor : Editor
    {
        #region Editor Variables
        private Color separatorColor = new Color();
        private bool isStylesInitialised = false;
        private GUIStyle labelFieldRichText;
        private GUIStyle miniLabelWrappedText;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Ammo Types");
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of 26 ammo types to fit weapons (and magazines)");
        #endregion

        #region GUIContent
        private readonly static GUIContent nameContent = new GUIContent("Type Name");
        private readonly static GUIContent damageMultiplierContent = new GUIContent("Damage Multiplier", "How much the Damage Amount of the projecile is multiplied when it hits an object or character. Default = 1 (no change).");
        private readonly static GUIContent impactMultiplierContent = new GUIContent("Impact Multiplier", "How much the force of impact is multiplied when the projectile hits an object or character. Default = 1 (no change).");
        #endregion

        #region Serialized Properties
        private SerializedProperty ammoTypesProp;
        private SerializedProperty ammoProp;
        private SerializedProperty ammoTypeProp;
        private SerializedProperty ammoNameProp;
        private SerializedProperty damageMultiplierProp;
        private SerializedProperty impactMultiplierProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            ammoTypesProp = serializedObject.FindProperty("ammoTypes");

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

            //StickyEditorHelper.DrawArray(serializedObject.FindProperty("ammoTypes"));

            EditorGUI.indentLevel++;

            for (int arrayIdx = 0; arrayIdx < ammoTypesProp.arraySize; arrayIdx++)
            {
                ammoProp = ammoTypesProp.GetArrayElementAtIndex(arrayIdx);

                ammoTypeProp = ammoProp.FindPropertyRelative("ammoType");
                ammoNameProp = ammoProp.FindPropertyRelative("ammoName");
                damageMultiplierProp = ammoProp.FindPropertyRelative("damageMultiplier");
                impactMultiplierProp = ammoProp.FindPropertyRelative("impactMultiplier");

                EditorGUILayout.LabelField("Type " + (S3DAmmo.AmmoType)ammoTypeProp.intValue, GUILayout.Width(80f));

                EditorGUILayout.PropertyField(ammoNameProp, nameContent);
                EditorGUILayout.PropertyField(damageMultiplierProp, damageMultiplierContent);
                EditorGUILayout.PropertyField(impactMultiplierProp, impactMultiplierContent);

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