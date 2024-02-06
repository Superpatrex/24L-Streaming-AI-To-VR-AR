using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The inspector editor for SSCInputBridge component
    /// </summary>
    [CustomEditor(typeof(SSCInputBridge))]
    public class SSCInputBridgeEditor : Editor
    {
        #region Custom Editor private variables
        protected SSCInputBridge sscInputBridge = null;
        protected bool isStylesInitialised = false;
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

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("Contains a component for sending Readable Interactive-enabled object data to a Sci-Fi Ship Controller ship");
        #endregion

        #region GUIContent - General
        protected readonly static GUIContent initialiseOnStartContent = new GUIContent(" Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the component is enabled through code.");
        protected readonly static GUIContent shipGameObjectContent = new GUIContent(" Ship GameObject", "The player ship from your scene which should contain a ShipControlModule on the same gameobject.");
        protected readonly static GUIContent inputAxisXContent = new GUIContent(" Input Axis X", "Interactive Readable left-right data, to be send to the input axis of the Sci-Fi Ship Controller player ship. [NOT CHANGEABLE AT RUNTIME]");
        protected readonly static GUIContent inputAxisZContent = new GUIContent(" Input Axis Z", "Interactive Readable forward-back data, to be send to the input axis of the Sci-Fi Ship Controller player ship. [NOT CHANGEABLE AT RUNTIME]");
        #endregion

        #region Serialized Properties - General
        protected SerializedProperty initialiseOnStartProp;
        protected SerializedProperty shipGameObjectProp;
        protected SerializedProperty inputAxisXProp;
        protected SerializedProperty inputAxisZProp;
        #endregion

        #region Events

        protected virtual void OnEnable()
        {
            sscInputBridge = (SSCInputBridge)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties - General
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            shipGameObjectProp = serializedObject.FindProperty("shipGameObject");
            inputAxisXProp = serializedObject.FindProperty("inputAxisX");
            inputAxisZProp = serializedObject.FindProperty("inputAxisZ");
            #endregion

            // Reset GUIStyles
            isStylesInitialised = false;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;
        }

        #endregion

        #region Private and Protected Methods

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
        /// Draw initialise properties in the inspector
        /// </summary>
        protected void DrawInitialise()
        {
            if (!S3DUtils.IsSSCAvailable)
            {
                EditorGUILayout.HelpBox("Sci-Fi Ship Controller does not appear to be installed in this project", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
        }

        /// <summary>
        /// Draw the input axis X (left-right) properties in the inspector
        /// </summary>
        protected void DrawInputAxisX()
        {
            EditorGUILayout.PropertyField(inputAxisXProp, inputAxisXContent);
        }

        /// <summary>
        /// Draw the input axis Z (forward-back) properties in the inspector
        /// </summary>
        protected void DrawInputAxisZ()
        {
            EditorGUILayout.PropertyField(inputAxisZProp, inputAxisZContent);
        }

        /// <summary>
        /// Draw the ship gameobject in the inspector
        /// </summary>
        protected void DrawShipGameObject()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shipGameObjectProp, shipGameObjectContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                sscInputBridge.SetShip((GameObject)shipGameObjectProp.objectReferenceValue);
            }
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Draw the base inspector. Override this when creating your own inherited class
        /// </summary>
        protected virtual void DrawBaseInspector()
        {
            #region Initialise
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
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

            DrawInitialise();
            DrawShipGameObject();
            DrawInputAxisX();
            DrawInputAxisZ();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
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