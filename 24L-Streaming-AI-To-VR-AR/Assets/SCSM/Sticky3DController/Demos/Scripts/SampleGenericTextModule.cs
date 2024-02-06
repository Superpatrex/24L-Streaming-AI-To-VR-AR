using UnityEngine;
using scsmmedia;

// Sticky3D Control Module Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace MyUniqueGame
{
    /// <summary>
    /// Demo script used to show how to create your own custom StickyGenericModule component.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// 1. The script should be attached to your Text Mesh gameobject. Typically the Text Mesh
    ///    will be a child of an empty gameobject. The parent will contain this script.
    /// 2. Make a prefab of this gameobject and delete the original from the scene.
    /// 3. Using our APIs in code: stickyManager = StickyManager.GetOrCreateManager().
    /// 4. Then call stickyManager.GetorCreateGenericPool(textModule)
    /// 5. Then stickyManager.InstantiateGenericObject(..) as required.
    /// See also Demo3DTextSpawner.cs.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/Generic Text Module")]
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    public class SampleGenericTextModule : StickyGenericModule
    {
        #region Private Variables

        // In your module, you might want to use TMPro rather than the older TextMesh.
        private TextMesh textMesh = null;
        #endregion

        #region Override Methods

        /// <summary>
        /// Override the Initialise method so we can add our own stuff as well.
        /// </summary>
        /// <param name="igParms"></param>
        /// <returns></returns>
        public override uint Initialise (S3DInstantiateGenericObjectParameters igParms)
        {
            uint _itemSeqNum = base.Initialise(igParms);

            if (textMesh == null)
            {
                textMesh = GetComponentInChildren<TextMesh>();
            }

            #if UNITY_EDITOR
            // Make sure the TextMesh component is part of the prefab
            if (textMesh == null)
            {
                Debug.LogWarning("Could not find TextMesh child component in " + gameObject.name + " did you forget to add it to the prefab?");
            }
            #endif

            // If we had some text animation, we could initialise, reset it, and/or play it here.

            return _itemSeqNum;
        }

        #endregion

        #region Public Member Methods

        /// <summary>
        /// Update the text for this object
        /// </summary>
        /// <param name="meshTxt"></param>
        public void SetText (string meshTxt)
        {
            if (textMesh != null)
            {
                textMesh.text = meshTxt;
            }
        }

        #endregion
    }
}
