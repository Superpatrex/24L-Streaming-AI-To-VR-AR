using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Simple script to have a Sticky3D character look at one or more objects using Head IK.
    /// If the object is a character, look at their eye level.
    /// EXAMPLE: See the NPCLookAtDemo scene.
    /// NOTE: This sample assumes the array of targets does not change after initialised.
    /// If you want to modify this script, create a new script in your own namespace
    /// and copy over the code you need. DO NOT MODIFY this script as it will be
    /// overwritten when you do the next S3D update.
    /// SETUP
    /// 1. Add this script to a S3D (NPC) character in the scene (not the player)
    /// 2. Configure the look at targets in the editor for this component
    /// 3. Run the scene
    /// 4. Optionally turn off randomlyCycleTargets, and call LookAtNextTarget(),
    ///    LookAtPreviousTarget(), or LookAtTarget(index) from a Custom Input on
    ///    a player S3D or from game code.
    /// </summary>
    [AddComponentMenu("Sticky3D Controller/Samples/NPC Look")]
    [DisallowMultipleComponent]
    public class SampleNPCLook : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = true;
        [Tooltip("Check if there is anything between the character and the object")]
        public bool checkLOS = false;
        [Tooltip("An array of GameObject or S3D characters to look at")]
        public GameObject[] lookAtTargets = null;
        [Tooltip("Randomly cycle between looked at each of the targets")]
        public bool randomlyCycleTargets = true;

        public float minCycleTime = 3f;
        public float maxCycleTime = 5f;
        #endregion

        #region Private Variables
        private StickyControlModule thisCharacter = null;
        private bool isInitialised = false;
        private int numTargets = 0;
        private bool[] isS3DTargets = null;
        private int currentTargetIndex = -1;
        private float nextTimer = 0f;
        private float currentDuration = 0f;
        private S3DRandom s3DRandom = null;
        #endregion

        #region Initialisation Methods
        // Start is called before the first frame update
        void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Update Events

        private void Update()
        {
            if (isInitialised && randomlyCycleTargets)
            {
                nextTimer += Time.deltaTime;
                if (nextTimer >= currentDuration)
                {
                    // Get a random look duration
                    currentDuration = s3DRandom.Range(minCycleTime, maxCycleTime);
                    nextTimer = 0f;

                    // Get a random target from the array
                    currentTargetIndex = s3DRandom.Range(0, numTargets-1);
                    LookAtTarget(currentTargetIndex);
                }
            }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            thisCharacter = GetComponent<StickyControlModule>();
            numTargets = lookAtTargets == null ? 0 : lookAtTargets.Length;

            // This is used when randomly cycling through targets
            s3DRandom = new S3DRandom();

            if (thisCharacter == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCLook - did you forget to attach this script to one of our non-player S3D characters?");
                #endif
            }
            else if (!thisCharacter.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCLook - your character " + thisCharacter.name + " is not initialised");
                #endif
            }
            else if (numTargets < 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCLook - you will need at least one Look At Target for the character to observe.");
                #endif
            }
            else if (s3DRandom == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SampleNPCLook - could not create an instance of S3DRandom");
                #endif
            }
            else
            {
                s3DRandom.SetSeed(117);

                // Cache if the target is an S3D character
                isS3DTargets = new bool[numTargets];

                bool validTargets = true;

                for (int tgIdx = 0; tgIdx < numTargets; tgIdx++)
                {
                    if (lookAtTargets[tgIdx] != null)
                    {
                        isS3DTargets[tgIdx] = lookAtTargets[tgIdx].GetComponent<StickyControlModule>() != null;
                    }
                    else
                    {
                        validTargets = false;
                        break;
                    }
                }

                if (!validTargets)
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("ERROR: SampleNPCLook - you have at least one target that is null.");
                    #endif
                }
                else
                {
                    isInitialised = true;
                }
            }
        }

        /// <summary>
        /// Use Head IK to look at at target in the array of targets
        /// </summary>
        /// <param name="targetIndex"></param>
        public void LookAtTarget (int targetIndex)
        {
            if (isInitialised)
            {
                if (targetIndex >= 0 && targetIndex < numTargets)
                {
                    thisCharacter.SetHeadIKTarget(lookAtTargets[targetIndex].transform, Vector3.zero, false, isS3DTargets[targetIndex]);

                    if (!thisCharacter.IsHeadIKEnabled) { thisCharacter.EnableHeadIK(true); }
                    else if (thisCharacter.IsHeadIKDisabling) { thisCharacter.StopHeadIKDisable(); }
                }
                #if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("ERROR: SampleNPCLook - target index (" + targetIndex + ") for " + thisCharacter.name + " is out-of-range. Valid values are between 0 and " + (numTargets-1).ToString());
                }
                #endif
            }
        }

        /// <summary>
        /// Look at the next target in the sequence. Will auto-wrap to the start.
        /// </summary>
        public void LookAtNextTarget()
        {
            if (isInitialised)
            {
                currentTargetIndex = (currentTargetIndex + 1) % numTargets;
                LookAtTarget(currentTargetIndex);
            }
        }

        /// <summary>
        /// Look at the previous target in the sequence. Will auto-wrap to the end.
        /// </summary>
        public void LookAtPreviousTarget()
        {
            if (isInitialised)
            {
                currentTargetIndex = (currentTargetIndex - 1) % numTargets;
                if (currentTargetIndex < 0) { currentTargetIndex = numTargets - 1; }
                LookAtTarget(currentTargetIndex);
            }
        }

        /// <summary>
        /// Set Randomly Cycle Targets on or off
        /// </summary>
        /// <param name="isOn"></param>
        public void SetRandomlyCycleTargets(bool isOn)
        {
            randomlyCycleTargets = isOn;
        }

        #endregion
    }
}
