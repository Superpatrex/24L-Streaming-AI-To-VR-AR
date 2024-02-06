// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class SSCEditorHelper
{
    #region Heading or Info Methods
    
    /// <summary>
    /// Display one of two technical preview messages in the editor
    /// </summary>
    /// <param name="radicalChangePossible"></param>
    public static void InTechPreview(bool radicalChangePossible = false)
    {
        if (radicalChangePossible)
        {
            EditorGUILayout.HelpBox("This feature is in technical preview and could radically change without notice.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("This feature is currently in technical preview", MessageType.Warning);
        }
    }

    /// <summary>
    /// Display a in-development warning in the editor
    /// </summary>
    public static void InDevelopment()
    {
        EditorGUILayout.HelpBox("This feature is in development, is incomplete, and could radically change without notice.", MessageType.Warning);
    }

    public static void NotImplemented()
    {
        EditorGUILayout.HelpBox("This feature has not yet been implemented", MessageType.Warning);
    }

    public static void PerformanceImpact()
    {
        EditorGUILayout.HelpBox("This feature may negatively impact performance", MessageType.Warning);
    }

    /// <summary>
    /// Draw the Sci-Fi Ship Controller version header in the inspector
    /// </summary>
    /// <param name="labelFieldRichText"></param>
    public static void SSCVersionHeader (GUIStyle labelFieldRichText)
    {
        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + SciFiShipController.ShipControlModule.SSCVersion + " " + SciFiShipController.ShipControlModule.SSCBetaVersion, labelFieldRichText);
        GUILayout.EndVertical();
    }

    #endregion

    #region Inspector GUIContent
    public readonly static GUIContent gizmoToggleBtnContent = new GUIContent("G", "Toggle gizmos and visualisations on/off for all items in the scene view");
    public readonly static GUIContent gizmoBtnContent = new GUIContent("G", "Toggle gizmos on/off in the scene view");
    public readonly static GUIContent gizmoFindBtnContent = new GUIContent("F", "Find (select) in the scene view.");
    public readonly static GUIContent gizmoLHBtnContent = new GUIContent("LH", "Show left-hand gizmos (default right-hand)");
    public readonly static GUIContent btnResetContent = new GUIContent("R", "Reset to default value(s)");
    public readonly static GUIContent debugModeIndent1Content = new GUIContent(" Debug Mode", "Use this to troubleshoot the data at runtime in the editor.");
    public readonly static GUIContent debugIsInitialisedIndent1Content = new GUIContent(" Is Initialised?");
    #endregion

    #region Inspector Methods

    /// <summary>
    /// Draw a foldout and label on a single line
    /// SSCEditorHelper.DrawSSCFoldout(showMessageSettingsInEditorProp, altMessageSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <param name="guiLabelContent"></param>
    /// <param name="foldoutStyleNoLabel"></param>
    /// <param name="defaultEditorFieldWidth"></param>
    public static void DrawSSCFoldout(SerializedProperty serializedProperty, GUIContent guiLabelContent, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth)
    {
        GUILayout.BeginHorizontal();
        EditorGUI.indentLevel += 1;
        EditorGUIUtility.fieldWidth = 15f;
        serializedProperty.boolValue = EditorGUILayout.Foldout(serializedProperty.boolValue, GUIContent.none, foldoutStyleNoLabel);
        EditorGUI.indentLevel -= 1;
        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
        EditorGUILayout.LabelField(guiLabelContent);
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw a foldout with no label
    /// Example: SSCEditorHelper.DrawSSCFoldout(isDisplayMessageListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <param name="foldoutStyleNoLabel"></param>
    /// <param name="defaultEditorFieldWidth"></param>
    public static void DrawSSCFoldout(SerializedProperty serializedProperty, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth)
    {
        EditorGUI.indentLevel += 1;
        EditorGUIUtility.fieldWidth = 15f;
        serializedProperty.boolValue = EditorGUILayout.Foldout(serializedProperty.boolValue, GUIContent.none, foldoutStyleNoLabel);
        EditorGUI.indentLevel -= 1;
        EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
    }

    /// <summary>
    /// Draw the standard set of Support, Discord, Help, and Tutorial buttons in the inspector
    /// </summary>
    /// <param name="buttonCompact"></param>
    public static void DrawSSCGetHelpButtons(GUIStyle buttonCompact)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(SSCEditorHelper.btnTxtGetSupport, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlGetSupport); }
        if (GUILayout.Button(SSCEditorHelper.btnDiscordContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlDiscordChannel); }
        if (GUILayout.Button(SSCEditorHelper.btnHelpContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.GetHelpURL()); }
        if (GUILayout.Button(SSCEditorHelper.tutorialsURLContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlTutorials); }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw a horzontal gap. e.g DrawSSCHorizontalGap(2f) // 2 pixels high
    /// </summary>
    /// <param name="h"></param>
    public static void DrawSSCHorizontalGap(float h)
    {
        GUILayoutUtility.GetRect(1f, h);
    }

    /// <summary>
    /// Draw a sprite in the inspector. Set the height so that it displays an empty space if the sprite is null
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="height"></param>
    public static void DrawSprite(GUIContent guiContent, Sprite sprite, int height, float labelWidth)
    {
        GUILayout.BeginHorizontal();
        if (guiContent != GUIContent.none) { GUILayout.Label(guiContent, GUILayout.Width(labelWidth)); }
        GUILayout.Label(AssetPreview.GetAssetPreview(sprite), GUILayout.Height(height));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw an array in the inspector
    /// </summary>
    /// <param name="serializedProperty"></param>
    /// <param name="guiContent"></param>
    public static void DrawArray(SerializedProperty serializedProperty, GUIContent labelGUIContent, float labelWidth, string elementName)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(labelGUIContent, GUILayout.Width(labelWidth-1f));
        EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("Array.size"), GUIContent.none);
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int arrayIdx = 0; arrayIdx < serializedProperty.arraySize; arrayIdx++)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(elementName + " " + (arrayIdx+1).ToString(), GUILayout.Width(labelWidth-16f));
            EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(arrayIdx), GUIContent.none);
            GUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// WIP - only left implemented
    /// Draw left and right indent sliders. Convert from/to -1.0 to 1.0 OffsetX
    /// Returns true if the offsetX property has changed
    /// </summary>
    /// <param name="offsetXProp"></param>
    /// <param name="widthProp"></param>
    /// <param name="labelWidth"></param>
    public static bool DrawOffsetLeftRight(SerializedProperty offsetXProp, SerializedProperty widthProp, float labelWidth)
    {
        float startoffsetX = offsetXProp.floatValue;

        // Normalised offsetX then subtract half the panel width
        float leftIndent = (startoffsetX + 1f) * 0.5f - (widthProp.floatValue * 0.5f);

        float newLeftIndent = EditorGUILayout.Slider(new GUIContent(" Left Indent"), leftIndent, -1f, 1f);
        if (newLeftIndent != leftIndent)
        {
            // Convert back into -1 to 1 value
            float offsetX = newLeftIndent + widthProp.floatValue - 1f;

            // Clamp -1.0 to 1.0
            offsetXProp.floatValue = offsetX < -1f ? -1f : offsetX > 1f ? 1f : offsetX;
        }

        return offsetXProp.floatValue != startoffsetX;
    }

    /// <summary>
    /// WIP - only bottom implemented
    /// Draw top and bottom indent sliders. Convert from/to -1.0 to 1.0 OffsetY
    /// Returns true if the offsetY property has changed
    /// </summary>
    /// <param name="offsetYProp"></param>
    /// <param name="heightProp"></param>
    /// <param name="labelWidth"></param>
    public static bool DrawOffsetTopBottom(SerializedProperty offsetYProp, SerializedProperty heightProp, float labelWidth)
    {
        float startoffsetY = offsetYProp.floatValue;

        // Normalised offsetX then subtract half the panel width
        float bottomIndent = (startoffsetY + 1f) * 0.5f - (heightProp.floatValue * 0.5f);

        float newBottomIndent = EditorGUILayout.Slider(new GUIContent(" Bottom Indent"), bottomIndent, -1f, 1f);
        if (newBottomIndent != bottomIndent)
        {
            // Convert back into -1 to 1 value
            float offsetY = newBottomIndent + heightProp.floatValue - 1f;

            // Clamp -1.0 to 1.0
            offsetYProp.floatValue = offsetY < -1f ? -1f : offsetY > 1f ? 1f : offsetY;
        }

        return offsetYProp.floatValue != startoffsetY;
    }

    /// <summary>
    /// Useful util drawing lines in the editor
    /// </summary>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="padding"></param>
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2f;
        r.x -= 2;
        r.width += 6f;
        EditorGUI.DrawRect(r, color);
    }

    /// <summary>
    /// Returns a vector3 as text for debugging with 0,1,2 or 3 decimal places
    /// </summary>
    /// <param name="v3"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static string GetVector3Text(Vector3 v3, int decimalPlaces)
    {
        float multiplier = decimalPlaces == 0 ? 10 : decimalPlaces == 1 ? 100f : decimalPlaces == 2 ? 1000f : 10000f;
        string sFormat = decimalPlaces == 0 ? "0" : decimalPlaces == 1 ? "0.0" : decimalPlaces == 2 ? "0.00" : "0.000";

        float x = Mathf.RoundToInt(v3.x * multiplier) / multiplier;
        float y = Mathf.RoundToInt(v3.y * multiplier) / multiplier;
        float z = Mathf.RoundToInt(v3.z * multiplier) / multiplier;

        return x.ToString(sFormat) + ", " + y.ToString(sFormat) + ", " + z.ToString(sFormat);
    }

    #endregion

    #region Scene View Methods

    /// <summary>
    /// Attempt to display the point in the centre of the sceneview
    /// zoomDistance is the distance in metres to zoom out from the point in the scene
    /// </summary>
    /// <param name="centrePoint"></param>
    /// <param name="zoomDistance"></param>
    /// <param name="sourceType"></param>
    public static void PositionSceneView(Vector3 centrePoint, float zoomDistance, Type sourceType)
    {
        try
        {
            // Switch to the scene view
            #if UNITY_2018_2_OR_NEWER
            CallMenu("Window/General/Scene");
            #else
            CallMenu("Window/Scene");
            #endif

            SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;

            // If the sceneView hasn't had the focus in this session, lastActiveSceneView will return null
            if (sceneView != null)
            {
                sceneView.LookAt(centrePoint);
                sceneView.size = zoomDistance;
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError(sourceType.Name + ": Couldn't position scene view\n" + ex.Message);
        }
    }

    /// <summary>
    /// Given a mouse position in 2D space, get the 3D Worldspace position of the "nearest" line of sight object
    /// in the scene view. Return 0,0,0 is something went wrong or no object in direct line.
    /// </summary>
    /// <param name="sceneView"></param>
    /// <param name="mousePosition"></param>
    /// <param name="normalOffset"></param>
    /// <param name="worldspacePoint"></param>
    /// <param name="showErrors"></param>
    /// <returns></returns>
    public static bool GetPositionFromMouse(SceneView sceneView, Vector2 mousePosition, float normalOffset, ref Vector3 worldspacePoint, bool showErrors)
    {
        bool isSuccessful = false;
        worldspacePoint = Vector3.zero;
        string methodName = "SSCEditorHelper.GetPositionFromMouse";

        try
        {
            if (sceneView != null)
            {
                // Only process mouse positions over a scene view.
                // This avoid the situation where scene view has focus but mouse is over another window
                var win = EditorWindow.mouseOverWindow;
                if (win != null && win.GetType() == typeof(SceneView))
                {
                    Camera svCamera = sceneView.camera;
                    if (svCamera != null)
                    {
                        // Cast a ray from the scene view camera through the mouse point onto (hopefully) the nearest object.
                        // Mouse position Y in screen space is inverted
                        Ray ray = svCamera.ScreenPointToRay(new Vector3(mousePosition.x, svCamera.pixelHeight - mousePosition.y, svCamera.nearClipPlane));
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            // We hit something
                            worldspacePoint = hit.point + (hit.normal * normalOffset);
                        }
                        else
                        {
                            // If we didn't hit anything, create a point infront of the scene view camera
                            worldspacePoint = ray.GetPoint(100f);
                        }

                        isSuccessful = true;
                    }
                }
            }

        }
        catch (System.Exception ex) { Debug.LogWarning(methodName + " - sorry, something went wrong\n" + ex.Message); }

        return isSuccessful;
    }

    /// <summary>
    /// Given a mouse position in 2D space, get the 3D Worldspace position of the "nearest" line of sight object.
    /// Return 0,0,0 is something went wrong.
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <param name="showErrors"></param>
    /// <returns></returns>
    public static Vector3 GetPositionFromMouse(Vector2 mousePosition, bool showErrors)
    {
        Vector3 locationPoint = Vector3.zero;
        string methodName = "SSCEditorHelper.GetPositionFromMouse";

        try
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            bool isHit = false;

            if (sceneView != null)
            {
                Camera svCamera = sceneView.camera;
                if (svCamera != null)
                {
                    // Cast a ray from the scene view camera through the mouse point onto (hopefully) the nearest object.
                    // Mouse position Y in screen space is inverted
                    Ray ray = svCamera.ScreenPointToRay(new Vector3(mousePosition.x, svCamera.pixelHeight - mousePosition.y, svCamera.nearClipPlane));
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        // We hit something
                        locationPoint = hit.point;
                        isHit = true;
                    }

                    if (!isHit)
                    {
                        // If we didn't hit anything, create a point infront of the scene view camera
                        locationPoint = ray.GetPoint(100f);
                    }
                }
            }

        }
        catch (System.Exception ex) { Debug.LogWarning(methodName + " - sorry, something went wrong\n" + ex.Message); }

        return locationPoint;
    }


    #endregion

    #region Menu Helper Methods

    /// <summary>
    /// Call an item from the Unity menu. Menu can also be one custom created.
    /// USAGE: SSCEditorHelper.CallMenu("Edit/Project Settings/Player");
    /// </summary>
    /// <param name="menuItemPath"></param>
    public static void CallMenu(string menuItemPath)
    {
        if (!string.IsNullOrEmpty(menuItemPath))
        {
            EditorApplication.ExecuteMenuItem(menuItemPath);
        }
    }

    #endregion

    #region Link Helper variables and Methods

    // URL buttons

    //public static readonly string urlAssetPage = "http://u3d.as/1oPf";
    public static readonly string urlGetSupport = "http://forum.unity3d.com/threads/594448";
    public static readonly string urlDiscordChannel = "https://discord.gg/CjzCK4b";
    public static readonly string urlTutorials = "https://scsmmedia.com/sci-fi-ship-controller#938b782a-2c40-4590-9f23-de983ed33787";

    //public static readonly string btnTxtAssetPage = "Asset Page";
    public static readonly string btnTxtGetSupport = "Get Support";
    public static readonly GUIContent tutorialsURLContent = new GUIContent("Tutorials", "Go to our website for a link to Video Tutorials about SSC concepts");
    public static readonly GUIContent btnHelpContent = new GUIContent("Help", "Sci-Fi Ship Controller manual. Requires Adobe Reader\nAvailable from\nadobe.com/reader");
    public static readonly GUIContent btnDiscordContent = new GUIContent("Discord", "Open SSC Discord Channel in browser");

    /// <summary>
    /// Get help PDF path to point to the local copy of the manual.
    /// </summary>
    /// <returns></returns>
    public static string GetHelpURL()
    {
        // Build a link to the file manual.
        #if UNITY_EDITOR_OSX
            return "File:///" + Application.dataPath.Replace(" " ,"%20") + "/scsm/SciFiShipController/ssc_manual.pdf";
        #else
            return "file:///" + Application.dataPath + "/scsm/SciFiShipController/ssc_manual.pdf";
        #endif
    }

    #endregion

    #region Audio

    /// <summary>
    /// Play an audio clip in the editor
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="startSample"></param>
    /// <param name="isLoop"></param>
    public static void PlayAudioClip(AudioClip audioClip, int startSample = 0, bool isLoop = false)
    {
        try
        {
            System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilType = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            #if UNITY_2020_2_OR_NEWER
            string playClipMethodName = "PlayPreviewClip";
            #else
            string playClipMethodName = "PlayClip";
            #endif

            // Get the method to play an audio clip in the editor
            System.Reflection.MethodInfo playClipMethod = audioUtilType.GetMethod(playClipMethodName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null, new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null
            );
            playClipMethod.Invoke(null, new object[] { audioClip, startSample, isLoop });
        }
        catch (System.Exception ex)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("SSCEditorHelper PlayAudioClip - " + ex.Message);
            #else
            if (ex != null) { }
            #endif
        }
    }

    /// <summary>
    /// Stop all audio clips playing in the editor
    /// </summary>
    public static void StopAllAudioClips()
    {
        try
        {
            System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilType = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            #if UNITY_2020_2_OR_NEWER
            string stopClipMethodName = "StopAllPreviewClips";
            #else
            string stopClipMethodName = "StopAllClips";
            #endif

            System.Reflection.MethodInfo stopClipsMethod = audioUtilType.GetMethod(stopClipMethodName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null, new System.Type[] { }, null
            );
            stopClipsMethod.Invoke(null, new object[] { } );
        }
        catch (System.Exception ex)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("SSCEditorHelper StopAllAudioClips - " + ex.Message);
            #else
            if (ex != null) { }
            #endif
        }
    }

    #endregion

    #region Camera Methods

    // See SSCUtils.cs

    #endregion

    #region Gameobject and Transform Methods

    /// <summary>
    /// Find or create a child gameobject. If it doesn't already exist, optionally make it a child of the parent.
    /// </summary>
    /// <param name="parentGameObject"></param>
    /// <param name="gameObjectName"></param>
    /// <param name="isMakeChildOnCreate"></param>
    /// <returns></returns>
    public static GameObject GetOrCreateChildGameObject(GameObject parentGameObject, string gameObjectName, bool isMakeChildOnCreate)
    {
        GameObject gameObject = null;

        if (parentGameObject != null)
        {
            Transform parentTfrm = parentGameObject.transform;
            Transform trfm = parentTfrm.Find(gameObjectName);

            // Not found so create a new gameobject
            if (trfm == null)
            {
                gameObject = new GameObject(gameObjectName);

                if (gameObject != null)
                {
                    trfm = gameObject.transform;

                    if (isMakeChildOnCreate)
                    {
                        trfm.SetParent(parentTfrm, false);
                    }
                }
            }
            else
            {
                gameObject = trfm.gameObject;
            }
        }

        return gameObject;
    }

    #endregion

    #region Project Helper Methods

    /// <summary>
    /// Reveal or hightlight the folder that in the Project window (if it is open and visible
    /// on the panel it is on). Highlights the item in yellow for a second or two.
    /// By default also selects the object. Not selecting the object can be useful if called
    /// from a CustomEditor inspector script so as not to loose focus.
    /// NOTE: The Project won't expand if it is already selected AND the user has collapsed it.
    /// USAGE: SSCEditorHelper.HighlightFolderInProjectWindow(SSCSetup.effectsFolder, true, true);
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="selectFolder"></param>
    /// <param name="showErrors"></param>
    public static void HighlightFolderInProjectWindow(string folderPath, bool selectFolder, bool showErrors)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(folderPath, typeof(UnityEngine.Object));
            if (obj != null)
            {
                // Highlight the item in yellow for a second or two.
                EditorGUIUtility.PingObject(obj);

                if (selectFolder) { UnityEditor.Selection.activeObject = obj; }
            }
        }
        else { Debug.Log("SSCEditorHelper.HighLightFolder - the following folder does not exist: " + folderPath); }
    }

    #endregion

    #region IO Helper Methods


    /// <summary>
    /// Get a full file path from the user.
    /// Has the option to restrict to only files in the current Unity project.
    /// fileExtensions format: { "Texture2D", "png,psd,jpg,jpeg", "All files", "*" }
    /// </summary>
    /// <param name="fileTypeName"></param>
    /// <param name="relativeFolderToOpen"></param>
    /// <param name="fileExtensions"></param>
    /// <param name="restrictToProject"></param>
    /// <param name="folderPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool GetFilePathFromUser(string fileTypeName, string relativeFolderToOpen, string[] fileExtensions, bool restrictToProject, ref string folderPath, ref string fileName)
    {
        bool isSuccessful = false;

        // Returns the full absolute path
        string path = EditorUtility.OpenFilePanelWithFilters(fileTypeName, relativeFolderToOpen, fileExtensions);
        if (path.Contains(Application.dataPath) || (!restrictToProject && !string.IsNullOrEmpty(path)))
        {
            // Make sure the text field doesn't have the focus, else it won't update until user
            // moves to another control.
            GUI.FocusControl("");

            folderPath = System.IO.Path.GetDirectoryName(path);
            fileName = System.IO.Path.GetFileName(path);

            isSuccessful = true;
        }
        // Did user cancel open file panel?
        else if (string.IsNullOrEmpty(path)) { }
        else
        {
            EditorUtility.DisplayDialog(fileTypeName + " File", "The file must be in the Assets folder of your project", "OK");
        }

        return isSuccessful;
    }

    /// <summary>
    /// Get a folder from the user (EDITOR ONLY)
    /// EXAMPLE: GetPathFromUser("Path Data", SSCSetup.sscFolder, false, ref pathFileFolder);
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="relativeFolderToOpen"></param>
    /// <param name="restrictToProject"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public static bool GetPathFromUser(string dialogTitle, string relativeFolderToOpen, bool restrictToProject, ref string folderPath)
    {
        bool isSuccessful = false;

        // Returns the full absolute path
        string path = EditorUtility.OpenFolderPanel(dialogTitle, relativeFolderToOpen, "");
        if ((!restrictToProject && !string.IsNullOrEmpty(path)) || path.Contains(Application.dataPath))
        {
            if (restrictToProject)
            {
                // Get the relative path from Assets
                if (path.Length > Application.dataPath.Length) { path = path.Remove(0, Application.dataPath.Length); }
                if (path.Length > 1)
                {
                    if (path[0] == '/') { path = path.Remove(0, 1); }
                }
            }

            // Make sure the text field doesn't have the focus, else it won't update until user
            // moves to another control.
            GUI.FocusControl("");

            folderPath = path;
            isSuccessful = true;
        }
        // Did user cancel open folder panel?
        else if (string.IsNullOrEmpty(path)) { }
        else
        {
            EditorUtility.DisplayDialog(dialogTitle + " Folder", "The folder must be in the Assets folder of your project", "OK");
        }

        return isSuccessful;
    }

    /// <summary>
    /// Get the Project assets folder for an item in the Project folder given a full file path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetAssetFolderFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) { return string.Empty; }
        else
        {
            // First attempts to simply strip out the Application.dataPath, then tries to replace the forward slash with back slashes and then try stripping the dataPath again.
            return "Assets" + System.IO.Path.GetDirectoryName(filePath).Replace(Application.dataPath, "").Replace(Application.dataPath.Replace("/", "\\"), "");
        }
    }

    #endregion

    #region Dialog Helpers

    /// <summary>
    /// Display an informational dialog box
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="dialogText"></param>
    public static void PromptGotIt(string dialogTitle, string dialogText)
    {
        EditorUtility.DisplayDialog(dialogTitle, dialogText, "Got it!");
    }

    /// <summary>
    /// Prompt user to respond Yes or No
    /// </summary>
    /// <param name="dialogTile"></param>
    /// <param name="dialogText"></param>
    /// <returns></returns>
    public static bool PromptYesNo(string dialogTile, string dialogText)
    {
        return EditorUtility.DisplayDialog(dialogTile, dialogText, "Yes", "NO!");
    }

    /// <summary>
    /// Prompt user to continue with an action.
    /// </summary>
    /// <param name="dialogTile"></param>
    /// <param name="dialogText"></param>
    /// <returns></returns>
    public static bool PromptForContinue(string dialogTile, string dialogText)
    {
        return EditorUtility.DisplayDialog(dialogTile, dialogText, "Yes", "CANCEL!");
    }

    /// <summary>
    /// Prompt the user to delete something or cancel.
    /// if (SSCEditorHelper.PromptForDelete("Delete Items?", labelText)) {...}
    /// </summary>
    /// <param name="dialogTile"></param>
    /// <param name="dialogText"></param>
    /// <returns></returns>
    public static bool PromptForDelete(string dialogTile, string dialogText)
    {
        return EditorUtility.DisplayDialog(dialogTile, dialogText, "Delete Now", "Cancel");
    }

    #endregion

    #region Canvas Helpers

    public static UnityEngine.UI.Image AddCanvasPanelIfMissing
    (
        UnityEngine.UI.Image[] uiImageArray,
        string panelName,
        float panelOffsetX, float panelOffsetY,
        float panelWidth, float panelHeight,
        float anchorMinX, float anchorMinY,
        float anchorMaxX, float anchorMaxY,
        Transform parentTrfm
    )
    {
        UnityEngine.UI.Image panelImg = ArrayUtility.Find(uiImageArray, img => img.name == panelName);
        if (panelImg == null)
        {
            GameObject panelGO = new GameObject(panelName);
            panelGO.layer = 5;
            panelGO.transform.SetParent(parentTrfm);
            RectTransform rectTrfm = panelGO.AddComponent<RectTransform>();
            rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

            rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

            panelGO.transform.position = new Vector3(panelOffsetX + (panelWidth * 0.5f), panelOffsetY + (panelHeight * 0.5f), 0f);

            panelGO.AddComponent<CanvasRenderer>();
            panelImg = panelGO.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(1f,1f,1f, 50f / 255f);
            panelImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            panelImg.type = UnityEngine.UI.Image.Type.Sliced;
            panelImg.raycastTarget = false;
            panelImg.fillCenter = false;
        }

        return panelImg;
    }

    public static UnityEngine.UI.RawImage AddCanvasRawPanelIfMissing
    (
        UnityEngine.UI.RawImage[] uiImageArray,
        string panelName,
        float panelOffsetX, float panelOffsetY,
        float panelWidth, float panelHeight,
        float anchorMinX, float anchorMinY,
        float anchorMaxX, float anchorMaxY,
        Texture imgTexture,
        Transform parentTrfm,
        Vector3 canvasScale
    )
    {
        UnityEngine.UI.RawImage panelImg = ArrayUtility.Find(uiImageArray, img => img.name == panelName);
        if (panelImg == null)
        {
            GameObject panelGO = new GameObject(panelName);
            panelGO.layer = 5;
            panelGO.transform.SetParent(parentTrfm);
            RectTransform rectTrfm = panelGO.AddComponent<RectTransform>();
            rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

            rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

            panelGO.transform.position = new Vector3((panelOffsetX * canvasScale.x) + (panelWidth * 0.5f * canvasScale.x), (panelOffsetY * canvasScale.y) + (panelHeight * 0.5f * canvasScale.y), 0f);

            panelGO.AddComponent<CanvasRenderer>();
            panelImg = panelGO.AddComponent<UnityEngine.UI.RawImage>();
            // No transparency as the colour for the texture will set the transparency for each element
            panelImg.color = new Color(1f,1f,1f, 255f / 255f);
            panelImg.raycastTarget = false;
            panelImg.texture = imgTexture;
        }

        return panelImg;
    }

    #endregion

    #region Search Helpers

    /// <summary>
    /// Does the string value of the property match the search criteria.
    /// Typically this would be the name value of a LocationData or PathData class instance.
    /// If the search string is empty, always return true.
    /// </summary>
    /// <param name="locationDataToMatch"></param>
    /// <param name="searchFilter"></param>
    /// <returns></returns>
    public static bool IsInSearchFilter(SerializedProperty serializedProperty, string searchFilter)
    {
        // If the search string is empty, always return true.
        if (string.IsNullOrEmpty(searchFilter)) { return true; }
        else
        {
            // If this property has no stringvalue (typically a name), it can't be a match
            return !string.IsNullOrEmpty(serializedProperty.stringValue) && serializedProperty.stringValue.ToLower().Contains(searchFilter.ToLower());
        }
    }

    /// <summary>
    /// Draw the search filter control in the editor
    /// </summary>
    /// <param name="filterContent"></param>
    /// <param name="labelStyle"></param>
    /// <param name="searchFieldStyle"></param>
    /// <param name="cancelButtonStyle"></param>
    /// <param name="editorSearchFilter"></param>
    public static void DrawSearchFilterControl(GUIContent filterContent, GUIStyle labelStyle, GUIStyle searchFieldStyle, GUIStyle cancelButtonStyle, ref string editorSearchFilter)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(2f));
        GUILayout.Label(filterContent, labelStyle, GUILayout.Width(50f));
        if (editorSearchFilter == null) { editorSearchFilter = string.Empty; }
        editorSearchFilter = GUILayout.TextField(editorSearchFilter, searchFieldStyle);

        // Sometimes the search text is reverse search almost seems invisible (small search icon and cancel cannot be seen)
        // Not sure how this occurs.
        if (GUILayout.Button("", cancelButtonStyle))
        {
            editorSearchFilter = string.Empty;
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
    }

    #endregion

    #region Prefab Helpers

    /// <summary>
    /// Return true if is part of a prefab in Project assets folder (or subfolders)
    /// but is NOT an instance of the prefab a scene.
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static bool IsPrefabAsset(GameObject go)
    {
        return PrefabUtility.IsPartOfPrefabAsset(go);
    }

    #endregion

    #region Miscellaneous

    #endregion
}
