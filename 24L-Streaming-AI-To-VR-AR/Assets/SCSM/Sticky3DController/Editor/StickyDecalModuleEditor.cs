using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyDecalModule class
    /// </summary>
    [CustomEditor(typeof(StickyDecalModule))]
    public class StickyDecalModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyDecalModule stickyDecalModule = null;
        private bool isDebuggingEnabled = false;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A poolable decal used when something is hit or damaged");
        #endregion

        #region GUIContent - Decal
        private readonly static GUIContent overlapAmountContent = new GUIContent(" Overlap Amount", "The amount of overlap permitted when placing near the edge of objects.");
        private readonly static GUIContent fadeOutTimeContent = new GUIContent(" Fade-out Time", "The time, in seconds that the decal will fade from view to prevent it popping out when it despawns. This should be less than the despawn time.");
        private readonly static GUIContent fadeOutFrequencyContent = new GUIContent(" Fade-out Frequency", "The time, in seconds, between each update to the decal material when fading out of view.");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        #endregion

        #region Serialized Properties - Decal
        private SerializedProperty overlapAmountProp;
        private SerializedProperty fadeOutTimeProp;
        private SerializedProperty fadeOutFrequencyProp;
        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyDecalModule = (StickyDecalModule)target;

            #region Find Properties - Decal
            overlapAmountProp = serializedObject.FindProperty("overlapAmount");
            fadeOutTimeProp = serializedObject.FindProperty("fadeOutTime");
            fadeOutFrequencyProp = serializedObject.FindProperty("fadeOutFrequency");

            #endregion
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Draw the decal Fade Out settings of Decals in the inspector
        /// </summary>
        protected void DrawFadeOutSettings()
        {
            EditorGUILayout.PropertyField(fadeOutTimeProp, fadeOutTimeContent);
            EditorGUILayout.PropertyField(fadeOutFrequencyProp, fadeOutFrequencyContent);
        }

        /// <summary>
        /// Draw the decal overlay amount in the inspector
        /// </summary>
        protected void DrawOverlayAmount()
        {
            EditorGUILayout.PropertyField(overlapAmountProp, overlapAmountContent);
        }

        #endregion

        #region DrawBaseInspector
        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyDecalModule.allowRepaint = false;
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

            DrawBaseSettings();
            DrawOverlayAmount();
            DrawFadeOutSettings();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyDecalModule != null)
            {
                //float rightLabelWidth = 150f;

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField(debugSourceStickyIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                //EditorGUILayout.LabelField(stickyBeamModule.sourceStickyId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                //EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyDecalModule.allowRepaint = true;
        }

        #endregion

        #region Static API Methods
        // Add a menu item so that a StickyDecalModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Decal")]
        public static StickyDecalModule CreateStickyDecalObject()
        {
            StickyDecalModule stickyDecalModule = null;

            // Create a new gameobject
            GameObject stickyDecalObj = new GameObject("StickyDecal");
            
            if (stickyDecalObj != null)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

                quad.transform.localScale = Vector3.one * 0.04f;
                quad.transform.SetParent(stickyDecalObj.transform);

                Collider quadCollider;
                if (quad.TryGetComponent(out quadCollider))
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(quadCollider);
                    #else
                    Destroy(quadCollider);
                    #endif
                }

                // Add the Sticky Decal Module before the audio source so it appear above it.
                stickyDecalModule = stickyDecalObj.AddComponent<StickyDecalModule>();

                if (stickyDecalModule != null)
                {
                    stickyDecalModule.despawnTime = 20f;
                    stickyDecalModule.fadeOutTime = 5f;
                    stickyDecalModule.fadeOutFrequency = 0.1f;
                    stickyDecalModule.isReparented = true;
                }             
            }

            return stickyDecalModule;
        }

        #endregion
    }
}