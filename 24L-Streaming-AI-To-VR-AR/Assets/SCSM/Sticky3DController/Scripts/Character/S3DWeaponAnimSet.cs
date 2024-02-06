using System.Collections.Generic;
using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This scriptable object can contain a list of animation clip pairs used with one or more
    /// Sticky3D characters model types.
    /// It can also contain animation actions to send to the character holding the weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "Sticky3D Weapon Anim Set", menuName = "Sticky3D/Weapon Anim Set")]
    public class S3DWeaponAnimSet : ScriptableObject
    {
        #region Public Variables

        /// <summary>
        /// The time, in seconds, to delay the character turning to face the
        /// target when aiming starts. This allows time to transition from a
        /// held animation, to an aiming animation.
        /// </summary>
        [Range(0f,5f)] public float aimIKTurnDelay = 0.25f;

        /// <summary>
        /// The first person camera near clipping plane when aiming
        /// </summary>
        [Range(0.001f, 0.5f)] public float aimIKFPNearClippingPlane = 0.3f;

        /// <summary>
        /// When first person aiming is enabled, the weapon is parented, in local space,
        /// with this offset from the camera.
        /// </summary>
        public Vector3 aimIKFPWeaponOffset = new Vector3(0f, -0.05f, 0.4f);

        /// <summary>
        /// When grabbing a weapon, this is the time, in seconds, it takes the animation
        /// to play that transitions from not holding, to holding the weapon.
        /// </summary>
        [Range(0f, 3f)] public float heldTransitionDuration = 0.5f;

        /// <summary>
        /// When the weapon is held by a character, it will always use Aim IK, even
        /// when it is not specifically being aimed.
        /// Will only override stickyControlModule.isAimIKWhenNotAiming when set
        /// to true on this scriptable object.
        /// </summary>
        public bool isAimIKWhenNotAiming;

        /// <summary>
        /// Should this be applied to all characters?
        /// </summary>
        public bool isAppliedToAllCharacters;

        /// <summary>
        /// If a non-NPC character has Free Look enabled, when the weapon is held (but not aimed),
        /// does Free Look remain enabled? Useful when a weapon is held in a relaxed pose not pointing forward.
        /// </summary>
        public bool isFreeLookWhenHeld;

        /// <summary>
        /// Weapon Override. Fire button 1 can only fire when the weapon is being aimed
        /// </summary>
        public bool isOnlyFireWhenAiming1;

        /// <summary>
        /// Weapon Override. Fire button 2 can only fire when the weapon is being aimed
        /// </summary>
        public bool isOnlyFireWhenAiming2;

        /// <summary>
        /// Should anim action parameter name verification be skipped when the set is applied?
        /// Can be faster but will cause errors if parameter names don't exist in animator.
        /// </summary>
        public bool isSkipParmVerify;

        /// <summary>
        /// When a weapon is aimed and held by a character in 3rd person, should it use the first-person camera?
        /// </summary>
        public bool isAimTPUsingFPCamera;

        /// <summary>
        /// Should some of the weapon settings be overridden when the character picks up this weapon?
        /// </summary>
        public bool isWeaponSettingsOverride;

        /// <summary>
        /// An array of Sticky3D Controller Module model IDs. A Model ID (found on the Engage tab)
        /// groups characters with similar attributes together. E.g. Same rig or animation controller.
        /// </summary>
        public int[] stickyModelIDs;

        /// <summary>
        /// A list of animation clip replacement pairs for a character with
        /// a matching Model ID.
        /// </summary>
        public List<S3DAnimClipPair> animClipPairList;

        /// <summary>
        /// The number of seconds to delay reverting clips for a character to their originals
        /// when the weapon is dropped.
        /// </summary>
        [Range(0f,10f)] public float animClipPairRevertDelay = 0.5f;

        /// <summary>
        /// A list of Anim Actions that includes the parameter name
        /// used in the animation controller of the character that
        /// matches the Model ID.
        /// </summary>
        public List<S3DAnimActionExt> animActionExtList;

        #endregion

        #region Private Variables

        [SerializeField] bool isModelsExpandedInEditor = true;

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the zero-based index in the list of Anim Clip Pairs using the ID or guidHash.
        /// Returns -1 if no match is found.
        /// </summary>
        /// <param name="guidHash"></param>
        /// <returns></returns>
        public int GetAnimClipPairIndex(int guidHash)
        {
            int animClipPairIndex = -1;
            int numAnimClipPairs = animClipPairList == null ? 0 : animClipPairList.Count;

            // Keep compiler happy
            if (isModelsExpandedInEditor) { }

            // We're not using a FindIndex because we want to avoid GC
            for (int stIdx = 0; stIdx < numAnimClipPairs; stIdx++)
            {
                S3DAnimClipPair s3dAnimClipPair = animClipPairList[stIdx];
                if (s3dAnimClipPair.guidHash == guidHash)
                {
                    animClipPairIndex = stIdx;
                    break;
                }
            }

            return animClipPairIndex;
        }

        /// <summary>
        /// Get the ID or guidHash of the Anim Clip Pair given the zero-based
        /// index position in the list. If no valid match is found, returns 0
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetAnimClipPairID(int index)
        {
            int numAnimClipPairs = animClipPairList == null ? 0 : animClipPairList.Count;

            if (index < 0 || index >= numAnimClipPairs) { return 0; }
            else { return animClipPairList[index].guidHash; }
        }

        /// <summary>
        /// This should be used after duplicating or copying a weapon anim set.
        /// It gives the various elements a unique identification (guidHash).
        /// </summary>
        public void ResetIdentifiers()
        {
            int numAnimClipPairs = animClipPairList == null ? 0 : animClipPairList.Count;

            for (int stIdx = 0; stIdx < numAnimClipPairs; stIdx++)
            {
                S3DAnimClipPair s3dAnimClipPair = animClipPairList[stIdx];
                s3dAnimClipPair.guidHash = S3DMath.GetHashCodeFromGuid();
            }

            int numAnimActions = animActionExtList == null ? 0 : animActionExtList.Count;

            for (int stIdx = 0; stIdx < numAnimActions; stIdx++)
            {
                S3DAnimActionExt animActionExt = animActionExtList[stIdx];
                animActionExt.guidHash = S3DMath.GetHashCodeFromGuid();
            }
        }

        #endregion
    }
}