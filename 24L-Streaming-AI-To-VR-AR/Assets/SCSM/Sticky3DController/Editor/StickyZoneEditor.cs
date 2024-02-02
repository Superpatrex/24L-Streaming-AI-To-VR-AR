using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyZone))]
    [CanEditMultipleObjects]
    public class StickyZoneEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables

        private StickyZone stickyZone = null;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        //private bool isRefreshing = false;
        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        // Anim Clip Sets
        private int s3dAnimClipSetDeletePos = -1;

        #endregion

        #region GUIContent
        private readonly static GUIContent headerContent = new GUIContent("This component enables you to override StickyControlModule settings in a 3D area using a trigger collider");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise On Start", "If enabled, the " +
            "Initialise() will be called as soon as Start() runs. This should be disabled if you are instantiating the zone through code.");
        private readonly static GUIContent overrideReferenceFrameContent = new GUIContent("Reference Frame", "Override the reference frame while a Sticky3D Controller is within the collider zone");
        private readonly static GUIContent overrideReferenceTransformContent = new GUIContent(" Reference Transform", "The reference transform to be used while a Sticky3D Controller is within the collider zone");
        private readonly static GUIContent isRestoreDefaultRefTransformOnExitContent = new GUIContent(" Restore Default Ref.", "Should the initial or default Reference Frame transform be restored when the Sticky3D Controller exits the zone?");
        private readonly static GUIContent isRestorePreviousRefTransformOnExitContent = new GUIContent(" Restore Previous Ref.", "Should the previous Reference Frame transform be restored when the Sticky3D Controller exits the zone? If the previous is null, the initial or default one is restored.");
        private readonly static GUIContent overrideLookFirstPersonContent = new GUIContent("Look First Person", "If enabled, when entering the zone area, set Look to First Person on the Sticky3D Controller, by turning off Third Person.");
        private readonly static GUIContent overrideLookThirdPersonContent = new GUIContent("Look Third Person", "If enabled, when entering the zone area, set Look to Third Person on the Sticky3D Controller.");
        private readonly static GUIContent overrideGravityContent = new GUIContent("Gravity", "Does the zone override the gravity of Sticky3D Controllers entering it?");
        private readonly static GUIContent gravitationalAccelerationContent = new GUIContent(" Gravitational Acceleration", "If overridden, the gravitational acceleration to apply to Sticky3D Controllers entering the zone.");
        private readonly static GUIContent overrideAnimClipsContent = new GUIContent("Animation Clips", "Does the zone override some animation clips of the Sticky3D Controllers entering it?");
        private readonly static GUIContent isRestorePreviousAnimClipsOnExitContent = new GUIContent("Restore Clips on Exit", "Should the original clips be restored when the Sticky3D Controller exits the zone?");
        private readonly static GUIContent animClipSetsContent = new GUIContent("Animation Clip Sets", "One or more Anim Clip Set scriptable objects that contain original and replacement animation clip pairs.");
        private readonly static GUIContent factionsToFilterContent = new GUIContent("Factions To Filter", "An optional array of Faction IDs that will limit which characters this zone applies to");
        private readonly static GUIContent modelsToFilterContent = new GUIContent("Models To Filter", "An optional array of Model IDs that will limit which characters this zone applies to");

        #endregion

        #region Properties
        private SerializedProperty initialiseOnStartProp;
        private SerializedProperty overrideReferenceFrameProp;
        private SerializedProperty referenceTransformProp;
        private SerializedProperty isRestoreDefaultRefTransformOnExitProp;
        private SerializedProperty isRestorePreviousRefTransformOnExitProp;
        private SerializedProperty overrideLookFirstPersonProp;
        private SerializedProperty overrideLookThirdPersonProp;
        private SerializedProperty overrideGravityProp;
        private SerializedProperty gravitationalAccelerationProp;
        private SerializedProperty overrideAnimClipsProp;
        private SerializedProperty isRestorePreviousAnimClipsOnExitProp;
        private SerializedProperty s3dAnimClipSetListProp;
        private SerializedProperty s3dAnimClipSetProp;
        private SerializedProperty factionsToFilterProp;
        private SerializedProperty modelsToFilterProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            stickyZone = (StickyZone)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties
            initialiseOnStartProp = serializedObject.FindProperty("initialiseOnStart");
            overrideReferenceFrameProp = serializedObject.FindProperty("overrideReferenceFrame");
            referenceTransformProp = serializedObject.FindProperty("referenceTransform");
            isRestoreDefaultRefTransformOnExitProp = serializedObject.FindProperty("isRestoreDefaultRefTransformOnExit");
            isRestorePreviousRefTransformOnExitProp = serializedObject.FindProperty("isRestorePreviousRefTransformOnExit");
            overrideLookFirstPersonProp = serializedObject.FindProperty("overrideLookFirstPerson");
            overrideLookThirdPersonProp = serializedObject.FindProperty("overrideLookThirdPerson");
            overrideGravityProp = serializedObject.FindProperty("overrideGravity");
            gravitationalAccelerationProp = serializedObject.FindProperty("gravitationalAcceleration");
            overrideAnimClipsProp = serializedObject.FindProperty("overrideAnimClips");
            isRestorePreviousAnimClipsOnExitProp = serializedObject.FindProperty("isRestorePreviousAnimClipsOnExit");
            s3dAnimClipSetListProp = serializedObject.FindProperty("s3dAnimClipSetList");
            factionsToFilterProp = serializedObject.FindProperty("factionsToFilter");
            modelsToFilterProp = serializedObject.FindProperty("modelsToFilter");
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

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            #endregion

            serializedObject.Update();
            
            EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);
            EditorGUILayout.PropertyField(overrideReferenceFrameProp, overrideReferenceFrameContent);
            if (overrideReferenceFrameProp.boolValue)
            {
                EditorGUILayout.PropertyField(referenceTransformProp, overrideReferenceTransformContent);
                EditorGUILayout.PropertyField(isRestoreDefaultRefTransformOnExitProp, isRestoreDefaultRefTransformOnExitContent);
                EditorGUILayout.PropertyField(isRestorePreviousRefTransformOnExitProp, isRestorePreviousRefTransformOnExitContent);
            }

            EditorGUILayout.PropertyField(overrideLookFirstPersonProp, overrideLookFirstPersonContent);
            EditorGUILayout.PropertyField(overrideLookThirdPersonProp, overrideLookThirdPersonContent);

            EditorGUILayout.PropertyField(overrideGravityProp, overrideGravityContent);
            if (overrideGravityProp.boolValue)
            {
                EditorGUILayout.PropertyField(gravitationalAccelerationProp, gravitationalAccelerationContent);
            }

            #region Override Animation Clips
            EditorGUILayout.PropertyField(overrideAnimClipsProp, overrideAnimClipsContent);
            if (overrideAnimClipsProp.boolValue)
            {
                EditorGUILayout.PropertyField(isRestorePreviousAnimClipsOnExitProp, isRestorePreviousAnimClipsOnExitContent);

                #region Add or Remove Anim Clip Pairs
                s3dAnimClipSetDeletePos = -1;
                int numAnimClipSets = s3dAnimClipSetListProp.arraySize;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(animClipSetsContent, GUILayout.Width(140f));

                // Limit to 20 sets of pairs - not sure why anyone would need more
                if (GUILayout.Button("+", buttonCompact, GUILayout.Width(20f)) && numAnimClipSets < 21)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(stickyZone, "Add Anim Clip Set");
                    // Add an empty slot
                    stickyZone.AddAnimClipSet(null);
                    isSceneModified = true;

                    // Read in the properties
                    serializedObject.Update();

                    numAnimClipSets = s3dAnimClipSetListProp.arraySize;
                }
                if (GUILayout.Button("-", buttonCompact, GUILayout.Width(20f)))
                {
                    if (numAnimClipSets > 0) { s3dAnimClipSetDeletePos = s3dAnimClipSetListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();
                #endregion

                #region Anim Clip Set List

                //GUILayout.BeginHorizontal();
                //float headerWidth = (defaultEditorLabelWidth + defaultEditorFieldWidth) * 0.5f;
                //EditorGUILayout.LabelField("Num", GUILayout.Width(25f));
                //EditorGUILayout.LabelField("Original", GUILayout.MinWidth(headerWidth));
                //EditorGUILayout.LabelField("Replacement");
                //GUILayout.EndHorizontal();

                for (int clipSetIdx = 0; clipSetIdx < numAnimClipSets; clipSetIdx++)
                {
                    s3dAnimClipSetProp = s3dAnimClipSetListProp.GetArrayElementAtIndex(clipSetIdx);
                    if (s3dAnimClipSetProp != null)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" " + (clipSetIdx + 1).ToString("00") + ".", GUILayout.Width(25f));

                        EditorGUILayout.PropertyField(s3dAnimClipSetProp, GUIContent.none);

                        if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { s3dAnimClipSetDeletePos = clipSetIdx; }
                        GUILayout.EndHorizontal();
                    }
                }

                #endregion

                #region Delete Anim Clip Set
                if (s3dAnimClipSetDeletePos >= 0)
                {
                    s3dAnimClipSetListProp.DeleteArrayElementAtIndex(s3dAnimClipSetDeletePos);
                    s3dAnimClipSetDeletePos = -1;

                    #if !UNITY_2019_3_OR_NEWER
                    serializedObject.ApplyModifiedProperties();
                    // In U2019.4+ avoid: EndLayoutGroup: BeginLayoutGroup must be called first.
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    GUIUtility.ExitGUI();
                    #endif
                }
                #endregion
            }

            #endregion

            #region Filters

            StickyEditorHelper.DrawArray(factionsToFilterProp, factionsToFilterContent, defaultEditorLabelWidth, "Faction");
            StickyEditorHelper.DrawArray(modelsToFilterProp, modelsToFilterContent, defaultEditorLabelWidth, "Model");

            #endregion

            EditorGUILayout.EndVertical();
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

        #region Public Static Methods

        // Add a menu item so that a StickyZone can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Zone (Sphere)")]
        public static StickyZone CreateStickyZoneSphere()
        {
            StickyZone stickyZone = null;

            // Create a new gameobject
            GameObject stickyZoneObj = new GameObject("StickyZone (Sphere)");
            if (stickyZoneObj != null)
            {
                SphereCollider zoneCollider = stickyZoneObj.AddComponent<SphereCollider>();

                if (zoneCollider != null)
                {
                    zoneCollider.isTrigger = true;

                    stickyZone = stickyZoneObj.AddComponent<StickyZone>();

                    #if UNITY_EDITOR
                    if (stickyZone == null)
                    {
                        Debug.LogWarning("ERROR: StickyZone.CreateStickyZoneSphere could not add StickyZone component to " + stickyZoneObj.name);
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: StickyZone.CreateStickyZoneSphere could not add a sphere collider to " + stickyZoneObj.name);
                }
                #endif
            }

            return stickyZone;
        }

        // Add a menu item so that a StickyZone can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Zone (Box)")]
        public static StickyZone CreateStickyZoneBox()
        {
            StickyZone stickyZone = null;

            // Create a new gameobject
            GameObject stickyZoneObj = new GameObject("StickyZone (Box)");
            if (stickyZoneObj != null)
            {
                BoxCollider zoneCollider = stickyZoneObj.AddComponent<BoxCollider>();

                if (zoneCollider != null)
                {
                    zoneCollider.isTrigger = true;

                    stickyZone = stickyZoneObj.AddComponent<StickyZone>();

                    #if UNITY_EDITOR
                    if (stickyZone == null)
                    {
                        Debug.LogWarning("ERROR: StickyZone.CreateStickyZoneBox could not add StickyZone component to " + stickyZoneObj.name);
                    }
                    #endif
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: StickyZone.CreateStickyZoneBox could not add a box collider to " + stickyZoneObj.name);
                }
                #endif
            }

            return stickyZone;
        }

        #endregion
    }
}