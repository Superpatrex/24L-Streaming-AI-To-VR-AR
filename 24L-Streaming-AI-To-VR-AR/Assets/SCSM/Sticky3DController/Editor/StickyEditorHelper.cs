using UnityEngine;
using UnityEditor;

// Sticky3D Control Module Copyright (c) 2019-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    public class StickyEditorHelper
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

        /// <summary>
        /// Warn the user that they haven't assigned an Ammo Types scriptable object
        /// </summary>
        public static void NoAmmoTypesAssigned()
        {
            EditorGUILayout.HelpBox("There is no Ammo Types scriptable object assigned", MessageType.Warning);
        }

        /// <summary>
        /// Warn the user that they haven't assigned an Mag Types scriptable object
        /// </summary>
        public static void NoMagTypesAssigned()
        {
            EditorGUILayout.HelpBox("There is no Mag Types scriptable object assigned", MessageType.Warning);
        }

        /// <summary>
        /// Warn the user that they haven't assigned an Mag Types scriptable object
        /// </summary>
        public static void NoMagAttachPointAssigned(int weaponButtonNumber)
        {
            EditorGUILayout.HelpBox("There is Mag Attach Point assigned for firing mechanism " + weaponButtonNumber.ToString(), MessageType.Warning);
        }

        public static void NotImplemented()
        {
            EditorGUILayout.HelpBox("This feature has not yet been implemented", MessageType.Warning);
        }

        public static void PerformanceImpact()
        {
            EditorGUILayout.HelpBox("This feature may negatively impact performance", MessageType.Warning);
        }

        #endregion

        #region Link Helper variables and Methods

        public static readonly string urlGetSupport = "http://forum.unity3d.com/threads/995707";
        public static readonly string urlDiscordChannel = "https://discord.gg/CjzCK4b";
        public static readonly string urlTutorials = "https://scsmmedia.com/sticky3d-controller#ee164ebd-6618-4a32-8811-ba4328a6f26c";

        public static readonly string btnTxtGetSupport = "Get Support";

        public static readonly GUIContent btnDiscordContent = new GUIContent("Discord", "Open S3D Discord Channel in browser");
        public static readonly GUIContent btnHelpContent = new GUIContent("Help", "Sticky3D Controller manual. Requires Adobe Reader\nAvailable from\nadobe.com/reader");
        public static readonly GUIContent tutorialsURLContent = new GUIContent("Tutorials", "Go to our website for a link to Video Tutorials about S3D concepts");
        public static readonly GUIContent btnNewContent = new GUIContent("New");
        public static readonly GUIContent resetBtnContent = new GUIContent("Reset", "Reset to default values");
        public static readonly GUIContent resetBtnSmlContent = new GUIContent("R", "Reset to default values");

        /// <summary>
        /// Get help PDF path to point to the local copy of the manual.
        /// </summary>
        /// <returns></returns>
        public static string GetHelpURL()
        {
            // Build a link to the file manual.
            #if UNITY_EDITOR_OSX
                return "File:///" + Application.dataPath.Replace(" " ,"%20") + "/scsm/Sticky3DController/s3d_manual.pdf";
            #else
                return "file:///" + Application.dataPath + "/scsm/Sticky3DController/s3d_manual.pdf";
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

        #region Handle Methods

        /// <summary>
        /// Draw the gizmos which show where the hand would be placed on the object. Arrow points in palm direction. Sphere is in the thumb up direction.
        /// Fingers show where they would face.
        /// </summary>
        /// <param name="handlePos"></param>
        /// <param name="handleRot"></param>
        /// <param name="gizmoColour"></param>
        /// <param name="relHandleSize"></param>
        /// <param name="isLeftHandGizmos"></param>
        public static void DrawHandNonInteractableGizmos(Vector3 handlePos, Quaternion handleRot, Color gizmoColour, float relHandleSize, bool isLeftHandGizmos)
        {
            using (new Handles.DrawingScope(gizmoColour))
            {
                // Forwards direction of the hand hold position
                Handles.ArrowHandleCap(GUIUtility.GetControlID(FocusType.Passive), handlePos, handleRot, 1.25f * relHandleSize, EventType.Repaint);

                // Draw the up direction with a sphere on top of a line
                Quaternion upRotation = handleRot * Quaternion.Euler(isLeftHandGizmos ? 90f : 270f, 0f, 0f);
                Handles.DrawLine(handlePos, handlePos + upRotation * (Vector3.forward * relHandleSize * 1.25f));
                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), handlePos + (upRotation * (Vector3.forward * relHandleSize * 1.25f)), upRotation, 0.3f * relHandleSize, EventType.Repaint);

                // Direction of Fingers
                // Althought they are not always in the correct order when drawn in the scene - like pinky is where thumb should be
                Quaternion fingerRotation = handleRot * Quaternion.Euler(0f, 90f, 0f);
                // Thumb
                Vector3 fingerStartPos = handlePos + (upRotation * (Vector3.forward * relHandleSize * 0.04f));
                Handles.DrawDottedLine(fingerStartPos, fingerStartPos + (fingerRotation * (Vector3.forward * relHandleSize * 0.7f)), 3f);
                // Index finger
                fingerStartPos += upRotation * (Vector3.back * relHandleSize * 0.04f);
                Handles.DrawDottedLine(fingerStartPos, fingerStartPos + (fingerRotation * (Vector3.forward * relHandleSize * 1.18f)), 3f);
                // Middle finger
                fingerStartPos += upRotation * (Vector3.back * relHandleSize * 0.04f);
                Handles.DrawDottedLine(fingerStartPos, fingerStartPos + (fingerRotation * (Vector3.forward * relHandleSize * 1.25f)), 3f);
                // ring finger
                fingerStartPos += upRotation * (Vector3.back * relHandleSize * 0.04f);
                Handles.DrawDottedLine(fingerStartPos, fingerStartPos + (fingerRotation * (Vector3.forward * relHandleSize * 1.2f)), 3f);
                // little finger
                fingerStartPos += upRotation * (Vector3.back * relHandleSize * 0.04f);
                Handles.DrawDottedLine(fingerStartPos, fingerStartPos + (fingerRotation * (Vector3.forward * relHandleSize * 1.0f)), 3f);
            }
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
        /// Draw the standard set of Support, Discord, Help, and Tutorial buttons in the inspector
        /// </summary>
        /// <param name="buttonCompact"></param>
        public static void DrawGetHelpButtons(GUIStyle buttonCompact)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(StickyEditorHelper.btnTxtGetSupport, buttonCompact)) { Application.OpenURL(StickyEditorHelper.urlGetSupport); }
            if (GUILayout.Button(StickyEditorHelper.btnDiscordContent, buttonCompact)) { Application.OpenURL(StickyEditorHelper.urlDiscordChannel); }
            if (GUILayout.Button(StickyEditorHelper.btnHelpContent, buttonCompact)) { Application.OpenURL(StickyEditorHelper.GetHelpURL()); }
            if (GUILayout.Button(StickyEditorHelper.tutorialsURLContent, buttonCompact)) { Application.OpenURL(StickyEditorHelper.urlTutorials); }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a Gizmos button that can be toggled on/off.
        /// </summary>
        /// <param name="serializedProperty"></param>
        public static void DrawGizmosButton(SerializedProperty serializedProperty, GUIStyle buttonNormal, GUIStyle buttonToggled)
        {
            // Show Gizmos button
            if (serializedProperty.boolValue) { if (GUILayout.Button(gizmoBtnContent, buttonToggled, GUILayout.MaxWidth(22f))) { serializedProperty.boolValue = false; } }
            else { if (GUILayout.Button(gizmoBtnContent, buttonNormal, GUILayout.MaxWidth(22f))) { serializedProperty.boolValue = true; } }
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Draw a horzontal gap. e.g DrawHorizontalGap(2f) // 2 pixels high
        /// </summary>
        /// <param name="h"></param>
        public static void DrawHorizontalGap (float h)
        {
            GUILayoutUtility.GetRect(1f, h);
        }

        /// <summary>
        /// Draw a basic word-wrapped label in a helpbox
        /// </summary>
        /// <param name="infoContent"></param>
        /// <param name="indent"></param>
        public static void DrawInformationLabel (GUIContent infoContent, float indent = 3f)
        {
            if (indent > 0f)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(indent));
            }

            EditorGUILayout.BeginVertical("HelpBox");

            //GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField(infoContent, new GUIStyle(EditorStyles.wordWrappedLabel));
            EditorGUILayout.EndVertical();

            if (indent > 0f)
            {
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draw a empty label with an indent of given pixels
        /// Usage:
        ///   EditorGUILayout.BeginHorizontal();
        ///   DrawLabelIndent(10f);
        ///   ...
        ///   EditorGUILayout.EndHorizontal();
        /// </summary>
        /// <param name="indent"></param>
        public static void DrawLabelIndent (float indent)
        {
            EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(indent));
        }

        /// <summary>
        /// Draw a label with an indent of the given pixels.
        /// Optionally specify a MaxWidth of the label.
        /// Usage:
        ///   EditorGUILayout.BeginHorizontal();
        ///   DrawLabelIndent(10f, myGUIContent);
        ///   ...
        ///   EditorGUILayout.EndHorizontal();
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="guiContent"></param>
        public static void DrawLabelIndent (float indent, GUIContent guiContent, float maxWidth = 0f)
        {
            DrawLabelIndent(indent);
            if (maxWidth > 0f) { EditorGUILayout.LabelField(guiContent, GUILayout.MaxWidth(maxWidth)); }
            else { EditorGUILayout.LabelField(guiContent); }
        }

        /// <summary>
        /// Draw a label with an indent of the given pixels.
        /// Optionally specify a MaxWidth of the label.
        /// Usage:
        ///   EditorGUILayout.BeginHorizontal();
        ///   StickyEditorHelper.DrawLabelIndent(10f, "My Label");
        ///   ...
        ///   EditorGUILayout.EndHorizontal();
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="labelText"></param>
        public static void DrawLabelIndent(float indent, string labelText, float maxWidth = 0f)
        {
            DrawLabelIndent(indent);
            if (maxWidth > 0f) { EditorGUILayout.LabelField(labelText, GUILayout.MaxWidth(maxWidth)); }
            else { EditorGUILayout.LabelField(labelText); }
        }

        /// <summary>
        /// Draw a Left Hand (gizmos) button that can be toggled on/off.
        /// </summary>
        /// <param name="serializedProperty"></param>
        public static void DrawLHGizmosButton(SerializedProperty serializedProperty, GUIStyle buttonNormal, GUIStyle buttonToggled)
        {
            // Show Gizmos button
            if (serializedProperty.boolValue) { if (GUILayout.Button(gizmoLHBtnContent, buttonToggled, GUILayout.MaxWidth(36f))) { serializedProperty.boolValue = false; } }
            else { if (GUILayout.Button(gizmoLHBtnContent, buttonNormal, GUILayout.MaxWidth(36f))) { serializedProperty.boolValue = true; } }
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Draw and indented PropertyField with label.
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="serializedProperty"></param>
        /// <param name="guiConent"></param>
        /// <param name="defaultEditorLabelWidth"></param>
        public static void DrawPropertyIndent(float indent, SerializedProperty serializedProperty, GUIContent guiContent, float defaultEditorLabelWidth)
        {
            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent, guiContent, defaultEditorLabelWidth - indent - 3f);
            EditorGUILayout.PropertyField(serializedProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw an indented Event PropertyField with the event name.
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="serializedProperty"></param>
        /// <param name="guiConent"></param>
        public static void DrawEventPropertyIndent(float indent, SerializedProperty serializedProperty, GUIContent guiContent)
        {
            EditorGUILayout.BeginHorizontal();
            StickyEditorHelper.DrawLabelIndent(indent);
            EditorGUILayout.PropertyField(serializedProperty, guiContent);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a button that can be toggled on/off. Return true if it has changed state.
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="buttonNormal"></param>
        /// <param name="buttonToggled"></param>
        /// <param name="buttonContent"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        public static bool DrawToggleButton(SerializedProperty serializedProperty, GUIStyle buttonNormal, GUIStyle buttonToggled, GUIContent buttonContent, float maxWidth)
        {
            bool startValue = serializedProperty.boolValue;

            if (serializedProperty.boolValue) { if (GUILayout.Button(buttonContent, buttonToggled, GUILayout.MaxWidth(maxWidth))) { serializedProperty.boolValue = false; } }
            else { if (GUILayout.Button(buttonContent, buttonNormal, GUILayout.MaxWidth(maxWidth))) { serializedProperty.boolValue = true; } }

            return startValue != serializedProperty.boolValue;
        }

        /// <summary>
        /// Delete or move an item within a list.
        /// Does NOT prompt before delete.
        /// </summary>
        /// <param name="listProp"></param>
        /// <param name="deletePos"></param>
        /// <param name="moveDownPos"></param>
        public static void DrawDeleteMoveInList(SerializedProperty listProp, ref int deletePos, ref int moveDownPos)
        {
            #region Delete/move item in list
            if (deletePos >= 0 || moveDownPos >= 0)
            {
                GUI.FocusControl(null);
                // Don't permit multiple operations in the same pass
                if (moveDownPos >= 0)
                {
                    if (listProp.arraySize > 1)
                    {
                        // Move down one position, or wrap round to 1st position in list
                        if (moveDownPos < listProp.arraySize - 1)
                        {
                            listProp.MoveArrayElement(moveDownPos, moveDownPos + 1);
                        }
                        else { listProp.MoveArrayElement(moveDownPos, 0); }
                    }
                    moveDownPos = -1;
                }
                else if (deletePos >= 0)
                {
                    listProp.DeleteArrayElementAtIndex(deletePos);
                }
            }

            #endregion
        }

        /// <summary>
        /// Draw a foldout and label on a single line.
        /// isFoldedOut = StickyEditorHelper.DrawS3DFoldout(isFoldedOut, mySettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
        /// </summary>
        /// <param name="isFoldout"></param>
        /// <param name="guiLabelContent"></param>
        /// <param name="foldoutStyleNoLabel"></param>
        /// <param name="defaultEditorFieldWidth"></param>
        /// <returns></returns>
        public static bool DrawS3DFoldout(bool isFoldout, GUIContent guiLabelContent, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isFoldout = EditorGUILayout.Foldout(isFoldout, GUIContent.none, foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            EditorGUILayout.LabelField(guiLabelContent);
            GUILayout.EndHorizontal();

            return isFoldout;
        }

        /// <summary>
        /// Draw a foldout and label on a single line
        /// StickyEditorHelper.DrawS3DFoldout(showMessageSettingsInEditorProp, altMessageSettingsContent, foldoutStyleNoLabel, defaultEditorFieldWidth);
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="guiLabelContent"></param>
        /// <param name="foldoutStyleNoLabel"></param>
        /// <param name="defaultEditorFieldWidth"></param>
        public static void DrawS3DFoldout(SerializedProperty serializedProperty, GUIContent guiLabelContent, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth)
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
        /// Example: StickyEditorHelper.DrawS3DFoldout(isPositionListExpandedProp, foldoutStyleNoLabel, defaultEditorFieldWidth);
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="foldoutStyleNoLabel"></param>
        /// <param name="defaultEditorFieldWidth"></param>
        public static void DrawS3DFoldout(SerializedProperty serializedProperty, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth)
        {
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            serializedProperty.boolValue = EditorGUILayout.Foldout(serializedProperty.boolValue, GUIContent.none, foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
        }

        /// <summary>
        /// Draw a min and max combined slider in the inspector.
        /// </summary>
        /// <param name="minProp"></param>
        /// <param name="maxProp"></param>
        /// <param name="guiLabelContent"></param>
        /// <param name="defaultEditorLabelWidth"></param>
        public static void DrawMinMaxSlider(SerializedProperty minProp, SerializedProperty maxProp, GUIContent guiLabelContent, float minLimit, float maxLimit, float defaultEditorLabelWidth)
        {
            float minValue = minProp.floatValue;
            float maxValue = maxProp.floatValue;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiLabelContent, GUILayout.Width(defaultEditorLabelWidth));
            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(40f));
            DrawLabelIndent(2f);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            DrawLabelIndent(2f);
            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(40f));
            GUILayout.EndHorizontal();

            minProp.floatValue = minValue;
            maxProp.floatValue = maxValue;
        }

        /// <summary>
        /// Draw minimum and maximum sliders.
        /// </summary>
        /// <param name="minProp"></param>
        /// <param name="maxProp"></param>
        /// <param name="guiMinLabelContent"></param>
        /// <param name="guiMaxLabelContent"></param>
        public static void DrawMinMaxSliders(SerializedProperty minProp, SerializedProperty maxProp, GUIContent guiMinLabelContent, GUIContent guiMaxLabelContent)
        {
            EditorGUILayout.PropertyField(minProp, guiMinLabelContent);
            EditorGUILayout.PropertyField(maxProp, guiMaxLabelContent);

            if (maxProp.floatValue < minProp.floatValue) { minProp.floatValue = maxProp.floatValue; }
            //if (minProp.floatValue > maxProp.floatValue) { maxProp.floatValue = minProp.floatValue; }
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
        /// <param name="labelGUIContent"></param>
        /// <param name="labelWidth"></param>
        /// <param name="elementName"></param>
        public static void DrawArray(SerializedProperty serializedProperty, GUIContent labelGUIContent, float labelWidth, string elementName)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelGUIContent, GUILayout.Width(labelWidth - 1f));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            for (int arrayIdx = 0; arrayIdx < serializedProperty.arraySize; arrayIdx++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(elementName + " " + (arrayIdx + 1).ToString(), GUILayout.Width(labelWidth - 16f));
                EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(arrayIdx), GUIContent.none);
                GUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw a number array without ability to add or remove items
        /// </summary>
        /// <param name="serializedProperty"></param>
        public static void DrawArray(SerializedProperty serializedProperty)
        {
            EditorGUI.indentLevel++;

            for (int arrayIdx = 0; arrayIdx < serializedProperty.arraySize; arrayIdx++)
            {
                EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(arrayIdx), new GUIContent((arrayIdx+1).ToString()));
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw an array in the inspector with Add/Remove buttons.
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="isExpandedProperty"></param>
        /// <param name="labelGUIContent"></param>
        /// <param name="labelWidth"></param>
        /// <param name="elementName"></param>
        /// <param name="buttonCompact"></param>
        /// <param name="foldoutStyleNoLabel"></param>
        /// <param name="defaultEditorFieldWidth"></param>
        public static void DrawArray
        (
            SerializedProperty serializedProperty, SerializedProperty isExpandedProperty, GUIContent labelGUIContent, float labelWidth,
            string elementName, GUIStyle buttonCompact, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth
        )
        {
            GUILayout.BeginHorizontal();

            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isExpandedProperty.boolValue = EditorGUILayout.Foldout(isExpandedProperty.boolValue, "", foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(labelGUIContent.text + serializedProperty.arraySize.ToString(": 00"), labelGUIContent.tooltip));

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                serializedProperty.arraySize++;

                //SerializedProperty element = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (serializedProperty.arraySize > 0) { serializedProperty.arraySize--; }
            }

            GUILayout.EndHorizontal();

            if (isExpandedProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                for (int arrayIdx = 0; arrayIdx < serializedProperty.arraySize; arrayIdx++)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(elementName + " " + (arrayIdx + 1).ToString(), GUILayout.Width(labelWidth - 8f));
                    EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(arrayIdx), GUIContent.none);
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draw a list with add/delete options
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <param name="isExpandedProperty"></param>
        /// <param name="labelGUIContent"></param>
        /// <param name="labelWidth"></param>
        /// <param name="elementName"></param>
        /// <param name="buttonCompact"></param>
        /// <param name="foldoutStyleNoLabel"></param>
        /// <param name="defaultEditorFieldWidth"></param>
        public static void DrawList
        (
            SerializedProperty serializedProperty, SerializedProperty isExpandedProperty, GUIContent labelGUIContent, float labelWidth,
            string elementName, GUIStyle buttonCompact, GUIStyle foldoutStyleNoLabel, float defaultEditorFieldWidth
        )
        {
            GUILayout.BeginHorizontal();

            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            isExpandedProperty.boolValue = EditorGUILayout.Foldout(isExpandedProperty.boolValue, "", foldoutStyleNoLabel);
            EditorGUI.indentLevel -= 1;

            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

            // Create a revised GUIContent and don't include a width so that the Buttons right align
            EditorGUILayout.LabelField(new GUIContent(labelGUIContent.text + serializedProperty.arraySize.ToString(": 00"), labelGUIContent.tooltip));

            if (GUILayout.Button("+", GUILayout.MaxWidth(20f)))
            {
                serializedProperty.arraySize++;
            }

            if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
            {
                if (serializedProperty.arraySize > 0) { serializedProperty.arraySize--; }
            }

            GUILayout.EndHorizontal();

            int deleteItem = -1;

            if (isExpandedProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawHorizontalGap(2f);
                for (int arrayIdx = 0; arrayIdx < serializedProperty.arraySize; arrayIdx++)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField((string.IsNullOrEmpty(elementName) ? "" : elementName + " ") + (arrayIdx + 1).ToString("00"), GUILayout.Width(labelWidth - 8f));
                    EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(arrayIdx), GUIContent.none);
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) { deleteItem = arrayIdx; }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                if (deleteItem >= 0)
                {
                    serializedProperty.DeleteArrayElementAtIndex(deleteItem);
                }
            }
        }

        /// <summary>
        /// Draw a toolbar or tabs in the inspector
        /// </summary>
        /// <param name="selectedTabIntProp"></param>
        /// <param name="tabTexts"></param>
        public static void DrawS3DToolbar(SerializedProperty selectedTabIntProp, GUIContent[] tabTexts)
        {
            int prevTab = selectedTabIntProp.intValue;

            // Show a toolbar to allow the user to switch between viewing different areas
            selectedTabIntProp.intValue = GUILayout.Toolbar(selectedTabIntProp.intValue, tabTexts);

            // When switching tabs, disable focus on previous control
            if (prevTab != selectedTabIntProp.intValue) { GUI.FocusControl(null); }
        }

        /// <summary>
        /// Draw the S3D version number in a box in the inspector
        /// </summary>
        /// <param name="labelFieldRichText"></param>
        public static void DrawStickyVersionLabel (GUIStyle labelFieldRichText)
        {
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sticky3D Controller</b> Version " + StickyControlModule.S3DVersion + " " + StickyControlModule.S3DBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Returns a float as text for debugging with 0,1,2 or 3 decimal places
        /// </summary>
        /// <param name="f"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static string GetFloatText(float f, int decimalPlaces)
        {
            float multiplier = decimalPlaces == 0 ? 10 : decimalPlaces == 1 ? 100f : decimalPlaces == 2 ? 1000f : 10000f;
            string sFormat = decimalPlaces == 0 ? "0" : decimalPlaces == 1 ? "0.0" : decimalPlaces == 2 ? "0.00" : "0.000";

            return (Mathf.RoundToInt(f * multiplier) / multiplier).ToString(sFormat);
        }

        /// <summary>
        /// Get a Gradient from a SerializedProperty of a Gradient
        /// </summary>
        /// <param name="gradientProperty"></param>
        /// <returns></returns>
        public static Gradient GetGradient(SerializedProperty gradientProperty)
        {
            System.Reflection.PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (propertyInfo == null) { return null; }
            else { return propertyInfo.GetValue(gradientProperty, null) as Gradient; }
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

        #region Inspector Styles


        public static GUIStyle GetCompactBtn (int btnFontSize = 10)
        {
            return new GUIStyle("Button")
            {
                fontSize = btnFontSize
            };
        }

        /// <summary>
        /// A compact toggle button in its non-toggled state.
        /// BtnSize options are currently 10 or 12
        /// </summary>
        /// <returns></returns>
        public static GUIStyle GetToggleCompactBtnStyleNormal(int btnSize = 10)
        {
            return new GUIStyle("Button")
            {
                fontSize = btnSize == 12 ? 12 : 10,
                fontStyle = FontStyle.Normal
            };
        }

        /// <summary>
        /// A toggled compact button style.
        /// BtnSize options are currently 10 or 12.
        /// </summary>
        /// <returns></returns>
        public static GUIStyle GetToggleCompactBtnStyleToggled(int btnSize = 10)
        {
            GUIStyle toggled = GetToggleCompactBtnStyleNormal(btnSize);
            toggled.fontStyle = FontStyle.Bold;
            toggled.normal.background = toggled.active.background;

            return toggled;
        }

        /// <summary>
        /// A toggled compact button style that has coloured text.
        /// BtnSize options are currently 10 or 12.
        /// Tested with dark and light editor skins.
        /// </summary>
        /// <returns></returns>
        public static GUIStyle GetToggleCompactBtnStyleToggledB(int btnSize = 10)
        {
            GUIStyle toggled = GetToggleCompactBtnStyleNormal(btnSize);
            toggled.normal.background = toggled.active.background;
            // Light sky blue for dark skin (on grey background).
            toggled.normal.textColor = EditorGUIUtility.isProSkin ? new Color(135f/255f, 206f/255f,250f/255f, 1f) : Color.blue;

            return toggled;
        }

        #endregion

        #region Dialog Boxes

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

        #endregion

        #region Material Methods

        /// <summary>
        /// Attempt to get a default built-in material
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public static Material GetDefaultMaterial(string materialName)
        {
            return AssetDatabase.GetBuiltinExtraResource<Material>(materialName);
        }

        #endregion

        #region Project Helper Methods

        /// <summary>
        /// Reveal or hightlight the folder that in the Project window (if it is open and visible
        /// on the panel it is on). Highlights the item in yellow for a second or two.
        /// By default also selects the object. Not selecting the object can be useful if called
        /// from a CustomEditor inspector script so as not to loose focus.
        /// NOTE: The Project won't expand if it is already selected AND the user has collapsed it.
        /// USAGE: StickyEditorHelper.HighlightFolderInProjectWindow(StickySetup.s3dFolder, true, true);
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
            else { Debug.Log("StickyEditorHelper.HighLightFolder - the following folder does not exist: " + folderPath); }
        }

        #endregion

        #region IO Helper Methods

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

        #endregion

        #region Menu Helper Methods

        /// <summary>
        /// Call an item from the Unity menu. Menu can also be one custom created.
        /// USAGE: StickyEditorHelper.CallMenu("Edit/Project Settings/Player");
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

        #endregion
    }
}