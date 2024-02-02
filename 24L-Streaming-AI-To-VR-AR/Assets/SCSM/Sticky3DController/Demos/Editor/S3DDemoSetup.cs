using UnityEditor;
using UnityEngine;

// Copyright (c) 2018-2021 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Script that performs miscellaneous setup tasks for Sticky3D Controller demos - runs when the project is opened in the Unity editor.
    /// See also S3DSetup
    /// </summary>
    [InitializeOnLoad]
    public static class S3DDemoSetup
    {
        #region Constructor
        static S3DDemoSetup()
        {
            int[] layerNumbersToAdd = { 28, 29 };
            string[] layersToAdd = { "Climbable", "Interactable" };

            StickySetup.FindTagAndLayerManager();
            StickySetup.CreateLayers(layersToAdd, layerNumbersToAdd, false);
        }

        #endregion
    }
}