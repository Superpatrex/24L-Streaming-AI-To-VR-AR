using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(SurfaceTurretModule))]
    public class SurfaceTurretModuleEditor : Editor
    {
        #region Enumaerations
        /// <summary>
        /// This is a subset of Weapon.WeaponType
        /// Only used in the editor
        /// </summary>
        public enum SurfaceWeaponType
        {
            TurretProjectile = 10,
            TurretBeam = 11
        }
        #endregion

        #region Custom Editor private variables
        private SurfaceTurretModule surfaceTurretModule;
        private bool isSceneModified = false;

        // formatting and style variables
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private SSCRadar sscRadar = null;
        private int prevWeaponType = 0;

        #endregion

        #region SceneView Variables

        private bool isSceneDirtyRequired = false;
        // Pale Green
        private Color weaponGizmoColour = new Color(152f / 255f, 251f / 255f, 152f / 255f, 1f);

        // Yellow for Y axis rotation arc
        private Color weaponGizmoTurretYColour = new Color(1f, 0.92f, 0.016f, 0.7f);

        // Red for X axis rotation arc
        private Color weaponGizmoTurretXColour = new Color(1f, 0f, 0f, 0.4f);

        private Weapon weaponComponent;
        private Vector3 componentHandlePosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;
        private List<Vector3> weaponFirePositionList;
        private int numCompWeaponFirePositions;
        private Quaternion sceneViewSurfaceTurretRotation = Quaternion.identity;
        private float handleDistanceScale = 1f;

        #endregion

        #region GUIContent
        private readonly static GUIContent turretHeaderContent = new GUIContent("This is useful when you want a turret that is not on a ship, like on the surface of a planet or a floating platform. For ship turrets, always use Weapons on the Combat tab of the ShipControlModule.");
        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise On Start", "If enabled, the Initialise() will be called as soon as Start() runs. This should be disabled if you are initialising the turret through code and using the SurfaceTurretModule API methods.");
        private readonly static GUIContent factionIdContent = new GUIContent("Faction Id", "The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe. Default (neutral) is 0.");
        private readonly static GUIContent squadronIdContent = new GUIContent("Squadron Id", "Although normally representing a squadron of ships, this can be used on a turret to group it with other things in your scene. Default (unset) is -1.");
        private readonly static GUIContent autoCreateLocationContent = new GUIContent("Auto Create Location", "Automatically create a Location in the SSCManager when turret is initialised");
        private readonly static GUIContent isVisibleToRadarContent = new GUIContent("Is Visible to Radar", "Is this turret (Location) visible to radar queries?");
        private readonly static GUIContent radarBlipSizeContent = new GUIContent("Radar Blip Size", "The relative size of the blip on the radar mini-map.");
        private readonly static GUIContent isDestroyOnNoHealthContent = new GUIContent("Destroy on No Health", "Should the turret be destroyed (removed from the scene) when its health reaches 0?");
        private readonly static GUIContent destructionEffectsObjectContent = new GUIContent("Effects Object", "The particle and / or sound effect " +
            "prefab that will be instantiated when the turret is destroyed.");

        private readonly static GUIContent destructObjectContent = new GUIContent("Destruct Object", "The destruct prefab that breaks into fragments when the turret is destroyed.");

        private readonly static GUIContent gravityHeaderContent = new GUIContent("Gravitational acceleration and direction can affect how a projectile behaves after it has been fired from the weapon.");
        private readonly static GUIContent gravitationalAccelerationContent = new GUIContent("Acceleration", "The acceleration due to " +
            "gravity in metres per second squared. Earth gravity is approximately 9.81 m/s^2.");
        private readonly static GUIContent gravityDirectionContent = new GUIContent("Direction", "The direction in which gravity acts on the ship in world space.");

        private readonly static GUIContent weaponTypeContent = new GUIContent("Weapon Type", "Projectile or Beam weapon");
        private readonly static GUIContent weaponRelativePositionContent = new GUIContent("Relative Position", "The position of the weapon in local space relative to the pivot point of the whole turret.");
        private readonly static GUIContent weaponIsMultipleFirePositionsContent = new GUIContent("Multiple Fire Positions", "If this weapon has multiple cannons or barrels");
        private readonly static GUIContent weaponFirePositionContent = new GUIContent("Fire Position Offsets", "The positions of the cannon or barrel relative to the position of the weapon.");
        private readonly static GUIContent weaponFireDirectionContent = new GUIContent("Fire Direction", "The direction in which " +
            "the weapon fires projectiles in local space. +ve Z is fire forwards, -ve Z is fire backwards.");
        private readonly static GUIContent weaponProjectilePrefabContent = new GUIContent("Projectile Prefab", "Prefab template of the " +
            "projectiles fired by this weapon. Projectile prefabs need to have a Projectile Module script attached to them.");
        private readonly static GUIContent weaponBeamPrefabContent = new GUIContent("Beam Prefab", "Prefab template of the " +
            "beam fired by this weapon. Beam prefabs need to have a Beam Module script attached to them.");
        private readonly static GUIContent weaponPowerUpTimeContent = new GUIContent("Power-up Time", "The minimum time (in seconds) between consecutive firings of the beam weapon.");
        private readonly static GUIContent weaponMaxRangeContent = new GUIContent("Max Range", "The maximum distance (in metres) the beam weapon can fire.");
        private readonly static GUIContent weaponChargeAmountContent = new GUIContent("Charge Amount", "The amount of charge currently available for this weapon.");
        private readonly static GUIContent weaponRechargeTimeContent = new GUIContent("Recharge Time", "The time (in seconds) it takes the fully discharged weapon to reach maximum charge");
        private readonly static GUIContent weaponReloadTimeContent = new GUIContent("Reload Time", "The minimum time (in seconds) between consecutive firings of the weapon.");
        private readonly static GUIContent weaponFiringButtonContent = new GUIContent("Auto Fire", "When a target is selected, the weapon will automatically attempt to fire at the target.");
        private readonly static GUIContent weaponUnlimitedAmmoContent = new GUIContent("Unlimited Ammo", "Can this weapon keep firing and never run out of ammunition?");
        private readonly static GUIContent weaponAmmunitionContent = new GUIContent("Ammunition", "The quantity of projectiles or ammunition available for this weapon.");
        private readonly static GUIContent weaponTurretPivotYContent = new GUIContent("Turret Pivot Y", "The transform of the pivot point around which the turret turns on the local y-axis");
        private readonly static GUIContent weaponTurretPivotXContent = new GUIContent("Turret Pivot X", "The transform on which the barrel(s) or cannon(s) elevate up or down on the local x-axis");
        private readonly static GUIContent weaponTurretMinYContent = new GUIContent("Turret Min. Y", "The minimum angle on the local y-axis the turret can rotate to");
        private readonly static GUIContent weaponTurretMaxYContent = new GUIContent("Turret Max. Y", "The maximum angle on the local y-axis the turret can rotate to");
        private readonly static GUIContent weaponTurretMinXContent = new GUIContent("Turret Min. X", "The minimum angle on the local x-axis the turret can elevate to");
        private readonly static GUIContent weaponTurretMaxXContent = new GUIContent("Turret Max. X", "The maximum angle on the local x-axis the turret can elevate to");
        private readonly static GUIContent weaponTurretMoveSpeedContent = new GUIContent("Turret Move Speed", "The rate at which the turret can rotate");
        private readonly static GUIContent weaponTurretReturnToParkIntervalContent = new GUIContent("Turret Park Interval", "When greater than 0, the number of seconds a turret will wait, after loosing a target, to begin returning to the original orientation.");
        private readonly static GUIContent weaponInaccuracyContent = new GUIContent("Turret Inaccuracy", "When inaccuracy is greater than 0, the turret may not aim at the optimum target position.");
        private readonly static GUIContent weaponCheckLineOfSightContent = new GUIContent("Check Line Of Sight", "Whether the weapon checks line of sight before firing (in order to prevent friendly fire) each frame. " +
            "Since this uses raycasts it can lead to reduced performance.");
        private readonly static GUIContent partStartingHealthContent = new GUIContent("Starting Health", "The initial health value of this surface turret. This is the amount of damage " + 
            "that needs to be done to the turret for it to reach its min performance.");

        private readonly static GUIContent weaponHeatLevelContent = new GUIContent("Heat Level", "The heat of the weapon - range 0.0 (starting temp) to 100.0 (max temp).");
        private readonly static GUIContent weaponHeatUpRateContent = new GUIContent("Heat Up Rate", "The rate heat is added per second for beam weapons. For projectile weapons, it is inversely proportional to the firing interval (reload time). If rate is 0, heat level never changes.");
        private readonly static GUIContent weaponHeatDownRateContent = new GUIContent("Cool Down Rate", "The rate heat is removed per second. This is the rate the weapon cools when not in use.");
        private readonly static GUIContent weaponOverHeatThresholdContent = new GUIContent("Overheat Threshold", "The heat level that the weapon will begin to overheat and start being less efficient.");
        private readonly static GUIContent weaponIsBurnoutOnMaxHeatContent = new GUIContent("Burnout on Max Heat", "When the weapon reaches max heat level of 100, will the weapon be inoperable until it is repaired?");

        private readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
        private readonly static GUIContent gizmoFindBtnContent = new GUIContent("F", "Find (select) in the scene view.");
        private readonly static GUIContent gotoEffectFolderBtnContent = new GUIContent("F", "Find and highlight the sample Effects folder");
        private readonly static GUIContent gotoDestructsFolderBtnContent = new GUIContent("F", "Find and highlight the sample Destructs folder");
        private readonly static GUIContent gotoProjectileFolderBtnContent = new GUIContent("F", "Find and highlight the sample Projectiles folder");
        private readonly static GUIContent gotoBeamFolderBtnContent = new GUIContent("F", "Find and highlight the sample Beam folder");

        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
        private readonly static GUIContent debugTurretDataContent = new GUIContent("Turret Data");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent(" Is Initialised?");
        private readonly static GUIContent debugIsDestroyedContent = new GUIContent(" Is Destroyed?");
        private readonly static GUIContent debugRadarIdContent = new GUIContent(" Radar Id");
        private readonly static GUIContent debugRadarTypeContent = new GUIContent(" Radar Type");
        private readonly static GUIContent debugRadarFactionIdContent = new GUIContent(" Radar Faction Id");
        private readonly static GUIContent debugRadarSquadronIdContent = new GUIContent(" Radar Squadron Id");
        private readonly static GUIContent debugTargetDataContent = new GUIContent("Target Data");
        private readonly static GUIContent debugTargetShipContent = new GUIContent(" Target Ship", "If a ship is being targeted, will return its name. If it is being targeted but is NULL, will assume destroyed.");
        private readonly static GUIContent debugTargetShipDamageRegionContent = new GUIContent(" Target Damage Region", "If a ship's damage region is being targeted, will return its name. If it is being targeted but the ship is NULL, will assume destroyed.");
        private readonly static GUIContent debugTargetGameObjectContent = new GUIContent(" Target GameObject", "If a gameobject is being targeted, will return its name");
        #endregion

        #region Serialized Properties
        private SerializedProperty destructionEffectsObjectProp;
        private SerializedProperty destructObjectProp;
        private SerializedProperty isDestroyOnNoHealthProp;
        private SerializedProperty weaponProp;
        private SerializedProperty weaponTypeProp;
        private SerializedProperty weaponAmmunitionProp;
        private SerializedProperty weaponNameProp;
        private SerializedProperty weaponFirePositionListProp;
        private SerializedProperty weaponShowGizmosInSceneViewProp;
        private SerializedProperty weaponShowInEditorProp;
        private SerializedProperty weaponFiringButtonProp;
        private SerializedProperty weaponIsMultipleFirePositionsProp;
        private SerializedProperty weaponProjectilePrefabProp;
        private SerializedProperty weaponBeamPrefabProp;
        private SerializedProperty weaponChargeAmountProp;
        private SerializedProperty weaponRechargeTimeProp;
        private SerializedProperty weaponHeatLevelProp;
        private SerializedProperty weaponOverHeatThresholdProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            surfaceTurretModule = (SurfaceTurretModule)target;

            // Scene view interaction
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            SceneView.onSceneGUIDelegate += SceneGUI;
            #endif

            if (surfaceTurretModule.weapon == null)
            {
                surfaceTurretModule.weapon = new Weapon();
                if (surfaceTurretModule.weapon != null)
                {
                    surfaceTurretModule.weapon.weaponType = Weapon.WeaponType.TurretProjectile;
                }

                // Set default gravity here too
                surfaceTurretModule.gravitationalAcceleration = 9.81f;
                surfaceTurretModule.gravityDirection = Vector3.down;
            }

            // Initialise properties
            destructionEffectsObjectProp = serializedObject.FindProperty("destructionEffectsObject");
            destructObjectProp = serializedObject.FindProperty("destructObject");
            isDestroyOnNoHealthProp = serializedObject.FindProperty("isDestroyOnNoHealth");
            weaponProp = serializedObject.FindProperty("weapon");

            weaponHeatLevelProp = weaponProp.FindPropertyRelative("heatLevel");
            weaponOverHeatThresholdProp = weaponProp.FindPropertyRelative("overHeatThreshold");

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            // Reset GUIStyles
            helpBoxRichText = null;
            labelFieldRichText = null;
            buttonCompact = null;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            // Deselect all components in the scene view
            DeselectAllComponents();
        }

        public void OnDestroy()
        {
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            #endif

            // Always unhide Unity tools when losing focus on this gameObject
            Tools.hidden = false;
        }

        /// <summary>
        /// Gets called automatically 10 times per second
        /// </summary>
        private void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (surfaceTurretModule.allowRepaint) { Repaint(); }
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise
            surfaceTurretModule.allowRepaint = false;
            isSceneModified = false;
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            #endregion

            #region Configure Buttons and Styles

            if (helpBoxRichText == null)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;
            }

            if (buttonCompact == null)
            {
                buttonCompact = new GUIStyle("Button");
                buttonCompact.fontSize = 10;
            }

            if (labelFieldRichText == null)
            {
                labelFieldRichText = new GUIStyle("Label");
                labelFieldRichText.richText = true;
            }

            // Set up the toggle buttons styles
            if (toggleCompactButtonStyleNormal == null)
            {
                // Create a new button or else will effect the Button style for other buttons too
                toggleCompactButtonStyleNormal = new GUIStyle("Button");
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }
            #endregion

            //DrawDefaultInspector();

            #region Header Info
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(turretHeaderContent, helpBoxRichText);
            #endregion

            serializedObject.Update();

            #region Find Properties

            weaponAmmunitionProp = weaponProp.FindPropertyRelative("ammunition");
            weaponFirePositionListProp = weaponProp.FindPropertyRelative("firePositionList");
            weaponShowGizmosInSceneViewProp = weaponProp.FindPropertyRelative("showGizmosInSceneView");
            weaponShowInEditorProp = weaponProp.FindPropertyRelative("showInEditor");
            weaponFiringButtonProp = weaponProp.FindPropertyRelative("firingButton");
            weaponIsMultipleFirePositionsProp = weaponProp.FindPropertyRelative("isMultipleFirePositions");

            #endregion

            #region General and Identification Settings
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialiseOnStart"), initialiseOnStartContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("factionId"), factionIdContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("squadronId"), squadronIdContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCreateLocation"), autoCreateLocationContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isVisibleToRadar"), isVisibleToRadarContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radarBlipSize"), radarBlipSizeContent);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(destructionEffectsObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
            if (GUILayout.Button(gotoEffectFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, false, true); }
            EditorGUILayout.PropertyField(destructionEffectsObjectProp, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(isDestroyOnNoHealthProp, isDestroyOnNoHealthContent);

            if (isDestroyOnNoHealthProp.boolValue)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(destructObjectContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
                if (GUILayout.Button(gotoDestructsFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.destructFolder, false, true); }
                EditorGUILayout.PropertyField(destructObjectProp, GUIContent.none);
                GUILayout.EndHorizontal();
            }

            #endregion

            #region Fire Position validation
            // If the fire position list is null, create the list and add a default position
            if (weaponFirePositionListProp == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                surfaceTurretModule.weapon.firePositionList = new List<Vector3>(2);
                surfaceTurretModule.weapon.firePositionList.Add(Vector3.zero);
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Weapon Find buttons
            GUILayout.BeginHorizontal();
            //EditorGUI.indentLevel += 1;
            weaponShowInEditorProp.boolValue = EditorGUILayout.Foldout(weaponShowInEditorProp.boolValue, "Weapon Settings");
            //EditorGUI.indentLevel -= 1;

            // Find (select) in the scene
            if (GUILayout.Button(gizmoFindBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
            {
                weaponProp.FindPropertyRelative("selectedInSceneView").boolValue = true;
                // Hide Unity tools
                Tools.hidden = true;
            }

            // Show Gizmos button
            if (weaponShowGizmosInSceneViewProp.boolValue)
            {
                if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f)))
                {
                    weaponShowGizmosInSceneViewProp.boolValue = false;
                    // we only have one weapon and set of gizmos so unhide the tools
                    Tools.hidden = false;
                }
            }
            else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { weaponShowGizmosInSceneViewProp.boolValue = true; } }

            GUILayout.EndHorizontal();
            #endregion

            #region Weapon Settings

            if (weaponShowInEditorProp.boolValue)
            {
                #region Find Weapon Properties
                weaponTypeProp = weaponProp.FindPropertyRelative("weaponType");
                weaponProjectilePrefabProp = weaponProp.FindPropertyRelative("projectilePrefab");
                weaponBeamPrefabProp = weaponProp.FindPropertyRelative("beamPrefab");
                weaponChargeAmountProp = weaponProp.FindPropertyRelative("chargeAmount");
                weaponRechargeTimeProp = weaponProp.FindPropertyRelative("rechargeTime");
                #endregion

                #region Weapon Type

                EditorGUI.BeginChangeCheck();

                // Set to a default type for surface turrets
                if (weaponTypeProp.intValue == Weapon.FixedProjectileInt || weaponTypeProp.intValue == Weapon.FixedBeamInt)
                {
                    weaponTypeProp.intValue = Weapon.TurretProjectileInt;
                }

                prevWeaponType = weaponTypeProp.intValue;

                // Look up the index position in the SurfaceWeaponType enumeration using the Weapon.WeaponType value.
                int weaponTypeIndex = ArrayUtility.IndexOf((SurfaceWeaponType[])System.Enum.GetValues(typeof(SurfaceWeaponType)), (SurfaceWeaponType)weaponTypeProp.intValue);

                EditorGUI.BeginChangeCheck();
                // NOTE: type.GetEnumNames() requires Scripting Runtime Version 4.0+
                //weaponTypeIndex = EditorGUILayout.Popup(weaponTypeContent, weaponTypeIndex, typeof(SurfaceWeaponType).GetEnumNames());
                weaponTypeIndex = EditorGUILayout.Popup(weaponTypeContent, weaponTypeIndex, System.Enum.GetNames(typeof(SurfaceWeaponType)));
                if (EditorGUI.EndChangeCheck())
                {
                    if (weaponTypeIndex >= 0)
                    {
                        // Convert back into a Weapon.WeaponType value
                        weaponTypeProp.intValue = (int)System.Enum.GetValues(typeof(SurfaceWeaponType)).GetValue(weaponTypeIndex);
                    }
                    else { weaponTypeProp.intValue = Weapon.TurretProjectileInt; }

                    // If the weapon has changed from Projectile to Beam or vis versa, clear the old prefab and set defaults.
                    if (weaponTypeProp.intValue == Weapon.TurretBeamInt)
                    {
                        weaponProjectilePrefabProp.objectReferenceValue = null;
                        // Currently beams have unlimited amno
                        weaponAmmunitionProp.intValue = -1;
                    }
                    else if (prevWeaponType == Weapon.TurretBeamInt)
                    {
                        weaponBeamPrefabProp.objectReferenceValue = null;
                    }
                }

                if (weaponTypeProp.intValue == Weapon.TurretBeamInt) { SSCEditorHelper.InTechPreview(false); }

                #endregion

                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("relativePosition"), weaponRelativePositionContent);

                #region Weapon Fire Positions
                EditorGUILayout.PropertyField(weaponIsMultipleFirePositionsProp, weaponIsMultipleFirePositionsContent);
                if (weaponIsMultipleFirePositionsProp.boolValue)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(weaponFirePositionContent);
                    int numWeaponFirePositions = weaponFirePositionListProp.arraySize;
                    if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                    {
                        weaponFirePositionListProp.arraySize += 1;
                        numWeaponFirePositions = weaponFirePositionListProp.arraySize;
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                    {
                        if (numWeaponFirePositions > 1)
                        {
                            weaponFirePositionListProp.arraySize -= 1;
                            numWeaponFirePositions--;
                        }
                    }
                    GUILayout.EndHorizontal();

                    for (int firePosIndex = 0; firePosIndex < numWeaponFirePositions; firePosIndex++)
                    {
                        EditorGUILayout.PropertyField(weaponFirePositionListProp.GetArrayElementAtIndex(firePosIndex));
                    }
                    GUILayout.EndVertical();
                }
                #endregion

                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("fireDirection"), weaponFireDirectionContent);

                // Beam or Projectile prefabs
                if (weaponTypeProp.intValue == Weapon.TurretBeamInt)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(weaponBeamPrefabContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
                    if (GUILayout.Button(gotoBeamFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.beamsFolder, false, true); }
                    EditorGUILayout.PropertyField(weaponBeamPrefabProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("reloadTime"), weaponPowerUpTimeContent);
                    EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("maxRange"), weaponMaxRangeContent);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(weaponProjectilePrefabContent, GUILayout.Width(defaultEditorLabelWidth - 28f));
                    if (GUILayout.Button(gotoProjectileFolderBtnContent, buttonCompact, GUILayout.Width(20f))) { SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.projectilesFolder, false, true); }
                    EditorGUILayout.PropertyField(weaponProjectilePrefabProp, GUIContent.none);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("reloadTime"), weaponReloadTimeContent);
                }

                // NOTE: Surface turrets don't have the option to be connected to the player input module for primary/secondary fire
                // buttons like ships do.
                bool isAutoFire = weaponFiringButtonProp.intValue == (int)Weapon.FiringButton.AutoFire;

                EditorGUI.BeginChangeCheck();
                isAutoFire = EditorGUILayout.Toggle(weaponFiringButtonContent, isAutoFire);
                if (EditorGUI.EndChangeCheck())
                {
                    weaponFiringButtonProp.intValue = isAutoFire ? (int)Weapon.FiringButton.AutoFire : (int)Weapon.FiringButton.None;
                }

                if (weaponFiringButtonProp.intValue == (int)Weapon.FiringButton.AutoFire)
                {
                    EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("checkLineOfSight"), weaponCheckLineOfSightContent);
                }

                // Beams auto-recharge. Amno applies to projectile weapons
                if (weaponTypeProp.intValue == Weapon.TurretBeamInt)
                {
                    EditorGUILayout.PropertyField(weaponChargeAmountProp, weaponChargeAmountContent);
                    EditorGUILayout.PropertyField(weaponRechargeTimeProp, weaponRechargeTimeContent);
                }
                else
                {
                    bool unlimitedAmmo = weaponAmmunitionProp.intValue < 0;
                    unlimitedAmmo = EditorGUILayout.Toggle(weaponUnlimitedAmmoContent, unlimitedAmmo);
                    if (unlimitedAmmo && weaponAmmunitionProp.intValue != -1) { weaponAmmunitionProp.intValue = -1; }
                    else if (!unlimitedAmmo)
                    {
                        if (weaponAmmunitionProp.intValue < 0) { weaponAmmunitionProp.intValue = 10; }
                        EditorGUILayout.PropertyField(weaponAmmunitionProp, weaponAmmunitionContent);
                    }
                }

                // Automatically update the weapon name to match the surface turret's gameobject name
                weaponNameProp = weaponProp.FindPropertyRelative("name");
                if (weaponNameProp.stringValue != surfaceTurretModule.transform.name)
                {
                    weaponNameProp.stringValue = surfaceTurretModule.transform.name;
                }

                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("startingHealth"), partStartingHealthContent);

                #region Weapon Heat

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(weaponHeatLevelProp, weaponHeatLevelContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    surfaceTurretModule.weapon.SetHeatLevel(weaponHeatLevelProp.floatValue);
                }
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("heatUpRate"), weaponHeatUpRateContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("heatDownRate"), weaponHeatDownRateContent);
                EditorGUILayout.PropertyField(weaponOverHeatThresholdProp, weaponOverHeatThresholdContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("isBurnoutOnMaxHeat"), weaponIsBurnoutOnMaxHeatContent);

                // Fix any backward compatibility issues
                if (weaponOverHeatThresholdProp.floatValue == 0f) { weaponOverHeatThresholdProp.floatValue = 80f; }

                #endregion

                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretPivotY"), weaponTurretPivotYContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretPivotX"), weaponTurretPivotXContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMinY"), weaponTurretMinYContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMaxY"), weaponTurretMaxYContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMinX"), weaponTurretMinXContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMaxX"), weaponTurretMaxXContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretMoveSpeed"), weaponTurretMoveSpeedContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("inaccuracy"), weaponInaccuracyContent);
                EditorGUILayout.PropertyField(weaponProp.FindPropertyRelative("turretReturnToParkInterval"), weaponTurretReturnToParkIntervalContent);
            }
            #endregion

            #region Gravity Settings
            EditorGUILayout.LabelField(gravityHeaderContent, helpBoxRichText);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravitationalAcceleration"), gravitationalAccelerationContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityDirection"), gravityDirectionContent);
            #endregion

            serializedObject.ApplyModifiedProperties();

            #region Mark Scene Dirty if required

            if (isSceneModified && !Application.isPlaying)
            {
                isSceneModified = false;
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            #endregion

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);

            if (isDebuggingEnabled && surfaceTurretModule != null)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.LabelField(debugTurretDataContent);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsDestroyedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.IsDestroyed ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugRadarIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.RadarId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                // Only display radar item data at runtime
                if (surfaceTurretModule.IsInitialised && surfaceTurretModule.RadarId >= 0)
                {
                    if (sscRadar == null) { sscRadar = SSCRadar.GetOrCreateRadar(); }

                    if (sscRadar != null && sscRadar.IsInitialised)
                    {
                        SSCRadarItem radarItem = sscRadar.GetRadarItem(surfaceTurretModule.RadarId);
                        if (radarItem != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugRadarTypeContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(radarItem.radarItemType.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugRadarFactionIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(radarItem.factionId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(debugRadarSquadronIdContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                            EditorGUILayout.LabelField(radarItem.squadronId.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                EditorGUILayout.LabelField(debugTargetDataContent);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.TargetShipName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetShipDamageRegionContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.TargetShipDamageRegionName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugTargetGameObjectContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(surfaceTurretModule.TargetGameObjectName, GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            #endregion

            surfaceTurretModule.allowRepaint = true;
        }

        #endregion

        #region Private Member Methods

        private void SceneGUI(SceneView sv)
        {
            if (surfaceTurretModule != null && surfaceTurretModule.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // Get the rotation of the surface turret gameobject in the scene
                sceneViewSurfaceTurretRotation = Quaternion.LookRotation(surfaceTurretModule.transform.forward, surfaceTurretModule.transform.up);

                #region Weapon

                using (new Handles.DrawingScope(weaponGizmoColour))
                {
                    weaponComponent = surfaceTurretModule.weapon;

                    if (weaponComponent != null)
                    {
                        // Prevent fire direction ever being a zero vector
                        if (weaponComponent.fireDirection == Vector3.zero) { weaponComponent.fireDirection = Vector3.forward; }

                        if (weaponComponent.showGizmosInSceneView && weaponComponent.turretPivotX != null)
                        {
                            //componentHandlePosition = surfaceTurretModule.transform.TransformPoint(weaponComponent.relativePosition);
                            // Do not use transform.TransformPoint because it won't work correctly when the parent gameobject is has scale not equal to 1,1,1.
                            componentHandlePosition = (weaponComponent.turretPivotX.rotation * weaponComponent.relativePosition) + surfaceTurretModule.transform.position;

                            // Get component handle rotation
                            componentHandleRotation = Quaternion.LookRotation(surfaceTurretModule.transform.TransformDirection(weaponComponent.fireDirection), surfaceTurretModule.transform.up);

                            handleDistanceScale = HandleUtility.GetHandleSize(componentHandlePosition);

                            // Draw a a fire direction arrow in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, 1f, EventType.Repaint);

                                // Draw Fire Positions (non-interactable)
                                if (weaponComponent.isMultipleFirePositions)
                                {
                                    weaponFirePositionList = weaponComponent.firePositionList;
                                    numCompWeaponFirePositions = weaponComponent.firePositionList == null ? 0 : weaponComponent.firePositionList.Count;

                                    for (int fposIdx = 0; fposIdx < numCompWeaponFirePositions; fposIdx++)
                                    {
                                        // Do not use transform.TransformPoint because it won't work correctly when the parent gameobject is has scale not equal to 1,1,1.
                                        Handles.SphereHandleCap(0, (weaponComponent.turretPivotX.rotation * (weaponComponent.relativePosition + weaponFirePositionList[fposIdx])) +
                                            surfaceTurretModule.transform.position, componentHandleRotation, 0.2f, EventType.Repaint);
                                    }
                                }

                                // Draw firing arc for turrets

                                // Horizontal rotation arc
                                using (new Handles.DrawingScope(weaponGizmoTurretYColour))
                                {
                                    Handles.DrawWireArc(componentHandlePosition, weaponComponent.turretPivotX.up, Quaternion.AngleAxis(weaponComponent.turretMinY, weaponComponent.turretPivotX.up) * weaponComponent.turretPivotX.forward, weaponComponent.turretMaxY + 360f - (weaponComponent.turretMinY + 360f), handleDistanceScale * 0.75f);
                                }

                                // Elevation arc
                                using (new Handles.DrawingScope(weaponGizmoTurretXColour))
                                {
                                    Handles.DrawSolidArc(componentHandlePosition, weaponComponent.turretPivotX.right * -1f, Quaternion.AngleAxis(weaponComponent.turretMinX, weaponComponent.turretPivotX.right * -1f) * weaponComponent.turretPivotX.forward, weaponComponent.turretMaxX + 360f - (weaponComponent.turretMinX + 360f), handleDistanceScale * 0.75f);
                                }
                            }

                            if (weaponComponent.selectedInSceneView)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the direction of thrust
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(surfaceTurretModule, "Rotate Weapon Fire Direction");
                                        weaponComponent.fireDirection = surfaceTurretModule.transform.InverseTransformDirection(componentHandleRotation * Vector3.forward);
                                    }
                                }
                                #if UNITY_2017_3_OR_NEWER
                                else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                #else
                                else if (Tools.current == Tool.Move)
                                #endif
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewSurfaceTurretRotation);

                                    // Use the position handle to edit the position of the weapon
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(surfaceTurretModule, "Move Weapon");
                                        weaponComponent.relativePosition = surfaceTurretModule.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                            }

                            // Allow the user to select/deselect the weapon location in the scene view
                            if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                            {
                                if (weaponComponent.selectedInSceneView)
                                {
                                    DeselectAllComponents();
                                }
                                else
                                {
                                    DeselectAllComponents();
                                    weaponComponent.selectedInSceneView = true;
                                    weaponComponent.showInEditor = true;
                                    // Hide Unity tools
                                    Tools.hidden = true;
                                }
                            }
                        }
                    }
                }

                #endregion

                if (isSceneDirtyRequired && !Application.isPlaying)
                {
                    isSceneDirtyRequired = false;
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                // Always unhide Unity tools and deselect all components when the object is disabled
                Tools.hidden = false;
                DeselectAllComponents();
            }
        }

        /// <summary>
        /// Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        private void DeselectAllComponents()
        {
            // Set all components to not be selected

            // Avoid situation where surface turret is destroyed in play mode while it is selected.
            if (surfaceTurretModule != null)
            {
                if (surfaceTurretModule.weapon != null)
                {
                    surfaceTurretModule.weapon.selectedInSceneView = false;
                }
            }

            // Unhide Unity tools
            Tools.hidden = false;
        }

        #endregion
    }
}