using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyBeamModule class
    /// </summary>
    [CustomEditor(typeof(StickyBeamModule))]
    public class StickyBeamModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyBeamModule stickyBeamModule = null;
        private bool isDebuggingEnabled = false;
        #endregion

        #region Static Strings
        private readonly static string effectsDespawnWarning = "The Despawn Time of the Effects Object is less than the Max Burst Duration.";
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A poolable beam that can be fired from a StickyWeapon");
        #endregion

        #region GUIContent - Beam
        private readonly static GUIContent damageTypeContent = new GUIContent(" Damage Type", "The type of damage the beam does when hitting a character. " +
            "The amount of damage dealt to a S3D upon collision is dependent on the character's resistance to this damage type. If the " +
            "damage type is set to Default, the character's damage multipliers are ignored i.e. the damage amount is unchanged.");
        private readonly static GUIContent damageRateContent = new GUIContent(" Damage Rate", "The amount of damage this beam does, per second, to the character or object it hits. NOTE: Non-character objects need a StickyDamageReceiver component.");

        private readonly static GUIContent speedContent = new GUIContent(" Speed", "The speed the beam travels in metres per second");
        private readonly static GUIContent beamStartWidthContent = new GUIContent(" Beam Width", "The (visual) width of the beam in metres");
        private readonly static GUIContent minBurstDurationContent = new GUIContent(" Min Burst Duration", "The minimum amount of time, in seconds, the beam must be active");
        private readonly static GUIContent maxBurstDurationContent = new GUIContent(" Max Burst Duration", "The maximum amount of time, in seconds, the beam can be active in a single burst");
        private readonly static GUIContent dischargeDurationContent = new GUIContent(" Discharge Duration", "The time (in seconds) it takes a single beam to discharge the beam weapon from full charge");
        private readonly static GUIContent effectsObjectContent = new GUIContent(" Effects Object", "The default particle and/or sound effect prefab that will be instantiated when the beam hits something. This does not fire when the beam is automatically despawned. For beams, this would typically be a looping effect.");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugSourceStickyIdContent = new GUIContent(" Source Sticky Id", "The Sticky Id of the character that fired this beam");
        private readonly static GUIContent debugSourceFactionIdContent = new GUIContent(" Source Faction Id", "The faction or alliance of the character that fired the beam belongs to.");
        private readonly static GUIContent debugSourceModelIdContent = new GUIContent(" Source Model Id", "The type, category, or model of the character that fired the beam.");
        #endregion

        #region Serialized Properties - Beam
        private SerializedProperty beamStartWidthProp;
        private SerializedProperty speedProp;
        private SerializedProperty damageTypeProp;
        private SerializedProperty damageRateProp;
        private SerializedProperty minBurstDurationProp;
        private SerializedProperty maxBurstDurationProp;
        private SerializedProperty dischargeDurationProp;
        private SerializedProperty effectsObjectProp;
        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyBeamModule = (StickyBeamModule)target;

            #region Find Properties - Beam
            beamStartWidthProp = serializedObject.FindProperty("beamStartWidth");
            speedProp = serializedObject.FindProperty("speed");
            damageTypeProp = serializedObject.FindProperty("damageType");
            damageRateProp = serializedObject.FindProperty("damageRate");
            minBurstDurationProp = serializedObject.FindProperty("minBurstDuration");
            maxBurstDurationProp = serializedObject.FindProperty("maxBurstDuration");
            dischargeDurationProp = serializedObject.FindProperty("dischargeDuration");
            effectsObjectProp = serializedObject.FindProperty("effectsObject");
            #endregion
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Draw the default Effects Object in the inspector
        /// </summary>
        protected void DrawDefaultHitFX()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(effectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosEffectsFolder, false, true); }
            EditorGUILayout.PropertyField(effectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        #endregion

        #region DrawBaseInspector

        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyBeamModule.allowRepaint = false;
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

            // Beams get parented to weapons. If this is not enabled, it won't get
            // re-parented to the pooling system transform when it is returned to the pool.
            if (!isReparentedProp.boolValue)
            {
                EditorGUILayout.HelpBox("Is Reparented must be enabled for Beams", MessageType.Error);
            }

            EditorGUILayout.PropertyField(speedProp, speedContent);
            EditorGUILayout.PropertyField(damageTypeProp, damageTypeContent);
            EditorGUILayout.PropertyField(damageRateProp, damageRateContent);

            // Draw the common generic object settings without the despawn time
            DrawMinMaxPoolSize();
            DrawIsReparented();

            EditorGUILayout.PropertyField(beamStartWidthProp, beamStartWidthContent);
            EditorGUILayout.PropertyField(minBurstDurationProp, minBurstDurationContent);
            EditorGUILayout.PropertyField(maxBurstDurationProp, maxBurstDurationContent);
            if (minBurstDurationProp.floatValue > maxBurstDurationProp.floatValue) { maxBurstDurationProp.floatValue = minBurstDurationProp.floatValue; }

            if (dischargeDurationProp.floatValue < 0.1f) { dischargeDurationProp.floatValue = 0.1f; }
            EditorGUILayout.PropertyField(dischargeDurationProp, dischargeDurationContent);

            if (stickyBeamModule.effectsObject != null)
            {
                if (stickyBeamModule.effectsObject.despawnTime < stickyBeamModule.maxBurstDuration)
                {
                    EditorGUILayout.HelpBox(effectsDespawnWarning, MessageType.Info);
                }
            }

            DrawDefaultHitFX();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyBeamModule != null)
            {
                float rightLabelWidth = 150f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyBeamModule.IsBeamEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceStickyIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyBeamModule.sourceStickyId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceFactionIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyBeamModule.sourceFactionId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceModelIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyBeamModule.sourceModelId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyBeamModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyBeamModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Beam")]
        public static StickyBeamModule CreateStickyBeam()
        {
            StickyBeamModule stickyBeamModule = null;

            // Create a new gameobject
            GameObject stickyBeamObj = new GameObject("StickyBeam");

            if (stickyBeamObj != null)
            {
                GameObject lrenObj = new GameObject("Beam");

                if (lrenObj != null)
                {
                    lrenObj.transform.SetParent(stickyBeamObj.transform);
                    LineRenderer lineRenderer = lrenObj.AddComponent<LineRenderer>();

                    if (lineRenderer != null)
                    {
                        lineRenderer.receiveShadows = false;
                        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        lineRenderer.loop = false;

                        /// TODO - cater for HDRP and URP
                        lineRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                        lineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                        lineRenderer.alignment = LineAlignment.View;
                        lineRenderer.startColor = Color.white;
                        lineRenderer.endColor = Color.white;

                        lineRenderer.sharedMaterial = StickyEditorHelper.GetDefaultMaterial("Default-Line.mat");

                        // Our calculations in weapon.MoveBeam(..) assume world-space positions
                        lineRenderer.useWorldSpace = true;

                        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero} );

                        stickyBeamModule = stickyBeamObj.AddComponent<StickyBeamModule>();

                        if (stickyBeamModule != null)
                        {
                            // Add any other config we need to do here
                            stickyBeamModule.isReparented = true;
                        }
                        #if UNITY_EDITOR
                        else
                        {
                            Debug.LogWarning("ERROR: StickyBeamModule.CreateStickyBeam could not add StickyBeamModule component to " + stickyBeamObj.name);
                        }
                        #endif
                    }
                    #if UNITY_EDITOR
                    else
                    {
                        Debug.LogWarning("ERROR: StickyBeamModule.CreateStickyBeam could not add LineRender component to " + stickyBeamObj.name);
                    }
                    #endif
                }
            }

            return stickyBeamModule;
        }
        #endregion
    }
}