using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickySocket))]
    [CanEditMultipleObjects]
    public class StickySocketEditor : Editor
    {
        #region Custom Editor protected variables
        // These are visible to inherited classes
        protected StickySocket stickySocket;
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
        protected bool isDebuggingEnabled = false;
        protected string[] interactiveTagNames = null;

        #endregion

        #region SceneView Variables

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This component, can be used to attach or snap interactive-enabled objects to a gameobject.");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Socket"), new GUIContent("Events") };

        #endregion

        #region GUIContent - General
        protected readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the Sticky Socket component is enabled through code.");
        protected readonly static GUIContent maxItemsContent = new GUIContent(" Max Items", "The maximum number of items to attach to this socket");
        protected readonly static GUIContent isDisableRegularColOnAddContent = new GUIContent(" Disable Regular Colliders", "When adding, non-trigger colliders on interactive-enabled objects will be disabled. When removed, they will be re-enabled.");
        protected readonly static GUIContent isDisableTriggerColOnAddContent = new GUIContent(" Disable Trigger Colliders", "When adding, trigger colliders on interactive-enabled objects will be disabled. When removed, they will be re-enabled.");
        protected readonly static GUIContent interactiveTagsContent = new GUIContent(" Interactive Tags", "This is a Scriptable Object containing a list of 32 tags. To create custom tags, in the Project pane, click Create->Sticky3D->Interactive Tags.");
        protected readonly static GUIContent permittedTagsContent = new GUIContent(" Permitted Tags", "The interactive-enabled objects that are permitted to be attached to the socket. See the S3DInteractiveTags scriptableobject.");

        #endregion

        #region GUIContent - Events
        protected readonly static GUIContent onHoverEnterContent = new GUIContent(" On Hover Enter", "These are triggered by a S3D character when they start looking at this socket.");
        protected readonly static GUIContent onHoverExitContent = new GUIContent(" On Hover Exit", "These are triggered by a S3D character when they stop looking at this socket.");
        protected readonly static GUIContent onPreAddContent = new GUIContent(" On Pre Add", "These are triggered near the start of the AddItem process for a StickyInteractive object.");
        protected readonly static GUIContent onPostAddContent = new GUIContent(" On Post Add", "These are triggered immediately after a StickyInteractive object has been added to the socket.");

        #endregion

        #region GUIContent - Popup
        protected readonly static GUIContent defaultPopupOffsetContent = new GUIContent(" Default Popup Offset", "The default local space offset a StickyPopupModule appears relative to the socket");
        protected readonly static GUIContent defaultPopupPrefabContent = new GUIContent(" Default Popup Prefab", "The default StickyPopupModule to display when a character engages with this socket");
        protected readonly static GUIContent emptyPopupOffsetContent = new GUIContent(" Empty Popup Offset", "The local space offset a StickyPopupModule appears relative to the socket when the socket is empty or has no interactive objects attached.");
        protected readonly static GUIContent emptyPopupPrefabContent = new GUIContent(" Empty Popup Prefab", "The StickyPopupModule to display when a character engages with the socket, and the socket is empty or has no interactive objects attached.");
        #endregion

        #region GUIContent - Debug
        protected readonly static GUIContent debugDefaultPopupPrefabIDContent = new GUIContent(" Default Popup Prefab ID");
        protected readonly static GUIContent debugEmptyPopupPrefabIDContent = new GUIContent(" Empty Popup Prefab ID");
        protected readonly static GUIContent debugNumberSocketedItemsContent = new GUIContent(" Socketed Items");
        #endregion

        #region Serialized Properties - General
        protected SerializedProperty selectedTabIntProp;
        protected SerializedProperty initialiseOnStartProp;
        protected SerializedProperty maxItemsProp;
        protected SerializedProperty isDisableRegularColOnAddProp;
        protected SerializedProperty isDisableTriggerColOnAddProp;
        protected SerializedProperty interactiveTagsProp;
        protected SerializedProperty permittedTagsProp;
        #endregion

        #region Serializable Properties - Events
        private SerializedProperty onHoverEnterProp;
        private SerializedProperty onHoverExitProp;
        private SerializedProperty onPreAddProp;
        private SerializedProperty onPostAddProp;

        #endregion

        #region Serialized Properties - Popup
        private SerializedProperty defaultPopupOffsetProp;
        private SerializedProperty defaultPopupPrefabProp;
        private SerializedProperty emptyPopupOffsetProp;
        private SerializedProperty emptyPopupPrefabProp;
        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            stickySocket = (StickySocket)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            interactiveTagNames = null;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            maxItemsProp = serializedObject.FindProperty("maxItems");
            isDisableRegularColOnAddProp = serializedObject.FindProperty("isDisableRegularColOnAdd");
            isDisableTriggerColOnAddProp = serializedObject.FindProperty("isDisableTriggerColOnAdd");
            interactiveTagsProp = serializedObject.FindProperty("interactiveTags");
            permittedTagsProp = serializedObject.FindProperty("permittedTags");

            #endregion

            #region Find Properties - Events
            onHoverEnterProp = serializedObject.FindProperty("onHoverEnter");
            onHoverExitProp = serializedObject.FindProperty("onHoverExit");
            onPreAddProp = serializedObject.FindProperty("onPreAdd");
            onPostAddProp = serializedObject.FindProperty("onPostAdd");
            #endregion

            #region Find Properties - Popup
            defaultPopupOffsetProp = serializedObject.FindProperty("defaultPopupOffset");
            defaultPopupPrefabProp = serializedObject.FindProperty("defaultPopupPrefab");
            emptyPopupOffsetProp = serializedObject.FindProperty("emptyPopupOffset");
            emptyPopupPrefabProp = serializedObject.FindProperty("emptyPopupPrefab");
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
        /// Draw the base events available in StickySocket
        /// </summary>
        protected void DrawBaseEvents()
        {
            EditorGUILayout.PropertyField(onHoverEnterProp, onHoverEnterContent);
            EditorGUILayout.PropertyField(onHoverExitProp, onHoverExitContent);
            EditorGUILayout.PropertyField(onPreAddProp, onPreAddContent);
            EditorGUILayout.PropertyField(onPostAddProp, onPostAddContent);
        }

        /// <summary>
        /// Draw the base general settings in the inspector.
        /// </summary>
        protected void DrawBaseGeneralSettings()
        {
            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
            EditorGUILayout.PropertyField(maxItemsProp, maxItemsContent);
            EditorGUILayout.PropertyField(isDisableRegularColOnAddProp, isDisableRegularColOnAddContent);
            EditorGUILayout.PropertyField(isDisableTriggerColOnAddProp, isDisableTriggerColOnAddContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(interactiveTagsProp, interactiveTagsContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                interactiveTagNames = null;
                serializedObject.Update();
            }

            if (interactiveTagNames == null)
            {
                if (interactiveTagsProp.objectReferenceValue != null)
                {
                    interactiveTagNames = ((S3DInteractiveTags)interactiveTagsProp.objectReferenceValue).GetTagNames(true);
                }
                else
                {
                    interactiveTagNames = new string[] { "Default" };
                }
            }

            permittedTagsProp.intValue = EditorGUILayout.MaskField(permittedTagsContent, permittedTagsProp.intValue, interactiveTagNames);
        }

        /// <summary>
        /// Draw if this object is initialised in the inspector.
        /// </summary>
        protected void DrawDebugIsInitialised()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(StickyEditorHelper.debugIsInitialisedIndent1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
            EditorGUILayout.LabelField(stickySocket.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw popup data in the inspector
        /// </summary>
        protected void DrawDebugPopupData()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugDefaultPopupPrefabIDContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
            EditorGUILayout.LabelField(stickySocket.DefaultPopupPrefabID.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugEmptyPopupPrefabIDContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
            EditorGUILayout.LabelField(stickySocket.EmptyPopupPrefabID.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(debugNumberSocketedItemsContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
            EditorGUILayout.LabelField(stickySocket.NumberOfSocketedItems.ToString(), GUILayout.MaxWidth(defaultEditorFieldWidth));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw enable or disable debugging in the inspector
        /// </summary>
        protected void DrawDebugToggle()
        {
            isDebuggingEnabled = EditorGUILayout.Toggle(StickyEditorHelper.debugModeIndent1Content, isDebuggingEnabled);
        }

        // Draw the default popup settings in the inspector
        protected void DrawPopupSettings()
        {
            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(defaultPopupOffsetProp, defaultPopupOffsetContent);
            EditorGUILayout.PropertyField(defaultPopupPrefabProp, defaultPopupPrefabContent);

            StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

            EditorGUILayout.PropertyField(emptyPopupOffsetProp, emptyPopupOffsetContent);
            EditorGUILayout.PropertyField(emptyPopupPrefabProp, emptyPopupPrefabContent);
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
            stickySocket.allowRepaint = false;
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
            DrawToolBar(tabTexts);
            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            #region Socket Settings
            if (selectedTabIntProp.intValue == 0)
            {
                DrawBaseGeneralSettings();

                DrawPopupSettings();
            }
            #endregion

            #region Event Settings
            else
            {
                // Add small horizontal gap
                StickyEditorHelper.DrawHorizontalGap(2f);

                DrawBaseEvents();
            }
            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickySocket.allowRepaint = true;

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawDebugToggle();
            if (isDebuggingEnabled && stickySocket != null)
            {
                Repaint();
                //float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                DrawDebugIsInitialised();

                DrawDebugPopupData();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }


        /// <summary>
        /// Draw the toolbar using the supplied array of tab text.
        /// </summary>
        /// <param name="tabGUIContent"></param>
        protected virtual void DrawToolBar(GUIContent[] tabGUIContent)
        {
            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabGUIContent);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }
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