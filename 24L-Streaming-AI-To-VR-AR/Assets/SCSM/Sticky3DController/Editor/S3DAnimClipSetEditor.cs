using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(S3DAnimClipSet))]
    public class S3DAnimClipSetEditor : Editor
    {
        #region Custom Editor private variables
        private S3DAnimClipSet s3dAnimClipSet = null;
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
        private readonly static GUIContent titleContent = new GUIContent("Anim Clip Set");
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of replacement animation clips that can be used with Sticky Zones or the Sticky Anim Replacer component.");
        #endregion

        #region GUIContent - General
        private readonly static GUIContent reverseContent = new GUIContent("<->", "Switch the order of all the clips");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty animClipPairListProp;
        private SerializedProperty animClipPairProp;
        private SerializedProperty originalClipProp;
        private SerializedProperty replacementlClipProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            s3dAnimClipSet = (S3DAnimClipSet)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            //// Used in Richtext labels
            //if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            //// Keep compiler happy - can remove this later if it isn't required
            //if (defaultTextColour.a > 0f) { }

            #region Find Properties - General
            animClipPairListProp = serializedObject.FindProperty("animClipPairList");
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
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField(titleContent);
            EditorGUILayout.LabelField(headerContent, EditorStyles.miniLabel);
            GUILayout.EndVertical();
            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region General Settings
            GUILayout.BeginVertical("HelpBox");

            #region Check if type List is null
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (s3dAnimClipSet.animClipPairList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                s3dAnimClipSet.animClipPairList = new List<S3DAnimClipPair>(10);
                EditorUtility.SetDirty(s3dAnimClipSet);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove Anim Clip Pairs
            s3dDeletePos = -1;
            int numAnimClipPairs = animClipPairListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anim Clip Pairs", GUILayout.Width(100f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numAnimClipPairs < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(s3dAnimClipSet, "Add Anim Clip Pair");
                s3dAnimClipSet.animClipPairList.Add(new S3DAnimClipPair());
                EditorUtility.SetDirty(s3dAnimClipSet);

                // Read in the properties
                serializedObject.Update();

                numAnimClipPairs = animClipPairListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numAnimClipPairs > 0) { s3dDeletePos = animClipPairListProp.arraySize - 1; }
            }

            if (GUILayout.Button(reverseContent, buttonCompact, GUILayout.Width(40f)))
            {
                for (int clipPairIdx = 0; clipPairIdx < numAnimClipPairs; clipPairIdx++)
                {
                    animClipPairProp = animClipPairListProp.GetArrayElementAtIndex(clipPairIdx);
                    if (animClipPairProp != null)
                    {
                        originalClipProp = animClipPairProp.FindPropertyRelative("originalClip");
                        replacementlClipProp = animClipPairProp.FindPropertyRelative("replacementClip");

                        AnimationClip originalClip = (AnimationClip)originalClipProp.objectReferenceValue;

                        originalClipProp.objectReferenceValue = replacementlClipProp.objectReferenceValue;
                        replacementlClipProp.objectReferenceValue = originalClip;
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
                    EditorGUILayout.LabelField(" " + (clipPairIdx + 1).ToString("00") + ".", GUILayout.Width(30f));

                    EditorGUILayout.PropertyField(animClipPairProp.FindPropertyRelative("originalClip"), GUIContent.none, GUILayout.MinWidth(headerWidth));
                    EditorGUILayout.PropertyField(animClipPairProp.FindPropertyRelative("replacementClip"), GUIContent.none);

                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dDeletePos = clipPairIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            #region Delete Anim Clip Pair
            if (s3dDeletePos >= 0)
            {
                animClipPairListProp.DeleteArrayElementAtIndex(s3dDeletePos);
                s3dDeletePos = -1;

                #if UNITY_2019_3_OR_NEWER
                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(s3dAnimClipSet);
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