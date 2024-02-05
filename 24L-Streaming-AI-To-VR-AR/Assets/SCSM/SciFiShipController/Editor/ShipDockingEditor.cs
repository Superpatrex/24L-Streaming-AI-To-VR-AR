using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(ShipDocking))]
    public class ShipDockingEditor : Editor
    {
        #region Enumerations

        #endregion

        #region Custom Editor private variables
        private ShipDocking shipDocking;

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
        private bool isDebuggingEnabled = false;
        private Color separatorColor = new Color();

        // Similar to isSceneDirtyRequired (SceneView variabled) but used for Inspector modifications.
        private bool isSceneModified = false;

        #endregion

        #region SceneView Variables

        private bool isSceneDirtyRequired = false;
        private Quaternion sceneViewTrfmRotation = Quaternion.identity;
        private ShipDockingAdapter adapterComponent = null;

        private Vector3 componentHandlePosition = Vector3.zero;
        //private Vector3 gizmoPosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;

        // olive skin colour
        private Color adapterGizmoColour = new Color(198f / 255f, 134f / 255f, 66f / 255f, 1f);
        private Color fadedGizmoColour;
        #endregion

        #region GUIContent General

        private readonly static GUIContent[] tabTexts = { new GUIContent("General"), new GUIContent("Events") };
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to dock with a Ship Docking Station at a Ship Docking Point.");
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent("Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. " +
          "This should be disabled if you are instantiating the Ship or ShipDocking through code and using the Docking API methods.");
        private readonly static GUIContent initialDockingStateContent = new GUIContent("Initial Docking State", "Ships can start in a state of Docked or Undocked");
        private readonly static GUIContent landingDistancePrecisionContent = new GUIContent("Landing Distance Precision", "How close the ship has to be (in metres) to the docking position before it can become docked.");
        private readonly static GUIContent landingAnglePrecisionContent = new GUIContent("Landing Angle Precision", "How close the ship has to be (in degrees) to the docking rotation before it can become docked.");
        private readonly static GUIContent hoverDistancePrecisionContent = new GUIContent("Hover Distance Precision", "How close the ship has to be (in metres) to the hovering position before it is deemed to have reached the hover position.");
        private readonly static GUIContent hoverAnglePrecisionContent = new GUIContent("Hover Angle Precision", "How close the ship has to be (in degrees) to the hovering rotation before it is deemed to have reached the hover position.");
        private readonly static GUIContent liftOffDurationContent = new GUIContent("Lift-off Duration", "Target time to lift off from the landing position and move to the hover position. Has no effect if the docking point hover height is 0.");
        private readonly static GUIContent landingDurationContent = new GUIContent("Landing Duration", "Target time to move from the hover position to the landing position. Has no effect if the docking point hover height is 0.");
        private readonly static GUIContent detectCollisionsWhenDockedContent = new GUIContent("Detect Collisions (Docked)", "Should physics collisions been detected when the state is Docked?");
        private readonly static GUIContent dockSnapToPosContent = new GUIContent("Dock Snap to Pos Axes", "The position axes to snap to when a ship gets close to the docking point, and becomes Docked. The snap amount can be affected by the Landing Distance Precision.");
        private readonly static GUIContent dockSnapToRotContent = new GUIContent("Dock Snap to Rot Axes", "The rotational axes to snap to when a ship gets close to the docking point, and becomes Docked. The snap amount can be affected by the Landing Angle Precision.");
        private readonly static GUIContent undockingDelayContent = new GUIContent("Undocking Delay", "When used with ShipDockingStation UndockShip(..), the number of seconds that the undocking manoeuvre is delayed. This allows you to create cinematic effects or perform other actions, before the Undocking process begins.");
        private readonly static GUIContent autoUndockTimeContent = new GUIContent("Auto Undock Time", "When the value is greater than 0, the number of seconds the ship waits while docked, before automatically attempting to start the undocking procedure.");
        private readonly static GUIContent mothershipUndockingContent = new GUIContent("<b>Mothership Undocking</b>");
        private readonly static GUIContent undockVertVelocityContent = new GUIContent(" Undock Vert Velocity", "This is additional velocity in an upwards direction relative to the mothership");
        private readonly static GUIContent undockFwdVelocityContent = new GUIContent(" Undock Fwd Velocity", "This is additional velocity in a forward direction relative to the mothership");
        private readonly static GUIContent catapultUndockingContent = new GUIContent("<b>Catapult Undocking</b>");
        private readonly static GUIContent catapultThrustContent = new GUIContent(" Catapult Thrust (kN)", "The amount of force applied by the catapult when undocking in KiloNewtons");
        private readonly static GUIContent catapultDurationContent = new GUIContent(" Catapult Duration", "The number of seconds that the force is applied from the catapult to the ship");

        //private readonly static GUIContent gizmoToggleBtnContent = new GUIContent("G", "Toggle gizmos and visualisations on/off for all items in the scene view");
        private readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
        private readonly static GUIContent gizmoFindBtnContent = new GUIContent("F", "Find (select) in the scene view.");

        #endregion

        #region GUIContent Events

        private readonly static GUIContent onPostDockedDelayContent = new GUIContent("  On Docked Delay", "The time, in seconds, to delay the actioning of any On Post Docked methods.");
        private readonly static GUIContent onPostDockedContent = new GUIContent("On Post Docked");
        private readonly static GUIContent onPostDockingHoverDelayContent = new GUIContent("  On Dock Hover Delay", "The time, in seconds, to delay the actioning of any On Post Docking Hover methods.");
        private readonly static GUIContent onPostDockingHoverContent = new GUIContent("On Post Docking Hover");
        private readonly static GUIContent onPostDockingStartDelayContent = new GUIContent("  On Dock Start Delay", "The time, in seconds, to delay the actioning of any On Post Docking Start methods.");
        private readonly static GUIContent onPostDockingStartContent = new GUIContent("On Post Docking Start");
        private readonly static GUIContent onPostUndockedDelayContent = new GUIContent("  On Undocked Delay", "The time, in seconds, to delay the actioning of any On Post Undocked methods.");
        private readonly static GUIContent onPostUndockedContent = new GUIContent("On Post Undocked");
        private readonly static GUIContent onPostUndockingHoverDelayContent = new GUIContent("  On Undock Hover Delay", "The time, in seconds, to delay the actioning of any On Post Undocking Hover methods.");
        private readonly static GUIContent onPostUndockingHoverContent = new GUIContent("On Post Undocking Hover");
        private readonly static GUIContent onPostUndockingStartDelayContent = new GUIContent("  On Undock Start Delay", "The time, in seconds, to delay the actioning of any On Post Undocking Start methods.");
        private readonly static GUIContent onPostUndockingStartContent = new GUIContent("On Post Undocking Start");

        #endregion

        #region GUIContent Ship Docking Adapter
        private readonly static GUIContent dkgAdRelativePositionContent = new GUIContent("Relative Position", "Local position relative to the Ship tranform position");
        private readonly static GUIContent dkgAdRelativeDirectionContent = new GUIContent("Relative Direction", "The direction the adapter is facing relative to the Ship. Default is down (y = -1). A +ve Z value is forwards, and -ve Z value is backwards.");
        #endregion

        #region GUIConnent Debugging
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display the data being set in the Ship Docking component at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugCurrentStateContent = new GUIContent("Current State");
        private readonly static GUIContent debugIsHoverTargetContent = new GUIContent("Is Hover Target?");
        //private readonly static GUIContent debugNotSetContent = new GUIContent("-", "not set");
        #endregion

        #region Serialize Properties
        private SerializedProperty selectedTabIntProp;
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty initialDockingStateProp;
        private SerializedProperty landingDistancePrecisionProp;
        private SerializedProperty landingAnglePrecisionProp;
        private SerializedProperty hoverDistancePrecisionProp;
        private SerializedProperty hoverAnglePrecisionProp;
        private SerializedProperty liftOffDurationProp;
        private SerializedProperty landingDurationProp;
        private SerializedProperty detectCollisionsWhenDockedProp;
        private SerializedProperty dockSnapToPosProp;
        private SerializedProperty dockSnapToRotProp;
        private SerializedProperty undockingDelayProp;
        private SerializedProperty autoUndockTimeProp;
        private SerializedProperty undockVertVelocityProp;
        private SerializedProperty undockFwdVelocityProp;
        private SerializedProperty catapultThrustProp;
        private SerializedProperty catapultDurationProp;
        private SerializedProperty dkgAdapterListProp;
        private SerializedProperty dkgAdProp;
        private SerializedProperty dkgAdRelativeDirectionProp;
        private SerializedProperty dkgAdShowInEditorProp;
        private SerializedProperty dkgAdShowGizmosInSceneViewProp;
        private SerializedProperty dkgAdSelectedInSceneViewProp;
        private SerializedProperty isDockingAdapterListExpandedProp;

        // Event properties
        private SerializedProperty onPostDockedDelayProp;
        private SerializedProperty onPostDockedProp;
        private SerializedProperty onPostDockingHoverDelayProp;
        private SerializedProperty onPostDockingHoverProp;
        private SerializedProperty onPostDockingStartDelayProp;
        private SerializedProperty onPostDockingStartProp;
        private SerializedProperty onPostUndockedDelayProp;
        private SerializedProperty onPostUndockedProp;
        private SerializedProperty onPostUndockingHoverDelayProp;
        private SerializedProperty onPostUndockingHoverProp;
        private SerializedProperty onPostUndockingStartDelayProp;
        private SerializedProperty onPostUndockingStartProp;

        #endregion

        #region Events

        public void OnEnable()
        {
            shipDocking = (ShipDocking)target;

            if (shipDocking.adapterList == null) { shipDocking.adapterList = new List<ShipDockingAdapter>(1); }

            // Only use if require scene view interaction
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }
            if (string.IsNullOrEmpty(txtColourName)) { }

            defaultEditorLabelWidth = 185f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region Find Properties - General
            selectedTabIntProp = serializedObject.FindProperty("selectedTabInt");
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            initialDockingStateProp = serializedObject.FindProperty("initialDockingState");
            landingDistancePrecisionProp = serializedObject.FindProperty("landingDistancePrecision");
            landingAnglePrecisionProp = serializedObject.FindProperty("landingAnglePrecision");
            hoverDistancePrecisionProp = serializedObject.FindProperty("hoverDistancePrecision");
            hoverAnglePrecisionProp = serializedObject.FindProperty("hoverAnglePrecision");
            liftOffDurationProp = serializedObject.FindProperty("liftOffDuration");
            landingDurationProp = serializedObject.FindProperty("landingDuration");
            detectCollisionsWhenDockedProp = serializedObject.FindProperty("detectCollisionsWhenDocked");
            dockSnapToPosProp = serializedObject.FindProperty("dockSnapToPos");
            dockSnapToRotProp = serializedObject.FindProperty("dockSnapToRot");
            undockingDelayProp = serializedObject.FindProperty("undockingDelay");
            autoUndockTimeProp = serializedObject.FindProperty("autoUndockTime");
            undockVertVelocityProp = serializedObject.FindProperty("undockVertVelocity");
            undockFwdVelocityProp = serializedObject.FindProperty("undockFwdVelocity");
            catapultThrustProp = serializedObject.FindProperty("catapultThrust");
            catapultDurationProp = serializedObject.FindProperty("catapultDuration");
            #endregion

            #region Find Properties - Events
            onPostDockedDelayProp = serializedObject.FindProperty("onPostDockedDelay");
            onPostDockedProp = serializedObject.FindProperty("onPostDocked");
            onPostDockingHoverDelayProp = serializedObject.FindProperty("onPostDockingHoverDelay");
            onPostDockingHoverProp = serializedObject.FindProperty("onPostDockingHover");
            onPostDockingStartDelayProp = serializedObject.FindProperty("onPostDockingStartDelay");
            onPostDockingStartProp = serializedObject.FindProperty("onPostDockingStart"); 
            onPostUndockedDelayProp = serializedObject.FindProperty("onPostUndockedDelay");
            onPostUndockedProp = serializedObject.FindProperty("onPostUndocked");
            onPostUndockingHoverDelayProp = serializedObject.FindProperty("onPostUndockingHoverDelay");
            onPostUndockingHoverProp = serializedObject.FindProperty("onPostUndockingHover");
            onPostUndockingStartDelayProp = serializedObject.FindProperty("onPostUndockingStartDelay");
            onPostUndockingStartProp = serializedObject.FindProperty("onPostUndockingStart");
            #endregion

            // Reset GUIStyles
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;
            foldoutStyleNoLabel = null;
            separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.grey;
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
            if (shipDocking.allowRepaint) { Repaint(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
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
            if (shipDocking != null && shipDocking.gameObject.activeInHierarchy)
            {
                isSceneDirtyRequired = false;
                int numDockingAdapters = shipDocking.adapterList == null ? 0 : shipDocking.adapterList.Count;

                // Get the rotation of the ship in the scene
                sceneViewTrfmRotation = Quaternion.LookRotation(shipDocking.transform.forward, shipDocking.transform.up);

                // Draw all the adapters in the scene view
                using (new Handles.DrawingScope(adapterGizmoColour))
                {
                    for (int daIdx = 0; daIdx < numDockingAdapters; daIdx++)
                    {
                        adapterComponent = shipDocking.adapterList[daIdx];

                        // Prevent adapter direction ever being a zero vector
                        if (adapterComponent.relativeDirection == Vector3.zero) { adapterComponent.relativeDirection = Vector3.down; }

                        if (adapterComponent.showGizmosInSceneView)
                        {
                            componentHandlePosition = shipDocking.transform.TransformPoint(adapterComponent.relativePosition);

                            // Get component handle rotation
                            componentHandleRotation = Quaternion.LookRotation(shipDocking.transform.TransformDirection(adapterComponent.relativeDirection), shipDocking.transform.up);

                            fadedGizmoColour = adapterGizmoColour;

                            // If this is not the selected Docking Point, show it a more transparent
                            if (!adapterComponent.selectedInSceneView)
                            {
                                fadedGizmoColour.a *= 0.65f;
                                if (fadedGizmoColour.a < 0.1f) { fadedGizmoColour.a = adapterGizmoColour.a; }
                            }

                            // Draw an adapter direction arrow in the scene that is non-interactable
                            if (Event.current.type == EventType.Repaint)
                            {
                                using (new Handles.DrawingScope(fadedGizmoColour))
                                {
                                    Handles.ArrowHandleCap(0, componentHandlePosition, componentHandleRotation, 1f, EventType.Repaint);
                                }
                            }

                            if (adapterComponent.selectedInSceneView)
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
                                        Undo.RecordObject(shipDocking, "Rotate Adapter Direction");

                                        adapterComponent.relativeDirection = shipDocking.transform.InverseTransformDirection(componentHandleRotation * Vector3.forward);
                                    }
                                }

                                else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                                {
                                    EditorGUI.BeginChangeCheck();

                                    // Draw a movement handle
                                    componentHandlePosition = Handles.PositionHandle(componentHandlePosition, sceneViewTrfmRotation);

                                    // Use the position handle to edit the position of the weapon
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        isSceneDirtyRequired = true;
                                        Undo.RecordObject(shipDocking, "Move Adapter");
                                        adapterComponent.relativePosition = shipDocking.transform.InverseTransformPoint(componentHandlePosition);
                                    }
                                }
                            }

                            using (new Handles.DrawingScope(fadedGizmoColour))
                            {
                                // Allow the user to select/deselect the adapter location in the scene view
                                if (Handles.Button(componentHandlePosition, Quaternion.identity, 0.5f, 0.25f, Handles.SphereHandleCap))
                                {
                                    if (adapterComponent.selectedInSceneView)
                                    {
                                        DeselectAllComponents();
                                        adapterComponent.showInEditor = false;
                                    }
                                    else
                                    {
                                        DeselectAllComponents();
                                        shipDocking.isAdapterListExpanded = false;
                                        ExpandList(shipDocking.adapterList, false);
                                        adapterComponent.selectedInSceneView = true;
                                        adapterComponent.showInEditor = true;
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
                        (componentList[cpi] as ShipDockingAdapter).showInEditor = isExpanded;
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
            if (shipDocking != null)
            {
                int numDockingAdapters = shipDocking.adapterList == null ? 0 : shipDocking.adapterList.Count;
                for (int daIdx = 0; daIdx < numDockingAdapters; daIdx++)
                {
                    shipDocking.adapterList[daIdx].selectedInSceneView = false;
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

                if (compType == typeof(ShipDockingAdapter))
                {
                    // Examine the first component
                    bool showGizmos = !(componentList[0] as ShipDockingAdapter).showGizmosInSceneView;

                    // Toggle gizmos and visualisations to opposite of first member
                    for (int cpi = 0; cpi < numComponents; cpi++)
                    {
                        (componentList[cpi] as ShipDockingAdapter).showGizmosInSceneView = showGizmos;
                    }

                    // When no Gizmos are shown , ensure we can see the standard Unity tools.
                    // This allows the user to move the Ship gameobject
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

        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there

        public override void OnInspectorGUI()
        {
            #region Initialise

            shipDocking.allowRepaint = false;
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

            #region General

            if (selectedTabIntProp.intValue == 0)
            {
                #region General Settings
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
                EditorGUILayout.PropertyField(initialDockingStateProp, initialDockingStateContent);
                EditorGUILayout.PropertyField(landingDistancePrecisionProp, landingDistancePrecisionContent);
                EditorGUILayout.PropertyField(landingAnglePrecisionProp, landingAnglePrecisionContent);
                EditorGUILayout.PropertyField(hoverDistancePrecisionProp, hoverDistancePrecisionContent);
                EditorGUILayout.PropertyField(hoverAnglePrecisionProp, hoverAnglePrecisionContent);
                EditorGUILayout.PropertyField(liftOffDurationProp, liftOffDurationContent);
                EditorGUILayout.PropertyField(landingDurationProp, landingDurationContent);
                EditorGUILayout.PropertyField(detectCollisionsWhenDockedProp, detectCollisionsWhenDockedContent);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(dockSnapToPosProp, dockSnapToPosContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    shipDocking.SetDockSnapToPosition((ShipDocking.DockSnapTo)dockSnapToPosProp.intValue);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(dockSnapToRotProp, dockSnapToRotContent);
                if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                {
                    shipDocking.SetDockSnapToRotation((ShipDocking.DockSnapTo)dockSnapToRotProp.intValue);
                }

                EditorGUILayout.PropertyField(undockingDelayProp, undockingDelayContent);
                EditorGUILayout.PropertyField(autoUndockTimeProp, autoUndockTimeContent);

                GUILayoutUtility.GetRect(1f, 3f);
                EditorGUILayout.LabelField(mothershipUndockingContent, labelFieldRichText);
                EditorGUILayout.PropertyField(undockVertVelocityProp, undockVertVelocityContent);
                EditorGUILayout.PropertyField(undockFwdVelocityProp, undockFwdVelocityContent);

                GUILayoutUtility.GetRect(1f, 3f);
                EditorGUILayout.LabelField(catapultUndockingContent, labelFieldRichText);
                // Display in kiloNewtons. Store in Newtons.
                catapultThrustProp.floatValue = EditorGUILayout.Slider(catapultThrustContent, catapultThrustProp.floatValue / 1000f, 0f, 50000f) * 1000f;

                //EditorGUILayout.PropertyField(catapultThrustProp, catapultThrustContent);
                EditorGUILayout.PropertyField(catapultDurationProp, catapultDurationContent);
                EditorGUILayout.EndVertical();
                #endregion

                #region Docking Adapters
                dkgAdapterListProp = serializedObject.FindProperty("adapterList");
                int numDockingAdapters = dkgAdapterListProp.arraySize;
                if (numDockingAdapters == 0)
                {
                    // For some reason, increasing the array size doesn't call the
                    // constructor. So set the defaults in code.
                    dkgAdapterListProp.arraySize = 1;
                    serializedObject.ApplyModifiedProperties();
                    shipDocking.adapterList[0].SetClassDefaults();
                    isSceneModified = true;
                    serializedObject.Update();
                }
                #endregion

                #region Add-Remove (FUTURE) Docking Adapters and Gizmos Buttons

                #endregion

                #region Ship Docking Adapter List

                numDockingAdapters = dkgAdapterListProp.arraySize;

                // Currently only 1 is used but in future may support multiple.
                for (int daIdx = 0; daIdx < numDockingAdapters; daIdx++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    dkgAdProp = dkgAdapterListProp.GetArrayElementAtIndex(daIdx);

                    dkgAdShowInEditorProp = dkgAdProp.FindPropertyRelative("showInEditor");
                    dkgAdShowGizmosInSceneViewProp = dkgAdProp.FindPropertyRelative("showGizmosInSceneView");
                    dkgAdSelectedInSceneViewProp = dkgAdProp.FindPropertyRelative("selectedInSceneView");
                    dkgAdRelativeDirectionProp = dkgAdProp.FindPropertyRelative("relativeDirection");

                    #region Docking Point Find/Move/Insert/Delete buttons
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 1;
                    dkgAdShowInEditorProp.boolValue = EditorGUILayout.Foldout(dkgAdShowInEditorProp.boolValue, "Ship Docking Adapter " + (daIdx + 1).ToString("00"));
                    EditorGUI.indentLevel -= 1;

                    // Find (select) in the scene
                    SelectItemInSceneViewButton(shipDocking.adapterList, dkgAdShowInEditorProp, dkgAdSelectedInSceneViewProp, dkgAdShowGizmosInSceneViewProp);

                    // Show Gizmos button
                    if (dkgAdShowGizmosInSceneViewProp.boolValue)
                    {
                        if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleToggled, GUILayout.MaxWidth(22f)))
                        {
                            dkgAdShowGizmosInSceneViewProp.boolValue = false;
                            // If it was selected, unselect it when turning gizmos off in the scene
                            if (dkgAdSelectedInSceneViewProp.boolValue)
                            {
                                dkgAdSelectedInSceneViewProp.boolValue = false;
                                Tools.hidden = false;
                            }
                        }
                    }
                    else { if (GUILayout.Button(gizmoBtnContent, toggleCompactButtonStyleNormal, GUILayout.MaxWidth(22f))) { dkgAdShowGizmosInSceneViewProp.boolValue = true; } }

                    // Move down button (FUTURE)

                    // Create duplicate button (FUTURE)

                    // Delete button (FUTURE) - can only delete if number of adapters > 1

                    GUILayout.EndHorizontal();
                    #endregion

                    if (dkgAdShowInEditorProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dkgAdProp.FindPropertyRelative("relativePosition"), dkgAdRelativePositionContent);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(dkgAdRelativeDirectionProp, dkgAdRelativeDirectionContent);
                        if (EditorGUI.EndChangeCheck() && dkgAdRelativeDirectionProp.vector3Value == Vector3.zero)
                        {
                            dkgAdRelativeDirectionProp.vector3Value = Vector3.down;
                        }
                    }

                    GUILayout.EndVertical();
                }

                #endregion

                #region Move/Insert/Delete Docking Adapters (FUTURE)

                #endregion
            }
            #endregion

            #region Event Settings
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Arrange in order of docking, docked, undocking, undocked.
                EditorGUILayout.PropertyField(onPostDockingStartDelayProp, onPostDockingStartDelayContent);
                EditorGUILayout.PropertyField(onPostDockingStartProp, onPostDockingStartContent);

                SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                EditorGUILayout.PropertyField(onPostDockingHoverDelayProp, onPostDockingHoverDelayContent);
                EditorGUILayout.PropertyField(onPostDockingHoverProp, onPostDockingHoverContent);

                SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                EditorGUILayout.PropertyField(onPostDockedDelayProp, onPostDockedDelayContent);
                EditorGUILayout.PropertyField(onPostDockedProp, onPostDockedContent);

                EditorGUILayout.PropertyField(onPostUndockingStartDelayProp, onPostUndockingStartDelayContent);
                EditorGUILayout.PropertyField(onPostUndockingStartProp, onPostUndockingStartContent);

                SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                EditorGUILayout.PropertyField(onPostUndockingHoverDelayProp, onPostUndockingHoverDelayContent);
                EditorGUILayout.PropertyField(onPostUndockingHoverProp, onPostUndockingHoverContent);

                SSCEditorHelper.DrawUILine(separatorColor, 2, 6);
                EditorGUILayout.PropertyField(onPostUndockedDelayProp, onPostUndockedDelayContent);
                EditorGUILayout.PropertyField(onPostUndockedProp, onPostUndockedContent);

                EditorGUILayout.EndVertical();
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

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && shipDocking != null)
            {
                ShipDocking.DockingState currentState = shipDocking.GetState();

                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDocking.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugCurrentStateContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(currentState.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsHoverTargetContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(shipDocking.IsHoverPointTarget ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion

            shipDocking.allowRepaint = true;
        }

        #endregion
    }
}