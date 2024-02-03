﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyMovingPlatform))]
    public class StickyMovingPlatformEditor : Editor
    {
        #region Static Strings

        #endregion

        #region Custom Editor private variables
        private StickyMovingPlatform stickyMovingPlatform;
        private bool isStylesInitialised = false;
        private bool isSceneModified = false;
        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle foldoutStyleNoLabel;
        //private Color separatorColor = new Color();
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;

        private int positionMoveDownPos = -1;
        private int positionInsertPos = -1;
        private int positionDeletePos = -1;

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This module enables you to control the moment and rotation of a platform");

        #endregion

        #region GUIContent - General
        private readonly static GUIContent initialiseOnAwakeContent = new GUIContent(" Initialise on Awake", "If enabled, Initialise() will be called as soon as Awake() runs. This should be disabled if you want to control when the Sticky Moving Platform is enabled through code.");
        private readonly static GUIContent moveContent = new GUIContent(" Move", "Does the platform move?");
        private readonly static GUIContent useRelativePositionsContent = new GUIContent(" Relative Positions", "Use positions relative to the initial gameobject position, rather than absolute world space positions.");
        private readonly static GUIContent averageMoveSpeedContent = new GUIContent(" Average Move Speed", "Average movement speed of the platform in metres per second");
        private readonly static GUIContent waitTimeContent = new GUIContent(" Wait Time", "The time the platform waits at each position");
        private readonly static GUIContent movementProfileContent = new GUIContent(" Movement Profile", "The *profile* of the platform's movement. Use this to make the movement more or less smooth.");
        private readonly static GUIContent rotateContent = new GUIContent(" Rotate", "Does the platform rotate?");
        private readonly static GUIContent startingRotationContent = new GUIContent(" Starting Rotation", "The starting rotation of the platform in degrees");
        private readonly static GUIContent rotationAxisContent = new GUIContent(" Rotation Axis", "The axis of rotation of the platform.");
        private readonly static GUIContent rotationSpeedContent = new GUIContent(" Rotation Speed", "The rotational speed of the platform in degrees per second.");
        private readonly static GUIContent moveUpdateTypeContent = new GUIContent(" Move Update Type", "The update loop or timing to use for moving or rotating the platform");

        #endregion

        #region Serialized Properties - General
        private SerializedProperty initialiseOnAwakeProp;
        private SerializedProperty moveProp;
        private SerializedProperty useRelativePositionsProp;
        private SerializedProperty averageMoveSpeedProp;
        private SerializedProperty waitTimeProp;
        private SerializedProperty movementProfileProp;
        private SerializedProperty rotateProp;
        private SerializedProperty startingRotationProp;
        private SerializedProperty rotationAxisProp;
        private SerializedProperty rotationSpeedProp;
        private SerializedProperty positionListProp;
        private SerializedProperty positionProp;
        private SerializedProperty isPositionListExpandedProp;
        private SerializedProperty moveUpdateTypeProp;
        #endregion

        #region Events

        public void OnEnable()
        {
            stickyMovingPlatform = (StickyMovingPlatform)target;

            defaultEditorLabelWidth = 175f;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            //separatorColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 2f) : Color.grey;

            // Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            // Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }

            #region Find Properties - General
            initialiseOnAwakeProp = serializedObject.FindProperty("initialiseOnAwake");
            moveProp = serializedObject.FindProperty("move");
            useRelativePositionsProp = serializedObject.FindProperty("useRelativePositions");
            averageMoveSpeedProp = serializedObject.FindProperty("averageMoveSpeed");
            waitTimeProp = serializedObject.FindProperty("waitTime");
            movementProfileProp = serializedObject.FindProperty("movementProfile");
            rotateProp = serializedObject.FindProperty("rotate");
            startingRotationProp = serializedObject.FindProperty("startingRotation");
            rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
            moveUpdateTypeProp = serializedObject.FindProperty("moveUpdateType");
            positionListProp = serializedObject.FindProperty("positions");
            //isPositionListExpandedProp = serializedObject.FindProperty("isPositionListExpanded");
            #endregion
        }

        #endregion

        #region Private Methods

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

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

            // Read in all the properties
            serializedObject.Update();

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            #endregion

            #region General Settings
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.PropertyField(initialiseOnAwakeProp, initialiseOnAwakeContent);
            EditorGUILayout.PropertyField(moveProp, moveContent);
            EditorGUILayout.PropertyField(useRelativePositionsProp, useRelativePositionsContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(averageMoveSpeedProp, averageMoveSpeedContent);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                stickyMovingPlatform.UpdateAverageMoveSpeed(averageMoveSpeedProp.floatValue);
            }

            EditorGUILayout.PropertyField(waitTimeProp, waitTimeContent);
            EditorGUILayout.PropertyField(movementProfileProp, movementProfileContent);
            EditorGUILayout.PropertyField(rotateProp, rotateContent);
            EditorGUILayout.PropertyField(startingRotationProp, startingRotationContent);
            EditorGUILayout.PropertyField(rotationAxisProp, rotationAxisContent);
            EditorGUILayout.PropertyField(rotationSpeedProp, rotationSpeedContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(moveUpdateTypeProp, moveUpdateTypeContent);
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
            {
                stickyMovingPlatform.SetMoveUpdateType((StickyMovingPlatform.MoveUpdateType)moveUpdateTypeProp.intValue);
            }

            GUILayout.EndVertical();
            #endregion

            #region Positions

            GUILayout.BeginVertical("HelpBox");

            #region Check if Positions is null or less than 2 items
            // Checking the property for being NULL doesn't check if the list is actually null.
            if (stickyMovingPlatform.positions == null)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                stickyMovingPlatform.positions = new List<Vector3>(new Vector3[] { Vector3.zero, Vector3.forward * 5f });
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();
            }
            else if (stickyMovingPlatform.positions.Count < 2)
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();

                if (stickyMovingPlatform.positions.Count == 1)
                {
                    stickyMovingPlatform.positions.Add(Vector3.forward * 5f);
                }
                else
                {
                    stickyMovingPlatform.positions.Add(Vector3.zero);
                    stickyMovingPlatform.positions.Add(Vector3.forward * 5f);
                }

                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();
            }
            #endregion

            #region Add-Remove AnimActions

            int numPositions = positionListProp.arraySize;

            // Reset button variables
            positionMoveDownPos = -1;
            positionInsertPos = -1;
            positionDeletePos = -1;

            GUILayout.BeginHorizontal();

            //StickyEditorHelper.DrawS3DFoldout(isPositionListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
            EditorGUILayout.LabelField("<color=" + txtColourName + "> Positions: " + numPositions.ToString("00") + "</color>", labelFieldRichText);

            if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
            {
                // Apply property changes
                serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(stickyMovingPlatform, "Add Move Position");
                stickyMovingPlatform.positions.Add(new Vector3());
                isSceneModified = true;
                // Read in the properties
                serializedObject.Update();

                numPositions = positionListProp.arraySize;
                if (numPositions > 0)
                {
                    // Force new position to be serialized in scene
                    positionProp = positionListProp.GetArrayElementAtIndex(numPositions - 1);
                    positionProp.vector3Value *= 2f;
                    positionProp.vector3Value *= 0.5f;
                }
            }
            if (GUILayout.Button("-", GUILayout.MaxWidth(30f)))
            {
                if (numPositions > 2) { positionDeletePos = positionListProp.arraySize - 1; }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Position List members

            numPositions = positionListProp.arraySize;

            GUILayoutUtility.GetRect(1f, 2f);

            //if (isPositionListExpandedProp.boolValue)
            {
                for (int pIdx = 0; pIdx < numPositions; pIdx++)
                {
                    positionProp = positionListProp.GetArrayElementAtIndex(pIdx);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(" Pos " + (pIdx + 1).ToString("000"), GUILayout.MaxWidth(55f));
                    EditorGUILayout.PropertyField(positionProp, GUIContent.none);

                    #region Move/Insert/Delete buttons
                    // Move down button
                    if (GUILayout.Button("V", buttonCompact, GUILayout.MaxWidth(20f)) && numPositions > 2) { positionMoveDownPos = pIdx; }
                    // Create duplicate button
                    if (GUILayout.Button("I", buttonCompact, GUILayout.MaxWidth(20f))) { positionInsertPos = pIdx; }
                    // Delete button
                    if (GUILayout.Button("X", buttonCompact, GUILayout.MaxWidth(20f))) { positionDeletePos = pIdx; }
                    #endregion

                    GUILayout.EndHorizontal();
                }
            }

            #endregion

            GUILayout.EndVertical();

            #region Move / Insert / Delete Position
            if (positionDeletePos >= 0 || positionInsertPos >= 0 || positionMoveDownPos >= 0)
            {
                GUI.FocusControl(null);
                // Don't permit multiple operations in the same pass
                if (positionMoveDownPos >= 0)
                {
                    // Move down one position, or wrap round to start of list
                    if (positionMoveDownPos < positionListProp.arraySize - 1)
                    {
                        positionListProp.MoveArrayElement(positionMoveDownPos, positionMoveDownPos + 1);
                    }
                    else { positionListProp.MoveArrayElement(positionMoveDownPos, 0); }

                    positionMoveDownPos = -1;
                }
                else if (positionInsertPos >= 0)
                {
                    // NOTE: Undo doesn't work with Insert.

                    // Apply property changes before potential list changes
                    serializedObject.ApplyModifiedProperties();

                    Vector3 originalPosition = stickyMovingPlatform.positions[positionInsertPos];

                    stickyMovingPlatform.positions.Insert(positionInsertPos, new Vector3(originalPosition.x, originalPosition.y, originalPosition.z));

                    // Read all properties from the Sticky Controller
                    serializedObject.Update();

                    positionProp = positionListProp.GetArrayElementAtIndex(positionInsertPos);

                    // Force new position to be serialized in scene
                    positionProp.vector3Value *= 2f;
                    positionProp.vector3Value *= 0.5f;

                    positionInsertPos = -1;

                    isSceneModified = true;
                }
                else if (positionDeletePos >= 0 && numPositions > 2)
                {
                    // In U2019.4+ DisplayDialog seems to trigger another OnInspectorGUI() and DeletePos is reset to -1.
                    int _deleteIndex = positionDeletePos;

                    if (EditorUtility.DisplayDialog("Delete Position " + (positionDeletePos + 1) + "?", "Position " + (positionDeletePos + 1).ToString("00") + " will be deleted\n\nThis action will remove the Position from the list and cannot be undone.", "Delete Now", "Cancel"))
                    {
                        positionListProp.DeleteArrayElementAtIndex(_deleteIndex);
                        positionDeletePos = -1;
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
    }
}