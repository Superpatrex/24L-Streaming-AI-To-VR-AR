using UnityEngine;
using UnityEditor;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// The custom inspector for the StickyProjectileModule class
    /// </summary>
    [CustomEditor(typeof(StickyProjectileModule))]
    public class StickyProjectileModuleEditor : StickyGenericModuleEditor
    {
        #region Custom Editor private variables
        private StickyProjectileModule stickyProjectileModule = null;
        private bool isDebuggingEnabled = false;
        #endregion

        #region Static Strings
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("A poolable projectile that can be fired from a StickyWeapon");
        #endregion

        #region GUIContent - Projectile
        private readonly static GUIContent damageTypeContent = new GUIContent(" Damage Type", "The type of damage the projectile does when hitting a character. " +
            "The amount of damage dealt to a S3D upon collision is dependent on the character's resistance to this damage type. If the " +
            "damage type is set to Default, the character's damage multipliers are ignored i.e. the damage amount is unchanged.");
        private readonly static GUIContent damageAmountContent = new GUIContent(" Damage Amount", "The amount of damage this projectile does to the character or object it hits. NOTE: Non-character objects need a Sticky Damage Receiver component.");
        private readonly static GUIContent impactForceContent = new GUIContent(" Impact Force", "The amount of force that is applied when hitting a rigidbody at the point of impact. NOTE: Non-character objects need a Sticky Damage Receiver component.");

        private readonly static GUIContent startSpeedContent = new GUIContent(" Start Speed", "The speed the projectile starts traveling in metres per second. Is Ignored for Raycast weapons.");
        private readonly static GUIContent modelRotationOffsetContent = new GUIContent(" Model Rotation", "The projectile should point forward on the z-axis. If it, say, points upward (y-axis), it needs to be rotated 90 degrees here around the x-axis.");
        
        private readonly static GUIContent effectsObjectContent = new GUIContent(" Effects Object", "The default particle and/or sound effect prefab that will be instantiated when the projectile hits something. This would typically be a non-looping effect.");
        private readonly static GUIContent s3dDecalsContent = new GUIContent(" Default Decal Set", "The default set of Sticky Decal Module prefabs used to select a random decal when the projectile hits an object.");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        private readonly static GUIContent gotoDecalFolderBtnContent = new GUIContent("F", "Find and highlight the sample Decals folder");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugSourceStickyIdContent = new GUIContent(" Source Sticky Id", "The Sticky Id of the character that fired this projectile");
        private readonly static GUIContent debugSourceFactionIdContent = new GUIContent(" Source Faction Id", "The faction or alliance of the character that fired the projectile belongs to.");
        private readonly static GUIContent debugSourceModelIdContent = new GUIContent(" Source Model Id", "The type, category, or model of the character that fired the projectile.");
        #endregion

        #region Serialized Properties - Projectile
        private SerializedProperty startSpeedProp;
        private SerializedProperty damageTypeProp;
        private SerializedProperty damageAmountProp;
        private SerializedProperty impactForceProp;
        private SerializedProperty modelRotationOffsetProp;
        private SerializedProperty effectsObjectProp;
        private SerializedProperty s3dDecalsProp;
        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyProjectileModule = (StickyProjectileModule)target;

            #region Find Properties - Beam
            startSpeedProp = serializedObject.FindProperty("startSpeed");
            damageTypeProp = serializedObject.FindProperty("damageType");
            damageAmountProp = serializedObject.FindProperty("damageAmount");
            impactForceProp = serializedObject.FindProperty("impactForce");
            modelRotationOffsetProp = serializedObject.FindProperty("modelRotationOffset");
            effectsObjectProp = serializedObject.FindProperty("effectsObject");
            s3dDecalsProp = serializedObject.FindProperty("s3dDecals");
            #endregion
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Draw the basic set of properties in the inspector
        /// </summary>
        protected void DrawBasicProperties()
        {
            EditorGUILayout.PropertyField(startSpeedProp, startSpeedContent);
            EditorGUILayout.PropertyField(damageTypeProp, damageTypeContent);
            EditorGUILayout.PropertyField(damageAmountProp, damageAmountContent);
            EditorGUILayout.PropertyField(impactForceProp, impactForceContent);
            EditorGUILayout.PropertyField(modelRotationOffsetProp, modelRotationOffsetContent);
        }

        /// <summary>
        /// Draw the default set of Decals in the inspector
        /// </summary>
        protected void DrawDefaultHitDecals()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(s3dDecalsContent, GUILayout.Width(defaultEditorLabelWidth - 24f));
            if (GUILayout.Button(gotoDecalFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.demosDecalsFolder, false, true); }
            EditorGUILayout.PropertyField(s3dDecalsProp, GUIContent.none);
            GUILayout.EndHorizontal();
        }

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
            stickyProjectileModule.allowRepaint = false;
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

            DrawBasicProperties();

            // Draw the common generic object settings without the despawn time
            DrawMinMaxPoolSize();
            DrawIsReparented();
            DrawDespawnTIme();

            DrawDefaultHitFX();
            DrawDefaultHitDecals();

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && stickyProjectileModule != null)
            {
                float rightLabelWidth = 150f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyProjectileModule.IsProjectileEnabled ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceStickyIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyProjectileModule.sourceStickyId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceFactionIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyProjectileModule.sourceFactionId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugSourceModelIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(stickyProjectileModule.sourceModelId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion

            stickyProjectileModule.allowRepaint = true;
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyProjectileModule can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Projectile")]
        public static StickyProjectileModule CreateStickyProjectile()
        {
            StickyProjectileModule stickyProjectileModule = null;

            // Create a new gameobject
            GameObject stickyProjectileObj = new GameObject("StickyProjectile");

            if (stickyProjectileObj != null)
            {

                stickyProjectileModule = stickyProjectileObj.AddComponent<StickyProjectileModule>();

                if (stickyProjectileModule != null)
                {
                    // Add any other config we need to do here
                    stickyProjectileModule.isReparented = true;
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: StickyProjectileModule.CreateStickyProjectile could not add StickyProjectileModule component to " + stickyProjectileObj.name);
                }
                #endif
            }

            return stickyProjectileModule;
        }

        #endregion
    }
}