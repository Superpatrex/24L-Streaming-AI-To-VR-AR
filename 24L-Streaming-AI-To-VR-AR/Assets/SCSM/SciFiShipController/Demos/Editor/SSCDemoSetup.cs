using UnityEditor;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    // Script that performs miscellaneous setup tasks for Sci-Fi Ship Controller demos - runs when the project is opened in the Unity editor
    // See also SSCSetup

    [InitializeOnLoad]
    public static class SSCDemoSetup
    {
        #region Constructor
        static SSCDemoSetup()
        {
            int[] layerNumbersToAdd = { Celestials.celestialsUnityLayer, 27 };
            string[] layersToAdd = { "SSC Celestials", "Small Ships" };

            SSCSetup.FindTagAndLayerManager();
            SSCSetup.CreateLayers(layersToAdd, layerNumbersToAdd);

        }

        #endregion
    }
}