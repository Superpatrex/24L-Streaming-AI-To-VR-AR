using UnityEngine;
using UnityEditor;

// Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    [CustomEditor(typeof(StickyInteractiveTester))]
    public class StickyInteractiveTesterEditor : Editor
    {
        #region Custom Editor private variables

        private bool isStylesInitialised = false;
        private GUIStyle helpBoxRichText;

        #endregion

        #region GUIContent - Headers
        private readonly static GUIContent headerContent = new GUIContent("This component can be used during development to quickly test if interaction " +
                                                "is happening in the editor at runtime. It is NOT designed to be deployed with your final project." +
                                                "\n\nReference this component in any StickyInteractive object event, then hook up the appropriate function." +
                                                "\n\ne.g. OnActivated, OnDropped, OnGrabbed etc.");
        #endregion

        #region OnInspectorGUI

        // This function overides what is normally seen in the inspector window
        // This allows stuff like buttons to be drawn there
        public override void OnInspectorGUI()
        {
            #region Configure Buttons and Styles
            // Set up rich text GUIStyles
            if (!isStylesInitialised)
            {
                helpBoxRichText = new GUIStyle("HelpBox");
                helpBoxRichText.richText = true;

                isStylesInitialised = true;
            }
            #endregion

            #region Header Info and Buttons
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            #endregion
        }

        #endregion

        #region Public Static Methods

        // Add a menu item so that a StickyInteractiveTester can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sticky3D Controller/Sticky Interactive Tester")]
        public static StickyInteractiveTester CreateStickyInteractiveTester()
        {
            StickyInteractiveTester stickyInteractiveTester = null;

            // Create a new gameobject
            GameObject stickyTesterObj = new GameObject("StickyInteractiveTester");

            if (stickyTesterObj != null)
            {
                stickyInteractiveTester = stickyTesterObj.AddComponent<StickyInteractiveTester>();

                #if UNITY_EDITOR
                if (stickyInteractiveTester == null)
                {
                    Debug.LogWarning("ERROR: StickyZone.CreateStickyInteractiveTester could not add StickyInteractiveTester component to " + stickyTesterObj.name);
                }
                #endif
            }

            return stickyInteractiveTester;
        }

        #endregion
    }
}