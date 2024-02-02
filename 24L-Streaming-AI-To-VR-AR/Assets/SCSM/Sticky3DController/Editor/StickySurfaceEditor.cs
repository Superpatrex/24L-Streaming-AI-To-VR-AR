using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickySurface))]
    [CanEditMultipleObjects]
    public class StickySurfaceEditor : Editor
    {
        #region Custom Editor private variables
        private StickySurface stickySurface = null;
        private bool isStylesInitialised = false;
        private string[] surfaceTypeArray;
        private GUIStyle helpBoxRichText;
        private bool isColliderFound = false;
        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("Uses a reference to a scriptable object to allow you to set the surface type for this collider. Typically used for footstep sound and effects.");
        #endregion

        #region GUIContent - General
        private readonly static GUIContent s3dSurfacesContent = new GUIContent("Surface Types", "A shared Scriptable Object in the Project containing a list of common surface types");
        private readonly static GUIContent surfaceTypeContent = new GUIContent("Surface Type", "The surface type for this collider. Used to trigger things like footstep sounds for a Sticky3D character.");
        private readonly static GUIContent isTerrainContent = new GUIContent("Is Terrain", "Is this object a Unity terrain rather than a regular mesh?");
        #endregion

        #region Serialized Properties - General
        private SerializedProperty s3dSurfacesProp;
        private SerializedProperty guidHashSurfaceTypeProp;
        private SerializedProperty isTerrainProp;
        #endregion

        #region Events
        private void OnEnable()
        {
            stickySurface = (StickySurface)target;

            #region Find Properties - General
            s3dSurfacesProp = serializedObject.FindProperty("s3dSurfaces");
            guidHashSurfaceTypeProp = serializedObject.FindProperty("guidHashSurfaceType");
            isTerrainProp = serializedObject.FindProperty("isTerrain");
            #endregion

            RefreshSurfaceTypes();

            isColliderFound = stickySurface.GetComponent<Collider>() != null;

            isStylesInitialised = false;
        }

        #endregion

        #region Private Methods
        private void RefreshSurfaceTypes()
        {
            surfaceTypeArray = S3DSurfaces.GetNameArray(stickySurface.s3dSurfaces, true);
        }

        #endregion

        #region OnInspectorGUI
        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                isStylesInitialised = true;
            }

            EditorGUILayout.LabelField(headerContent, helpBoxRichText);

            if (!isColliderFound)
            {
                EditorGUILayout.HelpBox("Did you forget to add the collider for this (mesh) surface?", MessageType.Warning);
            }

            GUILayout.BeginVertical("HelpBox");

            serializedObject.Update();

            EditorGUILayout.PropertyField(isTerrainProp, isTerrainContent);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(s3dSurfacesProp, s3dSurfacesContent);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshSurfaceTypes();
                GUIUtility.ExitGUI();
            }

            // Find the index of the surface type using the guidHash of the type.
            int surfaceTypeIndex = 0;

            if (s3dSurfacesProp.objectReferenceValue != null)
            {
                surfaceTypeIndex = ((S3DSurfaces)s3dSurfacesProp.objectReferenceValue).GetSurfaceTypeIndex(guidHashSurfaceTypeProp.intValue);

                // Cater for the "Not Set" item at the top of the list
                surfaceTypeIndex++;
            }

            EditorGUI.BeginChangeCheck();
            surfaceTypeIndex = EditorGUILayout.Popup(surfaceTypeContent, surfaceTypeIndex, surfaceTypeArray);
            if (EditorGUI.EndChangeCheck())
            {
                if (surfaceTypeIndex <= 0) { guidHashSurfaceTypeProp.intValue = 0; }
                else
                {
                    S3DSurfaces s3dSurfaces = ((S3DSurfaces)s3dSurfacesProp.objectReferenceValue);

                    guidHashSurfaceTypeProp.intValue = s3dSurfaces == null ? 0 : s3dSurfaces.GetSurfaceTypeID(surfaceTypeIndex-1);
                }
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.EndVertical();
        }
        #endregion
    }
}