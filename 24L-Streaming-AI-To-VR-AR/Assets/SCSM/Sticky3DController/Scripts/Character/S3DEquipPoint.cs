using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// For placing inactive interactive objects on the body of a character.
    /// Typically, items will be parented to a humanoid bone.
    /// </summary>
    [HelpURL("https://scsmmedia.com/s3d-documentation")]
    [System.Serializable]
    public class S3DEquipPoint
    {
        #region Public Variables
        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The interactive-enabled objects that are permitted to be attached to this Equip Point.
        /// See the S3DInteractiveTags scriptableobject.
        /// </summary>
        public int permittedTags = ~0;

        /// <summary>
        /// The transform, typically a bone, that will the objects will be parented to.
        /// </summary>
        public Transform parentTransform;

        /// <summary>
        /// The local space offset from the parent transform
        /// </summary>
        public Vector3 relativeOffset;

        /// <summary>
        /// The local space rotation, in Euler angles, from the parent transform.
        /// </summary>
        public Vector3 relativeRotation;

        /// <summary>
        /// A description of the location or purpose for the equip point. e.g., right weapon holster
        /// </summary>
        public string equipPointName;

        /// <summary>
        /// The list of interactive-enabled objects attached to this equip point.
        /// </summary>
        public List<S3DStoreItem> storeItemList;

        /// <summary>
        /// The maximum number of items to equip at this point
        /// </summary>
        [Range(1,10)] public int maxItems;

        /// [INTERNAL USE ONLY]
        /// Hashed GUID code to uniquely identify an Equip Point on a character. Used instead of the
        /// name to avoid GC when comparing two Equip Points.
        public int guidHash;

        /// <summary>
        /// Whether the equip point is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;

        /// <summary>
        /// Whether the equip point is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this equip point are shown in the scene view of the editor.
        /// </summary>
        public bool showGizmosInSceneView;

        #endregion

        #region Constructors
        public S3DEquipPoint()
        {
            SetClassDefaults();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        ///<param name="s3dEquipPoint"></param>
        public S3DEquipPoint(S3DEquipPoint s3dEquipPoint)
        {
            if (s3dEquipPoint == null) { SetClassDefaults(); }
            else
            {
                guidHash = s3dEquipPoint.guidHash;
                permittedTags = s3dEquipPoint.permittedTags;
                parentTransform = s3dEquipPoint.parentTransform;
                relativeOffset = s3dEquipPoint.relativeOffset;
                relativeRotation = s3dEquipPoint.relativeRotation;
                equipPointName = s3dEquipPoint.equipPointName;
                maxItems = s3dEquipPoint.maxItems;
                showInEditor = s3dEquipPoint.showInEditor;
                selectedInSceneView = false;
                showGizmosInSceneView = s3dEquipPoint.showGizmosInSceneView;
                // Perform a deep copy
                if (s3dEquipPoint.storeItemList != null) { storeItemList = s3dEquipPoint.storeItemList.ConvertAll(sItem => new S3DStoreItem(sItem)); } else { storeItemList = new List<S3DStoreItem>(); }
            }
        }
        #endregion

        #region Private Member Methods
        public void SetClassDefaults()
        {
            guidHash = S3DMath.GetHashCodeFromGuid();
            parentTransform = null;
            relativeOffset = Vector3.zero;
            relativeRotation = Vector3.zero;
            equipPointName = string.Empty;
            maxItems = 1;
            showInEditor = true;
            selectedInSceneView = false;
            showGizmosInSceneView = false;
            storeItemList = new List<S3DStoreItem>();
            permittedTags = ~0;
        }

        #endregion

    }
}