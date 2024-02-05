using System;
using UnityEngine;

// Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This script gets added to a proximity collider by the StickyShapesModule
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class StickyShapesProximity : MonoBehaviour
    {
        #region Public Variables

        #endregion

        #region Protected and Internal Variables

        [NonSerialized] internal StickyShapesModule stickyShapesModule = null;

        protected bool isInitialised = false;

        #endregion

        #region Events

        protected void OnTriggerEnter(Collider other)
        {
            if (isInitialised && stickyShapesModule.IsReactingEnabled)
            {
                StickyControlModule otherS3DCharacter = null;

                if (StickyControlModule.IsS3DCharacter(other, out otherS3DCharacter))
                {
                    stickyShapesModule.CharacterEnter(otherS3DCharacter);                    
                }
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (isInitialised && stickyShapesModule.IsReactingEnabled)
            {
                StickyControlModule otherS3DCharacter = null;

                if (StickyControlModule.IsS3DCharacter(other, out otherS3DCharacter))
                {
                    stickyShapesModule.CharacterExit(otherS3DCharacter);
                }
            }
        }

        #endregion

        #region Protected and Internal Methods

        /// <summary>
        /// This allows the component to detect objects entering and exiting the
        /// collider area, and passing that data back to the StickyShapesModule.
        /// </summary>
        /// <param name="parentShapeModule"></param>
        internal void Initialise(StickyShapesModule parentShapeModule)
        {
            stickyShapesModule = parentShapeModule;

            isInitialised = parentShapeModule != null;
        }

        #endregion
    }
}