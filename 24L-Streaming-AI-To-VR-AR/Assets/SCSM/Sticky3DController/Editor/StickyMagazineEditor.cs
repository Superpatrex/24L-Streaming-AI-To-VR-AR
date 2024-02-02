using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{

    /// <summary>
    /// The custom inspector for the StickyMagazine class
    /// </summary>
    [CustomEditor(typeof(StickyMagazine))]
    [HelpURL("http://scsmmedia.com/media/s3d_manual.pdf")]
    public class StickyMagazineEditor : StickyInteractiveEditor
    {
        #region Custom Editor private variables
        private StickyMagazine stickyMagazine = null;
        private string[] ammoTypeNames = null;
        private string[] magTypeNames = null;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("The interactive-enabled object that holds ammo for a weapon");
        private readonly static GUIContent[] tabTexts = { new GUIContent("Interactive"), new GUIContent("Magazine"), new GUIContent("Events") };
        #endregion

        #region GUIContent - Magazine
        private readonly static GUIContent generalSettingsContent = new GUIContent(" General Settings");
        private readonly static GUIContent magCapacityContent = new GUIContent(" Mag Capacity", "The amount of ammo the magazine can hold");
        private readonly static GUIContent ammoTypesContent = new GUIContent(" Available Ammo Types", "This is Scriptable Object containing a list of 26 Ammo types. To create custom types, in the Project pane, click Create->Sticky3D->Ammo Types.");
        private readonly static GUIContent ammoTypeContent = new GUIContent(" Ammo Type", "The ammo type that can be placed in this magazine");
        private readonly static GUIContent magTypesContent = new GUIContent(" Available Mag Types", "This is Scriptable Object containing a list of 26 Magazine types. To create custom types, in the Project pane, click Create->Sticky3D->Magazine Types.");
        private readonly static GUIContent magTypeContent = new GUIContent(" Mag Type", "The type of magazine. All magazines of the same type can be used with compatible weapons.");
        private readonly static GUIContent ammoCountContent = new GUIContent(" Ammo Count", "The amount of ammo currently in the magazine. It cannot exceed the Mag Capacity.");
        private readonly static GUIContent isDisableRegularColOnEquipContent = new GUIContent(" Disable Regular Colliders", "Disable regular colliders when equipped (attached) to a weapon.");
        #endregion

        #region Serialized Properties - Magazine
        private SerializedProperty showGeneralSettingsInEditorProp;
        private SerializedProperty showGravitySettingsInEditorProp;
        private SerializedProperty magCapacityProp;
        private SerializedProperty ammoTypesProp;
        private SerializedProperty compatibleAmmoTypeProp;
        private SerializedProperty magTypesProp;
        private SerializedProperty magTypeProp;
        private SerializedProperty ammoCountProp;
        private SerializedProperty isDisableRegularColOnEquipProp;

        #endregion

        #region Events

        protected override void OnEnable()
        {
            base.OnEnable();

            stickyMagazine = (StickyMagazine)target;

            #region Find Properties - Magazine
            showGeneralSettingsInEditorProp = serializedObject.FindProperty("showGeneralSettingsInEditor"); 
            magCapacityProp = serializedObject.FindProperty("magCapacity");
            ammoTypesProp = serializedObject.FindProperty("ammoTypes");
            compatibleAmmoTypeProp = serializedObject.FindProperty("compatibleAmmoType");
            magTypesProp = serializedObject.FindProperty("magTypes");
            magTypeProp = serializedObject.FindProperty("magType");
            ammoCountProp = serializedObject.FindProperty("ammoCount");
            isDisableRegularColOnEquipProp = serializedObject.FindProperty("isDisableRegularColOnEquip");
            #endregion
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Draw the ammo count in the inspector.
        /// </summary>
        protected void DrawAmmoCount()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ammoCountProp, ammoCountContent);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyMagazine.AmmoCount = ammoCountProp.intValue;
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw the ammo type in the inspector
        /// </summary>
        protected void DrawAmmoType()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ammoTypeContent, GUILayout.Width(100f));

            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(defaultEditorLabelWidth - 103f)))
            {
                ammoTypeNames = null;
            }

            if (ammoTypeNames == null)
            {
                ammoTypeNames = S3DAmmoTypes.GetAmmoTypeNames(stickyMagazine.AmmoTypes, true);
            }

            EditorGUI.BeginChangeCheck();
            compatibleAmmoTypeProp.intValue = EditorGUILayout.Popup(compatibleAmmoTypeProp.intValue, ammoTypeNames);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyMagazine.CompatibleAmmoType =  (S3DAmmo.AmmoType)compatibleAmmoTypeProp.intValue;
                serializedObject.Update();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the ammo types scriptable object in the inspector
        /// </summary>
        protected void DrawAmmoTypes()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ammoTypesProp, ammoTypesContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                ammoTypeNames = null;
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw equip settings in the inspector
        /// </summary>
        protected void DrawEquipSettings()
        {
            EditorGUILayout.PropertyField(isDisableRegularColOnEquipProp, isDisableRegularColOnEquipContent);
        }

        /// <summary>
        /// Draw the magazine capacity in the inspector.
        /// </summary>
        protected void DrawMagCapacity()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(magCapacityProp, magCapacityContent);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                stickyMagazine.MagCapacity = magCapacityProp.intValue;
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Draw the mag type in the inspector
        /// </summary>
        protected void DrawMagType()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(magTypeContent, GUILayout.Width(100f));

            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(defaultEditorLabelWidth - 103f)))
            {
                magTypeNames = null;
            }

            if (magTypeNames == null)
            {
                magTypeNames = S3DMagTypes.GetMagTypeNames(stickyMagazine.MagTypes, true);
            }

            magTypeProp.intValue = EditorGUILayout.Popup(magTypeProp.intValue, magTypeNames);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw the mag types scriptable object in the inspector
        /// </summary>
        protected void DrawMagTypes()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(magTypesProp, magTypesContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Force the names to be repopulated
                serializedObject.ApplyModifiedProperties();
                magTypeNames = null;
                serializedObject.Update();
            }
        }

        #endregion

        #region DrawBaseInspector

        protected override void DrawBaseInspector()
        {
            #region Initialise
            stickyInteractive.allowRepaint = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            isSceneModified = false;
            #endregion

            ConfigureButtonsAndStyles();

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            // Draw the toolbar using the tabTexts from this editor class (rather than the base class).
            DrawToolBar(tabTexts);
            #endregion

            EditorGUILayout.BeginVertical("HelpBox");

            #region Interactive Settings

            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.PropertyField(initialiseOnStartProp, initialiseOnStartContent);

                DrawEquippable();
                DrawGrabbable();
                DrawSocketable();
                DrawStashable();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawHandHoldSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawPopupSettings();

                StickyEditorHelper.DrawUILine(separatorColor, 2);
                DrawTagSettings();

                DrawGravitySettings();
            }
            #endregion

            #region Magazine Settings
            else if (selectedTabIntProp.intValue == 1)
            {
                #region General Settings
                StickyEditorHelper.DrawUILine(separatorColor, 2, 6);
                StickyEditorHelper.DrawS3DFoldout(showGeneralSettingsInEditorProp,generalSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);

                if (stickyMagazine.MagTypes == null) { StickyEditorHelper.NoMagTypesAssigned(); }
                if (stickyMagazine.AmmoTypes == null) { StickyEditorHelper.NoAmmoTypesAssigned(); }

                if (showGeneralSettingsInEditorProp.boolValue)
                {
                    DrawAmmoTypes();
                    DrawMagTypes();

                    DrawMagCapacity();

                    DrawMagType();

                    DrawAmmoType();
                    DrawAmmoCount();

                    DrawEquipSettings();
                }
                #endregion
            }
            #endregion

            #region Events
            else
            {
                DrawBaseEvents();
            }
            #endregion

            EditorGUILayout.EndVertical();

            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            CheckMarkSceneDirty();

            stickyInteractive.allowRepaint = true;

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawDebugToggle();
            if (isDebuggingEnabled && stickyMagazine != null)
            {
                Repaint();
                float rightLabelWidth = 175f;

                StickyEditorHelper.PerformanceImpact();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(StickyEditorHelper.debugIsInitialisedIndent1Content, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                EditorGUILayout.LabelField(stickyMagazine.IsMagazineInitialised ? "Yes" : "No", GUILayout.MaxWidth(defaultEditorFieldWidth));
                EditorGUILayout.EndHorizontal();

                DrawDebugHeldBy(10f, rightLabelWidth);
                DrawDebugSocketedOn(10f, rightLabelWidth);
                DrawDebugStashedBy(10f, rightLabelWidth);

                DrawDebugReferenceFrame(rightLabelWidth);
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        #endregion
    }
}