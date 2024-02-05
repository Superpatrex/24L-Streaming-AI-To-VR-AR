using UnityEngine;
using UnityEditor;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(EffectsModule))]
    public class EffectsModuleEditor : Editor
    {
        #region Custom Editor private variables

        // Formatting and style variables
        //private string txtColourName = "Black";
        //private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        #endregion

        #region GUIContent

        private readonly static GUIContent headerContent = new GUIContent("<b>Effects Module</b>\n\nThis module enables you to implement effects behaviour on the object it is attached to. If including an Audio Source with a clip, it should be attached to the same gameobject as this script.");
        private readonly static GUIContent usePoolingContent = new GUIContent("Use Pooling", "Use the Pooling system to manage create, re-use, and destroy effects objects.");
        private readonly static GUIContent minPoolSizeContent = new GUIContent("Min Pool Size", "When using the Pooling system, this is the number of effects objects kept in reserve for spawning and despawning.");
        private readonly static GUIContent maxPoolSizeContent = new GUIContent("Max Pool Size", "When using the Pooling system, this is the maximum number of effects objects permitted in the scene at any one time.");
        private readonly static GUIContent despawnTimeContent = new GUIContent("Despawn Time", "After this time (in seconds), the effects object is automatically despawned or removed from the scene.");
        private readonly static GUIContent isReparentedContent = new GUIContent("Is Reparented", "Does this object get parented to another object when activated? If so, it will be reparented to the pool transform after use.");
        private readonly static GUIContent defaultVolumeContent = new GUIContent("Default Volume", "If an audio source is included, the volume can be optionally set at runtime in C# code to the default volume.");
        #endregion

        #region Serialized Properties

        private SerializedProperty usePoolingProp;
        private SerializedProperty minPoolSizeProp;
        private SerializedProperty maxPoolSizeProp;
        private SerializedProperty isReparentedProp;
        private SerializedProperty defaultVolumeProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            //effectsModule = (EffectsModule)target;

            // Used in Richtext labels
            //if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            //if (defaultTextColour.a > 0f) { }

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties

            usePoolingProp = serializedObject.FindProperty("usePooling");
            isReparentedProp = serializedObject.FindProperty("isReparented");
            defaultVolumeProp = serializedObject.FindProperty("defaultVolume");

            #endregion
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
            helpBoxRichText = new GUIStyle("HelpBox");
            helpBoxRichText.richText = true;

            labelFieldRichText = new GUIStyle("Label");
            labelFieldRichText.richText = true;

            buttonCompact = new GUIStyle("Button");
            buttonCompact.fontSize = 10;

            #endregion

            // Read in all the properties
            serializedObject.Update();

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(usePoolingProp, usePoolingContent);
            if (usePoolingProp.boolValue)
            {
                minPoolSizeProp = serializedObject.FindProperty("minPoolSize");
                maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
                EditorGUILayout.PropertyField(minPoolSizeProp, minPoolSizeContent);
                EditorGUILayout.PropertyField(maxPoolSizeProp, maxPoolSizeContent);
                if (minPoolSizeProp.intValue > maxPoolSizeProp.intValue) { maxPoolSizeProp.intValue = minPoolSizeProp.intValue; }
                EditorGUILayout.PropertyField(isReparentedProp, isReparentedContent);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("despawnTime"), despawnTimeContent);
            EditorGUILayout.PropertyField(defaultVolumeProp, defaultVolumeContent);

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
