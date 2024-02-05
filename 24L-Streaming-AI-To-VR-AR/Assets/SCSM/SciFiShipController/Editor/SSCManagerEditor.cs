using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(SSCManager))]
    public class SSCManagerEditor : Editor
    {
        #region Custom Editor private variables

        private SSCManager sscManager;

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
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
        private int selectedTabInt = 0;
        private GUIStyle searchTextFieldStyle = null;
        private GUIStyle searchCancelButtonStyle = null;
        private float defaultSearchTextFieldWidth = 200f;

        private bool isDebuggingEnabled = false;
        private List<ProjectileTemplate> projectileTemplatesList;
        private List<BeamTemplate> beamTemplateList;
        private List<DestructTemplate> destructTemplateList;
        private List<EffectsObjectTemplate> effectsObjectTemplatesList;

        // location data variables
        private int numLocations = 0;
        private int numFilteredLocations = 0;
        private int locationDataMoveDownPos = -1;
        private int locationDataInsertPos = -1;
        private int locationDataDeletePos = -1;
        private Vector2 locationScrollPosition = Vector2.zero;
        private bool isModifySelectedMode = false;
        private float modifySelectedPosY = 0f;
        private float modifySelectedAddPosY = 0f;

        // path data variables
        private int numPaths = 0;
        private int numFilteredPaths = 0;
        private int pathDataMoveDownPos = -1;
        private int pathDataInsertPos = -1;
        private int pathDataDeletePos = -1;
        private int numPathLocations = 0;
        private int pathDataLocationMoveDownPos = -1;
        private int pathDataLocationInsertPos = -1;
        private int pathDataLocationDeletePos = -1;
        private bool pathLocationDataIsSelected = false; // Temp variable

        private string sscHelpPDF;

        #endregion

        #region Custom Editor Sceneview variables

        private bool isSceneDirtyRequired = false;
        private bool isSetFocusEnabled = false; // has the manager just been enabled and we need to set focus to the sceneview?
        private LocationData locationDataItem;
        private Vector3 itemHandlePosition = Vector3.zero;
        //private Vector3 gizmoPosition = Vector3.zero;
        private Quaternion itemHandleRotation = Quaternion.identity;
        private Vector3 itemHandleScale = Vector3.one;
        private Vector3 controlHandleSnap = Vector3.one * 0.3f;
        private Vector3 newControlPoint;
        private Color fadedGizmoColour;
        private float startFadeDistSqr = 4000000f; // 2km x 2km
        private float maxFadeDistSqr = 400000000f;


        // Draw Path variables
        private GUIStyle distanceLabel;
        private Vector3 pointLabelOffset = Vector3.up * 2f;
        private Vector3 locationPosition;
        private string locationName;

        #endregion

        #region GUIContent - General

        private GUIContent[] tabTexts = { new GUIContent("Locations"), new GUIContent("Paths"), new GUIContent("Options") };

        private readonly static GUIContent headerContent = new GUIContent("The Manager is automatically added to " + 
                        "the scene if it doesn't already exist at runtime. It includes projectile and effects object management for all ships along with Location and Path management."); 

        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display pool sizes at runtime in the editor. This can help optimise the min/max pool size values.");
        private readonly static GUIContent projectileHeaderContent = new GUIContent("<b>Projectile Templates</b>", "The Projectile Templates that use pooling or DOTS");
        private readonly static GUIContent projectilePoolingContent = new GUIContent(" Current Pool Size", "The current size of the projectile pool");
        #if SSC_ENTITIES
        private readonly static GUIContent projectileDOTSContent = new GUIContent(" Use DOTS is enabled", "The projectiles will be converted to Entities");
        #else
        private readonly static GUIContent projectileDOTSNotAvailableContent = new GUIContent(" Use DOTS is enabled but DOTS is not configured", "The projectile cannot use DOTS because it is not available in the project");
        #endif
        private readonly static GUIContent minmaxPoolSizeContent = new GUIContent("Min/Max", "The minimum and maximum size of the pool");
        private readonly static GUIContent beamHeaderContent = new GUIContent("<b>Beam Templates</b>", "The Beam Templates that use pooling");
        private readonly static GUIContent beamPoolingContent = new GUIContent(" Current Pool Size", "The current size of the beam pool");
        private readonly static GUIContent destructHeaderContent = new GUIContent("<b>Destruct Templates</b>", "The Destruct Templates that use pooling");
        private readonly static GUIContent destructPoolingContent = new GUIContent(" Current Pool Size", "The current size of the destruct pool");

        private readonly static GUIContent effectsHeaderContent = new GUIContent("<b>Effects Templates</b>", "The Effects Object Templates that use pooling");
        private readonly static GUIContent effectsPoolingContent = new GUIContent(" Current Pool Size", "The current size of the effects pool");
        private readonly static GUIContent noneWithPoolingContent = new GUIContent(" (None with <i>Use Pooling</i> enabled)");
        
        private readonly static GUIContent findBtnContent = new GUIContent("F", "Find in the scene view");
        private readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
        private readonly static GUIContent gizmoToggleBtnContent = new GUIContent("G", "Toggle gizmos and visualisations on/off for all items in the scene view");
        private readonly static GUIContent selectBtnContent = new GUIContent("S", "Toggle selection on/off in the scene view");
        private readonly static GUIContent modifyToggleBtnContent = new GUIContent("M", "Modify an attribute of all selected Locations");
        private readonly static GUIContent reversePathBtnContent = new GUIContent("<->", "Reverse the direction of the path");
        private readonly static GUIContent filterContent = new GUIContent("<b>Filter</b>", "Filter the items in the list by entering part of the item name");

        private readonly static GUIContent modifySelectedPosYContent = new GUIContent(" New Position Y-axis", "The new Position Y value for all selected Locations in this Path");
        private readonly static GUIContent modifySelectedAddPosYContent = new GUIContent(" Add Position Y-axis", "Add (or subtract) amount to Position Y value for all selected Locations in this Path");

        #endregion

        #region GUIContent - Locations
        private readonly static GUIContent locationHeaderContent = new GUIContent("To create a Location, move the mouse over scene view and press the + key. " +
                        "The scene window needs to have the focus. Hold SHIFT key to multi-select Locations.");
        private readonly static GUIContent locationDefaultNormalOffsetContent = new GUIContent("Default Normal Offset", "The distance to place a new Location away from the line of sight object");
        private readonly static GUIContent locationDataNameContent = new GUIContent("Location Name", "Name of the Location. E.g. Enemy Base, Route Marker 2, Command Post");
        private readonly static GUIContent locationDataPositionContent = new GUIContent("Location Position", "World-space coordinates of the Location");
        private readonly static GUIContent locationDataIsRadarEnabledContent = new GUIContent("Visible to Radar", "Is this Location visible to the radar system?");
        private readonly static GUIContent locationDataRadarBlipSizedContent = new GUIContent("Radar Blip Size", "The relative size of the blip on the radar mini-map.");
        private readonly static GUIContent locationDataFactionIdContent = new GUIContent("Faction Id", "The faction or alliance the Location belongs to. This can be used to identify if a Location is friend or foe. Neutral = 0.");
        #endregion

        #region GUIContent - Paths
        private readonly static GUIContent pathDataHeaderContent = new GUIContent("A Path is an ordered list of Locations. Add a new Path, select the Scene view, move the mouse over the " +
                                           " Scene, and press the + key where you want to add Locations. OR Add a Location slot in the Inspector before assigning an existing Location to that slot. " +
                                           " Deleting a Location slot does not delete the Location. There is also a context menu in the scene view. Hold SHIFT key to move all selected Locations in a Path.");
        private readonly static GUIContent pathDataNameContent = new GUIContent("Path Name", "Name of the path.");
        private readonly static GUIContent pathDataActiveContent = new GUIContent("A", "Make this the Path active for editing in the scene");
        private readonly static GUIContent pathDataActivateContent = new GUIContent("Activate for editing", "Make this the Path active for editing in the scene");
        private readonly static GUIContent pathDataActivatedContent = new GUIContent("Stop Editing", "Stop editing this Path in the scene");
        private readonly static GUIContent pathDataExportJsonContent = new GUIContent("Export Json", "Export the Path to a Json file");
        private readonly static GUIContent pathDataSnapToMeshContent = new GUIContent("Snap To Mesh", "Will attempt to position all locations on the Path above the top mesh in the scene.");
        private readonly static GUIContent pathDataImportJsonContent = new GUIContent("Import", "Import Path data from a Json file (should be from the same version of Sci-Fi Ship Controller)");
        private readonly static GUIContent pathDataTotalDistanceContent = new GUIContent("Total Distance", "The total spline length or distance of the Path");
        private readonly static GUIContent pathDataIsClosedCircuitContent = new GUIContent("Closed Circuit", "Is this Path a closed circuit or loop?");
        private readonly static GUIContent pathDataLineColourContent = new GUIContent("Line Colour", "The colour of the Path drawn in the scene.");
        private readonly static GUIContent pathDataLocationDefaultNormalOffsetContent = new GUIContent("Default Normal Offset", "The distance to place a new Location on the Path away from the line of sight object");
        private readonly static GUIContent pathDataLocationSnapMinMaxHeightContent = new GUIContent("Snap Min/Max Height", "The minimum and maximum heights checked when Snap To Mesh is used.");
        private readonly static GUIContent pathDataShowPointNumberLabelsInSceneContent = new GUIContent("Display Number Labels", "Display the Location number labels in the scene view");
        private readonly static GUIContent pathDataShowPointNameLabelsInSceneContent = new GUIContent("Display Name Labels", "Display the Location name labels in the scene view");
        private readonly static GUIContent pathDataShowDistanceLabelsInSceneContent = new GUIContent("Display Distances", "Display cumulative distances in the scene view");

        #endregion

        #region GUIContent - Options
        private readonly static GUIContent optionLocationContent = new GUIContent("<b>Location Options</b>");
        private readonly static GUIContent optionIsAutosizeLocationGizmoContent = new GUIContent("Autosize Gizmos", "Autosize Location gizmos in scene view so they are visible a long way away.");
        private readonly static GUIContent optionLocationGizmoColourContent = new GUIContent("Gizmo Colour", "Colour of the Location gizmos in the scene view");
        private readonly static GUIContent optionFindZoomDistanceContent = new GUIContent("Zoom Distance", "The default zoom distance when using the (F)ind buttons in the inspector, or Zoom content menu in the scene");
        private readonly static GUIContent optionPathContent = new GUIContent("<b>Path Options</b>");
        private readonly static GUIContent optionPathControlGizmoColourContent = new GUIContent("Control Gizmo Colour", "Colour of the Path tangent control gizmos in the scene view");
        private readonly static GUIContent optionPathDisplayResolutionContent = new GUIContent("Display Resolution", "The appromiate length of each Path segment drawn in the scene view");
        private readonly static GUIContent optionPathPrecisionContent = new GUIContent("Path Precision", "Determines how precisely the Path distance should be calculated. Keep as low as possible when editing in the scene.");
        private readonly static GUIContent optionPathControlOffsetContent = new GUIContent("Default Control Offset", "Default control point offset distance from the Path Location");
        //private readonly static GUIContent optionRadarContent = new GUIContent("<b>Radar Options</b>");

        #endregion

        #region Serialized Properties - Locations
        private SerializedProperty locationDataListProp;
        private SerializedProperty locationDefaultNormalOffsetProp;
        private SerializedProperty locationDataProp;
        private SerializedProperty locationDataNameProp;
        private SerializedProperty locationDataPositionProp;
        private SerializedProperty locationDataSelectedProp;
        private SerializedProperty locationDataGUIDHashProp;
        private SerializedProperty isLocationDataListExpandedProp;
        private SerializedProperty locationDataShowInEditorProp;
        private SerializedProperty locationDataShowGizmosInSceneViewProp;
        #endregion

        #region Serialized Properties - Paths
        private SerializedProperty pathDataListProp;
        private SerializedProperty pathDataActiveGUIDHashProp;
        private SerializedProperty pathDataProp;
        private SerializedProperty pathDataNameProp;
        private SerializedProperty pathDataGUIDHashProp;
        private SerializedProperty isPathDataListExpandedProp;
        private SerializedProperty pathDataShowInEditorProp;
        private SerializedProperty pathDataShowLocationsInEditorProp;
        private SerializedProperty pathDataShowInSceneViewProp;
        private SerializedProperty pathDataIsClosedCircuitProp;
        private SerializedProperty pathDataLineColourProp;
        private SerializedProperty pathDataShowNumberLabelsProp;
        private SerializedProperty pathDataShowNameLabelsProp;
        private SerializedProperty pathLocationDataListProp;
        private SerializedProperty pathLocationDataProp;
        private SerializedProperty pathLocationDataLocationProp;
        private SerializedProperty pathDataLocationNameProp;
        private SerializedProperty pathDataLocationPositionProp;
        private SerializedProperty pathDataLocationSelectedProp;
        private SerializedProperty pathDataLocationUnassignedProp;
        private SerializedProperty pathDataLocationGUIDHashProp;
        private SerializedProperty pathDataLocationShowGizmosInSceneViewProp;
        private SerializedProperty pathDataLocationShowInEditorProp;
        #endregion

        #region Serialized Properties - Options
        private SerializedProperty isAutosizeLocationGizmoProp;
        private SerializedProperty locationGizmoColourProp;
        private SerializedProperty findZoomDistanceProp;
        private SerializedProperty defaultPathControlGizmoColourProp;
        private SerializedProperty pathDisplayResolutionProp;
        private SerializedProperty pathPrecisionProp;
        private SerializedProperty defaultPathControlOffsetProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            sscManager = (SSCManager)target;

            // Only use if require scene view interaction
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            SceneView.onSceneGUIDelegate += SceneGUI;
            #endif

            //Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            //Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }

            // Reset guistyles to avoid issues - forces reinitialisation of button styles etc
            helpBoxRichText = null;
            labelFieldRichText = null;
            buttonCompact = null;
            buttonCompactBoldBlue = null;
            foldoutStyleNoLabel = null;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            searchTextFieldStyle = null;
            searchCancelButtonStyle = null;

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region FindProperties
            // Locations
            locationDataListProp = serializedObject.FindProperty("locationDataList");
            locationDefaultNormalOffsetProp = serializedObject.FindProperty("locationDefaultNormalOffset");
            isLocationDataListExpandedProp = serializedObject.FindProperty("isLocationDataListExpanded");

            // Paths
            pathDataListProp = serializedObject.FindProperty("pathDataList");
            isPathDataListExpandedProp = serializedObject.FindProperty("isPathDataListExpanded");
            pathDataActiveGUIDHashProp = serializedObject.FindProperty("pathDataActiveGUIDHash");

            // Options
            isAutosizeLocationGizmoProp = serializedObject.FindProperty("isAutosizeLocationGizmo");
            locationGizmoColourProp = serializedObject.FindProperty("locationGizmoColour");
            findZoomDistanceProp = serializedObject.FindProperty("findZoomDistance");
            defaultPathControlGizmoColourProp = serializedObject.FindProperty("defaultPathControlGizmoColour");
            defaultPathControlOffsetProp = serializedObject.FindProperty("defaultPathControlOffset");
            pathDisplayResolutionProp = serializedObject.FindProperty("pathDisplayResolution");
            pathPrecisionProp = serializedObject.FindProperty("pathPrecision");
            #endregion

            sscHelpPDF = SSCEditorHelper.GetHelpURL();

            // Deselect all Locations in the scene view
            //SelectAllLocations(false, false);

            // Turn off the gizmos for the selected Path Gameobject
            Tools.hidden = true;
            // Switch to the Move tool
            Tools.current = Tool.Move;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // This is a workaround to make the scene active so that
                // adding locations works first time without first having to
                // select the scene (right click, click the scene window title etc)
                isSetFocusEnabled = true;
            }
        }

        /// <summary>
        /// Called automatically by Unity when the gameobject loses focus
        /// </summary>
        private void OnDisable()
        {
            // Turn on the default scene handles
            Tools.hidden = false;
            Tools.current = Tool.Move;
        }

        /// <summary>
        /// Called when the gameobject loses focus or Unity Editor enters/exits
        /// play mode
        /// </summary>
        private void OnDestroy()
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
        private void OnInspectorUpdate()
        {
            // OnInspectorGUI() only registers events when the mouse is positioned over the custom editor window
            // This code forces OnInspectorGUI() to run every frame, so it registers events even when the mouse
            // is positioned over the scene view
            if (sscManager != null && sscManager.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Public Static Methods
        // Add a menu item so that a SSCManager can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sci-Fi Ship Controller/SSC Manager")]
        public static void CreateSSCManager()
        {
            SSCManager mgr = SSCManager.GetOrCreateManager();
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);
                EditorUtility.SetDirty(mgr);
            }
        }
        #endregion

        #region Private Member Methods

        /// <summary>
        /// Add (or subtract) to the y-axis position for all the selected Locations in the Path
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="searchFilter"></param>
        /// <param name="addPositionY"></param>
        private void AddPosYSelectList(PathData pathData, float addPositionY)
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(sscManager, "Path y-axis on selected");

            int numInList = pathData == null || pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            for (int idx = 0; idx < numInList; idx++)
            {
                // Get the PathLocationData class instance from the list
                PathLocationData pathLocationData = pathData.pathLocationDataList[idx];

                // Get the LocationData class reference from within the PathLocationData class instance
                LocationData locationData = pathLocationData.locationData;

                if (!locationData.isUnassigned && locationData.selectedInSceneView)
                {
                    Vector3 wsPosition = locationData.position;
                    wsPosition.y += addPositionY;

                    locationData.position = wsPosition;

                    // Update the control points to match the new Y position
                    Vector3 newControlPoint = pathLocationData.inControlPoint;
                    newControlPoint.y += addPositionY;
                    pathLocationData.inControlPoint = newControlPoint;
                    pathLocationData.outControlPoint = wsPosition + ((wsPosition - newControlPoint).normalized * (wsPosition - pathLocationData.outControlPoint).magnitude);
                }
            }

            if (numInList > 0) { sscManager.RefreshPathDistances(pathData); }
        }

        /// <summary>
        /// Attempt to find a path for the first selected location, and being to edit it
        /// </summary>
        private void EditLocationPath()
        {
            if (sscManager != null)
            {
                int numLocations = sscManager.locationDataList == null ? 0 : sscManager.locationDataList.Count;
                int numPaths = sscManager.pathDataList == null ? 0 : sscManager.pathDataList.Count;

                // If no paths or no locations, then nothing to edit
                if (numPaths > 0 && numLocations > 0)
                {
                    LocationData locationData = null;

                    for (int locIdx = 0; locIdx < numLocations; locIdx++)
                    {
                        locationData = sscManager.locationDataList[locIdx];
                        int locDataGUIDHash = locationData.guidHash;
                        if (locationData != null && locationData.selectedInSceneView)
                        {
                            // Is this location a member of a path?
                            for (int pIdx = 0; pIdx < numPaths; pIdx++)
                            {
                                PathData pathData = sscManager.pathDataList[pIdx];
                                if (pathData != null && pathData.pathLocationDataList != null && pathData.pathLocationDataList.Exists(locData => !locData.locationData.isUnassigned && locData.locationData.guidHash == locDataGUIDHash))
                                {
                                    // Found a path for this selected location
                                    // Set as active path
                                    sscManager.pathDataActiveGUIDHash = pathData.guidHash;
                                    // Make sure we're on the Path tab
                                    selectedTabInt = 1;
                                    // Get out of these loops and exit the function
                                    locIdx = numLocations;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the Position Y from the first selected locations in the path list of locations
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="searchFilter"></param>
        private float GetPosYSelectList(SerializedProperty listProp)
        {
            float positionY = 0f;
            if (listProp != null)
            {
                int numInList = listProp.arraySize;

                for (int idx = 0; idx < numInList; idx++)
                {
                    // Get the PathLocationData class instance from the list
                    pathLocationDataProp = listProp.GetArrayElementAtIndex(idx);
                    // Get the LocationData class reference from within the PathLocationData class instance
                    pathLocationDataLocationProp = pathLocationDataProp.FindPropertyRelative("locationData");

                    pathDataLocationPositionProp = pathLocationDataLocationProp.FindPropertyRelative("position");
                    pathDataLocationSelectedProp = pathLocationDataLocationProp.FindPropertyRelative("selectedInSceneView");
                    pathDataLocationUnassignedProp = pathLocationDataLocationProp.FindPropertyRelative("isUnassigned");

                    if (!pathDataLocationUnassignedProp.boolValue && pathDataLocationSelectedProp.boolValue)
                    {
                        positionY = pathDataLocationPositionProp.vector3Value.y;
                        break;
                    }

                }
            }
            return positionY;
        }

        /// <summary>
        /// Set all the selected Locations in the Path to have a new y-axis position
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="searchFilter"></param>
        /// <param name="newPositionY"></param>
        private void SetPosYSelectList(PathData pathData, float newPositionY)
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(sscManager, "Path y-axis on selected");

            int numInList = pathData == null || pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            for (int idx = 0; idx < numInList; idx++)
            {
                // Get the PathLocationData class instance from the list
                PathLocationData pathLocationData = pathData.pathLocationDataList[idx];

                // Get the LocationData class reference from within the PathLocationData class instance
                LocationData locationData = pathLocationData.locationData;

                if (!locationData.isUnassigned && locationData.selectedInSceneView)
                {
                    Vector3 wsPosition = locationData.position;
                    wsPosition.y = newPositionY;

                    locationData.position = wsPosition;

                    // Update the control points to match the new Y position
                    Vector3 newControlPoint = pathLocationData.inControlPoint;
                    newControlPoint.y = wsPosition.y;
                    pathLocationData.inControlPoint = newControlPoint;
                    pathLocationData.outControlPoint = wsPosition + ((wsPosition - newControlPoint).normalized * (wsPosition - pathLocationData.outControlPoint).magnitude);
                }
            }

            if (numInList > 0) { sscManager.RefreshPathDistances(pathData); }
        }

        private void SceneGUI(SceneView sv)
        {
            if (sscManager != null && sscManager.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;              
                Event currentEvent = Event.current;
                bool isRightButton = (currentEvent.button == 1);
                bool isCtrlKey = currentEvent.control;
                bool isShiftKey = currentEvent.shift;

                // Make the scene active so that it receives key events correctly
                // when the sscmanger is selected
                if (isSetFocusEnabled) { sv.Focus(); isSetFocusEnabled = false; }

                #region Context Menu
                if (currentEvent.type == EventType.MouseDown && isRightButton && Tools.current != Tool.View && Tools.current != Tool.None)
                {
                    // If user has Path tab selected, see if there is an active path
                    PathData activePath = selectedTabInt == 1 ? sscManager.GetPath(sscManager.pathDataActiveGUIDHash) : null;
                    bool isEditPath = activePath != null;

                    GenericMenu menu = new GenericMenu();
                    menu.AddDisabledItem(new GUIContent(isEditPath ? (string.IsNullOrEmpty(activePath.name) ? "Edit (unnamed) Path" : activePath.name) : "Edit Locations")); // Header
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Stop Editing Path"), false, () => { sscManager.pathDataActiveGUIDHash = -1; isEditPath = false; activePath = null; } );
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Start Editing Path for location"), false, () => { EditLocationPath(); });
                    }

                    #region Context Menu - Selection
                    menu.AddSeparator("");
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Select All (Active Path)"), false, () => { SelectPathLocations(activePath, true); });
                        menu.AddItem(new GUIContent("Unselect All (Active Path)"), false, () => { SelectPathLocations(activePath, false); });
                    }
                    menu.AddItem(new GUIContent("Select All Locations"), false, () => { SelectAllLocations(true, false); });
                    menu.AddItem(new GUIContent("Unselect All Locations"), false, () => { SelectAllLocations(false, false); });
                    #endregion

                    #region Context Menu - Add Location
                    menu.AddSeparator("");
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Insert Before 1st Selected (Active Path)"), false, () =>
                        {
                            int _firstSelected = activePath.pathLocationDataList.FindIndex(locData => locData.locationData.selectedInSceneView == true);

                            if (_firstSelected >= 0)
                            {
                                Undo.RecordObject(sscManager, "Insert in Active Path");
                                LocationData locationData = sscManager.InsertLocationAt(activePath, _firstSelected, true, true);
                                if (locationData != null) { locationData.selectedInSceneView = true; }
                                isSceneDirtyRequired = true;
                            }
                        }
                        );

                        menu.AddItem(new GUIContent("Append Location (Active Path)"), false, () =>
                        {
                            Vector3 posWS = Vector3.zero;
                            if (SSCEditorHelper.GetPositionFromMouse(sv, currentEvent.mousePosition, sscManager.locationDefaultNormalOffset, ref posWS, true))
                            {
                                currentEvent.Use();
                                Undo.RecordObject(sscManager, "Append Path Location");
                                
                                // Unselect existing path locations so only extended location path is selected afterwards
                                SelectPathLocations(activePath, false);

                                LocationData locationData = sscManager.AppendLocation(activePath, posWS, true, true);
                                if (locationData != null) { locationData.selectedInSceneView = true; }
                                isSceneDirtyRequired = true;
                            }
                        });

                        menu.AddItem(new GUIContent("Extend Active Path"), false, () =>
                        {
                            int numPathPoints = activePath.pathLocationDataList == null ? 0 : activePath.pathLocationDataList.Count;
                            Vector3 posWS = Vector3.zero;

                            // If no points in Path add one at the current mouse point
                            if (numPathPoints < 1)
                            {
                                if (SSCEditorHelper.GetPositionFromMouse(sv, currentEvent.mousePosition, sscManager.locationDefaultNormalOffset, ref posWS, true))
                                {
                                    currentEvent.Use();
                                    Undo.RecordObject(sscManager, "Append Path Location");
                                    LocationData locationData = sscManager.AppendLocation(activePath, posWS, true, true);
                                    if (locationData != null) { locationData.selectedInSceneView = true; }
                                    isSceneDirtyRequired = true;
                                }
                            }
                            else
                            {
                                Undo.RecordObject(sscManager, "Extend Active Path");
                                LocationData locationData = sscManager.AppendLocation(activePath, true, true);
                                if (locationData != null) { locationData.selectedInSceneView = true; }
                                isSceneDirtyRequired = true;
                            }
                        });
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add Location"), false, () =>
                        {
                            Vector3 posWS = Vector3.zero;
                            if (SSCEditorHelper.GetPositionFromMouse(sv, currentEvent.mousePosition, sscManager.locationDefaultNormalOffset, ref posWS, true))
                            {
                                currentEvent.Use();
                                Undo.RecordObject(sscManager, "Add Location");
                                LocationData locationData = sscManager.AppendLocation(posWS, true);
                                if (locationData != null) { locationData.selectedInSceneView = true; }
                                isSceneDirtyRequired = true;
                            }
                        });
                    }
                    #endregion

                    #region Context Menu - Control Points and Snap Location(s)
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Set Controls to Location Y (Selected)"), false, () =>
                        {
                            SetPathControlPointsY(activePath);
                            isSceneDirtyRequired = true;
                        });

                        menu.AddItem(new GUIContent("Snap Locations to Mesh Below (Selected)"), false, () =>
                        {
                            sscManager.SnapPathSelectedLocationsToMesh(activePath, Vector3.up, ~0, true);
                            isSceneDirtyRequired = true;
                        });
                    }
                    #endregion

                    menu.AddSeparator("");

                    #region Context Menu - Display
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Display/Distances"), activePath.showDistancesInScene, () =>
                        {
                            activePath.showDistancesInScene = !activePath.showDistancesInScene;
                            isSceneDirtyRequired = true;
                        });
                        menu.AddItem(new GUIContent("Display/Number Labels"), activePath.showPointNumberLabelsInScene, () =>
                        {
                            activePath.showPointNumberLabelsInScene = !activePath.showPointNumberLabelsInScene;
                            if (activePath.showPointNumberLabelsInScene) { activePath.showPointNameLabelsInScene = false; }
                            isSceneDirtyRequired = true;
                        });
                        menu.AddItem(new GUIContent("Display/Name Labels"), activePath.showPointNameLabelsInScene, () =>
                        {
                            activePath.showPointNameLabelsInScene = !activePath.showPointNameLabelsInScene;
                            if (activePath.showPointNameLabelsInScene) { activePath.showPointNumberLabelsInScene = false; }
                            isSceneDirtyRequired = true;
                        });
                    }
                    #endregion

                    #region Context Menu - Zoom
                    menu.AddItem(new GUIContent("Zoom/Selected Locations"), false, () => { ZoomExtent(sv, false, null); });
                    if (isEditPath)
                    {
                        menu.AddItem(new GUIContent("Zoom/Active Path"), false, () => { ZoomExtent(sv, true, activePath); });
                    }
                    #endregion

                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete All Selected Locations"), false, () => { DeleteSelectedLocations(); sv.Focus(); });
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Allow Scene View Rotation"), false, () => { Tools.current = Tool.View; });

                    // The Cancel option is not really necessary as use can just click anywhere else. However, it may help some users.
                    menu.AddItem(new GUIContent("Cancel"), false, () => { });
                    menu.ShowAsContext();
                    currentEvent.Use();
                }
                #endregion

                #region Add Locations
                // Did the user press "+" key to add a location in the scene?
                // Allow for + or = which are mostly the same key (+ is typically shift and '=' key).
                else if (currentEvent.type == EventType.KeyUp && (currentEvent.keyCode == KeyCode.Equals || currentEvent.keyCode == KeyCode.KeypadPlus))
                {
                    Vector3 posWS = Vector3.zero;
                    if (SSCEditorHelper.GetPositionFromMouse(sv, currentEvent.mousePosition, sscManager.locationDefaultNormalOffset, ref posWS, true))
                    {
                        currentEvent.Use();

                        // If user has Path tab selected, see if there is an active path
                        PathData activePath = selectedTabInt == 1 ? sscManager.GetPath(sscManager.pathDataActiveGUIDHash) : null;

                        Undo.RecordObject(sscManager, activePath != null ? "Add Path Location" : "Add Location");
                        // If the user has Path tab selected and there is an active Path, add the new Location to the Path,
                        // else just create a new Location in the scene.
                        LocationData locationData = activePath != null ? sscManager.AppendLocation(activePath, posWS, true, true) : sscManager.AppendLocation(posWS, true);
                        if (locationData != null) { locationData.selectedInSceneView = true; }
                    }
                  
                    isSceneDirtyRequired = true;
                }
                #endregion Add Locations

                #region Modify Locations and Active Path in the scene
                else
                {
                    #region Locations in the scene
                    int numLocations = sscManager.locationDataList == null ? 0 : sscManager.locationDataList.Count;

                    // If cannot find the camera, use the middle of the sceneview (not ideal but better than nothing)
                    Vector3 svCamPos = sv.camera != null ? sv.camera.transform.position : sv.pivot;
                    fadedGizmoColour = sscManager.locationGizmoColour;

                    using (new Handles.DrawingScope(sscManager.locationGizmoColour))
                    {
                        for (int lnIdx = 0; lnIdx < numLocations; lnIdx++)
                        {
                            locationDataItem = sscManager.locationDataList[lnIdx];

                            if (locationDataItem != null)
                            {
                                if (locationDataItem.showGizmosInSceneView)
                                {
                                    itemHandlePosition = locationDataItem.position;

                                    // Currently rotation and scale aren't being used.
                                    itemHandleRotation = Quaternion.identity;
                                    itemHandleScale.x = 0f;
                                    itemHandleScale.y = 0f;
                                    itemHandleScale.z = 0f;

                                    // Draw location in the scene that is non-interactable
                                    //if (Event.current.type == EventType.Repaint)
                                    //{
                                    //    //Handles.ArrowHandleCap(0, itemHandlePosition, itemHandleRotation, 1f, EventType.Repaint);
                                    //    Handles.SphereHandleCap(0, itemHandlePosition, itemHandleRotation, 1f, EventType.Repaint);
                                    //}

                                    // Here is where we would draw any shape that identifies this location type

                                    if (locationDataItem.selectedInSceneView)
                                    {
                                        // Choose which handle to draw based on which Unity tool is selected
                                        if (Tools.current == Tool.Rotate)
                                        {
                                            // Currently not required
                                        }
                                        else if (Tools.current == Tool.Scale)
                                        {
                                            // Currently not required
                                        }
                                        else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                        {
                                            EditorGUI.BeginChangeCheck();
                                            // Draw a movement handle
                                            itemHandlePosition = Handles.PositionHandle(itemHandlePosition, itemHandleRotation);
                                            // Use the position handle to edit the position of the location
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                isSceneDirtyRequired = true;

                                                if (currentEvent.shift)
                                                {
                                                    // Move all selected Locations in the Active Path
                                                    if (selectedTabInt == 1)
                                                    {
                                                        // This could be a little slow
                                                        PathData activePath = sscManager.GetPath(sscManager.pathDataActiveGUIDHash);
                                                        if (activePath != null)
                                                        {
                                                            Undo.RecordObject(sscManager, "Move Path Locations");
                                                            sscManager.MoveLocations(activePath, locationDataItem, itemHandlePosition, true);
                                                        }
                                                    }
                                                    // Move all selected Locations in the scene
                                                    else
                                                    {
                                                        Undo.RecordObject(sscManager, "Move Selected Locations");
                                                        sscManager.MoveLocations(locationDataItem, itemHandlePosition, true);
                                                    }
                                                }
                                                else
                                                {
                                                    Undo.RecordObject(sscManager, "Move Location");
                                                    // WARNING: Refreshing the Distances could get expensive on long Paths..
                                                    sscManager.UpdateLocation(locationDataItem, itemHandlePosition, true);
                                                }
                                            }
                                        }
                                    }

                                    float itemHandleSize = sscManager.isAutosizeLocationGizmo ? HandleUtility.GetHandleSize(itemHandlePosition) * 0.1f : 0.5f;

                                    // Determine how far this item is from the scene view camera and set the alpha value of the buttons
                                    float sqrDistToCamera = ((svCamPos.x - itemHandlePosition.x) * (svCamPos.x - itemHandlePosition.x)) + ((svCamPos.y - itemHandlePosition.y) * (svCamPos.y - itemHandlePosition.y)) + ((svCamPos.z - itemHandlePosition.z) * (svCamPos.z - itemHandlePosition.z));
                                    float gizmoAlpha = 1f - (sscManager.locationGizmoColour.a - 0.1f) * ((sqrDistToCamera - startFadeDistSqr) / maxFadeDistSqr);
                                    fadedGizmoColour.a = gizmoAlpha < 0.1f ? 0.1f : gizmoAlpha > 1f ? 1f : gizmoAlpha;

                                    using (new Handles.DrawingScope(fadedGizmoColour))
                                    {
                                        // Allow the user to select/deselect the location
                                        if (Handles.Button(itemHandlePosition, Quaternion.identity, itemHandleSize, itemHandleSize, Handles.SphereHandleCap))
                                        {
                                            if (locationDataItem.selectedInSceneView)
                                            {
                                                // If in multi-select mode or single selected, just unselect the clicked Location
                                                locationDataItem.selectedInSceneView = false;
                                                locationDataItem.showInEditor = false;
                                            }
                                            else
                                            {
                                                if (!isShiftKey)
                                                {
                                                    // If not in multi-select mode (Shift key held down), unselect all
                                                    SelectAllLocations(false, true);
                                                    ExpandList(sscManager.locationDataList, false);
                                                }
                                                // Select the Location clicked on
                                                locationDataItem.selectedInSceneView = true;
                                                locationDataItem.showInEditor = true;
                                                isSceneDirtyRequired = true;
                                                // Hide Unity tools
                                                Tools.hidden = true;
                                            }

                                            isSceneDirtyRequired = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion Locations in the scene

                    #region Edit Tangent Control Points for Active Path in the scene
                    // NOTE: Path lines are drawn in SSManager.OnDrawGizmosSelected()
                    using (new Handles.DrawingScope(sscManager.defaultPathControlGizmoColour))
                    {
                        PathData pathData = sscManager.GetPath(sscManager.pathDataActiveGUIDHash);
                        // Only show the tangents for the active Path which has show Gizmos enabled
                        if (pathData != null && pathData.showGizmosInSceneView)
                        {
                            int numPathLocationDatas = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                            // We "could" only set this once but it may have issues going in and out of play mode in the editor
                            if (pathData.showPointNumberLabelsInScene || pathData.showPointNameLabelsInScene || pathData.showDistancesInScene)
                            {
                                distanceLabel = new GUIStyle("Box");
                                distanceLabel.fontSize = 10;
                                distanceLabel.border = new RectOffset(1, 1, 1, 1);
                                distanceLabel.onFocused.textColor = UnityEngine.Color.white;
                            }

                            fadedGizmoColour = sscManager.defaultPathControlGizmoColour;

                            for (int pldIdx = 0; pldIdx < numPathLocationDatas; pldIdx++)
                            {
                                PathLocationData pathLocationData = pathData.pathLocationDataList[pldIdx];
                                if (pathLocationData != null && pathLocationData.locationData != null && !pathLocationData.locationData.isUnassigned)
                                {
                                    float controlHandleSize = sscManager.isAutosizeLocationGizmo ? HandleUtility.GetHandleSize(pathLocationData.inControlPoint) * 0.07f : 0.4f;
                                    locationPosition = pathLocationData.locationData.position;

                                    // Determine how far this Location is from the scene view camera and set the alpha value of the buttons, handles and Control Point lines
                                    float sqrDistToCamera = ((svCamPos.x - locationPosition.x) * (svCamPos.x - locationPosition.x)) + ((svCamPos.y - locationPosition.y) * (svCamPos.y - locationPosition.y)) + ((svCamPos.z - locationPosition.z) * (svCamPos.z - locationPosition.z));
                                    float gizmoAlpha = 1f - (sscManager.defaultPathControlGizmoColour.a - 0.1f) * ((sqrDistToCamera - startFadeDistSqr) / maxFadeDistSqr);
                                    fadedGizmoColour.a = gizmoAlpha < 0.1f ? 0.1f : gizmoAlpha > 1f ? 1f : gizmoAlpha;

                                    using (new Handles.DrawingScope(fadedGizmoColour))
                                    {
                                        #region FreeMove Control Handles
                                        if (Tools.current == Tool.View)
                                        {
                                            EditorGUI.BeginChangeCheck();
                                            #if UNITY_2022_1_OR_NEWER
                                            newControlPoint = Handles.FreeMoveHandle(pathLocationData.inControlPoint, controlHandleSize, controlHandleSnap, Handles.SphereHandleCap);
                                            #else
                                            newControlPoint = Handles.FreeMoveHandle(pathLocationData.inControlPoint, Quaternion.identity, controlHandleSize, controlHandleSnap, Handles.SphereHandleCap);
                                            #endif
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                isSceneDirtyRequired = true;
                                                UpdatePathControlPoint(pathLocationData, newControlPoint, true);
                                                pathData.isDirty = true;
                                            }

                                            EditorGUI.BeginChangeCheck();
                                            #if UNITY_2022_1_OR_NEWER
                                            newControlPoint = Handles.FreeMoveHandle(pathLocationData.outControlPoint, controlHandleSize, controlHandleSnap, Handles.SphereHandleCap);
                                            #else
                                            newControlPoint = Handles.FreeMoveHandle(pathLocationData.outControlPoint, Quaternion.identity, controlHandleSize, controlHandleSnap, Handles.SphereHandleCap);
                                            #endif
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                isSceneDirtyRequired = true;
                                                UpdatePathControlPoint(pathLocationData, newControlPoint, false);
                                                pathData.isDirty = true;
                                            }
                                        }
                                        #endregion

                                        #region Position Move Control Handles
                                        else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                        {
                                            // If the Location is selected, turn off the control handles
                                            // Check if in/out controls are enabled before marking scene dirty.
                                            if (pathLocationData.locationData.selectedInSceneView && (pathLocationData.inControlSelectedInSceneView || pathLocationData.outControlSelectedInSceneView))
                                            {
                                                pathLocationData.inControlSelectedInSceneView = false;
                                                pathLocationData.outControlSelectedInSceneView = false;
                                                isSceneDirtyRequired = true;
                                            }

                                            // If the controls are selected, display the movable Position Handles
                                            if (pathLocationData.inControlSelectedInSceneView)
                                            {
                                                EditorGUI.BeginChangeCheck();
                                                newControlPoint = Handles.PositionHandle(pathLocationData.inControlPoint, Quaternion.identity);
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    isSceneDirtyRequired = true;
                                                    UpdatePathControlPoint(pathLocationData, newControlPoint, true);
                                                    pathData.isDirty = true;
                                                }
                                            }

                                            if (pathLocationData.outControlSelectedInSceneView)
                                            {
                                                EditorGUI.BeginChangeCheck();
                                                newControlPoint = Handles.PositionHandle(pathLocationData.outControlPoint, Quaternion.identity);
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    isSceneDirtyRequired = true;
                                                    UpdatePathControlPoint(pathLocationData, newControlPoint, false);
                                                    pathData.isDirty = true;
                                                }
                                            }

                                            // Buttons which let you select or unselect the control points. When selected the above handles appear
                                            if (Handles.Button(pathLocationData.inControlPoint, Quaternion.identity, controlHandleSize, controlHandleSize, Handles.SphereHandleCap))
                                            {
                                                ToggleControl(pathData, pathLocationData, true);
                                                isSceneDirtyRequired = true;
                                            }

                                            if (Handles.Button(pathLocationData.outControlPoint, Quaternion.identity, controlHandleSize, controlHandleSize, Handles.SphereHandleCap))
                                            {
                                                ToggleControl(pathData, pathLocationData, false);
                                                isSceneDirtyRequired = true;
                                            }
                                        }
                                        #endregion

                                        Handles.DrawLine(pathLocationData.inControlPoint, locationPosition);
                                        Handles.DrawLine(pathLocationData.outControlPoint, locationPosition);
                                    }

                                    #region Display labels
                                    if (pathData.showPointNumberLabelsInScene || pathData.showPointNameLabelsInScene || pathData.showDistancesInScene)
                                    {
                                        // Only show the Point labels if they are enabled in Editor AND infront of the scene view camera
                                        // This prevents a ghosted handle being displayed when it is behind the camera.
                                        if (SSCUtils.IsPointInCameraView(SceneView.lastActiveSceneView.camera, locationPosition))
                                        {
                                            if (pathData.showPointNameLabelsInScene) { locationName = string.IsNullOrEmpty(pathLocationData.locationData.name) ? "no name" : pathLocationData.locationData.name.Trim(); }

                                            // Display Point number and Distance labels
                                            if (pathData.showPointNumberLabelsInScene && pathData.showDistancesInScene)
                                            {
                                                if (pldIdx == 0 && pathData.isClosedCircuit)
                                                {
                                                    Handles.Label(locationPosition + pointLabelOffset, "Pt: " + (pldIdx + 1).ToString("000") + "  Distance: 0.00 [Total " + pathLocationData.distanceCumulative.ToString("0.00") + "]", distanceLabel);
                                                }
                                                else
                                                {
                                                    Handles.Label(locationPosition + pointLabelOffset, "Pt: " + (pldIdx + 1).ToString("000") + "  Distance: " + pathLocationData.distanceCumulative.ToString("0.00"), distanceLabel);
                                                }
                                            }
                                            // Display Point name and Distance labels
                                            else if (pathData.showPointNameLabelsInScene && pathData.showDistancesInScene)
                                            {
                                                if (pldIdx == 0 && pathData.isClosedCircuit)
                                                {
                                                    Handles.Label(locationPosition + pointLabelOffset, locationName + "  Distance: 0.00 [Total " + pathLocationData.distanceCumulative.ToString("0.00") + "]", distanceLabel);
                                                }
                                                else
                                                {
                                                    Handles.Label(locationPosition + pointLabelOffset, locationName + "  Distance: " + pathLocationData.distanceCumulative.ToString("0.00"), distanceLabel);
                                                }
                                            }
                                            else if (pathData.showPointNumberLabelsInScene)
                                            {
                                                Handles.Label(locationPosition + pointLabelOffset, "Pt: " + (pldIdx + 1).ToString("000"), distanceLabel);
                                            }
                                            else if (pathData.showPointNameLabelsInScene)
                                            {
                                                Handles.Label(locationPosition + pointLabelOffset, locationName, distanceLabel);
                                            }
                                            // Just display distance
                                            else if (pldIdx == 0 && pathData.isClosedCircuit)
                                            {
                                                Handles.Label(locationPosition + pointLabelOffset, "Distance: 0.00 [" + pathLocationData.distanceCumulative.ToString("0.00") + "]", distanceLabel);
                                            }
                                            else
                                            {
                                                Handles.Label(locationPosition + pointLabelOffset, "Distance: " + pathLocationData.distanceCumulative.ToString("0.00"), distanceLabel);
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }

                        if (pathData != null && pathData.isDirty) { sscManager.RefreshPathDistances(pathData); }
                    }
                    #endregion Edit Tangent Control Points for Active Path in the scene
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
                // Always unhide Unity tools and deselect all Locations when the object is disabled
                Tools.hidden = false;
                SelectAllLocations(false, true);
            }
        }

        /// <summary>
        /// When Control Points can be manipulated with Position Handles, they need to be selected from
        /// a button. When they are selected, all other Control points (and positions) on the Path should be unselected.
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="controlPathLocationData"></param>
        /// <param name="isInControl"></param>
        private void ToggleControl(PathData pathData, PathLocationData controlPathLocationData, bool isInControl)
        {
            if (pathData != null && controlPathLocationData != null)
            {
                int numPathLocationDatas = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;
                bool isSelected = false;

                // Toggle the InControl Point selection
                if (isInControl)
                {
                    controlPathLocationData.inControlSelectedInSceneView = !controlPathLocationData.inControlSelectedInSceneView;

                    if (controlPathLocationData.inControlSelectedInSceneView)
                    {
                        controlPathLocationData.outControlSelectedInSceneView = false;
                        controlPathLocationData.locationData.selectedInSceneView = false;
                        isSelected = true;
                    }
                }
                // Toggle the OutControl Point selection
                else
                {
                    controlPathLocationData.outControlSelectedInSceneView = !controlPathLocationData.outControlSelectedInSceneView;

                    if (controlPathLocationData.outControlSelectedInSceneView)
                    {
                        controlPathLocationData.inControlSelectedInSceneView = false;
                        controlPathLocationData.locationData.selectedInSceneView = false;
                        isSelected = true;
                    }
                }

                // Only unselect all other positions and Control points if the current Control point has been selected
                for (int pldIdx = 0; isSelected && pldIdx < numPathLocationDatas; pldIdx++)
                {
                    PathLocationData pathLocationData = pathData.pathLocationDataList[pldIdx];
                    // Skip the current Control point being toggled
                    if (pathLocationData != null && pathLocationData.guidHash != controlPathLocationData.guidHash)
                    {
                        // Unselect all other Control points
                        pathLocationData.inControlSelectedInSceneView = false;
                        pathLocationData.outControlSelectedInSceneView = false;

                        // Added SSC v1.06. Unselect Location too
                        if (pathLocationData.locationData != null) { pathLocationData.locationData.selectedInSceneView = false; }
                    }
                }
            }
        }

        /// <summary>
        /// Toggle the selected status of list of filtered Locations based on the first item.
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="searchFilter"></param>
        private void ToggleSelectList(SerializedProperty listProp, string searchFilter)
        {
            if (listProp != null)
            {
                int numInList = listProp.arraySize;
                bool isFirstItem = true;
                bool isSelected = false;

                for (int idx = 0; idx < numInList; idx++)
                {
                    SerializedProperty itemProp = listProp.GetArrayElementAtIndex(idx);
                    if (itemProp != null)
                    {
                        SerializedProperty nameProp = itemProp.FindPropertyRelative("name");

                        if (!SSCEditorHelper.IsInSearchFilter(nameProp, searchFilter)) { continue; }

                        SerializedProperty isSelectedProp = itemProp.FindPropertyRelative("selectedInSceneView");

                        // Get the selction status of the first item in the list
                        if (isFirstItem)
                        {
                            isSelected = isSelectedProp.boolValue;
                            isFirstItem = false;
                        }

                        // Set selectedInSceneView to the toggled value based on the first item in the list
                        isSelectedProp.boolValue = !isSelected;
                    }
                }
            }
        }

        /// <summary>
        /// Select or Deselect all Locations in the scene view edit mode.
        /// If unHideUnityTools is false, Tools are not changed. If you want to hide the Unity Tools,
        /// then it must be done outside this method.
        /// </summary>
        /// <param name="unHideUnityTools"></param>
        private void SelectAllLocations(bool isSelected, bool unHideUnityTools)
        {
            // Avoid situation where sscmanager is destroyed in play mode while it is selected.
            if (sscManager != null)
            {
                int numLocations = sscManager.locationDataList == null ? 0 : sscManager.locationDataList.Count;
                LocationData locationData = null;

                for (int idx = 0; idx < numLocations; idx++)
                {
                    locationData = sscManager.locationDataList[idx];
                    if (locationData != null)
                    {
                        locationData.selectedInSceneView = isSelected;
                    }
                }

                // Added SSC v1.0.6
                if (!isSelected)
                {
                    PathData activePath = numLocations > 0 ? sscManager.GetPath(sscManager.pathDataActiveGUIDHash) : null;
                    DeselectAllControlPoints(activePath);
                }
            }
            // Unhide Unity tools
            if (unHideUnityTools) { Tools.hidden = false; }
        }

        /// <summary>
        /// Deselect all Control Points on a given Path
        /// </summary>
        /// <param name="pathData"></param>
        private static void DeselectAllControlPoints(PathData pathData)
        {
            if (pathData != null)
            {
                int numPathLocationDatas = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;
                for (int pldIdx = 0; pldIdx < numPathLocationDatas; pldIdx++)
                {
                    PathLocationData pathLocationData = pathData.pathLocationDataList[pldIdx];
                    if (pathLocationData != null)
                    {
                        // Unselect all other Control points
                        pathLocationData.inControlSelectedInSceneView = false;
                        pathLocationData.outControlSelectedInSceneView = false;
                    }
                }
            }
        }

        /// <summary>
        /// Select or unselect all the Locations in a Path.
        /// Always unselect in/out Controls for a valid Path Location
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="isSelected"></param>
        private void SelectPathLocations(PathData pathData, bool isSelected)
        {
            if (sscManager != null && pathData != null)
            {
                int numLocations = pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;
                PathLocationData pathLocationData = null;
                LocationData locationData = null;
                for (int idx = 0; idx < numLocations; idx++)
                {
                    pathLocationData = pathData.pathLocationDataList[idx];
                    locationData = pathLocationData == null ? null : pathLocationData.locationData;
                    if (locationData != null)
                    {
                        locationData.selectedInSceneView = isSelected;
                        pathLocationData.inControlSelectedInSceneView = false;
                        pathLocationData.outControlSelectedInSceneView = false;
                    }
                }
            }
        }

        /// <summary>
        /// Expand or collapse all locations within the list.
        /// If it is a list of Paths, always collapse the Locations in the Path
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList(SerializedProperty listProp, bool isExpanded)
        {
            if (listProp != null)
            {
                int numInList = listProp.arraySize;
                for (int idx = 0; idx < numInList; idx++)
                {
                    SerializedProperty itemProp = listProp.GetArrayElementAtIndex(idx);
                    if (itemProp != null)
                    {
                        // Locations and Paths have the same showInEditor field.
                        SerializedProperty showInEditorProp = itemProp.FindPropertyRelative("showInEditor");
                        if (showInEditorProp != null)
                        {
                            showInEditorProp.boolValue = isExpanded;
                        }

                        // If this is Path, then it will have a field to show or hide Locations
                        SerializedProperty showLocationsInEditorProp = itemProp.FindPropertyRelative("showLocationsInEditor");
                        if (showLocationsInEditorProp != null)
                        {
                            showLocationsInEditorProp.boolValue = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Expand or collapse all locations within the list
        /// </summary>
        /// <param name="locationDataList"></param>
        /// <param name="isExpanded"></param>
        private void ExpandList(List<LocationData> locationDataList, bool isExpanded)
        {
            int numInList = locationDataList == null ? 0 : locationDataList.Count;
            for (int idx = 0; idx < numInList; idx++)
            {
                LocationData locationData = locationDataList[idx];
                if (locationData != null)
                {
                    locationData.showInEditor = isExpanded;
                }
            }
        }

        /// <summary>
        /// Toggle all items in the list based on the value of showGizmosInSceneView for the first item.
        /// </summary>
        /// <param name="listProp"></param>
        private void ToggleGizmos(SerializedProperty listProp)
        {
            if (listProp != null)
            {
                int numInList = listProp.arraySize;

                if (numInList > 0)
                {
                    // Examine the first item in the list
                    SerializedProperty itemProp = listProp.GetArrayElementAtIndex(0);
                    if (itemProp != null)
                    {
                        SerializedProperty showGizmosInSceneViewProp = itemProp.FindPropertyRelative("showGizmosInSceneView");

                        if (showGizmosInSceneViewProp != null)
                        {
                            bool showGizmos = showGizmosInSceneViewProp.boolValue;

                            for (int idx = 0; idx < numInList; idx++)
                            {
                                itemProp = listProp.GetArrayElementAtIndex(idx);
                                if (itemProp != null)
                                {
                                    showGizmosInSceneViewProp = itemProp.FindPropertyRelative("showGizmosInSceneView");
                                    if (showGizmosInSceneViewProp != null) { showGizmosInSceneViewProp.boolValue = !showGizmos; }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete all selected Locations. Also removes them from an Paths.
        /// Prompts user for confirmation.
        /// </summary>
        private void DeleteSelectedLocations()
        {
            if (SSCEditorHelper.PromptForDelete("Delete Locations?", "Do you want to delete ALL selected Locations in the scene?\n\nThis will also remove them from any Paths"))
            {
                sscManager.DeleteSelectedLocations(true);
            }
        }

        /// <summary>
        /// Given a location, remove it from all paths in the list supplied.
        /// </summary>
        /// <param name="pathListProp"></param>
        /// <param name="locationGUIDHash"></param>
        private void DeleteLocationFromPaths(SerializedProperty pathListProp, int locationGUIDHash, bool refreshPath)
        {
            if (pathListProp != null)
            {
                // Loop through all the paths
                int numPathsInList = pathListProp.arraySize;
                bool isPathAffected = false;
                for (int idx = 0; idx < numPathsInList; idx++)
                {
                    // Get the Path
                    SerializedProperty _pathProp = pathListProp.GetArrayElementAtIndex(idx);
                    if (_pathProp != null)
                    {
                        // Get the list of Location slots in the Path
                        SerializedProperty _pathLocationDataListProp = _pathProp.FindPropertyRelative("pathLocationDataList");

                        if (_pathLocationDataListProp != null)
                        {
                            isPathAffected = false;
                            // Loop backwards through all the PathLocationData instances in the Path
                            for (int lIdx = _pathLocationDataListProp.arraySize - 1; lIdx >= 0; lIdx--)
                            {
                                // Get the PathLocationData instance
                                SerializedProperty _pathLocationDataProp = _pathLocationDataListProp.GetArrayElementAtIndex(lIdx);
                                if (_pathLocationDataProp != null)
                                {
                                    SerializedProperty _locationDataProp = _pathLocationDataProp.FindPropertyRelative("locationData");
                                    if (_locationDataProp != null)
                                    {
                                        SerializedProperty _locGUIDHashProp = _locationDataProp.FindPropertyRelative("guidHash");
                                        // If we find a match, remove the Location from the Path
                                        if (_locGUIDHashProp != null && _locGUIDHashProp.intValue == locationGUIDHash)
                                        {
                                            _pathLocationDataListProp.DeleteArrayElementAtIndex(lIdx);
                                            isPathAffected = true;
                                        }
                                    }
                                }
                            }

                            if (isPathAffected)
                            {
                                serializedObject.ApplyModifiedProperties();
                                PathData _pathData = sscManager.GetPath(_pathProp.FindPropertyRelative("guidHash").intValue);
                                if (_pathData != null)
                                {
                                    if (refreshPath) { sscManager.RefreshPathDistances(_pathData); }
                                    else { _pathData.isDirty = true; }
                                }
                                serializedObject.Update();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When the Location and Path Location lists are updated, especially during an Undo
        /// operation, the connect between the Location in the Path and the Location in the
        /// Location list can be lost. The method checks for and fixings that issue.
        /// </summary>
        private void VerifyPathConnections()
        {
            if (sscManager != null)
            {
                bool isConnected = false;
                bool isCheckConnection = true; // Used to check the first item for broken connection
                bool isUpdated = false;

                // Loop through all the paths
                int numPathsInList = sscManager.pathDataList == null ? 0 : sscManager.pathDataList.Count;
                for (int idx = 0; !isConnected && idx < numPathsInList; idx++)
                {
                    PathData _pathData = sscManager.pathDataList[idx];
                    if (_pathData != null)
                    {
                        List<PathLocationData> _pathLocationList = _pathData.pathLocationDataList;
                        int numLocationsInList = _pathLocationList == null ? 0 : _pathLocationList.Count;

                        // Loop through all the locations in the Path
                        for (int lIdx = 0; lIdx < numLocationsInList; lIdx++)
                        {
                            // Get the location within the Path
                            PathLocationData _pathLocation = _pathLocationList[lIdx];

                            if (_pathLocation != null)
                            {
                                // Get the guidHash code for the location within the Path
                                // Lookup the Location in the full Location list (not the location list in the Path)
                                LocationData locationDataToAssign = sscManager.locationDataList.Find(l => l.guidHash == _pathLocation.locationData.guidHash);
                                // Only examine assigned slots
                                if (locationDataToAssign != null)
                                {
                                    // Checked and confirmed that first location needs reconnecting.
                                    if (isCheckConnection)
                                    {
                                        // Only do the check once
                                        isCheckConnection = false;
                                        // Is first item check, connected?
                                        if (_pathLocation.locationData == locationDataToAssign)
                                        {
                                            // Assume all are connected
                                            isConnected = true;
                                            //Debug.Log("[DEBUG] " + _pathLocation.name + " is connected");
                                            break;
                                        }
                                    }

                                    if (!isConnected)
                                    {
                                        // If this is the first item to be reconnected, make the sscManager object as dirty in the scene
                                        if (!isUpdated)
                                        {
                                            Undo.RecordObject(sscManager, "Update Path Connections");
                                            isUpdated = true;
                                        }

                                        // Reconnect Path Location with the instance in the Location list
                                        //Debug.Log("[DEBUG] reconnecting " + _pathLocation.locationData.name + " " + Time.realtimeSinceStartup);
                                        _pathLocationList[lIdx].locationData = locationDataToAssign;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the position of an in or out control point. Then reflect this position for the other corresponding in/out control point.
        /// </summary>
        /// <param name="pathLocationData"></param>
        /// <param name="newControlPoint"></param>
        /// <param name="isInControl"></param>
        private void UpdatePathControlPoint(PathLocationData pathLocationData, Vector3 newControlPoint, bool isInControl)
        {
            Undo.RecordObject(sscManager, isInControl ? "Change In Control Point" : "Change Out Control Point");

            Vector3 wsPosition = pathLocationData.locationData.position;

            if (isInControl)
            {
                pathLocationData.inControlPoint = newControlPoint;
                pathLocationData.outControlPoint = wsPosition + ((wsPosition - newControlPoint).normalized * (wsPosition - pathLocationData.outControlPoint).magnitude);
            }
            else
            {
                pathLocationData.outControlPoint = newControlPoint;
                pathLocationData.inControlPoint = wsPosition + ((wsPosition - newControlPoint).normalized * (wsPosition - pathLocationData.inControlPoint).magnitude);
            }
        }

        /// <summary>
        /// Set the control point Y-axis value to the same as the Location
        /// for the selected Locations on the Path.
        /// </summary>
        /// <param name="pathData"></param>
        private void SetPathControlPointsY(PathData pathData)
        {
            int numPathLocationSlots = pathData == null || pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

            if (numPathLocationSlots > 0)
            {
                Undo.RecordObject(sscManager, "Set Control Points Y");

                for (int plIdx = 0; plIdx < numPathLocationSlots; plIdx++)
                {
                    PathLocationData pathLocationData = pathData.pathLocationDataList[plIdx];
                    if (pathLocationData != null && pathLocationData.locationData != null && !pathLocationData.locationData.isUnassigned && pathLocationData.locationData.selectedInSceneView)
                    {
                        Vector3 wsPosition = pathLocationData.locationData.position;
                        Vector3 newControlPoint = pathLocationData.inControlPoint;
                        newControlPoint.y = wsPosition.y;
                        pathLocationData.inControlPoint = newControlPoint;
                        pathLocationData.outControlPoint = wsPosition + ((wsPosition - newControlPoint).normalized * (wsPosition - pathLocationData.outControlPoint).magnitude);
                    }
                }

                sscManager.RefreshPathDistances(pathData);
            }
        }

        /// <summary>
        /// Dropdown menu callback method used when a location is selected for a path
        /// </summary>
        /// <param name="obj"></param>
        private void UpdatePathLocation(object obj)
        {
            // The menu data is passed as 3 integers in a Vector3Int
            // as Path guidHash, path Location LocationData guidHash, and the selected Location guidHash
            // which is being assigned to the location slot on the path.
            if (obj != null && obj.GetType() == typeof(Vector3Int))
            {
                Vector3Int objData = (Vector3Int)obj;

                if (sscManager != null && sscManager.pathDataList != null)
                {
                    // Find the path
                    PathData pathData = sscManager.pathDataList.Find(p => p.guidHash == objData.x);
                    if (pathData != null)
                    {
                        // Look up the guidHash of the LocationData within the PathLocationData (not the hash of the PathLocationData itself)
                        int pathLocIdx = pathData.pathLocationDataList.FindIndex(l => l.locationData.guidHash == objData.y);
                        LocationData locationDataToAssign = sscManager.locationDataList.Find(l => l.guidHash == objData.z);
                        
                        // Replace this existing location reference with a reference to the one being assigned
                        if (pathLocIdx >= 0 && pathLocIdx < pathData.pathLocationDataList.Count)
                        {
                            Undo.RecordObject(sscManager, "Assign Location to Path");
                            pathData.pathLocationDataList[pathLocIdx].locationData = locationDataToAssign;

                            // Reset distances to force a new estimated distance
                            // See ssManager.CalcPathSegments(..) for more details.
                            pathData.pathLocationDataList[pathLocIdx].distanceFromPreviousLocation = 0f;
                            pathData.pathLocationDataList[pathLocIdx].distanceCumulative = 0f;

                            // Set the default tangent control points
                            pathData.pathLocationDataList[pathLocIdx].inControlPoint = locationDataToAssign.position + Vector3.left;
                            pathData.pathLocationDataList[pathLocIdx].outControlPoint = locationDataToAssign.position + Vector3.right;

                            sscManager.RefreshPathDistances(pathData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Zoom to the extent of the selected locations or the current active path
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="zoomActivePath"></param>
        /// <param name="pathData"></param>
        private void ZoomExtent(SceneView sceneView, bool zoomActivePath, PathData pathData)
        {
            if (sceneView != null && sscManager != null)
            {
                Camera svCamera = sceneView.camera;
                if (svCamera != null)
                {
                    List<LocationData> zoomLocations = null;

                    if (zoomActivePath)
                    {
                        // Get all the assigned Locations on the active Path
                        int numPathLocationSlots = pathData == null || pathData.pathLocationDataList == null ? 0 : pathData.pathLocationDataList.Count;

                        if (numPathLocationSlots > 0) { zoomLocations = new List<LocationData>(numPathLocationSlots); }

                        for (int plIdx = 0; plIdx < numPathLocationSlots; plIdx++)
                        {
                            PathLocationData pathLocationData = pathData.pathLocationDataList[plIdx];
                            if (pathLocationData.locationData != null && !pathLocationData.locationData.isUnassigned)
                            {
                                zoomLocations.Add(pathLocationData.locationData);
                            }
                        }
                    }
                    else
                    {
                        // Get selected locations
                        zoomLocations = sscManager.locationDataList.FindAll(l => l.selectedInSceneView == true);
                    }

                    int numSelected = zoomLocations == null ? 0 : zoomLocations.Count;

                    if (numSelected > 0)
                    {
                        // calculate the bounds of the Locations
                        Bounds selectedBounds = new Bounds();
                                                   
                        for (int locIdx = 0; locIdx < numSelected; locIdx++)
                        {
                            // Override the 0,0,0 default centre of an empty Bounds struct.
                            if (locIdx == 0) { selectedBounds.center = zoomLocations[locIdx].position; }
                            selectedBounds.Encapsulate(zoomLocations[locIdx].position);
                        }

                        float size = Mathf.Abs(selectedBounds.size.x);
                        sceneView.size = size < sscManager.findZoomDistance ? sscManager.findZoomDistance : size;

                        // Place the scene view camera 8m above the position of the Location
                        Vector3 pivotPosition = selectedBounds.min;
                        // Raise scene view camera 25% of the height above the selected points
                        pivotPosition.y = selectedBounds.max.y + (selectedBounds.extents.y * 1.25f);
                        // Move the scene view camera 20% of the distance infront of the selected points
                        pivotPosition.z -= selectedBounds.extents.z * 0.2f;
                        sceneView.pivot = pivotPosition;

                        //sceneView.LookAt(selectedBounds.center, Quaternion.Euler(0f, 0f, 0f));
                        sceneView.LookAt(selectedBounds.center);

                        //SSCEditorHelper.PositionSceneView(pos, size, GetType());
                    }
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

            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            // BUG - this is currently preventing selection of Locations AND moving them in list
            VerifyPathConnections();
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
                searchCancelButtonStyle =  new GUIStyle(SSCUtils.ReflectionGetPropertyValue<GUIStyle>(typeof(EditorStyles), "toolbarSearchFieldCancelButton", null, true, true));
                #else
                searchCancelButtonStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));
                #endif
            }

            #endregion

            // Read in all the properties
            serializedObject.Update();

            #region Header
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            #endregion

            #region Debug Mode
            // Place above Locations etc so it is not affected by ScrollView when not many Locations.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);

            if (isDebuggingEnabled && sscManager != null)
            {
                #region Projectiles
                EditorGUILayout.LabelField(projectileHeaderContent, labelFieldRichText);

                // Display the size of the current pool for projectiles that use pooling
                projectileTemplatesList = sscManager.ProjectileTemplatesList;
                int numProjectileTemplates = projectileTemplatesList == null ? 0 : projectileTemplatesList.Count;

                if (numProjectileTemplates > 0)
                {
                    foreach (ProjectileTemplate projectileTemplate in projectileTemplatesList)
                    {
                        if (projectileTemplate.projectilePrefab != null)
                        {
                            EditorGUILayout.LabelField(projectileTemplate.projectilePrefab.name);
                            if (projectileTemplate.projectilePrefab.usePooling)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(projectilePoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                                EditorGUILayout.LabelField(projectileTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                                EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                                EditorGUILayout.LabelField(projectileTemplate.projectilePrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                                EditorGUILayout.LabelField(projectileTemplate.projectilePrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                                EditorGUILayout.EndHorizontal();
                            }
                            else if (projectileTemplate.projectilePrefab.useECS)
                            {
                                #if SSC_ENTITIES
                                EditorGUILayout.LabelField(projectileDOTSContent);
                                #else
                                EditorGUILayout.LabelField(projectileDOTSNotAvailableContent);
                                #endif
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(noneWithPoolingContent, labelFieldRichText);
                }
                EditorGUILayout.Space();

                #endregion

                #region Beams
                EditorGUILayout.LabelField(beamHeaderContent, labelFieldRichText);

                // Display the size of the current pool for beams that use pooling
                beamTemplateList = sscManager.BeamTemplatesList;
                int numBeamTemplates = beamTemplateList == null ? 0 : beamTemplateList.Count;

                if (numBeamTemplates > 0)
                {
                    foreach (BeamTemplate beamTemplate in beamTemplateList)
                    {
                        if (beamTemplate.beamPrefab != null && beamTemplate.beamPrefab.usePooling)
                        {
                            EditorGUILayout.LabelField(beamTemplate.beamPrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(beamPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(beamTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(beamTemplate.beamPrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(beamTemplate.beamPrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(noneWithPoolingContent, labelFieldRichText);
                }

                EditorGUILayout.Space();

                #endregion

                #region Destructs

                EditorGUILayout.LabelField(destructHeaderContent, labelFieldRichText);

                // Display the size of the current pool for destructs that use pooling
                destructTemplateList = sscManager.DestructTemplatesList;
                int numDestructTemplates = destructTemplateList == null ? 0 : destructTemplateList.Count;

                if (numDestructTemplates > 0)
                {
                    foreach (DestructTemplate destructTemplate in destructTemplateList)
                    {
                        if (destructTemplate.destructPrefab != null && destructTemplate.destructPrefab.usePooling)
                        {
                            EditorGUILayout.LabelField(destructTemplate.destructPrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(destructPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(destructTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(destructTemplate.destructPrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(destructTemplate.destructPrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(noneWithPoolingContent, labelFieldRichText);
                }

                EditorGUILayout.Space();

                #endregion

                #region Effects
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(effectsHeaderContent, labelFieldRichText);

                // Display the size of the current pool for effects that use pooling
                effectsObjectTemplatesList = sscManager.EffectsObjectTemplatesList;
                int numEffectObjectTemplates = effectsObjectTemplatesList == null ? 0 : effectsObjectTemplatesList.Count;

                if (numEffectObjectTemplates > 0)
                {
                    foreach (EffectsObjectTemplate effectsObjectTemplate in effectsObjectTemplatesList)
                    {
                        if (effectsObjectTemplate.effectsObjectPrefab != null && effectsObjectTemplate.effectsObjectPrefab.usePooling)
                        {
                            EditorGUILayout.LabelField(effectsObjectTemplate.effectsObjectPrefab.name);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(effectsPoolingContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth));
                            EditorGUILayout.LabelField(effectsObjectTemplate.currentPoolSize.ToString("0000"), GUILayout.MaxWidth(40f));
                            EditorGUILayout.LabelField(minmaxPoolSizeContent, GUILayout.MaxWidth(55f));
                            EditorGUILayout.LabelField(effectsObjectTemplate.effectsObjectPrefab.minPoolSize.ToString("0000"), GUILayout.MaxWidth(32f));
                            EditorGUILayout.LabelField(effectsObjectTemplate.effectsObjectPrefab.maxPoolSize.ToString("0000"), GUILayout.MaxWidth(defaultEditorFieldWidth));

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(noneWithPoolingContent, labelFieldRichText);
                }
                #endregion
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Help Toolbar
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SSCEditorHelper.btnTxtGetSupport, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlGetSupport); }
            if (GUILayout.Button(SSCEditorHelper.btnDiscordContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlDiscordChannel); }
            if (GUILayout.Button(SSCEditorHelper.btnHelpContent, buttonCompact)) { Application.OpenURL(sscHelpPDF); }
            if (GUILayout.Button(SSCEditorHelper.tutorialsURLContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlTutorials); }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Grid Tab control
            //selectedTabInt = -1;
            EditorGUI.BeginChangeCheck();
            selectedTabInt = GUILayout.SelectionGrid(selectedTabInt, tabTexts, 3, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck()) { GUI.FocusControl(null); }
            #endregion

            #region Locations Tab
            if (selectedTabInt == 0)
            {
                if (sscManager.locationDataList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    sscManager.locationDataList = new List<LocationData>(5);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField(locationHeaderContent, helpBoxRichText);

                // Reset button variables
                locationDataMoveDownPos = -1;
                locationDataInsertPos = -1;
                locationDataDeletePos = -1;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                #region Gizmos-Add-Remove Location Buttons

                numLocations = locationDataListProp.arraySize;             
                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                EditorGUIUtility.fieldWidth = 15f;
                EditorGUI.BeginChangeCheck();
                isLocationDataListExpandedProp.boolValue = EditorGUILayout.Foldout(isLocationDataListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    ExpandList(locationDataListProp, isLocationDataListExpandedProp.boolValue);
                }
                EditorGUI.indentLevel -= 1;
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Locations: " + numFilteredLocations + " of " + numLocations + "</b></color>", labelFieldRichText);

                if (numLocations > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        ToggleGizmos(locationDataListProp);
                    }
                }
                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(sscManager, "Add Location");
                    sscManager.locationDataList.Add(new LocationData());
                    // Read in the properties
                    serializedObject.Update();

                    numLocations = locationDataListProp.arraySize;
                    if (numLocations > 0)
                    {
                        // Force new location to be serialized in scene
                        locationDataProp = locationDataListProp.GetArrayElementAtIndex(numLocations - 1);
                        locationDataShowInEditorProp = locationDataProp.FindPropertyRelative("showInEditor");
                        locationDataShowInEditorProp.boolValue = !locationDataShowInEditorProp.boolValue;
                        // Show the new location
                        locationDataShowInEditorProp.boolValue = true;
                        // Select new Locations to make them more visible to user
                        locationDataProp.FindPropertyRelative("selectedInSceneView").boolValue = true;
                        locationDataProp.FindPropertyRelative("name").stringValue = "new location " + numLocations.ToString();
                    }

                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numLocations > 0)
                    {
                        // Get the last location in the list
                        locationDataProp = locationDataListProp.GetArrayElementAtIndex(locationDataListProp.arraySize - 1);

                        // Perform the delete in one place so that we can clean up the paths too.
                        locationDataDeletePos = locationDataListProp.arraySize - 1;
                    }
                }
                GUILayout.EndHorizontal();

                #endregion

                #region Location Search Filter and filtering buttons

                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(2f));
                GUILayout.Label(filterContent, labelFieldRichText, GUILayout.Width(50f));
                if (sscManager.editorSearchLocationFilter == null) { sscManager.editorSearchLocationFilter = string.Empty; }
                sscManager.editorSearchLocationFilter = GUILayout.TextField(sscManager.editorSearchLocationFilter, searchTextFieldStyle);

                // Sometimes the search text is reverse search almost seems invisible (small search icon and cancel cannot be seen)
                // Not sure how this occurs.
                if (GUILayout.Button("", searchCancelButtonStyle))
                {
                    sscManager.editorSearchLocationFilter = string.Empty;
                    GUI.FocusControl(null);
                }

                // Force buttons to be right justified
                GUILayoutUtility.GetRect(1f, 1f);

                // Toggle selection based on the first filtered one in the list
                if (GUILayout.Button(selectBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
                {
                    ToggleSelectList(locationDataListProp, sscManager.editorSearchLocationFilter);
                }
                GUILayout.EndHorizontal();
                // Provide space between previous controls and next ones
                GUILayoutUtility.GetRect(1f, 3f);

                #endregion

                #region General Location Settings
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(2f));
                EditorGUILayout.PropertyField(locationDefaultNormalOffsetProp, locationDefaultNormalOffsetContent);
                GUILayout.EndHorizontal();
                #endregion

                EditorGUILayout.EndVertical();

                #region Location List

                numLocations = locationDataListProp.arraySize;
                numFilteredLocations = 0;
                locationScrollPosition = EditorGUILayout.BeginScrollView(locationScrollPosition);

                for (int idx = 0; idx < numLocations; idx++)
                {
                    #region Get Location Properties (fields) and Filter if required
                    locationDataProp = locationDataListProp.GetArrayElementAtIndex(idx);
                    locationDataNameProp = locationDataProp.FindPropertyRelative("name");

                    // BUG: Debug Mode appear significantly below the end of the location list.
                    // This is due to the use of BeginScrollView which seems to have a minimum height/view area.
                    // Check if the list of Locations is being filtered. Ignore Locations not in the filter
                    if (!SSCEditorHelper.IsInSearchFilter(locationDataNameProp, sscManager.editorSearchLocationFilter)) { continue; }
                    numFilteredLocations++;

                    locationDataPositionProp = locationDataProp.FindPropertyRelative("position");
                    locationDataShowInEditorProp = locationDataProp.FindPropertyRelative("showInEditor");
                    locationDataShowGizmosInSceneViewProp = locationDataProp.FindPropertyRelative("showGizmosInSceneView");
                    locationDataSelectedProp = locationDataProp.FindPropertyRelative("selectedInSceneView");
                    #endregion

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    #region Location Find/Gizmos/Move/Insert/Delete buttons

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.indentLevel += 1;
                    // A Foldout with no label must have a style fixedWidth of low non-zero value, and have a small (global) fieldWidth.
                    EditorGUIUtility.fieldWidth = 15f;
                    locationDataShowInEditorProp.boolValue = EditorGUILayout.Foldout(locationDataShowInEditorProp.boolValue, GUIContent.none, foldoutStyleNoLabel);
                    EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                    EditorGUI.indentLevel -= 1;

                    EditorGUILayout.PropertyField(locationDataSelectedProp, GUIContent.none, GUILayout.Width(15f));
                    GUILayout.Label((idx + 1).ToString("0000") + " " + locationDataNameProp.stringValue, GUILayout.MaxWidth(125f));

                    // Force remaining buttons to be right justified
                    GUILayoutUtility.GetRect(1f, 1f);

                    if (locationDataShowGizmosInSceneViewProp.boolValue)
                    {
                        if (GUILayout.Button(findBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
                        {
                            // Move the scene view camera to the selected point
                            SSCEditorHelper.PositionSceneView(locationDataPositionProp.vector3Value, sscManager.findZoomDistance, this.GetType());
                        }
                    }

                    // Show Gizmos button
                    if (locationDataShowGizmosInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { locationDataShowGizmosInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { locationDataShowGizmosInSceneViewProp.boolValue = true; } }

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numLocations > 1) { locationDataMoveDownPos = idx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { locationDataInsertPos = idx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { locationDataDeletePos = idx; }
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    if (locationDataShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(locationDataNameProp, locationDataNameContent);
                        EditorGUILayout.PropertyField(locationDataPositionProp, locationDataPositionContent);
                        EditorGUILayout.PropertyField(locationDataProp.FindPropertyRelative("isRadarEnabled"), locationDataIsRadarEnabledContent);
                        EditorGUILayout.PropertyField(locationDataProp.FindPropertyRelative("radarBlipSize"), locationDataRadarBlipSizedContent);
                        EditorGUILayout.PropertyField(locationDataProp.FindPropertyRelative("factionId"), locationDataFactionIdContent);
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();

                #endregion Location List

                #region Move/Remove/Insert Locations

                if (locationDataDeletePos >= 0 || locationDataInsertPos >= 0 || locationDataMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (locationDataMoveDownPos >= 0)
                    {
                        // Update lists directly rather than using MoveArrayElement to avoid issues with
                        // the Locations within each Path.
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(sscManager, "Move Location in List");
                        LocationData moveLocationData = sscManager.locationDataList[locationDataMoveDownPos];
                        if (locationDataMoveDownPos < locationDataListProp.arraySize - 1)
                        {
                            sscManager.locationDataList.RemoveAt(locationDataMoveDownPos);
                            sscManager.locationDataList.Insert(locationDataMoveDownPos + 1, moveLocationData);
                        }
                        else
                        {
                            sscManager.locationDataList.Insert(0, moveLocationData);
                            sscManager.locationDataList.RemoveAt(sscManager.locationDataList.Count - 1);

                        }
                        serializedObject.Update();                      

                        locationDataMoveDownPos = -1;
                    }
                    else if (locationDataInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        sscManager.locationDataList.Insert(locationDataInsertPos, new LocationData(sscManager.locationDataList[locationDataInsertPos]));

                        serializedObject.Update();

                        // Hide original locationData and unselect it
                        locationDataListProp.GetArrayElementAtIndex(locationDataInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        locationDataListProp.GetArrayElementAtIndex(locationDataInsertPos + 1).FindPropertyRelative("selectedInSceneView").boolValue = false;
                        locationDataShowInEditorProp = locationDataListProp.GetArrayElementAtIndex(locationDataInsertPos).FindPropertyRelative("showInEditor");

                        // Update Location with a unique hashcode
                        locationDataGUIDHashProp = locationDataListProp.GetArrayElementAtIndex(locationDataInsertPos).FindPropertyRelative("guidHash");
                        locationDataGUIDHashProp.intValue = SSCMath.GetHashCodeFromGuid();

                        locationDataNameProp = locationDataListProp.GetArrayElementAtIndex(locationDataInsertPos).FindPropertyRelative("name");
                        locationDataNameProp.stringValue += " (dup)";

                        // Force new locationData to be serialized in scene
                        locationDataShowInEditorProp.boolValue = !locationDataShowInEditorProp.boolValue;

                        // Show inserted duplicate locationData
                        locationDataShowInEditorProp.boolValue = true;

                        locationDataInsertPos = -1;
                    }
                    else if (locationDataDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and locationDataDeletePos is reset to -1.
                        int _deleteIndex = locationDataDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Location " + (_deleteIndex + 1) + "?", "Location " + (_deleteIndex + 1) + " will be deleted\n\nThis action will remove the location from the list and all the paths.", "Delete Now", "Cancel"))
                        {
                            DeleteLocationFromPaths(pathDataListProp, locationDataListProp.GetArrayElementAtIndex(_deleteIndex).FindPropertyRelative("guidHash").intValue, true);

                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(sscManager, "Delete Location");
                            sscManager.locationDataList.RemoveAt(_deleteIndex);
                            serializedObject.Update();
                        }

                        locationDataDeletePos = -1;
                    }

                    SceneView.RepaintAll();
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }

                #endregion

            }
            #endregion Locations Tab

            #region Paths Tab
            else if (selectedTabInt == 1)
            {
                if (sscManager.pathDataList == null)
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    sscManager.pathDataList = new List<PathData>(5);
                    // Read in the properties
                    serializedObject.Update();
                }

                EditorGUILayout.LabelField(pathDataHeaderContent, helpBoxRichText);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                #region Add-Remove-Import Path Buttons

                numPaths = pathDataListProp.arraySize;
                GUILayout.BeginHorizontal();
                // Indent the Foldout 7px
                GUILayout.Label("", GUILayout.Width(7f));
                // Give a 2px gap between Foldout and Label
                EditorGUIUtility.fieldWidth = 2f;
                EditorGUI.BeginChangeCheck();
                isPathDataListExpandedProp.boolValue = EditorGUILayout.Foldout(isPathDataListExpandedProp.boolValue, "", foldoutStyleNoLabel);
                if (EditorGUI.EndChangeCheck())
                {
                    ExpandList(pathDataListProp, isPathDataListExpandedProp.boolValue);
                }
                EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
                EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Paths: " + numFilteredPaths + " of " + numPaths + "</b></color>", labelFieldRichText);

                if (GUILayout.Button(pathDataImportJsonContent, GUILayout.MaxWidth(65f)))
                {
                    string importPath = string.Empty, importFileName = string.Empty;
                    if (SSCEditorHelper.GetFilePathFromUser("Import Path Data", SSCSetup.sscFolder, new string[] { "JSON", "json" }, false, ref importPath, ref importFileName))
                    {
                        PathData importedPathData = SSCManager.ImportPathDataFromJson(importPath, importFileName);
                        if (importedPathData != null)
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(sscManager, "Import Path");

                            // Check to see if Locations already exist
                            numPathLocations = importedPathData.pathLocationDataList == null ? 0 : importedPathData.pathLocationDataList.Count;
                            for (int pathLocIdx = 0; pathLocIdx < numPathLocations; pathLocIdx++)
                            {
                                PathLocationData pathLocationData = importedPathData.pathLocationDataList[pathLocIdx];
                                if (pathLocationData != null)
                                {
                                    // If the Location already exists, re-link it to the imported Path
                                    LocationData existingLocationData = sscManager.locationDataList.Find(loc => loc.guidHash == pathLocationData.locationData.guidHash);
                                    if (existingLocationData != null)
                                    {
                                        pathLocationData.locationData = existingLocationData;
                                    }
                                    else
                                    {
                                        // Create new Locations for ones that don't already exist
                                        sscManager.locationDataList.Add(pathLocationData.locationData);
                                    }
                                }
                            }

                            sscManager.pathDataList.Add(importedPathData);
                            serializedObject.Update();
                        }
                    }
                }

                if (numPaths > 0)
                {
                    if (GUILayout.Button(gizmoToggleBtnContent, GUILayout.MaxWidth(22f)))
                    {
                        ToggleGizmos(pathDataListProp);
                    }
                }

                if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                {
                    // Apply property changes
                    serializedObject.ApplyModifiedProperties();
                    GUI.FocusControl(null);
                    Undo.RecordObject(sscManager, "Add Path");
                    sscManager.pathDataList.Add(new PathData() { locationDefaultNormalOffset = sscManager.locationDefaultNormalOffset });
                    // Read in the properties
                    serializedObject.Update();

                    numPaths = pathDataListProp.arraySize;
                    if (numPaths > 0)
                    {
                        // Force new path to be serialized in scene
                        pathDataProp = pathDataListProp.GetArrayElementAtIndex(numPaths - 1);
                        pathDataShowInEditorProp = pathDataProp.FindPropertyRelative("showInEditor");
                        pathDataShowInEditorProp.boolValue = !pathDataShowInEditorProp.boolValue;
                        // Show the new path
                        pathDataShowInEditorProp.boolValue = true;
                        // Make it the active path
                        pathDataActiveGUIDHashProp.intValue = pathDataProp.FindPropertyRelative("guidHash").intValue;
                    }

                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                {
                    if (numPaths > 0)
                    {
                        // Get the last path in the list
                        pathDataProp = pathDataListProp.GetArrayElementAtIndex(pathDataListProp.arraySize - 1);
                        if (EditorUtility.DisplayDialog("Delete Path?", "Do you wish to delete Path " + (pathDataListProp.arraySize).ToString("00") + "?", "Delete Now", "Cancel"))
                        {
                            GUI.FocusControl(null);
                            pathDataListProp.arraySize -= 1;
                            serializedObject.ApplyModifiedProperties();
                            SceneView.RepaintAll();
                        }
                        GUIUtility.ExitGUI();
                    }
                }
                GUILayout.EndHorizontal();

                #endregion

                #region Path Search Filter and filtering buttons

                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(2f));
                GUILayout.Label(filterContent, labelFieldRichText, GUILayout.Width(50f));
                if (sscManager.editorSearchPathFilter == null) { sscManager.editorSearchPathFilter = string.Empty; }
                sscManager.editorSearchPathFilter = GUILayout.TextField(sscManager.editorSearchPathFilter, searchTextFieldStyle);

                // Sometimes the search text is reverse search almost seems invisible (small search icon and cancel cannot be seen)
                // Not sure how this occurs.
                if (GUILayout.Button("", searchCancelButtonStyle))
                {
                    sscManager.editorSearchPathFilter = string.Empty;
                    GUI.FocusControl(null);
                }

                //// Force buttons to be right justified
                //GUILayoutUtility.GetRect(1f, 1f);
                //// Toggle selection based on the first filtered one in the list
                //if (GUILayout.Button(selectBtnContent, buttonCompact, GUILayout.MaxWidth(20f)))
                //{
                //    ToggleSelectList(pathDataListProp, sscManager.editorSearchPathFilter);
                //}

                GUILayout.EndHorizontal();
                // Provide space between previous controls and next ones
                GUILayoutUtility.GetRect(1f, 3f);

                #endregion

                EditorGUILayout.EndVertical();

                #region Path List

                // Reset Path button variables
                pathDataMoveDownPos = -1;
                pathDataInsertPos = -1;
                pathDataDeletePos = -1;

                numPaths = pathDataListProp.arraySize;
                numFilteredPaths = 0;

                for (int pathIdx = 0; pathIdx < numPaths; pathIdx++)
                {
                    #region Get Path Properties and Filter if required
                    pathDataProp = pathDataListProp.GetArrayElementAtIndex(pathIdx);

                    pathDataNameProp = pathDataProp.FindPropertyRelative("name");

                    // Check if the list of Paths is being filtered. Ignore Locations not in the filter
                    if (!SSCEditorHelper.IsInSearchFilter(pathDataNameProp, sscManager.editorSearchPathFilter)) { continue; }
                    numFilteredPaths++;

                    pathDataGUIDHashProp = pathDataProp.FindPropertyRelative("guidHash");
                    pathDataShowInEditorProp = pathDataProp.FindPropertyRelative("showInEditor");
                    pathDataShowLocationsInEditorProp = pathDataProp.FindPropertyRelative("showLocationsInEditor");
                    pathDataShowInSceneViewProp = pathDataProp.FindPropertyRelative("showGizmosInSceneView");
                    pathDataIsClosedCircuitProp = pathDataProp.FindPropertyRelative("isClosedCircuit");
                    pathDataLineColourProp = pathDataProp.FindPropertyRelative("pathLineColour");
                    pathDataShowNumberLabelsProp = pathDataProp.FindPropertyRelative("showPointNumberLabelsInScene");
                    pathDataShowNameLabelsProp = pathDataProp.FindPropertyRelative("showPointNameLabelsInScene");
                    #endregion

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    #region Path Active/Move/Insert/Delete buttons

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(7f));
                    pathDataShowInEditorProp.boolValue = EditorGUILayout.Foldout(pathDataShowInEditorProp.boolValue, (pathIdx + 1).ToString("000") + " " + pathDataNameProp.stringValue);

                    #region Active Path
                    // Bold the "A" text on the button if this is the active layer
                    if (GUILayout.Button(pathDataActiveContent, pathDataActiveGUIDHashProp.intValue == pathDataGUIDHashProp.intValue ? buttonCompactBoldBlue : buttonCompact, GUILayout.Width(20f)))
                    {
                        pathDataActiveGUIDHashProp.intValue = pathDataGUIDHashProp.intValue;
                    }
                    #endregion

                    // Show Gizmos button
                    if (pathDataShowInSceneViewProp.boolValue) { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f))) { pathDataShowInSceneViewProp.boolValue = false; } }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { pathDataShowInSceneViewProp.boolValue = true; } }

                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numPaths > 1) { pathDataMoveDownPos = pathIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { pathDataInsertPos = pathIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { pathDataDeletePos = pathIdx; }
                    GUILayout.EndHorizontal();

                    #endregion

                    if (pathDataShowInEditorProp.boolValue)
                    {
                        #region General Path Settings
                        EditorGUILayout.PropertyField(pathDataNameProp, pathDataNameContent);

                        GUILayout.BeginHorizontal();
                        if (pathDataActiveGUIDHashProp.intValue == pathDataGUIDHashProp.intValue)
                        {
                            if (GUILayout.Button(pathDataActivatedContent, buttonCompactBoldBlue, GUILayout.Width(defaultEditorLabelWidth - 5f)))
                            {
                                pathDataActiveGUIDHashProp.intValue = -1;
                            }
                        }
                        else if (GUILayout.Button(pathDataActivateContent, buttonCompact, GUILayout.Width(defaultEditorLabelWidth - 5f)))
                        {
                            pathDataActiveGUIDHashProp.intValue = pathDataGUIDHashProp.intValue;
                            // Auto-show path in scene
                            pathDataShowInSceneViewProp.boolValue = true;
                            // In case where Path name has the focus, disable it so that when user presses +/= key in the scene it doesn't
                            // just update the path name.
                            GUI.FocusControl(null);
                        }

                        #region PathData Export To Json
                        if (GUILayout.Button(pathDataExportJsonContent, buttonCompact, GUILayout.Width(100f)))
                        {
                            string exportPath = EditorUtility.SaveFilePanel("Save Path Data", "Assets", sscManager.pathDataList[pathIdx].name, "json");

                            if (SSCManager.SavePathDataAsJson(sscManager.pathDataList[pathIdx], exportPath))
                            {
                                // Check if path is in Project folder
                                if (exportPath.Contains(Application.dataPath))
                                {
                                    // Get the folder to highlight in the Project folder
                                    string folderPath = "Assets" + System.IO.Path.GetDirectoryName(exportPath).Replace(Application.dataPath, "");

                                    // Get the json file in the Project folder
                                    exportPath = "Assets" + exportPath.Replace(Application.dataPath, "");
                                    AssetDatabase.ImportAsset(exportPath);

                                    SSCEditorHelper.HighlightFolderInProjectWindow(folderPath, true, true);
                                }
                                Debug.Log("PathData exported to " + exportPath);
                            }
                            GUIUtility.ExitGUI();
                        }
                        #endregion

                        #region Snap To Mesh
                        if (GUILayout.Button(new GUIContent(pathDataSnapToMeshContent), buttonCompact, GUILayout.Width(100f)))
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(sscManager, "Path Snap To Mesh");
                            sscManager.SnapPathToMesh(sscManager.pathDataList[pathIdx], Vector3.up, ~0, true);
                            serializedObject.Update();
                            SceneView.RepaintAll();
                        }
                        #endregion

                        GUILayout.EndHorizontal();

                        EditorGUILayout.LabelField(pathDataTotalDistanceContent, new GUIContent(pathDataProp.FindPropertyRelative("splineTotalDistance").floatValue.ToString("0.00")));
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(pathDataIsClosedCircuitProp, pathDataIsClosedCircuitContent);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();
                            // Potentially adjust tangents when circuit is closed...
                            sscManager.RefreshPathDistances(sscManager.pathDataList[pathIdx]);
                            serializedObject.Update();
                        }
                        // Don't allow Number and Name labels to be displayed at the same time
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(pathDataShowNumberLabelsProp, pathDataShowPointNumberLabelsInSceneContent);
                        if (EditorGUI.EndChangeCheck() && pathDataShowNumberLabelsProp.boolValue)
                        {
                            pathDataShowNameLabelsProp.boolValue = false;
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(pathDataShowNameLabelsProp, pathDataShowPointNameLabelsInSceneContent);
                        if (EditorGUI.EndChangeCheck() && pathDataShowNameLabelsProp.boolValue)
                        {
                            pathDataShowNumberLabelsProp.boolValue = false;
                        }
                        EditorGUILayout.PropertyField(pathDataProp.FindPropertyRelative("showDistancesInScene"), pathDataShowDistanceLabelsInSceneContent);

                        // We store the colour as a vector4 so it can be more portable so use a ColorField rather than a PropertyField.
                        pathDataLineColourProp.vector4Value = EditorGUILayout.ColorField(pathDataLineColourContent, pathDataLineColourProp.vector4Value);
                        EditorGUILayout.PropertyField(pathDataProp.FindPropertyRelative("locationDefaultNormalOffset"), pathDataLocationDefaultNormalOffsetContent);

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(pathDataLocationSnapMinMaxHeightContent, GUILayout.Width(defaultEditorLabelWidth));
                        EditorGUILayout.PropertyField(pathDataProp.FindPropertyRelative("snapMinHeight"), GUIContent.none);
                        EditorGUILayout.PropertyField(pathDataProp.FindPropertyRelative("snapMaxHeight"), GUIContent.none);
                        GUILayout.EndHorizontal();

                        #endregion

                        pathLocationDataListProp = pathDataProp.FindPropertyRelative("pathLocationDataList");

                        if (pathLocationDataListProp != null)
                        {
                            #region Add-Remove Path Location slot, Modify Attribute, Reverse Buttons

                            // Reset path location slot variables
                            pathDataLocationMoveDownPos = -1;
                            pathDataLocationInsertPos = -1;
                            pathDataLocationDeletePos = -1;

                            numPathLocations = pathLocationDataListProp.arraySize;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(7f));
                            pathDataShowLocationsInEditorProp.boolValue = EditorGUILayout.Foldout(pathDataShowLocationsInEditorProp.boolValue, "Path Locations: " + numPathLocations.ToString("000"));

                            // Toggle modify selected path location attributes
                            if (GUILayout.Button(modifyToggleBtnContent, GUILayout.MaxWidth(22f)))
                            {
                                GUI.FocusControl(null);
                                isModifySelectedMode = !isModifySelectedMode;

                                if (isModifySelectedMode)
                                {
                                    modifySelectedPosY = GetPosYSelectList(pathLocationDataListProp);
                                }
                            }

                            if (GUILayout.Button(reversePathBtnContent, GUILayout.MaxWidth(40f)))
                            {
                                GUI.FocusControl(null);
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(sscManager, "Reverse Path");
                                sscManager.ReversePath(sscManager.pathDataList[pathIdx]);
                                SceneView.RepaintAll();
                                GUIUtility.ExitGUI();
                            }

                            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
                            {
                                // Apply property changes
                                serializedObject.ApplyModifiedProperties();
                                Undo.RecordObject(sscManager, "Add Path Location slot");
                                sscManager.pathDataList[pathIdx].pathLocationDataList.Add(new PathLocationData());

                                // Read in the properties
                                serializedObject.Update();

                                numPathLocations = pathLocationDataListProp.arraySize;
                                if (numPathLocations > 0)
                                {
                                    // Force new path location slot to be serialized in scene
                                    pathLocationDataProp = pathLocationDataListProp.GetArrayElementAtIndex(numPathLocations - 1);
                                    pathDataShowInEditorProp = pathLocationDataProp.FindPropertyRelative("showInEditor");
                                    pathDataShowInEditorProp.boolValue = !pathDataShowInEditorProp.boolValue;
                                    // Show the new path location slot
                                    pathDataShowInEditorProp.boolValue = true;
                                    pathDataShowLocationsInEditorProp.boolValue = true;
                                }

                                SceneView.RepaintAll();
                            }
                            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
                            {
                                if (numPathLocations > 0)
                                {
                                    pathDataLocationDeletePos = pathLocationDataListProp.arraySize - 1;
                                }
                            }
                            GUILayout.EndHorizontal();

                            #endregion

                            #region Modify Selected
                            if (isModifySelectedMode)
                            {
                                GUILayout.BeginHorizontal();
                                modifySelectedPosY = EditorGUILayout.FloatField(modifySelectedPosYContent, modifySelectedPosY);

                                if (GUILayout.Button("Change", GUILayout.Width(65f)))
                                {
                                    SetPosYSelectList(sscManager.pathDataList[pathIdx], modifySelectedPosY);

                                    isModifySelectedMode = false;
                                    GUIUtility.ExitGUI();
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                modifySelectedAddPosY = EditorGUILayout.FloatField(modifySelectedAddPosYContent, modifySelectedAddPosY);

                                if (GUILayout.Button("Change", GUILayout.Width(65f)))
                                {
                                    AddPosYSelectList(sscManager.pathDataList[pathIdx], modifySelectedAddPosY);

                                    isModifySelectedMode = false;
                                    GUIUtility.ExitGUI();
                                }
                                GUILayout.EndHorizontal();
                            }

                            #endregion

                            #region Path Location List
                            numPathLocations = pathLocationDataListProp.arraySize;

                            if (pathDataShowLocationsInEditorProp.boolValue)
                            {
                                for (int pathLocIdx = 0; pathLocIdx < numPathLocations; pathLocIdx++)
                                {
                                    #region Get Path Location Data properties (fields)
                                    // Get the PathLocationData class instance from the list
                                    pathLocationDataProp = pathLocationDataListProp.GetArrayElementAtIndex(pathLocIdx);
                                    // Get the LocationData class reference from within the PathLocationData class instance
                                    pathLocationDataLocationProp = pathLocationDataProp.FindPropertyRelative("locationData");
                                    // Get the properties (fields) of the LocationData within the PathLocationData class instance
                                    pathDataLocationNameProp = pathLocationDataLocationProp.FindPropertyRelative("name");
                                    pathDataLocationPositionProp = pathLocationDataLocationProp.FindPropertyRelative("position");
                                    pathDataLocationSelectedProp = pathLocationDataLocationProp.FindPropertyRelative("selectedInSceneView");
                                    pathDataLocationGUIDHashProp = pathLocationDataLocationProp.FindPropertyRelative("guidHash");
                                    pathDataLocationUnassignedProp = pathLocationDataLocationProp.FindPropertyRelative("isUnassigned");
                                    pathDataLocationShowGizmosInSceneViewProp = pathLocationDataLocationProp.FindPropertyRelative("showGizmosInSceneView");
                                    #endregion

                                    GUILayout.BeginHorizontal();

                                    // Find in scene if it is assigned to Location from the SSSManager.locationDataList (not just a member of the Path)
                                    if (GUILayout.Button(findBtnContent, buttonCompact, GUILayout.MaxWidth(20f)) && !pathDataLocationUnassignedProp.boolValue)
                                    {
                                        // Move the scene view camera to the selected point
                                        SSCEditorHelper.PositionSceneView(pathDataLocationPositionProp.vector3Value, sscManager.findZoomDistance, this.GetType());
                                        // Show it in the scene
                                        pathDataLocationShowGizmosInSceneViewProp.boolValue = true;
                                        // Select it in the scene (doesn't unselect other locations)
                                        pathDataLocationSelectedProp.boolValue = true;
                                    }

                                    // When using VerifyPathConnections() for some reason cannot update the Properties.
                                    EditorGUI.BeginChangeCheck();
                                    pathLocationDataIsSelected = EditorGUILayout.Toggle(GUIContent.none, pathDataLocationSelectedProp.boolValue, GUILayout.Width(15f));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        serializedObject.ApplyModifiedProperties();
                                        Undo.RecordObject(sscManager, "Path Location Selection");
                                        sscManager.pathDataList[pathIdx].pathLocationDataList[pathLocIdx].locationData.selectedInSceneView = pathLocationDataIsSelected;
                                        serializedObject.Update();
                                        SceneView.RepaintAll();
                                    }

                                    if (GUILayout.Button("..", buttonCompact, GUILayout.MaxWidth(20f)))
                                    {
                                        // Apply property changes
                                        serializedObject.ApplyModifiedProperties();

                                        int selectedIdx = sscManager.locationDataList.FindIndex(loc => loc.guidHash == pathDataLocationGUIDHashProp.intValue);

                                        // TODO - filter the drop down list by default (could be 100s or 1000s of locations in the scene).
                                        // Create a drop down list of all the locations
                                        GenericMenu dropdown = new GenericMenu();
                                        for (int i = 0; i < sscManager.locationDataList.Count; i++)
                                        {
                                            // Replace space #/%/& with different chars as Unity treats them as SHIFT/CTRL/ALT in menus.
                                            string _locationName = string.IsNullOrEmpty(sscManager.locationDataList[i].name) ? "No Name" : sscManager.locationDataList[i].name.Replace(" #", "_#").Replace(" &", " &&").Replace(" %", "_%");
                                            dropdown.AddItem(new GUIContent(_locationName), i == selectedIdx, UpdatePathLocation, new Vector3Int(pathDataGUIDHashProp.intValue, pathDataLocationGUIDHashProp.intValue, sscManager.locationDataList[i].guidHash));
                                        }
                                        dropdown.ShowAsContext();
                                        SceneView.RepaintAll();

                                        serializedObject.Update();
                                    }
                                    EditorGUILayout.LabelField(string.IsNullOrEmpty(pathDataLocationNameProp.stringValue) ? "unknown" : pathDataLocationNameProp.stringValue);

                                    // Move down button
                                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numPathLocations > 1) { pathDataLocationMoveDownPos = pathLocIdx; }
                                    // Create duplicate button
                                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { pathDataLocationInsertPos = pathLocIdx; }

                                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { pathDataLocationDeletePos = pathLocIdx; }

                                    GUILayout.EndHorizontal();
                                }
                            }
                            #endregion Path Location List

                            #region Move/Remove/Insert Path Location

                            if (pathDataLocationDeletePos >= 0 || pathDataLocationInsertPos >= 0 || pathDataLocationMoveDownPos >= 0)
                            {
                                GUI.FocusControl(null);

                                // Don't permit multiple operations in the same pass
                                if (pathDataLocationMoveDownPos >= 0)
                                {
                                    serializedObject.ApplyModifiedProperties();
                                    Undo.RecordObject(sscManager, "Move Location in Path List");
                                    PathLocationData movePathLocationData = sscManager.pathDataList[pathIdx].pathLocationDataList[pathDataLocationMoveDownPos];
                                    if (pathDataLocationMoveDownPos < pathLocationDataListProp.arraySize - 1)
                                    {
                                        sscManager.pathDataList[pathIdx].pathLocationDataList.RemoveAt(pathDataLocationMoveDownPos);
                                        sscManager.pathDataList[pathIdx].pathLocationDataList.Insert(pathDataLocationMoveDownPos + 1, movePathLocationData);
                                    }
                                    else
                                    {
                                        sscManager.pathDataList[pathIdx].pathLocationDataList.Insert(0, movePathLocationData);
                                        sscManager.pathDataList[pathIdx].pathLocationDataList.RemoveAt(sscManager.pathDataList[pathIdx].pathLocationDataList.Count - 1);
                                    }
                                    sscManager.RefreshPathDistances(sscManager.pathDataList[pathIdx]);
                                    serializedObject.Update();
                                    pathDataLocationMoveDownPos = -1;
                                }
                                else if (pathDataLocationInsertPos >= 0)
                                {
                                    // Apply property changes before potential list changes
                                    serializedObject.ApplyModifiedProperties();

                                    // Insert a new default location. The user will replace this with one from the list of Locations
                                    // For locations in a path, there is no advantage of duplicated an existing location slot
                                    Undo.RecordObject(sscManager, "Insert Path Location slot");
                                    sscManager.pathDataList[pathIdx].pathLocationDataList.Insert(pathDataLocationInsertPos, new PathLocationData());

                                    serializedObject.Update();

                                    // Hide original pathDataLocation
                                    pathLocationDataListProp.GetArrayElementAtIndex(pathDataLocationInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                                                                   
                                    SerializedProperty pathLocationDataInsertedProp = pathLocationDataListProp.GetArrayElementAtIndex(pathDataLocationInsertPos);
                                    pathDataLocationShowInEditorProp = pathLocationDataInsertedProp.FindPropertyRelative("showInEditor");
                                    pathLocationDataInsertedProp.FindPropertyRelative("locationData").FindPropertyRelative("isUnassigned").boolValue = true;

                                    // Force new pathDataLocation to be serialized in scene
                                    pathDataLocationShowInEditorProp.boolValue = !pathDataLocationShowInEditorProp.boolValue;

                                    // Show inserted duplicate pathDataLocation
                                    pathDataLocationShowInEditorProp.boolValue = true;

                                    pathDataLocationInsertPos = -1;
                                }
                                else if (pathDataLocationDeletePos >= 0)
                                {
                                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and pathDataLocationDeletePos is reset to -1.
                                    int _deleteIndex = pathDataLocationDeletePos;
                                    int _pathIndex = pathIdx;

                                    // Get the last Path Location slot in the list
                                    pathLocationDataProp = pathLocationDataListProp.GetArrayElementAtIndex(_deleteIndex);
                                    if (SSCEditorHelper.PromptForDelete("Delete Path Location slot?", "Do you wish to delete Path Location slot " + (_deleteIndex+1).ToString("00") + "?"))
                                    {
                                        pathDataShowLocationsInEditorProp.boolValue = true;

                                        // Change the list rather than use DeleteArrayElementAtIndex which has issues with undo.
                                        serializedObject.ApplyModifiedProperties();
                                        Undo.RecordObject(sscManager, "Delete Path Location Slot");
                                        sscManager.pathDataList[_pathIndex].pathLocationDataList.RemoveAt(_deleteIndex);
                                        sscManager.RefreshPathDistances(sscManager.pathDataList[_pathIndex]);
                                        pathDataLocationDeletePos = -1;
                                        
                                        SceneView.RepaintAll();
                                        GUIUtility.ExitGUI();

                                        //serializedObject.Update();
                                    }
                                }

                                SceneView.RepaintAll();
                            }

                            #endregion Move/Remove/Insert Path Location
                        }
                    }

                    // There is a bug here in 2019.4 and 2020.1 where the DisplayDialog causes formatting issues.
                    // EndLayoutGroup: BeginLayoutGroup must be called first.
                    // No 2019_4 define but earlier versions still seem to work. 
                    #if UNITY_2019_3_OR_NEWER
                    //if (pathDataLocationDeletePos >= 0 ) { GUILayout.BeginVertical(); }
                    if (pathDataLocationDeletePos >= 0 ) { GUIUtility.ExitGUI(); }
                    #endif

                    GUILayout.EndVertical();
                }

                #endregion Path List

                #region Move/Remove/Insert Paths

                if (pathDataDeletePos >= 0 || pathDataInsertPos >= 0 || pathDataMoveDownPos >= 0)
                {
                    GUI.FocusControl(null);

                    // Don't permit multiple operations in the same pass
                    if (pathDataMoveDownPos >= 0)
                    {
                        // Move down one position, or wrap round to start of list
                        if (pathDataMoveDownPos < pathDataListProp.arraySize - 1)
                        {
                            pathDataListProp.MoveArrayElement(pathDataMoveDownPos, pathDataMoveDownPos + 1);
                        }
                        else { pathDataListProp.MoveArrayElement(pathDataMoveDownPos, 0); }

                        pathDataMoveDownPos = -1;
                    }
                    else if (pathDataInsertPos >= 0)
                    {
                        // Apply property changes before potential list changes
                        serializedObject.ApplyModifiedProperties();

                        sscManager.pathDataList.Insert(pathDataInsertPos, new PathData(sscManager.pathDataList[pathDataInsertPos]));

                        serializedObject.Update();

                        // Hide original pathData
                        pathDataListProp.GetArrayElementAtIndex(pathDataInsertPos + 1).FindPropertyRelative("showInEditor").boolValue = false;
                        pathDataShowInEditorProp = pathDataListProp.GetArrayElementAtIndex(pathDataInsertPos).FindPropertyRelative("showInEditor");

                        // Update Path with a unique hashcode
                        pathDataGUIDHashProp = pathDataListProp.GetArrayElementAtIndex(pathDataInsertPos).FindPropertyRelative("guidHash");
                        pathDataGUIDHashProp.intValue = SSCMath.GetHashCodeFromGuid();

                        // Force new pathData to be serialized in scene
                        pathDataShowInEditorProp.boolValue = !pathDataShowInEditorProp.boolValue;

                        // Show inserted duplicate pathData
                        pathDataShowInEditorProp.boolValue = true;

                        pathDataInsertPos = -1;
                    }
                    else if (pathDataDeletePos >= 0)
                    {
                        // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and pathDataDeletePos is reset to -1.
                        int _deleteIndex = pathDataDeletePos;

                        if (EditorUtility.DisplayDialog("Delete Path " + (_deleteIndex + 1).ToString("00") + "?", "Path " + (_deleteIndex + 1).ToString("00") + " will be deleted\n\nThis action will remove the path from the list and cannot be undone.", "Delete Now", "Cancel"))
                        {
                            pathDataListProp.DeleteArrayElementAtIndex(_deleteIndex);
                            pathDataDeletePos = -1;
                        }
                    }

                    SceneView.RepaintAll();
                }

                #endregion

            }
            #endregion Paths Tab

            #region Options Tab
            else if (selectedTabInt == 2)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                // Location Options
                GUILayout.Label(optionLocationContent, labelFieldRichText);
                EditorGUILayout.PropertyField(isAutosizeLocationGizmoProp, optionIsAutosizeLocationGizmoContent);
                EditorGUILayout.PropertyField(locationGizmoColourProp, optionLocationGizmoColourContent);
                EditorGUILayout.PropertyField(findZoomDistanceProp, optionFindZoomDistanceContent);

                // Path Options
                GUILayout.Label(optionPathContent, labelFieldRichText);
                EditorGUILayout.PropertyField(defaultPathControlGizmoColourProp, optionPathControlGizmoColourContent);
                EditorGUILayout.PropertyField(pathDisplayResolutionProp, optionPathDisplayResolutionContent);
                EditorGUILayout.PropertyField(pathPrecisionProp, optionPathPrecisionContent);
                EditorGUILayout.PropertyField(defaultPathControlOffsetProp, optionPathControlOffsetContent);

                //// Radar Options
                //GUILayout.Label(optionRadarContent, labelFieldRichText);

                GUILayout.EndVertical();
            }
            #endregion

            // Apply property changes
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
