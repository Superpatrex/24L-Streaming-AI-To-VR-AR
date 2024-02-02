using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// A data class to pass a query to the radar system.
    /// </summary>
    /// [Unity.VisualScripting.Inspectable]
    public class SSCRadarQuery
    {
        #region Static Read-only valiables
        /// <summary>
        /// Don't consider factionId when performing a query
        /// </summary>
        public static readonly int IGNOREFACTION = -1;

        // Static optimised enum lookups
        public static readonly int querySortOrderNoneInt = (int)QuerySortOrder.None;
        public static readonly int querySortOrderDistanceAsc2DInt = (int)QuerySortOrder.DistanceAsc2D;
        public static readonly int querySortOrderDistanceAsc3DInt = (int)QuerySortOrder.DistanceAsc3D;
        public static readonly int querySortOrderDistanceDesc2DInt = (int)QuerySortOrder.DistanceDesc2D;
        public static readonly int querySortOrderDistanceDesc3DInt = (int)QuerySortOrder.DistanceDesc3D;
        #endregion

        #region Enumerations
        public enum QuerySortOrder : int
        {
            None = 0,
            DistanceAsc2D = 1,
            DistanceAsc3D = 2,
            DistanceDesc2D = 11,
            DistanceDesc3D = 12
        }
        #endregion

        #region Public variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// This is the world-space position around which the radar query emits.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public Vector3 centrePosition;

        /// <summary>
        /// The radar range in metres from the centrePosition
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public float range;

        /// <summary>
        /// Uses 3D distances to determine range when querying the radar data.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public bool is3DQueryEnabled;

        /// <summary>
        /// The sort order of the results. None is the fastest option and has
        /// the lowest performance impact.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public QuerySortOrder querySortOrder;

        /// <summary>
        /// The faction or alliance the item belongs to. This can be used to identify if an item is friend or foe.
        /// 0 = neutral. -1 means not set (ignore factionId in query).
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int factionId;

        /// <summary>
        /// [Optional] An array of factionIds to include from the query results. Only considered
        /// sscRadarQuery.factionId is -1.
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int[] factionsToInclude;

        /// <summary>
        /// [Optional] An array of factionIds to exclude from the query results
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int[] factionsToExclude;

        /// <summary>
        /// [Optional] An array of squadronIds to include from the query results
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int[] squadronsToInclude;

        /// <summary>
        /// [Optional] An array of squadronIds to exclude from the query results
        /// </summary>
        /// [Unity.VisualScripting.Inspectable]
        public int[] squadronsToExclude;

        // Uncomment, set, then create Debug.Logs in SSCRadar.GetRadarResults(..)
        //public int queryDebugTag;

        #endregion

        #region Constructors
        public SSCRadarQuery()
        {
            SetClassDefault();
        }
        #endregion

        #region Public Member Methods
        public void SetClassDefault()
        {
            centrePosition = Vector3.zero;
            range = 100f;
            is3DQueryEnabled = true;
            querySortOrder = QuerySortOrder.None;

            // -1 means don't consider in query
            factionId = IGNOREFACTION;

            factionsToExclude = null;
            squadronsToInclude = null;
            squadronsToExclude = null;
        }
        #endregion
    }
}