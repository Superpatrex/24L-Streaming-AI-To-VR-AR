using UnityEngine;

// Sticky3D Controller Copyright (c) 2018-2024 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// Interface for objects used in the pooling system
    /// </summary>
    public interface IStickyPoolable
    {
        #region Public Properties

        /// <summary>
        /// [READONLY] Is the Module initialised?
        /// </summary>
        bool IsInitialised { get; }

        /// <summary>
        /// Is the Module enabled?
        /// See also EnableModule() and DisableModule().
        /// </summary>
        bool IsModuleEnabled { get; }

        #endregion

        #region Public Virtual API Methods

        /// <summary>
        /// Destroy (deactivate) the Generic Module. Returns it to the pool.
        /// </summary>
        void DestroyGenericObject();

        /// <summary>
        /// Temporarily disable the module. Typically called automatically from
        /// stickyManager.PauseGenericObjects().
        /// </summary>
        void DisableModule();

        /// <summary>
        /// Re-enable the module. Typically called automatically from
        /// stickyManager.ResumeGenericObjects().
        /// </summary>
        void EnableModule();

        /// <summary>
        /// Initialise the module after it has been activated in the pool managed by StickyManager.
        /// </summary>
        /// <param name="igParms"></param>
        /// <returns></returns>
        uint Initialise(S3DInstantiateGenericObjectParameters igParms);
        //uint Initialise<T>(T igParms);

        #endregion
    }
}