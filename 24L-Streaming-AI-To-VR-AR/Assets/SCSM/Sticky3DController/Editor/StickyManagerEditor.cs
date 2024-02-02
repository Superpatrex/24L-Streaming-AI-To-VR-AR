using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyManager))]
    public class StickyManagerEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables

        private StickyManager stickyManager = null;
        private bool isDebuggingEnabled = false;

        // Formatting and style variables
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle buttonCompactBoldBlue;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        //private int selectedTabInt = 0;
        private GUIStyle searchTextFieldStyle = null;
        private GUIStyle searchCancelButtonStyle = null;
        private float defaultSearchTextFieldWidth = 200f;
        private Color separatorColor = new Color();

        [System.NonSerialized] private List<S3DGenericObjectTemplate> genericObjectTemplatesList;
        [System.NonSerialized] private List<S3DBeamTemplate> beamTemplatesList;
        [System.NonSerialized] private List<S3DDecalTemplate> decalTemplatesList;
        [System.NonSerialized] private List<S3DDynamicObjectTemplate> dynamicObjectTemplatesList;
        [System.NonSerialized] private List<S3DEffectsObjectTemplate> effectsObjectTemplatesList;
        [System.NonSerialized] private List<S3DProjectileTemplate> projectileTemplatesList;

        #endregion

        #region GUIContent - General
        private readonly static GUIContent headerContent = new GUIContent("The Manager is automatically added to " +
                        "the scene if it doesn't already exist at runtime. It includes an object management or pooling system.");

        private readonly static GUIContent ammoTypesContent = new GUIContent(" Available Ammo Types", "This is Scriptable Object containing a list of 26 Ammo types. To create custom types, in the Project pane, click Create->Sticky3D->Ammo Types.");
        private readonly static GUIContent startupBeamModuleListContent = new GUIContent("Startup Beam Modules", "The list of beam modules added to the pool at start-up. Beam Modules can also be added via code.");
        private readonly static GUIContent startupDecalsListContent = new GUIContent("Startup Decal Sets", "The list of decal sets added to the pool at start-up. Decals can also be added via code. Each set can contain multiple StickyDecalModule prefabs.");
        private readonly static GUIContent startupDynamicModuleListContent = new GUIContent("Startup Dynamic Modules", "The list of dynamic modules added to the pool at start-up. Dynamic objects can also be added via code.");
        private readonly static GUIContent startupEffectsModuleListContent = new GUIContent("Startup Effects Modules", "The list of effects modules added to the pool at start-up. Effects can also be added via code.");
        private readonly static GUIContent startupGenericModuleListContent = new GUIContent("Startup Generic Modules", "The list of generic modules added to the pool at start-up. Generic Modules can also be added via code.");
        private readonly static GUIContent startupProjectileModuleListContent = new GUIContent("Startup Projectile Modules", "The list of projectile modules added to the pool at start-up. Projectile Modules can also be added via code.");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display pool sizes at runtime in the editor. This can help optimise the min/max pool size values.");
        private readonly static GUIContent beamHeaderContent = new GUIContent("Beam Templates", "The Beam Templates that are pooled");
        private readonly static GUIContent decalHeaderContent = new GUIContent("Decal Templates", "The Decal Templates that are pooled");
        private readonly static GUIContent dynamicHeaderContent = new GUIContent("Dynamic Templates", "The Dynamic Templates that are pooled");
        private readonly static GUIContent effectsHeaderContent = new GUIContent("Effects Templates", "The Effect Templates that are pooled");
        private readonly static GUIContent genericHeaderContent = new GUIContent("Generic Templates", "The Generic Object Templates that are pooled");
        private readonly static GUIContent projectileHeaderContent = new GUIContent("Projectile Templates", "The Projectile Templates that are pooled");
        private readonly static GUIContent minmaxPoolSizeContent = new GUIContent("Min/Max", "The minimum and maximum size of the pool");
        private readonly static GUIContent beamPoolingContent = new GUIContent(" Current Pool Size", "The current size of the beam pool");
        private readonly static GUIContent decalPoolingContent = new GUIContent(" Current Pool Size", "The current size of the decal pool");
        private readonly static GUIContent dynamicPoolingContent = new GUIContent(" Current Pool Size", "The current size of the dynamic pool");
        private readonly static GUIContent effectsPoolingContent = new GUIContent(" Current Pool Size", "The current size of the effects pool");
        private readonly static GUIContent genericPoolingContent = new GUIContent(" Current Pool Size", "The current size of the generic pool");
        private readonly static GUIContent projectilePoolingContent = new GUIContent(" Current Pool Size", "The current size of the projectile pool");
        #endregion

        #region Properties
        private SerializedProperty ammoTypesProp;
        private SerializedProperty startupBeamModuleListProp;
        private SerializedProperty startupDecalsListProp;
        private SerializedProperty startupDynamicModuleListProp;
        private SerializedProperty startupEffectsModuleListProp;
        private SerializedProperty startupGenericModuleListProp;
        private SerializedProperty startupProjectileModuleListProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            stickyManager = (StickyManager)target;

            #region Reset guistyles to avoid issues
            // forces reinitialisation of button styles etc
            helpBoxRichText = null;
            labelFieldRichText = null;
            buttonCompact = null;
            buttonCompactBoldBlue = null;
            foldoutStyleNoLabel = null;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            searchTextFieldStyle = null;
            searchCancelButtonStyle = null;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            #endregion

            #region Find Properties
            ammoTypesProp = serializedObject.FindProperty("ammoTypes");
            startupBeamModuleListProp = serializedObject.FindProperty("startupBeamModuleList");
            startupDecalsListProp = serializedObject.FindProperty("startupDecalsList");
            startupDynamicModuleListProp = serializedObject.FindProperty("startupDynamicModuleList");
            startupEffectsModuleListProp = serializedObject.FindProperty("startupEffectsModuleList");
            startupGenericModuleListProp = serializedObject.FindProperty("startupGenericModuleList");
            startupProjectileModuleListProp = serializedObject.FindProperty("startupProjectileModuleList");
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
            if (helpBoxRichText == null)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;
            }

            if (labelFieldRichText == null)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;
            }

            if (buttonCompact == null)
            {
                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;
            }

            if (buttonCompactBoldBlue == null)
            {
                buttonCompactBoldBlue = new GUIStyle("Button");
                buttonCompactBoldBlue.fontSize = 10;
                buttonCompactBoldBlue.fontStyle = FontStyle.Bold;
                buttonCompactBoldBlue.normal.textColor = Color.blue;
            }

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }

            // Set up the toggle buttons styles
            if (toggleCompactButtonStyleNormal == null)
            {
                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleNormal.fontSize = 10;
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }

            if (searchTextFieldStyle == null)
            {
                #if UNITY_2022_3_OR_NEWER
                searchTextFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField);
                #else
                searchTextFieldStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
                #endif
                searchTextFieldStyle.stretchHeight = false;
                searchTextFieldStyle.stretchWidth = false;
                searchTextFieldStyle.fontSize = 10;
                searchTextFieldStyle.fixedHeight = 16f;
                searchTextFieldStyle.fixedWidth = defaultSearchTextFieldWidth;
            }

            if (searchCancelButtonStyle == null)
            {
                #if UNITY_2022_3_OR_NEWER
                searchCancelButtonStyle =  new GUIStyle(S3DUtils.ReflectionGetPropertyValue<GUIStyle>(typeof(EditorStyles), "toolbarSearchFieldCancelButton", null, true, true));
                #else
                searchCancelButtonStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));
                #endif
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            StickyEditorHelper.DrawStickyVersionLabel(labelFieldRichText);

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            StickyEditorHelper.DrawGetHelpButtons(buttonCompact);

            // TAB CONTROL GOES HERE

            GUILayout.EndVertical();

            StickyEditorHelper.InTechPreview(false);
            #endregion

            #region General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (stickyManager.AmmoTypes == null) { StickyEditorHelper.NoAmmoTypesAssigned(); }

            EditorGUILayout.PropertyField(ammoTypesProp, ammoTypesContent);

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(startupBeamModuleListProp, startupBeamModuleListContent);
            EditorGUILayout.PropertyField(startupDecalsListProp, startupDecalsListContent);
            EditorGUILayout.PropertyField(startupDynamicModuleListProp, startupDynamicModuleListContent);
            EditorGUILayout.PropertyField(startupEffectsModuleListProp, startupEffectsModuleListContent);
            EditorGUILayout.PropertyField(startupProjectileModuleListProp, startupProjectileModuleListContent);
            EditorGUILayout.PropertyField(startupGenericModuleListProp, startupGenericModuleListContent);
            EditorGUI.indentLevel -= 1;
            GUILayout.EndVertical();
            #endregion

            #region Debug Mode
            // Place above lists of items (to be added in future version like SSC) etc so it is not affected by ScrollView when not many items.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);

            if (isDebuggingEnabled && stickyManager != null && stickyManager.IsInitialised)
            {
                #region Beam Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(beamHeaderContent, labelFieldRichText);

                // Display the size of the current pool for beams
                beamTemplatesList = stickyManager.BeamTemplatesList;
                int numBeamTemplates = beamTemplatesList == null ? 0 : beamTemplatesList.Count;

                if (numBeamTemplates > 0)
                {
                    foreach (S3DBeamTemplate beamTemplate in beamTemplatesList)
                    {
                        if (beamTemplate.s3dBeamModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(beamTemplate.s3dBeamModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(beamPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(beamTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(beamTemplate.s3dBeamModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(beamTemplate.s3dBeamModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                #endregion

                #region Decal Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(decalHeaderContent, labelFieldRichText);
                // Display the size of the current pool for decal objects
                decalTemplatesList = stickyManager.DecalTemplatesList;
                int numDecalTemplates = decalTemplatesList == null ? 0 : decalTemplatesList.Count;

                if (numDecalTemplates > 0)
                {
                    foreach (S3DDecalTemplate decalTemplate in decalTemplatesList)
                    {
                        if (decalTemplate.s3dDecalModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(decalTemplate.s3dDecalModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(decalPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(decalTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(decalTemplate.s3dDecalModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(decalTemplate.s3dDecalModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                #endregion

                #region Dynamic Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(dynamicHeaderContent, labelFieldRichText);
                // Display the size of the current pool for dynamic objects
                dynamicObjectTemplatesList = stickyManager.DynamicObjectTemplatesList;
                int numDynamicTemplates = dynamicObjectTemplatesList == null ? 0 : dynamicObjectTemplatesList.Count;

                if (numDynamicTemplates > 0)
                {
                    foreach (S3DDynamicObjectTemplate dynamicTemplate in dynamicObjectTemplatesList)
                    {
                        if (dynamicTemplate.s3dDynamicModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(dynamicTemplate.s3dDynamicModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(dynamicPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(dynamicTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(dynamicTemplate.s3dDynamicModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(dynamicTemplate.s3dDynamicModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                #endregion

                #region Effects Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(effectsHeaderContent, labelFieldRichText);
                // Display the size of the current pool for effects objects
                effectsObjectTemplatesList = stickyManager.EffectsObjectTemplatesList;
                int numEffectsTemplates = effectsObjectTemplatesList == null ? 0 : effectsObjectTemplatesList.Count;

                if (numEffectsTemplates > 0)
                {
                    foreach (S3DEffectsObjectTemplate effectsTemplate in effectsObjectTemplatesList)
                    {
                        if (effectsTemplate.s3dEffectsModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(effectsTemplate.s3dEffectsModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(effectsPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(effectsTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(effectsTemplate.s3dEffectsModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(effectsTemplate.s3dEffectsModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                #endregion

                #region Projectile Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(projectileHeaderContent, labelFieldRichText);

                // Display the size of the current pool for projectiles
                projectileTemplatesList = stickyManager.ProjectileTemplatesList;
                int numProjectileTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;

                if (numProjectileTemplates > 0)
                {
                    foreach (S3DProjectileTemplate projectileTemplate in projectileTemplatesList)
                    {
                        if (projectileTemplate.s3dProjectileModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(projectileTemplate.s3dProjectileModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(projectilePoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(projectileTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(projectileTemplate.s3dProjectileModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(projectileTemplate.s3dProjectileModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                #endregion

                #region Generic Templates
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);

                EditorGUILayout.LabelField(genericHeaderContent, labelFieldRichText);

                // Display the size of the current pool for generic objects
                genericObjectTemplatesList = stickyManager.GenericObjectTemplatesList;
                int numGenericObjectTemplates = genericObjectTemplatesList == null ? 0 : genericObjectTemplatesList.Count;

                if (numGenericObjectTemplates > 0)
                {
                    foreach (S3DGenericObjectTemplate genericObjectTemplate in genericObjectTemplatesList)
                    {
                        if (genericObjectTemplate.s3dGenericModulePrefab != null)
                        {
                            EditorGUILayout.LabelField(genericObjectTemplate.s3dGenericModulePrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(genericPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(genericObjectTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(genericObjectTemplate.s3dGenericModulePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(genericObjectTemplate.s3dGenericModulePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                #endregion
            }
            EditorGUILayout.EndVertical();
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyManager can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Manager")]
        public static StickyManager CreateStickyManager()
        {
            return StickyManager.GetOrCreateManager();
        }

        #endregion
    }
}