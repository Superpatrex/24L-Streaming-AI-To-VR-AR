using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Sample code to show Health of a ship in the UI.
    /// This is only a sample to demonstrate how API calls could be used in your own code.
    /// Add this to an empty gameobject in the scene and drag a ship from the scene into
    /// the shipControlModule slot.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Show Ship Health")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleShowShipHealth : MonoBehaviour
    {
        #region Public variables
        /// <summary>
        /// If true, will run just before first frame is rendered. Set to false if
        /// you wish to add a reference to the ship from the scene at runtime.
        /// </summary>
        public bool initialiseOnAwake = false;
        public bool isHealthDisplayed = false;
        public ShipControlModule shipControlModule = null;
        public Color normalColour = Color.white;
        public Color warningColour = Color.yellow;
        public Color criticalColour = Color.red;

        #endregion

        #region Private variables
        private bool isInitialised = false;
        private Canvas canvas;
        private UnityEngine.UI.Text uiTextHealthLabel;
        private UnityEngine.UI.Text uiTextHealthValue;
        private UnityEngine.UI.Text uiTextShieldLabel;
        private UnityEngine.UI.Text uiTextShieldValue;
        private float healthStartValue = 0f;
        private float shieldStartValue = 0f;

        #endregion

        #region Initialisation Methods
        void Start()
        {
            if (!isInitialised && initialiseOnAwake) { Initialise(); }
        }

        public void Initialise()
        {
            if (isHealthDisplayed)
            {
                // Add a new canvas
                canvas = GetComponent<Canvas>();
                if (canvas == null) { canvas = gameObject.AddComponent<Canvas>(); }

                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    uiTextHealthLabel = CreateTextUI(transform, new Vector2(0f, 20f), 45f, 20f);
                    uiTextHealthValue = CreateTextUI(transform, new Vector2(55f, 20f), 40f, 20f);

                    if (uiTextHealthLabel != null) { uiTextHealthLabel.text = "Health"; }

                }

                // Setup the callback method and display the initial value
                if (shipControlModule != null)
                {
                    if (shipControlModule.shipInstance != null)
                    {
                        if (shipControlModule.shipInstance.mainDamageRegion.useShielding)
                        {
                            uiTextShieldLabel = CreateTextUI(transform, new Vector2(0f, 40f), 45f, 20f);
                            uiTextShieldValue = CreateTextUI(transform, new Vector2(55f, 40f), 40f, 20f);

                            if (uiTextShieldLabel != null) { uiTextShieldLabel.text = "Shield"; }
                        }

                        healthStartValue = shipControlModule.shipInstance.mainDamageRegion.Health;
                        shieldStartValue = shipControlModule.shipInstance.mainDamageRegion.ShieldHealth;

                        // Avoid div 0
                        if (healthStartValue == 0f) { healthStartValue = 0.01f; }
                        if (shieldStartValue == 0f) { shieldStartValue = 0.01f; }

                        shipControlModule.shipInstance.callbackOnDamage = ShipHealthUpdated;
                        ShipHealthUpdated(healthStartValue);
                        isInitialised = true;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a new UI Text panel
        /// </summary>
        /// <param name="parentTfrm"></param>
        /// <param name="topLeft"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private UnityEngine.UI.Text CreateTextUI(Transform parentTfrm, Vector2 topLeft, float width, float height)
        {
            UnityEngine.UI.Text uiText = null;

            // Create a text panel for the health value
            GameObject panelGO = new GameObject();
            if (panelGO != null)
            {
                if (parentTfrm != null) { panelGO.transform.SetParent(parentTfrm); }

                uiText = panelGO.AddComponent<UnityEngine.UI.Text>();
                if (uiText != null)
                {
                    RectTransform rtfrm = panelGO.GetComponent<RectTransform>();
                    rtfrm.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, topLeft.x, width);
                    rtfrm.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, topLeft.y, height);
                    uiText.font = SSCUtils.GetDefaultFont();
                    uiText.raycastTarget = false;
                }
            }

            return uiText;
        }

        private void Create3DText(Transform parentTfrm)
        {
            GameObject panelGO = new GameObject();
            if (panelGO != null)
            {
                if (parentTfrm != null) { panelGO.transform.SetParent(parentTfrm); }
            }
        }

        /// <summary>
        /// Call back method that is called from ship.cs after damage is applied.
        /// NOTE: This sample will generate some GC due to the use of ToString().
        /// Formatting the string will generate even more GC.
        /// </summary>
        /// <param name="health"></param>
        private void ShipHealthUpdated(float health)
        {
            if (isHealthDisplayed && uiTextHealthValue != null)
            {
                // Health values can be below zero so clamp them to 0.
                health = health < 0f ? 0f : health;
                uiTextHealthValue.text = health.ToString();

                SetTextUIColour(uiTextHealthValue, health / healthStartValue);

                // If shielding is in use for the main damage region,
                // look up the value.
                if (uiTextShieldValue != null && shipControlModule != null && shipControlModule.shipInstance != null)
                {
                    float shieldingHealth = shipControlModule.shipInstance.mainDamageRegion.ShieldHealth;
                    shieldingHealth = shieldingHealth < 0f ? 0f : shieldingHealth;
                    uiTextShieldValue.text = shieldingHealth.ToString();

                    SetTextUIColour(uiTextShieldValue, shieldingHealth / shieldStartValue);
                }
            }
        }

        /// <summary>
        /// Update the colour of the text based on the 0-1 value.
        /// </summary>
        /// <param name="uiText"></param>
        /// <param name="value"></param>
        private void SetTextUIColour(UnityEngine.UI.Text uiText, float value)
        {
            if (uiText != null)
            {
                if (value < 0.15f)
                {
                    uiText.color = criticalColour;
                }
                else if (value < 0.3f)
                {
                    uiText.color = warningColour;
                }
                else
                {
                    uiText.color = normalColour;
                }
            }
        }

        #endregion
    }
}