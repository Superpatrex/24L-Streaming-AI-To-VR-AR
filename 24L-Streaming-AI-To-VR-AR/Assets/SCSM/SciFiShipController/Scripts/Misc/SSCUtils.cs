using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing common utility methods
    /// </summary>
    public class SSCUtils
    {
        #region Readonly Strings

        // Statis strings use more memory but can held to reduce GC
        private static readonly string PrptyNameHDIntensity = "intensity";

        #endregion

        #region Animation

        /// <summary>
        /// Return the state Id (hash) of a state in an Animation Controller.
        /// NOTE: This does not test if the state exists in any of the layers,
        /// and simply returns a hash of the name.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public static int GetAnimationStateId (string stateName)
        {
            return Animator.StringToHash(stateName);
        }

        /// <summary>
        /// Check if the Animator is valid and ready for use.
        /// Avoids the (reoccurring) "Animator is not playing the AnimatorController" warning,
        /// and ensures animator parameters can be fetched correctly.
        /// This is the same as a method of the same name in Sticky3D Controller's S3DAnimParm struct.
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static bool IsAnimatorReady(Animator animator)
        {
            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                // NOTE: This will still raise 1 warning if the animator is not ready.
                if (animator.GetCurrentAnimatorStateInfo(0).length == 0)
                {
                    // Fix the condition that causes reoccurring "Animator is not playing the AnimatorController"
                    animator.Rebind();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Animation Curves

        /// <summary>
        /// Create an inverse AnimationCurve. This is useful if you want to get the time
        /// at a particular value.
        /// </summary>
        /// <param name="sourceAnimCurve"></param>
        /// <returns></returns>
        public static AnimationCurve InverseAnimCurve(AnimationCurve sourceAnimCurve)
        {
            AnimationCurve invAnimCurve = null;

            if (sourceAnimCurve != null)
            {
                invAnimCurve = new AnimationCurve();

                for (int k = 0; k < sourceAnimCurve.length; k++)
                {
                    // Invert slopes of in/out tangents
                    float newInTangent = (Mathf.Abs(sourceAnimCurve.keys[k].inTangent) > Mathf.Epsilon ?
                        (1f / sourceAnimCurve.keys[k].inTangent) : 1000f);
                    float newOutTangent = (Mathf.Abs(sourceAnimCurve.keys[k].outTangent) > Mathf.Epsilon ?
                        (1f / sourceAnimCurve.keys[k].outTangent) : 1000f);
                    // Create new inverted keyframe
                    Keyframe keyframe = new Keyframe(sourceAnimCurve.keys[k].value, sourceAnimCurve.keys[k].time,
                        newInTangent, newOutTangent, sourceAnimCurve.keys[k].inWeight, sourceAnimCurve.keys[k].outWeight);
                    // Add the inverted keyframe to the curve
                    invAnimCurve.AddKey(keyframe);
                }
            }

            return invAnimCurve;
        }

        #endregion

        #region Enumerations
        public enum ViewDirection : int
        {
            Unknown = 0,
            InFront = 1,
            Behind = 2,
            OffLeftEdge = 3,
            OffRightEdge = 4,
            OffLowerEdge = 5,
            OffUpperEdge = 6
        }
        #endregion

        #region Camera Methods

        /// <summary>
        /// Given a 3D point in worldscape, is it [potentially] in view of the supplied camera?
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldSpacePosition"></param>
        /// <returns></returns>
        public static bool IsPointInCameraView(Camera camera, Vector3 worldSpacePosition)
        {
            if (camera != null)
            {
                Vector3 screenPosition = camera.WorldToScreenPoint(worldSpacePosition);

                bool isBehindPlayer = screenPosition.z < 0.0f;
                bool offLeftEdge = screenPosition.x < 0.0f;
                bool offRightEdge = screenPosition.x > Screen.width;
                bool offLowerEdge = screenPosition.y < 0.0f;
                bool offUpperEdge = screenPosition.y > Screen.height;

                return (!(isBehindPlayer || offLeftEdge || offRightEdge || offLowerEdge || offUpperEdge));
            }
            else { return false; }
        }

        /// <summary>
        /// Is the 3D world-space point with a offset viewport inside the camera's full screen area?
        /// Any points of the viewport that are outside the screen area will also return false.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldSpacePosition"></param>
        /// <param name="screenSize">This is usually the screen resolution</param>
        /// <param name="viewportSize">The width and height as 0-1 values of the full viewSize</param>
        /// <param name="viewportOffset">-1.0 to 1.0 with 0,0 as the centre of the screen</param>
        /// <returns></returns>
        public static bool IsPointInScreenViewport(Camera camera, Vector3 worldSpacePosition, Vector2 screenSize, Vector2 viewportSize, Vector2 viewportOffset)
        {
            if (camera != null)
            {
                Vector3 screenPosition = camera.WorldToScreenPoint(worldSpacePosition);

                // Check if behind or outside visble screen area
                if (screenPosition.z < 0.0f || screenPosition.x < 0.0f || screenPosition.x > screenSize.x ||
                    screenPosition.y < 0.0f || screenPosition.y > screenSize.y) { return false; }
                else
                {
                    // Infront of camera

                    // Check 2D viewport x-axis
                    float vpWidth = screenSize.x * viewportSize.x;
                    float vpLeft = screenSize.x * 0.5f - (vpWidth * 0.5f) + (viewportOffset.x * screenSize.x);
                    float vpRight = vpLeft + vpWidth;

                    if (screenPosition.x < vpLeft || screenPosition.x > vpRight) { return false; }
                    else
                    {
                        // Check 2D viewport y-axis
                        float vpHeight = screenSize.y * viewportSize.y;
                        float vpBottom = screenSize.y * 0.5f - (vpHeight * 0.5f) + (viewportOffset.y * screenSize.y);
                        float vpTop = vpBottom + vpHeight;
                        if (screenPosition.y < vpBottom || screenPosition.y > vpTop) { return false; }
                        else { return true; }
                    }
                }
            }
            else { return false; }
        }

        /// <summary>
        /// Get the location or "view direction" of a 3D world-space point relative to the camera
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldSpacePosition"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns></returns>
        public static ViewDirection PointViewDirection(Camera camera, Vector3 worldSpacePosition, Vector2 screenSize)
        {
            if (camera != null)
            {
                Vector3 screenPosition = camera.WorldToScreenPoint(worldSpacePosition);

                if (screenPosition.z < 0.0f) { return ViewDirection.Behind; }
                else if (screenPosition.x < 0.0f) { return ViewDirection.OffLeftEdge; }
                else if (screenPosition.x > screenSize.x) { return ViewDirection.OffRightEdge; }
                else if (screenPosition.y < 0.0f) { return ViewDirection.OffLowerEdge; }
                else if (screenPosition.y > screenSize.y) { return ViewDirection.OffUpperEdge; }
                else { return ViewDirection.InFront; }
            }
            else { return ViewDirection.Unknown; }
        }

        #endregion

        #region Raycast Methods

        /// <summary>
        /// Update the minDistance and normal of closest object with a collider.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="direction"></param>
        /// <param name="lookDistance"></param>
        /// <param name="minDistance"></param>
        /// <param name="objectNormal"></param>
        public static void GetClosestCollider(Ray ray, Vector3 direction, float lookDistance, ref float minDistance, ref Vector3 objectNormal, out RaycastHit raycastHit)
        {
            ray.direction = direction;
            if (Physics.Raycast(ray, out raycastHit, lookDistance)) { if (raycastHit.distance < minDistance) { minDistance = raycastHit.distance; objectNormal = raycastHit.normal; } }
        }

        #endregion

        #region UI and Canvas Methods

        /// <summary>
        /// Find a canvas by name in the scene
        /// </summary>
        /// <param name="canvasName"></param>
        /// <returns></returns>
        public static Canvas FindCanvas(string canvasName)
        {
            Canvas canvas = null;

            if (!string.IsNullOrEmpty(canvasName))
            {
                #if UNITY_2022_2_OR_NEWER
                List<Canvas> canvasList = new List<Canvas>(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None));
                #else
                List<Canvas> canvasList = new List<Canvas>(UnityEngine.Object.FindObjectsOfType<Canvas>());
                #endif

                if (canvasList != null) { canvas = canvasList.Find(cv => cv.name == canvasName); }
            }

            return canvas;
        }

        /// <summary>
        /// Update a UI panel which is a child of a Canvas.
        /// Currently used by Radar
        /// </summary>
        /// <param name="rectTrfm"></param>
        /// <param name="panelOffsetX"></param>
        /// <param name="panelOffsetY"></param>
        /// <param name="panelWidth"></param>
        /// <param name="panelHeight"></param>
        /// <param name="anchorMinX"></param>
        /// <param name="anchorMinY"></param>
        /// <param name="anchorMaxX"></param>
        /// <param name="anchorMaxY"></param>
        /// <param name="canvasScale"></param>
        public static void UpdateCanvasPanel
        (
            RectTransform rectTrfm,
            float panelOffsetX, float panelOffsetY,
            float panelWidth, float panelHeight,
            float anchorMinX, float anchorMinY,
            float anchorMaxX, float anchorMaxY,
            Vector3 canvasScale
        )
        {
            if (rectTrfm != null)
            {
                rectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                rectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

                rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
                rectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

                rectTrfm.position = new Vector3(((panelOffsetX * canvasScale.x) + (panelWidth * 0.5f * canvasScale.x)), ((panelOffsetY * canvasScale.y) + (panelHeight * 0.5f * canvasScale.y)), 0f);

                //Debug.Log("[DEBUG] panelWidth " + panelWidth + " panelOffset: " + panelOffsetX + "," + panelOffsetY + " scalex: " + canvasScale.x + " rectTrm: " + rectTrfm.position);
            }
        }

        /// <summary>
        /// Set the maximum screen size to HD (1920x1080)
        /// Keep the existing aspect ratio.
        /// </summary>
        public static void MaxScreenHD()
        {
            // Check the current resolution
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (screenWidth > 1920)
            {
                //Reduce to HD but keep the same aspect ratio
                Screen.SetResolution(1920, Mathf.FloorToInt(1920f * (screenHeight / screenWidth)), true);
            }
        }

        /// <summary>
        /// Get a child RectTransform or UI panel of a parent given the name of the child.
        /// See also GetChildTransform(...)
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static RectTransform GetChildRectTransform(Transform parentTrfm, string childName, string callerName)
        {
            RectTransform rectTransform = null;
            Transform childTrm = parentTrfm == null || string.IsNullOrEmpty(childName) ? null : parentTrfm.Find(childName);

            if (childTrm != null)
            {
                rectTransform = childTrm.GetComponent<RectTransform>();
            }

#if UNITY_EDITOR
            if (rectTransform == null)
            {
                Debug.LogWarning(string.Format("ERROR: {0} could not find RectTransform child {1} under {2}", callerName, string.IsNullOrEmpty(childName) ? "[unknown]" : childName, parentTrfm == null ? " no parent" : parentTrfm.name));
            }
#endif

            return rectTransform;
        }

        /// <summary>
        /// Get or Create a RectTransform as a child of a given Transform.
        /// panelOffsetX, panelOffsetY range is -1.0 to 1.0.
        /// panelWidth and panelHeight range is 0.0 to 1.0.
        /// </summary>
        /// <param name="parentRectTrfm"></param>
        /// <param name="parentSize"></param>
        /// <param name="childName"></param>
        /// <param name="panelOffsetX"></param>
        /// <param name="panelOffsetY"></param>
        /// <param name="panelWidth"></param>
        /// <param name="panelHeight"></param>
        /// <param name="anchorMinX"></param>
        /// <param name="anchorMinY"></param>
        /// <param name="anchorMaxX"></param>
        /// <param name="anchorMaxY"></param>
        /// <returns></returns>
        public static RectTransform GetOrCreateChildRectTransform
        (
            RectTransform parentRectTrfm,
            Vector2 parentSize,
            string childName,
            float panelOffsetX, float panelOffsetY,
            float panelWidth, float panelHeight,
            float anchorMinX, float anchorMinY,
            float anchorMaxX, float anchorMaxY
        )
        {
            RectTransform childRectTrfm = null;

            if (parentRectTrfm != null && !string.IsNullOrEmpty(childName))
            {
                Transform childTrfm = parentRectTrfm.Find(childName);
                if (childTrfm == null)
                {
                    GameObject _tempGO = new GameObject(childName);
                    if (_tempGO != null)
                    {
                        _tempGO.layer = 5;
                        _tempGO.transform.SetParent(parentRectTrfm);
                        childRectTrfm = _tempGO.AddComponent<RectTransform>();

                        childRectTrfm.anchorMin = new Vector2(anchorMinX, anchorMinY);
                        childRectTrfm.anchorMax = new Vector2(anchorMaxX, anchorMaxY);

                        //Debug.Log("[DEBUG] GetOrCreateChildRectTransform parentSize: " + parentSize);

                        // Panel width and height are 0.0-1.0 values
                        childRectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth * parentSize.x);
                        childRectTrfm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight * parentSize.y);

                        // Force the child to be the same scale as the parent
                        childRectTrfm.localScale = Vector3.one;

                        // Original
                        //childRectTrfm.localPosition = new Vector3((panelOffsetX * parentSize.x) + (panelWidth * 0.5f * parentSize.x), (panelOffsetY * parentSize.y) + (panelHeight * 0.5f * parentSize.y), parentRectTrfm.localPosition.z);

                        // Test
                        childRectTrfm.localPosition = new Vector3(panelOffsetX * parentSize.x, panelOffsetY * parentSize.y, parentRectTrfm.localPosition.z);
                    }
                }
                else
                {
                    // If the transform exists but the RectTransform component is missing, it will return null
                    childRectTrfm = childTrfm.GetComponent<RectTransform>();
                }
            }

            return childRectTrfm;
        }

        /// <summary>
        /// Return the default Unity font
        /// </summary>
        /// <returns></returns>
        public static Font GetDefaultFont()
        {
            #if UNITY_2022_2_OR_NEWER
            return (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
            #else
            return (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            #endif
        }

        #endregion

        #region Texture Methods

        public static Texture2D CreateTexture(int width, int height, Color colour, bool apply)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            SSCUtils.FillTexture(tex, colour, apply);

            return tex;
        }

        /// <summary>
        /// This will generate GC. However, it uses Color32 and GetPixels32 which
        /// is more memory efficient than GetPixels.
        /// WARNING: Not recommended to be called each frame due to GC overhead.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="colour"></param>
        /// <param name="apply"></param>
        public static void FillTexture(Texture2D tex, Color32 colour, bool apply)
        {
            if (tex != null)
            {
                Color32[] colours = tex.GetPixels32();

                if (colours != null)
                {
                    for (int p = 0; p < colours.Length; p++)
                    {
                        colours[p] = colour;
                    }

                    // Copy array back to the texture
                    tex.SetPixels32(colours);
                    // Update the texture
                    if (apply) { tex.Apply(); }
                    colours = null;
                }
            }
        }

        #endregion

        #region Colour Methods

        /// <summary>
        /// Update one colour with the other. Same as dest = source.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color32 sourceColour, ref Color32 destColour, ref SSCColour destSSCColour, bool ifChanged)
        {
            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != sourceColour.r || destColour.g != sourceColour.g || destColour.b != sourceColour.b || destColour.a != sourceColour.a)
            {
                destColour.r = sourceColour.r;
                destColour.g = sourceColour.g;
                destColour.b = sourceColour.b;
                destColour.a = sourceColour.a;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Update one colour with the other. Same as dest = source without the implicit conversion.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color sourceColour, ref Color32 destColour, ref SSCColour destSSCColour, bool ifChanged)
        {
            byte srcR = (byte)(sourceColour.r * 255f);
            byte srcG = (byte)(sourceColour.g * 255f);
            byte srcB = (byte)(sourceColour.b * 255f);
            byte srcA = (byte)(sourceColour.a * 255f);

            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != srcR || destColour.g != srcG || destColour.b != srcB || destColour.a != srcA)
            {
                destColour.r = srcR;
                destColour.g = srcG;
                destColour.b = srcB;
                destColour.a = srcA;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Update one colour with the other. Same as dest = source without the implicit conversion.
        /// When ifChanged = true, the update will only occur if the source and dest have different values.
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        /// <param name="destSSCColour"></param>
        /// <param name="ifChanged"></param>
        public static void UpdateColour(ref Color sourceColour, ref Color destColour, ref SSCColour destSSCColour, bool ifChanged)
        {
            // if ifChanged is false OR the source and dest values are different, update the dest colour
            if (!ifChanged || destColour.r != sourceColour.r || destColour.g != sourceColour.g || destColour.b != sourceColour.b || destColour.a != sourceColour.a)
            {
                destColour.r = sourceColour.r;
                destColour.g = sourceColour.g;
                destColour.b = sourceColour.b;
                destColour.a = sourceColour.a;

                destSSCColour.Set(destColour.r, destColour.g, destColour.b, destColour.a, true);
            }
        }

        /// <summary>
        /// Attempt to copy a struct Color32 to a struct Color without allocating any memory
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        public static void Color32toColorNoAlloc(ref Color32 sourceColour, ref Color destColour)
        {
            // Avoid creating a new Color struct by not doing an implicit conversion.
            destColour.r = sourceColour.r / 255f;
            destColour.g = sourceColour.g / 255f;
            destColour.b = sourceColour.b / 255f;
            destColour.a = sourceColour.a / 255f;
        }

        /// <summary>
        /// Attempt to copy a struct Color to a struct Color without allocating any memory
        /// </summary>
        /// <param name="sourceColour"></param>
        /// <param name="destColour"></param>
        public static void ColortoColorNoAlloc(ref Color sourceColour, ref Color destColour)
        {
            // Avoid creating a new Color struct.
            destColour.r = sourceColour.r;
            destColour.g = sourceColour.g;
            destColour.b = sourceColour.b;
            destColour.a = sourceColour.a;
        }

        #endregion

        #region GameObject or Tranform Methods

        /// <summary>
        /// Get and existing child transform or create a new one if it does not exit.
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static Transform GetOrCreateChildTransform(Transform parentTrfm, string childName)
        {
            Transform childTrfm = null;

            if (!string.IsNullOrEmpty(childName))
            {
                childTrfm = parentTrfm.Find(childName);
                if (childTrfm == null)
                {
                    GameObject _tempGO = new GameObject(childName);
                    if (_tempGO != null)
                    {
                        childTrfm = _tempGO.transform;
                        childTrfm.SetParent(parentTrfm);
                    }
                }
            }

            return childTrfm;
        }

        /// <summary>
        /// Get a child transform or UI panel of a parent given the name of the child
        /// See also GetChildRectTransform(..)
        /// </summary>
        /// <param name="parentTrfm"></param>
        /// <param name="childName"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static Transform GetChildTransform(Transform parentTrfm, string childName, string callerName)
        {
            Transform childTrm = parentTrfm == null || string.IsNullOrEmpty(childName) ? null : parentTrfm.Find(childName);

#if UNITY_EDITOR
            if (childTrm == null)
            {
                Debug.LogWarning(string.Format("ERROR: {0} could not find child {1} under {2}", callerName, string.IsNullOrEmpty(childName) ? "[unknown]" : childName, parentTrfm == null ? " no parent" : parentTrfm.name));
            }
#endif

            return childTrm;
        }

        #endregion

        #region Mesh Methods

        /// <summary>
        /// Get the bounds (dimensions) of a prefab or gameobject.
        /// NOTE: The position and rotation need to be reset to 0,0,0 before calling this method
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="includeInactive"></param>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static Bounds GetBounds(Transform transform, bool includeInactive, bool showErrors)
        {
            Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

            if (transform == null)
            {
                if (showErrors)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("ERROR: GetBounds transform is null");
#endif
                }
            }
            else if (transform.position != Vector3.zero || transform.rotation != Quaternion.identity)
            {
                if (showErrors)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("ERROR: GetBounds transform position must be 0,0,0 and rotation must be Quaternion.identity (0,0,0)");
#endif
                }
            }
            else
            {
                MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>(includeInactive);
                int numRenderers = renderers == null ? 0 : renderers.Length;

                if (renderers != null)
                {
                    for (int r = 0; r < numRenderers; r++)
                    {
                        // bounds is not nullable
                        combinedBounds.Encapsulate(renderers[r].bounds);

                        //Debug.Log(renderers[r].name + " " + renderers[r].bounds.extents + " new extents: " + combinedBounds.extents.magnitude);
                    }
                }
            }

            return combinedBounds;
        }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Verify a display (monitor or screen) and if required, activate it.
        /// Multiple displays can be activated (once) and then cannot be de-activated for the current session.
        /// Currently turning it off doesn't work due to a Unity limitation with additional displays...
        /// In the editor Display.displays will always return 1. So as a workaround, go to Game view, click
        /// Add Tab->Game view and move it to the second monitor. Then, on second Game view, set Display to Display 2.
        /// In the editor, with 2+ Game views, Maximise On Play will have no effect.
        /// </summary>
        /// <param name="displayNumber">Range 1 to 8</param>
        /// <param name="activateIfRequired"></param>
        /// <returns></returns>
        public static bool VerifyTargetDisplay(int displayNumber, bool activateIfRequired)
        {
            bool isValid = false;

#if UNITY_EDITOR
            isValid = displayNumber > 0 && displayNumber < 9;
#else

            if (displayNumber > 0 && Display.displays.Length >= displayNumber)
            {
                // If the display is not active, activate it now
                if (activateIfRequired && !Display.displays[displayNumber-1].active) { Display.displays[displayNumber-1].Activate(); }
                isValid = true;
            }
#endif

            return isValid;
        }

        #endregion

        #region Layer Methods

        /// <summary>
        /// Check to see if a Unity Layer number is enabled in a LayerMask
        /// </summary>
        /// <param name="layerNumber"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static bool IsInLayerMask(int layerNumber, LayerMask layerMask)
        {
            return ((1 << layerNumber) & layerMask) != 0;
        }

        #endregion

        #region Json Methods

        /// <summary>
        /// Save a list to Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson<T>(List<T> list)
        {
            ListWrapper<T> listWrapper = new ListWrapper<T>();
            listWrapper.itemList = list;
            return JsonUtility.ToJson(listWrapper);
        }

        /// <summary>
        /// Convert json into a List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static List<T> FromJson<T>(string jsonText)
        {
            ListWrapper<T> listWrapper = JsonUtility.FromJson<ListWrapper<T>>(jsonText);
            return listWrapper.itemList;
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Return the full name (AssemblyQualifiedName) for the given type
        /// eg. 
        /// string qn = SSCUtils.GetClassFullName(typeof(UnityStandardAssets.ImageEffects.Bloom), true);
        /// </summary>
        /// <param name="t"></param>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static string GetClassFullName(Type t, bool showErrors)
        {
            string fullName = string.Empty;

            try
            {
                fullName = t.AssemblyQualifiedName;
            }
            catch (Exception ex)
            {
                if (showErrors) { Debug.LogWarning("SSCUtils - Class not installed: " + ex.Message); }
            }
            return fullName;
        }

        /// <summary>
        /// Return the class type from a full class name
        /// Returns NULL if the class isn't installed in the project
        /// e.g.
        /// System.Type type = GetClassTypeFromFullName("UnityStandardAssets.Water.WaterBase,Assembly-CSharp", true);
        /// </summary>
        /// <param name="classFullName"></param>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static System.Type GetClassTypeFromFullName(string classFullName, bool showErrors)
        {
            System.Type type = null;
            try
            {
                type = System.Type.GetType(classFullName, true, true);
            }
            catch (Exception ex)
            {
                if (showErrors) { Debug.LogWarning("SSCUtils - Class not installed: " + ex.Message); }
            }
            return type;
        }

        /// <summary>
        /// Get a field value of the given type from the class. If the field is not static, an instance of the class must be supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType"></param>
        /// <param name="fieldName"></param>
        /// <param name="classInstance"></param>
        /// <param name="includePrivate"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static T ReflectionGetValue<T>(System.Type classType, string fieldName, UnityEngine.Object classInstance, bool includePrivate, bool isStatic)
        {
            // Some types may not be nullable, so instead set to the default value for that type.
            T value = default(T);

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !isStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (isStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (isStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.FieldInfo fieldInfo = typeof(T).GetField(fieldName, bindingFlags);

            if (fieldInfo != null)
            {
                if (isStatic) { value = (T)fieldInfo.GetValue(null); }
                else { value = (T)fieldInfo.GetValue(classInstance); }
            }

            return value;
        }

        /// <summary>
        /// Get a property value of the given type from the class. If the property is not static, an instance of the class must be supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <param name="classInstance"></param>
        /// <param name="includePrivate"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static T ReflectionGetPropertyValue<T>(System.Type classType, string propertyName, UnityEngine.Object classInstance, bool includePrivate, bool isStatic)
        {
            // Some types may not be nullable, so instead set to the default value for that type.
            T value = default(T);

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !isStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (isStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (isStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.PropertyInfo propertyInfo = classType.GetProperty(propertyName, bindingFlags);

            if (propertyInfo != null)
            {
                if (isStatic) { value = (T)propertyInfo.GetValue(null); }
                else { value = (T)propertyInfo.GetValue(classInstance); }
            }

            return value;
        }

        /// <summary>
        /// Get a method so that it can be invoked, given the type of class it is in and its method name
        /// </summary>
        /// <param name="t"></param>
        /// <param name="methodName"></param>
        /// <param name="includePrivate"></param>
        /// <param name="includeStatic"></param>
        /// <returns></returns>
        public static System.Reflection.MethodInfo ReflectionGetMethod(Type t, string methodName, bool includePrivate, bool includeStatic)
        {
            if (string.IsNullOrEmpty(methodName)) { return null; }

            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            return t.GetMethod(methodName, bindingFlags);
        }


        public static void ReflectionOutputMethods(Type t, bool showParmeters, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            System.Reflection.MethodInfo[] methodInfos = t.GetMethods(bindingFlags);
            foreach (System.Reflection.MethodInfo mInfo in methodInfos)
            {
                Debug.Log("SSCUtils: Type: " + t.Name + " Method: " + mInfo.Name);

                if (showParmeters)
                {
                    System.Reflection.ParameterInfo[] parameters = mInfo.GetParameters();
                    if (parameters != null)
                    {
                        foreach (System.Reflection.ParameterInfo parm in parameters)
                        {
                            Debug.Log(" Parm: " + parm.Name + " ParmType: " + parm.ParameterType.Name);
                        }
                    }
                }
            }
        }

        public static void ReflectionOutputFields(Type t, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            // Get the Fields of the type based on the binding flags selected
            System.Reflection.FieldInfo[] fInfos = t.GetFields(bindingFlags);

            foreach (System.Reflection.FieldInfo fInfo in fInfos)
            {
                Debug.Log("SSCUtils: Type: " + t.Name + " field: " + fInfo.Name + " fldtype: " + fInfo.FieldType.Name);
            }
        }

        public static void ReflectionOutputProperties(Type t, bool includePrivate, bool includeStatic)
        {
            // Set up the binding flags
            System.Reflection.BindingFlags bindingFlags;

            if (includePrivate && !includeStatic)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            }
            else if (includeStatic && !includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            }
            else if (includeStatic && includePrivate)
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                               System.Reflection.BindingFlags.Static;
            }
            else
            {
                bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            }

            // Get the Properties of the type based on the binding flags selected
            System.Reflection.PropertyInfo[] piArray = t.GetProperties(bindingFlags);

            foreach (System.Reflection.PropertyInfo pi in piArray)
            {
                Debug.Log("SSCUtils: Type: " + t.Name + " property: " + pi.Name + " proptype: " + pi.PropertyType.Name);
            }
        }

        #endregion

        #region String Methods

        public static int GetMajorVersion(string versionString)
        {
            int numValue = 0;
            if (!string.IsNullOrEmpty(versionString))
            {
                int firstDot = versionString.IndexOf(".", 0);

                // First dot must exist and not be in first position
                if (firstDot > 0)
                {
                    int result;
                    if (int.TryParse(versionString.Substring(0, firstDot), out result)) { numValue = result; }
                }
            }
            return numValue;
        }

        public static int GetMinorVersion(string versionString)
        {
            int numValue = 0;
            if (!string.IsNullOrEmpty(versionString))
            {
                int firstDot = versionString.IndexOf(".", 0);

                // First dot must exist and not be in first position
                if (firstDot > 0)
                {
                    int result;
                    int secondDot = versionString.IndexOf(".", firstDot + 1);
                    if (secondDot == -1)
                    {
                        // e.g. 1.3  (not tested)
                        if (int.TryParse(versionString.Substring(firstDot + 1), out result)) { numValue = result; }
                    }
                    else
                    {
                        // e.g. 1.4.2
                        if (int.TryParse(versionString.Substring(firstDot + 1, secondDot - firstDot - 1), out result)) { numValue = result; }
                    }
                }
            }
            return numValue;
        }

        public static int GetPatchVersion(string versionString)
        {
            int numValue = 0;
            if (!string.IsNullOrEmpty(versionString))
            {
                int firstDot = versionString.IndexOf(".", 0);

                // First dot must exist and not be in first position
                if (firstDot > 0)
                {
                    int result;
                    int secondDot = versionString.IndexOf(".", firstDot + 1);
                    // Must exist and cannot be last character
                    if (secondDot > 0 && versionString.Length > secondDot + 1)
                    {
                        // e.g. 1.4.2
                        if (int.TryParse(versionString.Substring(secondDot + 1), out result)) { numValue = result; }
                    }
                }
            }
            return numValue;
        }

        /// <summary>
        /// Compare two 3-part numbers and determine if they are the same (0),
        /// the first is less than the second (-1) or the first is greater than
        /// the second (1).
        /// </summary>
        /// <param name="versionString1"></param>
        /// <param name="versionString2"></param>
        /// <returns></returns>
        public static int CompareVersionNumbers(string versionString1, string versionString2)
        {
            // Assume the same
            int numValue = 0;

            if (!(string.IsNullOrEmpty(versionString1) && string.IsNullOrEmpty(versionString2)))
            {
                int v1major = GetMajorVersion(versionString1);
                int v2major = GetMajorVersion(versionString2);

                if (v1major < v2major) { numValue = -1; }
                else if (v1major > v2major) { numValue = 1; }
                else
                {
                    // Both major versions are the same
                    int v1minor = GetMinorVersion(versionString1);
                    int v2minor = GetMinorVersion(versionString2);

                    if (v1minor < v2minor) { numValue = -1; }
                    else if (v1minor > v2minor) { numValue = 1; }
                    else
                    {
                        // Both minor versions are the same
                        int v1patch = GetPatchVersion(versionString1);
                        int v2patch = GetPatchVersion(versionString2);

                        if (v1patch < v2patch) { numValue = -1; }
                        else if (v1patch > v2patch) { numValue = 1; }
                    }
                }
            }

            return numValue;
        }

        /// <summary>
        /// Return a string from a value rounded to n (0-3) decimal place.
        /// Option to add a percentage sign to the end of string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <param name="isPercentage"></param>
        /// <returns></returns>
        public static string GetNumericString(float value, int decimalPlaces, bool isPercentage)
        {
            // Reduce GC as much as possible by not appending strings to each other
            if (decimalPlaces == 0)
            {
                if (isPercentage) { return (Mathf.RoundToInt(value) / 100f).ToString("0%"); }
                else { return Mathf.RoundToInt(value).ToString("0"); }
            }
            else
            {
                double dValue = (float)System.Math.Round(value, decimalPlaces);

                if (isPercentage)
                {
                    dValue /= 100f;
                    if (decimalPlaces == 1)
                    {
                        return dValue.ToString("0.0%");
                    }
                    else if (decimalPlaces == 2)
                    {
                        return dValue.ToString("0.00%");
                    }
                    else
                    {
                        return dValue.ToString("0.000%");
                    }
                }
                else if (decimalPlaces == 1)
                {
                    return dValue.ToString("0.0");
                }
                else if (decimalPlaces == 2)
                {
                    return dValue.ToString("0.00");
                }
                else
                {
                    return dValue.ToString("0.000");
                }
            }
        }

        #endregion

        #region Scriptable Render Pipeline Methods

        /// <summary>
        /// Attempt to get the HDAdditionalLightData type when using HDRP.
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static System.Type GetHDLightDataType(bool showErrors)
        {
            System.Type hdLightDataType = null;

            try
            {
                hdLightDataType = SSCUtils.GetClassTypeFromFullName("UnityEngine.Rendering.HighDefinition.HDAdditionalLightData, Unity.RenderPipelines.HighDefinition.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false);

                if (hdLightDataType == null)
                {
#if UNITY_EDITOR
                    if (showErrors) { Debug.LogWarning("SSCUtils.GetHDLightDataType could not get the HDAdditionalLightData type."); }
#endif
                }
            }
            catch
            {
                if (showErrors) { Debug.LogWarning("SSCUtils.GetHDLightDataType failed. High Definition Render Pipeline might not be installed in this project."); }
            }


            return hdLightDataType;
        }

        /// <summary>
        /// Attempt to return the intensity of a Light in HDRP.
        /// </summary>
        /// <param name="hdLightDataType"></param>
        /// <param name="light"></param>
        /// <returns></returns>
        public static float GetHDLightIntensity(System.Type hdLightDataType, Light light)
        {
            float intensity = 0f;

            if (hdLightDataType != null && light != null)
            {
                Component hdLightData = null;

                // Get the HDAdditionalLightData component which is attached to the Light
                if (light.TryGetComponent(hdLightDataType, out hdLightData))
                {
                    intensity = (float)hdLightDataType.GetProperty(PrptyNameHDIntensity).GetValue(hdLightData);
                }
            }

            return intensity;
        }

        /// <summary>
        /// Attempt to multiple the intensity of a Light in HDRP by the value provided.
        /// </summary>
        /// <param name="hdLightDataType"></param>
        /// <param name="light"></param>
        /// <param name="multiplier"></param>
        public static void HDLightIntensityMultiply(System.Type hdLightDataType, Light light, float multiplier)
        {
            if (hdLightDataType != null && light != null)
            {
                Component hdLightData = null;

                // Get the HDAdditionalLightData component which is attached to the Light
                if (light.TryGetComponent(hdLightDataType, out hdLightData))
                {
                    float intensity = (float)hdLightDataType.GetProperty(PrptyNameHDIntensity).GetValue(hdLightData);

                    hdLightDataType.GetProperty(PrptyNameHDIntensity).SetValue(hdLightData, intensity * multiplier);
                }
            }
        }

        /// <summary>
        /// Is the High Definition Render Pipeline installed in this project?
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static bool IsHDRP(bool showErrors)
        {
            bool isInstalled = false;

            try
            {
                var renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
                if (renderPipelineAsset != null && renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset")) { isInstalled = true; }
                else if (showErrors) { Debug.LogWarning("SSCUtils.IsHDRP: it appears that High Definition Render Pipeline is not installed in this project."); }
                //Debug.Log("renderPipelineAsset: " + (renderPipelineAsset.GetType()).Name);
            }
            catch
            {
                if (showErrors) { Debug.LogWarning("SSCUtils.IsHDRP: it appears that High Definition Render Pipeline is not installed in this project."); }
            }
            return isInstalled;
        }

        /// <summary>
        /// Is the Light Weight Render Pipeline installed in this project?
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static bool IsLWRP(bool showErrors)
        {
            bool isInstalled = false;

            try
            {
                var renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

                if (renderPipelineAsset != null && renderPipelineAsset.GetType().Name.Contains("LightweightRenderPipelineAsset")) { isInstalled = true; }
                else if (showErrors) { Debug.LogWarning("SSCUtils.IsLWRP: it appears that Light Weight Render Pipeline is not installed in this project."); }

                //Debug.Log("renderPipelineAsset: " + (renderPipelineAsset.GetType()).Name);
            }
            catch
            {
                if (showErrors) { Debug.LogWarning("SSCUtils.IsLWRP: it appears that Light Weight Render Pipeline is not installed in this project."); }
            }

            return isInstalled;
        }

        /// <summary>
        /// Is the Universal Render Pipeline installed in this project?
        /// </summary>
        /// <param name="showErrors"></param>
        /// <returns></returns>
        public static bool IsURP(bool showErrors)
        {
            bool isInstalled = false;

            try
            {
                var renderPipelineAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

                if (renderPipelineAsset != null && renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset")) { isInstalled = true; }
                else if (showErrors) { Debug.LogWarning("SSCUtils.IsURP: it appears that Universal Render Pipeline is not installed in this project."); }

                //Debug.Log("renderPipelineAsset: " + (renderPipelineAsset.GetType()).Name);
            }
            catch
            {
                if (showErrors) { Debug.LogWarning("SSCUtils.IsURP: it appears that Light Weight Render Pipeline is not installed in this project."); }
            }

            return isInstalled;
        }

        /// <summary>
        /// Attempt to set the intensity of a Light in HDRP
        /// </summary>
        /// <param name="hdLightDataType"></param>
        /// <param name="light"></param>
        /// <param name="intensity"></param>
        public static void SetHDLightIntensity(System.Type hdLightDataType, Light light, float intensity)
        {
            if (hdLightDataType != null && light != null)
            {
                Component hdLightData = null;

                // Get the HDAdditionalLightData component which is attached to the Light
                if (light.TryGetComponent(hdLightDataType, out hdLightData))
                {
                    hdLightDataType.GetProperty(PrptyNameHDIntensity).SetValue(hdLightData, intensity);
                }
            }
        }


        #endregion

        #region UnityEvents

        /// <summary>
        /// Get the number of listeners for a UnityEvent.
        /// Includes persistent (configured in inspector)
        /// and non-persistent (AddListener() at runtime).
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <returns></returns>
        public static int GetNumListeners (UnityEngine.Events.UnityEventBase unityEvent)
        {
            if (unityEvent != null)
            {
                // Get the Fieldinfo for the private m_Calls field.
                var field = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                // Get the list of methods to be invoked (persistent and non-persistent)
                var invokeCallList = field.GetValue(unityEvent);
                // Get the Count PropertyInfo
                var property = invokeCallList.GetType().GetProperty("Count");

                // Get the number in the list
                return (int)property.GetValue(invokeCallList);
            }
            else { return 0; }
        }

        /// <summary>
        /// Has the event got any persistent (configured in inspector) or non-persistent (AddListener() at runtime) listeners?
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <returns></returns>
        public static bool HasListeners (UnityEngine.Events.UnityEventBase unityEvent)
        {
            // Check Persistent count first as doesn't require reflection
            return unityEvent != null ? unityEvent.GetPersistentEventCount() > 0 || GetNumListeners(unityEvent) > 0 : false;
        }

        #endregion
    }


    #region List Wrapper Class
    /// <summary>
    /// Used with JsonUtility to convert a list to/from json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ListWrapper<T>
    {
        public List<T> itemList;
    }
    #endregion
}