using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for a wing.
    /// </summary>
    [System.Serializable]
    public class Wing
    {
        #region Public variables and properties

        // IMPORTANT - when changing this section also update SetClassDefault()
        // Also update ClassName(ClassName className) Clone Constructor (if there is one)

        /// <summary>
        /// The name of the wing
        /// </summary>
        public string name;

        /// <summary>
        /// Angle of attack (in degrees) of the wing relative to the local forwards direction. This is purely achieved by the camber of the wing.
        /// </summary>
        [Range(-5f, 15f)] public float angleOfAttack;
        /// <summary>
        /// The length of the wing (measured along the local x-axis).
        /// </summary>
        public float span;
        /// <summary>
        /// The width of the wing (measured along the local z-axis).
        /// </summary>
        public float chord;
        /// <summary>
        /// Position of the centre of the wing in local space relative to the pivot point of the ship. This is the position where the lift force will be applied at.
        /// </summary>
        public Vector3 relativePosition;
        /// <summary>
        /// The local space direction of lift provided by the wing. If you modify this, call Initialise().
        /// </summary>
        public Vector3 liftDirection;
        /// <summary>
        /// The normalised local space direction of lift provided by the wing.
        /// </summary>
        public Vector3 liftDirectionNormalised { get; private set; }

        /// <summary>
        /// The index of the damage region this wing is associated with. When the damage model of the ship is set to simple, this 
        /// is irrelevant. A negative value means it is associated with no damage region (so the wing's performance will not be 
        /// affected by damage). When the damage model of the ship is set to progressive, a value of zero means it is 
        /// associated with the main damage region. When the damage model of the ship is set to localised, a zero or positive value
        /// indicates which damage region it is associated with (using a zero-based indexing system).
        /// </summary>
        public int damageRegionIndex;
        /// <summary>
        /// The minimum (i.e. when its health reaches zero) performance level of this wing. The performance level affects how much
        /// lift is produced by this wing. At a performance level of one it produces the usual value. At a performance level of
        /// zero it produces no lift.
        /// </summary>
        [Range(0f, 1f)] public float minPerformance;
        /// <summary>
        /// The current performance level of this wing (determined by the Health value). The performance level affects how much
        /// lift is produced by this wing. At a performance level of one it produces the usual value. At a performance level of
        /// zero it produces no lift.
        /// </summary>
        public float CurrentPerformance { get; private set; }
        /// <summary>
        /// The starting health value of this wing.
        /// </summary>
        public float startingHealth;

        private float health;
        /// <summary>
        /// The current health value of this wing. 
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
        /// Whether the wing is shown as expanded in the inspector window of the editor.
        /// </summary>
        public bool showInEditor;
        /// <summary>
        /// Whether the wing node is shown as selected in the scene view of the editor.
        /// </summary>
        public bool selectedInSceneView;

        /// <summary>
        /// Whether the gizmos for this wing node are shown in the scene view of the editor
        /// </summary>
        public bool showGizmosInSceneView;

        #endregion

        #region Class constructors

        public Wing()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public Wing (Wing wing)
        {
            if (wing == null) { SetClassDefaults(); }
            else
            {
                this.name = wing.name;
                this.angleOfAttack = wing.angleOfAttack;
                this.span = wing.span;
                this.chord = wing.chord;
                this.relativePosition = wing.relativePosition;
                this.liftDirection = wing.liftDirection;
                this.damageRegionIndex = wing.damageRegionIndex;
                this.minPerformance = wing.minPerformance;
                this.startingHealth = wing.startingHealth;
                this.Health = wing.Health;
                this.showInEditor = wing.showInEditor;
                this.selectedInSceneView = wing.selectedInSceneView;
                this.showGizmosInSceneView = wing.showGizmosInSceneView;
                this.Initialise();
            }
        }

        #endregion

        #region Public Non-Static Methods

        public void SetClassDefaults()
        {
            this.name = "Wing";
            this.angleOfAttack = 5f;
            this.span = 10f;
            this.chord = 1f;
            this.relativePosition = Vector3.zero;
            this.liftDirection = Vector3.up;
            this.damageRegionIndex = -1;
            this.minPerformance = 0.25f;
            this.startingHealth = 100f;
            this.Health = 100f;
            this.showInEditor = true;
            this.selectedInSceneView = false;
            this.showGizmosInSceneView = true;
            this.Initialise();
        }

        /// <summary>
        /// Initialises data for the wing. This does some precalculation to allow for performance improvements.
        /// Call after modifying liftDirection.
        /// </summary>
        public void Initialise ()
        {
            // Calculate normalised vectors
            liftDirectionNormalised = liftDirection.normalized;
        }

        #endregion
    }
}
