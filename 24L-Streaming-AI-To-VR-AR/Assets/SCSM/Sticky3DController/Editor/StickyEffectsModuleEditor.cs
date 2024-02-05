using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyEffectsModule class
    /// </summary>
    [CustomEditor(typeof(StickyEffectsModule))]
    public class StickyEffectsModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyEffectsModule stickyEffectsModule = null;
        private bool isDebuggingEnabled = false;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A poolable particle and/or sound effect used when something is hit, damaged or destroyed");
        #endregion

        #region GUIContent - Effect
        private readonly static GUIContent effectsTypeContent = new GUIContent(" Effects Type", "Most effects modules will have the Default type. Sound FX is for items that specifically require a Sound FX rather than a general Sticky Effects Module.");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        #endregion

        #region Serialized Properties - Effect
        private SerializedProperty effectsTypeProp;
        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyEffectsModule = (StickyEffectsModule)target;

            #region Find Properties - Effect
            effectsTypeProp = serializedObject.FindProperty("effectsType");

            #endregion
        }

        #endregion

        #region Protected and Private Methods

        /// <summary>
        /// Draw the effects type in the inspector
        /// </summary>
        protected void DrawEffectsType()
        {
            EditorGUILayout.PropertyField(effectsTypeProp, effectsTypeContent);
        }

        #endregion

        #region DrawBaseInspector
        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyEffectsModule.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            DrawEffectsType();
            DrawBaseSettings();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyEffectsModule != null)
            {
                //float rightLabelWidth = 150f;

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField(debugSourceStickyIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                //EditorGUILayout.LabelField(stickyBeamModule.sourceStickyId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                //EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyEffectsModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyEffectsModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Effects Object")]
        public static StickyEffectsModule CreateStickyEffectsObject()
        {
            StickyEffectsModule stickyEffectsModule = null;

            // Create a new gameobject
            GameObject stickyEffectsObj = new GameObject("StickyEffectsObject");
            
            if (stickyEffectsObj != null)
            {
                GameObject particleObj = new GameObject("Particle1");

                if (particleObj != null)
                {
                    ParticleSystem particleSystem = particleObj.AddComponent<ParticleSystem>();
                    

                    particleObj.transform.SetParent(stickyEffectsObj.transform);
                }

                // Add the Sticky Effects Module before the audio source so it appear above it.
                stickyEffectsModule = stickyEffectsObj.AddComponent<StickyEffectsModule>();

                if (stickyEffectsModule != null)
                {
                    stickyEffectsModule.isReparented = true;
                }

                // Add a basic audio source
                AudioSource audioSource = stickyEffectsObj.AddComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.spatialBlend = 1f;
                    audioSource.playOnAwake = false;
                }
            }

            return stickyEffectsModule;
        }

        // Add a menu item so that a StickyEffectsModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Sound FX")]
        public static StickyEffectsModule CreateStickySoundFX()
        {
            StickyEffectsModule stickyEffectsModule = null;

            // Create a new gameobject
            GameObject stickyEffectsObj = new GameObject("StickySoundFX");
            
            if (stickyEffectsObj != null)
            {
                // Add the Sticky Effects Module before the audio source so it appear above it.
                stickyEffectsModule = stickyEffectsObj.AddComponent<StickyEffectsModule>();

                if (stickyEffectsModule != null)
                {
                    stickyEffectsModule.isReparented = true;
                    stickyEffectsModule.ModuleEffectsType = StickyEffectsModule.EffectsType.SoundFX;
                }

                // Add a basic audio source
                AudioSource audioSource = stickyEffectsObj.AddComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.spatialBlend = 1f;
                    audioSource.playOnAwake = false;
                }
            }

            return stickyEffectsModule;
        }

        #endregion
    }
}