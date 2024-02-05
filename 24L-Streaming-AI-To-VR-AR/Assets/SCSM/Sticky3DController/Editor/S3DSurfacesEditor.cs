using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(S3DSurfaces))]
    public class S3DSurfacesEditor : Editor
    {
        #region Custom Editor private variables
        private S3DSurfaces s3dSurfaces = null;
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

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of Surface Types typically used with character footsteps. Add Sticky Surface components to colliders to set their surface type.");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent damageDecalsContent = new GUIContent("Damage Decals", "A list of randomly selected decals that are used when the surface is hit by a projectile");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty surfaceTypeListProp;
        private SerializedProperty surfaceTypeProp;
        private SerializedProperty damageDecalsProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            s3dSurfaces = (S3DSurfaces)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            //// Used in Richtext labels
            //if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            //// Keep compiler happy - can remove this later if it isn't required
            //if (defaultTextColour.a > 0f) { }

            #region Find Properties - General
            surfaceTypeListProp = serializedObject.FindProperty("surfaceTypeList");
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
            if (s3dSurfaces.surfaceTypeList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dSurfaces.surfaceTypeList = new List<S3DSurfaceType>(10);
                EditorUtility.SetDirty(s3dSurfaces);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove types
            s3dDeletePos = -1;
            int numSurfaceTypes = surfaceTypeListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Surface Types", GUILayout.Width(100f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numSurfaceTypes < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dSurfaces, "Add Surface Type");
                s3dSurfaces.surfaceTypeList.Add(new S3DSurfaceType());
                EditorUtility.SetDirty(s3dSurfaces);

                // Read in the properties
                serializedObject.Update();

                numSurfaceTypes = surfaceTypeListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numSurfaceTypes > 0) { s3dDeletePos = surfaceTypeListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Surface Type List

            for (int stypeIdx = 0; stypeIdx < numSurfaceTypes; stypeIdx++)
            {
                surfaceTypeProp = surfaceTypeListProp.GetArrayElementAtIndex(stypeIdx);
                if (surfaceTypeProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (stypeIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUILayout.PropertyField(surfaceTypeProp.FindPropertyRelative("surfaceName"), GUIContent.none);

                    //EditorGUILayout.PropertyField(surfaceTypeProp, GUIContent.none);
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDeletePos = stypeIdx; }
                    GUILayout.EndHorizontal();

                    damageDecalsProp = surfaceTypeProp.FindPropertyRelative("damageDecals");

                    GUILayout.BeginHorizontal();
                    StickyEditorHelper.DrawLabelIndent(27f, damageDecalsContent, 100f);
                    EditorGUILayout.PropertyField(damageDecalsProp, GUIContent.none);
                    //StickyEditorHelper.DrawPropertyIndent(27f, damageDecalsProp, damageDecalsContent, defaultEditorLabelWidth);
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete Surface Type
            if (s3dDeletePos >= 0)
            {
                surfaceTypeListProp.DeleteArrayElementAtIndex(s3dDeletePos);
                s3dDeletePos = -1;

                #if UNITY_2019_3_OR_NEWER
                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dSurfaces);
                }
                GUIUtility.ExitGUI();
                #endif
            }
            #endregion

            //EditorGUILayout.PropertyField(property: surfaceTypeListProp, includeChildren: true); //, GUILayoutOption: surfaceTypeListContent);

            GUILayout.EndVertical();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}