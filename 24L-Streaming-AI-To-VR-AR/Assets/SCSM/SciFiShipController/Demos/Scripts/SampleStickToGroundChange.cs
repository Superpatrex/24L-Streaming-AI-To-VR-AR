using UnityEngine;
using SciFiShipController;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace MyUniqueGame
{
    /// <summary>
    /// Sample script used to show how to override the StickToGround options at runtime.
    /// This one controls target ground distance but you could change other options.
    /// WARNING: This is a DEMO script and is subject to change without notice during
    /// upgrades. This is just to show you how to do things in your own code.
    /// SETUP:
    /// 1. Add this script to your player ship
    /// 2. Add a Custom Player Input button (which can be held down) to Player Input Module
    /// 3. Add an event to Custom Player Input, drag in ShipControlModule from scene
    /// 4. Select SampleStickToGroundChange, IncreaseHeight()
    /// 5. Repeat steps 2-4 for DecreaseHeight()
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Stick To Ground Change")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleStickToGroundChange : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = true;
        [Range(0.1f, 20f)] public float rateOfAscent = 3f;
        [Range(0.1f, 20f)] public float rateOfDescent = 2f;
        [Range(0f, 100f)] public float maxAdditionalHeight = 20f;
        #endregion

        #region Private Variables
        private ShipControlModule shipControlModule = null;
        private Ship ship = null;
        private bool isInitialised = false;
        private float startTargetDistance = 0f;
        private float maxCheckDistanceDiff = 0;
        private float maxTargetDistance = 0f;
        private float currentTargetDistance = 0;
        #endregion

        #region Iniitialise Methods

        // Start is called before the first frame update
        private void Start()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this from your code if initialiseOnStart is false
        /// </summary>
        public void Initialise()
        {
            if (isInitialised) { return; }

            shipControlModule = GetComponent<ShipControlModule>();

            if (shipControlModule == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleStickToGroundChange - could not find the player ship component - this script needs to be attached to your ShipControlModule for your player ship");
                #endif
            }
            else if (!shipControlModule.IsInitialised)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SampleStickToGroundChange - your player ship is not initialised");
                #endif
            }
            else
            {
                // Get a reference to the shipInstance script
                ship = shipControlModule.shipInstance;

                // Remember starting values
                startTargetDistance = ship.targetGroundDistance;

                // Keep the difference between check dist and target dist constant so we
                // don't need to call ReinitialiseGroundMatchVariables() as we change the
                // target distnace.
                maxCheckDistanceDiff = ship.maxGroundCheckDistance - startTargetDistance;
                maxTargetDistance = startTargetDistance + maxAdditionalHeight;

                currentTargetDistance = startTargetDistance;

                isInitialised = true;
            }
        }

        /// <summary>
        /// Increase the StickToGround Target distance.
        /// Call this from a Custom Player Input on the Player Input Module.
        /// </summary>
        public void IncreaseHeight()
        {
            if (isInitialised)
            {
                if (currentTargetDistance < maxTargetDistance)
                {
                    currentTargetDistance += rateOfAscent * Time.deltaTime;

                    if (currentTargetDistance > maxTargetDistance) { currentTargetDistance = maxTargetDistance; }

                    ship.targetGroundDistance = currentTargetDistance;
                    ship.maxGroundCheckDistance = currentTargetDistance + maxCheckDistanceDiff;
                }
            }
        }

        /// <summary>
        /// Decrease the StickToGround Target distance.
        /// Call this from a Custom Player Input on the Player Input Module.
        /// </summary>
        public void DecreaseHeight()
        {
            if (isInitialised)
            {
                if (currentTargetDistance > startTargetDistance)
                {
                    currentTargetDistance -= rateOfDescent * Time.deltaTime;

                    if (currentTargetDistance < startTargetDistance) { currentTargetDistance = startTargetDistance; }

                    ship.targetGroundDistance = currentTargetDistance;
                    ship.maxGroundCheckDistance = currentTargetDistance + maxCheckDistanceDiff;
                }
            }
        }

        #endregion

    }
}