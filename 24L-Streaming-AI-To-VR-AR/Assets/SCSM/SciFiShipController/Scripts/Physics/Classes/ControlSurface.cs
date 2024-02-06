using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a control surface.
    /// </summary>
    [System.Serializable]
    public class ControlSurface
    {
        #region Enumerations

        public enum ControlSurfaceType
        {
            Aileron = 10,
            Elevator = 11,
            Rudder = 12,
            AirBrake = 20, 
            //Custom = 50
        }

        #endregion

        #region Public variables and properties

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The type of control surface this is. This defines the axis it rotates on as well as the inputs it is controlled by.
        /// If you modify this, call ReinitialiseInputVariables() on the ship this control surface is attached to.
        /// </summary>
        public ControlSurfaceType type;

        /// <summary>
        /// The length of the control surface (measured along the axis of rotation).
        /// </summary>
        public float span;
        /// <summary>
        /// The width of the control surface (measured along the local z-axis).
        /// </summary>
        public float chord;

        /// <summary>
        /// Position of the pivot of the control surface in local space relative to the pivot point of the ship.
        /// </summary>
        public Vector3 relativePosition;
        /// <summary>
        /// The local axis about which the control surface rotates. This is only relevant for Custom type (in other types it is defined automatically).
        /// </summary>
        public Vector3 rotationAxis;

        /// <summary>
        /// The index of the damage region this control surface is associated with. When the damage model of the ship is set to simple, this 
        /// is irrelevant. A negative value means it is associated with no damage region (so the control surface's performance will not be 
        /// affected by damage). When the damage model of the ship is set to progressive, a value of zero means it is 
        /// associated with the main damage region. When the damage model of the ship is set to localised, a zero or positive value
        /// indicates which damage region it is associated with (using a zero-based indexing system).
        /// </summary>
        public int damageRegionIndex;
        /// <summary>
        /// The minimum (i.e. when its health reaches zero) performance level of this control surface. The performance level affects how much
        /// aerodynamic effect is produced by this control surface. At a performance level of one it produces the usual value. At a performance level of
        /// zero it has no effect.
        /// </summary>
        [Range(0f, 1f)] public float minPerformance;
        /// <summary>
        /// The current performance level of this control surface (determined by the Health value). The performance level affects how much
        /// aerodynamic effect is produced by this control surface. At a performance level of one it produces the usual value. At a performance level of
        /// zero it has no effect.
        /// </summary>
        public float CurrentPerformance { get; private set; }
        /// <summary>
        /// The starting health value of this control surface.
        /// </summary>
        public float startingHealth;

        private float health;
        /// <summary>
        /// The current health value of this control surface. 
        /// </summary>
        public float Health
        {
            get { return health; }
            set
            {
                // Update the health value
                health = value;
                // Update the current performance value
                CurrentPerformance = value / startingHealth;
                CurrentPerformance = CurrentPerformance > minPerformance ? CurrentPerformance : minPerformance;
                CurrentPerformance = CurrentPerformance < 1f ? CurrentPerformance : 1f;
            }
        }


        /// <summary>
        /// Whether the control surface is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;
        /// <summary>
        /// Whether the control surface node is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this control surface are shown in the scene view of the editor
        /// </summary>
        public bool showGizmosInSceneView;

        #endregion

        #region Class constructors

        public ControlSurface ()
        {
            SetClassDefaults();
        }

        // Copy contructor
        public ControlSurface(ControlSurface controlSurface)
        {
            if (controlSurface == null) { SetClassDefaults(); }
            else
            {
                this.type = controlSurface.type;
                this.span = controlSurface.span;
                this.chord = controlSurface.chord;
                this.relativePosition = controlSurface.relativePosition;
                this.rotationAxis = controlSurface.rotationAxis;
                this.damageRegionIndex = controlSurface.damageRegionIndex;
                this.minPerformance = controlSurface.minPerformance;
                this.startingHealth = controlSurface.startingHealth;
                this.Health = controlSurface.Health;
                this.showInEditor = controlSurface.showInEditor;
                this.selectedInSceneView = controlSurface.selectedInSceneView;
                this.showGizmosInSceneView = controlSurface.showGizmosInSceneView;
            }
        }

        #endregion

        #region Public Non-Static Methods

        public void SetClassDefaults()
        {
            this.type = ControlSurfaceType.Aileron;
            this.span = 10f;
            this.chord = 1f;
            this.relativePosition = Vector3.zero;
            this.rotationAxis = Vector3.right;
            this.damageRegionIndex = -1;
            this.minPerformance = 0.25f;
            this.startingHealth = 100f;
            this.Health = 100f;
            this.showInEditor = true;
            this.selectedInSceneView = false;
            this.showGizmosInSceneView = true;
        }

        #endregion
    }
}
