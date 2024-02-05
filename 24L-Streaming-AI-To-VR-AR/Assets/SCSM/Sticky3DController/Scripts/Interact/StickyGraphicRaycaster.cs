using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Used on world space canvas with Sticky3D in VR
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Utilities/Sticky Graphic Raycaster (VR)")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [DisallowMultipleComponent]
    public class StickyGraphicRaycaster : GraphicRaycaster
    {
        #region Public Variables

        #endregion

        #region Public Properties

        #endregion

        #region Public Static Variables

        #endregion

        #region Private Variables - General

        private bool isInitialised = false;

        [System.NonSerialized] private Canvas canvas = null;

        private struct GraphicHit
        {
            public Graphic graphic;
            public Vector3 hitPoint;
            public Vector2 screenPosition;
        }


        [System.NonSerialized] private List<GraphicHit> sortedHitsList = new List<GraphicHit>();

        #endregion

        #region Public Delegates

        #endregion

        #region Private Initialise Methods

        protected override void Start()
        {
            if (gameObject.TryGetComponent(out canvas))
            {
                isInitialised = true;
            }
        }

        #endregion

        #region Update Methods

        #endregion

        #region Private and Internal Methods - General

        /// <summary>
        /// Does a ray intersect a RectTransform?
        /// Based on RectTransformUtility.ScreenPointToWorldPointInRectangle(..).
        /// </summary>
        /// <param name="rectTfrm"></param>
        /// <param name="ray"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        private bool IsIntersectedByRay(RectTransform rectTfrm, Ray ray, out Vector3 hitPoint)
        {
            // Create a plane for the RectTransform dimensions.
            Vector3[] rtCorners = new Vector3[4];
            // Bottom Left, Top Left, Top Right, Bottom Right.
            rectTfrm.GetWorldCorners(rtCorners);

            // NOTE: This is an infinite plane, rather than one bound by the corners.
            Plane plane = new Plane(rtCorners[0], rtCorners[1], rtCorners[2]);

            //Plane plane = new Plane(rectTfrm.rotation * Vector3.back, rectTfrm.position);
            float distance = 0f;

            // Check to see if it is not in the same plane
            float dotP = Vector3.Dot(Vector3.Normalize(rectTfrm.position - ray.origin), plane.normal);

            if (dotP != 0 && !plane.Raycast(ray, out distance))
            {
                hitPoint = Vector3.zero;
                return false;
            }
            else
            {
                Vector3 pointOnPlane = ray.GetPoint(distance);

                // Is the point within the bounds of the RectTransform?

                Vector3 bottomEdge = rtCorners[3] - rtCorners[0];
                Vector3 leftEdge = rtCorners[1] - rtCorners[0];
                float bottomDot = Vector3.Dot(pointOnPlane - rtCorners[0], bottomEdge);
                float leftDot = Vector3.Dot(pointOnPlane - rtCorners[0], leftEdge);

                bool isPointOnPlane = bottomDot < bottomEdge.sqrMagnitude && leftDot < leftEdge.sqrMagnitude && bottomDot >= 0 && leftDot >= 0;

                if (isPointOnPlane)
                {
                    hitPoint = rtCorners[0] + leftDot * leftEdge / leftEdge.sqrMagnitude + bottomDot * bottomEdge / bottomEdge.sqrMagnitude;
                    //Debug.Log("[DEBUG] " + rectTfrm.name + " hitPoint: " + hitPoint + " dist: " + distance);
                }
                else { hitPoint = Vector3.zero; }

                return isPointOnPlane;

                //return SciFiShipController.SSCMath.IsInQuad(rtCorners[0], rtCorners[1], rtCorners[2], rtCorners[3], pointOnPlane);
            }            
        }

        #endregion

        #region Events

        #endregion

        #region Public API Methods - General

        public override void Raycast (PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (isInitialised && S3DPointerEventData.IsS3DPointer(eventData))
            {
                S3DPointerEventData s3dEventData = eventData as S3DPointerEventData;

                //Debug.Log("[DEBUG] StickyGraphicRaycaster.Raycast + " + s3dEventData.ray.origin + " " + name + " T:" + Time.time);

                sortedHitsList.Clear();

                // Get a list of all the registered Graphics on the canvas
                IList<Graphic> gfcList = GraphicRegistry.GetGraphicsForCanvas(canvas);

                int numGraphics = gfcList == null ? 0 : gfcList.Count;

                for (int gfcIdx = 0; gfcIdx < numGraphics; gfcIdx++)
                {
                    Graphic graphic = gfcList[gfcIdx];

                    // Depth -1 is not drawn.
                    if (graphic.depth >= 0 && graphic.raycastTarget && !graphic.canvasRenderer.cull)
                    {
                        Vector3 hitPoint;
                        if (IsIntersectedByRay (graphic.rectTransform, s3dEventData.ray, out hitPoint))
                        {
                            // Get the screen hitpoint which is what Unity UI is expecting
                            Vector2 screenPos = eventCamera.WorldToScreenPoint(hitPoint);

                            if (graphic.Raycast(screenPos, eventCamera))
                            {
                                //GraphicHit graphicHit = new GraphicHit() { graphic = graphic, hitPoint = hitPoint };
                                sortedHitsList.Add(new GraphicHit() { graphic = graphic, hitPoint = hitPoint, screenPosition = screenPos });
                            }

                            //Debug.Log("[DEBUG] StickyGraphicRaycaster.Raycast + " + graphic.name + " parent "+ graphic.transform.parent.name + " " + name + " T:" + Time.time);
                        }
                    }
                }

                int numGraphicHits = sortedHitsList.Count;

                // Sort the UI graphics by depth
                if (numGraphicHits > 0)
                {
                    //Debug.Log("[DEBUG] Graphic hits: " + numGraphicHits);
                    sortedHitsList.Sort((g1, g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
                }

                for (int ghIdx = 0; ghIdx < numGraphicHits; ghIdx++)
                {
                    GraphicHit graphicHit = sortedHitsList[ghIdx];

                    // Check to see if we are on the wrong side of the RectTransform of the UI
                    if (ignoreReversedGraphics && Vector3.Dot(s3dEventData.ray.direction, graphicHit.graphic.transform.rotation * Vector3.forward) <= 0)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(s3dEventData.ray.origin, graphicHit.hitPoint);

                    /// TODO - StickyGraphicRaycaster - consider blocking objects.

                    RaycastResult rayCastResult = new RaycastResult
                    {
                        module = this,
                        distance = distance,
                        index = resultAppendList.Count,
                        depth = graphicHit.graphic.depth,
                        sortingLayer = canvas.sortingLayerID,
                        sortingOrder = canvas.sortingOrder,
                        worldPosition = graphicHit.hitPoint,
                        worldNormal = -graphicHit.graphic.transform.forward,
                        screenPosition = graphicHit.screenPosition,
                        gameObject = graphicHit.graphic.gameObject,
                        displayIndex = eventCamera.targetDisplay
                    };

                    resultAppendList.Add(rayCastResult);
                }

                //base.Raycast(eventData, resultAppendList);
            }
        }

        #endregion
    }
}