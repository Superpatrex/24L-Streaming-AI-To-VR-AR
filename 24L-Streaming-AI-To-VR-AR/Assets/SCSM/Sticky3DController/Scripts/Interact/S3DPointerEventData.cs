using UnityEngine;
using UnityEngine.EventSystems;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Used in VR to extend the PointerEventData class so that it can
    /// use raycasts instead of screen points
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class S3DPointerEventData : PointerEventData
    {
        #region Public Variables

        public Ray ray;

        #endregion

        #region Private Variables - General

        #endregion

        #region Public Properties

        #endregion

        #region Constructors

        public S3DPointerEventData (EventSystem eventSystem) : base (eventSystem)
        {

        }

        #endregion

        #region Public Static API Methods

        /// <summary>
        /// Is this PointerEventData of type S3DPointerEventData?
        /// </summary>
        /// <param name="pointerEventData"></param>
        /// <returns></returns>
        public static bool IsS3DPointer (PointerEventData pointerEventData)
        {
            return pointerEventData is S3DPointerEventData;
        }

        #endregion
    }
}