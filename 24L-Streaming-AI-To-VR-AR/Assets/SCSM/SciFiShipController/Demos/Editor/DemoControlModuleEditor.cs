using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(DemoControlModule))]
    public class DemoControlModuleEditor : Editor
    {
        #region Custom Editor private variables

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        private bool isSceneModified = false;       // used in OnInspectorGUI()
        private DemoControlModule demoControlModule = null;

        private int squadronMoveDownPos = -1;
        private int squadronInsertPos = -1;
        private int squadronDeletePos = -1;

        // SceneGUI variables
        private Vector3 componentHandlePosition = Vector3.zero;
        private Quaternion componentHandleRotation = Quaternion.identity;
        private bool isSceneViewModified = false;   // Used in SceneGUI(SceneView sv)
        private Squadron svSquadron = null;
        private Vector3 scale = Vector3.zero;
        private Vector3 theatrePos = Vector3.zero;

        #endregion

        #region GUIContent - Squadrons
        private readonly static GUIContent headerContent = new GUIContent("<b>Demo Control Module</b>\n\nThis module demonstrates how to spawn squadrons of ships");
        private readonly static GUIContent squadronHeaderContent = new GUIContent("Squadrons are groups of typically the same ship type. They can contain 0, 1 or more ships from the same faction or alliance.");
        private readonly static GUIContent squadronIdContent = new GUIContent("Squadron Id", "The unique number or ID for this squadron");
        private readonly static GUIContent squadronNameContent = new GUIContent("Squadron Name");
        private readonly static GUIContent factionIdContent = new GUIContent("Faction Id", "The Faction that the squadron belongs to or fights for.");
        private readonly static GUIContent anchorPositionContent = new GUIContent("Anchor Position", "The initial front middle position of the squadron. If there is more than one row on y-axis, rows will be created above this position.");
        private readonly static GUIContent fwdDirectionContent = new GUIContent("Forward Direction", "Direction as a normalised vector");
        private readonly static GUIContent anchorRotationContent = new GUIContent("Anchor Rotation", "The forward direction as euler angles. This is modified by setting the Forward Direction vector");
        private readonly static GUIContent tacticalFormationContent = new GUIContent("Tactical Formation", "The type of formation in which to spawn ships");
        private readonly static GUIContent rowsXContent = new GUIContent("Rows on x-axis", "The number of rows along the x-axis");
        private readonly static GUIContent rowsZContent = new GUIContent("Rows on z-axis", "The number of rows along the z-axis");
        private readonly static GUIContent rowsYContent = new GUIContent("Rows on y-axis", "The number of rows along the y-axis");
        private readonly static GUIContent offsetXContent = new GUIContent("Row offset x-axis", "The distance between rows on the x-axis");
        private readonly static GUIContent offsetZContent = new GUIContent("Row offset z-axis", "The distance between rows on the z-axis");
        private readonly static GUIContent offsetYContent = new GUIContent("Row offset y-axis", "The distance between rows on the y-axis");
        private readonly static GUIContent shipPrefabContent = new GUIContent("NPC Ship Prefab", "Non-Player-Character ship which will be used to populate the squadron");
        private readonly static GUIContent playerShipContent = new GUIContent("Player Ship", "Optionally reference to a player ship in the scene to lead this squadron");
        private readonly static GUIContent cameraTargetOffsetContent = new GUIContent("Camera Ship Offset", "The offset from the ship (in local space) for the camera to aim for.");
        #endregion

        #region GUIContent - AI Targets
        private readonly static GUIContent assignAITargetsContent = new GUIContent("Assign AI Targets","");
        private readonly static GUIContent aiTargetsContent = new GUIContent("AI Targets","");
        private readonly static GUIContent reassignTargetSecsContent = new GUIContent("Reassign Target Secs", "");
        private readonly static GUIContent AddAIScriptIfMissingContent = new GUIContent("Add AI Script If Missing", "");
        private readonly static GUIContent CrashAffectsHealthContent = new GUIContent("Crash Affects Health", "");
        private readonly static GUIContent theatreBoundsContent = new GUIContent("Theatre Bounds", "The region that the ships can fly or operate in. Extents are distance from centre.");

        #endregion

        #region GUIContent - Radar
        private readonly static GUIContent useRadarContent = new GUIContent("Use Radar", "Enable radar tracking for all ships");

        #endregion

        #region Serialized Properties

        private SerializedProperty squadronListProp;
        private SerializedProperty squadronProp;
        private SerializedProperty squadronIdProp;
        private SerializedProperty squadronNameProp;
        private SerializedProperty squadronShowInEditorProp;
        private SerializedProperty squadronTacticalFormationProp;
        private SerializedProperty squadronFwdDirectionProp;

        #endregion

        #region Event Methods

        private void OnEnable()
        {
            demoControlModule = (DemoControlModule)target;
            squadronListProp = serializedObject.FindProperty("squadronList");

            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            SceneView.duringSceneGui += SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            SceneView.onSceneGUIDelegate += SceneGUI;
            #endif

            Tools.hidden = true;

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            Tools.current = Tool.Move;

            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= SceneGUI;
            #else
            SceneView.onSceneGUIDelegate -= SceneGUI;
            #endif
        }

        private void OnDestroy()
        {
            Tools.hidden = false;
            Tools.current = Tool.Move;
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
            helpBoxRichText = new GUIStyle("HelpBox");
            helpBoxRichText.richText = true;

            labelFieldRichText = new GUIStyle("Label");
            labelFieldRichText.richText = true;

            buttonCompact = new GUIStyle("Button");
            buttonCompact.fontSize = 10;

            #endregion

            // Read in all the properties
            serializedObject.Update();

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            #region Squadrons

            EditorGUILayout.LabelField(squadronHeaderContent, helpBoxRichText);

            if (squadronListProp == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                demoControlModule.squadronList = new List<Squadron>();
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();
            }

            #region Add-Remove Squadrons

            int numSquadrons = squadronListProp.arraySize;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<color=" + txtColourName + "><b>Squadrons: " + numSquadrons.ToString("00") + "</b></color>", labelFieldRichText);

            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                demoControlModule.squadronList.Add(new Squadron());
                Undo.RecordObject(demoControlModule, "Add Squadron");
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();

                numSquadrons = squadronListProp.arraySize;
                if (numSquadrons > 0)
                {
                    // Force new squadron to be serialized in scene
                    squadronProp = squadronListProp.GetArrayElementAtIndex(numSquadrons - 1);
                    squadronIdProp = squadronProp.FindPropertyRelative("squadronId");
                    squadronIdProp.intValue = 0;
                    squadronIdProp.intValue = -1;
                }
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
            {
                if (numSquadrons > 0)
                {
                    // Get the last squadron in the list
                    squadronProp = squadronListProp.GetArrayElementAtIndex(squadronListProp.arraySize - 1);
                    if (EditorUtility.DisplayDialog("Delete Squadron?", "Do you wish to delete Squadron " + (squadronListProp.arraySize).ToString("00") + "?", "Delete Now", "Cancel"))
                    {
                        squadronListProp.arraySize -= 1;
                    }
                }
            }
            GUILayout.EndHorizontal();

            #endregion

            #region Squadron List

            // Reset button variables
            squadronMoveDownPos = -1;
            squadronInsertPos = -1;
            squadronDeletePos = -1;

            numSquadrons = squadronListProp.arraySize;
            for (int index = 0; index < numSquadrons; index++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                squadronProp = squadronListProp.GetArrayElementAtIndex(index);
                squadronIdProp = squadronProp.FindPropertyRelative("squadronId");
                squadronNameProp = squadronProp.FindPropertyRelative("squadronName");
                squadronShowInEditorProp = squadronProp.FindPropertyRelative("showInEditor");

                #region Squadron Move/Insert/Delete buttons
                GUILayout.BeginHorizontal();

                EditorGUI.indentLevel += 1;
                squadronShowInEditorProp.boolValue = EditorGUILayout.Foldout(squadronShowInEditorProp.boolValue, (index + 1).ToString("00") + ": " + squadronNameProp.stringValue);
                EditorGUI.indentLevel -= 1;

                //EditorGUILayout.LabelField((index + 1).ToString("00") + " " + squadronNameProp.stringValue);

                // Move down button
                if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numSquadrons > 1) { squadronMoveDownPos = index; }
                // Create duplicate button
                if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { squadronInsertPos = index; }
                // Delete button
                if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { squadronDeletePos = index; }
                GUILayout.EndHorizontal();
                #endregion

                #region Show/Edit Squadron
                if (squadronShowInEditorProp.boolValue)
                {
                    squadronTacticalFormationProp = squadronProp.FindPropertyRelative("tacticalFormation");
                    squadronFwdDirectionProp = squadronProp.FindPropertyRelative("fwdDirection");

                    EditorGUILayout.PropertyField(squadronIdProp, squadronIdContent);
                    EditorGUILayout.PropertyField(squadronNameProp, squadronNameContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("factionId"), factionIdContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("anchorPosition"), anchorPositionContent);

                    // Prevent zero rotation
                    if (squadronFwdDirectionProp.vector3Value == Vector3.zero) { squadronFwdDirectionProp.vector3Value = Vector3.forward; }

                    // Show the forward direction as a non-editable rotation
                    GUI.enabled = false;
                    EditorGUILayout.Vector3Field(anchorRotationContent, Quaternion.LookRotation(squadronFwdDirectionProp.vector3Value, Vector3.up).eulerAngles);
                    GUI.enabled = true;

                    EditorGUILayout.PropertyField(squadronFwdDirectionProp, fwdDirectionContent);

                    EditorGUILayout.PropertyField(squadronTacticalFormationProp, tacticalFormationContent);

                    if (squadronTacticalFormationProp.intValue != (int)Squadron.TacticalFormation.Vic && squadronTacticalFormationProp.intValue != (int)Squadron.TacticalFormation.Wedge)
                    {
                        EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("rowsX"), rowsXContent);
                    }
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("rowsZ"), rowsZContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("rowsY"), rowsYContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("offsetX"), offsetXContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("offsetZ"), offsetZContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("offsetY"), offsetYContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("shipPrefab"), shipPrefabContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("playerShip"), playerShipContent);
                    EditorGUILayout.PropertyField(squadronProp.FindPropertyRelative("cameraTargetOffset"), cameraTargetOffsetContent);
                }
                #endregion

                GUILayout.EndVertical();
            }

            #endregion

            #region Move/Insert/Delete Squadrons

            if (squadronDeletePos >= 0 || squadronInsertPos >= 0 || squadronMoveDownPos >= 0)
            {
                GUI.FocusControl(null);

                // Don't permit multiple operations in the same pass
                if (squadronMoveDownPos >= 0)
                {
                    // Move down one position, or wrap round to start of list
                    if (squadronMoveDownPos < squadronListProp.arraySize - 1)
                    {
                        squadronListProp.MoveArrayElement(squadronMoveDownPos, squadronMoveDownPos + 1);
                    }
                    else { squadronListProp.MoveArrayElement(squadronMoveDownPos, 0); }

                    squadronMoveDownPos = -1;
                }
                else if (squadronInsertPos >= 0)
                {
                    // Apply property changes before potential list changes
                    serializedObject.ApplyModifiedProperties();

                    demoControlModule.squadronList.Insert(squadronInsertPos, new Squadron(demoControlModule.squadronList[squadronInsertPos]));

                    // Read all properties from the DemoControlModule
                    serializedObject.Update();

                    // Force new squadron to be serialized in scene
                    squadronIdProp = squadronListProp.GetArrayElementAtIndex(squadronInsertPos).FindPropertyRelative("squadronId");
                    int originalID = squadronIdProp.intValue;
                    squadronIdProp.intValue = -99;
                    squadronIdProp.intValue = originalID;

                    squadronInsertPos = -1;

                    isSceneModified = true;
                }
                else if (squadronDeletePos >= 0)
                {
                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and squadronDeletePos is reset to -1.
                    int _deleteIndex = squadronDeletePos;

                    if (EditorUtility.DisplayDialog("Delete Squadron " + (squadronDeletePos + 1) + "?", "Squadron " + (squadronDeletePos + 1) + " will be deleted\n\nThis action will remove the squadron from the list and cannot be undone.", "Delete Now", "Cancel"))
                    {
                        squadronListProp.DeleteArrayElementAtIndex(_deleteIndex);
                        squadronDeletePos = -1;
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

            #region AI Targets

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignAITargets"), assignAITargetsContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aiTargets"), aiTargetsContent);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("reassignTargetSecs"), reassignTargetSecsContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AddAIScriptIfMissing"), AddAIScriptIfMissingContent);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CrashAffectsHealth"), CrashAffectsHealthContent);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("theatreBounds"), theatreBoundsContent);
            GUILayout.EndVertical();

            #endregion

            #region Radar
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRadar"), useRadarContent);
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

        #region Private Methods

        private void SceneGUI(SceneView sv)
        {
            // Only display
            if (demoControlModule != null)
            {
                int numSquadrons = demoControlModule.squadronList == null ? 0 : demoControlModule.squadronList.Count;

                isSceneViewModified = false;

                for (int sqIdx = 0; sqIdx < numSquadrons; sqIdx++)
                {
                    svSquadron = demoControlModule.squadronList[sqIdx];
                    componentHandlePosition = svSquadron.anchorPosition;
                    componentHandleRotation = Quaternion.LookRotation(svSquadron.fwdDirection, Vector3.up);

                    // Draw a sphere in the scene that is non-interactable
                    if (Event.current.type == EventType.Repaint)
                    {
                        using (new Handles.DrawingScope(Color.yellow))
                        {
                            Handles.SphereHandleCap(0, componentHandlePosition, componentHandleRotation, 3f, EventType.Repaint);
                        }
                    }

                    // Only display handle if the squadron is expanded in the editor
                    if (svSquadron.showInEditor)
                    {
                        if (Tools.current == Tool.Rotate)
                        {
                            EditorGUI.BeginChangeCheck();

                            // Draw a rotation handle
                            componentHandleRotation = Handles.RotationHandle(componentHandleRotation, componentHandlePosition);

                            // Use the rotation handle to edit the direction of thrust
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneViewModified = true;
                                Undo.RecordObject(demoControlModule, "Rotate Squadron Anchor Position");

                                // ================================
                                // TODO - THIS MAY BE WRONG....
                                // ================================
                                svSquadron.fwdDirection = componentHandleRotation * Vector3.forward;
                            }
                        }

                        #if UNITY_2017_3_OR_NEWER
                        else if (Tools.current == Tool.Move || Tools.current == Tool.Transform)
                        #else
                        else if (Tools.current == Tool.Move)
                        #endif
                        {
                            EditorGUI.BeginChangeCheck();
                            componentHandlePosition = Handles.PositionHandle(componentHandlePosition, componentHandleRotation);
                            if (EditorGUI.EndChangeCheck())
                            {
                                isSceneViewModified = true;
                                Undo.RecordObject(demoControlModule, "Move Squadron Anchor Position");
                                svSquadron.anchorPosition = componentHandlePosition;
                            }
                        }

                    }
                }

                // Draw the theatre of operations boundaries
                if (demoControlModule.theatreBounds.extents != Vector3.zero)
                {
                    Handles.DrawWireCube(demoControlModule.theatreBounds.center, demoControlModule.theatreBounds.extents * 2f);
                }

                if (Tools.current == Tool.Scale)
                {
                    scale = demoControlModule.theatreBounds.extents;
                    theatrePos = demoControlModule.theatreBounds.center;
                    EditorGUI.BeginChangeCheck();
                    scale = Handles.ScaleHandle(scale, theatrePos, Quaternion.identity, HandleUtility.GetHandleSize(theatrePos));
                    if (EditorGUI.EndChangeCheck())
                    {
                        demoControlModule.theatreBounds.extents = scale;
                        GUI.FocusControl(null);
                        Undo.RecordObject(demoControlModule, "Modify Theatre Extents");
                        isSceneViewModified = true;
                    }
                }

                #region Mark Scene Dirty if required

                if (isSceneViewModified && !Application.isPlaying)
                {
                    isSceneViewModified = false;
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }

                #endregion
            }
        }

        #endregion
    }
}
