using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The base editor for StickyGenericModules
    /// </summary>
    [CustomEditor(typeof(StickyGenericModule))]
    [CanEditMultipleObjects]
    public class StickyGenericModuleEditor : Editor
    {
        #region Custom Editor protected variables
        // These are visible to inherited classes
        protected StickyGenericModule stickyGenericModule;
        protected bool isStylesInitialised = false;
        protected bool isSceneModified = false;
        protected string labelText;
        protected GUIStyle labelFieldRichText;
        protected GUIStyle headingFieldRichText;
        protected GUIStyle helpBoxRichText;
        protected GUIStyle buttonCompact;
        protected GUIStyle foldoutStyleNoLabel;
        protected GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        protected GUIStyle toggleCompactButtonStyleToggled = null;
        protected Color separatorColor = new Color();
        protected float defaultEditorLabelWidth = 0f;
        protected float defaultEditorFieldWidth = 0f;
        #endregion

        #region Static Strings

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This component is used with the Sticky Manager to make an object in your scene poolable.");

        #endregion

        #region GUIContent - General
        protected readonly static GUIContent minPoolSizeContent = new GUIContent(" Min Pool Size", "The starting size of the pool.");
        protected readonly static GUIContent maxPoolSizeContent = new GUIContent(" Max Pool Size", "The maximum allowed size of the pool.");
        protected readonly static GUIContent despawnTimeContent = new GUIContent(" Despawn Time", "The object will be automatically despawned after this amount of time (in seconds) has elapsed.");
        protected readonly static GUIContent isReparentedContent = new GUIContent(" Is Reparented", "Does this object get parented to another object when activated? If so, it will be reparented to the pool transform after use.");
        #endregion

        #region Serialized Properties - General
        protected SerializedProperty minPoolSizeProp;
        protected SerializedProperty maxPoolSizeProp;
        protected SerializedProperty despawnTimeProp;
        protected SerializedProperty isReparentedProp;

        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            stickyGenericModule = (StickyGenericModule)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            #region Find Properties - General
            minPoolSizeProp = serializedObject.FindProperty("minPoolSize");
            maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
            despawnTimeProp = serializedObject.FindProperty("despawnTime");
            isReparentedProp = serializedObject.FindProperty("isReparented");
            #endregion
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// Comment out if not required
        /// </summary>
        private void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (stickyGenericModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private and Protected Methods

        /// <summary>
        /// If not in play mode, check if the scene has been modified and needs
        /// marking accordingly.
        /// </summary>
        protected void CheckMarkSceneDirty()
        {
            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Set up the buttons and styles used in OnInspectorGUI.
        /// Call this near the top of OnInspectorGUI.
        /// </summary>
        protected void ConfigureButtonsAndStyles()
        {
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

                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;

                isStylesInitialised = true;
            }
        }

        /// <summary>
        ///  Draw the despawn time setting in the inspector
        /// </summary>
        protected void DrawDespawnTIme()
        {
            EditorGUILayout.PropertyField(despawnTimeProp, despawnTimeContent);
        }

        /// <summary>
        /// Draw the Min and Max Pool size settings in the inspector
        /// </summary>
        protected void DrawMinMaxPoolSize()
        {
            EditorGUILayout.PropertyField(minPoolSizeProp, minPoolSizeContent);
            EditorGUILayout.PropertyField(maxPoolSizeProp, maxPoolSizeContent);
            if (minPoolSizeProp.intValue > maxPoolSizeProp.intValue) { maxPoolSizeProp.intValue = minPoolSizeProp.intValue; }
        }

        /// <summary>
        /// Draw the IsReparented setting in the inspector
        /// </summary>
        protected void DrawIsReparented()
        {
            EditorGUILayout.PropertyField(isReparentedProp, isReparentedContent);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// This function overides what is normally seen in the inspector window
        /// This allows stuff like buttons to be drawn there
        /// </summary>
        protected virtual void DrawBaseInspector()
        {
            #region Initialise
            stickyGenericModule.allowRepaint = false;
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

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickyGenericModule.allowRepaint = true;
        }

        /// <summary>
        /// Draw the base settings that should be common to most poolable generic objects
        /// </summary>
        protected virtual void DrawBaseSettings()
        {
            DrawMinMaxPoolSize();
            DrawDespawnTIme();
            DrawIsReparented();
        }

        #endregion

        #region OnInspectorGUI

        public override void OnInspectorGUI()
        {
            DrawBaseInspector();
        }

        #endregion
    }
}