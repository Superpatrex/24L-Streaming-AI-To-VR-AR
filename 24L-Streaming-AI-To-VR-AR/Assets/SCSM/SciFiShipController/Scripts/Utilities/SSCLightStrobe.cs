using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Simple strobe or pulse-like effect for a Point or Spot light
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Utilities/SSC Light Probe")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    [RequireComponent(typeof(Light))]
    public class SSCLightStrobe : MonoBehaviour
    {
        #region Public Variables
        public bool initialiseOnStart = false;
        [Tooltip("Should the strobe light be on when it is initialised?")]
        public bool enableOnStart = true;
        [Tooltip("The duration, in seconds, the light is pulsing")]
        public float cycleDuration = 1f;
        [Tooltip("The duration, in seconds, the light is at the minimum intensity")]
        public float waitTime = 0.5f;
        [Tooltip("The minimum intensity of the strobe light")]
        public float minIntensity = 0f;
        [Tooltip("The maximum intensity of the strobe light")]
        public float maxIntensity = 1f;
        #endregion

        #region Private Variables
        private Light strobeLight = null;
        private bool isInitialised = false;
        private float cycleTimer = 0f;
        private float waitTimer = 0f;
        private bool isWaiting = false;
        private bool isOn = false;
        private bool isHDRP = false;
        private System.Type hdLightType = null;
        #endregion

        #region Initialise Methods

        // Start is called before the first frame update
        void Start()
        {
            strobeLight = GetComponent<Light>();

            if (strobeLight == null)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("SSCLightStrobe on " + this.name + " could not find an attached Light component");
                #endif
            }
            else
            {
                if (initialiseOnStart) { Initialise(); }
                else { strobeLight.enabled = false; }
            }

        }
        #endregion

        #region Update Methods
        // Update is called once per frame
        void Update()
        {
            if (!isInitialised || !isOn) { return; }

            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer > waitTime)
                {
                    isWaiting = false;
                    cycleTimer = 0f;
                }
            }
            else
            {
                cycleTimer += Time.deltaTime;
                if (cycleTimer > cycleDuration)
                {
                    SetIntensity(minIntensity);
                    waitTimer = 0f;
                    isWaiting = true;
                }
                else
                {
                    SetIntensity(Mathf.Lerp(minIntensity, maxIntensity, cycleTimer / cycleDuration));
                }
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Set the intensity of the strobe light
        /// </summary>
        /// <param name="newIntensity"></param>
        private void SetIntensity(float newIntensity)
        {
            if (isHDRP)
            {          
                SSCUtils.SetHDLightIntensity(hdLightType, strobeLight, newIntensity);
            }
            else
            {
                strobeLight.intensity = newIntensity;
            }
        }

        #endregion

        #region Public Methods

        public void Initialise()
        {
            if (strobeLight == null) { strobeLight = GetComponent<Light>(); }

            if (strobeLight != null)
            {
                isWaiting = true;

                isHDRP = SSCUtils.IsHDRP(false);

                if (isHDRP)
                {
                    hdLightType = SSCUtils.GetHDLightDataType(true);
                }

                SetIntensity(minIntensity);

                isOn = enableOnStart;

                strobeLight.enabled = isOn;

                isInitialised = true;
            }
        }

        /// <summary>
        /// Turn on or off the strobe light
        /// </summary>
        /// <param name="turnOn"></param>
        public void TurnOn(bool turnOn = true)
        {
            if (isInitialised)
            {
                isOn = turnOn;
                strobeLight.enabled = isOn;
            }
        }

        // Turn off the strobe light
        public void TurnOff()
        {
            isOn = false;
            strobeLight.enabled = isOn;
        }

        #endregion
    }
}