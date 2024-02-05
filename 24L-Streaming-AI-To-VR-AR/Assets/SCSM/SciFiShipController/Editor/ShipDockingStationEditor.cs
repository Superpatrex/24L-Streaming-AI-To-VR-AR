using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipDockingStation))]
    public class ShipDockingStationEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables
        private ShipDockingStation shipDockingStation;
        private SSCManager sscManager;

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        // Similar to isSceneDirtyRequired (SceneView variabled) but used for Inspector modifications.
        private bool isSceneModified = false;

        private int dockingPointMoveDownPos = -1;
        private int dockingPointInsertPos = -1;
        private int dockingPointDeletePos = -1;

        #endregion

        #region SceneView Variables

        private bool isSceneDirtyRequired = false;
        private Quaternion sceneViewTrfmRotation = Quaternion.identity;
        private ShipDockingPoint dockingPointComponent = null;

        private Vector3 componentHandlePosition = Vector3.zero;
        //private Vector3 gizmoPosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;
        private float relativeHandleSize = 1f;

        private Color fadedGizmoColour;

        #endregion

        #region GUIContent General
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to dock other ships with this gameobject or Ship");
        private readonly static GUIContent[] tabTexts = { new GUIContent("General"), new GUIContent("Events") };
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent("Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. " +
          "This should be disabled if you are instantiating the Ship or ShipDockingStation through code and using the Docking API methods.");

        private readonly static GUIContent gizmoToggleBtnContent = new GUIContent("G", "Toggle gizmos and visualisations on/off for all items in the scene view");
        private readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
        private readonly static GUIContent gizmoFindBtnContent = new GUIContent("F", "Find (select) in the scene view.");
        private readonly static GUIContent resetBtnContent = new GUIContent("R", "Reset");

        private readonly static GUIContent dockingPointExportJsonContent = new GUIContent("Export Json", "Export Docking Points to a json file");
        private readonly static GUIContent dockingPointImportJsonContent = new GUIContent("Import Json", "Import Docking Points from a json file");

        #endregion

        #region GUIContent Events
        private readonly static GUIContent onPreUndockContent = new GUIContent("", "Methods that get called immediately before Undock or UndockDelayed are executed. WARNING: Do NOT call UndockShip from this event.");
        private readonly static GUIContent onPostDockedContent = new GUIContent("", "Methods that get called immediately after docking is complete. WARNING: Do NOT call DockShip from this event.");

        #endregion

        #region GUIContent Ship Docking Point
        private readonly static GUIContent dkgPtRelativePositionContent = new GUIContent("Relative Position", "Local position relative to the ShipDockingStation tranform position");
        private readonly static GUIContent dkgPtRelativeRotationContent = new GUIContent("Relative Rotation", "Docking Point rotation relative, in degrees, to the ShipDockingStation rotation");
        private readonly static GUIContent dkgPtEntryPathContent = new GUIContent("Docking Entry Path", "The optional Path (stored as a guidHash) which identifies the entry path a ship can take to dock at this docking point");
        private readonly static GUIContent dkgPtExitPathContent = new GUIContent("Undocking Exit Path", "The optional Path (stored as a guidHash) which identifies the exit path a ship can take to depart from this docking point");
        private readonly static GUIContent dkgPtHoverHeightContent = new GUIContent("Hover Height", "This is the optimum height above the docking point in the relative up direction, a ship hovers before arriving or departing");
        private readonly static GUIContent dkgPtDockedShipContent = new GUIContent("Assigned Ship", "The Ship in the scene that is currently docked, docking or undocking with the Ship Docking Point. This assigned ship requires a Ship Docking component.");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty selectedTabIntProp;
        private SerializedProperty dkgPtListProp;
        private SerializedProperty dkgPtProp;
        private SerializedProperty dkgPtShowInEditorProp;
        private SerializedProperty dkgPtEntryPathGUIDHashProp;
        private SerializedProperty dkgPtExitPathGUIDHashProp;
        private SerializedProperty dkgPtDockedShipProp;
        private SerializedProperty dkgPtSelectedInSceneViewProp;
        private SerializedProperty dkgPtShowGizmosInSceneViewProp;
        private SerializedProperty isDockingPointListExpandedProp;
        private SerializedProperty initialiseOnAwakeProp;
        #endregion

        #region Serialized Properties - Events
        private SerializedProperty onPreUndockProp;
        private SerializedProperty onPostDockedProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            shipDockingStation = (ShipDockingStation)target;

            // Only use if require scene view interaction
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            SceneView.onSceneGUIDelegate += SceneGUI;
            #endif

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            #endregion

            #region Find Properties - Events
            onPreUndockProp = serializedObject.FindProperty("onPreUndock");
            onPostDockedProp = serializedObject.FindProperty("onPostDocked");
            #endregion

            // Reset GUIStyles
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;

            if (sscManager == null) { sscManager = SSCManager.GetOrCreateManager(shipDockingStation.gameObject.scene.handle); }

            if (shipDockingStation != null && shipDockingStation.shipDockingPointList != null)
            {
                // If a docking point is marked as selected, turn off the transform tools
                if (shipDockingStation.shipDockingPointList.Exists(dp => dp.selectedInSceneView == true))
                {
                    Tools.hidden = true;
                }
            }
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        void OnDestroy()
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
        /// Comment out if not required
        /// </summary>
        void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (shipDockingStation.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods

        // <summary>
        /// Draw the toolbar using the supplied array of tab text.
        /// </summary>
        /// <param name="tabGUIContent"></param>
        private void DrawToolBar(GUIContent[] tabGUIContent)
        {
            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabGUIContent);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }
        }

        /// <summary>
        /// Draw gizmos and editable handles in the scene view
        /// </summary>
        /// <param name="sv"></param>
        private void SceneGUI(SceneView sv)
        {
            if (shipDockingStation != null && shipDockingStation.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;

                // Get the rotation of the gameobject / transform in the scene
                sceneViewTrfmRotation = Quaternion.LookRotation(shipDockingStation.transform.forward, shipDockingStation.transform.up);

                Vector3 localScale = shipDockingStation.transform.localScale;

                using (new Handles.DrawingScope(shipDockingStation.dockingPointGizmoColour))
                {
                    int numDockingPoints = shipDockingStation.shipDockingPointList == null ? 0 : shipDockingStation.shipDockingPointList.Count;
                    for (int dpi = 0; dpi < numDockingPoints; dpi++)
                    {
                        dockingPointComponent = shipDockingStation.shipDockingPointList[dpi];

                        fadedGizmoColour = shipDockingStation.dockingPointGizmoColour;

                        // If this is not the selected Docking Point, show it a little more transparent
                        if (!dockingPointComponent.selectedInSceneView)
                        {
                            fadedGizmoColour.a *= 0.65f;
                            if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = shipDockingStation.dockingPointGizmoColour.a; }
                        }

                        if (dockingPointComponent.showGizmosInSceneView)
                        {
                            // Get component handle position
                            componentHandlePosition = shipDockingStation.transform.TransformPoint(new Vector3(dockingPointComponent.relativePosition.x / localScale.x, dockingPointComponent.relativePosition.y / localScale.y, dockingPointComponent.relativePosition.z / localScale.z));

                            relativeHandleSize = HandleUtility.GetHandleSize(componentHandlePosition);

                            // Get component handle rotation
                            componentHandleRotation = shipDockingStation.transform.rotation * Quaternion.Euler(dockingPointComponent.relativeRotation);

                            // Draw point in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                using (new Handles.DrawingScope(fadedGizmoColour))
                                {
                                    // Forwards direction of the ship docking point
                                    Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition, componentHandleRotation, 1.25f * relativeHandleSize, EventType.Repaint);

                                    // Draw the up direction with a sphere on top of a line
                                    Quaternion upRotation = componentHandleRotation * Quaternion.Euler(270f, 0f, 0f);
                                    Handles.DrawLine(componentHandlePosition, componentHandlePosition + upRotation * (Vector3.forward * relativeHandleSize * 1.25f));
                                    Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), componentHandlePosition + (upRotation * (Vector3.forward * relativeHandleSize * 1.25f)), upRotation, 0.3f * relativeHandleSize, EventType.Repaint);
                                }
                            }

                            if (dockingPointComponent.selectedInSceneView)
                            {
                                // Choose which handle to draw based on which Unity tool is selected
                                if (Tools.current == Tool.Rotate)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a rotation handle
                                    componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                                    // Use the rotation handle to edit the docking point direction
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipDockingStation, "Rotate Docking Point");

                                        dockingPointComponent.relativeRotation = (Quaternion.Inverse(shipDockingStation.transform.rotation) * componentHandleRotation).eulerAngles;   
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
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewTrfmRotation);

                                    // Use the position handle to edit the position of the weapon
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipDockingStation, "Move Docking Point");
                                        //dockingPointComponent.relativePosition = shipDockingStation.transform.InverseTransformPoint(componentHandlePosition);
                                        dockingPointComponent.relativePosition = shipDockingStation.transform.InverseTransformPoint(new Vector3(componentHandlePosition.x * localScale.x, componentHandlePosition.y * localScale.y, componentHandlePosition.z * localScale.z));
                                    }
                                }
                            }

                            // If the docking point is not selected it will be faded, else it will be the original colour.
                            using (new Handles.DrawingScope(fadedGizmoColour))
                            {
                                // Allow the user to select/deselect the docking point in the scene view
                                if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f * relativeHandleSize, 0.25f * relativeHandleSize, Handles.SphereHandleCap))
                                {
                                    if (dockingPointComponent.selectedInSceneView)
                                    {
                                        DeselectAllComponents();
                                        dockingPointComponent.showInEditor = false;
                                    }
                                    else
                                    {
                                        DeselectAllComponents();
                                        shipDockingStation.isDockingPointListExpanded = false;
                                        ExpandList(shipDockingStation.shipDockingPointList, false);
                                        dockingPointComponent.selectedInSceneView = true;
                                        dockingPointComponent.showInEditor = true;
                                        isSceneDirtyRequired = true;
                                        // Hide Unity tools
                                        Tools.hidden = true;
                                    }
                                }
                            }
                        }
                    }
                }

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
        /// Expand (show) or collapse (hide) all items in a list in the editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList<T>(List<T> componentList, bool isExpanded)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(ShipDockingPoint))
                {
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as ShipDockingPoint).showInEditor = isExpanded;
                    }
                }
            }
        }

        /// <summary>
        /// Deselect all components in the scene view edit mode, and unhides the Unity tools
        /// </summary>
        private void DeselectAllComponents()
        {
            // Set all components to not be selected
            if (shipDockingStation != null)
            {
                int numDockingPoints = shipDockingStation.shipDockingPointList == null ? 0 : shipDockingStation.shipDockingPointList.Count;
                for (int dpi = 0; dpi < numDockingPoints; dpi++)
                {
                    shipDockingStation.shipDockingPointList[dpi].selectedInSceneView = false;
                }
            }
            // Unhide Unity tools
            Tools.hidden = false;
        }

        /// <summary>
        /// Toggle on/off the gizmos and visualisations of a list of components based
        /// on the state of the first component in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        private void ToggleGizmos<T>(List<T> componentList)
        {
            int numComponents = componentList == null ? 0 : componentList.Count;

            if (numComponents > 0)
            {
                System.Type compType = typeof(T);

                if (compType == typeof(ShipDockingPoint))
                {
                    // Examine the first component
                    bool showGizmos = !(componentList[0] as ShipDockingPoint).showGizmosInSceneView;

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as ShipDockingPoint).showGizmosInSceneView = showGizmos;
                    }

                    // When no Gizmos are shown , ensure we can see the standard Unity tools.
                    // This allows the user to move the ShipDockingStation gameobject
                    if (!showGizmos) { Tools.hidden = false; }
                }

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Draw a (F)ind button which will select the item in the scene view.
        /// Automatically show Gizmo in scene view if it is currently disabled
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentList"></param>
        /// <param name="showInEditorProp"></param>
        /// <param name="selectedInSceneViewProp"></param>
        /// <param name="showGizmoInSceneViewProp"></param>
        private void SelectItemInSceneViewButton<T>(List<T> componentList, SerializedProperty showInEditorProp, SerializedProperty selectedInSceneViewProp, SerializedProperty showGizmoInSceneViewProp)
        {
            if (GUILayout.Button(gizmoFindBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
            {
                serializedObject.ApplyModifiedProperties();
                DeselectAllComponents();
                ExpandList(componentList, false);
                serializedObject.Update();
                selectedInSceneViewProp.boolValue = true;
                showInEditorProp.boolValue = true;
                showGizmoInSceneViewProp.boolValue = true;
                // Hide Unity tools
                Tools.hidden = true;
            }
        }

        /// <summary>
        /// Display the path name and give option to select one from the scene (from SSCManager)
        /// </summary>
        /// <param name="propPathGUIDHash"></param>
        /// <param name="shipDockingPointIndex"></param>
        /// <param name="isEntryPath"></param>
        /// <param name="guiContent"></param>
        private void DrawPathInspector(SerializedProperty propPathGUIDHash, int shipDockingPointIndex, bool isEntryPath, GUIContent guiContent)
        {
            GUILayout.BeginHorizontal();
            #if UNITY_2019_3_OR_NEWER
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(defaultEditorLabelWidth - 25f));
            #else
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(defaultEditorLabelWidth - 29f));
            #endif
            PathData pathData = sscManager.GetPath(propPathGUIDHash.intValue);

            int selectedIdx = sscManager.pathDataList.FindIndex(path => path.guidHash == propPathGUIDHash.intValue);

            if (GUILayout.Button("..", buttonCompact, GUILayout.MaxWidth(20f)))
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();

                // Create a drop down list of all the paths
                GenericMenu dropdown = new GenericMenu();
                for (int i = 0; i < sscManager.pathDataList.Count; i++)
                {
                    // Replace space #/%/& with different chars as Unity treats them as SHIFT/CTRL/ALT in menus.
                    string _pathName = string.IsNullOrEmpty(sscManager.pathDataList[i].name) ? "No Name" : sscManager.pathDataList[i].name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                    dropdown.AddItem(new GUIContent(_pathName), i == selectedIdx, UpdatePath, new Vector3Int(shipDockingPointIndex, isEntryPath ? 0 : 1, sscManager.pathDataList[i].guidHash));
                }
                dropdown.ShowAsContext();
                SceneView.RepaintAll();

                serializedObject.Update();
            }

            EditorGUILayout.LabelField(pathData != null ? (string.IsNullOrEmpty(pathData.name) ? "No Name" : pathData.name)  : "No Path");

            // Clear the current path setting
            if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { propPathGUIDHash.intValue = 0; }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Dropdown menu callback method used when a entry or exit path is selected
        /// </summary>
        /// <param name="obj"></param>
        private void UpdatePath(object obj)
        {
            // The menu data is passed as 3 integers in a Vector3Int
            // The index of the docking point in the list
            // The selected Path guidHash
            // which is being assigned to the location slot on the path.
            if (obj != null && obj.GetType() == typeof(Vector3Int))
            {
                Vector3Int objData = (Vector3Int)obj;

                bool isEntryPath = objData.y == 0;

                if (shipDockingStation != null && shipDockingStation.shipDockingPointList != null)
                {
                    Undo.RecordObject(shipDockingStation, "Set " + (isEntryPath ? "Entry" : "Exit") + " Path ");

                    if (isEntryPath)
                    {
                        shipDockingStation.shipDockingPointList[objData.x].guidHashEntryPath = objData.z;
                    }
                    else { shipDockingStation.shipDockingPointList[objData.x].guidHashExitPath = objData.z; }
                }
            }
        }

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Initialise

            shipDockingStation.allowRepaint = false;
            isSceneModified = false;
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

            if (foldoutStyleNoLabel == null)
            {
                // When using a no-label foldout, don't forget to set the global
                // EditorGUIUtility.fieldWidth to a small value like 15, then back
                // to the original afterward.
                foldoutStyleNoLabel = new GUIStyle(EditorStyles.foldout);
                foldoutStyleNoLabel.fixedWidth = 0.01f;
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            SSCEditorHelper.SSCVersionHeader(labelFieldRichText);
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            DrawToolBar(tabTexts);
            #endregion

            #region General Properties
            if (selectedTabIntProp.intValue == 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
                EditorGUILayout.EndVertical();

                #region Import and Export Docking Points as json
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(dockingPointExportJsonContent, buttonCompact, GUILayout.Width(100f)))
                {
                    string exportPath = EditorUtility.SaveFilePanel("Save Docking Points", "Assets", shipDockingStation.name + "_docking_points", "json");

                    if (shipDockingStation.SaveDockingPointDataAsJson(exportPath))
                    {
                        // Check if path is in Project folder
                        if (exportPath.Contains(Application.dataPath))
                        {
                            // Get the folder to highlight in the Project folder
                            string folderPath = SSCEditorHelper.GetAssetFolderFromFilePath(exportPath);

                            // Get the json file in the Project folder
                            exportPath = "Assets" + exportPath.Replace(Application.dataPath, "");
                            AssetDatabase.ImportAsset(exportPath);

                            SSCEditorHelper.HighlightFolderInProjectWindow(folderPath, true, true);
                        }
                        Debug.Log("ShipDockingStation docking points exported to " + exportPath);
                    }
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button(dockingPointImportJsonContent, buttonCompact, GUILayout.Width(100f)))
                {
                    string importPath = string.Empty, importFileName = string.Empty;

                    if (SSCEditorHelper.GetFilePathFromUser("Import Docking Point Data", SSCSetup.sscFolder, new string[] { "JSON", "json" }, false, ref importPath, ref importFileName))
                    {
                        List<ShipDockingPoint> importedDockingPointList = shipDockingStation.ImportDockingPointDataFromJson(importPath, importFileName);

                        int numImportedDockingPoints = importedDockingPointList == null ? 0 : importedDockingPointList.Count;

                        if (numImportedDockingPoints > 0)
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(shipDockingStation, "Import Docking Points");

                            if (shipDockingStation.shipDockingPointList != null)
                            {
                                shipDockingStation.shipDockingPointList.Clear();
                            }
                            shipDockingStation.shipDockingPointList.AddRange(importedDockingPointList);
                            serializedObject.Update();
                        }
                    }

                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();

                #endregion

                #region Docking Points

                // Checking the property for being NULL doesn't check if the list is actually null.
                if (shipDockingStation.shipDockingPointList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    shipDockingStation.shipDockingPointList = new List<ShipDockingPoint>(10);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();
                }

                dkgPtListProp = serializedObject.FindProperty("shipDockingPointList");
                int numDockingPoints = dkgPtListProp.arraySize;

                #region Add-Remove Docking Points and Gizmos Buttons

                // Reset button variables
                dockingPointMoveDownPos = -1;
                dockingPointInsertPos = -1;
                dockingPointDeletePos = -1;

                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                isDockingPointListExpandedProp = serializedObject.FindProperty("isDockingPointListExpanded");
                EditorGUI.BeginChangeCheck();
                isDockingPointListExpandedProp.boolValue = EditorGUILayout.Foldout(isDockingPointListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    ExpandList(shipDockingStation.shipDockingPointList, isDockingPointListExpandedProp.boolValue);
                    // Read in the properties
                    serializedObject.Update();
                }
                EditorGUI.indentLevel -= 1;

                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + ">Ship Docking Points: " + numDockingPoints.ToString("00") + "</color>", labelFieldRichText);

                if (numDockingPoints > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        serializedObject.ApplyModifiedProperties();
                        ToggleGizmos(shipDockingStation.shipDockingPointList);
                        isSceneModified = true;
                        serializedObject.Update();
                    }
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(shipDockingStation, "Add Docking Point");
                    shipDockingStation.shipDockingPointList.Add(new ShipDockingPoint());
                    DeselectAllComponents();
                    ExpandList(shipDockingStation.shipDockingPointList, false);
                    isSceneModified = true;
                    // Read in the properties
                    serializedObject.Update();

                    numDockingPoints = dkgPtListProp.arraySize;
                    if (numDockingPoints > 0)
                    {
                        // Force new docking point to be serialized in scene
                        dkgPtProp = dkgPtListProp.GetArrayElementAtIndex(numDockingPoints - 1);
                        dkgPtShowInEditorProp = dkgPtProp.FindPropertyRelative("showInEditor");
                        dkgPtShowInEditorProp.boolValue = !dkgPtShowInEditorProp.boolValue;
                        // Show the new docking point and select it in the scnee
                        dkgPtShowInEditorProp.boolValue = true;
                        dkgPtProp.FindPropertyRelative("showGizmosInSceneView").boolValue = true;
                        dkgPtProp.FindPropertyRelative("selectedInSceneView").boolValue = true;
                        Tools.hidden = true;
                    }
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numDockingPoints > 0) { dockingPointDeletePos = dkgPtListProp.arraySize - 1; }
                }

                GUILayout.EndHorizontal();

                #endregion

                #region Ship Docking Point List

                numDockingPoints = dkgPtListProp.arraySize;

                for (int dpIdx = 0; dpIdx < numDockingPoints; dpIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    dkgPtProp = dkgPtListProp.GetArrayElementAtIndex(dpIdx);

                    dkgPtShowInEditorProp = dkgPtProp.FindPropertyRelative("showInEditor");
                    dkgPtShowGizmosInSceneViewProp = dkgPtProp.FindPropertyRelative("showGizmosInSceneView");
                    dkgPtSelectedInSceneViewProp = dkgPtProp.FindPropertyRelative("selectedInSceneView");
                    dkgPtEntryPathGUIDHashProp = dkgPtProp.FindPropertyRelative("guidHashEntryPath");
                    dkgPtExitPathGUIDHashProp = dkgPtProp.FindPropertyRelative("guidHashExitPath");
                    dkgPtDockedShipProp = dkgPtProp.FindPropertyRelative("dockedShip");

                    #region Docking Point Find/Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dkgPtShowInEditorProp.boolValue = EditorGUILayout.Foldout(dkgPtShowInEditorProp.boolValue, "Ship Docking Point " + (dpIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Find (select) in the scene
                    SelectItemInSceneViewButton(shipDockingStation.shipDockingPointList, dkgPtShowInEditorProp, dkgPtSelectedInSceneViewProp, dkgPtShowGizmosInSceneViewProp);

                    // Show Gizmos button
                    if (dkgPtShowGizmosInSceneViewProp.boolValue)
                    {
                        // Turn gizmos off
                        if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f)))
                        {
                            dkgPtShowGizmosInSceneViewProp.boolValue = false;
                            // If it was selected, unselect it when turning gizmos off in the scene
                            if (dkgPtSelectedInSceneViewProp.boolValue)
                            {
                                dkgPtSelectedInSceneViewProp.boolValue = false;
                                Tools.hidden = false;
                            }
                        }
                    }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { dkgPtShowGizmosInSceneViewProp.boolValue = true; } }

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numDockingPoints > 1) { dockingPointMoveDownPos = dpIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { dockingPointInsertPos = dpIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { dockingPointDeletePos = dpIdx; }
                    GUILayout.EndHorizontal();
                    #endregion

                    if (dkgPtShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dkgPtProp.FindPropertyRelative("relativePosition"), dkgPtRelativePositionContent);

                        GUILayout.BeginHorizontal();
                        #if UNITY_2019_3_OR_NEWER
                        EditorGUILayout.LabelField(dkgPtRelativeRotationContent, GUILayout.Width(defaultEditorLabelWidth - 25f));
                        #else
                        EditorGUILayout.LabelField(dkgPtRelativeRotationContent, GUILayout.Width(defaultEditorLabelWidth - 29f));
                        #endif
                        if (GUILayout.Button(resetBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            dkgPtProp.FindPropertyRelative("relativeRotation").vector3Value = Vector3.zero;
                        }
                        EditorGUILayout.PropertyField(dkgPtProp.FindPropertyRelative("relativeRotation"), GUIContent.none);
                        GUILayout.EndHorizontal();

                        DrawPathInspector(dkgPtEntryPathGUIDHashProp, dpIdx, true, dkgPtEntryPathContent);
                        DrawPathInspector(dkgPtExitPathGUIDHashProp, dpIdx, false, dkgPtExitPathContent);

                        EditorGUILayout.PropertyField(dkgPtProp.FindPropertyRelative("hoverHeight"), dkgPtHoverHeightContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dkgPtDockedShipProp, dkgPtDockedShipContent);
                        if (EditorGUI.EndChangeCheck() && dkgPtDockedShipProp.objectReferenceValue != null && EditorUtility.IsPersistent(dkgPtDockedShipProp.objectReferenceValue))
                        {
                            Debug.LogWarning("ShipDockingStation - only ships in the scene can be assigned a docking point");
                            dkgPtDockedShipProp.objectReferenceValue = null;
                        }
                    }
                
                    // There is a bug here in 2019.4 and 2020.1 where the DisplayDialog causes formatting issues.
                    // EndLayoutGroup: BeginLayoutGroup must be called first.
                    // No 2019_4 define but earlier versions still seem to work. 
                    //#if UNITY_2019_3_OR_NEWER
                    //if (dockingPointDeletePos >=0 ) { GUILayout.BeginVertical(); }
                    //#endif

                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Docking Points

                if (dockingPointDeletePos >= 0 || dockingPointInsertPos >= 0 || dockingPointMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);
                    // Don't permit multiple operations in the same pass
                    if (dockingPointMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (dockingPointMoveDownPos < dkgPtListProp.arraySize - 1)
                        {
                            dkgPtListProp.MoveArrayElement(dockingPointMoveDownPos, dockingPointMoveDownPos + 1);
                        }
                        else { dkgPtListProp.MoveArrayElement(dockingPointMoveDownPos, 0); }

                        dockingPointMoveDownPos = -1;
                    }
                    else if (dockingPointInsertPos >= 0)
                    {
                        // NOTE: Undo doesn't work with Insert.

                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        shipDockingStation.shipDockingPointList.Insert(dockingPointInsertPos, new ShipDockingPoint(shipDockingStation.shipDockingPointList[dockingPointInsertPos]));

                        // Read all properties from the ShipDockingStation
                        serializedObject.Update();

                        // Hide original dockingPoint
                        dkgPtListProp.GetArrayElementAtIndex(dockingPointInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        dkgPtShowInEditorProp = dkgPtListProp.GetArrayElementAtIndex(dockingPointInsertPos).FindPropertyRelative("showInEditor");

                        // Force new dockingPoint to be serialized in scene
                        dkgPtShowInEditorProp.boolValue = !dkgPtShowInEditorProp.boolValue;

                        // Show inserted duplicate dockingPoint
                        dkgPtShowInEditorProp.boolValue = true;

                        // Ensure we can see the inserted docking point in the scene view
                        dkgPtListProp.GetArrayElementAtIndex(dockingPointInsertPos).FindPropertyRelative("showGizmosInSceneView").boolValue = true;

                        // Select the inserted docking point
                        serializedObject.ApplyModifiedProperties();
                        DeselectAllComponents();
                        shipDockingStation.shipDockingPointList[dockingPointInsertPos].selectedInSceneView = true;
                        Tools.hidden = true;
                        serializedObject.Update();

                        dockingPointInsertPos = -1;

                        isSceneModified = true;
                    }
                    else if (dockingPointDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                        int _deleteIndex = dockingPointDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Ship Docking Point " + (dockingPointDeletePos + 1) + "?", "Docking Point " + (dockingPointDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the docking point from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            // If this docking point was selected, turn tools back on
                            if (dkgPtListProp.GetArrayElementAtIndex(_deleteIndex).FindPropertyRelative("selectedInSceneView").boolValue)
                            {
                                Tools.hidden = false;
                            }
                            dkgPtListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            dockingPointDeletePos = -1;
                        }
                    }

                    #if UNITY_2019_3_OR_NEWER
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

                #endregion
            }
            #endregion

            #region Events
            else
            {
                // Label workaroud for content or tooltip not working with UnityEvents property drawer
                EditorGUILayout.LabelField(onPreUndockContent, GUILayout.MaxHeight(5f));
                EditorGUILayout.PropertyField(onPreUndockProp);

                EditorGUILayout.LabelField(onPostDockedContent, GUILayout.MaxHeight(5f));
                EditorGUILayout.PropertyField(onPostDockedProp);
            }
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

            shipDockingStation.allowRepaint = true;
        }

        #endregion
    }
}