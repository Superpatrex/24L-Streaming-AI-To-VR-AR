using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sci-Fi Ship Controller. Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace SciFiShipController
{
    /// <summary>
    /// Class containing data for an AI Behaviour Output.
    /// </summary>
    public class AIBehaviourOutput
    {
        #region Public Variables

        /// <summary>
        /// The heading vector returned as an output by this behaviour output. This is the direction in world space
        /// the AI ship will attempt to face towards.
        /// </summary>
        public Vector3 heading;
        /// <summary>
        /// The up vector returned as an output by this behaviour output. This is the direction in world space
        /// the AI ship will attempt to align its upwards direction with.
        /// </summary>
        public Vector3 up;
        /// <summary>
        /// The velocity vector returned as an output by this behaviour output. This is the velocity in world space
        /// the AI ship will attempt to attain.
        /// </summary>
        public Vector3 velocity;
        /// <summary>
        /// The target vector (in world space) returned as an output by this behaviour output. If there is no particular target 
        /// position in world space that this behaviour output is aiming for, set this to Vector3.zero.
        /// </summary>
        public Vector3 target;
        /// <summary>
        /// Whether the target vector returned as an output by this behaviour output is set. This should be set to true if this
        /// behaviourInput has a particular target position in world space that it is aiming for, or if there is no particular
        /// target position (i.e. targetOutput is set to Vector3.zero) but you still want override behaviours 
        /// (such as obstacle avoidance etc.) to override this behaviour output. This should be set to false if this behaviour output
        /// has no particular target position in world space and is an override behaviour (i.e. it should remember a target
        /// position from another behaviour output and revert back to it upon completion).
        /// (If you don't know what to set this to, it should probably be set to true).
        /// </summary>
        public bool setTarget;
        /// <summary>
        /// Whether the targeting accuracy parameter in the ShipAIInputModule should be used to modify the target heading
        /// of this behaviour output (used for steering). This should generally be set to true when this behaviour output
        /// is to be used for shooting at a target. Otherwise, it should be set to false.
        /// </summary>
        public bool useTargetingAccuracy;

        #endregion

        #region Class Constructors

        // Class constructor #1
        public AIBehaviourOutput()
        {
            SetClassDefaults();
        }

        #endregion

        #region Public Member Methods

        public void SetClassDefaults()
        {
            heading = Vector3.zero;
            up = Vector3.zero;
            velocity = Vector3.zero;
            target = Vector3.zero;
            setTarget = true;
            useTargetingAccuracy = false;
        }

        /// <summary>
        /// Normalises any output vectors that need to be in normalised form.
        /// </summary>
        public void NormaliseOutputs()
        {
            heading.Normalize();
            up.Normalize();
        }

        #endregion
    }
}
