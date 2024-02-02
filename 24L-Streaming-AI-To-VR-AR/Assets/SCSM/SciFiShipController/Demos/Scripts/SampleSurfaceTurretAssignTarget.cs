using UnityEngine;
using SciFiShipController;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipControllerSample
{
    /// <summary>
    /// Sample script to assign a target to a Surface Turret at runtime.
    /// This is only a code segment to demonstrate how API calls could be used in
    /// your own code. Place it on an empty gameobject in the scene to see how
    /// it works.
    /// </summary>
    [AddComponentMenu("Sci-Fi Ship Controller/Samples/Surface Turret Assign Target")]
    [HelpURL("http://scsmmedia.com/ssc-documentation")]
    public class SampleSurfaceTurretAssignTarget : MonoBehaviour
    {
        #region Public variables
        public bool initialiseOnStart = true;

        [Header("Surface Turret")]
        public SurfaceTurretModule surfaceTurretModule = null;

        [Header("Target GameObject")]
        public GameObject enemyTarget = null;

        #endregion

        #region Initialisation Methods
        void Awake()
        {
            if (initialiseOnStart) { Initialise(); }
        }

        public void Initialise()
        {
            if (surfaceTurretModule == null) { Debug.LogWarning("Please specify a Surface Turret Module from the scene."); }
            else if (enemyTarget == null) { Debug.LogWarning("Please specify an enemy target gameobject from the scene."); }
            else
            {
                // If the surface turret has not been initialised already either through code or the "Initialise On Start"
                // checkbox on the Surface Turret Module, do it now.
                if (!surfaceTurretModule.IsInitialised) { surfaceTurretModule.Initialise(); }
                surfaceTurretModule.SetWeaponTarget(enemyTarget);
            }
        }

        #endregion
    }
}