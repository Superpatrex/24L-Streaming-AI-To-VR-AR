using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Sci-Fi Ship Controller/Utilities/Destroy Timer")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class DestroyTimer : MonoBehaviour
    {
        #region Public Variables

        #endregion

        #region Private Static Variables
        private readonly static string destroyTimerMethod = "DestroyGameObject";
        #endregion

        #region Initialisation Methods

        /// <summary>
        /// Initialise the DestroyTimer by setting the despawn time
        /// </summary>
        /// <param name="despawnTime"></param>
        public void Initialise(float despawnTime)
        {
            Invoke(destroyTimerMethod, despawnTime);
        }

        #endregion

        #region Private Methods

        private void DestroyGameObject()
        {
            Destroy(gameObject);
        }
        #endregion
    }
}
