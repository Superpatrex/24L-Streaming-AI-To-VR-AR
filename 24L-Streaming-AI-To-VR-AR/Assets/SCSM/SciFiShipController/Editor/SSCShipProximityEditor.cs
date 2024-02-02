using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(SSCShipProximity))]
    public class SSCShipProximityEditor : Editor
    {
        #region Static Strings

        #endregion

        #region Custom Editor private variables
        //private SSCShipProximity sscShipProximity;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        //private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This component let you call your game code, SSC API methods, and/or set properties on gameobjects when a ship enters or exits an area of your scene.");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise on Start", "If enabled, Initialise() will be called as soon as Start() runs. This should be disabled if you want to control when the SCC Ship Proximity is enabled through code.");
        private readonly static GUIContent initialiseDelayContent = new GUIContent("Initialise Delay", "How many seconds to delay initialisation if Initialise On Start is true");
        private readonly static GUIContent noNotifyDurationContent = new GUIContent("No Notify Duration", "The number of seconds, after initialisation, that events or callbacks will not be triggered by a ship entering or exiting the area.");
        private readonly static GUIContent tagsContent = new GUIContent("Unity Tags", "Array of Unity Tags for ships that affect this collider area. If none are provided, all ships can affect this area. NOTE: All tags MUST exist.");
        private readonly static GUIContent factionsToIncludeContent = new GUIContent("Factions to Include", "An optional array of ship Faction Ids to detect when entering the area.");
        private readonly static GUIContent factionsToExcludeContent = new GUIContent("Factions to Exclude", "An optional array of ship Faction Ids to ignore when entering the area.");
        private readonly static GUIContent squadronsToIncludeContent = new GUIContent("Squadrons to Include", "An optional array of ship Squadron Ids to detect when entering the area.");
        private readonly static GUIContent squadronsToExcludeContent = new GUIContent("Squadrons to Exclude", "An optional array of ship Squadron Ids to ignore when entering the area.");
        private readonly static GUIContent onEnterMethodsContent = new GUIContent("On Enter Methods", "Methods that get called when a ship enters the trigger area");
        private readonly static GUIContent onExitMethodsContent = new GUIContent("On Exit Methods", "Methods that get called when a ship exits the trigger area");

        #endregion

        #region Serialized Properties - General
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty initialiseDelayProp;
        private SerializedProperty noNotifyDurationProp;
        private SerializedProperty tagsProp;
        private SerializedProperty factionsToIncludeProp;
        private SerializedProperty factionsToExcludeProp;
        private SerializedProperty squadronsToIncludeProp;
        private SerializedProperty squadronsToExcludeProp;
        private SerializedProperty onEnterMethodsProp;
        private SerializedProperty onExitMethodsProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            //sscProximity = (SSCProximity)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            //separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #region Find Properties - General
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            initialiseDelayProp = serializedObject.FindProperty("initialiseDelay");
            noNotifyDurationProp = serializedObject.FindProperty("noNotifyDuration");
            tagsProp = serializedObject.FindProperty("tags");
            factionsToIncludeProp = serializedObject.FindProperty("factionsToInclude");
            factionsToExcludeProp = serializedObject.FindProperty("factionsToExclude");
            squadronsToIncludeProp = serializedObject.FindProperty("squadronsToInclude");
            squadronsToExcludeProp = serializedObject.FindProperty("squadronsToExclude");

            onEnterMethodsProp = serializedObject.FindProperty("onEnterMethods");
            onExitMethodsProp = serializedObject.FindProperty("onExitMethods");

            #endregion

            #region Find Buttons

            #endregion

        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Add a menu item so that a SSCShipProximity can be created via the GameObject > 3D Object menu
        /// </summary>
        /// <returns></returns>
        [MenuItem("GameObject/3D Object/Sci-Fi Ship Controller/SSC Ship Proximity (Sphere)")]
        public static SSCShipProximity CreateProximitySphere()
        {
            SSCShipProximity sscShipProximity = null;

            // Create a new gameobject
            GameObject proximityObj = new GameObject("SSCShipProximity (Sphere)");
            if (proximityObj != null)
            {
                SphereCollider proximityCollider = proximityObj.AddComponent<SphereCollider>();

                if (proximityCollider != null)
                {
                    proximityCollider.isTrigger = true;

                    sscShipProximity = proximityObj.AddComponent<SSCShipProximity>();

                    #if UNITY_EDITOR
                    if (sscShipProximity == null)
                    {
                        Debug.LogWarning("ERROR: SSCShipProximity.CreateProximitySphere could not add SSCShipProximity component to " + proximityObj.name);
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: SSCShipProximity.CreateProximitySphere could not add a sphere collider to " + proximityObj.name);
                }
                #endif
            }

            return sscShipProximity;
        }

        /// <summary>
        /// Add a menu item so that a SSCShipProximity can be created via the GameObject > 3D Object menu
        /// </summary>
        /// <returns></returns>
        [MenuItem("GameObject/3D Object/Sci-Fi Ship Controller/SSC Ship Proximity (Box)")]
        public static SSCShipProximity CreateProximityBox()
        {
            SSCShipProximity sscShipProximity = null;

            // Create a new gameobject
            GameObject proximityObj = new GameObject("SSCShipProximity (Box)");
            if (proximityObj != null)
            {
                BoxCollider proximityCollider = proximityObj.AddComponent<BoxCollider>();

                if (proximityCollider != null)
                {
                    proximityCollider.isTrigger = true;

                    sscShipProximity = proximityObj.AddComponent<SSCShipProximity>();

                    #if UNITY_EDITOR
                    if (sscShipProximity == null)
                    {
                        Debug.LogWarning("ERROR: SSCShipProximity.CreateProximityBox could not add SSCShipProximity component to " + proximityObj.name);
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: SSCShipProximity.CreateProximityBox could not add a box collider to " + proximityObj.name);
                }
                #endif
            }

            return sscShipProximity;
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
            isSceneModified = false;
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

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            #endregion

            #region General Settings
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
            EditorGUILayout.PropertyField(initialiseDelayProp, initialiseDelayContent);
            EditorGUILayout.PropertyField(noNotifyDurationProp, noNotifyDurationContent);

            SSCEditorHelper.DrawArray(tagsProp, tagsContent, defaultEditorLabelWidth, "Tag");
            SSCEditorHelper.DrawArray(factionsToIncludeProp, factionsToIncludeContent, defaultEditorLabelWidth, "Faction Id");
            SSCEditorHelper.DrawArray(factionsToExcludeProp, factionsToExcludeContent, defaultEditorLabelWidth, "Faction Id");
            SSCEditorHelper.DrawArray(squadronsToIncludeProp, squadronsToIncludeContent, defaultEditorLabelWidth, "Squadron Id");
            SSCEditorHelper.DrawArray(squadronsToExcludeProp, squadronsToExcludeContent, defaultEditorLabelWidth, "Squadron Id");

            GUILayoutUtility.GetRect(1f, 2f);

            EditorGUILayout.PropertyField(onEnterMethodsProp, onEnterMethodsContent);
            EditorGUILayout.PropertyField(onExitMethodsProp, onExitMethodsContent);

            GUILayout.EndVertical();
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion
        }

        #endregion
    }
}