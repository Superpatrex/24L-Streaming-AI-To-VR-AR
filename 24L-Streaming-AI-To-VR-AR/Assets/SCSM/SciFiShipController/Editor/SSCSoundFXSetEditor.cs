using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// The inspector editor for scriptable object list of effects modules.
    /// TOOD - Don't permit pooled and non-pooled in the same list
    /// </summary>
    [CustomEditor(typeof(SSCSoundFXSet))]
    public class SSCSoundFXSetEditor : Editor
    {
        #region Custom Editor private variables
        private SSCSoundFXSet sscSoundFXSet = null;
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

        private int sscDeletePos = -1;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent titleContent = new GUIContent("Sound FX Set");
        private readonly static GUIContent headerContent = new GUIContent("Contains a list of (sound) Effects Modules typically used when you want a collection of similar Sound FX");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty effectsModuleListProp;
        private SerializedProperty effectsModuleProp;
        #endregion
        
        #region Events

        private void OnEnable()
        {
            sscSoundFXSet = (SSCSoundFXSet)target;

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
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
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
            if (sscSoundFXSet.effectsModuleList == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                sscSoundFXSet.effectsModuleList = new List<EffectsModule>(10);
                EditorUtility.SetDirty(sscSoundFXSet);
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add or Remove types
            sscDeletePos = -1;
            int numEffects = effectsModuleListProp.arraySize;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Effects Modules", GUILayout.Width(140f));

            if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numEffects < 99)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(sscSoundFXSet, "Add Effects Slot");
                sscSoundFXSet.effectsModuleList.Add(null);
                EditorUtility.SetDirty(sscSoundFXSet);

                // Read in the properties
                serializedObject.Update();

                numEffects = effectsModuleListProp.arraySize;
            }
            if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
            {
                if (numEffects > 0) { sscDeletePos = effectsModuleListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region EffectsModule List

            bool isSaveChanges = false;

            for (int stypeIdx = 0; stypeIdx < numEffects; stypeIdx++)
            {
                effectsModuleProp = effectsModuleListProp.GetArrayElementAtIndex(stypeIdx);
                if (effectsModuleProp != null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" " + (stypeIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(effectsModuleProp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EffectsModule effectsModule = (EffectsModule)effectsModuleProp.objectReferenceValue;

                        if (effectsModule != null)
                        {
                            if (effectsModule.HasParticleSystems())
                            {
                                Debug.LogWarning("[ERROR] SSCSoundFX Set - EffectsModule (" + effectsModule.name + ") cannot have any ParticleSystems in a Sound FX set.");
                                effectsModuleProp.objectReferenceValue = null;
                            }
                            else if (!effectsModule.HasAudioSource())
                            {
                                Debug.LogWarning("[ERROR] SSCSoundFX Set - EffectsModule (" + effectsModule.name + ") must have an AudioSource to be used in a Sound FX set.");
                                effectsModuleProp.objectReferenceValue = null;
                            }
                        }

                        isSaveChanges = true;
                    }

                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { sscDeletePos = stypeIdx; }
                    GUILayout.EndHorizontal();
                }
            }

            if (isSaveChanges)
            {
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(sscSoundFXSet);
                }
            }

            #endregion

            #region Delete EffectsModule
            if (sscDeletePos >= 0)
            {
                effectsModuleListProp.DeleteArrayElementAtIndex(sscDeletePos);
                sscDeletePos = -1;

                serializedObject.ApplyModifiedProperties();
                // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(sscSoundFXSet);
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