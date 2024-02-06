using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Sci-Fi Ship Controller. Copyright (c) 2018-2022 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [CustomEditor(typeof(SSCRadar))]
    public class SSCRadarEditor : Editor
    {
        #region Custom Editor private variables
        private SSCRadar sscRadar;
        private readonly static string emptyString = "";

        // Formatting and style variables
        private string txtColourName = "Black";
        private Color defaultTextColour = Color.black;
        private string labelText;
        private GUIStyle labelFieldRichText;
        private GUIStyle headingFieldRichText;
        private GUIStyle helpBoxRichText;
        private GUIStyle buttonCompact;
        private GUIStyle buttonCompactBoldBlue;
        private GUIStyle foldoutStyleNoLabel;
        private static GUIStyle toggleCompactButtonStyleNormal = null;  // Small Toggle button. e.g. G(izmo) on/off
        private static GUIStyle toggleCompactButtonStyleToggled = null;
        private float defaultEditorLabelWidth = 0f;
        private float defaultEditorFieldWidth = 0f;
        private bool isDebuggingEnabled = false;
        private bool isRefreshUINextFrame = false;

        private string sscHelpPDF;
        #endregion

        #region Custom Editor Sceneview variables

        #endregion

        #region GUIContent - General
        private readonly static GUIContent generalShowInEditorContent = new GUIContent("General", "Expand general options");
        private readonly static GUIContent headerContent = new GUIContent("Radar is automatically added to the scene if it doesn't already exist at runtime." +
                        " This futuristic Automatic Dependent Surveillance system centrally manages all radar communications.");

        private readonly static GUIContent initialiseOnStartContent = new GUIContent("Initialise on Start", "If enabled, the GetOrCreateRadar() will be called as soon as Start() runs. " +
                  "If there is a UI (mini-map) configured, it will automatically be made visible. This should be disabled if you are instantiating the SSCRadar through code and using the SSCRadar API methods.");
        private readonly static GUIContent poolInitialSizeContent = new GUIContent("Initial Pool Size", "The number of items that you expect to be tracked by the radar system at any point in time.");
        private readonly static GUIContent displayRangeContent = new GUIContent("Range (metres)", "The range of the radar from the centre to the edges. Can be overridden at runtime using API methods.");
        private readonly static GUIContent is3DQueryEnabledContent = new GUIContent("3D Query", "Uses 3D distances to determine range when querying the radar data.");
        private readonly static GUIContent isQuerySortOrderContent = new GUIContent("Query Sort Order", "The order in which query results are returned. Use None where possible as it the fastest and has the lowest impact on performance.");
        #endregion

        #region GUIContent - Visuals
        private readonly static GUIContent visualsShowInEditorContent = new GUIContent("Visuals", "Expand visual options");
        private readonly static GUIContent visualsHeaderContent = new GUIContent("Optionally use our on-screen visuals or use the SSCRadar API to populate your own UI.");
        private readonly static GUIContent visualsScreenLocaleContent = new GUIContent("Screen Locale", "Position where radar will be displayed on the screen.");
        private readonly static GUIContent visualsScreenLocaleCustomContent = new GUIContent("Custom Locale", "X,Y coordinates where radar will be displayed on the screen.");
        private readonly static GUIContent visualsDisplayWidthContent = new GUIContent("Display Width", "The radar display width as a proportion of the screen width.");
        private readonly static GUIContent visualsCanvasSortOrderContent = new GUIContent("Canvas Sort Order", "The sort order of the canvas in the scene. Higher numbers are on top.");
        private readonly static GUIContent overlayColourContent = new GUIContent("Overlay Colour", "Colour of the overlay decals on the radar display");
        private readonly static GUIContent backgroundColourContent = new GUIContent("Background Colour", "Primary background colour on the radar display");
        private readonly static GUIContent blipFriendColourContent = new GUIContent("Blip Friend Colour", "When the built-in UI is used, this is the colour of any blip that are considered as friendly. Determined by the factionId when available");
        private readonly static GUIContent blipFoeColourContent = new GUIContent("Blip Foe Colour", "When the built-in UI is used, this is the colour of any blip that are considered as hostile. Determined by the factionId when available");
        private readonly static GUIContent blipNeutralColourContent = new GUIContent("Blip Neutral Colour", "When the built-in UI is used, this is the colour of any blip that are considered as neutral. Determined by the factionId when available. Faction Id = 0");
        private readonly static GUIContent minimapContent = new GUIContent("Mini-map UI Image", "A reference in the scene to the UI RawImage to be used to display the radar.");
        #endregion

        #region GUIContent - Movement
        private readonly static GUIContent movementShowInEditorContent = new GUIContent("Movement", "Expand movement options");
        private readonly static GUIContent movementHeaderContent = new GUIContent("When the built-in on-screen visuals (UI) are in use, the radar can be configured to <i>move</i> around with an object or ship or remain at a fixed position.");
        private readonly static GUIContent shipToFollowContent = new GUIContent("Ship to Follow", "The centre of the radar will move with this ship.");
        private readonly static GUIContent gameobjectToFollowContent = new GUIContent("GameObject to Follow", "The centre of the radar will move with this gameobject.");
        private readonly static GUIContent centrePositionContent = new GUIContent("Centre Position", "The centre of the radar.");
        #endregion

        #region GUIContent - Debug
        private readonly static GUIContent debugModeContent = new GUIContent("Debug Mode", "Use this to display radar data at runtime in the editor.");
        private readonly static GUIContent debugIsInitialisedContent = new GUIContent("Is Initialised?");
        private readonly static GUIContent debugResultQtyContent = new GUIContent("Result Count", "The number of items currently being tracked");
        private readonly static GUIContent debugDisplayFwdsContent = new GUIContent("Display Forwards", "The direction the in-built UI display is facing (if there is one)");
        #endregion

        #region Serialized Properties
        private SerializedProperty generalShowInEditorProp;
        private SerializedProperty visualsShowInEditorProp;
        private SerializedProperty movementShowInEditorProp;
        private SerializedProperty screenLocaleProp;
        private SerializedProperty screenLocaleCustomXYProp;
        private SerializedProperty displayWidthNProp;
        private SerializedProperty overlayColourProp;
        private SerializedProperty backgroundColourProp;
        private SerializedProperty radarImageProp;
        private SerializedProperty shipToFollowProp;
        private SerializedProperty gameobjectToFollowProp;
        private SerializedProperty centrePositionProp;
        private SerializedProperty displayRangeProp;
        private SerializedProperty canvasSortOrderProp;
        #endregion

        #region Events

        private void OnEnable()
        {
            sscRadar = (SSCRadar)target;

            //Used in Richtext labels
            if (EditorGUIUtility.isProSkin) { txtColourName = "White"; defaultTextColour = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f); }

            //Keep compiler happy - can remove this later if it isn't required
            if (defaultTextColour.a > 0f) { }
            if (string.IsNullOrEmpty(txtColourName)) { }

            // Reset guistyles to avoid issues - forces reinitialisation of button styles etc
            helpBoxRichText = null;
            labelFieldRichText = null;
            headingFieldRichText = null;
            buttonCompact = null;
            buttonCompactBoldBlue = null;
            foldoutStyleNoLabel = null;
            toggleCompactButtonStyleNormal = null;
            toggleCompactButtonStyleToggled = null;

            defaultEditorLabelWidth = 150f; // EditorGUIUtility.labelWidth;
            defaultEditorFieldWidth = EditorGUIUtility.fieldWidth;

            #region FindProperties
            generalShowInEditorProp = serializedObject.FindProperty("generalShowInEditor");
            visualsShowInEditorProp = serializedObject.FindProperty("visualsShowInEditor");
            movementShowInEditorProp = serializedObject.FindProperty("movementShowInEditor");
            screenLocaleProp = serializedObject.FindProperty("screenLocale");
            screenLocaleCustomXYProp = serializedObject.FindProperty("screenLocaleCustomXY");
            displayWidthNProp = serializedObject.FindProperty("radarDisplayWidthN");
            canvasSortOrderProp = serializedObject.FindProperty("canvasSortOrder");
            overlayColourProp = serializedObject.FindProperty("overlayColour");
            backgroundColourProp = serializedObject.FindProperty("backgroundColour");
            radarImageProp = serializedObject.FindProperty("radarImage");
            shipToFollowProp = serializedObject.FindProperty("shipToFollow");
            gameobjectToFollowProp = serializedObject.FindProperty("gameobjectToFollow");
            centrePositionProp = serializedObject.FindProperty("centrePosition");
            displayRangeProp = serializedObject.FindProperty("displayRange");

            #endregion

            sscHelpPDF = SSCEditorHelper.GetHelpURL();
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
            if (sscRadar != null && sscRadar.allowRepaint) { Repaint(); }
        }

        #endregion

        #region OnInspectorGUI

        public override void OnInspectorGUI()
        {
            // TEST - Show all fields
            //base.DrawDefaultInspector();

            #region Initialise
            EditorGUIUtility.labelWidth = defaultEditorLabelWidth;
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;

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

            if (headingFieldRichText == null)
            {
                headingFieldRichText = new GUIStyle(UnityEditor.EditorStyles.label);
                headingFieldRichText.richText = true;
                headingFieldRichText.normal.textColor = helpBoxRichText.normal.textColor;
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
                toggleCompactButtonStyleToggled = new GUIStyle(toggleCompactButtonStyleNormal);
                toggleCompactButtonStyleNormal.fontStyle = FontStyle.Normal;
                toggleCompactButtonStyleToggled.fontStyle = FontStyle.Bold;
                toggleCompactButtonStyleToggled.normal.background = toggleCompactButtonStyleToggled.active.background;
            }
            #endregion

            #region Header
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("<b>Sci-Fi Ship Controller</b> Version " + ShipControlModule.SSCVersion + " " + ShipControlModule.SSCBetaVersion, labelFieldRichText);
            GUILayout.EndVertical();
            EditorGUILayout.LabelField(headerContent, helpBoxRichText);
            #endregion

            #region Help Toolbar
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SSCEditorHelper.btnTxtGetSupport, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlGetSupport); }
            if (GUILayout.Button(SSCEditorHelper.btnDiscordContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlDiscordChannel); }
            if (GUILayout.Button(SSCEditorHelper.btnHelpContent, buttonCompact)) { Application.OpenURL(sscHelpPDF); }
            if (GUILayout.Button(SSCEditorHelper.tutorialsURLContent, buttonCompact)) { Application.OpenURL(SSCEditorHelper.urlTutorials); }
            EditorGUILayout.EndHorizontal();
            #endregion

            // Read in all the properties
            serializedObject.Update();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            #region General Settings
            DrawFoldoutWithLabel(generalShowInEditorProp, generalShowInEditorContent);
            if (generalShowInEditorProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialiseOnStart"), initialiseOnStartContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("poolInitialSize"), poolInitialSizeContent);

                // minimum range is 10 metres
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(displayRangeProp, displayRangeContent);
                if (EditorGUI.EndChangeCheck() && displayRangeProp.floatValue < 10f)
                {
                    displayRangeProp.floatValue = 10f;
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("is3DQueryEnabled"), is3DQueryEnabledContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("querySortOrder"), isQuerySortOrderContent);
            }
            #endregion

            #region Visual Settings
            DrawFoldoutWithLabel(visualsShowInEditorProp, visualsShowInEditorContent);
            if (visualsShowInEditorProp.boolValue)
            {
                EditorGUILayout.LabelField(visualsHeaderContent, helpBoxRichText);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("On screen radar", GUILayout.Width(defaultEditorLabelWidth-4f));

                if (radarImageProp.objectReferenceValue != null)
                {
                    if (GUILayout.Button("Remove", GUILayout.MaxWidth(75f)))
                    {
                        if (SSCEditorHelper.PromptForDelete("Delete the Mini-Map?", "Do you want to DELETE the Mini-Map and ALL child objects from the scene"))
                        {
                            // Note: undo doesn't rollback setting the reference to null...
                            Undo.DestroyObjectImmediate(((UnityEngine.UI.RawImage)radarImageProp.objectReferenceValue).gameObject);
                            radarImageProp.objectReferenceValue = null;
                        }
                    }
                    if (GUILayout.Button("Refresh", GUILayout.MaxWidth(75f)) || isRefreshUINextFrame)
                    {
                        isRefreshUINextFrame = false;
                        OnScreenRadar("MinimapREFRESH", screenLocaleProp.intValue, screenLocaleCustomXYProp.vector2Value, displayWidthNProp.floatValue,
                                        overlayColourProp.colorValue, backgroundColourProp.colorValue, radarImageProp);
                    }
                }
                else
                {
                    if (GUILayout.Button("Mini-map", GUILayout.MaxWidth(75f)))
                    {
                        OnScreenRadar("MinimapADD", screenLocaleProp.intValue, screenLocaleCustomXYProp.vector2Value, displayWidthNProp.floatValue,
                                         overlayColourProp.colorValue, backgroundColourProp.colorValue, radarImageProp);
                        
                        // Older versions of Unity don't get the correct canvas when they are first created.
                        // So, run the MinimapREFRESH method in the next frame.
                        #if !UNITY_2019_3_OR_NEWER
                        isRefreshUINextFrame = true;
                        #endif
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayoutUtility.GetRect(1f, 4f);

                EditorGUILayout.PropertyField(screenLocaleProp, visualsScreenLocaleContent);
                if (screenLocaleProp.intValue == (int)SSCRadar.RadarScreenLocale.Custom)
                {
                    EditorGUILayout.PropertyField(screenLocaleCustomXYProp, visualsScreenLocaleCustomContent);
                }

                EditorGUILayout.PropertyField(displayWidthNProp, visualsDisplayWidthContent);
                EditorGUILayout.PropertyField(canvasSortOrderProp, visualsCanvasSortOrderContent);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(overlayColourProp, overlayColourContent);
                EditorGUILayout.PropertyField(backgroundColourProp, backgroundColourContent);
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying)
                {
                    sscRadar.RefreshResults();
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blipFriendColour"), blipFriendColourContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blipFoeColour"), blipFoeColourContent);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blipNeutralColour"), blipNeutralColourContent);

                EditorGUILayout.PropertyField(radarImageProp, minimapContent);
            }
            #endregion

            #region Movement Settings
            DrawFoldoutWithLabel(movementShowInEditorProp, movementShowInEditorContent);
            if (movementShowInEditorProp.boolValue)
            {
                EditorGUILayout.LabelField(movementHeaderContent, helpBoxRichText);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(shipToFollowProp, shipToFollowContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (shipToFollowProp.objectReferenceValue != null)
                    {
                        gameobjectToFollowProp.objectReferenceValue = null;
                    }

                    if (EditorApplication.isPlaying)
                    {
                        sscRadar.FollowShip((ShipControlModule)shipToFollowProp.objectReferenceValue);
                    }
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(gameobjectToFollowProp, gameobjectToFollowContent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (gameobjectToFollowProp.objectReferenceValue != null)
                    {
                        shipToFollowProp.objectReferenceValue = null;
                    }

                    if (EditorApplication.isPlaying)
                    {
                        sscRadar.FollowGameObject((GameObject)gameobjectToFollowProp.objectReferenceValue);
                    }
                }
                EditorGUILayout.PropertyField(centrePositionProp, centrePositionContent);
            }
            #endregion

            GUILayout.EndVertical();
            // Apply property changes
            serializedObject.ApplyModifiedProperties();

            #region Debug Mode
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isDebuggingEnabled = EditorGUILayout.Toggle(debugModeContent, isDebuggingEnabled);
            if (isDebuggingEnabled && sscRadar != null && sscRadar.IsInitialised)
            {
                float rightLabelWidth = 150f;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugIsInitialisedContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(sscRadar.IsInitialised ? "Yes" : "No", GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugResultQtyContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(sscRadar.ResultCount.ToString(), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(debugDisplayFwdsContent, labelFieldRichText, GUILayout.Width(defaultEditorLabelWidth - 3f));
                EditorGUILayout.LabelField(SSCEditorHelper.GetVector3Text(sscRadar.DisplayRotation.eulerAngles,3), GUILayout.MaxWidth(rightLabelWidth));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            #endregion
        }

        #endregion

        #region Private Draw methods - General

        private void DrawFoldoutWithLabel(SerializedProperty showInEditorProp, GUIContent headerLabel)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel += 1;
            EditorGUIUtility.fieldWidth = 15f;
            showInEditorProp.boolValue = EditorGUILayout.Foldout(showInEditorProp.boolValue, emptyString, foldoutStyleNoLabel);
            EditorGUIUtility.fieldWidth = defaultEditorFieldWidth;
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField(headerLabel, headingFieldRichText);
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Private Member Methods

        /// <summary>
        /// Add the on-screen radar to the canvas. Create one if it doesn't exist.
        /// TODO - refresh with size and position
        /// </summary>
        /// <param name="command"></param>
        /// <param name="screenLocaleInt"></param>
        /// <param name="screenLocaleCustomXY"></param>
        /// <param name="panelWidthN"></param>
        /// <param name="overlayColour"></param>
        /// <param name="serializedProperty"></param>
        private void OnScreenRadar
        (
         string command,
         int screenLocaleInt,
         Vector2 screenLocaleCustomXY,
         float panelWidthN,
         Color overlayColour,
         Color backgroundColour,
         SerializedProperty serializedProperty
        )
        {
            GameObject radarCanvasGO;
            Canvas radarCanvas;
            Vector2 canvasSize;
            Vector3 canvasScale;

            sscRadar.GetorCreateRadarCanvas(out radarCanvasGO, out radarCanvas, out canvasSize, out canvasScale);

            if (radarCanvas != null && radarCanvasGO != null)
            {
                if (command.StartsWith("Minimap"))
                {
                    Vector2 anchorMin = Vector2.zero;
                    Vector2 anchorMax = Vector2.zero;
                    Vector2 panelOffset = Vector2.zero;

                    float panelWidth = Mathf.Ceil(panelWidthN * canvasSize.x), panelHeight = panelWidth;

                    sscRadar.GetMinimapScreenLocation(screenLocaleInt, canvasSize, new Vector2(panelWidth, panelHeight), ref anchorMin, ref anchorMax, ref panelOffset);

                    int texWidth = Mathf.CeilToInt(panelWidth);
                    int texHeight = Mathf.CeilToInt(panelHeight);

                    UnityEngine.UI.RawImage panelImg = null;
                    Texture2D radarTex = null;

                    if (command.Contains("ADD"))
                    {
                        radarTex = SSCUtils.CreateTexture(texWidth, texHeight, Color.clear, false);

                        UnityEngine.UI.RawImage[] uiImages = radarCanvasGO.GetComponentsInChildren<UnityEngine.UI.RawImage>();

                        panelImg = SSCEditorHelper.AddCanvasRawPanelIfMissing(uiImages, "Minimap", panelOffset.x, panelOffset.y, panelWidth, panelHeight,
                                                                              anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y, radarTex, radarCanvasGO.transform, canvasScale);
                    }
                    else if (command.Contains("REFRESH") && serializedProperty.objectReferenceValue != null)
                    {
                        panelImg = (UnityEngine.UI.RawImage)serializedProperty.objectReferenceValue;
                        radarTex = (Texture2D)panelImg.texture;

                        if (radarTex.width != texWidth || radarTex.height != texHeight)
                        {
                            #if !UNITY_2021_2_OR_NEWER
                            radarTex.Resize(texWidth, texHeight);
                            #else
                            radarTex.Reinitialize(texWidth, texHeight);
                            #endif
                        }

                        SSCUtils.UpdateCanvasPanel(panelImg.rectTransform, panelOffset.x, panelOffset.y, panelWidth, panelHeight,
                                                          anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y, canvasScale);
                        SSCUtils.FillTexture(radarTex, Color.clear, false);
                    }

                    if (panelImg != null)
                    {
                        int outerRadius = (int)(texWidth / 2f);

                        sscRadar.DrawCircle(radarTex, (int)(texWidth / 2f), (int)(texHeight / 2f), outerRadius, overlayColour, true, false);
                        sscRadar.DrawCircle(radarTex, (int)(texWidth / 2f), (int)(texHeight / 2f), outerRadius - 4, backgroundColour, true, false);

                        // Only apply once all operations have finished
                        radarTex.Apply();
                        panelImg.texture = radarTex;

                        serializedProperty.objectReferenceValue = panelImg;
                    }

                    Selection.activeGameObject = sscRadar.gameObject;
                }
            }
        }

        #endregion

        #region Public Static Methods
        // Add a menu item so that a SSCRadar can be created via the GameObject > 3D Object menu
        [MenuItem("GameObject/3D Object/Sci-Fi Ship Controller/SSC Radar")]
        public static void CreateSSCRadar()
        {
            SSCRadar r = SSCRadar.GetOrCreateRadar();
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(r.gameObject.scene);
                EditorUtility.SetDirty(r);
            }
        }
        #endregion
    }
}